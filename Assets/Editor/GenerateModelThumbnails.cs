using UnityEngine;
using UnityEditor;

public static class GenerateModelThumbnails
{
    private const string SEARCH_PATH = "Assets/Resources/GameAssets/Models";
    private const string WRITE_PATH = "Assets/Resources/Thumbnails/";

    [MenuItem("Tools/Generate N-Space model thumbnails")]
    public static void Generate()
    {
        foreach (var fileInfo in new System.IO.DirectoryInfo(WRITE_PATH).GetFiles())
        {
            fileInfo.Delete();
        }

        string[] guids = AssetDatabase.FindAssets("", new string[]{ SEARCH_PATH });
        foreach (string guid in guids)
        {
            string fullPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(fullPath);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);

            Texture2D thumbnail = AssetPreview.GetAssetPreview(mesh);
            if (thumbnail != null)
            {
                RenderTexture rt = RenderTexture.GetTemporary(thumbnail.width, thumbnail.height);
                Graphics.Blit(thumbnail, rt);

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = rt;

                Texture2D readableTexture = new Texture2D(thumbnail.width, thumbnail.height);
                readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                readableTexture.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);

                string writePath = WRITE_PATH + fileName + ".png";
                System.IO.File.WriteAllBytes(writePath, readableTexture.EncodeToPNG());
                Object.DestroyImmediate(readableTexture);

                var importer = AssetImporter.GetAtPath(writePath) as TextureImporter;
                importer.textureType = TextureImporterType.GUI;
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh();
    }
}
