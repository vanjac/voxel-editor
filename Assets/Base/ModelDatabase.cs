using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ModelCategory
{
    public string name;
    public List<string> models;
}

[System.Serializable]
[CreateAssetMenu(fileName = "models", menuName = "ScriptableObjects/N-Space Model Database")]
public class ModelDatabase : ScriptableObject
{
    public List<ModelCategory> categories = new List<ModelCategory>();
}
