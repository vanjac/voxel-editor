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
#endif
    }

    public static bool FileWaitingToImport()
    {
#if UNITY_ANDROID
        return AndroidShareReceive.FileWaitingToImport();
#else
        return false;
#endif
    }

    public static void ClearFileWaitingToImport()
    {
#if UNITY_ANDROID
        AndroidShareReceive.ClearFileWaitingToImport();
#endif
    }

    public static void ImportSharedFile(string filePath)
    {
#if UNITY_ANDROID
        AndroidShareReceive.ImportSharedFile(filePath);
#endif
    }
}
