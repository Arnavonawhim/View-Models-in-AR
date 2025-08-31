using UnityEngine;

[CreateAssetMenu(fileName = "ModelDatabase", menuName = "AR/Model Database")]
public class ModelDatabase : ScriptableObject
{
    public ModelData[] availableModels;
}
