using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mengatur seluruh logika Percobaan 2 (Spot Plate / Plate Tetes).
/// Pasang di ManajerLatihan.
/// Grid 3 kolom x 4 baris (baris ke-4 kosong / tidak dipakai soal 2-4).
/// Kolom 1=Asam Benzoat, Kolom 2=MgCl2, Kolom 3=Petroleum Eter.
/// Baris 1=Akuades, Baris 2=Etanol, Baris 3=Etanol+Akuades (1:1).
/// </summary>
public class ManajerSpotPlate : MonoBehaviour
{
    public static ManajerSpotPlate Instance { get; private set; }

    // ===== Referensi UI =====
    [Header("Plate Tetes")]
    [Tooltip("Prefab / model 3D plate tetes.")]
    public GameObject objekPlateTetes;
    [Tooltip("Prefab spatula yang ditampilkan saat bahan asam benzoat dipilih.")]
    public GameObject objekSpatulaAsam;
    [Tooltip("Prefab serbuk asam benzoat yang ditempelkan di ujung spatula.")]
    public GameObject objekSerbukAsam;
    [Tooltip("Slot UI tempat plate tetes di-drop (sama dengan SlotRak di P1).")]
    public SlotTabung slotPlate;

    [Header("Panel Soal")]
    public GameObject soal1; // siapkan plate
    public GameObject soal2; // asam benzoat kolom1
    public GameObject soal3; // mgcl2 kolom2
    public GameObject soal4; // petroleum eter kolom3
    public GameObject soal5; // akuades baris1
    public GameObject soal6; // etanol baris2
    public GameObject soal7; // etanol+akuades baris3 + tespen
    [Header("Diagram Soal 2")]
    public Sprite diagramPenempatanAsam;

    [Header("Notifikasi")]
    public GameObject panelNotif;
    public TMP_Text teksNotif;
    public float durasiNotif = 1.8f;

    [Header("Tusuk Gigi & Tespen")]
    public GameObject objekTusukGigi;
    public GameObject objekTespen;

    [Header("Material")]
    public Material matLarutan;    // URP Lit transparan
    public Material matPadatan;    // partikel kecil
    public Material matTespen;

    // ===== State internal =====
    public enum FaseP2 { Idle, PlateSiap, IsiKolom1, IsiKolom2, IsiKolom3, CampurBaris1, CampurBaris2, CampurBaris3, Selesai }
    private FaseP2 fase = FaseP2.Idle;

    // Grid spot: [kolom 0-2][baris 0-2]
    private SpotData[,] grid = new SpotData[3, 3];

    // Warna dasar tiap kolom (sampel)
    private static readonly Color[] warnaSampel =
    {
        new Color(0.95f, 0.95f, 0.80f, 0.9f), // asam benzoat: putih kekuningan
        new Color(0.85f, 0.95f, 1.00f, 0.9f), // mgcl2: putih kebiru-biruan
        new Color(0.75f, 0.85f, 0.60f, 0.9f), // petroleum eter: hijau muda
    };

    // Hasil campuran per kolom x baris
    // [kolom][baris] -> (kelarutan, warna hasil)
    private static readonly string[,] hasilKelarutan =
    {
        { "Sedikit larut", "Larut", "Larut" },       // asam benzoat
        { "Sangat larut",  "Larut", "Larut" },       // mgcl2
        { "Tidak larut",   "Larut", "Larut" },       // petroleum eter
    };

    private static readonly Color[,] warnaHasil =
    {
        { new Color(0.92f,0.92f,0.72f,0.75f), new Color(0.95f,0.90f,0.70f,0.85f), new Color(0.95f,0.90f,0.70f,0.85f) },
        { new Color(0.60f,0.85f,1.00f,0.85f), new Color(0.60f,0.85f,1.00f,0.85f), new Color(0.60f,0.85f,1.00f,0.85f) },
        { new Color(0.75f,0.85f,0.60f,0.85f), new Color(0.78f,0.88f,0.60f,0.85f), new Color(0.78f,0.88f,0.60f,0.85f) },
    };

    private class SpotData
    {
        public GameObject silinder;   // cairan di spot
        public Material mat;
        public bool terisi;           // sudah ada sampel
        public bool[] campuran = new bool[3]; // campuran baris sudah selesai
    }

    // Jumlah tetes/spatula yang sudah dimasukkan per kolom (untuk soal 2-4)
    private int[] hitungIsian = new int[3];
    private bool[] kolomSelesai = new bool[3];
    private bool[] barisSelesai = new bool[3];
    private bool[,] sampelDiuji = new bool[3, 3];

    // Coroutine aktif
    private Coroutine animAktif;

    // Referensi plate yang sudah di-drop
    private Transform platTransform;
    private int pendingKolomAsam = -1;
    private int pendingBarisAsam = -1;
    private GameObject pendingModelAsam;
    private GameObject panelTakaranAsam;
    private GameObject panelDiagramAsam;
    private GameObject spatulaAsamDiSamping;
    private readonly List<GameObject> penandaAsam = new List<GameObject>();
    private bool sedangMenakarAsam;

    public void TambahkanSerbukKeSpatula(GameObject spatula)
    {
        if (spatula == null || objekSerbukAsam == null) return;
        Transform ujung = spatula.transform.Find("UjungSerbukAsam");
        if (ujung == null)
        {
            var titik = new GameObject("UjungSerbukAsam");
            ujung = titik.transform;
            ujung.SetParent(spatula.transform, false);
            ujung.localPosition = new Vector3(0f, 0.08f, 0f);
            ujung.localRotation = Quaternion.identity;
        }
        if (ujung.childCount > 0) return;
        var serbuk = Instantiate(objekSerbukAsam, ujung, false);
        serbuk.name = "SerbukAsamBenzoat";
        serbuk.transform.localPosition = Vector3.zero;
        serbuk.transform.localRotation = Quaternion.identity;
        serbuk.transform.localScale = Vector3.one * 0.12f;
        foreach (var c in serbuk.GetComponentsInChildren<Collider>()) Destroy(c);
    }

    public void PilihBahanAsamBenzoat()
    {
        if (fase != FaseP2.IsiKolom1 || platTransform == null || pendingKolomAsam >= 0) return;
        if (spatulaAsamDiSamping == null)
        {
            if (objekSpatulaAsam == null) return;
            spatulaAsamDiSamping = Instantiate(objekSpatulaAsam);
            spatulaAsamDiSamping.name = "Spatula_AsamBenzoat";
            spatulaAsamDiSamping.transform.rotation = platTransform.rotation;
            spatulaAsamDiSamping.SetActive(true);
            TambahkanSerbukKeSpatula(spatulaAsamDiSamping);

            Bounds plateBounds;
            Bounds spoonBounds;
            if (AmbilBoundsRenderer(platTransform.gameObject, out plateBounds) &&
                AmbilBoundsRenderer(spatulaAsamDiSamping, out spoonBounds))
            {
                float lebarPlate = Mathf.Max(plateBounds.size.x, plateBounds.size.y);
                float panjangSendok = Mathf.Max(spoonBounds.size.x, spoonBounds.size.y);
                float skala = panjangSendok > 0.001f ? (lebarPlate * 0.48f / panjangSendok) : 1f;
                spatulaAsamDiSamping.transform.localScale *= skala;
                AmbilBoundsRenderer(spatulaAsamDiSamping, out spoonBounds);
                spatulaAsamDiSamping.transform.position =
                    platTransform.position + platTransform.right *
                    (plateBounds.extents.x + spoonBounds.extents.x * 0.55f + 0.04f) +
                    platTransform.forward * 0.04f;
            }
        }
        Notif("Spatula asam benzoat siap digunakan");
    }

    public GameObject AmbilSpatulaAsamUntukDrag()
    {
        if (fase != FaseP2.IsiKolom1) return null;
        GameObject hasil = spatulaAsamDiSamping;
        spatulaAsamDiSamping = null;
        return hasil;
    }

    private static bool AmbilBoundsRenderer(GameObject target, out Bounds bounds)
    {
        bounds = new Bounds();
        if (target == null) return false;
        bool ada = false;
        foreach (var renderer in target.GetComponentsInChildren<Renderer>())
        {
            if (!ada) { bounds = renderer.bounds; ada = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return ada;
    }

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private bool AdaSoalP2Aktif()
    {
        return (soal1 != null && soal1.activeInHierarchy) ||
               (soal2 != null && soal2.activeInHierarchy) ||
               (soal3 != null && soal3.activeInHierarchy) ||
               (soal4 != null && soal4.activeInHierarchy) ||
               (soal5 != null && soal5.activeInHierarchy) ||
               (soal6 != null && soal6.activeInHierarchy) ||
               (soal7 != null && soal7.activeInHierarchy);
    }

    private void LateUpdate()
    {
        // Toast P2 tidak boleh tertinggal/terpicu saat layar P1 atau menu aktif.
        if (panelNotif != null && panelNotif.activeSelf && !AdaSoalP2Aktif())
            panelNotif.SetActive(false);
    }

    // ======================================================
    // API publik yang dipanggil dari CardTahanGeser3D / ManajerTakaran
    // ======================================================

    /// <summary>Dipanggil saat PlatTetes di-drop. True = diterima.</summary>
    public bool DropPlate(Vector2 posLayar, GameObject model, Camera cam)
    {
        if (fase != FaseP2.Idle) return false;
        if (slotPlate == null || !slotPlate.Mengandung(posLayar, cam)) return false;
        slotPlate.Isi(model, cam, 0.5f);
        platTransform = model.transform;
        InfoModel3D info = model.GetComponent<InfoModel3D>();
        if (info == null) info = model.AddComponent<InfoModel3D>();
        info.skalaAsset = model.transform.localScale;
        info.Perbarui();
        fase = FaseP2.PlateSiap;
        IniGrid();
        return true;
    }

    /// <summary>Dipanggil saat bahan/alat di-drop ke area plate. True = diterima.</summary>
    public bool DropBahan(string namaBahan, Vector2 posLayar, Camera cam, GameObject model = null)
    {
        if (platTransform == null) return false;
        // Cari spot terdekat dengan posisi drop
        int kolom, baris;
        if (!CariSpot(posLayar, cam, out kolom, out baris)) return false;

        switch (fase)
        {
            case FaseP2.IsiKolom1:
                if (namaBahan != "AsamBenzoat" || kolom != 0) { TampilSalah(); return false; }
                if (pendingKolomAsam >= 0) return false;
                pendingKolomAsam = kolom;
                pendingBarisAsam = baris;
                pendingModelAsam = model;
                TampilkanPilihanTakaranAsam();
                return true;
            case FaseP2.IsiKolom2:
                if (namaBahan != "MGCL2" || kolom != 1) { TampilSalah(); return false; }
                TambahIsian(kolom, baris); return true;
            case FaseP2.IsiKolom3:
                if (namaBahan != "PetroleumEter" || kolom != 2) { TampilSalah(); return false; }
                TambahIsian(kolom, baris); return true;
            case FaseP2.CampurBaris1:
                if (namaBahan != "Aquades" || baris != 0) { TampilSalah(); return false; }
                if (animAktif == null) animAktif = StartCoroutine(CampurBaris(0, cam));
                return true;
            case FaseP2.CampurBaris2:
                if (namaBahan != "Etanol" || baris != 1) { TampilSalah(); return false; }
                if (animAktif == null) animAktif = StartCoroutine(CampurBaris(1, cam));
                return true;
            case FaseP2.CampurBaris3:
                if ((namaBahan != "EtanolAquades" && namaBahan != "Aquades") || baris != 2) { TampilSalah(); return false; }
                if (animAktif == null) animAktif = StartCoroutine(CampurBaris(2, cam));
                return true;
        }
        return false;
    }

    private void TampilkanPilihanTakaranAsam()
    {
        if (panelTakaranAsam == null)
        {
            Canvas canvas = panelNotif != null ? panelNotif.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
            if (canvas == null) { PilihTakaranAsam("Satu ujung"); return; }
            panelTakaranAsam = new GameObject("PanelTakaranAsamBenzoat", typeof(RectTransform), typeof(Image));
            panelTakaranAsam.transform.SetParent(canvas.transform, false);
            var rt = panelTakaranAsam.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300f, 190f);
            panelTakaranAsam.GetComponent<Image>().color = new Color(0.04f, 0.09f, 0.18f, 0.97f);
            BuatTeksTakaran(panelTakaranAsam.transform, "Pilih takaran asam benzoat", 17, 12f);
            BuatTombolTakaran(panelTakaranAsam.transform, "Satu ujung", 35f);
            BuatTombolTakaran(panelTakaranAsam.transform, "Setengah spatula", 80f);
            BuatTombolTakaran(panelTakaranAsam.transform, "1 spatula", 125f);
        }
        panelTakaranAsam.SetActive(true);
    }

    private void BuatTeksTakaran(Transform parent, string label, int ukuran, float y)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -y);
        rt.sizeDelta = new Vector2(280f, 35f);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = ukuran;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
    }

    private void BuatTombolTakaran(Transform parent, string label, float y)
    {
        var go = new GameObject("Pilihan_" + label.Replace(" ", ""), typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -y);
        rt.sizeDelta = new Vector2(210f, 38f);
        go.GetComponent<Image>().color = new Color(0.12f, 0.28f, 0.48f, 1f);
        go.GetComponent<Button>().onClick.AddListener(() => PilihTakaranAsam(label));
        BuatTeksTakaran(go.transform, label, 15, 0f);
    }

    public void PilihTakaranAsam(string takaran)
    {
        if (pendingKolomAsam < 0 || sedangMenakarAsam) return;
        if (takaran != "Satu ujung") { TampilSalah(); return; }
        sedangMenakarAsam = true;
        if (panelTakaranAsam != null) panelTakaranAsam.SetActive(false);
        StartCoroutine(AnimasiTakaranAsam());
    }

    private IEnumerator AnimasiTakaranAsam()
    {
        GameObject spatula = pendingModelAsam;
        if (spatula != null)
        {
            Quaternion awal = spatula.transform.rotation;
            Quaternion miring = awal * Quaternion.Euler(0f, 0f, -28f);
            float t = 0f;
            while (t < 0.22f)
            {
                t += Time.deltaTime;
                spatula.transform.rotation = Quaternion.Slerp(awal, miring, Mathf.Clamp01(t / 0.22f));
                yield return null;
            }
            yield return new WaitForSeconds(0.16f);
            t = 0f;
            while (t < 0.22f)
            {
                t += Time.deltaTime;
                spatula.transform.rotation = Quaternion.Slerp(miring, awal, Mathf.Clamp01(t / 0.22f));
                yield return null;
            }
        }
        TambahIsian(pendingKolomAsam, pendingBarisAsam);
        if (pendingModelAsam != null) Destroy(pendingModelAsam);
        pendingModelAsam = null;
        pendingKolomAsam = pendingBarisAsam = -1;
        sedangMenakarAsam = false;
        Notif("Asam benzoat: satu ujung spatula");
    }

    /// <summary>Dipanggil saat TesKonduktor di-drop ke spot. True = diterima.</summary>
    public bool DropTespen(Vector2 posLayar, Camera cam)
    {
        if (fase != FaseP2.CampurBaris3 && fase != FaseP2.Selesai) return false;
        if (!barisSelesai[2]) return false;
        int kolom, baris;
        if (!CariSpot(posLayar, cam, out kolom, out baris) || baris != 2) { TampilSalah(); return false; }
        if (sampelDiuji[kolom, baris]) return false;
        if (animAktif == null) animAktif = StartCoroutine(AnimTespen(kolom, baris, cam));
        return true;
    }

    // ---- Soal gate checks ----
    public bool PlateSudahAda() => fase >= FaseP2.PlateSiap;
    public bool Kolom1Selesai() => kolomSelesai[0];
    public bool Kolom2Selesai() => kolomSelesai[1];
    public bool Kolom3Selesai() => kolomSelesai[2];
    public bool Baris1Selesai() => barisSelesai[0];
    public bool Baris2Selesai() => barisSelesai[1];
    public bool Baris3Selesai()
    {
        if (!barisSelesai[2]) return false;
        for (int k = 0; k < 3; k++) if (!sampelDiuji[k, 2]) return false;
        return true;
    }

    /// <summary>Fase P2 saat ini (Idle = Percobaan 2 belum dimulai).</summary>
    public FaseP2 FaseAktif { get { return fase; } }

    public void AktifkanFase(FaseP2 f)
    {
        fase = f;
        PerbaruiDiagramAsam();
        PerbaruiPenandaAsam();
    }

    private void PerbaruiDiagramAsam()
    {
        bool tampil = fase == FaseP2.IsiKolom1 && diagramPenempatanAsam != null;
        if (panelDiagramAsam == null && tampil)
        {
            Canvas canvas = panelNotif != null ? panelNotif.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
            if (canvas == null) return;
            panelDiagramAsam = new GameObject("DiagramPenempatanAsamBenzoat", typeof(RectTransform), typeof(Image));
            panelDiagramAsam.transform.SetParent(canvas.transform, false);
            var rt = panelDiagramAsam.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(16f, 16f);
            rt.sizeDelta = new Vector2(220f, 172f);
            var image = panelDiagramAsam.GetComponent<Image>();
            image.sprite = diagramPenempatanAsam;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
        if (panelDiagramAsam != null) panelDiagramAsam.SetActive(tampil);
    }

    public void ResetSemua()
    {
        StopAllCoroutines();
        animAktif = null;
        fase = FaseP2.Idle;
        for (int k = 0; k < 3; k++) for (int b = 0; b < 3; b++) { var s = grid[k, b]; if (s != null && s.silinder) Destroy(s.silinder.gameObject); }
        grid = new SpotData[3, 3];
        hitungIsian = new int[3];
        kolomSelesai = new bool[3];
        barisSelesai = new bool[3];
        sampelDiuji = new bool[3, 3];
        platTransform = null;
        foreach (var marker in penandaAsam)
        {
            if (marker == null) continue;
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null) Destroy(renderer.sharedMaterial);
            Destroy(marker);
        }
        penandaAsam.Clear();
        pendingKolomAsam = pendingBarisAsam = -1;
        pendingModelAsam = null;
        if (spatulaAsamDiSamping != null) Destroy(spatulaAsamDiSamping);
        spatulaAsamDiSamping = null;
        sedangMenakarAsam = false;
        if (panelTakaranAsam != null) panelTakaranAsam.SetActive(false);
        if (panelDiagramAsam != null) panelDiagramAsam.SetActive(false);
        if (slotPlate != null) slotPlate.Kosongkan();
        if (panelNotif != null) panelNotif.SetActive(false);
    }

    // ======================================================
    // Internal
    // ======================================================

    void IniGrid()
    {
        for (int k = 0; k < 3; k++) for (int b = 0; b < 3; b++) grid[k, b] = new SpotData();
    }

    private void PerbaruiPenandaAsam()
    {
        bool tampil = fase == FaseP2.IsiKolom1 && platTransform != null;
        if (tampil && penandaAsam.Count == 0)
        {
            for (int b = 0; b < 3; b++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "PenandaAsamBenzoat_Baris" + (b + 1);
                var collider = marker.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                marker.transform.SetParent(platTransform, false);
                marker.transform.localPosition = new Vector3(-0.88f, -0.88f + b * 0.88f, -0.04f);
                marker.transform.localScale = new Vector3(0.44f, 0.018f, 0.44f);
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", new Color(1f, 0.72f, 0.02f, 0.9f));
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
                marker.GetComponent<Renderer>().sharedMaterial = mat;
                penandaAsam.Add(marker);
            }
        }
        for (int b = 0; b < penandaAsam.Count; b++)
        {
            bool belumDiisi = grid[0, b] == null || !grid[0, b].terisi;
            if (penandaAsam[b] != null) penandaAsam[b].SetActive(tampil && belumDiisi);
        }
    }

    void TambahIsian(int kolom, int baris)
    {
        if (grid[kolom, baris].terisi) return;
        hitungIsian[kolom]++;
        TampilSpot(kolom, baris, warnaSampel[kolom]);
        if (kolom == 0) PerbaruiPenandaAsam();
        if (hitungIsian[kolom] >= 3) { kolomSelesai[kolom] = true; Notif("Kolom terisi!"); }
    }

    bool CariSpot(Vector2 posLayar, Camera cam, out int kolom, out int baris)
    {
        kolom = baris = 0;
        if (platTransform == null) return false;
        // Konversi posisi layar ke lokal plate
        Ray ray = cam.ScreenPointToRay(new Vector3(posLayar.x, posLayar.y, 0));
        Plane pl = new Plane(platTransform.forward, platTransform.position);
        float dist; if (!pl.Raycast(ray, out dist)) return false;
        Vector3 hit = platTransform.InverseTransformPoint(ray.GetPoint(dist));
        // Spot grid: kolom di sumbu X, baris di sumbu Y
        float kf = (hit.x + 1.3f) / 0.88f; // offset dari bounds
        float bf = (hit.y + 1.3f) / 0.88f;
        kolom = Mathf.RoundToInt(kf); baris = Mathf.RoundToInt(bf);
        return kolom >= 0 && kolom < 3 && baris >= 0 && baris < 3;
    }

    void TampilSpot(int k, int b, Color warna)
    {
        var s = grid[k, b]; if (s == null) return;
        s.terisi = true;
        if (s.silinder == null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            go.name = "Spot_" + k + b;
            go.transform.SetParent(platTransform, false);
            // Posisi lokal spot di dalam plate (dari ukuran mesh)
            float ox = -0.88f + k * 0.88f;
            float oy = -0.88f + b * 0.88f;
            go.transform.localPosition = new Vector3(ox, oy, -0.12f);
            go.transform.localScale = Vector3.one * 0.32f;
            var mat = new Material(matLarutan != null ? matLarutan : new Material(Shader.Find("Universal Render Pipeline/Lit")));
            go.GetComponent<Renderer>().sharedMaterial = mat;
            s.silinder = go;
            s.mat = mat;
        }
        SetWarna(s.mat, warna);
    }

    static void SetWarna(Material m, Color c)
    {
        if (m == null) return;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color"))     m.SetColor("_Color", c);
    }

    void TampilSalah()
    {
        if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();
    }

    void Notif(string pesan)
    {
        if (panelNotif == null || !AdaSoalP2Aktif()) return;
        if (teksNotif != null) teksNotif.text = pesan;
        panelNotif.SetActive(true);
        StopCoroutine("TutupNotif");
        StartCoroutine(TutupNotif());
    }

    IEnumerator TutupNotif()
    {
        yield return new WaitForSeconds(durasiNotif);
        if (panelNotif != null) panelNotif.SetActive(false);
    }

    static float Ease(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }

    // Animasi tusuk gigi aduk di satu spot
    IEnumerator AnimAduk(int k, int b, GameObject tusuk, Camera cam)
    {
        if (platTransform == null || tusuk == null) yield break;
        var s = grid[k, b]; if (s == null || !s.terisi) yield break;
        float ox = -0.88f + k * 0.88f, oy = -0.88f + b * 0.88f;
        Vector3 centerLok = new Vector3(ox, oy, -0.5f);
        Vector3 centerWld = platTransform.TransformPoint(centerLok);
        Vector3 awal = tusuk.transform.position;
        // terbang ke atas spot
        float t = 0;
        while (t < 0.3f) { t += Time.deltaTime; tusuk.transform.position = Vector3.Lerp(awal, centerWld, Ease(t / 0.3f)); yield return null; }
        // putar 3 kali
        for (int i = 0; i < 3; i++)
        {
            t = 0;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float angle = Ease(t / 0.35f) * 360f;
                float r = 0.18f;
                Vector3 off = platTransform.TransformVector(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * r, Mathf.Sin(angle * Mathf.Deg2Rad) * r, 0));
                tusuk.transform.position = centerWld + off;
                yield return null;
            }
        }
        // kembali
        t = 0;
        while (t < 0.25f) { t += Time.deltaTime; tusuk.transform.position = Vector3.Lerp(centerWld, awal, Ease(t / 0.25f)); yield return null; }
    }

    // Animasi campur satu baris lengkap
    IEnumerator CampurBaris(int baris, Camera cam)
    {
        // Spawn tusuk gigi
        GameObject tusuk = null;
        if (objekTusukGigi != null)
        {
            tusuk = Object.Instantiate(objekTusukGigi);
            InfoModel3D im = tusuk.AddComponent<InfoModel3D>();
            im.skalaAsset = tusuk.transform.localScale;
            if (platTransform != null) tusuk.transform.position = platTransform.position + cam.transform.up * 0.5f;
        }

        for (int k = 0; k < 3; k++)
        {
            // Tampilkan cairan pelarut di spot
            Color warnaPerlarut = baris == 0 ? new Color(0.55f, 0.75f, 1f, 0.6f) :
                                  baris == 1 ? new Color(0.85f, 0.95f, 0.65f, 0.6f) :
                                              new Color(0.70f, 0.85f, 0.82f, 0.6f);
            TampilSpot(k, baris, warnaPerlarut);

            // Animasi partikel tetes kecil (3 partikel)
            if (platTransform != null)
            {
                for (int p = 0; p < 3; p++)
                {
                    yield return new WaitForSeconds(0.08f);
                    StartCoroutine(Tetes(k, baris, warnaPerlarut));
                }
            }
            yield return new WaitForSeconds(0.25f);

            // Aduk
            if (tusuk != null) yield return AnimAduk(k, baris, tusuk, cam);

            // Tampilkan hasil campuran
            Color warnaC = warnaHasil[k, baris];
            TampilSpot(k, baris, warnaC);
            string hasil = hasilKelarutan[k, baris];

            // Efek visual sesuai kelarutan
            if (hasil == "Tidak larut") yield return EfekTidakLarut(k, baris, warnaPerlarut, warnaC);
            else if (hasil == "Sedikit larut") yield return EfekSedikitLarut(k, baris, warnaC);
            // Sangat larut: warna langsung berubah (sudah dilakukan di TampilSpot)

            Notif("Berhasil dicampur: " + hasil);
            yield return new WaitForSeconds(0.5f);
        }

        if (tusuk != null) Destroy(tusuk);
        barisSelesai[baris] = true;
        animAktif = null;
    }

    IEnumerator Tetes(int k, int b, Color warna)
    {
        if (platTransform == null) yield break;
        float ox = -0.88f + k * 0.88f, oy = -0.88f + b * 0.88f;
        Vector3 tujuan = platTransform.TransformPoint(new Vector3(ox, oy, -0.12f));
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.transform.localScale = Vector3.one * 0.08f;
        go.transform.position = tujuan + platTransform.forward * -0.5f;
        var mat = new Material(matLarutan != null ? matLarutan : new Material(Shader.Find("Universal Render Pipeline/Lit")));
        SetWarna(mat, warna);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        float t = 0;
        Vector3 awal = go.transform.position;
        while (t < 0.22f) { t += Time.deltaTime; go.transform.position = Vector3.Lerp(awal, tujuan, Ease(t / 0.22f)); yield return null; }
        Destroy(go);
    }

    IEnumerator EfekTidakLarut(int k, int b, Color warnaAir, Color warnaEter)
    {
        // Lapisan terpisah: spot dibagi dua warna secara vertikal
        var s = grid[k, b];
        if (s == null || s.silinder == null) yield break;
        // Buat silinder kedua (lapisan bawah)
        var go2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go2.GetComponent<Collider>());
        go2.transform.SetParent(platTransform, false);
        go2.transform.localPosition = s.silinder.transform.localPosition + Vector3.up * 0.12f;
        go2.transform.localScale = Vector3.one * 0.28f;
        var mat2 = new Material(matLarutan != null ? matLarutan : new Material(Shader.Find("Universal Render Pipeline/Lit")));
        SetWarna(mat2, warnaAir);
        go2.GetComponent<Renderer>().sharedMaterial = mat2;
        float t = 0;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            float k2 = Ease(t / 0.6f);
            SetWarna(mat2, Color.Lerp(warnaEter, warnaAir, k2));
            go2.transform.localPosition = s.silinder.transform.localPosition + Vector3.up * Mathf.Lerp(0f, 0.15f, k2);
            yield return null;
        }
    }

    IEnumerator EfekSedikitLarut(int k, int b, Color warnaC)
    {
        if (platTransform == null) yield break;
        float ox = -0.88f + k * 0.88f, oy = -0.88f + b * 0.88f;
        Vector3 pusat = platTransform.TransformPoint(new Vector3(ox, oy, -0.12f));
        // Spawn 5 partikel kecil yang mengambang
        var partikel = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(p.GetComponent<Collider>());
            p.transform.position = pusat + Random.insideUnitSphere * 0.1f;
            p.transform.localScale = Vector3.one * 0.05f;
            var m = new Material(matPadatan != null ? matPadatan : new Material(Shader.Find("Universal Render Pipeline/Lit")));
            Color warnaP = warnaC * 0.6f; warnaP.a = 0.9f;
            SetWarna(m, warnaP);
            p.GetComponent<Renderer>().sharedMaterial = m;
            partikel.Add(p);
        }
        float t = 0;
        while (t < 1.2f)
        {
            t += Time.deltaTime;
            foreach (var p in partikel) if (p) p.transform.position += Random.insideUnitSphere * 0.003f;
            yield return null;
        }
        foreach (var p in partikel) if (p) Destroy(p);
    }

    IEnumerator AnimTespen(int k, int b, Camera cam)
    {
        if (objekTespen == null) { animAktif = null; yield break; }
        GameObject tespen = Object.Instantiate(objekTespen);
        if (platTransform != null)
        {
            float ox = -0.88f + k * 0.88f, oy = -0.88f + b * 0.88f;
            Vector3 tujuan = platTransform != null ? platTransform.TransformPoint(new Vector3(ox, oy, -0.5f)) : Vector3.zero;
            Vector3 awal = tespen.transform.position + cam.transform.up;
            float t = 0;
            while (t < 0.4f) { t += Time.deltaTime; tespen.transform.position = Vector3.Lerp(awal, tujuan, Ease(t / 0.4f)); yield return null; }
            yield return new WaitForSeconds(0.5f);
            sampelDiuji[k, b] = true;
            Notif("Berhasil diuji");
        }
        yield return new WaitForSeconds(0.6f);
        Destroy(tespen);
        fase = FaseP2.Selesai;
        animAktif = null;
    }
}
