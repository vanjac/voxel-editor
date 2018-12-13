#if UNITY_ANDROID

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AndroidShareReceive : MonoBehaviour
{
    public static bool FileWaitingToImport()
    {
        using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            if (intent.Call<bool>("hasExtra", "used"))
                return false;
            using (var uri = intent.Call<AndroidJavaObject>("getData"))
            {
                if (uri == null)
                    return false;
                return uri.Call<string>("toString") != "";
            }
        }
    }

    public static void ClearFileWaitingToImport()
    {
        using (var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        using (intent.Call<AndroidJavaObject>("putExtra", "used", true))
        { }
    }

    public static void ImportSharedFile(string filePath)
    {
        using (FileStream fileStream = File.Create(filePath))
        {
            ReadSharedURL(fileStream);
        }
    }

    private static void ReadSharedURL(FileStream fileStream)
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
}

#endif