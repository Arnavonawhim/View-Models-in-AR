using UnityEngine;

[System.Serializable]
public class ModelData
{
    public string modelName;
    public GameObject modelPrefab;
    public Sprite thumbnailSprite;
    public Material modelMaterial;
}

[CreateAssetMenu(fileName = "ModelDatabase", menuName = "AR/Model Database")]
public class ModelDatabase : ScriptableObject
{
    public ModelData[] availableModels;
}