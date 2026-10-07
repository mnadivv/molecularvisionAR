using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Pasang di GameObject "card3dlogic" (UI Image).
/// Klik card isi   -> card diganti card3dlogicKosong + objek 3D muncul di depan card.
/// Klik card kosong -> objek 3D hilang + card isi kembali.
/// Tidak perlu komponen Button, tapi scene harus punya EventSystem
/// dan Raycast Target pada Image kedua card harus aktif.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class card3dlogic : MonoBehaviour, IPointerClickHandler
{
    [Header("Referensi")]
    [Tooltip("Card pengganti (card3dlogicKosong), boleh dalam keadaan nonaktif")]
    public GameObject cardKosong;

    [Tooltip("Objek 3D tabungreaksi. Boleh prefab (dari Project) atau objek di Hierarchy")]
    public GameObject objek3D;

    [Tooltip("Kamera yang melihat scene. Kosongkan untuk pakai Camera.main")]
    public Camera kamera;

    [Header("Posisi Objek 3D")]
    [Tooltip("Jarak objek dari card ke arah depan (ke arah penonton)")]
    public float jarakDepan = 0.3f;

    [Tooltip("Geser tambahan (world space), mis. naik sedikit")]
    public Vector3 offsetTambahan = Vector3.zero;

    [Tooltip("Objek 3D menghadap kamera")]
    public bool hadapKamera = false;

    [Header("Ukuran Objek 3D")]
    [Tooltip("Pengali ukuran. 1 = ukuran asli, 2 = dua kali lebih besar, 0.5 = setengah")]
    public float skala = 1f;

    [Tooltip("Pengali tambahan per sumbu (X, Y, Z), kalau tabung perlu dipanjangkan/dipipihkan")]
    public Vector3 skalaPerSumbu = Vector3.one;

    private RectTransform rect;
    private Canvas canvas;
    private GameObject objekAktif;      // instance objek 3D yang sedang dipakai
    private Vector3 ukuranAsli = Vector3.one;
    private bool sedangTampil;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (kamera == null) kamera = Camera.main;

        // Siapkan objek 3D
        if (objek3D != null)
        {
            if (objek3D.scene.IsValid())
            {
                objekAktif = objek3D;           // objek di Hierarchy
                objekAktif.SetActive(false);
            }
            ukuranAsli = objek3D.transform.localScale;
        }

        // Card kosong ikut menerima klik untuk membalikkan keadaan
        if (cardKosong != null)
        {
            var relay = cardKosong.GetComponent<KlikRelay>();
            if (relay == null) relay = cardKosong.AddComponent<KlikRelay>();
            relay.aksi = Kembalikan;
            cardKosong.SetActive(false);
        }
    }

    // Supaya ukuran bisa diubah langsung di Inspector saat Play
    private void Update()
    {
        if (sedangTampil && objekAktif != null)
            TerapkanUkuran();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (sedangTampil) return;
        Tampilkan();
    }

    private void Tampilkan()
    {
        sedangTampil = true;
        Vector3 posisiDepan = HitungPosisiDepanCard();

        // Ganti card
        if (cardKosong != null)
        {
            cardKosong.transform.SetSiblingIndex(transform.GetSiblingIndex());
            cardKosong.SetActive(true);
        }

        // Munculkan objek 3D
        if (objek3D != null)
        {
            if (objekAktif == null)
                objekAktif = Instantiate(objek3D);   // prefab -> buat instance

            objekAktif.transform.position = posisiDepan;
            TerapkanUkuran();

            if (hadapKamera && kamera != null)
                objekAktif.transform.rotation = Quaternion.LookRotation(
                    objekAktif.transform.position - kamera.transform.position);

            objekAktif.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    // Dipanggil saat card kosong diklik
    private void Kembalikan()
    {
        if (!sedangTampil) return;
        sedangTampil = false;

        if (objekAktif != null)
            objekAktif.SetActive(false);

        transform.SetSiblingIndex(cardKosong != null
            ? cardKosong.transform.GetSiblingIndex()
            : transform.GetSiblingIndex());

        if (cardKosong != null) cardKosong.SetActive(false);
        gameObject.SetActive(true);
    }

    private void TerapkanUkuran()
    {
        objekAktif.transform.localScale = Vector3.Scale(ukuranAsli * skala, skalaPerSumbu);
    }

    private Vector3 HitungPosisiDepanCard()
    {
        Vector3 posisi;

        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            // Canvas world space: "depan" card = berlawanan dengan forward canvas
            posisi = rect.position - rect.forward * jarakDepan;
        }
        else
        {
            // Canvas overlay / camera: posisi card di layar -> posisi world
            Camera camUI = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                ? canvas.worldCamera : null;

            Vector2 titikLayar = RectTransformUtility.WorldToScreenPoint(camUI, rect.position);
            Camera cam = kamera != null ? kamera : Camera.main;
            posisi = cam.ScreenToWorldPoint(new Vector3(titikLayar.x, titikLayar.y, jarakDepan));
        }

        return posisi + offsetTambahan;
    }

    // Komponen kecil yang otomatis dipasang ke card3dlogicKosong (tidak perlu di-drop manual)
    private class KlikRelay : MonoBehaviour, IPointerClickHandler
    {
        public System.Action aksi;
        public void OnPointerClick(PointerEventData eventData) { aksi?.Invoke(); }
    }
}