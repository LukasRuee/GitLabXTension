using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUI_GitLabxTensionEditor.Editor
{
    public class TUI_IssueBoard : EditorWindow
    {
        static List<TUI_Issue> _issues = new List<TUI_Issue>();
        [SerializeField] private VisualTreeAsset _issueBoardTree;
        [SerializeField] StyleSheet _styleSheet;
        private ListView Lv_Issues;
        private VisualElement Ve_IssueTitles;
        private VisualElement Ve_IssueDescription;
        private Label Lbl_IssueDescription;
        private Button Btn_IssueCreator;
        private ProgressBar Pg_FetchIssues;

        [MenuItem("Tools/GitlabXTension/Issue Board")]
        public static void ShowEditor()
        {
            if (TUI_SecurityManager.IsLoggedIn)
            {
                var wnd = GetWindow<TUI_IssueBoard>();
                wnd.titleContent = new GUIContent("Issue board");
            }
            else
            {
                Logger.AddLog("Please log in first", Weight.Warning);
                CreateWindow<TUI_GitLabxTensionAuthentication>();
            }
        }
        private void CreateGUI()
        {
            _issueBoardTree.CloneTree(rootVisualElement);

            Btn_IssueCreator = rootVisualElement.Query<Button>("Btn_IssueCreator");
            Btn_IssueCreator.clickable.clicked += OnButtonClicked;

            Lbl_IssueDescription = rootVisualElement.Query<Label>("Lbl_IssueDescription");

            Ve_IssueTitles = rootVisualElement.Query<VisualElement>("Ve_IssueTitles");
            Ve_IssueDescription = rootVisualElement.Query<VisualElement>("Ve_IssueDescription");

            Lv_Issues = rootVisualElement.Query<ListView>("Lv_Issues");
            Pg_FetchIssues = rootVisualElement.Query<ProgressBar>("Pg_FetchIssues");

            FetchData();
        }
        private async void FetchData()
        {
            string api_url = TUI_SecurityManager.LoggedInUserSettings.GitLabAPIUrl;
            int project_id = TUI_SecurityManager.LoggedInUserSettings.ProjectId;
            _issues = await TUI_ApiProtokoll.GetDataListFromURL<TUI_Issue>($"{api_url}/projects/{project_id}/issues", ReportProgress);

            if (_issues.Count > 0)
            {
                Lv_Issues = CreateIssueList();
                Ve_IssueTitles.Add(Lv_Issues);
                Lv_Issues.Rebuild();
                Lv_Issues.selectionChanged += OnItemChosen;
            }
            else
            {
                Lv_Issues = new ListView();
            }
            Pg_FetchIssues.parent.Remove(Pg_FetchIssues);
        }
        /// <summary>
        /// Progress callback to update ProgressBar
        /// </summary>
        /// <param name="progress">Current progress (0 - 1)</param>
        private void ReportProgress(float progress)
        {
            // Convert progress to integer percentage
            float percentage = (progress * Pg_FetchIssues.highValue);

            // Ensure percentage doesn't exceed 100
            percentage = Mathf.Clamp(percentage, Pg_FetchIssues.lowValue, Pg_FetchIssues.highValue);

            // Update ProgressBar value
            Pg_FetchIssues.value = percentage;
        }

        /// <summary>
        /// If a item gets selected
        /// </summary>
        /// <param name="obj">The selected item</param>
        private void OnItemChosen(object obj)
        {
            string text = _issues[Lv_Issues.selectedIndex].description;
            if (string.IsNullOrEmpty(text))
            {
                Lbl_IssueDescription.text = "No description";
            }
            {
                Lbl_IssueDescription.text = text;
            }
        }
        /// <summary>
        /// Created the Issue List
        /// </summary>
        /// <returns>Returns a Listview with the issues</returns>
        public ListView CreateIssueList()
        {
            Func<VisualElement> makeItem = () => new Label();
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = _issues[i].title;
            int itemHeight = 16;
            return new ListView(_issues, itemHeight, makeItem, bindItem);
        }
        /// <summary>
        /// If the button gets clicked
        /// </summary>
        private void OnButtonClicked()
        {
            CreateWindow<TUI_IssueCreator>();
        }
    }
}
