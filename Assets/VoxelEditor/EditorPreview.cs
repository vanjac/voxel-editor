using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewBehaviorAttribute : System.Attribute
{

}

public class EditorPreviewEntity : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Editor Preview", typeof(EditorPreviewEntity));

    public Vector3 position;

    public override EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true)
    {
        var gameObject = new GameObject();
        var c = gameObject.AddComponent<EditorPreviewComponent>();
        c.transform.position = position;
        c.entity = this;
        if (storeComponent)
            component = c;
        return c;
    }

    public override bool AliveInEditor()
    {
        return component != null;
    }
}

public class EditorPreviewComponent : DynamicEntityComponent
{

}