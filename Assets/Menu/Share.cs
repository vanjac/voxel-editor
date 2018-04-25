using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// modified from https://github.com/ChrisMaire/unity-native-sharing/blob/master/Assets/Plugins/NativeShare.cs
// and https://stackoverflow.com/a/28694269

public class ShareMap
{
    public static void ShareAndroid(string filePath)
    {
        string fileContent;
        using (FileStream fileStream = File.Open(filePath, FileMode.Open))
        {
            using (var sr = new StreamReader(fileStream))
            {
                fileContent = sr.ReadToEnd();
            }
        }

        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            using (intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND")))
            { }
            using (intentObject.Call<AndroidJavaObject>("setType", "application/json"))
            { }
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), Path.GetFileName(filePath)))
            { }
            // TODO: this has a length limit which is possible to exceed
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), fileContent))
            { }

            using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share map...");
                currentActivity.Call("startActivity", jChooser);
            }
        }
    }
}