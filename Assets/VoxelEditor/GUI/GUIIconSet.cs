﻿using UnityEngine;

[System.Serializable]
public struct GUIIconSet
{
    public static GUIIconSet instance
    {
        get
        {
            if (GUIManager.instance == null)
                return new GUIIconSet();
            return GUIManager.instance.iconSet;
        }
    }

    public Texture close, x, next, create, applySelection, clearSelection, paint, play, overflow, world, help;
    public Texture helpCircle, done, rotateLeft, rotateRight, flipHorizontal, flipVertical, about, select;
    public Texture entityTag, target, rename, copy, delete, share, bevel, no, pause, restart, editor, playAudio;
    public Texture reddit, youTube, undo, import, fill, draw, singleObject, objectType, behavior, newTexture;
    public Texture worldImport, newItem, website, donate, pan, orbit, sensor, random;
    public Texture indoorLarge, floatingLarge, newWorldLarge, helpLarge, compassLarge;

    public Texture[] tagIcons;

    [System.Serializable]
    public struct BevelIconSet
    {
        public Texture quarter, half, full;
        public Texture square, flat, curve, stair2, stair4;
    }
    public BevelIconSet bevelIcons;
}
