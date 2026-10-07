using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pasang di CardTabungReaksi (UI Image di dalam ScrollAlat).
/// Tahan card  -> ScrollAlat tersembunyi + model 3D muncul di posisi card (ukuran menyesuaikan card).
/// Tahan + geser -> model 3D mengikuti jari/kursor.
/// Lepas -> model 3D hilang + ScrollAlat muncul lagi.
/// Geser jauh sebelum waktuTahan tercapai -> dianggap scroll, daftar alat bergulir seperti biasa.
/// Gerakan kecil (jari goyang) selama menahan TIDAK dianggap scroll.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardTahanGeser3D : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Referensi")]
    [Tooltip("Model 3D (prefab/FBX) yang dimunculkan")]
    public GameObject objek3D;
    [Tooltip("Kamera yang melihat scene. Kosongkan untuk pakai Camera.main")]
    public Camera kamera;
    [Tooltip("CanvasGroup di ScrollAlat. Kosongkan: dicari/ditambah otomatis")]
    public CanvasGroup panelScrollAlat;

    [Header("Tahan")]
    [Tooltip("Lama menahan (detik) sebelum model muncul. 0 = langsung saat ditekan")]
    public float waktuTahan = 0.3f;
    [Tooltip("Batas gerak jari (piksel) selama menahan. Lebih dari ini = scroll daftar")]
    public float toleransiGeser = 40f;

    [Header("Posisi dan Ukuran")]
    [Tooltip("Jarak model dari kamera")]
    public float jarakDepan = 0.3f;
    [Tooltip("1 = pas selebar/setinggi card. Naikkan untuk memperbesar")]
    public float pengaliUkuran = 1f;
    [Tooltip("Putaran tambahan (derajat) relatif terhadap kamera")]
    public Vector3 rotasiTambahan = Vector3.zero;

    private const string NamaWadah = "Model3D_Spawn";

    private RectTransform rect;
    private Canvas canvas;
    private ScrollRect scrollRect;
    private Camera cam;

    private bool pointerDitekan;
    private bool modeScroll;
    private bool modelAktif;
    private float waktuMulai;
    private Vector2 posisiTekan;
    private Vector2 posisiTerakhir;
    private int idPointer;

    private GameObject instans;
    private Vector2 offsetLayar;
    private Vector3 offsetPusatLokal;
    private float kedalaman;
    private Vector2 posisiLayarModel;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        scrollRect = GetComponentInParent<ScrollRect>();

        if (panelScrollAlat == null && scrollRect != null)
        {
            panelScrollAlat = scrollRect.GetComponent<CanvasGroup>();
            if (panelScrollAlat == null)
                panelScrollAlat = scrollRect.gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnDisable()
    {
        ResetSemua();
    }

    private void OnApplicationFocus(bool fokus)
    {
        if (!fokus) ResetSemua();
    }

    private void OnApplicationPause(bool jeda)
    {
        if (jeda) ResetSemua();
    }

    private void Update()
    {
        if (pointerDitekan && !modelAktif && !modeScroll &&
            Time.unscaledTime - waktuMulai >= waktuTahan)
        {
            Aktifkan();
        }
    }

    private void LateUpdate()
    {
        if (modelAktif && instans != null && cam != null)
        {
            GerakkanModel(posisiLayarModel);
            if (ManajerSlotTabung.Instance != null)
                ManajerSlotTabung.Instance.Sorot(posisiLayarModel, true, cam);
        }
    }

    // ---------- Event pointer ----------

    public void OnPointerDown(PointerEventData e)
    {
        // Tekanan terbaru selalu menang, supaya state tidak macet kalau event lepas terlewat
        ResetSemua();
        pointerDitekan = true;
        idPointer = e.pointerId;
        posisiTekan = e.position;
        posisiTerakhir = e.position;
        waktuMulai = Time.unscaledTime;
        if (waktuTahan <= 0f) Aktifkan();
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (e.pointerId != idPointer) return;
        pointerDitekan = false;
        if (modelAktif)
        {
            posisiLayarModel = e.position;
            Selesai(true);
        }
    }

    public void OnInitializePotentialDrag(PointerEventData e)
    {
        if (scrollRect != null) scrollRect.OnInitializePotentialDrag(e);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        // Sengaja kosong: keputusan scroll/tahan diambil di OnDrag berdasarkan jarak gerak
    }

    public void OnDrag(PointerEventData e)
    {
        if (modelAktif)
        {
            GerakkanModel(e.position);
            return;
        }

        if (!pointerDitekan || scrollRect == null) return;

        if (!modeScroll)
        {
            posisiTerakhir = e.position;
            // Masih dalam toleransi: anggap jari goyang, tetap menunggu tahan
            if ((e.position - posisiTekan).sqrMagnitude < toleransiGeser * toleransiGeser) return;

            // Geser jauh sebelum waktuTahan: ini scroll
            modeScroll = true;
            scrollRect.OnBeginDrag(e);
        }

        scrollRect.OnDrag(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (modeScroll && scrollRect != null) scrollRect.OnEndDrag(e);
        modeScroll = false;
    }

    // ---------- Logika utama ----------

    private void Aktifkan()
    {
        cam = kamera != null ? kamera : Camera.main;
        if (objek3D == null || cam == null)
        {
            Debug.LogWarning("CardTahanGeser3D: objek3D atau kamera belum diisi.");
            pointerDitekan = false;
            return;
        }

        kedalaman = jarakDepan;
        if (cam.nearClipPlane >= kedalaman)
            Debug.LogWarning("CardTahanGeser3D: Near Clip kamera >= jarakDepan, model bisa terpotong. Besarkan jarakDepan.");

        // 1. Ukuran dan pusat card di layar
        Camera camUI = null;
        if (canvas != null)
        {
            Canvas root = canvas.rootCanvas;
            if (root.renderMode == RenderMode.ScreenSpaceCamera) camUI = root.worldCamera;
            else if (root.renderMode == RenderMode.WorldSpace) camUI = root.worldCamera != null ? root.worldCamera : cam;
        }

        Vector3[] sudut = new Vector3[4];
        rect.GetWorldCorners(sudut);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 4; i++)
        {
            Vector2 s = RectTransformUtility.WorldToScreenPoint(camUI, sudut[i]);
            min = Vector2.Min(min, s);
            max = Vector2.Max(max, s);
        }

        Vector2 pusatLayar = (min + max) * 0.5f;
        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(min.x, min.y, kedalaman));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(max.x, max.y, kedalaman));
        Vector3 diag = tr - bl;
        float lebarCard = Mathf.Abs(Vector3.Dot(diag, cam.transform.right));
        float tinggiCard = Mathf.Abs(Vector3.Dot(diag, cam.transform.up));

        // 2. Buat model di bawah wadah khusus, tegak lurus terhadap kamera
        instans = Instantiate(objek3D);
        instans.name = objek3D.name + "_drag";
        instans.transform.SetParent(AmbilWadah(), true);
        instans.transform.rotation = cam.transform.rotation * Quaternion.Euler(rotasiTambahan) * objek3D.transform.rotation;
        instans.transform.position = cam.transform.position + cam.transform.forward * kedalaman;
        instans.SetActive(true);

        // 3. Skala supaya pas dengan ukuran card
        Vector3 pusatKam;
        Vector2 ukuran;
        if (UkurDiRuangKamera(instans, cam.transform, out pusatKam, out ukuran) && ukuran.x > 0.0001f && ukuran.y > 0.0001f)
        {
            float faktor = Mathf.Min(lebarCard / ukuran.x, tinggiCard / ukuran.y) * pengaliUkuran;
            instans.transform.localScale *= faktor;
            UkurDiRuangKamera(instans, cam.transform, out pusatKam, out ukuran);
        }
        else
        {
            Debug.LogWarning("CardTahanGeser3D: model tidak punya Renderer, ukuran tidak bisa disesuaikan.");
        }

        // 4. Taruh pusat model tepat di pusat card
        Vector3 targetDunia = cam.ScreenToWorldPoint(new Vector3(pusatLayar.x, pusatLayar.y, kedalaman));
        Vector3 pusatDunia = cam.transform.TransformPoint(pusatKam);
        instans.transform.position += targetDunia - pusatDunia;
        offsetPusatLokal = cam.transform.InverseTransformVector(instans.transform.position - targetDunia);
        offsetLayar = pusatLayar - posisiTerakhir;
        posisiLayarModel = posisiTerakhir;

        // 5. Sembunyikan ScrollAlat (tetap aktif supaya event lepas klik tetap diterima)
        if (panelScrollAlat != null)
        {
            panelScrollAlat.alpha = 0f;
            panelScrollAlat.blocksRaycasts = false;
        }

        modelAktif = true;
    }

    private void GerakkanModel(Vector2 posisiLayar)
    {
        if (instans == null || cam == null) return;
        posisiLayarModel = posisiLayar;
        Vector2 s = posisiLayar + offsetLayar;
        Vector3 pusat = cam.ScreenToWorldPoint(new Vector3(s.x, s.y, kedalaman));
        instans.transform.position = pusat + cam.transform.TransformVector(offsetPusatLokal);
    }

    private void Selesai(bool cobaTaruh)
    {
        modelAktif = false;
        ManajerSlotTabung manajer = ManajerSlotTabung.Instance;
        if (manajer != null) manajer.Sorot(posisiLayarModel, false, cam);
        if (instans != null)
        {
            bool ditaruh = false;
            if (cobaTaruh && manajer != null)
                ditaruh = manajer.CobaTaruh(posisiLayarModel, instans, cam, kedalaman);
            if (!ditaruh) Destroy(instans);
            instans = null;
        }
        if (panelScrollAlat != null)
        {
            panelScrollAlat.alpha = 1f;
            panelScrollAlat.blocksRaycasts = true;
        }
    }

    private void ResetSemua()
    {
        pointerDitekan = false;
        modeScroll = false;
        if (modelAktif) Selesai(false);
    }

    private static Transform AmbilWadah()
    {
        GameObject g = GameObject.Find(NamaWadah);
        if (g == null) g = new GameObject(NamaWadah);
        return g.transform;
    }

    // Hitung pusat dan ukuran (X,Y) model di ruang kamera
    public static bool UkurDiRuangKamera(GameObject go, Transform kamT, out Vector3 pusat, out Vector2 ukuran)
    {
        pusat = Vector3.zero;
        ukuran = Vector2.zero;
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        bool ada = false;
        Vector3 mn = Vector3.zero;
        Vector3 mx = Vector3.zero;

        for (int i = 0; i < rs.Length; i++)
        {
            Renderer r = rs[i];
            Bounds b;
            bool lokal = false;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (!(r is SkinnedMeshRenderer) && mf != null && mf.sharedMesh != null)
            {
                b = mf.sharedMesh.bounds;
                lokal = true;
            }
            else
            {
                b = r.bounds;
            }

            for (int k = 0; k < 8; k++)
            {
                Vector3 sign = new Vector3((k & 1) == 0 ? -1f : 1f, (k & 2) == 0 ? -1f : 1f, (k & 4) == 0 ? -1f : 1f);
                Vector3 c = b.center + Vector3.Scale(b.extents, sign);
                Vector3 w = lokal ? r.transform.TransformPoint(c) : c;
                Vector3 p = kamT.InverseTransformPoint(w);
                if (!ada) { mn = p; mx = p; ada = true; }
                else { mn = Vector3.Min(mn, p); mx = Vector3.Max(mx, p); }
            }
        }

        if (!ada) return false;
        pusat = (mn + mx) * 0.5f;
        ukuran = new Vector2(mx.x - mn.x, mx.y - mn.y);
        return true;
    }
}
