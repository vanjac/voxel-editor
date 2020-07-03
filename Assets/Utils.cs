using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// an empty monobehaviour for running coroutines on any GameObject
public class CoroutineMonoBehaviour : MonoBehaviour { }


public static class Vector3Extension
{
    public static Vector3Int ToInt(this Vector3 v)
    {
        return new Vector3Int(
            Mathf.RoundToInt(v.x),
            Mathf.RoundToInt(v.y),
            Mathf.RoundToInt(v.z)
            );
    }
}