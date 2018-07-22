using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewBehaviorAttribute : System.Attribute
{

}

public class EntityPreviewManager
{
    private static List<GameObject> behaviorPreviewObjects = new List<GameObject>();

    public static void EntitySelected(Entity entity)
    {
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            if (System.Attribute.GetCustomAttribute(behavior.GetType(), typeof(EditorPreviewBehaviorAttribute)) != null)
            {
                var previewObj = new GameObject();
                if (behavior.targetEntity.entity != null)
                    previewObj.transform.position = behavior.targetEntity.entity.PositionInEditor();
                else
                    previewObj.transform.position = entity.PositionInEditor();
                behavior.MakeComponent(previewObj);
                behaviorPreviewObjects.Add(previewObj);
            }
        }
    }

    public static void EntityDeselected()
    {
        foreach (GameObject obj in behaviorPreviewObjects)
            GameObject.Destroy(obj);
        behaviorPreviewObjects.Clear();
    }
}