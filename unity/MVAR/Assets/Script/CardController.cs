using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Card Settings")]
    public Sprite defaultSprite;
    public Sprite pressedSprite;

    [Header("3D Object Settings")]
    public GameObject object3DPrefab;
    
    [Tooltip("Jarak object dari kamera")]
    public float spawnDistance = 3f;
    
    [Tooltip("Skala object")]
    public float objectScale = 1f;
    
    [Tooltip("Geser posisi setelah spawn (X=kanan, Y=atas)")]
    public Vector2 screenOffset = Vector2.zero;

    private Image cardImage;
    private GameObject spawnedObject;
    private Camera mainCam;
    private bool isPressed = false;

    void Start()
    {
        cardImage = GetComponent<Image>();
        mainCam = Camera.main;

        if (defaultSprite != null)
            cardImage.sprite = defaultSprite;
    }

    void Update()
    {
        if (isPressed && Input.GetMouseButtonUp(0))
        {
            ReleaseCard();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        if (pressedSprite != null)
            cardImage.sprite = pressedSprite;

        Spawn3DObject();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ReleaseCard();
    }

    void ReleaseCard()
    {
        isPressed = false;

        if (defaultSprite != null)
            cardImage.sprite = defaultSprite;

        Destroy3DObject();
    }

    void Spawn3DObject()
    {
        if (object3DPrefab == null)
        {
            Debug.LogError("[CardController] object3DPrefab belum di-assign!");
            return;
        }

        if (mainCam == null) mainCam = Camera.main;

        // === CARA BARU: Hitung posisi berdasarkan viewport kamera ===
        // Ambil posisi card di layar
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rect.position);
        screenPos += screenOffset;

        // Konversi ke viewport (0-1)
        Vector3 viewportPos = mainCam.ScreenToViewportPoint(screenPos);

        // Set Z = spawnDistance (di depan kamera)
        Vector3 worldPos = mainCam.ViewportToWorldPoint(
            new Vector3(viewportPos.x, viewportPos.y, spawnDistance)
        );

        Debug.Log($"[CardController] Screen: {screenPos} | World: {worldPos}");

        // Spawn object
        spawnedObject = Instantiate(object3DPrefab, worldPos, Quaternion.identity);
        spawnedObject.transform.localScale = Vector3.one * objectScale;

        // Paksa Z supaya pas di depan kamera
        Vector3 euler = mainCam.transform.eulerAngles;
        spawnedObject.transform.rotation = Quaternion.Euler(0, euler.y, 0); // hadap kamera

        if (spawnedObject.GetComponent<ObjectDragger>() == null)
            spawnedObject.AddComponent<ObjectDragger>();

        Debug.Log($"[CardController] Spawned at: {spawnedObject.transform.position}");
    }

    void Destroy3DObject()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    void OnDisable()
    {
        Destroy3DObject();
    }
}