using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIIconSet : MonoBehaviour
{
    public static GUIIconSet instance;

    public Texture close, x, next, create, applySelection, clearSelection, paint, play, overflow, world, help;
    public Texture helpCircle, done, rotateLeft, rotateRight, flipHorizontal, flipVertical, compass, about, select;
    public Texture entityTag, target, rename, copy, delete, share, bevel, no, pause, restart, editor;

    [System.Serializable]
    public struct BevelIconSet
    {
        public Texture quarter, half, full;
        public Texture square, flat, curve, stair2, stair4;
    }
    public BevelIconSet bevelIcons;

    public void Start()
    {
        instance = this;
    }
}
