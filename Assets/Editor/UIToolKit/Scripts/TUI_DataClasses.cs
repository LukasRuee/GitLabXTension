using System;

namespace TUI_GitLabxTensionEditor
{
    #nullable enable
    public struct TUI_UserData
    {
        public int ProjectId { get; private set; }
        public string GitLabAPIUrl
        {
            get => _gitLabProjectUrl + "/api/v4";   
            private set{}
        }
        private string _gitLabProjectUrl;
        public string GitLabProjectUrl
        {
            get => _gitLabProjectUrl;
            private set
            {
                _gitLabProjectUrl = value;
                GitLabAPIUrl = value + "/api/v4";
            }
        }
        public void ResetData()
        {
            ProjectId = 0;
            GitLabAPIUrl = string.Empty;
            _gitLabProjectUrl = string.Empty;
        }
        public void SetData(int newProjectId, string newGitLabProjectUrl)
        {
            ProjectId = newProjectId;
            _gitLabProjectUrl = newGitLabProjectUrl;
        }
    }
    #region GitLab datastructs
    public struct TUI_NewIssue
    {
        public string project_id; //URL-encoded path of the project
        public int assignee_id; //multiple ids with gitlab premium / ultimate
        public bool confidential;
        public string description;
        public string due_date;
        public string issue_type;
        public string labels; //Comma-separated label names for an issue.
        public int milestone_id;
        public string? milestone_name;
        public string title; // must be filled out
    }
    public struct TUI_Issue
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public TUI_Assignee? closed_by { get; set; }
        public string[]? labels { get; set; }
        public TUI_Milestone? milestone { get; set; }
    }
    public struct TUI_Assignee
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }
    public struct TUI_User
    {
        public int id;
        public string username;
    }
    public struct TUI_Project
    {
        public int id;
        public string name;
    }
    public struct TUI_Milestone
    {
        public int id;
        public int[]? iids;
        public int? project_id;
        public string? title;
        public string? description;
        public string? state;
        public DateTime? created_at;
        public DateTime? updated_at;
        public DateTime? due_date;
        public DateTime? start_date;
        public bool? expired;
        public string? web_url;
    }
    public struct TUI_Label
    {
        public int id;
        public string name;
        public string? description;
        public string? description_html;
        public string? text_color;
        public string? color;
        public bool? subscribed;
        public int? priority;
        public bool? is_project_label;
    }
    #endregion
}
