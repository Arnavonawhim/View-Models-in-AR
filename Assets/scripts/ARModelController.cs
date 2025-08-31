using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ARModelController : MonoBehaviour
{
    [Header("AR Components")]
    public ARSessionOrigin arSessionOrigin;
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public Camera arCamera;
    
    [Header("UI References")]
    public Transform modelButtonParent;
    public GameObject modelButtonPrefab;
    public GameObject controlPanel;
    public Button rotateButton;
    public Button scaleButton;
    public Button deleteButton;
    public Button backButton;
    
    [Header("Model Settings")]
    public ModelDatabase modelDatabase;
    public float rotationSpeed = 100f;
    public float scaleSpeed = 0.5f;
    public float minScale = 0.1f;
    public float maxScale = 2f;
    
    private List<GameObject> spawnedModels = new List<GameObject>();
    private GameObject selectedModel;
    private bool isRotateMode = false;
    private bool isScaleMode = false;
    private Vector3 lastTouchPosition;
    private float lastTouchDistance;
    
    void Start()
    {
        SetupUI();
        CreateModelButtons();
        SetupARPlane();
    }
    
    void Update()
    {
        HandleTouchInput();
    }
    
    void SetupUI()
    {
        controlPanel.SetActive(false);
        
        rotateButton.onClick.AddListener(() => {
            isRotateMode = !isRotateMode;
            isScaleMode = false;
            UpdateButtonColors();
        });
        
        scaleButton.onClick.AddListener(() => {
            isScaleMode = !isScaleMode;
            isRotateMode = false;
            UpdateButtonColors();
        });
        
        deleteButton.onClick.AddListener(DeleteSelectedModel);
        backButton.onClick.AddListener(() => GameManager.Instance.LoadMainMenu());
    }
    
    void CreateModelButtons()
    {
        foreach (ModelData model in modelDatabase.availableModels)
        {
            GameObject buttonObj = Instantiate(modelButtonPrefab, modelButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            
            // Setup button appearance
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            Image buttonImage = buttonObj.GetComponentsInChildren<Image>()[1]; // Second image is thumbnail
            
            buttonText.text = model.modelName;
            if (model.thumbnailSprite != null)
                buttonImage.sprite = model.thumbnailSprite;
            
            // Setup button functionality
            ModelData capturedModel = model; // Capture for closure
            button.onClick.AddListener(() => SelectModelToPlace(capturedModel));
        }
    }
    
    void SetupARPlane()
    {
        // This will be called when planes are detected
        planeManager.planesChanged += OnPlanesChanged;
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Handle plane detection if needed
    }
    
    void SelectModelToPlace(ModelData modelData)
    {
        // Store the model to place on next touch
        StartCoroutine(PlaceModelCoroutine(modelData));
    }
    
    System.Collections.IEnumerator PlaceModelCoroutine(ModelData modelData)
    {
        Debug.Log($"Tap on a detected plane to place {modelData.modelName}");
        
        bool modelPlaced = false;
        while (!modelPlaced)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                    {
                        Pose hitPose = hits[0].pose;
                        PlaceModel(modelData, hitPose.position, hitPose.rotation);
                        modelPlaced = true;
                    }
                }
            }
            yield return null;
        }
    }
    
    void PlaceModel(ModelData modelData, Vector3 position, Quaternion rotation)
    {
        GameObject newModel = Instantiate(modelData.modelPrefab, position, rotation);
        spawnedModels.Add(newModel);
        
        // Add touch detection
        ModelTouchHandler touchHandler = newModel.AddComponent<ModelTouchHandler>();
        touchHandler.Initialize(this);
        
        Debug.Log($"Placed {modelData.modelName} at {position}");
    }
    
    void HandleTouchInput()
    {
        if (selectedModel == null || Input.touchCount == 0) return;
        
        Touch touch = Input.GetTouch(0);
        
        if (isRotateMode && touch.phase == TouchPhase.Moved)
        {
            Vector2 deltaPosition = touch.position - (Vector2)lastTouchPosition;
            selectedModel.transform.Rotate(Vector3.up, -deltaPosition.x * rotationSpeed * Time.deltaTime, Space.World);
        }
        else if (isScaleMode && Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float deltaDistance = currentDistance - lastTouchDistance;
                float scaleDelta = deltaDistance * scaleSpeed * Time.deltaTime;
                
                Vector3 newScale = selectedModel.transform.localScale + Vector3.one * scaleDelta;
                newScale = Vector3.Clamp(newScale, Vector3.one * minScale, Vector3.one * maxScale);
                selectedModel.transform.localScale = newScale;
            }
            
            lastTouchDistance = currentDistance;
        }
        
        lastTouchPosition = touch.position;
    }
    
    public void SelectModel(GameObject model)
    {
        selectedModel = model;
        controlPanel.SetActive(true);
        
        // Visual feedback for selected model
        foreach (GameObject spawned in spawnedModels)
        {
            Renderer renderer = spawned.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = (spawned == selectedModel) ? Color.yellow : Color.white;
            }
        }
    }
    
    void DeleteSelectedModel()
    {
        if (selectedModel != null)
        {
            spawnedModels.Remove(selectedModel);
            Destroy(selectedModel);
            selectedModel = null;
            controlPanel.SetActive(false);
            isRotateMode = false;
            isScaleMode = false;
        }
    }
    
    void UpdateButtonColors()
    {
        rotateButton.GetComponent<Image>().color = isRotateMode ? Color.green : Color.white;
        scaleButton.GetComponent<Image>().color = isScaleMode ? Color.green : Color.white;
    }
}