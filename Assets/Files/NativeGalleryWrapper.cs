using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NativeGalleryWrapper
{
    public static void ImportTexture(System.Action<Texture2D> callback)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
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
        }, "Select a texture image");

        if (permission != NativeGallery.Permission.Granted)
            DialogGUI.ShowMessageDialog(GUIManager.guiGameObject,
                "Please grant N-Space permission to access your photo gallery.");
    }
}
