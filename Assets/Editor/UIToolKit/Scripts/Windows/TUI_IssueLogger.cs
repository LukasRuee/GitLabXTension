using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUI_GitLabxTensionEditor
{
    public enum Weight
    {
        Success,
        Info,
        Warning,
        Error
    }
    public class TUI_IssueLogger : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _issueLogTree;
        [SerializeField] StyleSheet _styleSheet;
        private ListView Lv_Logs;
        private Button Btn_ClearLogs; 

        [MenuItem("Tools/GitlabXTension/Logger")]
        public static void ShowEditor()
        {
            var wnd = GetWindow<TUI_IssueLogger>();
            wnd.titleContent = new GUIContent("Issue board");
        }
        private void CreateGUI()
        {
            _issueLogTree.CloneTree(rootVisualElement);
            Lv_Logs = rootVisualElement.Query<ListView>("Lv_Logs");
            Btn_ClearLogs = rootVisualElement.Query<Button>("Btn_ClearLogs");

            Btn_ClearLogs.clicked += Logger.ClearLogs;

            UpdateLogs();
        }
        /// <summary>
        /// Updates the Listview
        /// </summary>
        public void UpdateLogs()
        {
            Func<VisualElement> makeItem = () => new Label();

            Action<VisualElement, int> bindItem = (e, i) => { (e as Label).text = Logger.Logs[i].text; e.style.color = Logger.Logs[i].Color; };

            Lv_Logs.makeItem = makeItem;
            Lv_Logs.bindItem = bindItem;
            Lv_Logs.itemsSource = Logger.Logs;
            Lv_Logs.Rebuild();
        }
    }

    static public class Logger
    {
        static public List<Log> Logs { get; private set; } = new List<Log>();
        static TUI_IssueLogger _issueLogger;
        /// <summary>
        /// Adds a log
        /// </summary>
        /// <param name="text">Text data</param>
        /// <param name="weight">Weight parameter</param>
        static public void AddLog(string text, Weight weight)
        {
            Log newLog = new Log(text, weight);
            Logs.Add(newLog);
            CheckLogger();
            _issueLogger.UpdateLogs();
        }
        /// <summary>
        /// Clears the Logs
        /// </summary>
        static public void ClearLogs()
        {
            Logs.Clear();
            _issueLogger.UpdateLogs();
        }
        /// <summary>
        /// Checks if a logger is open
        /// </summary>
        private static void CheckLogger()
        {
            if (_issueLogger == null)
            {
                _issueLogger = EditorWindow.CreateWindow<TUI_IssueLogger>();
            }
        }
    }
    public class Log : Label
    {
        public Weight Weight { get; private set; }
        public Color Color { get; private set; }

        public Log(string text, Weight weight)
        {
            AddToClassList(ussClassName);
            this.text = text;
            switch (weight)
            {
                case Weight.Success:
                    Color = Color.green;
                    break;
                case Weight.Info:
                    Color = Color.white;
                    break;
                case Weight.Warning:
                    Color = Color.yellow;
                    break;
                case Weight.Error:
                    Color = Color.red;
                    break;
            }
        }
    }

}