using UnityEditor;

public static class EditorUtils {
    [MenuItem("Assets/Copy GUID")]
    static void CopyGUID() {
        EditorGUIUtility.systemCopyBuffer = string.Join("\n", Selection.assetGUIDs);
    }
}
