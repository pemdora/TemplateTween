using UnityEngine;

// Select an object and display it 
public class ObjectSelector : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform uiDisplayObj;  // N'importe quel script qui implémente IUIDisplay 
    private IUIDisplay uiDisplay;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (uiDisplayObj != null)
        {
            uiDisplay = uiDisplayObj.GetComponent<IUIDisplay>();
        }

        if (uiDisplay == null)
        {
            Debug.Log($"<color=orange> Missing UIDisplay component or Object not assigned! </color>");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition); // Raycast from camera with mouse input

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject; // Hit a 3d object
                string objectName = clickedObject.name;

                if (uiDisplay != null) // Show UI display
                {
                    uiDisplay.Open(objectName);
                }
            }
        }
    }
}