using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace OctoberStudio.Save
{
    [DefaultExecutionOrder(-100)]
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        public static readonly string SAVE_FILE_NAME_EDITOR = "game_save_editor";
        public static readonly string SAVE_FILE_NAME = "game_save";

        [SerializeField] SaveType saveType = SaveType.SaveFile;

        [Space]
        [SerializeField] bool clearSave;

        [Space]
        [SerializeField] bool autoSaveEnabled;
        [SerializeField] float autoSaveDelay;

#if !UNITY_6000_6_OR_NEWER
        [Space]
        [SerializeField] protected bool useJsonSerialization = true;
#endif

        private SaveDatabase SaveDatabase { get; set; }

        public bool IsSaveLoaded { get; private set; }

        public event UnityAction OnSaveLoaded;

        private Coroutine saveCoroutine;

        protected bool isSaving = false;

        private void Awake()
        {
            if (!GameController.RegisterSaveManager(this))
            {
                Destroy(gameObject);

                return;
            }

            if (clearSave)
            {
                InitClear();
            }
            else
            {
                Load();
            }

            if (autoSaveEnabled)
            {
                StartCoroutine(AutoSaveCoroutine());
            }
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(int hash) where T : ISave, new()
        {
            if (!IsSaveLoaded)
            {
                Debug.LogError("Save file has not been loaded yet");
                return default;
            }

            return SaveDatabase.GetSave<T>(hash);
        }

        /// <summary>
        /// Returns an instance from the save database, or creates a new one
        /// </summary>
        /// <typeparam name="T">Should implement ISave interface</typeparam>
        /// <param name="uniqueName">The unique identifier of the object you want to retrieve</param>
        /// <returns></returns>
        public T GetSave<T>(string uniqueName) where T : ISave, new()
        {
            return GetSave<T>(uniqueName.GetHashCode());
        }

        private void InitClear()
        {
            SaveDatabase = new SaveDatabase();
            SaveDatabase.Init();

            Debug.Log("New save is created");

            IsSaveLoaded = true;
        }

        private void Load()
        {
            if (IsSaveLoaded)
                return;

            if (saveType == SaveType.SaveFile)
            {
                // Try to read and deserialize file or create new one
                SaveDatabase = LoadSave();

                SaveDatabase.Init();

                Debug.Log("Save file is loaded");
            }
            else
            {
                var json = PlayerPrefs.GetString("save");
                SaveDatabase = JsonUtility.FromJson<SaveDatabase>(json);
                if (SaveDatabase == null) SaveDatabase = new SaveDatabase();

                SaveDatabase.Init();

                Debug.Log("Loaded SaveDatabase from PlayerPrefs");
            }

            IsSaveLoaded = true;

            OnSaveLoaded?.Invoke();
        }

        private SaveDatabase LoadSave()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonObject = load(SAVE_FILE_NAME);
            if(!string.IsNullOrEmpty(jsonObject))
            {
                try
                {
                    SaveDatabase deserializedObject = JsonUtility.FromJson<SaveDatabase>(jsonObject);

                    return deserializedObject;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }

            return new SaveDatabase();
#else
            SaveDatabase saveDatabase = null;

            var saveFileName = Application.isEditor ? SAVE_FILE_NAME_EDITOR : SAVE_FILE_NAME;
#if !UNITY_6000_6_OR_NEWER
            if (useJsonSerialization)
            {
                saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, true, useLogs: false);
                if (saveDatabase == null)
                {
                    saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, false, useLogs: false);
                    if (saveDatabase != null)
                    {
                        Debug.Log("Save file is migrated to JSON format");
                    }
                    else
                    {
                        saveDatabase = new SaveDatabase();
                    }
                }

                return saveDatabase;
            } else
            {
                saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, false, useLogs: false);
                if(saveDatabase == null)
                {
                    saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, true, useLogs: false);
                    if(saveDatabase == null)
                    {
                        saveDatabase = new SaveDatabase();
                    }
                }

                return saveDatabase;
            }
#else
            saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, useLogs: false);

            if (saveDatabase == null)
            {
                saveDatabase = new SaveDatabase();
            }
            return saveDatabase;
#endif
#endif
        }

        private IEnumerator SaveCoroutine(bool multithreading = false)
        {
            while (isSaving)
            {
                yield return null;
            }

            isSaving = true;

            if (multithreading)
            {
                var completed = false;
                Exception threadException = null;

                Thread saveThread = new Thread(() =>
                {
                    try
                    {
                        var saveFileName = Application.isEditor ? SAVE_FILE_NAME_EDITOR : SAVE_FILE_NAME;
#if !UNITY_6000_6_OR_NEWER
                        SerializationHelper.SerializePersistent(SaveDatabase, saveFileName, useJsonSerialization);
#else
                        SerializationHelper.SerializePersistent(SaveDatabase, saveFileName);
#endif
                    }
                    catch (Exception e)
                    {
                        threadException = e;
                    }
                    finally
                    {
                        completed = true;
                    }
                });

                saveThread.Start();

                yield return new WaitUntil(() => completed);

                if (threadException != null)
                {
                    Debug.LogException(threadException);
                }
            }
            else
            {
                try
                {
                    var saveFileName = Application.isEditor ? SAVE_FILE_NAME_EDITOR : SAVE_FILE_NAME;
#if !UNITY_6000_6_OR_NEWER
                    SerializationHelper.SerializePersistent(SaveDatabase, saveFileName, useJsonSerialization);
#else
                    SerializationHelper.SerializePersistent(SaveDatabase, saveFileName);
#endif
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("Save file is updated");

            isSaving = false;
            saveCoroutine = null;
        }

        private void ForceSave()
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

            if (saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString("save", JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SAVE_FILE_NAME);
                Debug.Log("Save file is updated");
#else
                var saveFileName = Application.isEditor ? SAVE_FILE_NAME_EDITOR : SAVE_FILE_NAME;

                if (!SerializationHelper.IsFileLocked(saveFileName))
                {
#if !UNITY_6000_6_OR_NEWER
                    SerializationHelper.SerializePersistent(SaveDatabase, saveFileName, useJsonSerialization);
#else
                    SerializationHelper.SerializePersistent(SaveDatabase, saveFileName);
#endif
                    Debug.Log("Save file is updated");
                }
#endif
            }
        }

        /// <summary>
        /// Saves the current state of the game to the file system
        /// </summary>
        /// <param name="multithreading"> if true, saves the file in another thread. Do not use multitherading in OnDestroy</param>
        public void Save(bool multithreading = false)
        {
            if (SaveDatabase == null) return;
            SaveDatabase.Flush();

            if (saveType == SaveType.PlayerPrefs)
            {
                PlayerPrefs.SetString("save", JsonUtility.ToJson(SaveDatabase));
                PlayerPrefs.Save();

                Debug.Log("Save Database is sent to PlayerPrefs");
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLSave(SaveDatabase, SAVE_FILE_NAME);
                Debug.Log("Save file is updated");
#else
                if (saveCoroutine == null) saveCoroutine = StartCoroutine(SaveCoroutine(multithreading));
#endif
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void WebGLSave(SaveDatabase saveDatabase, string fileName)
        {
            string jsonObject = JsonUtility.ToJson(saveDatabase);

            save(fileName, jsonObject);
        }
#endif

        private IEnumerator AutoSaveCoroutine()
        {
            var wait = new WaitForSecondsRealtime(autoSaveDelay);

            while (true)
            {
                yield return wait;

                Save(true);
            }
        }

        public static void DeleteSaveFile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            deleteItem(SAVE_FILE_NAME);
#else
            SerializationHelper.DeletePersistent(SAVE_FILE_NAME);
            SerializationHelper.DeletePersistent(SAVE_FILE_NAME_EDITOR);
#endif

            PlayerPrefs.DeleteAll();

            Debug.Log("Save file is deleted!");
        }

        private void OnDestroy()
        {
            ForceSave();
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ForceSave();
#endif
        }

        /// <summary>
        /// Android and IOS Phones minimize applications instead of destroying them 
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
            if(!focus) ForceSave();
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string load(string keyName);

        [DllImport("__Internal")]
        private static extern void save(string keyName, string data);

        [DllImport("__Internal")]
        private static extern void deleteItem(string keyName);
#endif

#if UNITY_EDITOR
        public static SaveDatabase GetSaveDatabaseEditor()
        {
            SaveDatabase saveDatabase = null;

            var saveFileName = SaveManager.SAVE_FILE_NAME_EDITOR;
#if !UNITY_6000_6_OR_NEWER
            
                saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, true, useLogs: false);
                if (saveDatabase == null)
                {
                    saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, false, useLogs: false);
                    if (saveDatabase == null)
                    {
                        saveDatabase = new SaveDatabase();
                    }
                }

                return saveDatabase;

#else
            saveDatabase = SerializationHelper.DeserializePersistent<SaveDatabase>(saveFileName, useLogs: false);

            if (saveDatabase == null)
            {
                saveDatabase = new SaveDatabase();
            }
            return saveDatabase;
#endif
        }

        public static void SaveSaveDatabaseEditor(SaveDatabase saveDatabase)
        {
            var saveFileName = SaveManager.SAVE_FILE_NAME_EDITOR;

#if !UNITY_6000_6_OR_NEWER
            SerializationHelper.SerializePersistent(saveDatabase, saveFileName, true);
#else
            SerializationHelper.SerializePersistent(saveDatabase, saveFileName);
#endif
        }

#endif
        }

    public enum SaveType
    {
        SaveFile,
        PlayerPrefs,
    }
}