using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// modified from https://github.com/ChrisMaire/unity-native-sharing/blob/master/Assets/Plugins/NativeShare.cs
// and https://stackoverflow.com/a/28694269
// and https://github.com/ChrisMaire/unity-native-sharing/issues/33#issuecomment-346729881

public class ShareMap
{
    public static void ShareAndroid(string filePath)
    {
        using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            using (intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND")))
            { }
            using (intentObject.Call<AndroidJavaObject>("setType", "application/json"))
            { }
            // default file name for Google Drive
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), Path.GetFileName(filePath)))
            { }
            // default snippet name for Slack
            using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), Path.GetFileName(filePath)))
            { }

            using (AndroidJavaClass fileProviderClass = new AndroidJavaClass("android.support.v4.content.FileProvider"))
            using (AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            using (AndroidJavaObject uris = new AndroidJavaObject("java.util.ArrayList"))
            {
                string packageName = unityContext.Call<string>("getPackageName");
                string authority = packageName + ".provider";

                AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", filePath);
                AndroidJavaObject uriObj = fileProviderClass.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

                int FLAG_GRANT_READ_URI_PERMISSION = intentObject.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");
                intentObject.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);

                using (intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObj))
                { }
            }

            AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share map...");
            currentActivity.Call("startActivity", jChooser);
        }
    }

    public static string GetSharedURLAndroid()
    {
        using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            if (intent.Call<bool>("hasExtra", "used"))
                return "";
            using (var uri = intent.Call<AndroidJavaObject>("getData"))
            {
                if (uri == null)
                    return "";
                return uri.Call<string>("toString");
            }
        }
    }

    public static void ReadSharedURLAndroid(FileStream fileStream)
    {
        using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (var uri = intent.Call<AndroidJavaObject>("getData"))
        using (var contentResolver = activity.Call<AndroidJavaObject>("getContentResolver"))
        using (var inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", uri))
        {
            byte[] buffer = new byte[1024];
            var bufferPtr = AndroidJNIHelper.ConvertToJNIArray(buffer);
            // get the method id of InputStream.read(byte[] b, int off, int len)
            var readMethodId = AndroidJNIHelper.GetMethodID(inputStream.GetRawClass(), "read", "([BII)I");
            jvalue[] args = new jvalue[3];
            // construct arguments to pass to InputStream.read
            args[0].l = bufferPtr; // buffer
            args[1].i = 0; // offset
            args[2].i = buffer.Length; // length

            while (true)
            {
                int bytesRead = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), readMethodId, args);
                if (bytesRead <= 0)
                    break;
                byte[] newBuffer = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(bufferPtr);
                fileStream.Write(newBuffer, 0, bytesRead);
            }
            fileStream.Flush();
        }
    }

    public static bool CatchSharedFile()
    {
#if UNITY_ANDROID
        return GetSharedURLAndroid() != "";
#else
        return false;
#endif
    }

    public static void MarkIntentUsedAndroid()
    {
        using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (intent.Call<AndroidJavaObject>("putExtra", "used", true))
        { }
    }
}