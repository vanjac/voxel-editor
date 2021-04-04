using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewBehaviorAttribute : System.Attribute
{

}

public static class EntityPreviewManager
{
    private static List<GameObject> selfPreviewObjects = new List<GameObject>();
    private static List<GameObject> otherPreviewObjects = new List<GameObject>();

    public static bool IsEditorPreviewBehavior(EntityBehavior behavior)
    {
        return System.Attribute.GetCustomAttribute(behavior.GetType(), typeof(EditorPreviewBehaviorAttribute)) != null;
    }

    public static void EntitySelected(Entity entity)
    {
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            if (IsEditorPreviewBehavior(behavior))
            {
                var previewObj = new GameObject();
                previewObj.tag = "EditorPreview";
                if (behavior.targetEntity.entity != null)
                    previewObj.transform.position = behavior.targetEntity.entity.PositionInEditor();
                else
                    previewObj.transform.position = entity.PositionInEditor();
                behavior.MakeComponent(previewObj);
                if (behavior.targetEntity.entity == null || behavior.targetEntity.entity == entity)
                    selfPreviewObjects.Add(previewObj);
                else
                    otherPreviewObjects.Add(previewObj);
            }
        }
    }

    public static void EntityDeselected()
    {
        foreach (GameObject obj in selfPreviewObjects)
            GameObject.Destroy(obj);
        selfPreviewObjects.Clear();
        foreach (GameObject obj in otherPreviewObjects)
            GameObject.Destroy(obj);
        otherPreviewObjects.Clear();
    }

    public static void BehaviorUpdated(Entity entity, EntityBehavior behavior)
    {
        if (!IsEditorPreviewBehavior(behavior))
            return;
        EntityDeselected();
        if (entity != null)
            EntitySelected(entity);
    }

    public static void UpdateEntityPosition(Entity entity)
    {
        if (selfPreviewObjects.Count == 0)
            return;
        Vector3 pos = entity.PositionInEditor();
        foreach (GameObject obj in selfPreviewObjects)
        {
            if (obj == null)
                continue; // map may have been closed with an object still selected
            obj.transform.position = pos;
        }
    }
}