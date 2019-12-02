using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShareMap
{
    public static void Share(string filePath)
    {
#if UNITY_ANDROID
        AndroidShare.Share(filePath);
#elif UNITY_IOS
        IOSShare.Share(filePath);
#endif
    }

    public static void OpenFileManager()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidShareReceive.OpenFileManager();
#else
        Application.OpenURL("file:///");
#endif
    }

    public static bool FileWaitingToImport()
    {
#if UNITY_IOS || UNITY_EDITOR
        return IOSShareReceive.FileWaitingToImport();
#elif UNITY_ANDROID
        return AndroidShareReceive.FileWaitingToImport();
#else
        return false;
#endif
    }

    public static void ClearFileWaitingToImport()
    {
#if UNITY_IOS || UNITY_EDITOR
        IOSShareReceive.ClearFileWaitingToImport();
#elif UNITY_ANDROID
        AndroidShareReceive.ClearFileWaitingToImport();
#endif
    }

    public static void ImportSharedFile(string filePath)
    {
#if UNITY_IOS || UNITY_EDITOR
        IOSShareReceive.ImportSharedFile(filePath);
#elif UNITY_ANDROID
        AndroidShareReceive.ImportSharedFile(filePath);
#endif
    }

    public static Stream GetImportStream()
    {
#if UNITY_IOS || UNITY_EDITOR
        return IOSShareReceive.GetImportStream();
#elif UNITY_ANDROID
        return AndroidShareReceive.GetImportStream();
#else
        return null;
#endif
    }
}
