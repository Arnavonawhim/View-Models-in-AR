using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class ARModelController : MonoBehaviour
{
    [Header("AR Components")]
    public GameObject xrOriginObject; // Changed to GameObject to avoid type issues
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
    private ModelData modelToPlace;
    private bool isPlacingModel = false;
    
    void Start()
    {
        SetupUI();
        CreateModelButtons();
        SetupARPlane();
    }
    
    void Update()
    {
        HandleTouchInput();
        HandleModelPlacement();
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
        backButton.onClick.AddListener(() => {
            Debug.Log("Back button clicked - Loading Main Menu");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Load scene 0 (MainMenu)
        });
    }
    
    void CreateModelButtons()
    {
        // Debug check
        if (modelDatabase == null)
        {
            Debug.LogError("ModelDatabase is not assigned!");
            return;
        }
        
        if (modelButtonParent == null)
        {
            Debug.LogError("Model Button Parent is not assigned!");
            return;
        }
        
        if (modelButtonPrefab == null)
        {
            Debug.LogError("Model Button Prefab is not assigned!");
            return;
        }
        
        Debug.Log($"Creating {modelDatabase.availableModels.Length} model buttons");
        
        foreach (ModelData model in modelDatabase.availableModels)
        {
            Debug.Log($"Creating button for: {model.modelName}");
            
            GameObject buttonObj = Instantiate(modelButtonPrefab, modelButtonParent);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError("Button component not found on model button prefab!");
                continue;
            }
            
            // Setup button appearance
            Text buttonText = buttonObj.GetComponentInChildren<Text>();
            Image[] images = buttonObj.GetComponentsInChildren<Image>();
            Image buttonImage = images.Length > 1 ? images[1] : null; // Second image is thumbnail
            
            if (buttonText != null)
                buttonText.text = model.modelName;
            else
                Debug.LogWarning("Button text component not found!");
                
            if (model.thumbnailSprite != null && buttonImage != null)
                buttonImage.sprite = model.thumbnailSprite;
            
            // Setup button functionality
            ModelData capturedModel = model; // Capture for closure
            button.onClick.AddListener(() => {
                Debug.Log($"Model button clicked: {capturedModel.modelName}");
                SelectModelToPlace(capturedModel);
            });
        }
        
        Debug.Log("Model buttons creation completed");
    }
    
    void SetupARPlane()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }
    
    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Handle plane detection if needed
        Debug.Log($"Planes detected: {args.added.Count} added, {args.updated.Count} updated");
    }
    
    void SelectModelToPlace(ModelData modelData)
    {
        modelToPlace = modelData;
        isPlacingModel = true;
        Debug.Log($"Selected {modelData.modelName} for placement. Tap on a detected plane to place it.");
    }
    
    void HandleModelPlacement()
    {
        if (!isPlacingModel || modelToPlace == null) return;
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPosition = touch.position;
                
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    PlaceModel(modelToPlace, hitPose.position, hitPose.rotation);
                    isPlacingModel = false;
                    modelToPlace = null;
                }
            }
        }
    }
    
    void PlaceModel(ModelData modelData, Vector3 position, Quaternion rotation)
    {
        GameObject newModel = Instantiate(modelData.modelPrefab, position, rotation);
        spawnedModels.Add(newModel);
        
        // Ensure the model has a collider for touch detection
        if (newModel.GetComponent<Collider>() == null)
        {
            newModel.AddComponent<BoxCollider>();
        }
        
        // Add touch detection script
        ModelTouchHandler touchHandler = newModel.GetComponent<ModelTouchHandler>();
        if (touchHandler == null)
        {
            touchHandler = newModel.AddComponent<ModelTouchHandler>();
        }
        touchHandler.Initialize(this);
        
        Debug.Log($"Placed {modelData.modelName} at {position}");
    }
    
    void HandleTouchInput()
    {
        if (selectedModel == null || Input.touchCount == 0 || isPlacingModel) return;
        
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            
            if (isRotateMode && touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = touch.position - (Vector2)lastTouchPosition;
                selectedModel.transform.Rotate(Vector3.up, -deltaPosition.x * rotationSpeed * Time.deltaTime, Space.World);
                selectedModel.transform.Rotate(arCamera.transform.right, deltaPosition.y * rotationSpeed * Time.deltaTime, Space.World);
            }
            
            lastTouchPosition = touch.position;
        }
        else if (Input.touchCount == 2 && isScaleMode)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                lastTouchDistance = currentDistance;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                float deltaDistance = currentDistance - lastTouchDistance;
                float scaleDelta = deltaDistance * scaleSpeed * Time.deltaTime * 0.01f;
                
                Vector3 newScale = selectedModel.transform.localScale + Vector3.one * scaleDelta;
                newScale = new Vector3(
                    Mathf.Clamp(newScale.x, minScale, maxScale),
                    Mathf.Clamp(newScale.y, minScale, maxScale),
                    Mathf.Clamp(newScale.z, minScale, maxScale)
                );
                selectedModel.transform.localScale = newScale;
                
                lastTouchDistance = currentDistance;
            }
        }
    }
    
    public void SelectModel(GameObject model)
    {
        // Deselect previous model
        if (selectedModel != null)
        {
            ResetModelColor(selectedModel);
        }
        
        selectedModel = model;
        controlPanel.SetActive(true);
        
        // Highlight selected model
        HighlightModel(selectedModel);
        
        // Reset modes
        isRotateMode = false;
        isScaleMode = false;
        UpdateButtonColors();
    }
    
    void HighlightModel(GameObject model)
    {
        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
    
    void ResetModelColor(GameObject model)
    {
        Renderer renderer = model.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
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
        ColorBlock rotateColors = rotateButton.colors;
        rotateColors.normalColor = isRotateMode ? Color.green : Color.white;
        rotateButton.colors = rotateColors;
        
        ColorBlock scaleColors = scaleButton.colors;
        scaleColors.normalColor = isScaleMode ? Color.green : Color.white;
        scaleButton.colors = scaleColors;
    }
    
    public void DeselectModel()
    {
        if (selectedModel != null)
        {
            ResetModelColor(selectedModel);
            selectedModel = null;
            controlPanel.SetActive(false);
            isRotateMode = false;
            isScaleMode = false;
            UpdateButtonColors();
        }
    }
}
