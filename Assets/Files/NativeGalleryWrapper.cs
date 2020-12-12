﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class NativeGalleryWrapper
{
    public static void ImportTexture(System.Action<Texture2D> callback)
    {
        CheckPermission(NativeGallery.GetImageFromGallery((path) => {
            if (path == null)
            {
                callback(null);
                return;
            }
            Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
            if (texture == null)
                DialogGUI.ShowMessageDialog(GUIManager.guiGameObject, "Error importing image");
            else
                Debug.Log("Dimensions: " + texture.width + ", " + texture.height);
            callback(texture);
        }, "Select a texture image"));
    }

    public static void ImportAudioStream(System.Action<Stream> callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
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
                DialogGUI.ShowMessageDialog(GUIManager.guiGameObject, "Error importing audio file");
                Debug.LogError(e);
            }
        }));
#else
        ShareMap.OpenFileManager();
#endif
    }

    private static void CheckPermission(NativeGallery.Permission permission)
    {
        if (permission != NativeGallery.Permission.Granted)
            DialogGUI.ShowMessageDialog(GUIManager.guiGameObject,
                "Please grant N-Space permission to access your photo gallery.");
    }
}