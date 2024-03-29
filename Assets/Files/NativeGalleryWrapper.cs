﻿using System.IO;
using UnityEngine;

public static class NativeGalleryWrapper
{
    public static void ImportTexture(System.Action<Texture2D> callback)
    {
        CheckPermission(NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null)
            {
                callback(null);
                return;
            }
            Texture2D texture = NativeGallery.LoadImageAtPath(path,
                maxSize: 1024, markTextureNonReadable: false);
            if (texture == null)
                DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject, "Error importing image");
            else
                Debug.Log("Dimensions: " + texture.width + ", " + texture.height);
            callback(texture);
        }, "Select a texture image"));
    }

    // callback MUST dispose the stream when done!
    public static void ImportAudioStream(System.Action<Stream> callback)
    {
        // TODO: get this to work with Android eventually
        // right now it can't open files unless they are in a .nomedia folder (doesn't have read permission)
        // and the files it can open have names obscured.
        //#if UNITY_ANDROID && !UNITY_EDITOR
#if false
        CheckPermission(NativeGallery.GetAudioFromGallery((path) => {
            if (path == null)
            {
                callback(null);
                return;
            }
            try
            {
                callback(File.Open(path, FileMode.Open));
            }
            catch (System.Exception e)
            {
                DialogGUI.ShowMessageDialog(GUIPanel.guiGameObject, "Error importing audio file");
                Debug.LogError(e);
            }
        }));
#else
        if (!ShareMap.OpenFileManager())
            DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject, "Error opening file manager. Find an audio file and open it with N-Space.");
#endif
    }

    private static void CheckPermission(NativeGallery.Permission permission)
    {
        if (permission != NativeGallery.Permission.Granted)
            DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject,
                "Please grant N-Space permission to access your photo gallery.");
    }
}
