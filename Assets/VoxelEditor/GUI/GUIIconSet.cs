using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIIconSet : MonoBehaviour
{
    public static GUIIconSet instance;

    public Texture close;
    public Texture create;
    public Texture applySelection;
    public Texture clearSelection;
    public Texture paint;
    public Texture play;
    public Texture overflow;
    public Texture world;
    public Texture done;
    public Texture rotateLeft;
    public Texture rotateRight;
    public Texture flipHorizontal;
    public Texture flipVertical;
    public Texture missingTexture;
    public Texture compass;

    public void Start()
    {
        instance = this;
    }
}
