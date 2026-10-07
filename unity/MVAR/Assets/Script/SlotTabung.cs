using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Satu titik penempatan tabung (label A atau B).
/// Pasang di objek UI SlotA / SlotB. Dipakai oleh ManajerSlotTabung.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlotTabung : MonoBehaviour
{
    [Tooltip("Nama slot, hanya untuk penamaan objek 3D (A / B)")]
    public string namaSlot = "A";
    [Tooltip("Gambar penanda area drop. Disembunyikan saat slot terisi")]
    public Graphic penandaArea;
    [Range(0f, 1f)] public float alphaNormal = 0.18f;
    [Range(0f, 1f)] public float alphaSorot = 0.45f;

    private RectTransform rect;
    private Canvas canvas;
    private GameObject isi;

    public bool Terisi { get { return isi != null; } }

    public RectTransform Rect
    {
        get
        {
            if (rect == null) rect = (RectTransform)transform;
            return rect;
        }
    }

    public Camera KameraUI(Camera camAR)
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        Canvas root = canvas.rootCanvas;
        if (root.renderMode == RenderMode.ScreenSpaceCamera) return root.worldCamera;
        if (root.renderMode == RenderMode.WorldSpace) return root.worldCamera != null ? root.worldCamera : camAR;
        return null;
    }

    public bool Mengandung(Vector2 posisiLayar, Camera camAR)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(Rect, posisiLayar, KameraUI(camAR));
    }

    public void AmbilRectLayar(Camera camAR, out Vector2 min, out Vector2 max, out Vector2 pusat)
    {
        Camera camUI = KameraUI(camAR);
        Vector3[] sudut = new Vector3[4];
        Rect.GetWorldCorners(sudut);
        min = new Vector2(float.MaxValue, float.MaxValue);
        max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 4; i++)
        {
            Vector2 s = RectTransformUtility.WorldToScreenPoint(camUI, sudut[i]);
            min = Vector2.Min(min, s);
            max = Vector2.Max(max, s);
        }
        pusat = (min + max) * 0.5f;
    }

    public void SetSorot(bool sorot)
    {
        if (penandaArea == null || Terisi) return;
        Color c = penandaArea.color;
        c.a = sorot ? alphaSorot : alphaNormal;
        penandaArea.color = c;
    }

    /// <summary>Tempatkan model 3D di slot ini, ukurannya disesuaikan dengan area slot.</summary>
    public void Isi(GameObject model, Camera cam, float kedalaman)
    {
        isi = model;

        Vector2 min, max, pusat;
        AmbilRectLayar(cam, out min, out max, out pusat);
        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(min.x, min.y, kedalaman));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(max.x, max.y, kedalaman));
        Vector3 diag = tr - bl;
        float lebar = Mathf.Abs(Vector3.Dot(diag, cam.transform.right));
        float tinggi = Mathf.Abs(Vector3.Dot(diag, cam.transform.up));

        Vector3 pusatKam;
        Vector2 ukuran;
        if (CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out pusatKam, out ukuran)
            && ukuran.x > 0.0001f && ukuran.y > 0.0001f)
        {
            float f = Mathf.Min(lebar / ukuran.x, tinggi / ukuran.y);
            model.transform.localScale *= f;
            CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out pusatKam, out ukuran);
        }

        Vector3 offset = cam.transform.InverseTransformPoint(model.transform.position) - pusatKam;
        Quaternion rotRelatif = Quaternion.Inverse(cam.transform.rotation) * model.transform.rotation;

        TabungTertempel t = model.GetComponent<TabungTertempel>();
        if (t == null) t = model.AddComponent<TabungTertempel>();
        t.Pasang(this, cam, kedalaman, offset, rotRelatif);

        model.name = "Tabung_" + namaSlot;
        if (penandaArea != null) penandaArea.enabled = false;
    }

    public void Kosongkan()
    {
        if (isi != null) Destroy(isi);
        isi = null;
        if (penandaArea != null)
        {
            penandaArea.enabled = true;
            SetSorot(false);
        }
    }
}
