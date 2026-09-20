using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace OctoberStudio.Enemy
{
    public class EnemyTypeWindow : EditorWindow
    {
        [SerializeField] VisualTreeAsset visualTree;

        protected VisualElement root;
        protected ScrollView scrollView;

        protected List<EnumData> dataList;

        protected Button saveButton;

        public static void ShowWindow()
        {
            EnemyTypeWindow wnd = GetWindow<EnemyTypeWindow>(false, "Enemy Type", true);
        }

        public virtual void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            VisualElement container = visualTree.Instantiate();

            scrollView = container.Q<ScrollView>("ScrollView");
            var resetButton = container.Q<Button>("Reset_Button");
            saveButton = container.Q<Button>("Save_Button");
            var addButton = container.Q<Button>("Add_Button");

            resetButton.clicked += ResetButtonClicked;
            saveButton.clicked += SaveButtonClicked;
            addButton.clicked += AddButtonClicked;

            ResetButtonClicked();

            root.Add(container);
        }

        protected virtual void CreateList()
        {
            var values = new List<EnemyType>((EnemyType[])Enum.GetValues(typeof(EnemyType)));
            dataList = new List<EnumData>();

            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];

                var data = new EnumData();
                data.name = value.ToString();
                data.id = (int)value;
                dataList.Add(data);

                scrollView.Add(data.Create(Delete));
            }
        }

        protected virtual void Delete(EnumData data)
        {
            data.visualElement.Clear();
            scrollView.Remove(data.visualElement);

            dataList.Remove(data);
        }

        protected virtual void ResetButtonClicked()
        {
            scrollView.Clear();
            CreateList();
        }

        protected virtual void SaveButtonClicked()
        {
            var name = "EnemyType";
            MonoScript script = null;

            string[] assets = AssetDatabase.FindAssets((string.IsNullOrEmpty(name) ? "" : name + " ") + "t:" + typeof(MonoScript).Name);
            if (assets.Length > 0)
            {
                string assetPath;
                for (int i = 0; i < assets.Length; i++)
                {
                    assetPath = AssetDatabase.GUIDToAssetPath(assets[i]);
                    if (Path.GetFileNameWithoutExtension(assetPath) == name)
                    {
                        script = (MonoScript)AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript));
                    }
                }
            }

            var path = Application.dataPath.Replace("/Assets", "/") + AssetDatabase.GetAssetPath(script);

            for (int i = 0; i < dataList.Count; i++)
            {
                dataList[i].Save();
            }

            UpdateEnum(path);
        }

        protected virtual void AddButtonClicked()
        {
            var data = new EnumData();

            data.name = "--CHANGE--";
            data.id = dataList.Count;
            dataList.Add(data);

            scrollView.Add(data.Create(Delete));
        }

        protected virtual void Update()
        {
            bool canBeSaved = true;

            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];

                bool isNameEmpty = data.nameField.text == "";
                data.SetNameBackWarning(isNameEmpty);

                bool isDuplicateName = false;

                for (int j = 0; j < dataList.Count; j++)
                {
                    if (j == i) continue;

                    var test = dataList[j];

                    if (data.nameField.text == test.nameField.text)
                    {
                        isDuplicateName = true;
                        break;
                    }
                }

                bool isNameCorrect = Regex.IsMatch(data.nameField.text, "^[A-Za-z_][A-Za-z0-9_]{0,19}$");

                data.SetNameWarning(!isNameCorrect || isDuplicateName);

                bool isDuplicateId = false;

                for (int j = 0; j < dataList.Count; j++)
                {
                    if (j == i) continue;

                    var test = dataList[j];

                    if (data.idField.value == test.idField.value)
                    {
                        isDuplicateId = true;
                        break;
                    }
                }

                data.SetIdWarning(isDuplicateId);

                canBeSaved = !isNameEmpty && !isDuplicateName && isNameCorrect && !isDuplicateId;
            }

            saveButton.SetEnabled(canBeSaved);
        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {

        }

        protected virtual void UpdateEnum(string path)
        {
            var enumText = new System.Text.StringBuilder();
            enumText.Append("namespace OctoberStudio.Enemy");
            enumText.AppendLine();
            enumText.Append("{");
            enumText.AppendLine();
            enumText.Append("\tpublic enum EnemyType");
            enumText.AppendLine();
            enumText.Append("\t{");

            for (int i = 0; i < dataList.Count; i++)
            {
                enumText.AppendLine();
                enumText.Append($"\t\t{dataList[i].name} = {dataList[i].id},");
            }

            enumText.AppendLine();
            enumText.Append("\t}");
            enumText.AppendLine();
            enumText.Append("}");

            System.IO.File.WriteAllText(path, enumText.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        protected class EnumData
        {
            public string name;
            public int id;

            public VisualElement visualElement;
            public TextField nameField;
            public IntegerField idField;

            protected VisualElement nameInput;
            protected TextElement nameElement;
            protected VisualElement idInput;
            protected TextElement idElement;

            protected Color backColor;
            protected Color textColor;

            public virtual void Save()
            {
                name = nameField.value;
                id = idField.value;
            }

            public virtual VisualElement Create(UnityAction<EnumData> delete)
            {
                visualElement = new VisualElement();
                visualElement.style.flexDirection = FlexDirection.Row;

                nameField = new TextField();

                nameField.value = name;
                nameField.style.flexGrow = 0.6f;
                nameField.style.flexBasis = 1;

                nameInput = nameField.Q("unity-text-input");
                nameElement = nameInput.Q<TextElement>();

                backColor = nameInput.style.backgroundColor.value;
                textColor = nameElement.style.color.value;

                visualElement.Add(nameField);

                idField = new IntegerField();
                idField.value = id;
                idField.style.flexGrow = 0.2f;
                idField.style.flexBasis = 0.5f;

                idInput = idField.Q("unity-text-input");
                idElement = idInput.Q<TextElement>();

                visualElement.Add(idField);

                var deleteButton = new Button();
                deleteButton.text = "Delete";
                deleteButton.style.flexGrow = 0.2f;
                deleteButton.clicked += () => delete.Invoke(this);

                visualElement.Add(deleteButton);

                return visualElement;
            }

            public virtual void SetNameBackWarning(bool value)
            {
                nameInput.style.backgroundColor = value ? Color.red : backColor;
            }

            public virtual void SetNameWarning(bool value)
            {
                nameElement.style.color = value ? Color.red : new Color(0.8f, 0.8f, 0.8f, 1);
            }

            public virtual void SetIdWarning(bool value)
            {
                idElement.style.color = value ? Color.red : new Color(0.8f, 0.8f, 0.8f, 1);
            }
        }
    }
}