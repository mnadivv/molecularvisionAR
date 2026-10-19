using UnityEngine;
public class ModelTertempelLayar : MonoBehaviour
{
    private RectTransform target;
    private Camera kamera;
    private Vector2 offsetLayar;
    private float kedalaman;
    private Quaternion rotasiRelatif;
    private bool terpasang;
    public void Ikat(RectTransform area, Camera cam)
    {
        target = area;
        kamera = cam != null ? cam : Camera.main;
        if (target == null || kamera == null) return;
        Camera kameraUI = KameraUntukTarget(target, kamera);
        Vector2 pusatTarget = RectTransformUtility.WorldToScreenPoint(kameraUI, target.position);
        Vector3 titikModel = kamera.WorldToScreenPoint(transform.position);
        offsetLayar = new Vector2(titikModel.x, titikModel.y) - pusatTarget;
        kedalaman = Mathf.Max(kamera.nearClipPlane + 0.02f,
            Vector3.Dot(transform.position - kamera.transform.position, kamera.transform.forward));
        rotasiRelatif = Quaternion.Inverse(kamera.transform.rotation) * transform.rotation;
        terpasang = true;
    }
    private void LateUpdate()
    {
        if (!terpasang || target == null || kamera == null) return;
        Camera kameraUI = KameraUntukTarget(target, kamera);
        Vector2 pusatTarget = RectTransformUtility.WorldToScreenPoint(kameraUI, target.position);
        transform.position = kamera.ScreenToWorldPoint(new Vector3(
            pusatTarget.x + offsetLayar.x, pusatTarget.y + offsetLayar.y, kedalaman));
        transform.rotation = kamera.transform.rotation * rotasiRelatif;
    }
    private static Camera KameraUntukTarget(RectTransform rect, Camera fallback)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (canvas == null) return fallback;
        Canvas root = canvas.rootCanvas;
        if (root.renderMode == RenderMode.ScreenSpaceCamera) return root.worldCamera;
        if (root.renderMode == RenderMode.WorldSpace)
            return root.worldCamera != null ? root.worldCamera : fallback;
        return null;
    }
}