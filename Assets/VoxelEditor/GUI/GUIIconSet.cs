using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public Texture helpCircle, done, rotateLeft, rotateRight, flipHorizontal, flipVertical, compass, about, select;
    public Texture entityTag, target, rename, copy, delete, share, bevel, no, pause, restart, editor, playAudio;
    public Texture reddit, youTube, gitHub, undo;

    [System.Serializable]
    public struct BevelIconSet
    {
        public Texture quarter, half, full;
        public Texture square, flat, curve, stair2, stair4;
    }
    public BevelIconSet bevelIcons;
}
