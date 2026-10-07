using UnityEngine;

public class ObjectDragger : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging = false;
    private float zDistance;
    private Vector3 offset;

    void Start()
    {
        mainCam = Camera.main;
        zDistance = 5f; // default
    }

    void Update()
    {
        // Mulai drag
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    isDragging = true;
                    zDistance = mainCam.WorldToScreenPoint(transform.position).z;
                    offset = transform.position - GetMouseWorldPos();
                    Debug.Log($"[Drag] Start: {gameObject.name}");
                }
            }
        }

        // Drag
        if (isDragging && Input.GetMouseButton(0))
        {
            transform.position = GetMouseWorldPos() + offset;
        }

        // Lepas
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging) Debug.Log($"[Drag] End: {gameObject.name}");
            isDragging = false;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = zDistance;
        return mainCam.ScreenToWorldPoint(mouseScreen);
    }
}