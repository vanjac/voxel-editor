using System.Collections.Generic;
using UnityEngine;

public class EditorPreviewBehaviorAttribute : System.Attribute {
    public bool marker;
    public EditorPreviewBehaviorAttribute(bool marker = false) {
        this.marker = marker;
    }
}

public static class EntityPreviewManager {
    private static Dictionary<Entity, List<GameObject>> entityPreviewObjects = new Dictionary<Entity, List<GameObject>>();
    private static Dictionary<Entity, List<Behaviour>> markerPreviewComponents = new Dictionary<Entity, List<Behaviour>>();

    public static EditorPreviewBehaviorAttribute GetEditorPreviewAttribute(System.Type type) =>
        (EditorPreviewBehaviorAttribute)System.Attribute.GetCustomAttribute(type, typeof(EditorPreviewBehaviorAttribute));

    public static void AddEntity(Entity entity) {
        RemoveEntity(entity);

        var objectEntity = entity as ObjectEntity;
        var previewObjects = new List<GameObject>();
        var previewComponents = new List<Behaviour>();
        foreach (EntityBehavior behavior in entity.behaviors) {
            var attr = GetEditorPreviewAttribute(behavior.GetType());
            if (attr != null && (!attr.marker || (objectEntity != null && objectEntity.marker != null))) {
                if (behavior.targetEntity.entity != null && behavior.targetEntity.entity != entity) {
                    continue; // TODO: targeted behaviors not supported
                }
                if (behavior.condition != EntityBehavior.Condition.BOTH) {
                    continue;
                }
                GameObject previewObj;
                if (attr.marker) {
                    previewObj = objectEntity.marker.gameObject;
                } else {
                    previewObj = new GameObject();
                    previewObj.tag = "EditorPreview";
                    previewObj.transform.position = entity.PositionInEditor();
                    previewObjects.Add(previewObj);
                }
                var component = behavior.MakeComponent(previewObj);
                if (attr.marker) {
                    previewComponents.Add(component);
                }
            }
        }
        if (previewObjects.Count != 0) {
            entityPreviewObjects[entity] = previewObjects;
        }
        if (previewComponents.Count != 0) {
            markerPreviewComponents[entity] = previewComponents;
        }
    }

    public static void RemoveEntity(Entity entity) {
        if (entityPreviewObjects.TryGetValue(entity, out var previewObjects)) {
            foreach (GameObject obj in previewObjects) {
                GameObject.Destroy(obj);
            }
            entityPreviewObjects.Remove(entity);
        }
        if (markerPreviewComponents.TryGetValue(entity, out var previewComponents)) {
            foreach (var component in previewComponents) {
                Object.Destroy(component);
            }
            markerPreviewComponents.Remove(entity);
        }
    }

    public static void BehaviorUpdated(IEnumerable<Entity> entities, System.Type behaviorType) {
        if (GetEditorPreviewAttribute(behaviorType) != null) {
            foreach (var entity in entities) {
                AddEntity(entity);
            }
        }
    }

    public static void UpdateEntityPosition(Entity entity) {
        if (entityPreviewObjects.TryGetValue(entity, out var previewObjects)) {
            Vector3 pos = entity.PositionInEditor();
            foreach (GameObject obj in previewObjects) {
                if (obj == null) {
                    continue; // map may have been closed with an object still selected
                }
                obj.transform.position = pos;
            }
        }
    }
}