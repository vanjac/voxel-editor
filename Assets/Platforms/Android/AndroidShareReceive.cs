#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;

public static class AndroidShareReceive
{
    private static string tempPath = null;

    public static bool OpenFileManager()
    {
        using (AndroidJavaObject activity = AndroidShare.GetCurrentActivity())
        using (AndroidJavaClass downloadManagerClass = new AndroidJavaClass("android.app.DownloadManager"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            using (intentObject.Call<AndroidJavaObject>("setAction", downloadManagerClass.GetStatic<string>("ACTION_VIEW_DOWNLOADS")))
            { }
            try
            {
                activity.Call("startActivity", intentObject);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError(e);
                return false;
            }
        }
        return true;
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
            if (tempPath != null)
                File.Delete(tempPath);
            tempPath = null;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
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
        string name = Path.GetFileNameWithoutExtension(GetImportURI());
        Debug.Log("Original name: " + name);
        // %2F is a '/'
        string[] parts = name.Split(new string[] {"%2f", "%2F"}, StringSplitOptions.RemoveEmptyEntries);
        name = parts[parts.Length - 1];
        name = string.Concat(Regex.Split(name, "%[0-9a-fA-F]{2}"));
        name = string.Concat(name.Split('%'));
        name = string.Concat(name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        name = name.Trim();
        if (name.Length == 0)
            name = "Imported";
        else if (name.Length > 32)
            name = name.Substring(0, 32);

        tempPath = Path.Combine(Application.temporaryCachePath, name);
        FileStream tmp = File.Create(tempPath);
        ReadSharedURL(tmp);
        tmp.Seek(0, SeekOrigin.Begin);
        return tmp;
    }

    private static string GetImportURI()
    {
        using (var activity = AndroidShare.GetCurrentActivity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (var uri = intent.Call<AndroidJavaObject>("getData"))
            return uri.Call<string>("toString");
    }

    private static void ReadSharedURL(Stream outputStream)
    {
        using (var activity = AndroidShare.GetCurrentActivity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (var uri = intent.Call<AndroidJavaObject>("getData"))
        using (var inputStream = GetInputStreamForURI(uri, activity))
        {
            sbyte[] buffer = new sbyte[8192];
            var bufferPtr = AndroidJNIHelper.ConvertToJNIArray(buffer);
            // get the method id of InputStream.read(byte[] b, int off, int len)
            var readMethodId = AndroidJNIHelper.GetMethodID(inputStream.GetRawClass(), "read", "([BII)I");
            jvalue[] args = new jvalue[3];
            // construct arguments to pass to InputStream.read
            args[0].l = bufferPtr; // buffer
            args[1].i = 0; // offset
            args[2].i = buffer.Length; // length

            byte[] outBuffer = new byte[8192];
            while (true)
            {
                int bytesRead = AndroidJNI.CallIntMethod(inputStream.GetRawObject(), readMethodId, args);
                if (bytesRead <= 0)
                    break;
                sbyte[] newBuffer = AndroidJNIHelper.ConvertFromJNIArray<sbyte[]>(bufferPtr);
                Buffer.BlockCopy(newBuffer, 0, outBuffer, 0, bytesRead);  // convert sbytes to bytes
                outputStream.Write(outBuffer, 0, bytesRead);
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