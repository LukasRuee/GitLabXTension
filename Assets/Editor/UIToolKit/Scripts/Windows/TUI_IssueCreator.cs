using Codice.Utils;
using System;
using System.Collections.Generic;
using TUI_GitLabxTensionEditor.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUI_GitLabxTensionEditor
{
    public class TUI_IssueCreator : EditorWindow
    {
        private List<TUI_Milestone> _milestones = new List<TUI_Milestone>();
        private List<int> _milestoneIds = new List<int>();
        private List<string> _milestonesTitles = new List<string>();
        private int[] _milestonesIntArray;

        private List<string> _selectedLabels = new List<string>();
        private List<TUI_Label> _labels = new List<TUI_Label>();
        private List<string> _labelsNames = new List<string>();

        private TUI_User _user;
        private TUI_Project _project;
        private TUI_NewIssue _issue;

        [SerializeField] private VisualTreeAsset _issueCreatorTree;
        private TextField Tf_Title;
        private TextField Tf_Description;
        private Button Btn_IssueCreator;
        private Button Btn_AssignMe;
        private Button Btn_DueToday; 
        private TextField Tf_DueDate;
        private Toggle Tgl_IsIncident;
        private Toggle Tgl_IsConfidential;
        private IntegerField If_AsigneeID;
        private DropdownField DdF_Milestones;
        private DropdownField DdF_Labels;
        private Label Lbl_Labels;
        private ProgressBar Pg_CreateIssue;

        string _api_url;
        string _project_url;
        int _project_id;
        public static void ShowEditor()
        {
            var wnd = GetWindow<TUI_IssueCreator>();
            wnd.titleContent = new GUIContent("Issue creator");
        }
        private void CreateGUI()
        {
            _issueCreatorTree.CloneTree(rootVisualElement);

            Tf_Title = rootVisualElement.Query<TextField>("Tf_Title");
            Tf_Description = rootVisualElement.Query<TextField>("Tf_Description");

            Btn_IssueCreator = rootVisualElement.Query<Button>("Btn_IssueCreator");
            Btn_AssignMe = rootVisualElement.Query<Button>("Btn_AssignMe");
            Btn_DueToday = rootVisualElement.Query<Button>("Btn_DueToday");

            Tgl_IsIncident = rootVisualElement.Query<Toggle>("Tgl_IsIncident");
            Tgl_IsConfidential = rootVisualElement.Query<Toggle>("Tgl_IsConfidential");

            DdF_Milestones = rootVisualElement.Query<DropdownField>("DdF_Milestones");
            DdF_Labels = rootVisualElement.Query<DropdownField>("DdF_Labels");

            Lbl_Labels = rootVisualElement.Query<Label>("Lbl_Labels");

            If_AsigneeID = rootVisualElement.Query<IntegerField>("If_AsigneeID");
            Tf_DueDate = rootVisualElement.Query<TextField>("Tf_DueDate");

            Pg_CreateIssue = rootVisualElement.Query<ProgressBar>("Pg_CreateIssue");
            FetchIssueData();

            //Subscriptions
            Btn_IssueCreator.clickable.clicked += CreateIssueAsync; 
            Btn_AssignMe.clickable.clicked += AssignMe;
            Btn_DueToday.clickable.clicked += SetTime;

            Tgl_IsIncident.RegisterValueChangedCallback(evt => SetIssueType(evt.newValue));
            Tgl_IsConfidential.RegisterValueChangedCallback(evt => SetConfidential(evt.newValue));

            DdF_Milestones.choices.Clear();
            DdF_Labels.choices.Clear();

            PopulateDropdown(DdF_Milestones, _milestonesTitles);
            PopulateDropdown(DdF_Labels, _labelsNames);

            DdF_Milestones.RegisterValueChangedCallback(evt => SetMilestone(evt.newValue));
            DdF_Labels.RegisterValueChangedCallback(evt => ToggleLabels(evt.newValue));

            Tf_Title.RegisterValueChangedCallback(evt => SetTitle(evt.newValue));
            Tf_Description.RegisterValueChangedCallback(evt => SetDescription(evt.newValue));
        }
        /// <summary>
        /// Sets current Time
        /// </summary>
        private void SetTime()
        {
            Tf_DueDate.value = DateTime.Now.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// Sets title text
        /// </summary>
        /// <param name="value"></param>
        private void SetTitle(string value)
        {
            _issue.title = value;
        }
        /// <summary>
        /// Sets description text
        /// </summary>
        /// <param name="value"></param>
        private void SetDescription(string value)
        {
            _issue.description = value;
        }
        /// <summary>
        /// Fetches issuedata from gitlab
        /// </summary>
        private async void FetchIssueData()
        {
            //Get user data
            _api_url = TUI_SecurityManager.LoggedInUserSettings.GitLabAPIUrl;
            _project_url = TUI_SecurityManager.LoggedInUserSettings.GitLabProjectUrl;
            _project_id = TUI_SecurityManager.LoggedInUserSettings.ProjectId;

            //fetch issue data
            _issue = new TUI_NewIssue();
            _milestones = await TUI_ApiProtokoll.GetDataListFromURL<TUI_Milestone>($"{_api_url}/projects/{_project_id}/milestones", ReportProgress);
            Pg_CreateIssue.title = "Fetching milestines";
            _labels = await TUI_ApiProtokoll.GetDataListFromURL<TUI_Label>($"{_api_url}/projects/{_project_id}/labels", ReportProgress);
            Pg_CreateIssue.title = "Fetching labels";
            _user = await TUI_ApiProtokoll.GetDataFromURL<TUI_User>($"{_api_url}/user", ReportProgress);
            Pg_CreateIssue.title = "Fetching users";
            _project = await TUI_ApiProtokoll.GetDataFromURL<TUI_Project>($"{_api_url}/projects/{_project_id}", ReportProgress);
            Pg_CreateIssue.title = "Fetching project";

            if (_milestones.Count != 0)
            {
                foreach (var milestone in _milestones)
                {
                    _milestoneIds.Add(milestone.id);
                    _milestonesTitles.Add(milestone.title);
                }
            }
            if (_labels.Count != 0)
            {
                foreach (var label in _labels)
                {
                    _labelsNames.Add(label.name);
                }
            }

            _milestonesIntArray = new int[_milestoneIds.Count];
            _milestoneIds.CopyTo(_milestonesIntArray);
        }
        /// <summary>
        /// Sets the confidential state
        /// </summary>
        /// <param name="value"></param>
        private void SetConfidential(bool value)
        {
            _issue.confidential = value;
        }
        /// <summary>
        /// Sets a milestone
        /// </summary>
        /// <param name="MilestoneName">Selected milestone name</param>
        private void SetMilestone(string MilestoneName)
        {
            _issue.milestone_id = _milestoneIds[_milestonesTitles.IndexOf(MilestoneName)];
        }
        /// <summary>
        /// Sets the selected labels
        /// </summary>
        /// <param name="labelName">Name of the label to set or remove</param>
        private void ToggleLabels(string labelName)
        {
            if (_selectedLabels.Contains(labelName))
            {
                _selectedLabels.Remove(labelName);
            }
            else
            {
                _selectedLabels.Add(labelName);
            }

            if (_selectedLabels.Count == 0)
            {
                Lbl_Labels.text = "No labels selected";
            }
            else
            {
                for(int i = 0; i < _selectedLabels.Count; i++)
                {
                    if(i == 0)
                    {
                        Lbl_Labels.text = _selectedLabels[0];
                    }
                    else
                    {
                        Lbl_Labels.text += $",{_selectedLabels[i]}";
                    }
                }
            }
            DdF_Labels.value = null;
        }
        /// <summary>
        /// Populates a UI Toolkit Dropdownfield
        /// </summary>
        /// <param name="dropdown">Dropdown to populate</param>
        /// <param name="options">List of options</param>
        private void PopulateDropdown(DropdownField dropdown, List<string> options)
        {
            dropdown.choices.Clear();

            foreach (string option in options)
            {
                dropdown.choices.Add(option);
            }
        }
        /// <summary>
        /// Sets the issuetype
        /// </summary>
        /// <param name="isIssue">If its a issue</param>
        private void SetIssueType(bool isIssue)
        {
            if (isIssue)
            {
                _issue.issue_type = "issue";
            }
            else
            {
                _issue.issue_type = "incident";
            }
        }
        /// <summary>
        /// Creates a Issue
        /// </summary>
        private async void CreateIssueAsync()
        {
            try
            {
                foreach(string label in _selectedLabels)
                {
                    for(int i = 0; i < _labels.Count; i++)
                    {
                        if (_labels[i].name == label)
                        {
                            if (i == 0)
                            {
                                _issue.labels += _labels[i].name;
                            }
                            else
                            {
                                _issue.labels += $",{_labels[i].name}";
                            }
                        }
                    }
                }
                _issue.assignee_id = If_AsigneeID.value;
                _issue.due_date = Tf_DueDate.value;
                _issue.project_id = HttpUtility.UrlEncode($"{_project_url}/{_project.name}");
                await TUI_ApiProtokoll.PostDataAsJson(_issue, $"{_api_url}/projects/{_project.id}/issues", ReportProgress);
                Close();
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error creating issue: {ex.Message}", Weight.Error);
            }
        }
        /// <summary>
        /// Progress callback to update ProgressBar
        /// </summary>
        /// <param name="progress">Current progress (0 - 1)</param>
        private void ReportProgress(float progress)
        {
            // Convert progress to integer percentage
            float percentage = (progress * Pg_CreateIssue.highValue);

            // Ensure percentage doesn't exceed 100
            percentage = Mathf.Clamp(percentage, Pg_CreateIssue.lowValue, Pg_CreateIssue.highValue);

            // Update ProgressBar value
            Pg_CreateIssue.value = percentage;
        }
        /// <summary>
        /// Assignes the current logged in user
        /// </summary>
        private void AssignMe()
        {
            If_AsigneeID.value = _user.id;
            _issue.assignee_id = _user.id;
        }
    }
}
