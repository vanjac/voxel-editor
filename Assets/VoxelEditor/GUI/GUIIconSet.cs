using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIIconSet : MonoBehaviour
{
    public static GUIIconSet instance;

    public Texture close;
    public Texture x;
    public Texture next;
    public Texture create;
    public Texture applySelection;
    public Texture clearSelection;
    public Texture paint;
    public Texture play;
    public Texture overflow;
    public Texture world;
    public Texture help;
    public Texture helpCircle;
    public Texture done;
    public Texture rotateLeft;
    public Texture rotateRight;
    public Texture flipHorizontal;
    public Texture flipVertical;
    public Texture compass;
    public Texture about;
    public Texture select;
    public Texture entityTag;

    public void Start()
    {
        instance = this;
    }
}
