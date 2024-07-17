using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildWithPlatformAssetBundles
        : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
    private const string BUNDLE_DIR = "Assets/StreamingAssets/";
    private const string TEMP_DIR = "Assets/StreamingAssets_unused/";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        if (!Directory.Exists(TEMP_DIR)) {
            Directory.CreateDirectory(TEMP_DIR);
            AssetDatabase.Refresh();
        }

        var targetName = EditorUserBuildSettings.activeBuildTarget.ToString();
        var platformBundleName = "nspace_default_" + targetName.Replace("Standalone", "").ToLower();
        bool foundPlatformBundle = false;
        foreach (var guid in AssetDatabase.FindAssets("", new string[] { BUNDLE_DIR })) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileName(path);
            if (name != platformBundleName) {
                AssetDatabase.MoveAsset(path, TEMP_DIR + name);
            } else {
                foundPlatformBundle = true;
            }
        }
        AssetDatabase.Refresh();
        if (!foundPlatformBundle) {
            throw new System.Exception("Didn't find platform AssetBundle!");
        }
    }

    public void OnPostprocessBuild(BuildReport report) {
        foreach (var guid in AssetDatabase.FindAssets("", new string[] { TEMP_DIR })) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileName(path);
            AssetDatabase.MoveAsset(path, BUNDLE_DIR + name);
        }
        AssetDatabase.DeleteAsset(TEMP_DIR);
        AssetDatabase.Refresh();
    }
}
