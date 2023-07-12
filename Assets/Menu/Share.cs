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

    public static bool OpenFileManager()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return AndroidShareReceive.OpenFileManager();
#elif UNITY_IOS && !UNITY_EDITOR
        // Files app
        Application.OpenURL("shareddocuments://");
        return true;
#else
        Application.OpenURL("file:///");
        return true;
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

    // creates a stream; wrap inside "using"!
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
