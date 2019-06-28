#if UNITY_IOS || UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class IOSShareReceive
{
    private const string INBOX_PATH1 = "Documents/Inbox"; // from Gmail, etc.
    private const string INBOX_PATH2 = "tmp/com.vantjac.voxel-Inbox"; // from Files

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

    private static string GetFileToImport()
    {
        string appDir = Application.persistentDataPath.Replace("Documents", "");

        string firstFile = FirstFile(appDir + INBOX_PATH1);
        if (firstFile != null)
            return firstFile;
        return FirstFile(appDir + INBOX_PATH2);
    }

    private static string FirstFile(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            return null;
        string[] files = Directory.GetFiles(dirPath);
        if (files.Length == 0)
            return null;
        else
            return files[0];
    }

    public static void ImportSharedFile(string filePath)
    {
        string oldFilePath = GetFileToImport();
        if (oldFilePath == null)
            throw new System.Exception("No file waiting to import!");
        Debug.Log("Moving from " + oldFilePath + " to " + filePath);
        File.Copy(oldFilePath, filePath);
    }

    public static Stream GetImportStream()
    {
        string oldFilePath = GetFileToImport();
        if (oldFilePath == null)
            throw new System.Exception("No file waiting to import!");
        return File.Open(oldFilePath, FileMode.Open);
    }
}

#endif