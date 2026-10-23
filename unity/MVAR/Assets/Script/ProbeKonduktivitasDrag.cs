using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Token UI yang berperan sebagai ujung probe merah atau hitam yang dapat diseret.</summary>
public class ProbeKonduktivitasDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ManajerSpotPlate manajer;
    private RectTransform rect;
    private RectTransform parentRect;
    private CanvasGroup canvasGroup;
    private Vector2 posisiAwal;
    private bool merah;

    public void Inisialisasi(ManajerSpotPlate pemilik, bool probeMerah)
    {
        manajer = pemilik;
        merah = probeMerah;
        rect = transform as RectTransform;
        parentRect = transform.parent as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (rect == null) rect = transform as RectTransform;
        if (parentRect == null && transform.parent != null) parentRect = transform.parent as RectTransform;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null) return;
        posisiAwal = rect.anchoredPosition;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null || parentRect == null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        Camera uiCam = canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.rootCanvas.worldCamera
            : null;
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, uiCam, out local))
            rect.anchoredPosition = local;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        if (manajer != null) manajer.ProbeBaris3Dilepas(merah, eventData.position, rect);
        if (rect != null) rect.anchoredPosition = posisiAwal;
    }
}
