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

    [Header("Ukuran Model Soal 2")]
    [Range(0.5f, 1.2f)] public float skalaCawanAsamBenzoat = 1.0f;

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
    private Bounds cachedBoundsPlateLokal;
    private bool boundsPlateTersimpan;
    private int pendingKolomAsam = -1;
    private int pendingBarisAsam = -1;
    private GameObject pendingModelAsam;
    private GameObject spatulaTerakhirDiTanda;
    private GameObject panelTakaranAsam;
    private GameObject panelDiagramAsam;
    private GameObject spatulaAsamDiSamping;
    private GameObject cawanAsamBenzoat;
    [SerializeField] private RectTransform markerCawanAsamScene;
    [SerializeField] private RectTransform[] targetDropSpatula = new RectTransform[3];
    [SerializeField] private GameObject panelPilihanSpatula;
    [SerializeField] private Button buttonUjung;
    [SerializeField] private Button buttonSedang;
    [SerializeField] private Button buttonPenuh;
    private GameObject penandaDropCawanAsam;
    private RectTransform rectPenandaDropCawanAsam;
    private bool sumberAsamDiletakkan;
    private readonly List<GameObject> penandaAsam = new List<GameObject>();
    private readonly Dictionary<RectTransform, Vector2> kecepatanPosisiMarker = new Dictionary<RectTransform, Vector2>();
    private bool sedangMenakarAsam;

    public void TambahkanSerbukKeSpatula(GameObject spatula)
    {
        if (spatula == null || objekSerbukAsam == null) return;
        Transform ujung = PastikanUjungSpatula(spatula);
        if (ujung == null) return;
        if (ujung.childCount > 0) return;
        // FBX sumber memuat cawan dan kristal sekaligus; ambil hanya mesh serbuk agar spatula tidak tampak seperti cawan.
        Transform meshSerbuk = objekSerbukAsam.transform.Find("Serbuk_AsamBenzoat");
        GameObject prefabSerbuk = meshSerbuk != null ? meshSerbuk.gameObject : objekSerbukAsam;
        Bounds boundsSpatula;
        bool adaBoundsSpatula = AmbilBoundsRenderer(spatula, out boundsSpatula);
        var serbuk = Instantiate(prefabSerbuk, ujung, false);
        serbuk.name = "SerbukAsamBenzoat";
        serbuk.transform.localPosition = Vector3.zero;
        serbuk.transform.localRotation = Quaternion.identity;
        Bounds boundsSerbuk;
        if (adaBoundsSpatula && AmbilBoundsRenderer(serbuk, out boundsSerbuk))
        {
            float ukuranUjung = Mathf.Max(boundsSpatula.size.x, Mathf.Max(boundsSpatula.size.y, boundsSpatula.size.z)) * 0.14f;
            float ukuranSerbuk = Mathf.Max(boundsSerbuk.size.x, Mathf.Max(boundsSerbuk.size.y, boundsSerbuk.size.z));
            if (ukuranSerbuk > 0.0001f) serbuk.transform.localScale *= ukuranUjung / ukuranSerbuk;
        }
        foreach (var c in serbuk.GetComponentsInChildren<Collider>()) Destroy(c);
    }

    private void TegakkanSpatula(GameObject spatula, Camera kamera)
    {
        if (spatula == null) return;
        if (kamera == null) kamera = Camera.main;
        Vector3 arahAtas = kamera != null ? kamera.transform.up : (platTransform != null ? platTransform.up : Vector3.up);
        Renderer[] renderers = spatula.GetComponentsInChildren<Renderer>(true);
        Renderer terpanjang = null;
        Vector3 sumbuLokal = Vector3.right;
        float panjangTerbesar = 0f;
        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            Vector3 ukuran = r.localBounds.size;
            float panjang = Mathf.Max(ukuran.x, Mathf.Max(ukuran.y, ukuran.z));
            if (panjang <= panjangTerbesar) continue;
            panjangTerbesar = panjang;
            terpanjang = r;
            sumbuLokal = ukuran.x >= ukuran.y && ukuran.x >= ukuran.z ? Vector3.right :
                (ukuran.y >= ukuran.z ? Vector3.up : Vector3.forward);
        }
        if (terpanjang == null || panjangTerbesar <= 0.0001f) return;
        Vector3 sumbuDunia = terpanjang.transform.TransformDirection(sumbuLokal).normalized;
        if (sumbuDunia.sqrMagnitude > 0.5f && arahAtas.sqrMagnitude > 0.5f)
            // Ujung sendok berada pada sisi positif sumbu panjang; arahkan ujung ke bawah agar gagang berdiri ke atas.
            spatula.transform.rotation = Quaternion.FromToRotation(sumbuDunia, -arahAtas) * spatula.transform.rotation;
    }

    private static void TerangkanSpatula(GameObject spatula)
    {
        if (spatula == null) return;
        var block = new MaterialPropertyBlock();
        foreach (Renderer r in spatula.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null) continue;
            r.GetPropertyBlock(block);
            Material mat = r.sharedMaterial;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor")) block.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_Color")) block.SetColor("_Color", Color.white);
                if (mat.HasProperty("_Metallic")) block.SetFloat("_Metallic", 0f);
                if (mat.HasProperty("_Smoothness")) block.SetFloat("_Smoothness", 0.35f);
            }
            r.SetPropertyBlock(block);
        }
    }

    private Transform PastikanUjungSpatula(GameObject spatula)
    {
        if (spatula == null) return null;
        Transform ujung = spatula.transform.Find("UjungSerbukAsam");
        if (ujung == null)
        {
            var titik = new GameObject("UjungSerbukAsam");
            ujung = titik.transform;
            ujung.SetParent(spatula.transform, false);
            Bounds boundsSpatulaLokal = AmbilBoundsLokal(spatula.transform, spatula);
            // Model spatula memanjang pada sumbu X. Letakkan serbuk di ujung cekungnya,
            // bukan di tengah gagang agar gerakan menciduk dan menuang terlihat.
            ujung.localPosition = new Vector3(boundsSpatulaLokal.max.x,
                boundsSpatulaLokal.center.y, boundsSpatulaLokal.center.z);
            ujung.localRotation = Quaternion.identity;
        }
        return ujung;
    }

    public void PilihBahanAsamBenzoat()
    {
        if (fase != FaseP2.IsiKolom1 || platTransform == null || pendingKolomAsam >= 0) return;
        if (spatulaAsamDiSamping == null)
        {
            if (objekSpatulaAsam == null) return;
            spatulaAsamDiSamping = Instantiate(objekSpatulaAsam);
            spatulaAsamDiSamping.name = "Spatula_AsamBenzoat";
            spatulaAsamDiSamping.transform.rotation = Camera.main != null ? Camera.main.transform.rotation : platTransform.rotation;
            TegakkanSpatula(spatulaAsamDiSamping, Camera.main);
            TerangkanSpatula(spatulaAsamDiSamping);
            spatulaAsamDiSamping.SetActive(true);
            

            Bounds plateBounds;
            Bounds spoonBounds;
            if (AmbilBoundsRenderer(platTransform.gameObject, out plateBounds) &&
                AmbilBoundsRenderer(spatulaAsamDiSamping, out spoonBounds))
            {
                float lebarPlate = Mathf.Max(plateBounds.size.x, plateBounds.size.y);
                float panjangSendok = Mathf.Max(spoonBounds.size.x, spoonBounds.size.y);
                float skala = panjangSendok > 0.001f ? (lebarPlate * 0.055f / panjangSendok) : 1f;
                spatulaAsamDiSamping.transform.localScale *= skala;
                AmbilBoundsRenderer(spatulaAsamDiSamping, out spoonBounds);
                spatulaAsamDiSamping.transform.position =
                    platTransform.position + platTransform.right *
                    (plateBounds.extents.x + spoonBounds.extents.x * 0.55f + 0.04f) +
                    platTransform.forward * 0.04f;
            }
        }

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

    private void Awake()
    {
        Instance = this;
        if (markerCawanAsamScene != null)
        {
            rectPenandaDropCawanAsam = markerCawanAsamScene;
            penandaDropCawanAsam = markerCawanAsamScene.gameObject;
            penandaDropCawanAsam.SetActive(false);
        }
        if (panelPilihanSpatula != null) panelPilihanSpatula.SetActive(false);
        if (buttonUjung != null) { buttonUjung.onClick.AddListener(() => PilihTakaranAsam("Satu ujung")); AturLabelTakaran(buttonUjung, "Satu ujung"); }
        if (buttonSedang != null) { buttonSedang.onClick.AddListener(() => PilihTakaranAsam("Setengah spatula")); AturLabelTakaran(buttonSedang, "Setengah spatula"); }
        if (buttonPenuh != null) { buttonPenuh.onClick.AddListener(() => PilihTakaranAsam("1 spatula")); AturLabelTakaran(buttonPenuh, "1 spatula"); }
        PindahkanTargetSpatulaKeCanvas();
        PerbaruiTargetDropSpatula();
    }
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
        // Jika panel Soal 2 P2 aktif setelah plate diletakkan, pulihkan fase kolom pertama
        // meskipun callback tombol navigasi hanya mengubah panel UI.
        if (soal2 != null && soal2.activeInHierarchy && platTransform != null && fase != FaseP2.IsiKolom1 && !kolomSelesai[0])
            AktifkanFase(FaseP2.IsiKolom1);

        // Toast P2 tidak boleh tertinggal/terpicu saat layar P1 atau menu aktif.
        if (panelNotif != null && panelNotif.activeSelf && !AdaSoalP2Aktif())
            panelNotif.SetActive(false);
        // Fase dapat aktif sebelum plate selesai diinisialisasi; pastikan ghost target dibuat begitu plate tersedia.
        if (fase == FaseP2.IsiKolom1 && platTransform != null) { PerbaruiPenandaAsam(); PerbaruiPenandaDropCawan(); }
        PerbaruiTargetDropSpatula();
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
        boundsPlateTersimpan = false;
        cachedBoundsPlateLokal = AmbilBoundsPlateLokal();
        boundsPlateTersimpan = true;
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
        if (fase == FaseP2.IsiKolom1 && namaBahan == "Spatula") return DropSpatulaKeTarget(posLayar, cam, model);
        // Cari spot terdekat dengan posisi drop
        int kolom, baris;
        if (!CariSpot(posLayar, cam, out kolom, out baris)) return false;

        switch (fase)
        {
            case FaseP2.IsiKolom1:
                if (namaBahan != "Spatula" || !sumberAsamDiletakkan || cawanAsamBenzoat == null || kolom != 0) { TampilSalah(); return false; }
                if (pendingKolomAsam >= 0 || grid[0, baris].terisi) return false;
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

    private Vector3 PosisiTargetDropSpatula(int urutanSamping)
    {
        // Asam benzoat berada di kolom pertama; target bergerak turun per baris.
        return PosisiLubangSpot(0, urutanSamping, 0.001f);
    }

    private bool DropSpatulaKeTarget(Vector2 posisiLayar, Camera cam, GameObject model)
    {
        if (!sumberAsamDiletakkan || cawanAsamBenzoat == null || model == null ||
            sedangMenakarAsam || pendingKolomAsam >= 0 || hitungIsian[0] >= 3)
            return false;

        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        int targetIndex = hitungIsian[0];
        RectTransform target = targetDropSpatula != null && targetIndex < targetDropSpatula.Length
            ? targetDropSpatula[targetIndex] : null;
        if (target == null) { TampilSalah(); return false; }
        Canvas canvasTarget = target.GetComponentInParent<Canvas>(true);
        Camera cameraUI = canvasTarget != null && canvasTarget.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvasTarget.rootCanvas.worldCamera : null;
        Vector2 pusatTarget = RectTransformUtility.WorldToScreenPoint(cameraUI, target.position);
        Vector3[] sudutTarget = new Vector3[4];
        target.GetWorldCorners(sudutTarget);
        float radiusTarget = 0f;
        for (int i = 0; i < sudutTarget.Length; i++)
        {
            Vector2 titik = RectTransformUtility.WorldToScreenPoint(cameraUI, sudutTarget[i]);
            radiusTarget = Mathf.Max(radiusTarget, Vector2.Distance(pusatTarget, titik));
        }
        bool kenaTarget = RectTransformUtility.RectangleContainsScreenPoint(target, posisiLayar, cameraUI) ||
            Vector2.Distance(posisiLayar, pusatTarget) <= Mathf.Max(48f, radiusTarget + 12f);
        if (!kenaTarget)
        { TampilSalah(); return false; }

        // Buat titik ujung terlebih dulu supaya animasi bisa menghitung lintasan
        // ujung spatula meskipun spatula baru saja diambil dari menu alat.
        model.SetActive(true);
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true)) renderer.enabled = true;
        Transform holder = model.transform.Find("UjungSerbukAsam");
        if (holder != null)
            for (int i = holder.childCount - 1; i >= 0; i--) Destroy(holder.GetChild(i).gameObject);
        model.transform.SetParent(platTransform, true);
        PosisikanSpatulaDiCekungan(model, targetIndex);
        pendingKolomAsam = 0;
        pendingBarisAsam = targetIndex;
        pendingModelAsam = model;
        PerbaruiTargetDropSpatula();
        TampilkanPilihanTakaranAsam();
        return true;
    }

    private void PosisikanSpatulaDiCekungan(GameObject model, int baris)
    {
        if (model == null || platTransform == null) return;
        // Pertahankan bidang model pada permukaan plate, lalu putar di bidang layar
        // agar batang spatula mengarah tegak ke atas (sumbu layar Y).
        // FBX spatula berada mendatar pada sumbu X. Putar pada bidang layar
        // agar gagang mengarah tegak ke atas, sama dengan spatula sebelum di-drop.
        Camera kamera = Camera.main;
        model.transform.rotation = kamera != null ? kamera.transform.rotation : platTransform.rotation;
        TegakkanSpatula(model, kamera);
        TerangkanSpatula(model);
        // Jangan skala ulang setelah drag: ukuran harus tetap sama dengan yang
        // terlihat ketika spatula diambil dari kartu alat.
        Transform tip = PastikanUjungSpatula(model);
        Bounds boundsPlateWorld;
        float jarakDiAtasPermukaan = AmbilBoundsRenderer(platTransform.gameObject, out boundsPlateWorld)
            ? Mathf.Clamp(boundsPlateWorld.extents.magnitude * 0.04f, 0.006f, 0.02f)
            : 0.01f;
        // Pakai offset dunia beberapa milimeter supaya spatula tidak tertutup mesh plate
        // saat model AR memiliki skala kecil.
        Vector3 targetTip = PosisiLubangSpot(0, baris, 0f) + platTransform.up * jarakDiAtasPermukaan;
        model.transform.position += targetTip - tip.position;
    }

    private static Bounds AmbilBoundsLokal(Transform referensi, GameObject model)
    {
        Bounds hasil = new Bounds(Vector3.zero, Vector3.zero);
        bool ada = false;
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            Bounds world = renderer.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = new Vector3((i & 1) == 0 ? world.min.x : world.max.x,
                    (i & 2) == 0 ? world.min.y : world.max.y,
                    (i & 4) == 0 ? world.min.z : world.max.z);
                p = referensi.InverseTransformPoint(p);
                if (!ada) { hasil = new Bounds(p, Vector3.zero); ada = true; }
                else hasil.Encapsulate(p);
            }
        }
        return hasil;
    }

    private static void AturLabelTakaran(Button button, string label)
    {
        TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) tmp.text = label;
        Text text = button.GetComponentInChildren<Text>(true);
        if (text != null) text.text = label;
    }

    private void PerbaruiTargetDropSpatula()
    {
        bool soalAktif = soal2 != null && soal2.activeInHierarchy;
        bool sumberSiap = fase == FaseP2.IsiKolom1 && soalAktif &&
            sumberAsamDiletakkan && cawanAsamBenzoat != null && hitungIsian[0] < 3;
        if (targetDropSpatula != null)
        {
            // Tiga marker mengikuti tepat tiga titik sampel pada permukaan plat tetes.
            for (int i = 0; i < targetDropSpatula.Length; i++)
                if (targetDropSpatula[i] != null)
                {
                    RectTransform marker = targetDropSpatula[i];
                    if (sumberSiap)
                    {
                        marker.sizeDelta = new Vector2(54f, 54f);
                        PosisikanMarkerDiLubang(marker, 0, i, 0.001f);
                    }
                    bool targetBerikutnya = pendingKolomAsam < 0 &&
                        i == hitungIsian[0] && (grid[0, i] == null || !grid[0, i].terisi);
                    marker.gameObject.SetActive(sumberSiap && targetBerikutnya);
                }
        }
        if (panelPilihanSpatula != null)
            panelPilihanSpatula.SetActive(sumberSiap && pendingKolomAsam >= 0 && !sedangMenakarAsam);
    }

    private void PindahkanTargetSpatulaKeCanvas()
    {
        Canvas canvas = panelNotif != null
            ? panelNotif.GetComponentInParent<Canvas>(true)
            : FindObjectOfType<Canvas>();
        if (canvas == null || targetDropSpatula == null) return;
        for (int i = 0; i < targetDropSpatula.Length; i++)
        {
            RectTransform marker = targetDropSpatula[i];
            if (marker == null) continue;
            if (marker.parent != canvas.transform)
                marker.SetParent(canvas.transform, false);
            marker.anchorMin = marker.anchorMax = new Vector2(0.5f, 0.5f);
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.localScale = Vector3.one;
            marker.localRotation = Quaternion.identity;
        }
    }

    private void PosisikanMarkerDiLubang(RectTransform marker, int kolom, int baris, float offsetNormal)
    {
        Camera cam = Camera.main;
        if (cam == null || marker == null || marker.parent == null) return;
        Canvas canvas = marker.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        Vector3 world = PosisiLubangSpot(kolom, baris, offsetNormal);
        Vector3 screen = cam.WorldToScreenPoint(world);
        if (screen.z <= 0f) return;
        RectTransform parent = marker.parent as RectTransform;
        Camera uiCam = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
        Vector2 local;
        if (parent != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCam, out local))
        {
            // Marker sudah berada di root Canvas dengan anchor tengah, jadi koordinat lokal langsung dipakai.
            if (!marker.gameObject.activeInHierarchy)
            {
                marker.anchoredPosition = local;
                kecepatanPosisiMarker[marker] = Vector2.zero;
            }
            else
            {
                Vector2 velocity;
                kecepatanPosisiMarker.TryGetValue(marker, out velocity);
                marker.anchoredPosition = Vector2.SmoothDamp(marker.anchoredPosition, local, ref velocity, 0.06f, Mathf.Infinity, Time.unscaledDeltaTime);
                kecepatanPosisiMarker[marker] = velocity;
            }
        }

    }

    private void TampilkanPilihanTakaranAsam()
    {
        if (panelPilihanSpatula != null) { panelPilihanSpatula.SetActive(true); PerbaruiTargetDropSpatula(); return; }
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
        if (takaran != "Ujung" && takaran != "Satu ujung") { TampilSalah(); return; }
        sedangMenakarAsam = true;
        if (panelTakaranAsam != null) panelTakaranAsam.SetActive(false);
        if (panelPilihanSpatula != null) panelPilihanSpatula.SetActive(false);
        PerbaruiTargetDropSpatula();
        StartCoroutine(AnimasiTakaranAsam());
    }

    private IEnumerator GerakkanSpatula(GameObject spatula, Vector3 posisiTujuan, Quaternion rotasiTujuan, float durasi)
    {
        if (spatula == null) yield break;
        Vector3 posisiAwal = spatula.transform.position;
        Quaternion rotasiAwal = spatula.transform.rotation;
        float waktu = 0f;
        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            float t = Ease(waktu / durasi);
            spatula.transform.position = Vector3.Lerp(posisiAwal, posisiTujuan, t);
            spatula.transform.rotation = Quaternion.Slerp(rotasiAwal, rotasiTujuan, t);
            yield return null;
        }
        spatula.transform.position = posisiTujuan;
        spatula.transform.rotation = rotasiTujuan;
    }

    private IEnumerator AnimasiTakaranAsam()
    {
        GameObject spatula = pendingModelAsam;
        int baris = pendingBarisAsam;
        Vector3 lubang = PosisiLubangSpot(0, baris, 0.001f);
        Vector3 awal = spatula != null ? spatula.transform.position : lubang;
        Quaternion rotAwal = spatula != null ? spatula.transform.rotation : Quaternion.identity;
        // Jaga spatula tetap mendatar; gerak mengambil dan meletakkan dilakukan dari atas.
        Quaternion rotAmbil = Quaternion.AngleAxis(-8f, platTransform.right) * rotAwal;
        Quaternion rotTuang = Quaternion.AngleAxis(8f, platTransform.right) * rotAwal;
        Transform ujung = spatula != null ? PastikanUjungSpatula(spatula) : null;
        Vector3 offsetUjungLokal = spatula != null && ujung != null
            ? Quaternion.Inverse(rotAwal) * (ujung.position - spatula.transform.position) : Vector3.zero;
        Vector3 posisiAmbil = awal;
        Vector3 posisiScoopDalam = awal;
        Vector3 posisiScoopGeser = awal;
        Vector3 posisiScoopAngkat = awal;
        Vector3 posisiTuang = lubang;

        Bounds cawanBounds;
        if (spatula != null && ujung != null && cawanAsamBenzoat != null &&
            AmbilBoundsRenderer(cawanAsamBenzoat, out cawanBounds))
        {
            Transform arahCawan = cawanAsamBenzoat.transform;
            float tinggiCawan = Mathf.Abs(arahCawan.up.x) * cawanBounds.extents.x +
                Mathf.Abs(arahCawan.up.y) * cawanBounds.extents.y +
                Mathf.Abs(arahCawan.up.z) * cawanBounds.extents.z;
            float lebarCawan = Mathf.Abs(arahCawan.right.x) * cawanBounds.extents.x +
                Mathf.Abs(arahCawan.right.y) * cawanBounds.extents.y +
                Mathf.Abs(arahCawan.right.z) * cawanBounds.extents.z;
            // Datang dari atas cawan, turun ke permukaan serbuk, menyendok ke samping,
            // lalu terangkat kembali tanpa menembus wadah dari sisi bawah.
            Vector3 titikMulaiSendok = cawanBounds.center + arahCawan.up * (tinggiCawan * 1.45f);
            Vector3 titikDalamCawan = cawanBounds.center + arahCawan.up * (tinggiCawan * 0.18f)
                - arahCawan.right * (lebarCawan * 0.18f);
            Vector3 titikSerok = titikDalamCawan + arahCawan.right * (lebarCawan * 0.34f);
            Vector3 titikAngkat = titikSerok + arahCawan.up * (tinggiCawan * 0.72f);
            posisiAmbil = titikMulaiSendok - rotAmbil * offsetUjungLokal;
            posisiScoopDalam = titikDalamCawan - rotAmbil * offsetUjungLokal;
            posisiScoopGeser = titikSerok - rotAmbil * offsetUjungLokal;
            posisiScoopAngkat = titikAngkat - rotAmbil * offsetUjungLokal;
            // Ujung sendok berhenti tepat di dalam cawan, kemudian menyapu serbuk
            // melintasi cekungan sebelum diangkat dan dibawa ke lubang bertanda.
            Vector3 targetLubang = lubang + platTransform.up * 0.006f;
            posisiTuang = targetLubang - rotTuang * offsetUjungLokal;
        }

        if (spatula != null)
        {
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiAmbil, rotAmbil, 0.55f));
            // Turunkan ujung sendok masuk ke serbuk, sapu melintasi cawan, lalu angkat.
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopDalam, rotAmbil, 0.24f));
            // Serbuk langsung terlihat menempel ketika ujung menyentuh bahan,
            // lalu ikut tersapu dan terangkat bersama spatula.
            TambahkanSerbukKeSpatula(spatula);
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopGeser, rotAmbil, 0.34f));
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopAngkat, rotAmbil, 0.28f));
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiTuang, rotTuang, 0.72f));
        }

        ujung = spatula != null ? spatula.transform.Find("UjungSerbukAsam") : null;
        Vector3 ke = PosisiLubangSpot(0, baris, 0f) + platTransform.up * 0.004f;
        Transform massaSerbuk = ujung != null ? ujung.Find("SerbukAsamBenzoat") : null;
        if (massaSerbuk != null) massaSerbuk.SetParent(null, true);
        // Pindahkan satu mesh serbuk yang menempel di ujung spatula ke lubang.
        if (massaSerbuk != null)
        {
            Vector3 awalMassa = massaSerbuk.position;
            for (int frame = 0; frame < 30; frame++)
            {
                float k = Ease(frame / 29f);
                massaSerbuk.position = Vector3.Lerp(awalMassa, ke, k);
                yield return null;
            }
            massaSerbuk.SetParent(platTransform, true);
            massaSerbuk.position = ke;
        }

        if (spatula != null)
        {
            float t = 0f;
            while (t < 0.42f)
            {
                t += Time.deltaTime;
                spatula.transform.rotation = Quaternion.Slerp(rotTuang, rotAwal, Ease(t / 0.42f));
                yield return null;
            }
            // Biarkan alat tetap terlihat di tanda setelah dosis selesai.
            spatula.transform.SetParent(platTransform, true);
            spatulaTerakhirDiTanda = spatula;
        }

        TambahIsian(0, baris);
        pendingModelAsam = null;
        pendingKolomAsam = pendingBarisAsam = -1;
        sedangMenakarAsam = false;
        PerbaruiTargetDropSpatula();

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

    public bool AturanKartuSoal2Aktif { get { return fase == FaseP2.IsiKolom1; } }

    public bool BolehAmbilKartuSoal2(GameObject item)
    {
        if (!AturanKartuSoal2Aktif || item == null) return false;
        if (item.name == "Asam Benzoat") return !sumberAsamDiletakkan && cawanAsamBenzoat == null;
        if (item.name == "Spatula") return BolehAmbilSpatulaAsam();
        return false;
    }

    public bool BolehAmbilKartuPlateTetes(GameObject item)
    {
        if (fase != FaseP2.Idle || soal1 == null || !soal1.activeInHierarchy || item == null) return false;
        CardTahanGeser3D kartu = item.GetComponent<CardTahanGeser3D>();
        return kartu != null && kartu.namaBahanSP == "PlateTetes";
    }

    public void AktifkanFase(FaseP2 f)
    {
        if(f==FaseP2.IsiKolom1&&fase!=f){sumberAsamDiletakkan=false;if(cawanAsamBenzoat!=null){Destroy(cawanAsamBenzoat);cawanAsamBenzoat=null;}}
        fase = f;
        PerbaruiDiagramAsam();
        PerbaruiPenandaAsam();
        PerbaruiCawanAsam();
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
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(140f, -62f);
            rt.sizeDelta = new Vector2(122f, 92f);
            var image = panelDiagramAsam.GetComponent<Image>();
            image.sprite = diagramPenempatanAsam;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
        if (panelDiagramAsam != null) panelDiagramAsam.SetActive(tampil);
    }

    private void SembunyikanMarkerDropCawan()
    {
        if (penandaDropCawanAsam != null && (markerCawanAsamScene == null || penandaDropCawanAsam != markerCawanAsamScene.gameObject)) Destroy(penandaDropCawanAsam);
        penandaDropCawanAsam = markerCawanAsamScene != null ? markerCawanAsamScene.gameObject : null;
        rectPenandaDropCawanAsam = markerCawanAsamScene;
        if (penandaDropCawanAsam != null) penandaDropCawanAsam.SetActive(false);
    }

    private void PerbaruiCawanAsam()
    {
        bool tampil = fase == FaseP2.IsiKolom1 && platTransform != null; if (!tampil) { SembunyikanMarkerDropCawan(); if (cawanAsamBenzoat != null) { Destroy(cawanAsamBenzoat); cawanAsamBenzoat = null; } sumberAsamDiletakkan = false; return; } PerbaruiPenandaDropCawan();
    }
    private void PerbaruiPenandaDropCawan()
    {
        if (fase != FaseP2.IsiKolom1 || platTransform == null || sumberAsamDiletakkan ||
            (soal2 != null && !soal2.activeInHierarchy))
        {
            if (penandaDropCawanAsam != null) penandaDropCawanAsam.SetActive(false);
            return;
        }

        Canvas cv = markerCawanAsamScene != null
            ? markerCawanAsamScene.GetComponentInParent<Canvas>(true)
            : (panelNotif != null ? panelNotif.GetComponentInParent<Canvas>(true) : FindObjectOfType<Canvas>());
        if (cv == null) cv = FindObjectOfType<Canvas>();
        if (cv == null) return;

        if (markerCawanAsamScene == null)
        {
            Transform target = cv.transform.Find("TargetDropAsamBenzoat");
            if (target != null) markerCawanAsamScene = target as RectTransform;
        }
        if (markerCawanAsamScene == null)
        {
            var target = new GameObject("TargetDropAsamBenzoat", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            target.transform.SetParent(cv.transform, false);
            markerCawanAsamScene = target.GetComponent<RectTransform>();
            markerCawanAsamScene.sizeDelta = new Vector2(100f, 100f);
        }

        rectPenandaDropCawanAsam = markerCawanAsamScene;
        penandaDropCawanAsam = markerCawanAsamScene.gameObject;
        if (penandaDropCawanAsam.transform.parent != cv.transform)
            penandaDropCawanAsam.transform.SetParent(cv.transform, true);

        Image imageMarker = penandaDropCawanAsam.GetComponent<Image>();
        if (imageMarker == null)
        {
            Transform visual = penandaDropCawanAsam.transform.Find("VisualTargetDrop");
            if (visual == null)
            {
                var visualObject = new GameObject("VisualTargetDrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                visualObject.transform.SetParent(penandaDropCawanAsam.transform, false);
                visualObject.transform.SetAsFirstSibling();
                var visualRect = visualObject.GetComponent<RectTransform>();
                visualRect.anchorMin = Vector2.zero;
                visualRect.anchorMax = Vector2.one;
                visualRect.offsetMin = Vector2.zero;
                visualRect.offsetMax = Vector2.zero;
                visual = visualObject.transform;
            }
            imageMarker = visual.GetComponent<Image>();
        }
        imageMarker.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        imageMarker.type = Image.Type.Simple;
        imageMarker.raycastTarget = false;
        imageMarker.color = new Color(0.72f, 0.75f, 0.78f, sumberAsamDiletakkan ? 0.32f : 0.68f);

        penandaDropCawanAsam.SetActive(true);
        penandaDropCawanAsam.transform.SetAsLastSibling();
    }

    public bool BolehAmbilSpatulaAsam(){return fase==FaseP2.IsiKolom1&&sumberAsamDiletakkan&&cawanAsamBenzoat!=null&&pendingKolomAsam<0&&!sedangMenakarAsam&&hitungIsian[0]<3;}
    public bool DropSumberAsam(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.IsiKolom1 || platTransform == null || sumberAsamDiletakkan || model == null) return false;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Canvas cv = rectPenandaDropCawanAsam != null ? rectPenandaDropCawanAsam.GetComponentInParent<Canvas>() : null;
        Camera uiCam = cv != null && cv.renderMode != RenderMode.ScreenSpaceOverlay ? cv.worldCamera : null;
        bool tepatPadaMarker = rectPenandaDropCawanAsam != null &&
            RectTransformUtility.RectangleContainsScreenPoint(rectPenandaDropCawanAsam, pos, uiCam);
        
        if (!tepatPadaMarker) return false;

        float d = cam.WorldToScreenPoint(platTransform.position).z;
        if (d <= cam.nearClipPlane) d = 1f;

        Vector2 titikTengah = cam.WorldToScreenPoint(platTransform.position);
        float targetWidth = 0f;
        float targetHeight = 0f;
        if (rectPenandaDropCawanAsam != null)
        {
            Vector3[] corners = new Vector3[4];
            rectPenandaDropCawanAsam.GetWorldCorners(corners);
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
                min = Vector2.Min(min, screen);
                max = Vector2.Max(max, screen);
            }
            titikTengah = (min + max) * 0.5f;
            Vector3 bawahKiri = cam.ScreenToWorldPoint(new Vector3(min.x, min.y, d));
            Vector3 atasKanan = cam.ScreenToWorldPoint(new Vector3(max.x, max.y, d));
            Vector3 ukuranTarget = atasKanan - bawahKiri;
            targetWidth = Mathf.Abs(Vector3.Dot(ukuranTarget, cam.transform.right));
            targetHeight = Mathf.Abs(Vector3.Dot(ukuranTarget, cam.transform.up));
        }
        else
        {
            targetWidth = targetHeight = 0.12f;
        }

        Vector3 target = cam.ScreenToWorldPoint(new Vector3(titikTengah.x, titikTengah.y, d));
        // Hadapkan sisi depan model ke kamera; jangan diputar 90 derajat.
        model.transform.rotation = cam.transform.rotation;
        Vector3 center;
        Vector2 size;
        if (CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out center, out size) &&
            size.x > 0.001f && size.y > 0.001f)
        {
            float faktor = Mathf.Min(targetWidth / size.x, targetHeight / size.y) * skalaCawanAsamBenzoat;
            model.transform.localScale *= faktor;
            CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out center, out size);
        }
        model.transform.position = target - cam.transform.TransformVector(center);
        // Parent to plate while preserving the fitted pose and the target placement.
        model.transform.SetParent(platTransform, true);
        PusatkanRendererKeLayar(model, cam, titikTengah, d);
        foreach (var x in model.GetComponentsInChildren<Collider>(true)) Destroy(x);
        foreach (var x in model.GetComponentsInChildren<Camera>(true)) Destroy(x);
        foreach (var x in model.GetComponentsInChildren<Light>(true)) Destroy(x);
        model.name = "CawanAsamBenzoat_Sumber";
        cawanAsamBenzoat = model;
        sumberAsamDiletakkan = true;
        PerbaruiTargetDropSpatula();

        return true;
    }

    private void PusatkanRendererKeLayar(GameObject model, Camera cam, Vector2 targetScreen, float depth)
    {
        if (model == null || cam == null) return;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        bool ada = false;
        Bounds bounds = new Bounds();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled) continue;
            if (!ada) { bounds = renderer.bounds; ada = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        if (!ada) return;
        Vector3 currentScreen = cam.WorldToScreenPoint(bounds.center);
        if (currentScreen.z <= cam.nearClipPlane) return;
        Vector3 currentWorld = cam.ScreenToWorldPoint(new Vector3(currentScreen.x, currentScreen.y, currentScreen.z));
        Vector3 targetWorld = cam.ScreenToWorldPoint(new Vector3(targetScreen.x, targetScreen.y, depth));
        model.transform.position += targetWorld - currentWorld;
    }

    private bool PosisiLayarDiAtasPlat(Vector2 posisi, Camera cam)
    {
        Bounds bounds;
        if (!AmbilBoundsRenderer(platTransform.gameObject, out bounds)) return false;
        Vector3 pusat = cam.WorldToScreenPoint(bounds.center);
        if (pusat.z <= 0f) return false;

        Vector3 kanan = cam.transform.right;
        Vector3 atas = cam.transform.up;
        float setengahLebarDunia = Mathf.Abs(kanan.x) * bounds.extents.x +
            Mathf.Abs(kanan.y) * bounds.extents.y + Mathf.Abs(kanan.z) * bounds.extents.z;
        float setengahTinggiDunia = Mathf.Abs(atas.x) * bounds.extents.x +
            Mathf.Abs(atas.y) * bounds.extents.y + Mathf.Abs(atas.z) * bounds.extents.z;
        float setengahLebarLayar = Mathf.Abs(cam.WorldToScreenPoint(bounds.center + kanan * setengahLebarDunia).x - pusat.x);
        float setengahTinggiLayar = Mathf.Abs(cam.WorldToScreenPoint(bounds.center + atas * setengahTinggiDunia).y - pusat.y);
        Rect area = new Rect(pusat.x - setengahLebarLayar, pusat.y - setengahTinggiLayar,
            setengahLebarLayar * 2f, setengahTinggiLayar * 2f);
        area.xMin -= 24f;
        area.yMin -= 24f;
        area.xMax += 24f;
        area.yMax += 24f;
        return area.Contains(posisi);
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
        if (cawanAsamBenzoat != null) { Destroy(cawanAsamBenzoat); cawanAsamBenzoat = null; }
        sumberAsamDiletakkan = false; SembunyikanMarkerDropCawan(); kecepatanPosisiMarker.Clear();
        if (spatulaAsamDiSamping != null) { Destroy(spatulaAsamDiSamping); spatulaAsamDiSamping = null; }
        if (spatulaTerakhirDiTanda != null) { Destroy(spatulaTerakhirDiTanda); spatulaTerakhirDiTanda = null; }
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
        // RectTransform target di Canvas sudah menjadi satu-satunya penanda yang
        // terlihat dan dapat di-drop. Jangan gambar kubus dunia di atasnya (double marker).
        if (targetDropSpatula != null && targetDropSpatula.Length > 0)
        {
            foreach (var markerLama in penandaAsam)
                if (markerLama != null) markerLama.SetActive(false);
            return;
        }
        if (tampil && (penandaAsam.Count != 3 || penandaAsam.Exists(m => m == null)))
        {
            foreach (var markerLama in penandaAsam) if (markerLama != null) Destroy(markerLama);
            penandaAsam.Clear();
        }
        if (tampil && penandaAsam.Count == 0)
        {
            for (int b = 0; b < 3; b++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "PenandaAsamBenzoat_Baris" + (b + 1);
                var collider = marker.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                marker.transform.SetParent(platTransform, false);
                marker.transform.localPosition = KoordinatSpotLokal(0, b);
                marker.transform.localRotation = Quaternion.identity;
                // Marker persegi mengikuti ukuran satu lubang pada grid CariSpot.
                float ukuranMarker = Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.z) * 0.30f;
                marker.transform.localScale = new Vector3(ukuranMarker, 0.008f, ukuranMarker);
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Standard");
                var mat = new Material(shader);
                Color markerColor = new Color(0.82f, 0.86f, 0.9f, 0.68f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", markerColor);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", markerColor);
                if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
                if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
                if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
                marker.GetComponent<Renderer>().sharedMaterial = mat;
                penandaAsam.Add(marker);
            }
        }
        for (int b = 0; b < penandaAsam.Count; b++)
        {
            bool belumDiisi = grid[0, b] == null || !grid[0, b].terisi;
            bool targetBerikutnya = pendingKolomAsam < 0 && b == hitungIsian[0];
            if (penandaAsam[b] != null) penandaAsam[b].SetActive(tampil && belumDiisi && targetBerikutnya);
        }
    }

    void TambahIsian(int kolom, int baris)
    {
        if (grid[kolom, baris].terisi) return;
        hitungIsian[kolom]++;
        if (kolom == 0)
        {
            // Tempatkan tiga dosis pada kolom asam yang sama, dari baris atas ke bawah.
            TampilSpot(0, baris, warnaSampel[kolom]);
            PerbaruiPenandaAsam();
        }
        else TampilSpot(kolom, baris, warnaSampel[kolom]);
        if (hitungIsian[kolom] >= 3) { kolomSelesai[kolom] = true; if (kolom != 0) Notif("Kolom terisi!"); }
    }

    private const float JarakGridSpot = 0.88f;

    private Bounds AmbilBoundsPlateLokal()
    {
        if (boundsPlateTersimpan) return cachedBoundsPlateLokal;
        Bounds lokal = new Bounds(Vector3.zero, Vector3.one);
        bool ada = false;
        if (platTransform == null) return lokal;
        foreach (Renderer r in platTransform.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 p = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                p = platTransform.InverseTransformPoint(p);
                if (!ada) { lokal = new Bounds(p, Vector3.zero); ada = true; } else lokal.Encapsulate(p);
            }
        }
        return lokal;
    }

    private Vector3 PosisiLubangSpot(int kolom, int baris, float offsetNormal)
    {
        Vector3 local = KoordinatSpotLokal(kolom, baris);
        local.y += offsetNormal;
        return platTransform.TransformPoint(local);
    }
    private const float KedalamanPenandaSpot = -0.14f;

    private Vector3 KoordinatSpotLokal(int kolom, int baris)
    {
        Bounds b = AmbilBoundsPlateLokal();
        float x = b.center.x + (kolom - 1) * b.extents.x * 0.58f;
        // Plate fisik memiliki empat baris; praktikum memakai tiga baris atas
        // dan menyisakan baris paling bawah.
        float z = b.center.z + (1 - baris) * b.extents.z * 0.43f;
        // Mesh plate tipis pada sumbu Y. Permukaannya berada di sisi Y maksimum;
        // X dan Z membentuk bidang lubang-lubang plate tetes.
        return new Vector3(x, b.max.y + 0.001f, z);
    }

    bool CariSpot(Vector2 posLayar, Camera cam, out int kolom, out int baris)
    {
        kolom = baris = 0;
        if (platTransform == null) return false;
        // Konversi posisi layar ke lokal plate
        Ray ray = cam.ScreenPointToRay(new Vector3(posLayar.x, posLayar.y, 0));
        Plane pl = new Plane(platTransform.up, platTransform.position);
        float dist; if (!pl.Raycast(ray, out dist)) return false;
        Vector3 hit = platTransform.InverseTransformPoint(ray.GetPoint(dist));
        // Spot grid: kolom di sumbu X, baris di sumbu Y
        Bounds b = AmbilBoundsPlateLokal();
        float stepX = Mathf.Max(0.001f, b.extents.x * 0.58f);
        float stepY = Mathf.Max(0.001f, b.extents.z * 0.43f);
        float kf = (hit.x - b.center.x) / stepX + 1f;
        float bf = 1f - (hit.z - b.center.z) / stepY;
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
            go.transform.localPosition = KoordinatSpotLokal(k, b);
            Bounds boundsPlate = AmbilBoundsPlateLokal();
            float diameterSpot = Mathf.Min(boundsPlate.extents.x, boundsPlate.extents.z) * 0.20f;
            go.transform.localScale = new Vector3(diameterSpot, diameterSpot, diameterSpot);
            // Tempatkan pusat larutan di dalam cekungan, sejajar dengan pusat lubang.
            go.transform.localPosition = KoordinatSpotLokal(k, b) - Vector3.up * (diameterSpot * 0.15f);
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
        Vector3 centerLok = KoordinatSpotLokal(k, b) - Vector3.up * 0.02f;
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
                Vector3 off = platTransform.TransformVector(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * r, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * r));
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
        Vector3 tujuan = PosisiLubangSpot(k, b, 0.02f);
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.transform.localScale = Vector3.one * 0.08f;
        go.transform.position = tujuan - platTransform.up * 0.5f;
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
        Vector3 pusat = PosisiLubangSpot(k, b, 0.02f);
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
            Vector3 tujuan = platTransform != null ? PosisiLubangSpot(k, b, 0.02f) : Vector3.zero;
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
