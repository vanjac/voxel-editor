#if UNITY_ANDROID

using System.IO;
using UnityEngine;

// modified from https://github.com/ChrisMaire/unity-native-sharing/blob/master/Assets/Plugins/NativeShare.cs
// and https://stackoverflow.com/a/28694269
// and https://github.com/ChrisMaire/unity-native-sharing/issues/33#issuecomment-346729881

public static class AndroidShare {
    public static void Share(string filePath) {
        using (AndroidJavaObject activity = GetCurrentActivity())
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent")) {
            using (intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"))) {
            }
            using (intentObject.Call<AndroidJavaObject>("setType", "application/vnd.vantjac.nspace")) {
            }
            // default file name for Google Drive
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), Path.GetFileName(filePath))) {
            }
            // default snippet name for Slack
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), Path.GetFileName(filePath))) {
            }

            using (AndroidJavaClass fileProviderClass = new AndroidJavaClass("android.support.v4.content.FileProvider"))
            using (AndroidJavaObject unityContext = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            using (AndroidJavaObject uris = new AndroidJavaObject("java.util.ArrayList"))
            using (AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", filePath)) {
                int FLAG_GRANT_READ_URI_PERMISSION = intentObject.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");
                using (intentObject.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION)) {
                }

                string packageName = unityContext.Call<string>("getPackageName");
                string authority = packageName + ".provider";
                using (AndroidJavaObject uriObj = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj))
                using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObj)) {
                }
            }

            using (AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share world...")) {
                activity.Call("startActivity", jChooser);
            }
        }
    }

    public static AndroidJavaObject GetCurrentActivity() {
        using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
}

#endif