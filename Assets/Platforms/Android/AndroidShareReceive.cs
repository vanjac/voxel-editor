#if UNITY_ANDROID && !UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AndroidShareReceive
{
    public static void OpenFileManager()
    {
        using (AndroidJavaObject activity = AndroidShare.GetCurrentActivity())
        using (AndroidJavaClass downloadManagerClass = new AndroidJavaClass("android.app.DownloadManager"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            using (intentObject.Call<AndroidJavaObject>("setAction", downloadManagerClass.GetStatic<string>("ACTION_VIEW_DOWNLOADS")))
            { }
            activity.Call("startActivity", intentObject);
        }
    }

    public static bool FileWaitingToImport()
    {
        using (var activity = AndroidShare.GetCurrentActivity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            if (intent.Call<bool>("hasExtra", "used"))
                return false;
            using (var uri = intent.Call<AndroidJavaObject>("getData"))
            {
                if (uri == null)
                    return false;
                string uriString = uri.Call<string>("toString");
                if (uriString == "")
                    return false;
                Debug.Log("Intent uri " + uriString);
                Debug.Log("Intent type " + intent.Call<string>("getType"));
                return true;
            }
        }
    }

    public static void ClearFileWaitingToImport()
    {
        using (var activity = AndroidShare.GetCurrentActivity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (intent.Call<AndroidJavaObject>("putExtra", "used", true))
        { }
        try
        {
            File.Delete(GetTempPath());
        }
        catch (System.Exception e) { }
    }

    public static void ImportSharedFile(string filePath)
    {
        using (FileStream fileStream = File.Create(filePath))
        {
            ReadSharedURL(fileStream);
        }
    }

    public static Stream GetImportStream()
    {
        FileStream tmp = File.Create(GetTempPath());
        ReadSharedURL(tmp);
        tmp.Seek(0, SeekOrigin.Begin);
        return tmp;
    }

    private static string GetTempPath()
    {
        return Path.Combine(Application.temporaryCachePath, "Imported");
    }

    private static void ReadSharedURL(Stream outputStream)
    {
        using (var activity = AndroidShare.GetCurrentActivity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (var uri = intent.Call<AndroidJavaObject>("getData"))
        using (var inputStream = GetInputStreamForURI(uri, activity))
        {
            byte[] buffer = new byte[8192];
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
                outputStream.Write(newBuffer, 0, bytesRead);
            }
            outputStream.Flush();
        }
    }

    private static AndroidJavaObject GetInputStreamForURI(AndroidJavaObject uri, AndroidJavaObject activity)
    {
        string scheme = uri.Call<string>("getScheme");
        if (scheme == "content" || scheme == "android.resource" || scheme == "file")
        {
            using (var contentResolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            {
                return contentResolver.Call<AndroidJavaObject>("openInputStream", uri);
            }
        }
        else
        {
            using (var url = new AndroidJavaObject("java.net.URL", uri.Call<string>("toString")))
            {
                return url.Call<AndroidJavaObject>("openStream");
            }
        }
    }
}

#endif