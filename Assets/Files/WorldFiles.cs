using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public static class WorldFiles {
    private const string TEMP_NAME = ".temp";
    private const string BACKUP_NAME = ".backup";

    public static string GetWorldsDirectory() => Application.persistentDataPath;

    public static string GetNewWorldPath(string name) =>
        Path.Combine(GetWorldsDirectory(), name + ".nspace");

    public static string GetTempPath() => GetNewWorldPath(TEMP_NAME);

    public static string GetBackupPath() => GetNewWorldPath(BACKUP_NAME);

    public static string GetThumbnailPath(string worldPath) =>
        Path.ChangeExtension(worldPath, "jpg");

    // will throw an exception on failure
    public static void RestoreTempFile(string path) {
        File.Replace(GetTempPath(), path, GetBackupPath(), true);
    }

    public static bool IsWorldFile(string path) => path.ToLower().EndsWith(".nspace");

    public static bool IsOldWorldFile(string path) => path.ToLower().EndsWith(".json");

    public static void ListWorlds(List<string> worldPaths, List<string> worldNames) {
        worldPaths.Clear();
        worldNames.Clear();
        var directory = new DirectoryInfo(WorldFiles.GetWorldsDirectory());
        var files = directory.GetFiles().OrderByDescending(f => f.LastWriteTime);
        foreach (var fi in files) {
            string name = Path.GetFileNameWithoutExtension(fi.Name);
            if (WorldFiles.IsWorldFile(fi.FullName) && name != BACKUP_NAME && name != TEMP_NAME) {
                worldPaths.Add(fi.FullName);
                worldNames.Add(name);
            } else if (WorldFiles.IsOldWorldFile(fi.FullName)) {
                string newPath = WorldFiles.GetNewWorldPath(name);
                Debug.Log("Updating " + fi.FullName + " to " + newPath);
                File.Move(fi.FullName, newPath);
                worldPaths.Add(newPath);
                worldNames.Add(name);
            }
        }
    }

    public static bool ValidateName(string name, out string errorMessage) {
        errorMessage = null;
        if (name.Length == 0) {
            return false;
        }
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
            errorMessage = GUIPanel.StringSet.ErrorSpecialCharacter;
            return false;
        }

        if (name.StartsWith(".")) {
            errorMessage = GUIPanel.StringSet.ErrorPeriodName;
            return false;
        }

        string path = WorldFiles.GetNewWorldPath(name);
        if (File.Exists(path)) {
            errorMessage = GUIPanel.StringSet.ErrorWorldAlreadyExists;
            return false;
        }
        return true;  // you are valid <3
    }
}
