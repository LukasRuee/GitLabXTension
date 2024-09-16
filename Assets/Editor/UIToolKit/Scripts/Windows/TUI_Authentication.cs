using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUI_GitLabxTensionEditor
{
    public class TUI_GitLabxTensionAuthentication : EditorWindow
    {
        private string _userName = string.Empty;
        private string _password = string.Empty;

        [SerializeField] private VisualTreeAsset _authenticationTree;
        private Button Btn_SignIn;
        private Button Btn_LogIn;
        private TextField TF_Username;
        private TextField TF_PW;

        [MenuItem("Tools/GitlabXTension/Authentication")]
        public static void ShowEditor()
        {
            var wnd = GetWindow<TUI_GitLabxTensionAuthentication>();
            wnd.titleContent = new GUIContent("Authentication");
        }
        private void CreateGUI()
        {
            _authenticationTree.CloneTree(rootVisualElement);
            Btn_SignIn = rootVisualElement.Query<Button>("Btn_SignIn");
            Btn_LogIn = rootVisualElement.Query<Button>("Btn_LogIn");
            TF_Username = rootVisualElement.Query<TextField>("TF_Username");
            TF_PW = rootVisualElement.Query<TextField>("TF_PW");

            Btn_SignIn.clickable.clicked += SignIn;
            Btn_LogIn.clickable.clicked += LogIn;
            TF_PW.value = _password;
        }
        /// <summary>
        /// Logs in the user
        /// </summary>
        private void LogIn()
        {
            if (TUI_SecurityManager.LogIn(TF_Username.value, TF_PW.value))
            {
                GetWindow<TUI_GitLabxTensionAuthentication>().Close();
            }
            else
            {
                Logger.AddLog("Wrong username or password", Weight.Info);
            }
        }
        /// <summary>
        /// Checks if this user can be signed in
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        private bool CheckSignInInput(string Input)
        {
            if (!TUI_SecurityManager.IsNewUsernameValid(TF_Username.value))
            {
                Logger.AddLog("Please set a username with those rules:\n" +
                $"At least {TUI_SecurityManager.MinimumLetters} letters\n" +
                "does not conatin a whitespace", Weight.Info);
                return false;
            }
            else if (!TUI_SecurityManager.IsUsernameAvailable(TF_Username.value))
            {
                Logger.AddLog("This username is not avivable", Weight.Info);
                return false;
            }
            else if (!TUI_SecurityManager.IsNewPasswordValid(TF_PW.value))
            {
                Logger.AddLog("Please set a password with those rules:\n" +
                $"At least {TUI_SecurityManager.MinimumLetters} letters\n" +
                "A uppercase letter\n" +
                "a number\n" +
                "a special character\n" +
                "does not conatin a whitespace", Weight.Info);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Signs the user in
        /// </summary>
        private void SignIn()
        {
            if(CheckSignInInput(_userName) && CheckSignInInput(_password))
            {
                TUI_SecurityManager.RegisterUser(TF_Username.value, TF_PW.value);
            }
        }
    }
}