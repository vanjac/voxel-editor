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
        private LineRenderer line;

        void Start()
        {
            if (sourceEntity == null || targetEntity == null)
                return;
            Color color = ColorI(i);
            line = gameObject.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.1f;
            line.material = _lineMaterial;
            line.startColor = line.endColor = color;
            UpdatePositions();
        }

        public void UpdatePositions()
        {
            line.SetPosition(0, sourceEntity.PositionInEditor());
            line.SetPosition(1, targetEntity.PositionInEditor());
        }
    }


    private static bool updateTargets = false;
    private static Entity currentEntity;
    private static List<Entity> targetEntities = new List<Entity>();
    private static HashSet<Entity> entitiesToClear = new HashSet<Entity>();
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
        foreach (Entity clear in entitiesToClear)
        {
            if (clear != null && clear != entity)
                clear.SetHighlight(Color.clear);
        }
        entitiesToClear.Clear();
        entitiesToClear.UnionWith(targetEntities);

        targetEntities.Clear();
        currentTargetEntityI = -1;

        if (currentEntity != entity)
        {
            if (currentEntity != null)
            {
                // entity deselected
                currentEntity.SetHighlight(Color.clear);
            }
            if (entity != null)
            {
                // entity selected
                entity.SetHighlight(Color.white);
                EntityPreviewManager.AddEntity(entity); // refresh
            }
        }
        currentEntity = entity;
        behaviorTarget = null;
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
        entitiesToClear.Remove(entity);
        currentTargetEntityI = targetEntities.Count - 1;
        if (entity != null)
            entity.SetHighlight(GetColor());
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
        else
        {
            foreach (Transform child in transform)
                child.GetComponent<EntityReferenceLine>().UpdatePositions();
        }

        if (currentEntity != null)
            EntityPreviewManager.UpdateEntityPosition(currentEntity);
    }
}
