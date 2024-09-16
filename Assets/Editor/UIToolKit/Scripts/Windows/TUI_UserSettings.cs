using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUI_GitLabxTensionEditor.Editor
{
    public class TUI_UserSettings : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _issueUserSettingsTree;
        private Button Btn_SetPAT;
        private Button Btn_SetPW;
        private Button Btn_SetSettings;

        private TextField TF_PAT;
        private TextField TF_GitUrl;
        private IntegerField IF_ProjectID;
        private TextField TF_NewPW;

        private int _newID;
        private string _newURL;
        private TUI_UserData _newUserSettings;

        [MenuItem("Tools/GitlabXTension/User settings")]
        public static void ShowEditor()
        {
            if (TUI_SecurityManager.IsLoggedIn)
            {
                var wnd = GetWindow<TUI_UserSettings>();
                wnd.titleContent = new GUIContent("User settings");
            }
            else
            {
                Logger.AddLog("Please log in first", Weight.Warning);
                CreateWindow<TUI_GitLabxTensionAuthentication>();
            }
        }
        private void CreateGUI()
        {
            _issueUserSettingsTree.CloneTree(rootVisualElement);
            //Get elements
            Btn_SetPAT = rootVisualElement.Query<Button>("Btn_SetPAT");
            Btn_SetPW = rootVisualElement.Query<Button>("Btn_SetPW");
            Btn_SetSettings = rootVisualElement.Query<Button>("Btn_SetSettings");

            TF_PAT = rootVisualElement.Query<TextField>("TF_PAT");
            TF_GitUrl = rootVisualElement.Query<TextField>("TF_GitUrl");
            TF_NewPW = rootVisualElement.Query<TextField>("TF_NewPW");

            IF_ProjectID = rootVisualElement.Query<IntegerField>("IF_ProjectID");

            //Subscriptions
            Btn_SetPAT.clickable.clicked += SetPAT;
            Btn_SetPW.clickable.clicked += ResetPassword;
            Btn_SetSettings.clickable.clicked += SetNewSettings;

            _newURL = TUI_SecurityManager.LoggedInUserSettings.GitLabProjectUrl;
            _newID = TUI_SecurityManager.LoggedInUserSettings.ProjectId;

            TF_GitUrl.value = _newURL;
            IF_ProjectID.value = _newID;
        }
        /// <summary>
        /// Sets the new settings for the user
        /// </summary>
        private void SetNewSettings()
        {
            _newUserSettings.SetData(IF_ProjectID.value, TF_GitUrl.value);
            TUI_SecurityManager.SaveUserSettings(_newUserSettings);
        }
        /// <summary>
        /// Overrides old password with the new one
        /// </summary>
        private void ResetPassword()
        {
            TUI_SecurityManager.ResetPassword(TUI_SecurityManager.LoggedInUser, TF_NewPW.value);
        }
        /// <summary>
        /// Overrides old PAT with the new one
        /// </summary>
        private void SetPAT()
        {
            TUI_SecurityManager.SavePAT(TF_PAT.value);
        }
    }
}
