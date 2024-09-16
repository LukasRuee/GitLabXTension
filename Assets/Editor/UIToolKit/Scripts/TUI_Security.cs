//using Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace TUI_GitLabxTensionEditor
{
    static public class TUI_SecurityManager
    {
        //Public information
        static public bool IsLoggedIn { get; private set; }
        static public TUI_UserData LoggedInUserSettings { get; private set; }
        static public string LoggedInUser { get; private set; }
        //Cript key
        static private byte[] _encryptionKey;
        static private byte[] _encryptionIV;
        //Local infofmation
        static private string _userDataFolderPath = Application.persistentDataPath + "/UserData";
        static public int MinimumLetters { get; private set; } = 6;

        #region public functions
        /// <summary>
        /// Saves the new PAT
        /// </summary>
        /// <param name="personalAccessToken"></param>
        static public void SavePAT(string personalAccessToken)
        {
            try
            {
                string userFolderPath = Path.Combine(_userDataFolderPath, LoggedInUser);
                CheckFilePath(userFolderPath);
                string userDataFilePath = Path.Combine(userFolderPath, "API.json");

                byte[] PAT = TUI_Crypter.EncryptStringToBytes(personalAccessToken, _encryptionKey, _encryptionIV);

                File.WriteAllBytes(Path.Combine(userFolderPath, "API.json"), PAT);
                Logger.AddLog("Succesfully saved PAT", Weight.Success);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error saving user data for {LoggedInUser}: {ex.Message}", Weight.Error);
            }
        }
        /// <summary>
        /// Gets the PAT for the API
        /// </summary>
        /// <returns>PAT as a string</returns>
        static public string GetPAT()
        {
            try
            {
                string userFilePath = Path.Combine(_userDataFolderPath, LoggedInUser, "API.json");

                if (File.Exists(userFilePath))
                {
                    byte[] APIData = File.ReadAllBytes(userFilePath);
                    return TUI_Crypter.DecryptStringFromBytes(APIData, _encryptionKey, _encryptionIV);
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error loading user data for {LoggedInUser}: {ex.Message}", Weight.Error);
            }
            return null;
        }
        /// <summary>
        /// Logs the current logged in user out
        /// </summary>
        static public void LogOut()
        {
            _encryptionKey = null;
            _encryptionIV = null;
            IsLoggedIn = false;
            LoggedInUserSettings.ResetData();
            LoggedInUser = string.Empty;
            Logger.AddLog($"Successfully logged out", Weight.Success);
        }
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="username">The new username</param>
        /// <param name="password">The new password</param>
        /// <returns>If successfull true, else false</returns>
        static public void RegisterUser(string username, string password)
        {
            if (IsUsernameAvailable(username))
            {
                CreateCryptionKey(username);

                string salt = TUI_Hash.GenerateSalt();
                string hashedPassword = TUI_Hash.HashData(password, salt);

                SaveUserCredentials(username, hashedPassword, salt);
                Logger.AddLog($"Successfully created user.", Weight.Success);
            }
            else
            {
                Logger.AddLog($"Username '{username}' is already taken.", Weight.Warning);
            }
        }
        /// <summary>
        /// Checks if the password is valid
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns>Returns true if valid, else returns false</returns>
        static public bool IsNewPasswordValid(string newPassword)
        {
            int letterCount = newPassword.Count(char.IsLetter);
            if (!newPassword.Any(char.IsDigit) || !newPassword.Any(char.IsUpper) || letterCount < MinimumLetters || newPassword.Any(char.IsWhiteSpace) ||
                (!newPassword.Any(char.IsSymbol) && !newPassword.Any(char.IsPunctuation)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Sets a new password
        /// </summary>
        /// <param name="username">The new username</param>
        /// <param name="password">The new password</param>
        static public void ResetPassword(string username, string password)
        {
            if(IsNewPasswordValid(password) && IsNewUsernameValid(username))
            {
                LoggedInUser = string.Empty;
                IsLoggedIn = false;
                string salt = TUI_Hash.GenerateSalt();
                string hashedPassword = TUI_Hash.HashData(password, salt);
                SaveUserCredentials(username, hashedPassword, salt);
                Logger.AddLog($"Successfully set password", Weight.Success);
            }
            Logger.AddLog($"Password or Username is not in the correct format", Weight.Error);
        }
        /// <summary>
        /// Checks if the new username is valid
        /// </summary>
        /// <param name="newUsername"></param>
        /// <returns>Returns true if valid, else returns false</returns>
        static public bool IsNewUsernameValid(string newUsername)
        {
            int letterCount = newUsername.Count(char.IsLetter);
            if (letterCount < MinimumLetters || newUsername.Any(char.IsWhiteSpace))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Saves the current usersettings
        /// </summary>
        /// <param name="newSettings">The new Settings to set</param>
        static public void SaveUserSettings(TUI_UserData newSettings)
        {
            LoggedInUserSettings = newSettings;
            try
            {
                string userFolderPath = Path.Combine(_userDataFolderPath, LoggedInUser);
                CheckFilePath(userFolderPath);
                string userDataFilePath = Path.Combine(userFolderPath, "userSettings.json");

                var userData = new
                {
                    ProjectId = TUI_Crypter.EncryptStringToBytes(newSettings.ProjectId.ToString(), _encryptionKey, _encryptionIV),
                    GitLabProjectUrl = TUI_Crypter.EncryptStringToBytes(newSettings.GitLabProjectUrl, _encryptionKey, _encryptionIV)
                };
                string userDataJson = JsonConvert.SerializeObject(userData);

                File.WriteAllText(Path.Combine(userFolderPath, "userSettings.json"), userDataJson);
                Logger.AddLog($"Successfully stored data", Weight.Success);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error saving user data for {LoggedInUser}: {ex.Message}", Weight.Error);
            }
        }
        /// <summary>
        /// Retrieve the UserSettings from Local
        /// </summary>
        /// <returns>Users settings as TUI_Settings</returns>
        static private TUI_UserData GetUserSettings()
        {
            TUI_UserData newSettings = new TUI_UserData();
            try
            {
                string userFilePath = Path.Combine(_userDataFolderPath, LoggedInUser, "userSettings.json");

                if (File.Exists(userFilePath))
                {
                    string userDataString = File.ReadAllText(userFilePath);
                    dynamic userData = JsonConvert.DeserializeObject(userDataString);

                    byte[] projectIdBytes = userData.ProjectId;
                    byte[] gitLabProjectUrlBytes = userData.GitLabProjectUrl;

                    int newProjectId = int.Parse(TUI_Crypter.DecryptStringFromBytes(projectIdBytes, _encryptionKey, _encryptionIV));
                    string newGitLabProjectUrl = TUI_Crypter.DecryptStringFromBytes(gitLabProjectUrlBytes, _encryptionKey, _encryptionIV);

                    newSettings.SetData(newProjectId, newGitLabProjectUrl);
                    return newSettings;
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error loading user data for {LoggedInUser}: {ex.Message}", Weight.Error);
            }
            return newSettings;
        }
        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <param name="username">unsername to login</param>
        /// <param name="password">password to login</param>
        /// <returns></returns>
        static public bool LogIn(string username, string password)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
            {
                Logger.AddLog($"Please enter something", Weight.Warning);
                return false;
            }
            (string savedHashedPassword, byte[] savedSalt) = LoadUserCredentials(username);
            if (VerifyHashedPassword(password, savedHashedPassword, savedSalt))
            {
                IsLoggedIn = true;
                LoggedInUser = username;
                (_encryptionKey, _encryptionIV) = GetCryptionKeys(username);
                LoggedInUserSettings = GetUserSettings();
                Logger.AddLog($"Successfully logged in {username}", Weight.Success);
                return true;
            }
            Logger.AddLog($"Could not login {username}", Weight.Warning);
            return false;
        }
        /// <summary>
        /// Checks if a username is avivable / not already set
        /// </summary>
        /// <param name="username"></param>
        /// <returns>If successfull true, else false</returns>
        static public bool IsUsernameAvailable(string username)
        {
            try
            {
                string userFolderPath = Path.Combine(_userDataFolderPath, username);
                return !Directory.Exists(userFolderPath);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error checking if username '{username}' is available: {ex.Message}", Weight.Error);
                return false;
            }
        }
        #endregion
        #region security functions
        /// <summary>
        /// Saves the user credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="salt"></param>
        static private void SaveUserCredentials(string username, string hashedPassword, string salt)
        {
            try
            {
                string userFolderPath = Path.Combine(_userDataFolderPath, username);
                CheckFilePath(userFolderPath);

                File.WriteAllText(Path.Combine(userFolderPath, "credentials.json"), hashedPassword);
                File.WriteAllText(Path.Combine(userFolderPath, "salt.json"), salt);
                Logger.AddLog($"Successfully saved uder credentials", Weight.Success);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error saving user data for {username}: {ex.Message}", Weight.Error);
            }
        }
        /// <summary>
        /// Creates a cryptionkey
        /// </summary>
        /// <param name="username">Key will be only for this user</param>
        static private void CreateCryptionKey(string username)
        {
            try
            {
                byte[] newKey = null;
                byte[] newIV = null;
                (newKey, newIV) = TUI_Crypter.GenerateKeys();

                string userFolderPath = Path.Combine(_userDataFolderPath, username);
                CheckFilePath(userFolderPath);

                File.WriteAllBytes(Path.Combine(userFolderPath, "key.json"), newKey);
                File.WriteAllBytes(Path.Combine(userFolderPath, "iv.json"), newIV);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error saving user data for {username}: {ex.Message}", Weight.Error);
            }
        }
        /// <summary>
        /// Retrieves encryption key
        /// </summary>
        /// <param name="username">Key of this user</param>
        /// <returns>Returns the keys in byte arrays</returns>
        static private (byte[], byte[]) GetCryptionKeys(string username)
        {
            try
            {
                string userFolderPath = Path.Combine(_userDataFolderPath, username);
                CheckFilePath(userFolderPath);

                byte[] key = File.ReadAllBytes(Path.Combine(userFolderPath, "key.json"));
                byte[] iv = File.ReadAllBytes(Path.Combine(userFolderPath, "iv.json"));

                return (key, iv);
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error saving user data for {username}: {ex.Message}", Weight.Error);
                return (null, null);
            }
        }
        /// <summary>
        /// Checks if a filepath exists or must be created
        /// </summary>
        /// <param name="userFolderPath">Local path to check</param>
        /// <returns></returns>
        static public bool CheckFilePath(string userFolderPath)
        {
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);

                // Set the folder as hidden
                DirectoryInfo directoryInfo = new DirectoryInfo(userFolderPath);
                directoryInfo.Attributes |= FileAttributes.Hidden;

                return false;
            }
            return true;
        }
        /// <summary>
        /// Loads the users credentials
        /// </summary>
        /// <param name="username">Username to load the hashed password</param>
        /// <returns>If successfull true, else false</returns>
        static private (string, byte[]) LoadUserCredentials(string username)
        {
            try
            {
                string hashedPassword = File.ReadAllText(Path.Combine(_userDataFolderPath, username, "credentials.json"));
                string salt = File.ReadAllText(Path.Combine(_userDataFolderPath, username, "salt.json"));
                return (hashedPassword, Convert.FromBase64String(salt));
            }
            catch (Exception ex)
            {
                if(!ex.Message.Contains("Could not find a part of the path"))
                {
                    Logger.AddLog($"Error loading user data for {username}", Weight.Error);
                }
                else
                {
                    Logger.AddLog($"Error loading user data for {username}: {ex.Message}", Weight.Error);
                }
            }
            return (null, null);
        }
        /// <summary>
        /// Verifys the hashed password
        /// </summary>
        /// <param name="password">From user put in password</param>
        /// <param name="savedHashedPassword">Load users hashed password</param>
        /// <param name="salt">Loads users salt</param>
        /// <returns>If successfull true, else false</returns>
        static private bool VerifyHashedPassword(string password, string savedHashedPassword, byte[] salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(savedHashedPassword) || salt.Length == 0)
            {
                return false;
            }
            else
            {
                string hashedInputPassword = TUI_Hash.HashData(password, Convert.ToBase64String(salt));
                if (hashedInputPassword == savedHashedPassword)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
    static class TUI_Crypter
    {
        /// <summary>
        /// Generates encryption keys (Key and IV)
        /// </summary>
        /// <returns>Returns keys in byte arrays</returns>
        static public(byte[], byte[]) GenerateKeys()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();
                return (aesAlg.Key, aesAlg.IV);
            }
        }
        /// <summary>
        /// Encrypts data into bytes
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <param name="Key">Key to encryt data</param>
        /// <param name="IV">IV to encryt data</param>
        /// <returns>Returns encrypted data as byte arrays</returns>
        static public byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (string.IsNullOrEmpty(plainText) || (Key == null || Key.Length <= 0) || (IV == null || IV.Length <= 0))
            {
                return null;
            }
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        /// <summary>
        /// Decrypts data into a string
        /// </summary>
        /// <param name="cipherText">Byte array to decrypt</param>
        /// <param name="Key">Key to decryt data</param>
        /// <param name="IV">IV to decryt data</param>
        /// <returns>Returns decrypted data as a string</returns>
        static public string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if ((cipherText == null || cipherText.Length <= 0) || (Key == null || Key.Length <= 0) || (IV == null || IV.Length <= 0))
            {
                return null;
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
    static class TUI_Hash
    {
        /// <summary>
        /// Generates a salt
        /// </summary>
        /// <returns>Returns the generated salt</returns>
        static public string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }
        /// <summary>
        /// Hashes data
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <param name="salt">Salt to use for Hash</param>
        /// <returns>Returns the hashed data</returns>
        static public string HashData(string data, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(data);
            byte[] saltedPasswordBytes = new byte[passwordBytes.Length + saltBytes.Length];

            Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}