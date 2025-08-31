using UnityEngine;

public class ModelTouchHandler : MonoBehaviour
{
    private ARModelController controller;
    
    public void Initialize(ARModelController arController)
    {
        controller = arController;
    }
    
    void OnMouseDown()
    {
        if (controller != null)
        {
            controller.SelectModel(gameObject);
        }
    }
}