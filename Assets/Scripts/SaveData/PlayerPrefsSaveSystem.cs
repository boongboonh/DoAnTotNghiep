using Newtonsoft.Json;
using UnityEngine;


//using Newtonsoft.Json;
//instal git: com.unity.nuget.newtonsoft-json

namespace GF.Save
{
    public static class PlayerPrefsSaveSystem
    {
        public static void SaveInt(string key, int value)
        {
            SetInt(key, value);
            Flush();
        }

        public static int LoadInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SaveFloat(string key, float value)
        {
            SetFloat(key, value);
            Flush();
        }

        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SaveString(string key, string value)
        {
            SetString(key, value);
            Flush();
        }

        public static string LoadString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        //1 true, 0 false
        public static void SaveBool(string key, bool value)
        {
            SaveInt(key, value ? 1 : 0);
        }

        public static bool LoadBool(string key, bool defaultValue = false)
        {
            int defaultInt = defaultValue ? 1 : 0;
            return PlayerPrefs.GetInt(key, defaultInt) == 1;
        }

        // ----------- Generic Save / Load with Newtonsoft.Json -----------
        public static void SaveData<T>(string key, T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.None);
            SaveString(key, json);
        }

        public static T LoadData<T>(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return default;
            string json = PlayerPrefs.GetString(key);
            return JsonConvert.DeserializeObject<T>(json);
        }

        // ----------- Utilities -----------
        // Stage writes for checkpoints and settings that update multiple values together.
        public static void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public static void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public static void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
        public static void Flush() => PlayerPrefs.Save();

        public static bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public static void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public static void ClearAll() => PlayerPrefs.DeleteAll();
    }
}

