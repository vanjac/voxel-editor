using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewBehaviorAttribute : System.Attribute
{

}

public static class EntityPreviewManager
{
    private static Dictionary<Entity, List<GameObject>> entityPreviewObjects = new Dictionary<Entity, List<GameObject>>();

    public static bool IsEditorPreviewBehavior(EntityBehavior behavior)
    {
        return System.Attribute.GetCustomAttribute(behavior.GetType(), typeof(EditorPreviewBehaviorAttribute)) != null;
    }

    public static void AddEntity(Entity entity)
    {
        if (entityPreviewObjects.ContainsKey(entity))
            RemoveEntity(entity);

        var previewObjects = new List<GameObject>();
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            if (IsEditorPreviewBehavior(behavior))
            {
                if (behavior.targetEntity.entity != null && behavior.targetEntity.entity != entity)
                    continue; // TODO: targeted behaviors not supported
                if (behavior.condition != EntityBehavior.Condition.BOTH)
                    continue;
                var previewObj = new GameObject();
                previewObj.tag = "EditorPreview";
                previewObj.transform.position = entity.PositionInEditor();
                behavior.MakeComponent(previewObj);
                previewObjects.Add(previewObj);
            }
        }
        if (previewObjects.Count != 0)
            entityPreviewObjects[entity] = previewObjects;
    }

    public static void RemoveEntity(Entity entity)
    {
        if (entityPreviewObjects.TryGetValue(entity, out var previewObjects))
        {
            foreach (GameObject obj in entityPreviewObjects[entity])
                GameObject.Destroy(obj);
            entityPreviewObjects.Remove(entity);
        }
    }

    public static void BehaviorUpdated(Entity entity, EntityBehavior behavior)
    {
        if (!IsEditorPreviewBehavior(behavior))
            return;
        if (entity != null)
            AddEntity(entity);
    }

    public static void UpdateEntityPosition(Entity entity)
    {
        if (entityPreviewObjects.TryGetValue(entity, out var previewObjects))
        {
            Vector3 pos = entity.PositionInEditor();
            foreach (GameObject obj in entityPreviewObjects[entity])
            {
                if (obj == null)
                    continue; // map may have been closed with an object still selected
                obj.transform.position = pos;
            }
        }
    }
}