using System.IO;
using UnityEngine;

public static class NativeGalleryWrapper {
    public static void ImportTexture(System.Action<Texture2D> callback) {
        RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image, () => {
            NativeGallery.GetImageFromGallery((path) => {
                if (path == null) {
                    callback(null);
                    return;
                }
                Texture2D texture = NativeGallery.LoadImageAtPath(path,
                    maxSize: 1024, markTextureNonReadable: false);
                if (texture == null) {
                    DialogGUI.ShowMessageDialog(
                        GUIPanel.GuiGameObject, GUIPanel.StringSet.ErrorImageImport);
                } else {
                    Debug.Log($"Dimensions: {texture.width}, {texture.height}");
                }
                callback(texture);
            }, GUIPanel.StringSet.SelectTextureImage);
        }, GUIPanel.StringSet.ErrorImagePermission);
    }

    // callback MUST dispose the stream when done!
    public static void ImportAudioStream(System.Action<Stream> callback) {
        // TODO: get this to work with Android eventually
        // right now it can't open files unless they are in a .nomedia folder (doesn't have read permission)
        // and the files it can open have names obscured.
        //#if UNITY_ANDROID && !UNITY_EDITOR
#if false
        RequestPermission(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Audio, () => {
            NativeGallery.GetAudioFromGallery((path) => {
                if (path == null) {
                    callback(null);
                    return;
                }
                try {
                    callback(File.Open(path, FileMode.Open));
                } catch (System.Exception e) {
                    DialogGUI.ShowMessageDialog(
                        GUIPanel.GuiGameObject, GUIPanel.StringSet.ErrorAudioImport);
                    Debug.LogError(e);
                }
            });
        }, GUIPanel.StringSet.ErrorAudioPermission);
#else
        if (!ShareMap.OpenFileManager()) {
            DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject, GUIPanel.StringSet.ErrorFileManager);
        }
#endif
    }

    private static void RequestPermission(
            NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaType,
            System.Action grantedAction, string errorMessage) {
        NativeGallery.RequestPermissionAsync(permission => {
            if (permission == NativeGallery.Permission.Granted) {
                grantedAction();
            } else {
                DialogGUI.ShowMessageDialog(GUIPanel.GuiGameObject, errorMessage);
            }
        }, permissionType, mediaType);
    }
}
