#if UNITY_IOS

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IOSShareReceive
{
    public static bool FileWaitingToImport()
    {
        return GetFileToImport() != null;
    }

    public static void ClearFileWaitingToImport()
    {
        string fileToImport = GetFileToImport();
        if (fileToImport != null)
            File.Delete(fileToImport);
    }

    private static string InboxPath()
    {
        return Application.persistentDataPath.Replace("Documents", "tmp/com.vantjac.voxel-Inbox");
    }

    private static string GetFileToImport()
    {
        string[] inboxFiles = Directory.GetFiles(InboxPath());
        if (inboxFiles.Length == 0)
            return null;
        else
            return inboxFiles[0];
    }

    public static void ImportSharedFile(string filePath)
    {
        string oldFilePath = GetFileToImport();
        if (oldFilePath == null)
            throw new System.Exception("No file waiting to import!");
        Debug.Log("Moving from " + oldFilePath + " to " + filePath);
        File.Copy(oldFilePath, filePath);
    }
}

#endif