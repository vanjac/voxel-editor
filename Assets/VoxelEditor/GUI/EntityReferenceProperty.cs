using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityReferencePropertyManager
{
    private static Entity currentEntity;
    private static int propertyNum;

    public static void Reset(Entity entity)
    {
        currentEntity = entity;
        propertyNum = 0;
    }

    public static void Next()
    {
        propertyNum += 1;
    }

    public static Color GetColor()
    {
        return Color.HSVToRGB(((propertyNum - 1) * .618f) % 1.0f, 1.0f, 1.0f);
    }

    public static string GetName(Entity entity)
    {
        if (entity == currentEntity)
            return "Self";
        return entity.TypeName();
    }
}