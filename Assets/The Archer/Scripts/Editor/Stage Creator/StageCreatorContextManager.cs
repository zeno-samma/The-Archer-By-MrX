using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class StageCreatorContextManager
    {
        protected StageListItem bufferedStageObject;
        protected RoomListItem bufferedRoomObject;
        protected WaveListItem bufferedWaveObject;

        public bool HasBufferedStageObject => bufferedStageObject != null;
        public bool HasBufferedRoomObject => bufferedRoomObject != null;
        public bool HasBufferedWaveObject => bufferedWaveObject != null;

        public StageCreatorWindow Window { get; protected set; }

        public StageCreatorContextManager(StageCreatorWindow window)
        {
            Window = window;
        }

        public void ShowContextMenu(StageListItem stage)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Values"), false, () => Copy(stage));
            if (HasBufferedStageObject && bufferedStageObject != stage)
            {
                menu.AddItem(new GUIContent("Paste Values"), false, () => Paste(stage));
            } else
            {
                menu.AddDisabledItem(new GUIContent("Paste Values"));
            }
                
            menu.AddItem(new GUIContent("Duplicate"), false, () => Duplicate(stage));
            menu.AddItem(new GUIContent("Delete"), false, () => Delete(stage));

            menu.ShowAsContext();
        }

        public void ShowContextMenu(RoomListItem room)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Values"), false, () => Copy(room));
            if (HasBufferedRoomObject && bufferedRoomObject != room)
            {
                menu.AddItem(new GUIContent("Paste Values"), false, () => Paste(room));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste Values"));
            }

            menu.AddItem(new GUIContent("Duplicate"), false, () => Duplicate(room));
            menu.AddItem(new GUIContent("Delete"), false, () => Delete(room));

            menu.ShowAsContext();
        }

        public void ShowContextMenu(WaveListItem wave)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Values"), false, () => Copy(wave));
            if (HasBufferedWaveObject && bufferedWaveObject != wave)
            {
                menu.AddItem(new GUIContent("Paste Values"), false, () => Paste(wave));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste Values"));
            }

            menu.AddItem(new GUIContent("Duplicate"), false, () => Duplicate(wave));
            menu.AddItem(new GUIContent("Delete"), false, () => Delete(wave));

            menu.ShowAsContext();
        }

        protected virtual void Copy(StageListItem stageObject)
        {
            bufferedStageObject = stageObject;
        }

        protected virtual void Copy(RoomListItem roomObject)
        {
            if(roomObject == StageCreatorWindow.Instance.StagePage.RoomsListContainer.RoomsListView.SelectedItem)
            {
                StageCreatorWindow.Instance.StagePage.SaveScene();
            }
            bufferedRoomObject = roomObject;
        }

        protected virtual void Copy(WaveListItem waveObject)
        {
            if(waveObject == StageCreatorWindow.Instance.StagePage.WavesListContainer.WavesListView.SelectedItem)
            {
                StageCreatorWindow.Instance.StagePage.SaveScene();
            }
            bufferedWaveObject = waveObject;
        }

        protected virtual void Paste(StageListItem stageObject)
        {
            if (HasBufferedStageObject && bufferedStageObject != stageObject)
            {
                Undo.RecordObject(stageObject.TargetObject, "Paste Stage Values");
                stageObject.PasteValuesFrom(bufferedStageObject);
            }
        }

        protected virtual void Paste(RoomListItem roomObject)
        {
            if (HasBufferedRoomObject && bufferedRoomObject != roomObject)
            {
                bool clearedScene = false;
                if(roomObject == StageCreatorWindow.Instance.StagePage.RoomsListContainer.SelectedRoom)
                {
                    StageCreatorWindow.Instance.StagePage.ClearScene();
                    clearedScene = true;
                }
                Undo.RecordObject(roomObject.RoomProperty.serializedObject.targetObject, "Paste Stage Values");
                roomObject.PasteValuesFrom(bufferedRoomObject);

                if(clearedScene) StageCreatorWindow.Instance.StagePage.LoadScene();
            }
        }

        protected virtual void Paste(WaveListItem waveObject)
        {
            if (HasBufferedWaveObject && bufferedWaveObject != waveObject)
            {
                bool clearedScene = false;
                if(waveObject == StageCreatorWindow.Instance.StagePage.WavesListContainer.WavesListView.SelectedItem)
                {
                    StageCreatorWindow.Instance.StagePage.ClearSceneEnemies();
                    clearedScene = true;
                }
                Undo.RecordObject(waveObject.WaveProperty.serializedObject.targetObject, "Paste Stage Values");
                waveObject.PasteValuesFrom(bufferedWaveObject);

                if (clearedScene) StageCreatorWindow.Instance.StagePage.LoadSceneEnemies();
            }
        }

        protected virtual void Duplicate(StageListItem stageObject)
        {
            Window.StageDatabasePage.DuplicateStageAsset(stageObject.StageObject);
        }

        protected virtual void Duplicate(RoomListItem roomObject)
        {
            Window.StagePage.RoomsListContainer.DuplicateRoom(roomObject.RoomProperty);
        }

        protected virtual void Duplicate(WaveListItem waveObject)
        {
            Window.StagePage.WavesListContainer.DuplicateWave(waveObject.WaveProperty);
        }

        protected virtual void Delete(StageListItem stageObject)
        {
            Window.StageDatabasePage.DeleteStageAsset(stageObject.StageObject);
        }

        protected virtual void Delete(RoomListItem roomObject)
        {
            Window.StagePage.RoomsListContainer.DeleteRoom(roomObject.RoomProperty);
        }

        protected virtual void Delete(WaveListItem waveObject)
        {
            Window.StagePage.WavesListContainer.DeleteWave(waveObject.WaveProperty);
        }
    }
}