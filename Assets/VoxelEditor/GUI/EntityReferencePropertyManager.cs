using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityReferencePropertyManager : MonoBehaviour
{
    public class EntityReferenceLine : MonoBehaviour
    {
        public int i;
        public Entity sourceEntity;
        public Entity targetEntity;

        void Start()
        {
            if (sourceEntity == null || targetEntity == null)
                return;
            Color color = ColorI(i);
            LineRenderer line = gameObject.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.1f;
            line.material = _lineMaterial;
            line.startColor = line.endColor = color;
            line.SetPosition(0, sourceEntity.PositionInEditor());
            line.SetPosition(1, targetEntity.PositionInEditor());
        }
    }


    private static bool updateTargets = false;
    private static Entity currentEntity;
    private static List<Entity> targetEntities = new List<Entity>();
    private static Entity behaviorTarget;
    private static int currentTargetEntityI = -1;

    private static Material _lineMaterial;
    public Material lineMaterial;

    private void Clear()
    {
        targetEntities.Clear();
        currentEntity = null;
        behaviorTarget = null;
        currentTargetEntityI = -1;
    }

    public static void Reset(Entity entity)
    {
        foreach (Entity target in targetEntities)
        {
            if (!(target is Substance))
                continue;
            ((Substance)target).highlight = Color.clear;
            foreach (Voxel v in ((Substance)target).voxels)
                v.UpdateHighlight();
        }
        if (currentEntity != entity)
        {
            if (currentEntity != null)
            {
                // entity deselected
                if (currentEntity is Substance)
                {
                    ((Substance)currentEntity).highlight = Color.clear;
                    foreach (Voxel v in ((Substance)currentEntity).voxels)
                        v.UpdateHighlight();
                }
                EntityPreviewManager.EntityDeselected();
            }
            if (entity != null)
            {
                // entity selected
                if (entity is Substance)
                {
                    ((Substance)entity).highlight = Color.white;
                    foreach (Voxel v in ((Substance)entity).voxels)
                        v.UpdateHighlight();
                }
                EntityPreviewManager.EntitySelected(entity);
            }
        }
        currentEntity = entity;
        behaviorTarget = null;
        targetEntities.Clear();
        currentTargetEntityI = -1;
    }

    // TODO: delete this when it is no longer needed
    public static Entity CurrentEntity()
    {
        return currentEntity;
    }

    public static void Next(Entity entity)
    {
        int existingIndex = targetEntities.IndexOf(entity); // TODO: not efficient
        if (existingIndex != -1)
        {
            currentTargetEntityI = existingIndex;
            return;
        }
        targetEntities.Add(entity);
        currentTargetEntityI = targetEntities.Count - 1;
        if (entity is Substance)
        {
            ((Substance)entity).highlight = GetColor();
            foreach (Voxel v in ((Substance)entity).voxels)
                v.UpdateHighlight();
        }
    }

    public static void SetBehaviorTarget(Entity entity)
    {
        behaviorTarget = entity;
    }

    public static Color GetColor()
    {
        if (targetEntities[currentTargetEntityI] == currentEntity
                || targetEntities[currentTargetEntityI] == null)
            return Color.white;
        return ColorI(currentTargetEntityI);
    }

    private static Color ColorI(int i)
    {
        return Color.HSVToRGB((i * .618f) % 1.0f, 0.8f, 1.0f);
    }

    public static string GetName()
    {
        Entity entity = targetEntities[currentTargetEntityI];
        if (entity == null)
            return "None";
        else if (entity == currentEntity)
            return "Self";
        else if (entity == behaviorTarget)
            return "Target";
        else
            return entity.ToString();
    }

    void Awake()
    {
        _lineMaterial = lineMaterial;
        Clear();
    }

    void OnDestroy()
    {
        Clear();
    }

    void Update()
    {
        if (currentEntity == null && targetEntities.Count != 0)
        {
            targetEntities.Clear();
            currentTargetEntityI = -1;
        }

        if (transform.childCount != targetEntities.Count)
            updateTargets = true;
        else
            foreach (Transform child in transform)
            {
                EntityReferenceLine line = child.GetComponent<EntityReferenceLine>();
                if (targetEntities[line.i] != line.targetEntity || currentEntity != line.sourceEntity)
                {
                    updateTargets = true;
                    break;
                }
            }

        if (updateTargets)
        {
            updateTargets = false;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            for (int i = 0; i < targetEntities.Count; i++)
            {
                GameObject lineObject = new GameObject();
                lineObject.transform.parent = transform;
                EntityReferenceLine line = lineObject.AddComponent<EntityReferenceLine>();
                line.i = i;
                line.sourceEntity = currentEntity;
                line.targetEntity = targetEntities[i];
            }
            return; // wait a frame to let the new/deleted objects update
        }

        if (currentEntity != null)
            EntityPreviewManager.UpdateEntityPosition(currentEntity);
    }
}
