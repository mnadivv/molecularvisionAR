using System;
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
    [Tooltip("Opsional: gambar panduan penempatan untuk Soal 3 (MgCl2). Kosongkan jika tidak ada diagram.")]
    public Sprite diagramPenempatanMgCl2;

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
        { new Color(0.04f,0.34f,0.96f,0.96f), new Color(0.60f,0.85f,1.00f,0.85f), new Color(0.60f,0.85f,1.00f,0.85f) },
        { new Color(0.75f,0.85f,0.60f,0.85f), new Color(0.78f,0.88f,0.60f,0.85f), new Color(0.78f,0.88f,0.60f,0.85f) },
    };

    private class SpotData
    {
        public GameObject silinder;   // cairan di spot
        public Material mat;
        public bool terisi;           // sudah ada sampel
        public bool[] campuran = new bool[3]; // campuran baris sudah selesai
        public bool bentukCairan;
    }

    // Jumlah tetes/spatula yang sudah dimasukkan per kolom (untuk soal 2-4)
    private int[] hitungIsian = new int[3];
    private bool[] kolomSelesai = new bool[3];
    private bool[] barisSelesai = new bool[3];
    private bool[] aquadesDitetes = new bool[3];
    private bool[] etanolDitetes = new bool[3];
    private bool[] aquadesBaris3Ditetes = new bool[3];
    private bool[] etanolBaris3Ditetes = new bool[3];
    private bool[] tetesanBaris3Berlangsung = new bool[3];
    private bool sumberEtanolBaris3Diletakkan;
    private GameObject cawanEtanolBaris3Sumber;
    private bool[,] probeMerahBaris3 = new bool[3, 3];
    private bool[,] probeHitamBaris3 = new bool[3, 3];
    private bool voltmeterDipasang;
    private GameObject voltmeterAktif;
    private GameObject panelProbeVoltmeter;
    private RectTransform[] tokenProbeBaris3 = new RectTransform[2];
    private Vector2[] posisiHomeProbe = new Vector2[2];
    private TMP_Text teksStatusProbe;
    private GameObject panelToastUji;
    private TMP_Text teksToastUji;
    private Coroutine tutupToastUji;
    private Vector3 posisiToastUjiEtanol;
    private bool posisiToastUjiEtanolTersimpan;
    private RectTransform[] markerProbeSampel = new RectTransform[9];
    // Titik keluar pada model Aquades mengikuti konfigurasi Percobaan 1 Soal 3 di praktikum1.unity.
    private static readonly Vector3 TitikKeluarAquadesPercobaan1Soal3 = new Vector3(-0.97f, 4.15f, -0.5f);
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
    private GameObject cawanEtanolSumber;
    private int pendingLubangPetroleum = -1;
    private GameObject pipetPetroleumAktif;
    private Coroutine animasiPetroleumAktif;
    private int pendingLubangEtanol = -1;
    private GameObject pipetEtanolAktif;
    private Coroutine animasiEtanolAktif;
    [SerializeField] private RectTransform markerCawanAsamScene;
    [SerializeField] private RectTransform[] targetDropSpatula = new RectTransform[3];
    [SerializeField] private GameObject panelPilihanSpatula;
    [SerializeField] private Button buttonUjung;
    [SerializeField] private Button buttonSedang;
    [SerializeField] private Button buttonPenuh;
    private GameObject penandaDropCawanAsam;
    private RectTransform rectPenandaDropCawanAsam;
    private bool sumberAsamDiletakkan;
    private bool sumberEtanolDiletakkan;
    private readonly List<GameObject> penandaAsam = new List<GameObject>();
    private readonly Dictionary<RectTransform, Vector2> kecepatanPosisiMarker = new Dictionary<RectTransform, Vector2>();
    private bool sedangMenakarAsam;
    private bool instruksiAdukAktif;
    private bool sedangMenuangMgCl2;
    private static Sprite spriteVisualTarget;
    private static Sprite spriteKotakTarget;
    private readonly List<GameObject> gelembungMgCl2 = new List<GameObject>();
    private readonly List<GameObject> kristalMgCl2Soal5 = new List<GameObject>();
    private readonly List<GameObject> efekKelarutanPermanen = new List<GameObject>();

    // Soal 2 (asam benzoat, baris 1) dan Soal 3 (MgCl2, baris 2) memakai alur yang sama:
    // cawan sumber -> spatula -> tiga lubang pada satu baris -> pilih takaran -> animasi menyendok.
    private bool FaseSumberAktif { get { return fase == FaseP2.IsiKolom1 || fase == FaseP2.IsiKolom2; } }
    private const int BarisPetroleum = 2; // indeks lokal 2 tampil pada baris paling atas plate ini
    private int BarisSumberAktif { get { return fase == FaseP2.IsiKolom1 ? 0 : 1; } }
    private int IndeksSumberAktif { get { return fase == FaseP2.IsiKolom2 ? 1 : 0; } }
    private GameObject PanelSoalSumberAktif { get { return fase == FaseP2.IsiKolom2 ? soal3 : soal2; } }
    private bool SpotSumberTerisi(int baris, int urutan)
    {
        // Grid fisik memisahkan baris Soal 4 (atas), Soal 3 (tengah), dan Soal 2 (bawah).
        SpotData s = grid[urutan, baris];
        return s != null && s.terisi && s.silinder != null;
    }

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
            float ukuranUjung = Mathf.Max(boundsSpatula.size.x, Mathf.Max(boundsSpatula.size.y, boundsSpatula.size.z)) * 0.22f;
            float ukuranSerbuk = Mathf.Max(boundsSerbuk.size.x, Mathf.Max(boundsSerbuk.size.y, boundsSerbuk.size.z));
            if (ukuranSerbuk > 0.0001f) serbuk.transform.localScale *= ukuranUjung / ukuranSerbuk;
        }
        foreach (var c in serbuk.GetComponentsInChildren<Collider>()) Destroy(c);
    }

    /// <summary>Menempelkan segenggam kristal MgCl2 kecil di ujung spatula (padanan serbuk asam benzoat pada Soal 2).</summary>
    public void TambahkanKristalMgCl2KeSpatula(GameObject spatula)
    {
        if (spatula == null) return;
        Transform ujung = PastikanUjungSpatula(spatula);
        if (ujung == null || ujung.childCount > 0) return;
        Material matKristal = null;
        if (cawanAsamBenzoat != null)
        {
            Transform kristalCawan = cawanAsamBenzoat.transform.Find("Kristal_MgCl2");
            Renderer rk = kristalCawan != null ? kristalCawan.GetComponent<Renderer>() : null;
            if (rk != null) matKristal = rk.sharedMaterial;
        }
        if (matKristal == null) matKristal = matPadatan;
        Bounds boundsSpatula;
        bool adaBoundsSpatula = AmbilBoundsRenderer(spatula, out boundsSpatula);
        var gumpal = new GameObject("SerbukMgCl2");
        gumpal.transform.SetParent(ujung, false);
        gumpal.transform.localPosition = Vector3.zero;
        gumpal.transform.localRotation = Quaternion.identity;
        // Susunan tetap (bukan acak) supaya ketiga dosis terlihat konsisten.
        Vector3[] posisiButir =
        {
            new Vector3(0f, 0f, 0f), new Vector3(0.28f, 0.05f, 0.12f), new Vector3(-0.26f, 0.08f, -0.10f),
            new Vector3(0.06f, 0.20f, -0.26f), new Vector3(-0.08f, -0.04f, 0.30f), new Vector3(0.30f, -0.06f, -0.22f),
            new Vector3(-0.30f, 0f, 0.24f)
        };
        float[] ukuranButir = { 0.34f, 0.24f, 0.26f, 0.22f, 0.20f, 0.22f, 0.18f };
        for (int i = 0; i < posisiButir.Length; i++)
        {
            var butir = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(butir.GetComponent<Collider>());
            butir.name = "ButirMgCl2";
            butir.transform.SetParent(gumpal.transform, false);
            butir.transform.localPosition = posisiButir[i];
            butir.transform.localRotation = Quaternion.Euler(17f * i + 8f, 31f * i, 23f * i + 5f);
            butir.transform.localScale = Vector3.one * ukuranButir[i];
            if (matKristal != null) butir.GetComponent<Renderer>().sharedMaterial = matKristal;
        }
        MatikanBayangan(gumpal);
        Bounds boundsGumpal;
        if (adaBoundsSpatula && AmbilBoundsRenderer(gumpal, out boundsGumpal))
        {
            float ukuranUjung = Mathf.Max(boundsSpatula.size.x, Mathf.Max(boundsSpatula.size.y, boundsSpatula.size.z)) * 0.20f;
            float ukuranGumpal = Mathf.Max(boundsGumpal.size.x, Mathf.Max(boundsGumpal.size.y, boundsGumpal.size.z));
            if (ukuranGumpal > 0.0001f) gumpal.transform.localScale *= ukuranUjung / ukuranGumpal;
        }
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

    private void HadapkanCekunganSpatulaKeKamera(GameObject spatula, Camera kamera)
    {
        if (spatula == null || kamera == null) return;
        Vector3 sisiCekung = spatula.transform.up;
        Vector3 arahKeKamera = -kamera.transform.forward;
        if (Vector3.Dot(sisiCekung, arahKeKamera) < 0f)
            spatula.transform.rotation = Quaternion.AngleAxis(180f, spatula.transform.right) * spatula.transform.rotation;
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
            spatulaAsamDiSamping.transform.rotation = (Camera.main != null ? Camera.main.transform.rotation : platTransform.rotation) * objekSpatulaAsam.transform.localRotation;
            TegakkanSpatula(spatulaAsamDiSamping, Camera.main);
            HadapkanCekunganSpatulaKeKamera(spatulaAsamDiSamping, Camera.main);
            TerangkanSpatula(spatulaAsamDiSamping);
            spatulaAsamDiSamping.SetActive(true);
            

            Bounds plateBounds;
            Bounds spoonBounds;
            if (AmbilBoundsRenderer(platTransform.gameObject, out plateBounds) &&
                AmbilBoundsRenderer(spatulaAsamDiSamping, out spoonBounds))
            {
                float lebarPlate = Mathf.Max(plateBounds.size.x, plateBounds.size.y);
                float panjangSendok = Mathf.Max(spoonBounds.size.x, spoonBounds.size.y);
                float skala = panjangSendok > 0.001f ? (lebarPlate * 0.04f / panjangSendok) : 1f;
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
        if (!FaseSumberAktif) return null;
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
        enabled = true;
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
        // Selaraskan tiap panel soal dengan baris targetnya pada plate.
        if (platTransform != null && soal5 != null && soal5.activeInHierarchy && !barisSelesai[0] && fase != FaseP2.CampurBaris1)
            AktifkanFase(FaseP2.CampurBaris1);
        else if (platTransform != null && soal4 != null && soal4.activeInHierarchy && !kolomSelesai[2] && fase != FaseP2.IsiKolom3)
            AktifkanFase(FaseP2.IsiKolom3);
        else if (platTransform != null && soal3 != null && soal3.activeInHierarchy && !kolomSelesai[1] && fase != FaseP2.IsiKolom2)
            AktifkanFase(FaseP2.IsiKolom2);
        else if (platTransform != null && soal2 != null && soal2.activeInHierarchy && !kolomSelesai[0] && fase != FaseP2.IsiKolom1)
            AktifkanFase(FaseP2.IsiKolom1);

        if (panelNotif != null && panelNotif.activeSelf && !AdaSoalP2Aktif())
            panelNotif.SetActive(false);
        if ((FaseSumberAktif || fase == FaseP2.IsiKolom3 || fase == FaseP2.CampurBaris2 || fase == FaseP2.CampurBaris3) && platTransform != null)
        {
            if (FaseSumberAktif) PerbaruiPenandaAsam();
            PerbaruiPenandaDropCawan();
        }
        PerbaruiTargetDropSpatula();
    }
    // ======================================================
    // API publik yang dipanggil dari CardTahanGeser3D / ManajerTakaran
    // ======================================================

    private static void MatikanBayangan(GameObject objek)
    {
        if (objek == null) return;
        foreach (Renderer renderer in objek.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void BeningkanSlotKaca(GameObject objek)
    {
        if (objek == null) return;
        ManajerTakaran takaran = ManajerTakaran.Instance;
        if (takaran == null || takaran.materialKaca == null) return;
        Renderer[] renderers = objek.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            bool berubah = false;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] == null) continue;
                string nama = materials[j].name.ToLowerInvariant();
                if (!nama.Contains("kaca") && !nama.Contains("glass")) continue;
                materials[j] = takaran.materialKaca;
                berubah = true;
            }
            if (berubah) renderers[i].sharedMaterials = materials;
        }
    }

    /// <summary>Dipanggil saat PlatTetes di-drop. True = diterima.</summary>
    public bool DropPlate(Vector2 posLayar, GameObject model, Camera cam)
    {
        if (fase != FaseP2.Idle) return false;
        if (slotPlate == null || !slotPlate.Mengandung(posLayar, cam)) return false;
        slotPlate.Isi(model, cam, 0.5f);
        // Plate AR tidak perlu melempar bayangan ke atas lubang atau latar kamera.
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
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
        if (FaseSumberAktif && namaBahan == "Spatula") return DropSpatulaKeTarget(posLayar, cam, model);
        if (fase == FaseP2.CampurBaris3 && namaBahan == "TesKonduktor") return DropTespen(posLayar, cam, model);
        if (fase == FaseP2.CampurBaris3 && namaBahan == "Etanol") return DropSumberEtanolBaris3(posLayar, cam, model);
        if (fase == FaseP2.IsiKolom3 && namaBahan == "PetroleumEter") return DropSumberPetroleumEter(posLayar, cam, model);
        if (fase == FaseP2.IsiKolom3 && namaBahan == "PipetTetes") return DropPipetTetes(posLayar, cam, model);
        if (fase == FaseP2.CampurBaris2 && namaBahan == "Etanol") return DropSumberEtanol(posLayar, cam, model);
        if (fase == FaseP2.CampurBaris2 && namaBahan == "PipetTetes") return DropPipetEtanol(posLayar, cam, model);
        if (fase == FaseP2.IsiKolom2 && namaBahan == "MGCL2") return DropSumberMgCl2(posLayar, cam, model);
        int kolom = 2, baris;
        bool spotDitemukan;
        if (fase == FaseP2.IsiKolom3 && namaBahan == "PetroleumEter")
            spotDitemukan = CariLubangPetroleum(posLayar, cam, out baris);
        else
            spotDitemukan = CariSpot(posLayar, cam, out kolom, out baris);
        if (!spotDitemukan) return false;
        switch (fase)
        {
            case FaseP2.IsiKolom1:
                if (namaBahan != "Spatula" || !sumberAsamDiletakkan || cawanAsamBenzoat == null || kolom != 0) { TampilSalah(); return false; }
                if (pendingKolomAsam >= 0 || grid[baris, 2].terisi) return false;
                pendingKolomAsam = kolom;
                pendingBarisAsam = baris;
                pendingModelAsam = model;
                TampilkanPilihanTakaranAsam();
                return true;
            case FaseP2.IsiKolom2:
                if (namaBahan != "MGCL2" || kolom != 1) { TampilSalah(); return false; }
                TambahIsian(kolom, baris); return true;
            case FaseP2.IsiKolom3:
                TampilSalah(); return false;
            case FaseP2.CampurBaris1:
                if (baris != 0 || kolom < 0 || kolom >= 3 || model == null) { TampilSalah(); return false; }
                if (namaBahan == "Aquades")
                {
                    if (SemuaAquadesSudahDiteteskan() || aquadesDitetes[kolom]) { TampilSalah(); return false; }
                    if (animAktif == null) animAktif = StartCoroutine(TeteskanAquadesKeSampel(kolom, cam, model));
                    return true;
                }
                if (namaBahan == "TusukGigi")
                {
                    if (!SemuaAquadesSudahDiteteskan() || !AdaAquadesBelumDiaduk()) { TampilSalah(); return false; }
                    int targetAduk = LubangAquadesBerikutnyaBelumDiaduk();
                    if (kolom != targetAduk) { TampilSalah(); return false; }
                    if (animAktif == null) animAktif = StartCoroutine(AdukSatuSampelAquades(kolom, cam, model));
                    return true;
                }
                TampilSalah();
                return false;
            case FaseP2.CampurBaris2:
                if (namaBahan != "TusukGigi" || baris != 1 || kolom < 0 || kolom >= 3 || model == null ||
                    !SemuaEtanolSudahDiteteskan() || kolom != LubangEtanolBerikutnyaBelumDiaduk())
                { TampilSalah(); return false; }
                if (animAktif == null) animAktif = StartCoroutine(AdukSatuSampelEtanol(kolom, cam, model));
                return true;
            case FaseP2.CampurBaris3:
                if (baris != 2 || kolom < 0 || kolom >= 3 || model == null) { TampilSalah(); return false; }
                if (namaBahan == "Aquades")
                {
                    if (SemuaAquadesBaris3Diteteskan() || aquadesBaris3Ditetes[kolom]) { TampilSalah(); return false; }
                    if (animAktif == null) animAktif = StartCoroutine(TeteskanPelarutBaris3(kolom, "Aquades", cam, model));
                    return true;
                }
                if (namaBahan == "PipetTetes" && SemuaAquadesBaris3Diteteskan())
                {
                    if (!sumberEtanolBaris3Diletakkan || cawanEtanolBaris3Sumber == null ||
                        SemuaEtanolBaris3Diteteskan() || etanolBaris3Ditetes[kolom]) { TampilSalah(); return false; }
                    if (animAktif == null) animAktif = StartCoroutine(TeteskanPelarutBaris3(kolom, "PipetTetes", cam, model));
                    return true;
                }
                if (namaBahan == "TusukGigi")
                {
                    if (!SemuaPelarutBaris3Diteteskan() || kolom != LubangBaris3BerikutnyaBelumDiaduk()) { TampilSalah(); return false; }
                    if (animAktif == null) animAktif = StartCoroutine(AdukSatuSampelBaris3(kolom, cam, model));
                    return true;
                }
                TampilSalah();
                return false;
        }
        return false;
    }

    private bool SemuaAquadesBaris3Diteteskan()
    {
        return aquadesBaris3Ditetes != null && aquadesBaris3Ditetes.Length == 3 &&
               aquadesBaris3Ditetes[0] && aquadesBaris3Ditetes[1] && aquadesBaris3Ditetes[2];
    }

    private bool SemuaEtanolBaris3Diteteskan()
    {
        return etanolBaris3Ditetes != null && etanolBaris3Ditetes.Length == 3 &&
               etanolBaris3Ditetes[0] && etanolBaris3Ditetes[1] && etanolBaris3Ditetes[2];
    }

    private bool SemuaPelarutBaris3Diteteskan()
    {
        return SemuaAquadesBaris3Diteteskan() && SemuaEtanolBaris3Diteteskan();
    }

    private bool AdaAquadesBelumBaris3()
    {
        for (int k = 0; k < 3; k++) if (!aquadesBaris3Ditetes[k]) return true;
        return false;
    }

    private bool AdaEtanolBelumBaris3()
    {
        for (int k = 0; k < 3; k++) if (!etanolBaris3Ditetes[k]) return true;
        return false;
    }

    private int LubangBaris3BerikutnyaBelumDiaduk()
    {
        for (int k = 0; k < 3; k++)
            if (grid[k, 2] != null && grid[k, 2].terisi && !grid[k, 2].campuran[2]) return k;
        return -1;
    }

    private IEnumerator TeteskanPelarutBaris3(int k, string namaBahan, Camera cam, GameObject model)
    {
        if (cam == null) cam = Camera.main;
        bool aquades = namaBahan == "Aquades";
        bool pipet = namaBahan == "PipetTetes";
        if (platTransform == null || model == null || cam == null || k < 0 || k >= 3 ||
            (aquades ? aquadesBaris3Ditetes[k] : !SemuaAquadesBaris3Diteteskan() ||
             !sumberEtanolBaris3Diletakkan || cawanEtanolBaris3Sumber == null || etanolBaris3Ditetes[k]))
        {
            if (model != null) Destroy(model);
            animAktif = null;
            yield break;
        }

        foreach (Collider c in model.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody rb in model.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
        foreach (Camera c in model.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (Light l in model.GetComponentsInChildren<Light>(true)) l.enabled = false;
        MatikanBayangan(model);
        model.transform.SetParent(platTransform, true);
        tetesanBaris3Berlangsung[k] = true;
        if (targetDropSpatula != null && k < targetDropSpatula.Length && targetDropSpatula[k] != null)
            targetDropSpatula[k].gameObject.SetActive(false);

        Vector3 lubang = PosisiLubangSpot(k, 2, 0.006f);
        Quaternion rotasiAwal = model.transform.rotation;
        if (pipet)
        {
            Quaternion rotasiPipet = cam.transform.rotation * Quaternion.Euler(309.72903f, 270f, 90f);
            Bounds boundsSumber;
            Vector3 posisiAmbil = cawanEtanolBaris3Sumber.transform.position + cam.transform.up * 0.05f;
            Vector3 posisiAtasSumber = posisiAmbil + cam.transform.up * 0.05f;
            if (AmbilBoundsRenderer(cawanEtanolBaris3Sumber, out boundsSumber))
            {
                float tinggiSumber = Mathf.Abs(cam.transform.up.x) * boundsSumber.extents.x +
                                     Mathf.Abs(cam.transform.up.y) * boundsSumber.extents.y +
                                     Mathf.Abs(cam.transform.up.z) * boundsSumber.extents.z;
                posisiAtasSumber = boundsSumber.center + cam.transform.up * (tinggiSumber + 0.035f);
                posisiAmbil = boundsSumber.center + cam.transform.up * (tinggiSumber - 0.008f);
            }
            Vector3 posisiUjung = lubang + cam.transform.up * 0.018f;
            Vector3 akarAtasSumber = BatasiTitikPipetDalamLayar(cam, PosisiAkarPipetUntukUjung(model, cam, posisiAtasSumber));
            Vector3 akarSumber = BatasiTitikPipetDalamLayar(cam, PosisiAkarPipetUntukUjung(model, cam, posisiAmbil));
            Vector3 akarAtasLubang = BatasiTitikPipetDalamLayar(cam,
                PosisiAkarPipetUntukUjung(model, cam, lubang + cam.transform.up * 0.08f));
            Vector3 akarLubang = BatasiTitikPipetDalamLayar(cam, PosisiAkarPipetUntukUjung(model, cam, posisiUjung));
            yield return GerakkanPipetPetroleum(model, cam, akarAtasSumber, rotasiPipet, 0.32f);
            yield return GerakkanPipetPetroleum(model, cam, akarSumber, rotasiPipet, 0.24f);
            yield return AnimasiSedotEtanol(cam, posisiAmbil);
            yield return GerakkanPipetPetroleum(model, cam, akarAtasSumber, rotasiPipet, 0.2f);
            yield return GerakkanPipetPetroleum(model, cam, akarAtasLubang, rotasiPipet, 0.34f);
            yield return GerakkanPipetPetroleum(model, cam, akarLubang, rotasiPipet, 0.2f);
        }
        else
        {
            Vector3 ujung = PosisiMulutBotol(model);
            Vector3 posisiUjung = lubang + cam.transform.up * 0.07f;
            model.transform.position += posisiUjung - ujung;
        }

        Color warnaCair = aquades ? new Color(0.47f, 0.77f, 1f, 0.76f) : new Color(0.79f, 0.88f, 1f, 0.76f);
        ManajerTakaran takaran = ManajerTakaran.Instance;
        bool jumlahBenar = false;
        if (takaran == null)
        {
            TampilSalah();
        }
        else
        {
            yield return takaran.TeteskanKeSpot("sampel " + (k + 1), 5,
                hitung => AnimasiSatuTetesBaris3(model, k, cam, warnaCair, pipet, rotasiAwal),
                hasil => jumlahBenar = hasil, aquades ? "akuades" : "etanol");
        }

        if (jumlahBenar)
        {
            if (aquades) aquadesBaris3Ditetes[k] = true;
            else etanolBaris3Ditetes[k] = true;
            Color warnaAwal = aquades
                ? Color.Lerp(warnaSampel[k], warnaCair, 0.40f)
                : Color.Lerp(warnaSampel[k], warnaCair, 0.58f);
            TampilSpot(k, 2, warnaAwal, false, true);
            AturKekasaranCairanEtanol(grid[k, 2].mat);
        }
        if (model != null) Destroy(model);
        tetesanBaris3Berlangsung[k] = false;
        PerbaruiTargetDropSpatula();
        animAktif = null;
    }

    private IEnumerator AnimasiSatuTetesBaris3(GameObject model, int k, Camera cam, Color warna,
                                                bool pipet, Quaternion rotasiAwal)
    {
        if (model == null || platTransform == null || cam == null) yield break;
        Vector3 titikKeluar = PosisiMulutBotol(model);
        Vector3 lokalUjung = model.transform.InverseTransformPoint(titikKeluar);
        if (!pipet)
        {
            Quaternion rotasiTuang = Quaternion.AngleAxis(90f, cam.transform.forward) * model.transform.rotation;
            yield return PutarBotolAquadesPadaUjung(model, titikKeluar, lokalUjung, rotasiTuang, 0.16f);
            titikKeluar = PosisiMulutBotol(model);
        }

        Vector3 tujuan = PosisiLubangSpot(k, 2, 0.004f);
        GameObject tetes = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider collider = tetes.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        tetes.name = "Tetes" + (pipet ? "Pipet" : (warna.b > warna.r ? "Aquades" : "Etanol")) + "Baris3";
        Material material = BuatMaterialTetesPetroleum(warna);
        tetes.GetComponent<Renderer>().sharedMaterial = material;
        float diameter = Mathf.Max(0.0035f, Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.028f);
        tetes.transform.localScale = new Vector3(diameter * 0.72f, diameter, diameter * 0.72f);
        tetes.transform.position = titikKeluar;
        MatikanBayangan(tetes);
        float waktu = 0f;
        while (waktu < 0.24f)
        {
            if (tetes == null || model == null) yield break;
            waktu += Time.deltaTime;
            float t = Ease(waktu / 0.24f);
            tetes.transform.position = Vector3.Lerp(titikKeluar, tujuan, t);
            yield return null;
        }
        if (tetes != null) Destroy(tetes);
        if (material != null) Destroy(material);
        if (!pipet && model != null)
            yield return PutarBotolAquadesPadaUjung(model, titikKeluar, lokalUjung, rotasiAwal, 0.14f);
    }

    private IEnumerator AdukSatuSampelBaris3(int k, Camera cam, GameObject tusuk)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || tusuk == null || !SemuaPelarutBaris3Diteteskan() ||
            k != LubangBaris3BerikutnyaBelumDiaduk())
        {
            if (tusuk != null) Destroy(tusuk);
            animAktif = null;
            yield break;
        }

        foreach (Collider c in tusuk.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody rb in tusuk.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
        foreach (Camera c in tusuk.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (Light l in tusuk.GetComponentsInChildren<Light>(true)) l.enabled = false;
        MatikanBayangan(tusuk);
        tusuk.transform.SetParent(platTransform, true);
        AturUkuranTusukGigi(tusuk);
        yield return AnimAdukAquades(k, tusuk, cam, 2);

        TampilSpot(k, 2, warnaHasil[k, 2], false, true);
        AturKekasaranCairanEtanol(grid[k, 2].mat);
        grid[k, 2].campuran[2] = true;
        if (tusuk != null) Destroy(tusuk);
        if (LubangBaris3BerikutnyaBelumDiaduk() < 0)
        {
            barisSelesai[2] = true;
            // Sumber etanol tidak lagi dibutuhkan saat praktikum berpindah ke tahap uji konduktivitas.
            if (cawanEtanolBaris3Sumber != null)
            {
                Bounds boundsSumberEtanol;
                posisiToastUjiEtanol = AmbilBoundsRenderer(cawanEtanolBaris3Sumber, out boundsSumberEtanol)
                    ? boundsSumberEtanol.center
                    : cawanEtanolBaris3Sumber.transform.position;
                posisiToastUjiEtanolTersimpan = true;
                Destroy(cawanEtanolBaris3Sumber);
                cawanEtanolBaris3Sumber = null;
            }
            sumberEtanolBaris3Diletakkan = false;
            PerbaruiPenandaDropCawan();
            PerbaruiTargetDropSpatula();
        }
        animAktif = null;
        PerbaruiTargetDropSpatula();
    }

    private IEnumerator TeteskanAquadesKeSampel(int k, Camera cam, GameObject botol)
    {
        if (cam == null) cam = Camera.main;
        if (platTransform == null || botol == null || cam == null || k < 0 || k >= 3)
        {
            if (botol != null) Destroy(botol);
            animAktif = null;
            yield break;
        }

        if (grid[k, 0] == null) grid[k, 0] = new SpotData();
        if (k != 1) TampilSpot(k, 0, warnaSampel[k], false, true);
        foreach (Collider c in botol.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (Rigidbody rb in botol.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
        Quaternion rotasiTegak = botol.transform.rotation;
        // Besarkan botol sedikit agar sumber tetesan lebih mudah terlihat.
        botol.transform.localScale *= 1.10f;
        if (cam != null)
        {
            float depth = Mathf.Max(cam.nearClipPlane + 0.01f, Vector3.Dot(botol.transform.position - cam.transform.position, cam.transform.forward));
            float angkatLayar = cam.orthographic
                ? cam.orthographicSize * 0.08f
                : depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.08f;
            botol.transform.position += cam.transform.up * angkatLayar;
            PastikanPipetTetapTerlihat(botol, cam);

            float jarakKeLubang = Mathf.Max(cam.nearClipPlane + 0.01f, Vector3.Dot(PosisiLubangSpot(k, 0, 0.008f) - cam.transform.position, cam.transform.forward));
            float jarakAliran = cam.orthographic
                ? cam.orthographicSize * 0.16f
                : jarakKeLubang * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.16f;
            Vector3 ujungAwal = PosisiMulutBotol(botol);
            Vector3 ujungDiAtasLubang = PosisiLubangSpot(k, 0, 0.008f) + cam.transform.up * jarakAliran;
            botol.transform.position += ujungDiAtasLubang - ujungAwal;
        }

        ManajerTakaran takaran = ManajerTakaran.Instance;
        bool jumlahBenar = false;
        if (takaran == null)
        {
            TampilSalah();
            Destroy(botol);
            animAktif = null;
            yield break;
        }
        yield return takaran.TeteskanKeSpot("sampel " + (k + 1), 5,
            hitung => AnimasiTetesAquades(botol, k, cam, rotasiTegak),
            hasil => jumlahBenar = hasil, "akuades");

        if (!jumlahBenar)
        {
            if (botol != null) Destroy(botol);
            animAktif = null;
            PerbaruiTargetDropSpatula();
            yield break;
        }

        aquadesDitetes[k] = true;
        if (k != 1)
        {
            Color warnaSebelumAduk = Color.Lerp(warnaSampel[k], new Color(0.55f, 0.75f, 1f, 0.62f), 0.36f);
            TampilSpot(k, 0, warnaSebelumAduk, false, true);
        }
        Notif("Akuades selesai diteteskan: " + JumlahAquadesSelesai() + "/3 sampel");
        if (botol != null) Destroy(botol);
        if (SemuaAquadesSudahDiteteskan()) TampilkanInstruksiAduk();
        animAktif = null;
        PerbaruiTargetDropSpatula();
    }

    private IEnumerator AnimasiTetesAquades(GameObject botol, int k, Camera cam, Quaternion rotasiTegak)
    {
        if (botol == null || platTransform == null) yield break;
        Vector3 tujuan = PosisiLubangSpot(k, 0, 0.008f);
        Vector3 titikSedotan = PosisiMulutBotol(botol);
        Vector3 titikSedotanLokal = botol.transform.InverseTransformPoint(titikSedotan);
        Quaternion rotasiTegakSaatIni = botol.transform.rotation;
        Vector3 sumbuLayar = cam != null ? cam.transform.forward : Vector3.forward;
        // Seperti Percobaan 1 Soal 3: botol berputar 90 derajat ke kiri pada bidang layar.
        Quaternion rotasiTuang = Quaternion.AngleAxis(90f, sumbuLayar) * rotasiTegakSaatIni;
        yield return PutarBotolAquadesPadaUjung(botol, titikSedotan, titikSedotanLokal, rotasiTuang, 0.5f);

        Vector3 awal = PosisiMulutBotol(botol);
        float ukuranAcuan = Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y);
        float diameterTetes = Mathf.Max(0.003f, ukuranAcuan * 0.032f);
        Material material = BuatMaterialTetesPetroleum(new Color(0.48f, 0.78f, 1f, 0.62f));
        GameObject tetes = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider colliderTetes = tetes.GetComponent<Collider>();
        if (colliderTetes != null) Destroy(colliderTetes);
        tetes.name = "TetesAquadesSoal5";
        tetes.GetComponent<Renderer>().sharedMaterial = material;
        tetes.transform.position = awal;
        tetes.transform.localScale = new Vector3(diameterTetes * 0.72f, diameterTetes, diameterTetes * 0.72f);
        MatikanBayangan(tetes);
        float durasiTetes = 0.28f;
        while (durasiTetes > 0f && tetes != null && botol != null)
        {
            durasiTetes -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(durasiTetes / 0.28f);
            tetes.transform.position = Vector3.Lerp(awal, tujuan, Ease(t));
            yield return null;
        }
        if (tetes != null) Destroy(tetes);
        if (material != null) Destroy(material);
        if (botol != null)
            yield return PutarBotolAquadesPadaUjung(botol, titikSedotan, titikSedotanLokal, rotasiTegak, 0.4f);
    }

    private IEnumerator PutarBotolAquadesPadaUjung(GameObject botol, Vector3 titikPutar, Vector3 titikPutarLokal, Quaternion rotasiTujuan, float durasi)
    {
        if (botol == null) yield break;
        Quaternion rotasiAwal = botol.transform.rotation;
        float waktu = 0f;
        while (waktu < durasi)
        {
            if (botol == null) yield break;
            waktu += Time.deltaTime;
            botol.transform.rotation = Quaternion.Slerp(rotasiAwal, rotasiTujuan, Ease(waktu / durasi));
            Vector3 offsetTitikPutar = botol.transform.TransformPoint(titikPutarLokal) - botol.transform.position;
            botol.transform.position = titikPutar - offsetTitikPutar;
            yield return null;
        }
        if (botol != null)
        {
            botol.transform.rotation = rotasiTujuan;
            Vector3 offsetTitikPutar = botol.transform.TransformPoint(titikPutarLokal) - botol.transform.position;
            botol.transform.position = titikPutar - offsetTitikPutar;
        }
    }

    private Vector3 PosisiMulutBotol(GameObject botol)
    {
        if (botol == null) return Vector3.zero;
        Vector3 titikKeluarReferensi = botol.transform.TransformPoint(TitikKeluarAquadesPercobaan1Soal3);
        Bounds boundsBotol;
        if (AmbilBoundsRenderer(botol, out boundsBotol))
        {
            Vector3 toleransi = boundsBotol.extents * 0.15f;
            Bounds batasTitikKeluar = new Bounds(boundsBotol.center, boundsBotol.size + toleransi * 2f);
            if (batasTitikKeluar.Contains(titikKeluarReferensi)) return titikKeluarReferensi;
        }
        Transform ujungTuang = CariUjungTuangAquades(botol.transform);
        if (ujungTuang != null)
        {
            MeshFilter[] meshFilters = ujungTuang.GetComponentsInChildren<MeshFilter>(true);
            Vector3 titikMeshUjung = ujungTuang.position;
            float jarakMeshTerjauh = -1f;
            for (int m = 0; m < meshFilters.Length; m++)
            {
                Mesh mesh = meshFilters[m] != null ? meshFilters[m].sharedMesh : null;
                if (mesh == null) continue;
                Vector3[] vertices = mesh.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    Vector3 titik = meshFilters[m].transform.TransformPoint(vertices[v]);
                    float jarak = (titik - botol.transform.position).sqrMagnitude;
                    if (jarak > jarakMeshTerjauh) { jarakMeshTerjauh = jarak; titikMeshUjung = titik; }
                }
            }
            if (jarakMeshTerjauh >= 0f) return titikMeshUjung;

            Renderer rendererUjung = ujungTuang.GetComponent<Renderer>();
            if (rendererUjung == null) rendererUjung = ujungTuang.GetComponentInChildren<Renderer>(true);
            if (rendererUjung != null)
            {
                Bounds boundsUjung = rendererUjung.bounds;
                Vector3 titikUjung = boundsUjung.center;
                float jarakTerjauh = -1f;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 titik = new Vector3((i & 1) == 0 ? boundsUjung.min.x : boundsUjung.max.x,
                                                (i & 2) == 0 ? boundsUjung.min.y : boundsUjung.max.y,
                                                (i & 4) == 0 ? boundsUjung.min.z : boundsUjung.max.z);
                    float jarak = (titik - botol.transform.position).sqrMagnitude;
                    if (jarak > jarakTerjauh) { jarakTerjauh = jarak; titikUjung = titik; }
                }
                return titikUjung;
            }
            return ujungTuang.position;
        }
        Bounds bounds;
        if (!AmbilBoundsRenderer(botol, out bounds)) return botol.transform.position + botol.transform.up * 0.05f;
        Vector3 sumbuAtas = botol.transform.up;
        float setengahTinggi = Mathf.Abs(sumbuAtas.x) * bounds.extents.x +
                               Mathf.Abs(sumbuAtas.y) * bounds.extents.y +
                               Mathf.Abs(sumbuAtas.z) * bounds.extents.z;
        return bounds.center + sumbuAtas * setengahTinggi;
    }

    private Transform CariUjungTuangAquades(Transform akar)
    {
        if (akar == null) return null;
        foreach (Transform child in akar.GetComponentsInChildren<Transform>(true))
        {
            string nama = child.name.ToLowerInvariant();
            if (nama.Contains("straw") || nama.Contains("spout") || nama.Contains("nozzle") ||
                nama.Contains("sedot") || nama.Contains("ujungpipa") || nama.Contains("mulutbotol") ||
                nama.Contains("outlet") || nama.Contains("semprot"))
                return child;
        }
        return null;
    }

    private Vector3 PosisiAkarBotolDenganMulutDiTitik(GameObject botol, Quaternion rotasi, Vector3 targetMulut)
    {
        if (botol == null) return targetMulut;
        Quaternion rotasiAwal = botol.transform.rotation;
        botol.transform.rotation = rotasi;
        Vector3 offsetMulut = PosisiMulutBotol(botol) - botol.transform.position;
        botol.transform.rotation = rotasiAwal;
        return targetMulut - offsetMulut;
    }

    private Vector3 PosisiTargetDropSpatula(int urutanSamping)
    {
        // Asam benzoat mengisi tiga lubang pertama secara mendatar pada baris ketiga.
        return PosisiLubangSpot(urutanSamping, 0, 0.001f);
    }

    private bool DropSpatulaKeTarget(Vector2 posisiLayar, Camera cam, GameObject model)
    {
        int barisSumber = BarisSumberAktif;
        int indeksSumber = IndeksSumberAktif;
        if (!sumberAsamDiletakkan || cawanAsamBenzoat == null || model == null ||
            sedangMenakarAsam || pendingKolomAsam >= 0 || hitungIsian[indeksSumber] >= 3)
            return false;

        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        int targetIndex = -1;
        float jarakTargetTerdekat = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {

            Vector3 posisiTarget = cam.WorldToScreenPoint(PosisiLubangSpot(i, barisSumber, 0.001f));
            if (posisiTarget.z <= 0f) continue;
            float jarak = Vector2.Distance(posisiLayar, new Vector2(posisiTarget.x, posisiTarget.y));
            if (jarak < jarakTargetTerdekat)
            {
                jarakTargetTerdekat = jarak;
                targetIndex = i;
            }
        }
        RectTransform target = targetDropSpatula != null && targetIndex >= 0 && targetIndex < targetDropSpatula.Length
            ? targetDropSpatula[targetIndex] : null;
        if (targetIndex < 0 || target == null || jarakTargetTerdekat > 48f || SpotSumberTerisi(barisSumber, targetIndex))
        { TampilSalah(); return false; }

        // Buat titik ujung terlebih dulu supaya animasi bisa menghitung lintasan
        // ujung spatula meskipun spatula baru saja diambil dari menu alat.
        model.SetActive(true);
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true)) renderer.enabled = true;
        MatikanBayangan(model);
        Transform holder = model.transform.Find("UjungSerbukAsam");
        if (holder != null)
            for (int i = holder.childCount - 1; i >= 0; i--) Destroy(holder.GetChild(i).gameObject);
        model.transform.SetParent(platTransform, true);
        PosisikanSpatulaDiCekungan(model, targetIndex);
        pendingKolomAsam = indeksSumber;
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
        model.transform.rotation = (kamera != null ? kamera.transform.rotation : platTransform.rotation) * objekSpatulaAsam.transform.localRotation;
        TegakkanSpatula(model, kamera);
        HadapkanCekunganSpatulaKeKamera(model, kamera);
        TerangkanSpatula(model);
        model.transform.localScale *= 0.65f;
        // Jangan skala ulang setelah drag: ukuran harus tetap sama dengan yang
        // terlihat ketika spatula diambil dari kartu alat.
        Transform tip = PastikanUjungSpatula(model);
        Bounds boundsPlateWorld;
        float jarakDiAtasPermukaan = AmbilBoundsRenderer(platTransform.gameObject, out boundsPlateWorld)
            ? Mathf.Clamp(boundsPlateWorld.extents.magnitude * 0.04f, 0.006f, 0.02f)
            : 0.01f;
        // Pakai offset dunia beberapa milimeter supaya spatula tidak tertutup mesh plate
        // saat model AR memiliki skala kecil.
        Vector3 targetTip = PosisiLubangSpot(baris, BarisSumberAktif, 0f) + platTransform.forward * jarakDiAtasPermukaan;
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

    private static Sprite BuatSpriteVisualTarget()
    {
        if (spriteVisualTarget != null) return spriteVisualTarget;
        const int ukuran = 32;
        Texture2D tekstur = new Texture2D(ukuran, ukuran, TextureFormat.RGBA32, false);
        tekstur.name = "RuntimeTargetRing";
        tekstur.hideFlags = HideFlags.HideAndDontSave;
        tekstur.filterMode = FilterMode.Bilinear;
        Color[] piksel = new Color[ukuran * ukuran];
        Vector2 pusat = new Vector2((ukuran - 1) * 0.5f, (ukuran - 1) * 0.5f);
        for (int y = 0; y < ukuran; y++)
            for (int x = 0; x < ukuran; x++)
            {
                float jarak = Vector2.Distance(new Vector2(x, y), pusat);
                float cincin = 1f - Mathf.Clamp01((Mathf.Abs(jarak - 12f) - 1f) * 2f);
                float titik = 1f - Mathf.Clamp01((jarak - 1.5f) * 2f);
                piksel[y * ukuran + x] = new Color(1f, 1f, 1f, Mathf.Max(cincin, titik));
            }
        tekstur.SetPixels(piksel);
        tekstur.Apply(false, true);
        spriteVisualTarget = Sprite.Create(tekstur, new Rect(0, 0, ukuran, ukuran), new Vector2(0.5f, 0.5f), 100f);
        spriteVisualTarget.name = "RuntimeTargetRingSprite";
        spriteVisualTarget.hideFlags = HideFlags.HideAndDontSave;
        return spriteVisualTarget;
    }

    private static Sprite BuatSpriteKotakTarget()
    {
        if (spriteKotakTarget == null)
        {
            spriteKotakTarget = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            spriteKotakTarget.name = "RuntimeTargetSquareSprite";
            spriteKotakTarget.hideFlags = HideFlags.HideAndDontSave;
        }
        return spriteKotakTarget;
    }
    private static void PastikanVisualTarget(RectTransform marker, bool kotak = false)
    {
        if (marker == null) return;
        Transform visual = marker.Find("VisualTargetDrop");
        UnityEngine.UI.Image image = visual != null ? visual.GetComponent<UnityEngine.UI.Image>() : marker.GetComponent<UnityEngine.UI.Image>();
        if (image == null) image = marker.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = kotak ? BuatSpriteKotakTarget() : BuatSpriteVisualTarget();
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.preserveAspect = true;
        image.color = kotak ? new Color(0.82f, 0.82f, 0.82f, 0.72f) : new Color(1f, 0.82f, 0.12f, 0.72f);
        image.raycastTarget = false;
        image.enabled = true;
    }
    private void PastikanTargetDropSpatula()
    {
        if (targetDropSpatula == null || targetDropSpatula.Length != 3)
            targetDropSpatula = new RectTransform[3];

        Canvas canvas = panelNotif != null
            ? panelNotif.GetComponentInParent<Canvas>(true)
            : FindObjectOfType<Canvas>(true);
        if (canvas == null) return;

        for (int i = 0; i < targetDropSpatula.Length; i++)
        {
            if (targetDropSpatula[i] != null) continue;
            GameObject marker = new GameObject("TargetDropSpotPlate_" + (i + 1), typeof(RectTransform), typeof(Image));
            marker.transform.SetParent(canvas.transform, false);
            targetDropSpatula[i] = marker.GetComponent<RectTransform>();
            targetDropSpatula[i].sizeDelta = new Vector2(30f, 30f);
            marker.SetActive(false);
        }
        PindahkanTargetSpatulaKeCanvas();
    }

    private void PerbaruiTargetDropSpatula()
    {
        PastikanTargetDropSpatula();
        int barisSumber = BarisSumberAktif;
        GameObject panelSoal = PanelSoalSumberAktif;
        bool soalAktif = panelSoal != null && panelSoal.activeInHierarchy;
        bool mgclAktif = fase == FaseP2.IsiKolom2;
        bool sumberSiap = FaseSumberAktif && platTransform != null && soalAktif &&
                          (mgclAktif || (sumberAsamDiletakkan && cawanAsamBenzoat != null)) &&
                          hitungIsian[IndeksSumberAktif] < 3;
        bool petroleumTargetTampil = fase == FaseP2.IsiKolom3 && platTransform != null &&
                                    soal4 != null && soal4.activeInHierarchy && hitungIsian[2] < 3;
        bool aquadesTargetTampil = fase == FaseP2.CampurBaris1 && platTransform != null &&
                                   soal5 != null && soal5.activeInHierarchy;
        bool etanolTargetTampil = fase == FaseP2.CampurBaris2 && platTransform != null &&
                                  soal6 != null && soal6.activeInHierarchy && sumberEtanolDiletakkan && cawanEtanolSumber != null;
        bool baris3Aktif = fase == FaseP2.CampurBaris3 && platTransform != null &&
                           soal7 != null && soal7.activeInHierarchy;
        bool airBaris3Tampil = baris3Aktif && !SemuaAquadesBaris3Diteteskan();
        bool etanolBaris3Tampil = baris3Aktif && SemuaAquadesBaris3Diteteskan() &&
                                  sumberEtanolBaris3Diletakkan && cawanEtanolBaris3Sumber != null &&
                                  !SemuaEtanolBaris3Diteteskan();
        bool adukBaris3Tampil = baris3Aktif && SemuaPelarutBaris3Diteteskan() && !barisSelesai[2];
        if (targetDropSpatula != null)
        {
            for (int i = 0; i < targetDropSpatula.Length; i++)
            {
                RectTransform marker = targetDropSpatula[i];
                if (marker == null) continue;
                bool lubangTerisi = sumberSiap
                    ? SpotSumberTerisi(barisSumber, i)
                    : petroleumTargetTampil
                        ? grid[i, BarisPetroleum] != null && grid[i, BarisPetroleum].terisi
                        : etanolTargetTampil
                            ? SemuaEtanolSudahDiteteskan() && grid[i, 1] != null && grid[i, 1].campuran[1]
                            : aquadesTargetTampil && (SemuaAquadesSudahDiteteskan()
                                ? grid[i, 0] != null && grid[i, 0].campuran[0]
                                : aquadesDitetes[i]);
                if (sumberSiap || petroleumTargetTampil || aquadesTargetTampil || etanolTargetTampil ||
                    airBaris3Tampil || etanolBaris3Tampil || adukBaris3Tampil)
                {
                    PastikanVisualTarget(marker, false);
                    marker.sizeDelta = new Vector2(36f, 36f);
                    int barisTarget = petroleumTargetTampil ? BarisPetroleum :
                                      etanolTargetTampil ? 1 :
                                      aquadesTargetTampil ? 0 :
                                      (airBaris3Tampil || etanolBaris3Tampil || adukBaris3Tampil) ? 2 : barisSumber;
                    PosisikanMarkerDiLubang(marker, i, barisTarget, 0.001f);
                }
                int targetAduk = SemuaAquadesSudahDiteteskan() ? LubangAquadesBerikutnyaBelumDiaduk() : -1;
                int targetAdukEtanol = SemuaEtanolSudahDiteteskan() ? LubangEtanolBerikutnyaBelumDiaduk() : -1;
                bool sudahBaris3 = grid[i, 2] != null && grid[i, 2].campuran[2];
                bool tampilkanTarget = (sumberSiap && pendingKolomAsam < 0 && !lubangTerisi) ||
                                       (petroleumTargetTampil && !lubangTerisi) ||
                                       (etanolTargetTampil && !lubangTerisi &&
                                        (SemuaEtanolSudahDiteteskan() ? i == targetAdukEtanol : !etanolDitetes[i])) ||
                                       (aquadesTargetTampil && !lubangTerisi &&
                                        (SemuaAquadesSudahDiteteskan() ? i == targetAduk : !aquadesDitetes[i])) ||
                                       (airBaris3Tampil && !aquadesBaris3Ditetes[i] && !tetesanBaris3Berlangsung[i]) ||
                                       (etanolBaris3Tampil && !etanolBaris3Ditetes[i] && !tetesanBaris3Berlangsung[i]) ||
                                       (adukBaris3Tampil && !sudahBaris3 && i == LubangBaris3BerikutnyaBelumDiaduk());
                marker.gameObject.SetActive(tampilkanTarget);
                float pulse = tampilkanTarget ? 1f + 0.10f * Mathf.Sin(Time.unscaledTime * 4f + i * 0.35f) : 1f;
                marker.localScale = Vector3.one * pulse;
            }
        }
        bool targetVoltmeterTampil = baris3Aktif && barisSelesai[2] && !voltmeterDipasang;
        if (targetVoltmeterTampil && targetDropSpatula != null && targetDropSpatula.Length > 0 && targetDropSpatula[0] != null)
        {
            RectTransform marker = targetDropSpatula[0];
            Bounds plate = AmbilBoundsPlateLokal();
            Vector3 posisiTarget = platTransform.TransformPoint(new Vector3(plate.extents.x * 0.58f, plate.extents.y * 0.58f, 0.018f));
            PastikanVisualTarget(marker, false);
            marker.sizeDelta = new Vector2(42f, 42f);
            PosisikanMarkerDiPosisiDunia(marker, posisiTarget);
            marker.gameObject.SetActive(true);
            marker.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f));
        }
        PerbaruiMarkerProbeSampel(baris3Aktif && barisSelesai[2] && voltmeterDipasang);
        if (panelPilihanSpatula != null)
            panelPilihanSpatula.SetActive(sumberSiap && pendingKolomAsam >= 0 && !sedangMenakarAsam);
    }

    private void PerbaruiMarkerProbeSampel(bool tahapUjiAktif)
    {
        if (markerProbeSampel == null || markerProbeSampel.Length != 9)
            markerProbeSampel = new RectTransform[9];
        Canvas canvas = panelNotif != null
            ? panelNotif.GetComponentInParent<Canvas>(true)
            : FindObjectOfType<Canvas>(true);
        if (canvas == null)
        {
            for (int i = 0; i < markerProbeSampel.Length; i++)
                if (markerProbeSampel[i] != null) markerProbeSampel[i].gameObject.SetActive(false);
            return;
        }
        Transform root = canvas.rootCanvas.transform;
        for (int k = 0; k < 3; k++)
        {
            for (int b = 0; b < 3; b++)
            {
                int index = k * 3 + b;
                RectTransform marker = markerProbeSampel[index];
                if (marker == null)
                {
                    GameObject go = new GameObject("TargetProbeKonduktivitas_" + (index + 1), typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(root, false);
                    marker = go.GetComponent<RectTransform>();
                    marker.anchorMin = marker.anchorMax = new Vector2(0.5f, 0.5f);
                    marker.pivot = new Vector2(0.5f, 0.5f);
                    marker.sizeDelta = new Vector2(34f, 34f);
                    marker.SetAsLastSibling();
                    markerProbeSampel[index] = marker;
                }
                else if (marker.parent != root)
                {
                    marker.SetParent(root, false);
                }

                bool sampelAda = grid[k, b] != null && grid[k, b].terisi && grid[k, b].silinder != null;
                bool tampil = tahapUjiAktif && sampelAda && !sampelDiuji[k, b];
                if (tampil)
                {
                    PastikanVisualTarget(marker, false);
                    PosisikanMarkerDiLubang(marker, k, b, 0.002f);
                    marker.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.unscaledTime * 4f + index * 0.25f));
                }
                marker.gameObject.SetActive(tampil);
            }
        }
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
            // Pastikan lingkaran target tidak tertutup panel atau elemen UI lain.
            marker.SetAsLastSibling();
            marker.localRotation = Quaternion.identity;
        }
    }

    private void PosisikanMarkerDiLubang(RectTransform marker, int kolom, int baris, float offsetNormal)
    {
        if (marker == null || marker.parent == null) return;
        PosisikanMarkerDiPosisiDunia(marker, PosisiLubangSpot(kolom, baris, offsetNormal));
    }

    private void PosisikanMarkerDiPosisiDunia(RectTransform marker, Vector3 world)
    {
        if (marker == null || marker.parent == null) return;
        Canvas canvas = marker.GetComponentInParent<Canvas>();
        if (canvas == null) return;
        Camera cam = Camera.main;
        if (cam == null) cam = canvas.rootCanvas.worldCamera;
        if (cam == null) cam = FindObjectOfType<Camera>();
        if (cam == null) return;
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
        int indeksSumber = pendingKolomAsam >= 0 ? pendingKolomAsam : IndeksSumberAktif;
        int barisSumber = indeksSumber == 0 ? 0 : 1;
        Vector3 lubang = PosisiLubangSpot(baris, barisSumber, 0.001f);
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
            Vector3 targetLubang = lubang + platTransform.forward * 0.006f;
            posisiTuang = targetLubang - rotTuang * offsetUjungLokal;
        }

        if (spatula != null)
        {
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiAmbil, rotAmbil, 0.55f));
            // Turunkan ujung sendok masuk ke serbuk, sapu melintasi cawan, lalu angkat.
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopDalam, rotAmbil, 0.24f));
            // Serbuk langsung terlihat menempel ketika ujung menyentuh bahan,
            // lalu ikut tersapu dan terangkat bersama spatula.
            if (indeksSumber == 0) TambahkanSerbukKeSpatula(spatula);
            else TambahkanKristalMgCl2KeSpatula(spatula);
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopGeser, rotAmbil, 0.34f));
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiScoopAngkat, rotAmbil, 0.28f));
            yield return StartCoroutine(GerakkanSpatula(spatula, posisiTuang, rotTuang, 0.72f));
        }

        ujung = spatula != null ? spatula.transform.Find("UjungSerbukAsam") : null;
        Vector3 ke = PosisiLubangSpot(baris, barisSumber, 0f) + platTransform.forward * 0.004f;
        Transform massaSerbuk = ujung != null ? ujung.Find(indeksSumber == 0 ? "SerbukAsamBenzoat" : "SerbukMgCl2") : null;
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
            if (indeksSumber == 1) gelembungMgCl2.Add(massaSerbuk.gameObject);
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
            Destroy(spatula);
            spatulaTerakhirDiTanda = null;
        }

        if (indeksSumber == 0) TambahIsian(0, baris);
        else TambahIsianMgCl2(baris, barisSumber);
        pendingModelAsam = null;
        pendingKolomAsam = pendingBarisAsam = -1;
        sedangMenakarAsam = false;
        PerbaruiTargetDropSpatula();

    }

    /// <summary>Voltmeter diterima setelah ketiga larutan baris ketiga selesai diaduk.</summary>
    public bool DropTespen(Vector2 posLayar, Camera cam, GameObject model = null)
    {
        if (fase != FaseP2.CampurBaris3 || !barisSelesai[2] || voltmeterDipasang || model == null)
            return false;
        if (cam == null) cam = Camera.main;
        if (cam == null || !PosisiLayarDiAtasPlat(posLayar, cam)) { TampilSalah(); return false; }

        foreach (Collider c in model.GetComponentsInChildren<Collider>(true)) Destroy(c);
        foreach (Rigidbody rb in model.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
        foreach (Camera c in model.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (Light l in model.GetComponentsInChildren<Light>(true)) l.enabled = false;
        MatikanBayangan(model);
        model.transform.SetParent(platTransform, true);
        Bounds plate = AmbilBoundsPlateLokal();
        Vector3 target = platTransform.TransformPoint(new Vector3(plate.extents.x * 0.58f, plate.extents.y * 0.58f, 0.018f));
        Bounds boundsModel;
        if (AmbilBoundsRenderer(model, out boundsModel))
            model.transform.position += target - boundsModel.center;
        else
            model.transform.position = target;

        voltmeterAktif = model;
        voltmeterDipasang = true;
        BuatPanelProbeVoltmeter();
        PerbaruiTargetDropSpatula();
        return true;
    }

    private void BuatPanelProbeVoltmeter()
    {
        Canvas canvas = panelNotif != null
            ? panelNotif.GetComponentInParent<Canvas>(true)
            : FindObjectOfType<Canvas>(true);
        if (canvas == null) return;
        Transform root = canvas.rootCanvas.transform;

        if (panelProbeVoltmeter != null) Destroy(panelProbeVoltmeter);
        panelProbeVoltmeter = new GameObject("PanelProbeKonduktivitas", typeof(RectTransform), typeof(Image));
        panelProbeVoltmeter.transform.SetParent(root, false);
        RectTransform panel = panelProbeVoltmeter.GetComponent<RectTransform>();
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -72f);
        panel.sizeDelta = new Vector2(340f, 132f);
        Image latar = panelProbeVoltmeter.GetComponent<Image>();
        latar.color = new Color(0.035f, 0.07f, 0.12f, 0.94f);
        latar.raycastTarget = false;

        BuatLabelProbe(panelProbeVoltmeter.transform, "Geser probe merah dan hitam ke larutan", 15, new Vector2(0f, -24f), new Vector2(320f, 28f));
        teksStatusProbe = BuatLabelProbe(panelProbeVoltmeter.transform, "Teruji: 0 / 9 sampel", 12, new Vector2(0f, -112f), new Vector2(300f, 18f));
        tokenProbeBaris3[0] = BuatTokenProbe(panelProbeVoltmeter.transform, 0, "MERAH", new Color(0.88f, 0.16f, 0.17f, 1f), new Vector2(-72f, -72f));
        tokenProbeBaris3[1] = BuatTokenProbe(panelProbeVoltmeter.transform, 1, "HITAM", new Color(0.16f, 0.19f, 0.23f, 1f), new Vector2(72f, -72f));
        posisiHomeProbe[0] = tokenProbeBaris3[0].anchoredPosition;
        posisiHomeProbe[1] = tokenProbeBaris3[1].anchoredPosition;
    }

    private TMP_Text BuatLabelProbe(Transform parent, string teks, int ukuran, Vector2 posisi, Vector2 ukuranLabel)
    {
        GameObject label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(parent, false);
        RectTransform rt = label.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuranLabel;
        TMP_Text tmp = label.GetComponent<TMP_Text>();
        tmp.text = teks;
        tmp.fontSize = ukuran;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private RectTransform BuatTokenProbe(Transform parent, int warna, string label, Color warnaToken, Vector2 posisi)
    {
        GameObject token = new GameObject("Probe" + label, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(ProbeKonduktivitasDrag));
        token.transform.SetParent(parent, false);
        RectTransform rt = token.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posisi;
        rt.sizeDelta = new Vector2(46f, 30f);
        Image image = token.GetComponent<Image>();
        image.color = warnaToken;
        image.raycastTarget = true;

        GameObject text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        text.transform.SetParent(token.transform, false);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        TMP_Text tmp = text.GetComponent<TMP_Text>();
        tmp.text = label;
        tmp.fontSize = 8;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        ProbeKonduktivitasDrag drag = token.GetComponent<ProbeKonduktivitasDrag>();
        drag.Inisialisasi(this, warna == 0);
        return rt;
    }

    public void ProbeBaris3Dilepas(bool merah, Vector2 posisiLayar, RectTransform token)
    {
        if (!voltmeterDipasang || !barisSelesai[2]) return;
        Camera cam = Camera.main;
        if (cam == null) { TampilSalah(); return; }
        int k, b;
        if (!CariSpot(posisiLayar, cam, out k, out b) ||
            grid[k, b] == null || !grid[k, b].terisi || grid[k, b].silinder == null)
        {
            TampilSalah();
            return;
        }
        Vector3 pusatLayar3D = cam.WorldToScreenPoint(PosisiLubangSpot(k, b, 0.002f));
        if (pusatLayar3D.z <= 0f || Vector2.Distance(posisiLayar, new Vector2(pusatLayar3D.x, pusatLayar3D.y)) > 48f)
        {
            TampilSalah();
            return;
        }
        if (sampelDiuji[k, b]) return;

        if (merah) probeMerahBaris3[k, b] = true;
        else probeHitamBaris3[k, b] = true;
        StartCoroutine(AnimasiGarisProbe(k, b, merah, token));
        if (probeMerahBaris3[k, b] && probeHitamBaris3[k, b])
        {
            sampelDiuji[k, b] = true;
            TampilkanToastBerhasilUji();
            PerbaruiStatusProbe();
            PerbaruiMarkerProbeSampel(true);
            if (Baris3Selesai())
            {
                fase = FaseP2.Selesai;
                if (panelProbeVoltmeter != null) panelProbeVoltmeter.SetActive(false);
                PerbaruiTargetDropSpatula();
            }
        }
        else
        {
            PerbaruiStatusProbe();
        }
    }

    private void PerbaruiStatusProbe()
    {
        if (teksStatusProbe == null) return;
        int jumlah = 0;
        for (int k = 0; k < 3; k++)
            for (int b = 0; b < 3; b++)
                if (sampelDiuji[k, b]) jumlah++;
        teksStatusProbe.text = "Teruji: " + jumlah + " / 9 sampel";
    }

    private IEnumerator AnimasiGarisProbe(int k, int b, bool merah, RectTransform token)
    {
        if (token == null || platTransform == null) yield break;
        Canvas canvas = token.GetComponentInParent<Canvas>();
        if (canvas == null) yield break;
        RectTransform root = canvas.rootCanvas.transform as RectTransform;
        if (root == null) yield break;
        Camera uiCam = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
        Camera cam = Camera.main;
        if (cam == null) yield break;

        int indexProbe = merah ? 0 : 1;
        Vector2 posisiDrop = token.anchoredPosition;
        token.anchoredPosition = posisiHomeProbe[indexProbe];
        Vector2 awalLayar = RectTransformUtility.WorldToScreenPoint(uiCam, token.position);
        token.anchoredPosition = posisiDrop;
        Vector3 targetLayar3 = cam.WorldToScreenPoint(PosisiLubangSpot(k, b, 0.002f));
        Vector2 awal, akhir;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, awalLayar, uiCam, out awal) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(root, new Vector2(targetLayar3.x, targetLayar3.y), uiCam, out akhir))
            yield break;

        GameObject kabel = new GameObject("KabelProbeAnimasi", typeof(RectTransform), typeof(Image));
        kabel.transform.SetParent(root, false);
        kabel.transform.SetAsFirstSibling();
        RectTransform garis = kabel.GetComponent<RectTransform>();
        garis.anchorMin = garis.anchorMax = new Vector2(0.5f, 0.5f);
        garis.pivot = new Vector2(0f, 0.5f);
        garis.anchoredPosition = awal;
        Vector2 delta = akhir - awal;
        garis.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        garis.sizeDelta = new Vector2(0f, merah ? 4f : 3f);
        Image image = kabel.GetComponent<Image>();
        image.color = merah ? new Color(0.92f, 0.12f, 0.14f, 0.9f) : new Color(0.08f, 0.10f, 0.13f, 0.95f);
        image.raycastTarget = false;

        float durasi = 0.22f;
        float waktu = 0f;
        while (waktu < durasi && kabel != null)
        {
            waktu += Time.unscaledDeltaTime;
            garis.sizeDelta = new Vector2(delta.magnitude * Ease(waktu / durasi), garis.sizeDelta.y);
            yield return null;
        }
        if (kabel != null) Destroy(kabel);
    }

    private void TampilkanToastBerhasilUji()
    {
        Canvas canvas = panelNotif != null
            ? panelNotif.GetComponentInParent<Canvas>(true)
            : FindObjectOfType<Canvas>(true);
        if (canvas == null) return;
        if (panelToastUji == null)
        {
            panelToastUji = new GameObject("ToastUjiKonduktivitas", typeof(RectTransform), typeof(Image));
            panelToastUji.transform.SetParent(canvas.rootCanvas.transform, false);
            RectTransform rt = panelToastUji.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(224f, 68f);
            panelToastUji.GetComponent<Image>().color = new Color(0.035f, 0.07f, 0.12f, 0.96f);
            teksToastUji = BuatLabelProbe(panelToastUji.transform, "", 14, new Vector2(0f, -4f), new Vector2(212f, 60f));
        }
        RectTransform toastRect = panelToastUji.GetComponent<RectTransform>();
        toastRect.anchorMin = toastRect.anchorMax = new Vector2(0.5f, 0.5f);
        toastRect.pivot = new Vector2(0.5f, 0.5f);
        toastRect.sizeDelta = new Vector2(224f, 68f);
        Vector2 posisiToastLayar = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f - 32f);
        Camera cam = Camera.main;
        if (posisiToastUjiEtanolTersimpan && cam != null)
        {
            Vector3 screen = cam.WorldToScreenPoint(posisiToastUjiEtanol + cam.transform.up * 0.045f);
            if (screen.z > 0f)
            {
                posisiToastLayar = new Vector2(screen.x, screen.y - 14f);
                posisiToastLayar.x = Mathf.Clamp(posisiToastLayar.x, 120f, Screen.width - 120f);
                posisiToastLayar.y = Mathf.Clamp(posisiToastLayar.y, 72f, Screen.height - 72f);
            }
        }
        RectTransform root = canvas.rootCanvas.transform as RectTransform;
        Camera uiCam = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
        Vector2 posisiLokal;
        if (root != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(root, posisiToastLayar, uiCam, out posisiLokal))
            toastRect.anchoredPosition = posisiLokal;
        teksToastUji.text = "Berhasil\n\ndiuji";
        panelToastUji.SetActive(true);
        if (tutupToastUji != null) StopCoroutine(tutupToastUji);
        tutupToastUji = StartCoroutine(TutupToastUji());
    }

    private IEnumerator TutupToastUji()
    {
        yield return new WaitForSecondsRealtime(durasiNotif);
        if (panelToastUji != null) panelToastUji.SetActive(false);
        tutupToastUji = null;
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
        for (int k = 0; k < 3; k++)
            for (int b = 0; b < 3; b++)
                if (grid[k, b] == null || !grid[k, b].terisi || grid[k, b].silinder == null || !sampelDiuji[k, b]) return false;
        return true;
    }

    /// <summary>Fase P2 saat ini (Idle = Percobaan 2 belum dimulai).</summary>
    public FaseP2 FaseAktif { get { return fase; } }

    public bool AturanKartuSoal2Aktif { get { return FaseSumberAktif || fase == FaseP2.IsiKolom3 || fase == FaseP2.CampurBaris1 || fase == FaseP2.CampurBaris2 || fase == FaseP2.CampurBaris3; } }

    public bool BolehAmbilKartuSoal2(GameObject item)
    {
        if (!AturanKartuSoal2Aktif || item == null) return false;
        CardTahanGeser3D kartu = item.GetComponent<CardTahanGeser3D>();
        string namaItem = kartu != null && !string.IsNullOrEmpty(kartu.namaBahanSP) ? kartu.namaBahanSP : item.name;
        if (fase == FaseP2.IsiKolom1 && item.name == "Asam Benzoat") return !sumberAsamDiletakkan && cawanAsamBenzoat == null;
        if (fase == FaseP2.IsiKolom2 && item.name == "MGCL") return !sumberAsamDiletakkan && cawanAsamBenzoat == null;
        if (fase == FaseP2.IsiKolom3 && item.name == "Petroleum Eter") return !sumberAsamDiletakkan && cawanAsamBenzoat == null;
        if (fase == FaseP2.IsiKolom3 && item.name == "PipetTetes") return sumberAsamDiletakkan && cawanAsamBenzoat != null && pendingLubangPetroleum < 0 && hitungIsian[2] < 3;
        if (fase == FaseP2.CampurBaris1 && item.name == "Aquades") return !SemuaAquadesSudahDiteteskan() && AdaAquadesBelumDiteteskan();
        if (fase == FaseP2.CampurBaris2 && item.name == "Etanol") return !sumberEtanolDiletakkan && cawanEtanolSumber == null;
        if (fase == FaseP2.CampurBaris2 && item.name == "PipetTetes") return sumberEtanolDiletakkan && cawanEtanolSumber != null && pendingLubangEtanol < 0 && AdaEtanolBelumDiteteskan();

        bool itemTusukGigi = namaItem == "TusukGigi" || namaItem == "Tusuk Gigi" ||
                             (kartu != null && kartu.namaBahanSP == "TusukGigi");
        if (fase == FaseP2.CampurBaris1 && itemTusukGigi) return SemuaAquadesSudahDiteteskan() && AdaAquadesBelumDiaduk();
        if (fase == FaseP2.CampurBaris2 && itemTusukGigi) return SemuaEtanolSudahDiteteskan() && AdaEtanolBelumDiaduk();
        if (fase == FaseP2.CampurBaris3)
        {
            if (itemTusukGigi) return SemuaPelarutBaris3Diteteskan() && LubangBaris3BerikutnyaBelumDiaduk() >= 0;
            if (namaItem == "Tes Konduktor" || namaItem == "TesKonduktor")
                return barisSelesai[2] && !voltmeterDipasang;
            if (namaItem == "PipetTetes")
                return SemuaAquadesBaris3Diteteskan() && sumberEtanolBaris3Diletakkan &&
                       cawanEtanolBaris3Sumber != null && !SemuaEtanolBaris3Diteteskan() && AdaEtanolBelumBaris3();
            if (namaItem == "Aquades") return !SemuaAquadesBaris3Diteteskan() && AdaAquadesBelumBaris3();
            if (namaItem == "Etanol") return SemuaAquadesBaris3Diteteskan() && !SemuaEtanolBaris3Diteteskan() && !sumberEtanolBaris3Diletakkan && cawanEtanolBaris3Sumber == null;
        }
        if (item.name == "Spatula") return BolehAmbilSpatulaAsam();
        return false;
    }
    private bool AdaAquadesBelumDiteteskan()
    {
        for (int k = 0; k < 3; k++) if (!aquadesDitetes[k]) return true;
        return false;
    }

    private bool SemuaAquadesSudahDiteteskan()
    {
        return aquadesDitetes != null && aquadesDitetes.Length == 3 && aquadesDitetes[0] && aquadesDitetes[1] && aquadesDitetes[2];
    }

    private bool AdaAquadesBelumDiaduk()
    {
        for (int k = 0; k < 3; k++)
            if (aquadesDitetes[k] && grid[k, 0] != null && !grid[k, 0].campuran[0]) return true;
        return false;
    }

    private int LubangAquadesBerikutnyaBelumDiaduk()
    {
        for (int k = 0; k < 3; k++)
            if (aquadesDitetes[k] && grid[k, 0] != null && !grid[k, 0].campuran[0]) return k;
        return -1;
    }

    private int JumlahAquadesSelesai()
    {
        int jumlah = 0;
        for (int k = 0; k < 3; k++) if (aquadesDitetes[k]) jumlah++;
        return jumlah;
    }

    private bool AdaEtanolBelumDiteteskan()
    {
        for (int k = 0; k < 3; k++) if (!etanolDitetes[k]) return true;
        return false;
    }

    private bool SemuaEtanolSudahDiteteskan()
    {
        return etanolDitetes != null && etanolDitetes.Length == 3 && etanolDitetes[0] && etanolDitetes[1] && etanolDitetes[2];
    }

    private bool AdaEtanolBelumDiaduk()
    {
        for (int k = 0; k < 3; k++)
            if (etanolDitetes[k] && grid[k, 1] != null && !grid[k, 1].campuran[1]) return true;
        return false;
    }

    private int LubangEtanolBerikutnyaBelumDiaduk()
    {
        for (int k = 0; k < 3; k++)
            if (etanolDitetes[k] && grid[k, 1] != null && !grid[k, 1].campuran[1]) return k;
        return -1;
    }

    private int JumlahEtanolSelesai()
    {
        int jumlah = 0;
        for (int k = 0; k < 3; k++) if (etanolDitetes[k]) jumlah++;
        return jumlah;
    }

    private void TampilkanInstruksiAduk()
    {
        // Panduan aduk ditunjukkan oleh target pada lubang; panel hijau tidak dipakai.
        instruksiAdukAktif = false;
        if (panelNotif != null) panelNotif.SetActive(false);
        PerbaruiTargetDropSpatula();
    }

    private void TampilkanInstruksiAdukEtanol()
    {
        // Tusuk gigi tetap diwajibkan, tetapi tanpa panel notifikasi hijau.
        instruksiAdukAktif = false;
        if (panelNotif != null) panelNotif.SetActive(false);
        PerbaruiTargetDropSpatula();
    }
    private IEnumerator AdukSatuSampelAquades(int k, Camera cam, GameObject tusuk)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || tusuk == null || !SemuaAquadesSudahDiteteskan() ||
            k != LubangAquadesBerikutnyaBelumDiaduk())
        {
            if (tusuk != null) Destroy(tusuk);
            animAktif = null;
            yield break;
        }

        instruksiAdukAktif = false;
        if (panelNotif != null) panelNotif.SetActive(false);
        foreach (Collider c in tusuk.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody rb in tusuk.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
        foreach (Camera c in tusuk.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (Light l in tusuk.GetComponentsInChildren<Light>(true)) l.enabled = false;
        MatikanBayangan(tusuk);
        tusuk.transform.SetParent(platTransform, true);
        AturUkuranTusukGigi(tusuk);

        yield return AnimAdukAquades(k, tusuk, cam);
        string hasil = hasilKelarutan[k, 0];
        Color warnaAir = new Color(0.55f, 0.75f, 1f, 0.62f);
        if (k == 1)
        {
            HapusKristalMgCl2Soal5();
            HapusSerbukMgCl2Lama();
            TampilkanMgCl2LarutSeragam();
        }
        else TampilSpot(k, 0, warnaHasil[k, 0], false, true);
        if (hasil == "Tidak larut") yield return EfekTidakLarut(k, 0, warnaAir, warnaHasil[k, 0]);
        else if (hasil == "Sedikit larut") yield return EfekSedikitLarut(k, 0, warnaHasil[k, 0]);
        grid[k, 0].campuran[0] = true;
        if (tusuk != null) Destroy(tusuk);
        if (AdaAquadesBelumDiaduk())
            TampilkanInstruksiAduk();
        else
        {
            barisSelesai[0] = true;
            Notif("Berhasil dicampur. Semua hasil kelarutan sudah terlihat.");
        }
        animAktif = null;
        PerbaruiTargetDropSpatula();
    }

    private IEnumerator AdukSatuSampelEtanol(int k, Camera cam, GameObject tusuk)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || tusuk == null || !SemuaEtanolSudahDiteteskan() ||
            k != LubangEtanolBerikutnyaBelumDiaduk())
        {
            if (tusuk != null) Destroy(tusuk);
            animAktif = null;
            yield break;
        }

        instruksiAdukAktif = false;
        if (panelNotif != null) panelNotif.SetActive(false);
        foreach (Collider c in tusuk.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (Rigidbody rb in tusuk.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
        foreach (Camera c in tusuk.GetComponentsInChildren<Camera>(true)) c.enabled = false;
        foreach (Light l in tusuk.GetComponentsInChildren<Light>(true)) l.enabled = false;
        MatikanBayangan(tusuk);
        tusuk.transform.SetParent(platTransform, true);
        AturUkuranTusukGigi(tusuk);

        yield return AnimAdukAquades(k, tusuk, cam, 1);
        string hasil = hasilKelarutan[k, 1];
        TampilSpot(k, 1, warnaHasil[k, 1], false, true);
        AturKekasaranCairanEtanol(grid[k, 1].mat);
        grid[k, 1].campuran[1] = true;
        if (tusuk != null) Destroy(tusuk);

        string namaSampel = k == 0 ? "Asam benzoat" : k == 1 ? "MgCl2" : "Petroleum eter";
        Notif("Berhasil dicampur: " + namaSampel + " + etanol = " + hasil + ".");
        if (AdaEtanolBelumDiaduk())
            TampilkanInstruksiAdukEtanol();
        else
        {
            barisSelesai[1] = true;
            Notif("Berhasil dicampur. Ketiga sampel + etanol: Larut.");
        }
        animAktif = null;
        PerbaruiTargetDropSpatula();
    }

    private void AturUkuranTusukGigi(GameObject tusuk)
    {
        if (tusuk == null || platTransform == null) return;
        Bounds bounds;
        if (!AmbilBoundsRenderer(tusuk, out bounds)) return;
        float panjangSaatIni = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        Bounds boundsPlate = AmbilBoundsPlateLokal();
        float skalaPlate = Mathf.Max(platTransform.lossyScale.x, Mathf.Max(platTransform.lossyScale.y, platTransform.lossyScale.z));
        float panjangTarget = Mathf.Min(boundsPlate.extents.x, boundsPlate.extents.y) * 0.48f * skalaPlate;
        if (panjangSaatIni > 0.0001f)
            tusuk.transform.localScale *= Mathf.Clamp(panjangTarget / panjangSaatIni, 0.01f, 100f);
    }

    private IEnumerator AnimAdukAquades(int k, GameObject tusuk, Camera cam, int baris = 0)
    {
        if (tusuk == null || platTransform == null) yield break;
        Quaternion rotasiPrefab = objekTusukGigi != null ? objekTusukGigi.transform.localRotation : Quaternion.identity;
        Quaternion rotasiAduk = (cam != null ? cam.transform.rotation : platTransform.rotation) * rotasiPrefab;
        tusuk.transform.rotation = rotasiAduk;
        Bounds bounds;
        float setengahPanjang = AmbilBoundsRenderer(tusuk, out bounds)
            ? Mathf.Abs(cam.transform.up.x) * bounds.extents.x + Mathf.Abs(cam.transform.up.y) * bounds.extents.y + Mathf.Abs(cam.transform.up.z) * bounds.extents.z
            : Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.2f;
        Vector3 offsetPusat = AmbilBoundsRenderer(tusuk, out bounds) ? bounds.center - tusuk.transform.position : Vector3.zero;
        Vector3 permukaan = PosisiLubangSpot(k, baris, 0.006f);
        Vector3 posisiAtas = permukaan + cam.transform.up * setengahPanjang * 1.05f - offsetPusat;
        Vector3 posisiMasuk = permukaan + cam.transform.up * setengahPanjang * 0.42f - offsetPusat;
        yield return GerakkanPipetPetroleum(tusuk, cam, posisiAtas, rotasiAduk, 0.24f);
        yield return GerakkanPipetPetroleum(tusuk, cam, posisiMasuk, rotasiAduk, 0.2f);

        float radius = Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.025f;
        for (int putaran = 0; putaran < 3; putaran++)
        {
            float waktu = 0f;
            while (waktu < 0.42f)
            {
                waktu += Time.deltaTime;
                float sudut = (putaran + Ease(waktu / 0.42f)) * 360f * Mathf.Deg2Rad;
                Vector3 geser = platTransform.TransformVector(new Vector3(Mathf.Cos(sudut) * radius, Mathf.Sin(sudut) * radius, 0f));
                tusuk.transform.position = posisiMasuk + geser;
                tusuk.transform.rotation = Quaternion.AngleAxis(Mathf.Sin(sudut) * 9f, cam.transform.forward) * rotasiAduk;
                yield return null;
            }
        }
        yield return GerakkanPipetPetroleum(tusuk, cam, posisiAtas, rotasiAduk, 0.18f);
    }

    public bool BolehAmbilKartuPlateTetes(GameObject item)
    {
        if (fase != FaseP2.Idle || soal1 == null || !soal1.activeInHierarchy || item == null) return false;
        CardTahanGeser3D kartu = item.GetComponent<CardTahanGeser3D>();
        return kartu != null && kartu.namaBahanSP == "PlateTetes";
    }

    public void AktifkanFase(FaseP2 f)
    {
        if((f==FaseP2.IsiKolom1||f==FaseP2.IsiKolom2)&&fase!=f){sumberAsamDiletakkan=false;if(cawanAsamBenzoat!=null){Destroy(cawanAsamBenzoat);cawanAsamBenzoat=null;}}
        if (f == FaseP2.CampurBaris2 && fase != f)
        {
            etanolDitetes = new bool[3];
            barisSelesai[1] = false;
            pendingLubangEtanol = -1;
            sumberEtanolDiletakkan = false;
            if (cawanEtanolSumber != null) { Destroy(cawanEtanolSumber); cawanEtanolSumber = null; }
            for (int k = 0; k < 3; k++)
                if (grid[k, 1] != null) grid[k, 1].campuran[1] = false;
        }
        if (f == FaseP2.CampurBaris3 && fase != f)
        {
            aquadesBaris3Ditetes = new bool[3];
            etanolBaris3Ditetes = new bool[3];
            tetesanBaris3Berlangsung = new bool[3];
            sumberEtanolBaris3Diletakkan = false;
            if (cawanEtanolBaris3Sumber != null) { Destroy(cawanEtanolBaris3Sumber); cawanEtanolBaris3Sumber = null; }
            sumberEtanolDiletakkan = false;
            if (cawanEtanolSumber != null) { Destroy(cawanEtanolSumber); cawanEtanolSumber = null; }
            probeMerahBaris3 = new bool[3, 3];
            probeHitamBaris3 = new bool[3, 3];
            posisiToastUjiEtanolTersimpan = false;
            barisSelesai[2] = false;
            voltmeterDipasang = false;
            if (voltmeterAktif != null) Destroy(voltmeterAktif);
            voltmeterAktif = null;
            sampelDiuji = new bool[3, 3];
            if (panelProbeVoltmeter != null) { Destroy(panelProbeVoltmeter); panelProbeVoltmeter = null; }
            if (tutupToastUji != null) { StopCoroutine(tutupToastUji); tutupToastUji = null; }
            if (panelToastUji != null) { Destroy(panelToastUji); panelToastUji = null; teksToastUji = null; }
            for (int k = 0; k < 3; k++)
            {
                if (grid[k, 2] != null)
                {
                    if (grid[k, 2].silinder != null) Destroy(grid[k, 2].silinder);
                    if (grid[k, 2].mat != null) Destroy(grid[k, 2].mat);
                }
                grid[k, 2] = new SpotData();
            }
        }
        fase = f;
        if (f == FaseP2.CampurBaris1 && platTransform != null)
        {
            // Soal 5 memakai tiga lubang terbawah: asam benzoat, MgCl2, lalu petroleum eter.
            HapusKristalMgCl2Soal5();
            HapusSerbukMgCl2Lama();
            HapusEfekKelarutanPermanen();
            for (int k = 0; k < 3; k++)
            {
                if (grid[k, 0] != null)
                {
                    if (grid[k, 0].silinder != null) Destroy(grid[k, 0].silinder);
                    if (grid[k, 0].mat != null) Destroy(grid[k, 0].mat);
                }
                grid[k, 0] = new SpotData();
                if (k == 1)
                {
                    TampilSpot(k, 0, warnaSampel[k], false, false);
                    if (grid[k, 0].silinder != null)
                    {
                        float ukuranAcuan = Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y);
                        float diameterSerbuk = ukuranAcuan * 0.13f;
                        grid[k, 0].silinder.transform.localScale = Vector3.one * diameterSerbuk;
                        grid[k, 0].silinder.transform.localPosition = KoordinatSpotLokal(k, 0) - Vector3.forward * diameterSerbuk * 0.15f;
                    }
                }
                else TampilSpot(k, 0, warnaSampel[k], false, true);
            }
            BuatKristalMgCl2Soal5();
        }
        PerbaruiDiagramAsam();
        PerbaruiPenandaAsam();
        PerbaruiCawanAsam();
        PerbaruiTargetDropSpatula();
    }

    private void PerbaruiDiagramAsam()
    {
        Sprite spriteDiagram = fase == FaseP2.IsiKolom1 ? diagramPenempatanAsam : (fase == FaseP2.IsiKolom2 ? diagramPenempatanMgCl2 : null);
        bool tampil = spriteDiagram != null;
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
            image.sprite = spriteDiagram;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
        if (panelDiagramAsam != null && tampil) { var imgDiagram = panelDiagramAsam.GetComponent<Image>(); if (imgDiagram != null) imgDiagram.sprite = spriteDiagram; }
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
        bool faseEtanol = fase == FaseP2.CampurBaris2;
        bool faseEtanolBaris3 = fase == FaseP2.CampurBaris3;
        bool tampil = (FaseSumberAktif || faseEtanol || faseEtanolBaris3) && platTransform != null;
        if (!tampil)
        {
            SembunyikanMarkerDropCawan();
            if (cawanAsamBenzoat != null) { Destroy(cawanAsamBenzoat); cawanAsamBenzoat = null; }
            if (cawanEtanolSumber != null) { Destroy(cawanEtanolSumber); cawanEtanolSumber = null; }
            if (cawanEtanolBaris3Sumber != null) { Destroy(cawanEtanolBaris3Sumber); cawanEtanolBaris3Sumber = null; }
            sumberAsamDiletakkan = false;
            sumberEtanolDiletakkan = false;
            sumberEtanolBaris3Diletakkan = false;
            return;
        }
        PerbaruiPenandaDropCawan();
    }
    private void PerbaruiPenandaDropCawan()
    {
        bool faseEtanol = fase == FaseP2.CampurBaris2;
        bool faseEtanolBaris3 = fase == FaseP2.CampurBaris3 && SemuaAquadesBaris3Diteteskan() && !SemuaEtanolBaris3Diteteskan();
        bool sumberSudahDiletakkan = faseEtanolBaris3 ? sumberEtanolBaris3Diletakkan :
            faseEtanol ? sumberEtanolDiletakkan : sumberAsamDiletakkan;
        GameObject panelSoalAktif = fase == FaseP2.IsiKolom3 ? soal4 :
            faseEtanolBaris3 ? soal7 : faseEtanol ? soal6 : PanelSoalSumberAktif;
        bool faseSumberPetroleum = fase == FaseP2.IsiKolom3;
        if ((!FaseSumberAktif && !faseSumberPetroleum && !faseEtanol && !faseEtanolBaris3) || platTransform == null || sumberSudahDiletakkan ||
            (panelSoalAktif != null && !panelSoalAktif.activeInHierarchy))
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
        imageMarker.color = new Color(0.72f, 0.75f, 0.78f, sumberSudahDiletakkan ? 0.45f : 0.85f);

        TMP_Text labelSumber = penandaDropCawanAsam.GetComponentInChildren<TMP_Text>(true);
        if (labelSumber != null) labelSumber.text = fase == FaseP2.IsiKolom3 ? "Letakkan\nPetroleum Ether" :
            (faseEtanol || faseEtanolBaris3) ? "Letakkan\nEtanol" :
            (fase == FaseP2.IsiKolom2 ? "Letakkan\nMgCl<sub>2</sub>" : "Letakkan\nAsam Benzoat");
        penandaDropCawanAsam.SetActive(true);
        penandaDropCawanAsam.transform.SetAsLastSibling();
    }

    public bool BolehAmbilSpatulaAsam(){return FaseSumberAktif&&sumberAsamDiletakkan&&cawanAsamBenzoat!=null&&pendingKolomAsam<0&&!sedangMenakarAsam&&hitungIsian[IndeksSumberAktif]<3;}
    public bool DropSumberAsam(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.IsiKolom1) return false;
        return DropSumberBahan(pos, cam, model, "CawanAsamBenzoat_Sumber");
    }

    /// <summary>Soal 3: cawan MgCl2 diletakkan pada marker yang sama dengan cawan asam benzoat di Soal 2.</summary>
    public bool DropSumberMgCl2(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.IsiKolom2) return false;
        return DropSumberBahan(pos, cam, model, "CawanMgCl2_Sumber");
    }

    public bool DropSumberPetroleumEter(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.IsiKolom3) return false;
        return DropSumberBahan(pos, cam, model, "SumberPetroleumEther");
    }

    public bool DropSumberEtanol(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.CampurBaris2) return false;
        return DropSumberBahan(pos, cam, model, "SumberEtanol");
    }

    public bool DropSumberEtanolBaris3(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.CampurBaris3 || !SemuaAquadesBaris3Diteteskan() || SemuaEtanolBaris3Diteteskan() ||
            sumberEtanolBaris3Diletakkan || cawanEtanolBaris3Sumber != null)
            return false;
        return DropSumberBahan(pos, cam, model, "SumberEtanolBaris3", true);
    }

    public bool DropPipetTetes(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.IsiKolom3 || !sumberAsamDiletakkan || cawanAsamBenzoat == null ||
            model == null || pendingLubangPetroleum >= 0 || hitungIsian[2] >= 3)
            return false;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        int lubang;
        if (!CariLubangPetroleum(pos, cam, out lubang)) { TampilSalah(); return false; }

        pendingLubangPetroleum = lubang;
        pipetPetroleumAktif = model;
        model.SetActive(true);
        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) Destroy(collider);
        foreach (Camera kameraModel in model.GetComponentsInChildren<Camera>(true)) kameraModel.enabled = false;
        foreach (Light lampuModel in model.GetComponentsInChildren<Light>(true)) lampuModel.enabled = false;
        MatikanBayangan(model);
        model.transform.SetParent(platTransform, true);
        model.transform.localScale *= 1.25f;
        animasiPetroleumAktif = StartCoroutine(AnimasiPipetPetroleum(lubang, model, cam));
        PerbaruiTargetDropSpatula();
        return true;
    }
    private IEnumerator AnimasiPipetPetroleum(int lubang, GameObject pipet, Camera cam)
    {
        bool benar = false;
        if (pipet == null || cawanAsamBenzoat == null || cam == null) yield break;

        Quaternion rotasiPipet = cam.transform.rotation * Quaternion.Euler(309.72903f, 270f, 90f);
        pipet.transform.rotation = rotasiPipet;
        Bounds boundsSumber;
        Vector3 posisiAmbil = cawanAsamBenzoat.transform.position + cam.transform.up * 0.06f;
        Vector3 posisiAtasSumber = posisiAmbil + cam.transform.up * 0.06f;
        if (AmbilBoundsRenderer(cawanAsamBenzoat, out boundsSumber))
        {
            float setengahTinggiSumber = Mathf.Abs(cam.transform.up.x) * boundsSumber.extents.x +
                                         Mathf.Abs(cam.transform.up.y) * boundsSumber.extents.y +
                                         Mathf.Abs(cam.transform.up.z) * boundsSumber.extents.z;
            // Pipet masuk sedikit ke mulut di puncak wadah, lalu mundur setelah cairan tersedot.
            posisiAtasSumber = boundsSumber.center + cam.transform.up * (setengahTinggiSumber + 0.045f);
            posisiAmbil = boundsSumber.center + cam.transform.up * (setengahTinggiSumber - 0.012f);
        }
        Vector3 titikLubang = PosisiLubangSpot(lubang, BarisPetroleum, 0.02f);
        Vector3 posisiAtasLubang = titikLubang + cam.transform.up * 0.08f;
        Vector3 posisiUjungPipetDiLubang = titikLubang + cam.transform.up * 0.015f;
        Vector3 tujuanAtasSumber = PosisiAkarPipetUntukUjung(pipet, cam, posisiAtasSumber);
        Vector3 tujuanAmbil = PosisiAkarPipetUntukUjung(pipet, cam, posisiAmbil);
        Vector3 tujuanAtasLubang = PosisiAkarPipetUntukUjung(pipet, cam, posisiAtasLubang);
        Vector3 tujuanLubang = PosisiAkarPipetUntukUjung(pipet, cam, posisiUjungPipetDiLubang);
        tujuanAtasSumber = BatasiTitikPipetDalamLayar(cam, tujuanAtasSumber);
        tujuanAmbil = BatasiTitikPipetDalamLayar(cam, tujuanAmbil);
        tujuanAtasLubang = BatasiTitikPipetDalamLayar(cam, tujuanAtasLubang);
        tujuanLubang = BatasiTitikPipetDalamLayar(cam, tujuanLubang);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasSumber, rotasiPipet, 0.42f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAmbil, rotasiPipet, 0.30f);
        yield return new WaitForSeconds(0.18f);
        yield return AnimasiSedotPetroleum(cam, posisiAmbil);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasSumber, rotasiPipet, 0.26f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasLubang, rotasiPipet, 0.42f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanLubang, rotasiPipet, 0.24f);

        ManajerTakaran takaran = ManajerTakaran.Instance;
        if (takaran == null)
        {
            TampilSalah();
        }
        else
        {
            yield return StartCoroutine(takaran.TeteskanKeSpot("lubang " + (lubang + 1), 5,
                hitung => AnimasiSatuTetesPetroleum(pipet, lubang, cam),
                hasil => benar = hasil));
        }

        if (benar && grid[lubang, BarisPetroleum] != null && !grid[lubang, BarisPetroleum].terisi)
        {
            TampilSpot(lubang, BarisPetroleum, warnaSampel[2], false, true);
            hitungIsian[2]++;
            if (hitungIsian[2] >= 3)
            {
                kolomSelesai[2] = true;
                Notif("Tiga lubang petroleum ether sudah menerima 5 tetes.");
            }
        }

        if (pipet != null) Destroy(pipet);
        pipetPetroleumAktif = null;
        animasiPetroleumAktif = null;
        pendingLubangPetroleum = -1;
        PerbaruiTargetDropSpatula();
    }

    private IEnumerator AnimasiSedotPetroleum(Camera cam, Vector3 posisiMulut)
    {
        if (cam == null || platTransform == null) yield break;
        GameObject cairanTersedot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider colliderCairan = cairanTersedot.GetComponent<Collider>();
        if (colliderCairan != null) Destroy(colliderCairan);
        cairanTersedot.name = "CairanTersedotPetroleum";
        Vector3 awal = posisiMulut - cam.transform.up * 0.025f;
        Vector3 akhir = posisiMulut + cam.transform.up * 0.055f;
        cairanTersedot.transform.position = awal;
        float ukuran = Mathf.Max(0.006f, Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.025f);
        cairanTersedot.transform.localScale = new Vector3(ukuran * 0.65f, ukuran, ukuran * 0.65f);
        Material material = BuatMaterialTetesPetroleum(warnaSampel[2]);
        cairanTersedot.GetComponent<Renderer>().sharedMaterial = material;
        MatikanBayangan(cairanTersedot);
        float waktu = 0f;
        while (waktu < 0.38f)
        {
            if (cairanTersedot == null) yield break;
            waktu += Time.deltaTime;
            float progres = Ease(waktu / 0.38f);
            cairanTersedot.transform.position = Vector3.Lerp(awal, akhir, progres);
            Color warna = warnaSampel[2];
            warna.a = Mathf.Lerp(0.7f, 0.2f, progres);
            SetWarna(material, warna);
            yield return null;
        }
        if (cairanTersedot != null) Destroy(cairanTersedot);
        if (material != null) Destroy(material);
    }
    private Vector3 PosisiAkarPipetUntukUjung(GameObject pipet, Camera cam, Vector3 posisiUjung)
    {
        if (pipet == null || cam == null) return posisiUjung;
        Bounds bounds;
        if (!AmbilBoundsRenderer(pipet, out bounds)) return posisiUjung;
        float setengahTinggi = Mathf.Abs(cam.transform.up.x) * bounds.extents.x +
                               Mathf.Abs(cam.transform.up.y) * bounds.extents.y +
                               Mathf.Abs(cam.transform.up.z) * bounds.extents.z;
        Vector3 pusatTujuan = posisiUjung + cam.transform.up * setengahTinggi;
        return pipet.transform.position + (pusatTujuan - bounds.center);
    }
    private Vector3 BatasiTitikPipetDalamLayar(Camera cam, Vector3 posisi)
    {
        if (cam == null) return posisi;
        Vector3 viewport = cam.WorldToViewportPoint(posisi);
        float depth = Vector3.Dot(posisi - cam.transform.position, cam.transform.forward);
        if (depth <= cam.nearClipPlane + 0.08f) depth = cam.nearClipPlane + 0.08f;
        if (viewport.z <= cam.nearClipPlane) { viewport.x = 0.5f; viewport.y = 0.5f; }
        viewport.x = Mathf.Clamp(viewport.x, 0.12f, 0.88f);
        viewport.y = Mathf.Clamp(viewport.y, 0.14f, 0.86f);
        return cam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
    }

    private void PastikanPipetTetapTerlihat(GameObject pipet, Camera cam)
    {
        if (pipet == null || cam == null) return;
        Bounds bounds;
        if (!AmbilBoundsRenderer(pipet, out bounds))
        {
            pipet.transform.position = BatasiTitikPipetDalamLayar(cam, pipet.transform.position);
            return;
        }

        Vector3 centerViewport = cam.WorldToViewportPoint(bounds.center);
        float depth = Vector3.Dot(bounds.center - cam.transform.position, cam.transform.forward);
        if (depth <= cam.nearClipPlane) return;
        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3((i & 1) == 0 ? bounds.min.x : bounds.max.x,
                                         (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                                         (i & 4) == 0 ? bounds.min.z : bounds.max.z);
            Vector3 vp = cam.WorldToViewportPoint(corner);
            if (vp.z <= cam.nearClipPlane) continue;
            min = Vector3.Min(min, vp);
            max = Vector3.Max(max, vp);
        }
        if (float.IsInfinity(min.x) || float.IsInfinity(min.y)) return;
        float geserX = min.x < 0.04f ? 0.04f - min.x : (max.x > 0.96f ? 0.96f - max.x : 0f);
        float geserY = min.y < 0.08f ? 0.08f - min.y : (max.y > 0.92f ? 0.92f - max.y : 0f);
        Vector3 sebelum = cam.ViewportToWorldPoint(new Vector3(centerViewport.x, centerViewport.y, depth));
        Vector3 sesudah = cam.ViewportToWorldPoint(new Vector3(centerViewport.x + geserX, centerViewport.y + geserY, depth));
        pipet.transform.position += sesudah - sebelum;
    }

    private Material BuatMaterialTetesPetroleum(Color warna)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = matLarutan != null ? new Material(matLarutan) : new Material(shader);
        warna.a = 0.62f;
        SetWarna(material, warna);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);
        if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.88f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.04f);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
    private void AturKekasaranCairanEtanol(Material material)
    {
        if (material == null) return;
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.54f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.54f);
    }

    private IEnumerator GerakkanPipetPetroleum(GameObject pipet, Camera cam, Vector3 tujuan, Quaternion rotasi, float durasi)
    {
        if (pipet == null) yield break;
        Vector3 awal = pipet.transform.position;
        Quaternion rotasiAwal = pipet.transform.rotation;
        float waktu = 0f;
        while (waktu < durasi)
        {
            if (pipet == null) yield break;
            waktu += Time.deltaTime;
            float t = Ease(waktu / durasi);
            pipet.transform.position = Vector3.Lerp(awal, tujuan, t);
            pipet.transform.rotation = Quaternion.Slerp(rotasiAwal, rotasi, t);
            PastikanPipetTetapTerlihat(pipet, cam);
            yield return null;
        }
        if (pipet != null)
        {
            pipet.transform.position = tujuan;
            pipet.transform.rotation = rotasi;
            PastikanPipetTetapTerlihat(pipet, cam);
        }
    }

    private IEnumerator AnimasiSatuTetesPetroleum(GameObject pipet, int lubang, Camera cam)
    {
        if (platTransform == null || pipet == null) yield break;
        Vector3 tujuan = PosisiLubangSpot(lubang, BarisPetroleum, 0.002f);
        Bounds boundsPipet;
        Vector3 awal = pipet.transform.position - cam.transform.up * 0.06f;
        if (AmbilBoundsRenderer(pipet, out boundsPipet))
        {
            float setengahTinggiPipet = Mathf.Abs(cam.transform.up.x) * boundsPipet.extents.x +
                                       Mathf.Abs(cam.transform.up.y) * boundsPipet.extents.y +
                                       Mathf.Abs(cam.transform.up.z) * boundsPipet.extents.z;
            awal = boundsPipet.center - cam.transform.up * (setengahTinggiPipet * 0.95f);
        }
        GameObject tetes = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider colliderTetes = tetes.GetComponent<Collider>();
        if (colliderTetes != null) Destroy(colliderTetes);
        tetes.name = "TetesPetroleumEther";
        tetes.transform.position = awal;
        float ukuranTetes = Mathf.Max(0.004f, Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.016f);
        Vector3 skalaTetes = new Vector3(ukuranTetes * 0.45f, ukuranTetes * 0.85f, ukuranTetes * 0.45f);
        tetes.transform.localScale = skalaTetes;
        Color warnaCair = warnaSampel[2];
        Material materialTetes = BuatMaterialTetesPetroleum(warnaCair);
        Renderer rendererTetes = tetes.GetComponent<Renderer>();
        if (rendererTetes != null) rendererTetes.sharedMaterial = materialTetes;
        MatikanBayangan(tetes);

        float waktu = 0f;
        while (waktu < 0.22f)
        {
            if (tetes == null) yield break;
            waktu += Time.deltaTime;
            float progres = Ease(waktu / 0.22f);
            tetes.transform.position = Vector3.Lerp(awal, tujuan, progres);
            Vector3 skalaGenangan = Vector3.Scale(skalaTetes, new Vector3(1.08f, 0.22f, 1.08f));
            tetes.transform.localScale = Vector3.Lerp(skalaTetes, skalaGenangan, Mathf.Clamp01((progres - 0.78f) / 0.22f));
            warnaCair.a = Mathf.Lerp(0.62f, 0.35f, progres);
            SetWarna(materialTetes, warnaCair);
            yield return null;
        }
        if (tetes != null) Destroy(tetes);
        if (materialTetes != null) Destroy(materialTetes);
    }

    public bool DropPipetEtanol(Vector2 pos, Camera cam, GameObject model)
    {
        if (fase != FaseP2.CampurBaris2 || !sumberEtanolDiletakkan || cawanEtanolSumber == null ||
            model == null || pendingLubangEtanol >= 0 || !AdaEtanolBelumDiteteskan())
            return false;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        int kolom;
        if (!CariLubangEtanol(pos, cam, out kolom))
        {
            TampilSalah();
            return false;
        }

        pendingLubangEtanol = kolom;
        pipetEtanolAktif = model;
        model.SetActive(true);
        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) Destroy(collider);
        foreach (Camera kameraModel in model.GetComponentsInChildren<Camera>(true)) kameraModel.enabled = false;
        foreach (Light lampuModel in model.GetComponentsInChildren<Light>(true)) lampuModel.enabled = false;
        MatikanBayangan(model);
        model.transform.SetParent(platTransform, true);
        model.transform.localScale *= 1.25f;
        animasiEtanolAktif = StartCoroutine(AnimasiPipetEtanol(kolom, model, cam));
        PerbaruiTargetDropSpatula();
        return true;
    }

    private IEnumerator AnimasiPipetEtanol(int lubang, GameObject pipet, Camera cam)
    {
        bool benar = false;
        if (pipet == null || cawanEtanolSumber == null || cam == null) yield break;

        Quaternion rotasiPipet = cam.transform.rotation * Quaternion.Euler(309.72903f, 270f, 90f);
        pipet.transform.rotation = rotasiPipet;
        Bounds boundsSumber;
        Vector3 posisiAmbil = cawanEtanolSumber.transform.position + cam.transform.up * 0.06f;
        Vector3 posisiAtasSumber = posisiAmbil + cam.transform.up * 0.06f;
        if (AmbilBoundsRenderer(cawanEtanolSumber, out boundsSumber))
        {
            float setengahTinggiSumber = Mathf.Abs(cam.transform.up.x) * boundsSumber.extents.x +
                                         Mathf.Abs(cam.transform.up.y) * boundsSumber.extents.y +
                                         Mathf.Abs(cam.transform.up.z) * boundsSumber.extents.z;
            posisiAtasSumber = boundsSumber.center + cam.transform.up * (setengahTinggiSumber + 0.045f);
            posisiAmbil = boundsSumber.center + cam.transform.up * (setengahTinggiSumber - 0.012f);
        }
        Vector3 titikLubang = PosisiLubangSpot(lubang, 1, 0.02f);
        Vector3 posisiAtasLubang = titikLubang + cam.transform.up * 0.08f;
        Vector3 posisiUjungPipetDiLubang = titikLubang + cam.transform.up * 0.015f;
        Vector3 tujuanAtasSumber = BatasiTitikPipetDalamLayar(cam, PosisiAkarPipetUntukUjung(pipet, cam, posisiAtasSumber));
        Vector3 tujuanAmbil = BatasiTitikPipetDalamLayar(cam, PosisiAkarPipetUntukUjung(pipet, cam, posisiAmbil));
        // Lintasan akhir masuk dari atas kamera tepat menuju lubang, searah dengan posisi pipet.
        Vector3 tujuanAtasLubang = PosisiAkarPipetUntukUjung(pipet, cam, posisiAtasLubang);
        Vector3 tujuanLubang = PosisiAkarPipetUntukUjung(pipet, cam, posisiUjungPipetDiLubang);

        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasSumber, rotasiPipet, 0.42f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAmbil, rotasiPipet, 0.30f);
        yield return new WaitForSeconds(0.18f);
        yield return AnimasiSedotEtanol(cam, posisiAmbil);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasSumber, rotasiPipet, 0.26f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanAtasLubang, rotasiPipet, 0.42f);
        yield return GerakkanPipetPetroleum(pipet, cam, tujuanLubang, rotasiPipet, 0.24f);

        ManajerTakaran takaran = ManajerTakaran.Instance;
        if (takaran == null)
        {
            TampilSalah();
        }
        else
        {
            yield return StartCoroutine(takaran.TeteskanKeSpot("sampel " + (lubang + 1), 5,
                hitung => AnimasiSatuTetesEtanol(pipet, lubang, cam),
                hasil => benar = hasil, "etanol"));
        }

        if (benar && lubang >= 0 && lubang < 3 && grid[lubang, 1] != null)
        {
            etanolDitetes[lubang] = true;
            Color warnaEtanol = new Color(0.82f, 0.92f, 1f, 0.62f);
            TampilSpot(lubang, 1, warnaEtanol, false, true);
            AturKekasaranCairanEtanol(grid[lubang, 1].mat);
            Notif("Etanol selesai diteteskan: " + JumlahEtanolSelesai() + "/3 sampel.");
            if (SemuaEtanolSudahDiteteskan()) TampilkanInstruksiAdukEtanol();
        }

        if (pipet != null) Destroy(pipet);
        pipetEtanolAktif = null;
        animasiEtanolAktif = null;
        pendingLubangEtanol = -1;
        PerbaruiTargetDropSpatula();
    }

    private IEnumerator AnimasiSedotEtanol(Camera cam, Vector3 posisiMulut)
    {
        if (cam == null || platTransform == null) yield break;
        GameObject cairanTersedot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider colliderCairan = cairanTersedot.GetComponent<Collider>();
        if (colliderCairan != null) Destroy(colliderCairan);
        cairanTersedot.name = "CairanTersedotEtanol";
        Vector3 awal = posisiMulut - cam.transform.up * 0.025f;
        Vector3 akhir = posisiMulut + cam.transform.up * 0.055f;
        cairanTersedot.transform.position = awal;
        float ukuran = Mathf.Max(0.006f, Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.025f);
        cairanTersedot.transform.localScale = new Vector3(ukuran * 0.65f, ukuran, ukuran * 0.65f);
        Color warna = new Color(0.86f, 0.95f, 1f, 0.62f);
        Material material = BuatMaterialTetesPetroleum(warna);
        cairanTersedot.GetComponent<Renderer>().sharedMaterial = material;
        MatikanBayangan(cairanTersedot);
        float waktu = 0f;
        while (waktu < 0.38f)
        {
            if (cairanTersedot == null) yield break;
            waktu += Time.deltaTime;
            float progres = Ease(waktu / 0.38f);
            cairanTersedot.transform.position = Vector3.Lerp(awal, akhir, progres);
            warna.a = Mathf.Lerp(0.7f, 0.2f, progres);
            SetWarna(material, warna);
            yield return null;
        }
        if (cairanTersedot != null) Destroy(cairanTersedot);
        if (material != null) Destroy(material);
    }

    private IEnumerator AnimasiSatuTetesEtanol(GameObject pipet, int lubang, Camera cam)
    {
        if (platTransform == null || pipet == null || cam == null) yield break;
        Vector3 tujuan = PosisiLubangSpot(lubang, 1, 0.002f);
        Bounds boundsPipet;
        Vector3 awal = pipet.transform.position - cam.transform.up * 0.06f;
        if (AmbilBoundsRenderer(pipet, out boundsPipet))
        {
            float setengahTinggiPipet = Mathf.Abs(cam.transform.up.x) * boundsPipet.extents.x +
                                       Mathf.Abs(cam.transform.up.y) * boundsPipet.extents.y +
                                       Mathf.Abs(cam.transform.up.z) * boundsPipet.extents.z;
            awal = boundsPipet.center - cam.transform.up * (setengahTinggiPipet * 0.95f);
        }
        GameObject tetes = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider colliderTetes = tetes.GetComponent<Collider>();
        if (colliderTetes != null) Destroy(colliderTetes);
        tetes.name = "TetesEtanol";
        tetes.transform.position = awal;
        float ukuranTetes = Mathf.Max(0.004f, Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.016f);
        Vector3 skalaTetes = new Vector3(ukuranTetes * 0.45f, ukuranTetes * 0.85f, ukuranTetes * 0.45f);
        tetes.transform.localScale = skalaTetes;
        Color warnaCair = new Color(0.86f, 0.95f, 1f, 0.62f);
        Material materialTetes = BuatMaterialTetesPetroleum(warnaCair);
        AturKekasaranCairanEtanol(materialTetes);
        Renderer rendererTetes = tetes.GetComponent<Renderer>();
        if (rendererTetes != null) rendererTetes.sharedMaterial = materialTetes;
        MatikanBayangan(tetes);

        float waktu = 0f;
        while (waktu < 0.22f)
        {
            if (tetes == null) yield break;
            waktu += Time.deltaTime;
            float progres = Ease(waktu / 0.22f);
            tetes.transform.position = Vector3.Lerp(awal, tujuan, progres);
            Vector3 skalaGenangan = Vector3.Scale(skalaTetes, new Vector3(1.08f, 0.22f, 1.08f));
            tetes.transform.localScale = Vector3.Lerp(skalaTetes, skalaGenangan, Mathf.Clamp01((progres - 0.78f) / 0.22f));
            warnaCair.a = Mathf.Lerp(0.62f, 0.35f, progres);
            SetWarna(materialTetes, warnaCair);
            yield return null;
        }
        if (tetes != null) Destroy(tetes);
        if (materialTetes != null) Destroy(materialTetes);
    }

    private bool DropSumberBahan(Vector2 pos, Camera cam, GameObject model, string namaObjek, bool etanolBaris3 = false)
    {
        bool faseEtanol = fase == FaseP2.CampurBaris2;
        bool faseEtanolBaris3 = fase == FaseP2.CampurBaris3 && etanolBaris3;
        bool sumberSudahDiletakkan = faseEtanolBaris3 ? sumberEtanolBaris3Diletakkan :
            faseEtanol ? sumberEtanolDiletakkan : sumberAsamDiletakkan;
        if ((!FaseSumberAktif && fase != FaseP2.IsiKolom3 && !faseEtanol && !faseEtanolBaris3) ||
            platTransform == null || sumberSudahDiletakkan || model == null) return false;
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
        // Petroleum ether dan etanol memakai orientasi yang sama, dengan bukaan menghadap ke atas dari sudut kamera.
        Quaternion rotasiWadah = Quaternion.Euler(270.02f, 0f, 0f);
        if (fase == FaseP2.IsiKolom3 || faseEtanol || faseEtanolBaris3)
            rotasiWadah = Quaternion.AngleAxis(90f, cam.transform.forward) * (Quaternion.AngleAxis(-90f, cam.transform.up) * (cam.transform.rotation * rotasiWadah));
        else
            rotasiWadah = cam.transform.rotation * rotasiWadah;
        model.transform.rotation = rotasiWadah;
        Vector3 center;
        Vector2 size;
        if (CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out center, out size) &&
            size.x > 0.001f && size.y > 0.001f)
        {
            float faktor = Mathf.Min(targetWidth / size.x, targetHeight / size.y) * skalaCawanAsamBenzoat;
            if (fase == FaseP2.IsiKolom3) faktor *= 0.55f;
            if (faseEtanol || faseEtanolBaris3) faktor *= 0.72f;
            model.transform.localScale *= faktor;
            CardTahanGeser3D.UkurDiRuangKamera(model, cam.transform, out center, out size);
        }
        model.transform.position = target - cam.transform.TransformVector(center);
        // Parent to plate while preserving the fitted pose and the target placement.
        model.transform.SetParent(platTransform, true);
        PusatkanRendererKeLayar(model, cam, titikTengah, d);
        foreach (var x in model.GetComponentsInChildren<Collider>(true)) Destroy(x);
        foreach (var x in model.GetComponentsInChildren<Camera>(true)) x.enabled = false;
        foreach (var x in model.GetComponentsInChildren<Light>(true)) x.enabled = false;
        BeningkanSlotKaca(model);
        MatikanBayangan(model);
        model.name = namaObjek;
        if (faseEtanolBaris3)
        {
            cawanEtanolBaris3Sumber = model;
            sumberEtanolBaris3Diletakkan = true;
        }
        else if (faseEtanol)
        {
            cawanEtanolSumber = model;
            sumberEtanolDiletakkan = true;
        }
        else
        {
            cawanAsamBenzoat = model;
            sumberAsamDiletakkan = true;
        }
        PerbaruiPenandaDropCawan();
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
        foreach (var bulir in gelembungMgCl2)
        {
            if (bulir == null) continue;
            var renderer = bulir.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null) Destroy(renderer.sharedMaterial);
            Destroy(bulir);
        }
        gelembungMgCl2.Clear();
        HapusKristalMgCl2Soal5();
        foreach (var efek in efekKelarutanPermanen)
        {
            if (efek == null) continue;
            Renderer rendererEfek = efek.GetComponent<Renderer>();
            if (rendererEfek != null && rendererEfek.sharedMaterial != null) Destroy(rendererEfek.sharedMaterial);
            Destroy(efek);
        }
        efekKelarutanPermanen.Clear();
        sedangMenuangMgCl2 = false;        grid = new SpotData[3, 3];
        hitungIsian = new int[3];
        kolomSelesai = new bool[3];
        barisSelesai = new bool[3];
        aquadesDitetes = new bool[3];
        etanolDitetes = new bool[3];
        aquadesBaris3Ditetes = new bool[3];
        etanolBaris3Ditetes = new bool[3];
        tetesanBaris3Berlangsung = new bool[3];
        probeMerahBaris3 = new bool[3, 3];
        probeHitamBaris3 = new bool[3, 3];
        posisiToastUjiEtanolTersimpan = false;
        voltmeterDipasang = false;
        if (voltmeterAktif != null) { Destroy(voltmeterAktif); voltmeterAktif = null; }
        if (panelProbeVoltmeter != null) { Destroy(panelProbeVoltmeter); panelProbeVoltmeter = null; }
        tutupToastUji = null;
        if (panelToastUji != null) { Destroy(panelToastUji); panelToastUji = null; teksToastUji = null; }
        teksStatusProbe = null;
        tokenProbeBaris3 = new RectTransform[2];
        posisiHomeProbe = new Vector2[2];
        instruksiAdukAktif = false;
        sampelDiuji = new bool[3, 3];
        platTransform = null;
        if (cawanAsamBenzoat != null) { Destroy(cawanAsamBenzoat); cawanAsamBenzoat = null; }
        if (cawanEtanolSumber != null) { Destroy(cawanEtanolSumber); cawanEtanolSumber = null; }
        if (cawanEtanolBaris3Sumber != null) { Destroy(cawanEtanolBaris3Sumber); cawanEtanolBaris3Sumber = null; }
        sumberAsamDiletakkan = false;
        sumberEtanolDiletakkan = false;
        sumberEtanolBaris3Diletakkan = false;
        SembunyikanMarkerDropCawan(); kecepatanPosisiMarker.Clear();
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
        pendingLubangPetroleum = -1;
        pendingLubangEtanol = -1;
        if (animasiPetroleumAktif != null) StopCoroutine(animasiPetroleumAktif);
        if (animasiEtanolAktif != null) StopCoroutine(animasiEtanolAktif);
        animasiPetroleumAktif = null;
        animasiEtanolAktif = null;
        if (pipetPetroleumAktif != null) Destroy(pipetPetroleumAktif);
        if (pipetEtanolAktif != null) Destroy(pipetEtanolAktif);
        pipetPetroleumAktif = null;
        pipetEtanolAktif = null;
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
                marker.transform.localPosition = KoordinatSpotLokal(b, BarisSumberAktif);
                marker.transform.localRotation = Quaternion.identity;
                // Marker persegi mengikuti ukuran satu lubang pada grid CariSpot.
                float ukuranMarker = Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.30f;
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
            int barisMarkerAsam = BarisSumberAktif;
            bool belumDiisi = grid[b, barisMarkerAsam] == null || !grid[b, barisMarkerAsam].terisi;
            bool targetBerikutnya = pendingKolomAsam < 0 && b == hitungIsian[0];
            if (penandaAsam[b] != null) penandaAsam[b].SetActive(tampil && belumDiisi && targetBerikutnya);
        }
    }

    private void TambahIsianMgCl2(int kolom, int baris)
    {
        if (grid[kolom, baris] == null) grid[kolom, baris] = new SpotData();
        if (grid[kolom, baris].terisi) return;
        TampilSpot(kolom, baris, warnaSampel[1]);
        if (targetDropSpatula != null && kolom >= 0 && kolom < targetDropSpatula.Length && targetDropSpatula[kolom] != null)
            targetDropSpatula[kolom].gameObject.SetActive(false);
        PerbaruiTargetDropSpatula();
        hitungIsian[1]++;
        if (hitungIsian[1] >= 3)
        {
            kolomSelesai[1] = true;
            Notif("Kolom terisi!");
        }
    }

    private void BuatKristalMgCl2Soal5()
    {
        if (platTransform == null || kristalMgCl2Soal5.Count > 0) return;
        Bounds plateBounds = AmbilBoundsPlateLokal();
        float ukuranAcuan = Mathf.Min(plateBounds.extents.x, plateBounds.extents.y);
        if (ukuranAcuan <= 0f) return;

        Vector2[] sebaran =
        {
            new Vector2(-0.040f, 0.018f), new Vector2(0.020f, 0.040f),
            new Vector2(0.044f, -0.020f), new Vector2(-0.024f, -0.038f),
            new Vector2(0.000f, 0.000f), new Vector2(0.012f, -0.012f)
        };
        Color[] warnaKristal =
        {
            new Color(0.97f, 1f, 1f, 1f), new Color(0.78f, 0.94f, 1f, 1f),
            new Color(0.90f, 0.98f, 1f, 1f), new Color(0.72f, 0.88f, 0.98f, 1f),
            new Color(1f, 1f, 1f, 1f), new Color(0.84f, 0.95f, 1f, 1f)
        };
        Vector3 pusat = KoordinatSpotLokal(1, 0) + Vector3.forward * 0.009f;
        for (int i = 0; i < sebaran.Length; i++)
        {
            GameObject kristal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            kristal.name = "KristalMgCl2Soal5_" + (i + 1);
            Collider collider = kristal.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            kristal.transform.SetParent(platTransform, false);
            kristal.transform.localPosition = pusat + new Vector3(sebaran[i].x * ukuranAcuan, sebaran[i].y * ukuranAcuan, i * 0.00015f);
            float diameter = ukuranAcuan * (i == 4 ? 0.025f : 0.020f);
            kristal.transform.localScale = new Vector3(diameter, diameter * 0.58f, diameter * 0.58f);
            kristal.transform.localRotation = Quaternion.Euler(31f * i, 47f * i, 23f * i);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            SetWarna(material, warnaKristal[i]);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.2f);
            kristal.GetComponent<Renderer>().sharedMaterial = material;
            MatikanBayangan(kristal);
            kristalMgCl2Soal5.Add(kristal);
        }
    }

    private void HapusKristalMgCl2Soal5()
    {
        var sudahDihapus = new HashSet<GameObject>();
        foreach (GameObject kristal in kristalMgCl2Soal5)
        {
            if (kristal == null) continue;
            sudahDihapus.Add(kristal);
            Renderer rendererKristal = kristal.GetComponent<Renderer>();
            if (rendererKristal != null && rendererKristal.sharedMaterial != null) Destroy(rendererKristal.sharedMaterial);
            Destroy(kristal);
        }
        kristalMgCl2Soal5.Clear();

        // Bersihkan juga sisa visual setelah skrip mengalami reload saat Play Mode.
        if (platTransform == null) return;
        foreach (Transform anak in platTransform.GetComponentsInChildren<Transform>(true))
        {
            if (anak == null || !anak.name.StartsWith("KristalMgCl2Soal5", StringComparison.Ordinal)) continue;
            GameObject objek = anak.gameObject;
            if (!sudahDihapus.Add(objek)) continue;
            Renderer rendererKristal = objek.GetComponent<Renderer>();
            if (rendererKristal != null && rendererKristal.sharedMaterial != null) Destroy(rendererKristal.sharedMaterial);
            Destroy(objek);
        }
    }

    private void HapusSerbukMgCl2Lama()
    {
        foreach (GameObject serbuk in gelembungMgCl2)
            if (serbuk != null) Destroy(serbuk);
        gelembungMgCl2.Clear();

        // Serbuk yang sudah terlepas dari spatula tetap dihapus walau referensi list hilang.
        if (platTransform == null) return;
        foreach (Transform anak in platTransform.GetComponentsInChildren<Transform>(true))
        {
            if (anak == null) continue;
            if (anak.name == "SerbukMgCl2" || anak.name == "ButirMgCl2")
                Destroy(anak.gameObject);
        }
    }

    private void TampilkanMgCl2LarutSeragam()
    {
        if (platTransform == null || grid[1, 0] == null) return;

        // Satu genangan tipis menggantikan seluruh bentuk serbuk/kristal setelah diaduk.
        TampilSpot(1, 0, warnaHasil[1, 0], false, true);
        SpotData spot = grid[1, 0];
        if (spot.silinder == null) return;

        Bounds batasPlate = AmbilBoundsPlateLokal();
        float diameter = Mathf.Min(batasPlate.extents.x, batasPlate.extents.y) * 0.18f;
        spot.silinder.transform.localPosition = KoordinatSpotLokal(1, 0) + Vector3.forward * 0.004f;
        spot.silinder.transform.localScale = new Vector3(diameter, diameter, diameter * 0.10f);
        spot.bentukCairan = true;
        SetWarna(spot.mat, warnaHasil[1, 0]);
    }

    private void HapusEfekKelarutanPermanen()
    {
        foreach (GameObject efek in efekKelarutanPermanen)
        {
            if (efek == null) continue;
            Renderer rendererEfek = efek.GetComponent<Renderer>();
            if (rendererEfek != null && rendererEfek.sharedMaterial != null) Destroy(rendererEfek.sharedMaterial);
            Destroy(efek);
        }
        efekKelarutanPermanen.Clear();
    }

    void TambahIsian(int kolom, int baris)
    {
        int kolomGrid = kolom == 0 ? baris : kolom;
        int barisGrid = kolom == 0 ? BarisSumberAktif : baris;
        if (grid[kolomGrid, barisGrid].terisi) return;
        hitungIsian[kolom]++;
        if (kolom == 0)
        {
            // Soal 2 memakai tiga lubang mendatar pada baris paling bawah.
            TampilSpot(kolomGrid, barisGrid, warnaSampel[kolom]);
            PerbaruiPenandaAsam();
        }
        else TampilSpot(kolomGrid, barisGrid, warnaSampel[kolom]);
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
        local.z += offsetNormal;
        return platTransform.TransformPoint(local);
    }
    private const float KedalamanPenandaSpot = -0.14f;

    private Vector3 KoordinatSpotLokal(int kolom, int baris)
    {
        Bounds b = AmbilBoundsPlateLokal();
        float x = b.center.x + (kolom - 1.5f) * b.extents.x * 0.44f;
        // Empat lubang mendatar: tiga lubang pada baris ketiga dipakai, lubang keempat kosong.
        float y = b.center.y + (1 - baris) * b.extents.y * 0.58f;
        // Bidang lubang berada pada bidang lokal X/Y dan permukaan atas menghadap sumbu Z.
        return new Vector3(x, y, b.max.z + 0.001f);
    }

    private bool CariLubangPetroleum(Vector2 posLayar, Camera cam, out int baris)
    {
        baris = 0;
        if (platTransform == null || cam == null || !PosisiLayarDiAtasPlat(posLayar, cam)) return false;

        float jarakTerdekat = float.MaxValue;
        bool targetTerisi = false;
        for (int i = 0; i < 3; i++)
        {
            Vector3 layar = cam.WorldToScreenPoint(PosisiLubangSpot(i, BarisPetroleum, 0.001f));
            if (layar.z <= 0f) continue;
            float jarak = Vector2.Distance(posLayar, new Vector2(layar.x, layar.y));
            if (jarak < jarakTerdekat)
            {
                jarakTerdekat = jarak;
                baris = i;
                targetTerisi = grid[i, BarisPetroleum] != null && grid[i, BarisPetroleum].terisi;
            }
        }

        // Penerimaan mengikuti pusat lingkaran target, dengan toleransi sentuhan jari.
        return jarakTerdekat <= 42f && !targetTerisi;
    }

    private bool CariLubangEtanol(Vector2 posLayar, Camera cam, out int kolom)
    {
        kolom = -1;
        if (platTransform == null || cam == null || !PosisiLayarDiAtasPlat(posLayar, cam)) return false;

        float jarakTerdekat = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            Vector3 layar = cam.WorldToScreenPoint(PosisiLubangSpot(i, 1, 0.001f));
            if (layar.z <= 0f) continue;
            float jarak = Vector2.Distance(posLayar, new Vector2(layar.x, layar.y));
            if (jarak < jarakTerdekat)
            {
                jarakTerdekat = jarak;
                kolom = i;
            }
        }

        return kolom >= 0 && jarakTerdekat <= 42f && !etanolDitetes[kolom];
    }

    bool CariSpot(Vector2 posLayar, Camera cam, out int kolom, out int baris)
    {
        kolom = baris = 0;
        if (platTransform == null || cam == null || !PosisiLayarDiAtasPlat(posLayar, cam)) return false;

        // Gunakan proyeksi layar dari pusat lubang yang sama dengan penanda UI. Ini menjaga
        // hit-test tetap akurat saat plate miring dan modelnya memakai pivot yang tidak di tengah.
        float jarakTerdekat = float.MaxValue;
        for (int k = 0; k < 3; k++)
        {
            for (int b = 0; b < 3; b++)
            {
                Vector3 layar = cam.WorldToScreenPoint(PosisiLubangSpot(k, b, 0.001f));
                if (layar.z <= 0f) continue;
                float jarak = Vector2.Distance(posLayar, new Vector2(layar.x, layar.y));
                if (jarak < jarakTerdekat)
                {
                    jarakTerdekat = jarak;
                    kolom = k;
                    baris = b;
                }
            }
        }
        return jarakTerdekat < float.MaxValue;
    }
    void TampilSpot(int k, int b, Color warna, bool asamPadaBarisHorizontal = false, bool petroleumCair = false)
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
            Vector3 posisiSpotLokal = KoordinatSpotLokal(asamPadaBarisHorizontal ? b : k, asamPadaBarisHorizontal ? 0 : b);
            go.transform.localPosition = posisiSpotLokal;
            Bounds boundsPlate = AmbilBoundsPlateLokal();
            float diameterSpot = Mathf.Min(boundsPlate.extents.x, boundsPlate.extents.y) * (petroleumCair ? 0.18f : 0.20f);
            if (petroleumCair)
            {
                // Genangan tipis berada di atas bidang plate, bukan bola padat yang menutupi lubang.
                go.transform.localScale = new Vector3(diameterSpot, diameterSpot, diameterSpot * 0.10f);
                go.transform.localPosition = posisiSpotLokal + Vector3.forward * 0.003f;
            }
            else
            {
                go.transform.localScale = new Vector3(diameterSpot, diameterSpot, diameterSpot);
                go.transform.localPosition = posisiSpotLokal - Vector3.forward * (diameterSpot * 0.15f);
            }
            Material mat = petroleumCair
                ? BuatMaterialTetesPetroleum(warna)
                : (matLarutan != null ? new Material(matLarutan) : new Material(Shader.Find("Universal Render Pipeline/Lit")));
            go.GetComponent<Renderer>().sharedMaterial = mat;
            MatikanBayangan(go);
            s.silinder = go;
            s.mat = mat;
            s.bentukCairan = petroleumCair;
        }
        if (petroleumCair && s.silinder != null && !s.bentukCairan)
        {
            Vector3 lokal = KoordinatSpotLokal(asamPadaBarisHorizontal ? b : k, asamPadaBarisHorizontal ? 0 : b);
            Bounds boundsPlate = AmbilBoundsPlateLokal();
            float diameter = Mathf.Min(boundsPlate.extents.x, boundsPlate.extents.y) * 0.18f;
            s.silinder.transform.localPosition = lokal + Vector3.forward * 0.003f;
            s.silinder.transform.localScale = new Vector3(diameter, diameter, diameter * 0.10f);
            Material cair = BuatMaterialTetesPetroleum(warna);
            if (s.mat != null) Destroy(s.mat);
            s.mat = cair;
            s.silinder.GetComponent<Renderer>().sharedMaterial = cair;
            s.bentukCairan = true;
        }
        Color warnaTampil = warna;
        if (petroleumCair) warnaTampil.a = 0.62f;
        SetWarna(s.mat, warnaTampil);
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
        // Popup keberhasilan hijau pada animasi Percobaan 2 tidak ditampilkan.
        instruksiAdukAktif = false;
        if (panelNotif != null) panelNotif.SetActive(false);
    }

    IEnumerator TutupNotif()
    {
        yield return new WaitForSeconds(durasiNotif);
        if (!instruksiAdukAktif && panelNotif != null) panelNotif.SetActive(false);
    }

    static float Ease(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }

    // Animasi tusuk gigi aduk di satu spot
    IEnumerator AnimAduk(int k, int b, GameObject tusuk, Camera cam)
    {
        if (platTransform == null || tusuk == null) yield break;
        var s = grid[k, b]; if (s == null || !s.terisi) yield break;
        Vector3 centerLok = KoordinatSpotLokal(k, b) + Vector3.forward * 0.006f;
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
                Bounds batasPlate = AmbilBoundsPlateLokal();
                float r = Mathf.Min(batasPlate.extents.x, batasPlate.extents.y) * 0.035f;
                Vector3 off = platTransform.TransformVector(new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * r, Mathf.Sin(angle * Mathf.Deg2Rad) * r, 0f));
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
            tusuk = UnityEngine.Object.Instantiate(objekTusukGigi);
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
        go.transform.position = tujuan + platTransform.forward * 0.5f;
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
        var s = grid[k, b];
        if (s == null || s.silinder == null) yield break;
        // Dua fase cairan terlihat sebagai dua genangan tipis yang berdampingan di dalam satu lubang.
        Vector3 pusatLokal = s.silinder.transform.localPosition;
        float diameter = s.silinder.transform.localScale.x;
        s.silinder.transform.localScale = new Vector3(diameter * 0.54f, diameter * 0.86f, diameter * 0.10f);
        s.silinder.transform.localPosition = pusatLokal - Vector3.right * diameter * 0.22f;
        SetWarna(s.mat, warnaEter);
        var go2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go2.GetComponent<Collider>());
        go2.transform.SetParent(platTransform, false);
        go2.name = "LapisanAquadesTidakLarut";
        go2.transform.localPosition = pusatLokal + Vector3.right * diameter * 0.22f + Vector3.forward * 0.001f;
        go2.transform.localScale = new Vector3(diameter * 0.54f, diameter * 0.86f, diameter * 0.10f);
        var mat2 = BuatMaterialTetesPetroleum(warnaAir);
        SetWarna(mat2, warnaAir);
        go2.GetComponent<Renderer>().sharedMaterial = mat2;
        MatikanBayangan(go2);
        efekKelarutanPermanen.Add(go2);
        yield return new WaitForSeconds(0.25f);
    }

    IEnumerator EfekSedikitLarut(int k, int b, Color warnaC)
    {
        if (platTransform == null || grid[k, b] == null) yield break;
        Vector3 pusatLokal = KoordinatSpotLokal(k, b) + Vector3.forward * 0.009f;
        float diameter = grid[k, b].silinder != null ? grid[k, b].silinder.transform.localScale.x :
            Mathf.Min(AmbilBoundsPlateLokal().extents.x, AmbilBoundsPlateLokal().extents.y) * 0.14f;
        var partikel = new List<GameObject>();
        Vector3[] offset =
        {
            new Vector3(-0.24f, 0.13f, 0f), new Vector3(0.20f, 0.18f, 0f),
            new Vector3(-0.12f, -0.20f, 0f), new Vector3(0.25f, -0.12f, 0f),
            new Vector3(0.02f, 0.02f, 0f)
        };
        for (int i = 0; i < 5; i++)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(p.GetComponent<Collider>());
            p.name = "PartikelBelumLarut";
            p.transform.SetParent(platTransform, false);
            p.transform.localPosition = pusatLokal + new Vector3(offset[i].x * diameter, offset[i].y * diameter, 0.002f);
            float ukuran = diameter * (i == 4 ? 0.14f : 0.10f);
            p.transform.localScale = Vector3.one * ukuran;
            var m = new Material(matPadatan != null ? matPadatan : new Material(Shader.Find("Universal Render Pipeline/Lit")));
            Color warnaP = warnaC * 0.6f; warnaP.a = 0.9f;
            SetWarna(m, warnaP);
            p.GetComponent<Renderer>().sharedMaterial = m;
            MatikanBayangan(p);
            partikel.Add(p);
            efekKelarutanPermanen.Add(p);
        }
        Vector3[] posisiAkhir = new Vector3[partikel.Count];
        for (int i = 0; i < partikel.Count; i++)
        {
            posisiAkhir[i] = partikel[i].transform.localPosition;
            partikel[i].transform.localPosition += Vector3.forward * diameter * 0.16f;
        }
        float t = 0f;
        while (t < 0.45f)
        {
            t += Time.deltaTime;
            for (int i = 0; i < partikel.Count; i++)
                if (partikel[i] != null)
                    partikel[i].transform.localPosition = Vector3.Lerp(partikel[i].transform.localPosition, posisiAkhir[i], Ease(t / 0.45f));
            yield return null;
        }
    }

    IEnumerator AnimTespen(int k, int b, Camera cam)
    {
        if (objekTespen == null) { animAktif = null; yield break; }
        GameObject tespen = UnityEngine.Object.Instantiate(objekTespen);
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
