//using Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace TUI_GitLabxTensionEditor.Editor
{
    static public class TUI_ApiProtokoll
    {
        /// <summary>
        /// Used to get a list of data per link from an JSON
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="url">Url to access for data</param>
        /// <returns></returns>
        static async public Task <List<T>> GetDataListFromURL<T>(string url, Action<float> progressCallback)
        {
            progressCallback?.Invoke(0);
            List<T> dataList = new List<T>();
            string responseAsJSON = string.Empty;
            try
            {
                responseAsJSON = await GetDataAsString(url);
                progressCallback?.Invoke(0.5f);
                if (string.IsNullOrEmpty(responseAsJSON))
                {
                    Logger.AddLog($"Error fetching milestones from GitLab API", Weight.Error);
                    Logger.AddLog($"Check youre usersettings", Weight.Warning);
                    return null;
                }
            }
            catch (WebException ex)
            {
                Logger.AddLog($"Error fetching milestones from GitLab API: {ex.Message}", Weight.Error);
                return null;
            }
            finally
            {
                dataList = JsonConvert.DeserializeObject<List<T>>(responseAsJSON);
                progressCallback?.Invoke(1);
            }
            return dataList;
        }
        /// <summary>
        /// Used to get a single datastruct per link from an JSON
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="url">Url to access for data</param>
        /// <returns></returns>
        static async public Task<T> GetDataFromURL<T>(string url, Action<float> progressCallback)
        {
            progressCallback?.Invoke(0);
            T data;
            string responseAsJSON = string.Empty;
            try
            {
                responseAsJSON = await GetDataAsString(url);
                progressCallback?.Invoke(0.5f);
                if (string.IsNullOrEmpty(responseAsJSON))
                {
                    Logger.AddLog($"Error fetching milestones from GitLab API", Weight.Error);
                    Logger.AddLog($"Check youre usersettings", Weight.Warning);
                    return default(T);
                }
            }
            catch (WebException ex)
            {
                Logger.AddLog($"Error fetching milestones from GitLab API: {ex.Message}", Weight.Error);
                return default(T);
            }
            finally
            {
                data = JsonConvert.DeserializeObject<T>(responseAsJSON);
                progressCallback?.Invoke(1);
            }
            return data;
        }
        /// <summary>
        /// Used to get a JSON and returns it as a string
        /// </summary>
        /// <param name="url">Url to access for data</param>
        /// <returns></returns>
        static async public Task<string> GetDataAsString(string url)
        {
            try
            {
                string responseAsJSON;
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add("PRIVATE-TOKEN", TUI_SecurityManager.GetPAT());

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(dataStream);
                    responseAsJSON = streamReader.ReadToEnd();
                    streamReader.Close();
                    dataStream.Close();
                }
                if (string.IsNullOrEmpty(responseAsJSON))
                {
                    Logger.AddLog($"Error fetching milestones from GitLab API", Weight.Error);
                    Logger.AddLog($"Check youre usersettings", Weight.Warning);
                    return string.Empty;
                }
                return responseAsJSON;
            }
            catch(Exception ex)
            {
                if(ex.Message.Contains("Invalid URI"))
                {
                    Logger.AddLog("Url in settings is wrong or not set", Weight.Error);
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// Posts a datastruct asynchron as a JSON
        /// </summary>
        /// <typeparam name="T">Type of the datastruct</typeparam>
        /// <param name="data">The datastruct</param>
        /// <param name="url">Url to post</param>
        /// <param name="progressCallback">Action to update post state</param>
        /// <returns></returns>
        static public async Task PostDataAsJson<T>(T data, string url, Action<float> progressCallback)
        {
            progressCallback?.Invoke(0);
            try
            {
                string dataAsJSON = JsonConvert.SerializeObject(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("PRIVATE-TOKEN", TUI_SecurityManager.GetPAT());
                request.Method = "POST";
                request.ContentType = "application/json";

                using (StreamWriter streamWriter = new StreamWriter(await request.GetRequestStreamAsync()))
                {
                    await streamWriter.WriteAsync(dataAsJSON);
                    progressCallback?.Invoke(0.5f); // Indicate 0% completion
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Logger.AddLog("Issue created successfully", Weight.Info);
                    }
                    else
                    {
                        Logger.AddLog($"Failed to create issue. Status code: {response.StatusCode}", Weight.Error);
                        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            Logger.AddLog($"Error response: {await streamReader.ReadToEndAsync()}", Weight.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddLog($"Error posting JSON data to GitLab API: {ex.Message}", Weight.Error);
            }
            finally
            {
                progressCallback?.Invoke(1);
            }
        }
    }
}