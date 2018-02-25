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

        private Vector3 EntityPosition(Entity entity)
        {
            if (entity is Substance)
            {
                // TODO: needs to find center of bounding box
                foreach (Voxel voxel in ((Substance)entity).voxels)
                    return voxel.transform.position + Vector3.one * 0.5f;
            }
            else if (entity is ObjectEntity)
            {
                return ((ObjectEntity)entity).marker.transform.position;
            }
            return Vector3.zero;
        }

        void Start()
        {
            LineRenderer line = gameObject.AddComponent<LineRenderer>();
            line.startWidth = line.endWidth = 0.1f;
            line.material = _lineMaterial;
            line.startColor = line.endColor = ColorI(i);
            line.SetPosition(0, EntityPosition(sourceEntity));
            line.SetPosition(1, EntityPosition(targetEntity));
        }
    }


    private static Entity currentEntity;
    private static List<Entity> targetEntities = new List<Entity>();

    private static Material _lineMaterial;
    public Material lineMaterial;

    public static void Reset(Entity entity)
    {
        currentEntity = entity;
        targetEntities.Clear();
    }

    public static void Next(Entity entity)
    {
        targetEntities.Add(entity);
    }

    public static Color GetColor()
    {
        return ColorI(targetEntities.Count - 1);
    }

    private static Color ColorI(int i)
    {
        return Color.HSVToRGB((i * .618f) % 1.0f, 1.0f, 1.0f);
    }

    public static string GetName()
    {
        Entity entity = targetEntities[targetEntities.Count - 1];
        if (entity == currentEntity)
            return "Self";
        return entity.ToString();
    }

    void Awake()
    {
        _lineMaterial = lineMaterial;
    }

    void Update()
    {
        if (currentEntity == null && targetEntities.Count != 0)
            targetEntities.Clear();

        bool updateTargets = false;

        if (transform.childCount != targetEntities.Count)
            updateTargets = true;
        else
            foreach (Transform child in transform)
            {
                EntityReferenceLine line = child.GetComponent<EntityReferenceLine>();
                if (targetEntities[line.i] != line.targetEntity)
                    updateTargets = true;
            }

        if (updateTargets)
        {
            Debug.Log("Update targets");
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
    }
}