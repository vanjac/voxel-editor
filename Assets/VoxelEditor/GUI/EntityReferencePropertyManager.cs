using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityReferencePropertyManager : MonoBehaviour
{
    private static Entity currentEntity;
    private static List<Entity> targetEntities = new List<Entity>();

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
        return entity.TypeName();
    }

    void Update()
    {
        // TODO: this is not efficient at all
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        if (currentEntity == null || targetEntities.Count == 0)
            return;
        Vector3 sourcePosition = EntityPosition(currentEntity);
        int i = 0;
        foreach (Entity targetEntity in targetEntities)
        {
            GameObject lineObject = new GameObject();
            lineObject.transform.parent = transform;
            LineRenderer line = lineObject.AddComponent<LineRenderer>();

            line.startColor = line.endColor = ColorI(i);
            line.startWidth = line.endWidth = 0.1f;
            line.material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            line.SetPosition(0, sourcePosition);
            line.SetPosition(1, EntityPosition(targetEntity));
            i += 1;
        }
    }

    private Vector3 EntityPosition(Entity entity)
    {
        foreach (Voxel voxel in ((Substance)entity).voxels)
            return voxel.transform.position + Vector3.one * 0.5f;
        return Vector3.zero;
    }
}