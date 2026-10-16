using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di objek yang selalu aktif (mis. ScriptLogic/ManajerLatihan).
/// Mengatur langkah takaran: bahan dari kartu ScrollBahan dijatuhkan ke tabung di rak.
///  - Tuang  (botol akuades): muncul pilihan 1/2/3 ml. Salah = kartu salah, benar = botol miring dan cairan menetes.
///  - Tetes  (pipet tetes): tekan tombol Tetes sebanyak yang diinginkan lalu Selesai. Jumlah tetes salah = kartu salah.
/// Warna cairan dicampur menurut volume tiap bahan. Langkah dengan efek reaksi (Pb(NO3)2) menampilkan animasi
/// tercampur, larutan keruh dan endapan putih PbCl2, lalu tabung dituang ke gelas kimia.
/// Soal baru bisa lanjut kalau semua langkah takaran soal itu selesai (dicek lewat TakaranSelesai).
/// </summary>
public class ManajerTakaran : MonoBehaviour
{
    public static ManajerTakaran Instance { get; private set; }

    public enum Jenis { Tuang, Tetes }

    [System.Serializable]
    public class LangkahTakaran
    {
        public string nama;
        [Tooltip("Nama bahan untuk teks petunjuk (mis. akuades, NaCl, CHCl3).")]
        public string namaBahan = "akuades";
        [Tooltip("Panel soal tempat langkah ini berlaku.")]
        public GameObject panelSoal;
        [Tooltip("Kartu bahan di ScrollBahan yang diambil untuk langkah ini.")]
        public GameObject kartu;
        [Tooltip("Kartu sumber cairan yang harus dijatuhkan ke targetSumber sebelum alat diambil (opsional).")]
        public GameObject kartuSumber;
        [Tooltip("Area penempatan sumber cairan di layar.")]
        public RectTransform targetSumber;
        [HideInInspector] public GameObject modelSumber;
        [Tooltip("Tabung (SlotA / SlotB) yang harus diisi.")]
        public SlotTabung tabungTarget;
        public Jenis jenis = Jenis.Tuang;
        [Tooltip("Tuang: jumlah ml yang benar. Tetes: jumlah tetes yang benar.")]
        public int jumlahBenar = 1;
        [Tooltip("Warna bahan ini. Warna di tabung = campuran semua bahan menurut volumenya.")]
        public Color warnaCairan = new Color(0.85f, 0.94f, 1f, 1f);
        [Tooltip("Titik ujung tempat cairan keluar, di ruang model asli (sebelum diputar).")]
        public Vector3 titikKeluar;
        [Tooltip("Putaran sumbu Z (derajat) supaya ujung menghadap ke bawah.")]
        public float sudutMiring = 60f;
        [Tooltip("Tinggi ujung di atas mulut tabung (satuan model rak). Naikkan supaya botol tidak tertutup rak.")]
        public float tinggiDiAtasTabung = 0.6f;
        [Tooltip("Putaran awal model (Euler) di dalam rig, sebelum diputar sudutMiring. Isi sama dengan rotasi prefab kalau model di FBX sudah tegak (mis. pipet baru). Nol = pakai arah mentah model.")]
        public Vector3 rotasiModel = Vector3.zero;

        [Header("Efek reaksi (opsional)")]
        [Tooltip("Setelah tetesan masuk, tampilkan animasi tercampur dan warna berubah halus.")]
        public bool efekCampur;
        [Tooltip("Larutan menjadi keruh dan terbentuk endapan putih (PbCl2).")]
        public bool bentukEndapan;
        [Tooltip("Setelah reaksi, isi tabung dituang ke gelas kimia (menjadi larutan putih keabu-abuan).")]
        public bool tuangKeGelas;

        [HideInInspector] public bool selesai;
        [HideInInspector] public bool sedangDiproses;
    }

    private class PermintaanSesi
    {
        public LangkahTakaran langkah;
        public GameObject model;
        public Camera kamera;
    }
    [Header("=== LANGKAH TAKARAN ===")]
    public List<LangkahTakaran> langkah = new List<LangkahTakaran>();
    [Tooltip("Semua tabung (SlotA, SlotB). Dilepas di tabung yang salah = kartu salah.")]
    public SlotTabung[] semuaTabung;

    [Header("=== PANEL UI ===")]
    public GameObject panelTakaran;
    public GameObject grupVolume;
    public GameObject grupTetes;
    public TMP_Text teksJudul;
    public TMP_Text teksTetes;
    public Button[] tombolVolume;
    public int[] nilaiVolume = { 1, 2, 3 };
    public Button tombolTetes;
    public Button tombolSelesai;
    public Button tombolBatal;

    [Header("=== TAMPILAN CAIRAN ===")]
    [Tooltip("Bahan (transparan) untuk tetesan yang jatuh.")]
    public Material materialTetes;
    [Tooltip("Bahan (transparan) untuk cairan di dalam tabung.")]
    public Material materialCairan;
    [Tooltip("Bahan kaca tabung yang transparan. Dipasang ke tabung saat cairan pertama masuk supaya isinya terlihat. Kosong = tidak diganti.")]
    public Material materialKaca;

    [Header("=== UKURAN (satuan model rak) ===")]
    public float panjangTabung = 4.2f;
    public float jarakDasarKaca = 0.12f;
    public float radiusCairan = 0.38f;
    public float tinggiPerMl = 0.4f;
    public float tinggiPerTetes = 0.04f;
    public float maksTinggiCairan = 3.2f;
    public int tetesPerMlAnimasi = 14;
    public float ukuranTetes = 0.16f;
    public int maksKlikTetes = 12;

    [Header("=== WARNA CAMPURAN ===")]
    [Tooltip("Seberapa pekat warna satu tetes dibanding volumenya. Makin besar, warna tetesan makin terlihat di tabung.")]
    public float kekuatanWarnaTetes = 3f;

    [Header("=== REAKSI PbCl2 DI TABUNG ===")]
    public Color warnaLarutanKeruh = new Color(0.93f, 0.93f, 0.9f, 1f);
    public Color warnaEndapan = new Color(0.98f, 0.98f, 0.96f, 1f);
    public float tinggiEndapanTabung = 0.3f;
    public int jumlahButirEndapan = 26;

    [Header("=== GELAS KIMIA ===")]
    [Tooltip("Model gelas kimia (GelasKimiaAkuades.fbx). Harus punya anak PbCl2_Liquid dan PbCl2_Sediment.")]
    public GameObject prefabGelasKimia;
    [Tooltip("Warna larutan di gelas kimia (putih keabu-abuan).")]
    public Color warnaLarutanGelas = new Color(0.84f, 0.84f, 0.86f, 1f);
    [Tooltip("Setengah lebar rak (satuan model rak).")]
    public float setengahLebarRak = 3.5f;
    public float tinggiRak = 4.2f;
    [Tooltip("Jarak gelas dari sisi rak.")]
    public float jarakGelasDariRak = 1.7f;
    [Tooltip("Gelas dan tabung yang dituang digeser ke depan rak (ke arah kamera) sebesar ini supaya tidak menembus rak.")]
    public float jarakDepanRak = 2f;
    public float tinggiGelas = 2.06f;
    [Tooltip("Tinggi mulut tabung di atas bibir gelas saat menuang.")]
    public float mulutDiAtasGelas = 0.9f;
    [Tooltip("Sudut miring tabung saat menuang (derajat).")]
    public float sudutTuangGelas = 100f;
    public float lamaTuang = 1.5f;
    [Tooltip("Dasar dan tinggi cairan PbCl2 di dalam model gelas kimia.")]
    public float dasarCairanGelas = 0.2f;
    public float tinggiCairanGelas = 0.96f;

    private class CairanTabung
    {
        public Transform silinder;
        public Material mat;
        public float tinggi;
        public Color warna;
        public float bobot;
        public Color warnaTunda;
        public float bobotTunda;
        public float alphaDasar = 0.6f;
        public float alphaTampil = 0.6f;
        public Transform endapan;
        public Material matEndapan;
        public float tinggiEndapan;
    }

    private class GelasKimia
    {
        public Transform root;
        public Transform pivotCairan;
        public Transform pivotEndapan;
        public List<GameObject> butir = new List<GameObject>();
    }

    private readonly Dictionary<SlotTabung, CairanTabung> cairan = new Dictionary<SlotTabung, CairanTabung>();
    private bool sesiAktif;
    private readonly Queue<PermintaanSesi> antreanSesi = new Queue<PermintaanSesi>();
    private bool memprosesAntrean;
    public bool SedangMemproses { get { return sesiAktif; } }
    private int pilihanVolume = -1;
    private bool batal;
    private int permintaanTetes;
    private bool tekanSelesai;
    private int tetesAktif;
    private GameObject rigAktif;
    private GameObject rigTuang;
    private GameObject gelasAktif;
    private Transform rakTersembunyi;
    private readonly Dictionary<SlotTabung, RectTransform> bayanganCairan = new Dictionary<SlotTabung, RectTransform>();
    private readonly Dictionary<SlotTabung, Vector2> ukuranAwalBayangan = new Dictionary<SlotTabung, Vector2>();

    private void Start()
    {
        if (semuaTabung == null) return;
        for (int i = 0; i < semuaTabung.Length; i++)
            PerbaruiBayangan(semuaTabung[i], 0f);
    }

    private void Update()
    {
        // Target sumber dipakai bersama oleh beberapa soal. Atur tiap target sekali saja,
        // berdasarkan langkah yang panelnya sedang terbuka, agar soal berikutnya tidak mematikannya.
        for (int i = 0; i < langkah.Count; i++)
        {
            LangkahTakaran acuan = langkah[i];
            if (acuan == null || acuan.targetSumber == null || acuan.kartuSumber == null) continue;
            bool sudahDiproses = false;
            for (int p = 0; p < i; p++)
                if (langkah[p] != null && langkah[p].targetSumber == acuan.targetSumber) { sudahDiproses = true; break; }
            if (sudahDiproses) continue;
            LangkahTakaran langkahAktif = null;
            for (int j = 0; j < langkah.Count; j++)
            {
                LangkahTakaran kandidat = langkah[j];
                if (kandidat != null && kandidat.targetSumber == acuan.targetSumber && kandidat.kartuSumber != null &&
                    !kandidat.selesai && kandidat.modelSumber == null &&
                    (kandidat.panelSoal == null || kandidat.panelSoal.activeInHierarchy))
                { langkahAktif = kandidat; break; }
            }
            bool tampil = langkahAktif != null;
            UnityEngine.UI.RawImage gambarKotak = acuan.targetSumber.GetComponent<UnityEngine.UI.RawImage>();
            if (gambarKotak != null)
            {
                if (gambarKotak.texture == null) gambarKotak.texture = Texture2D.whiteTexture;
                gambarKotak.color = new Color(0.78f, 0.78f, 0.78f, 0.34f);
                gambarKotak.raycastTarget = false;
                if (gambarKotak.enabled != tampil) gambarKotak.enabled = tampil;
                if (tampil) acuan.targetSumber.transform.SetAsLastSibling();
            }
            Transform label = acuan.targetSumber.Find("LabelSumber");
            if (label != null)
            {
                TMP_Text teksSumber = label.GetComponentInChildren<TMP_Text>(true);
                if (teksSumber != null && tampil)
                {
                    string namaLabu = langkahAktif.kartuSumber.name == "NACL" ? "Labu NaCl" : langkahAktif.kartuSumber.name == "CHCL3" ? "Labu CHCl3" : langkahAktif.kartuSumber.name == "PBNO3" ? "Labu Pb(NO3)2" : "Labu " + langkahAktif.kartuSumber.name;
                    teksSumber.text = "Letakkan\n" + namaLabu;
                }
                if (label.gameObject.activeSelf != tampil) label.gameObject.SetActive(tampil);
            }
        }
        if (semuaTabung != null)
        {
            for (int i = 0; i < semuaTabung.Length; i++)
            {
                SlotTabung slot = semuaTabung[i]; if (slot == null) continue;
                Transform marker = slot.transform.Find("BayanganPipet"); if (marker == null) continue;
                bool show = false;
                for (int j = 0; j < langkah.Count; j++) { LangkahTakaran l = langkah[j]; if (l != null && !l.selesai && !l.sedangDiproses && l.tabungTarget == slot && (l.kartuSumber == null || l.modelSumber != null) && (l.panelSoal == null || l.panelSoal.activeInHierarchy)) { show = true; break; } }
                if (marker.gameObject.activeSelf != show) marker.gameObject.SetActive(show);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        if (panelTakaran != null) panelTakaran.SetActive(false);

        if (tombolVolume != null)
        {
            for (int i = 0; i < tombolVolume.Length; i++)
            {
                if (tombolVolume[i] == null) continue;
                int nilai = (nilaiVolume != null && i < nilaiVolume.Length) ? nilaiVolume[i] : i + 1;
                tombolVolume[i].onClick.AddListener(() => { pilihanVolume = nilai; });
            }
        }
        if (tombolTetes != null) tombolTetes.onClick.AddListener(() => { permintaanTetes++; });
        if (tombolSelesai != null) tombolSelesai.onClick.AddListener(() => { tekanSelesai = true; });
        if (tombolBatal != null) tombolBatal.onClick.AddListener(() => { batal = true; });
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------- Dipakai ManajerPilihItem ----------

    /// <summary>True kalau semua langkah takaran untuk panel soal ini sudah selesai (atau soal ini tidak punya takaran).</summary>
    public bool TakaranSelesai(GameObject panelSoal)
    {
        for (int i = 0; i < langkah.Count; i++)
        {
            LangkahTakaran l = langkah[i];
            if (l != null && l.panelSoal == panelSoal && !l.selesai) return false;
        }
        return true;
    }

    /// <summary>Kosongkan semua cairan, gelas kimia, dan kunci ulang langkah (dipanggil saat progres di-reset).</summary>
    public void ResetSemua()
    {
        StopAllCoroutines();
        if (rigAktif != null) Destroy(rigAktif);
        rigAktif = null;
        if (rigTuang != null) Destroy(rigTuang);
        rigTuang = null;
        if (gelasAktif != null) Destroy(gelasAktif);
        gelasAktif = null;
        sesiAktif = false;
        memprosesAntrean = false;
        while (antreanSesi.Count > 0)
        {
            PermintaanSesi tertunda = antreanSesi.Dequeue();
            if (tertunda.langkah != null) tertunda.langkah.sedangDiproses = false;
            if (tertunda.model != null) Destroy(tertunda.model);
        }
        tetesAktif = 0;
        if (panelTakaran != null) panelTakaran.SetActive(false);
        foreach (KeyValuePair<SlotTabung, CairanTabung> kv in cairan)
        {
            if (kv.Value == null) continue;
            if (kv.Value.silinder != null) Destroy(kv.Value.silinder.gameObject);
            if (kv.Value.endapan != null) Destroy(kv.Value.endapan.gameObject);
        }
        cairan.Clear();
        for (int i = 0; i < langkah.Count; i++)
            if (langkah[i] != null)
            {
                langkah[i].selesai = false;
                langkah[i].sedangDiproses = false;
                if (langkah[i].modelSumber != null) Destroy(langkah[i].modelSumber);
                langkah[i].modelSumber = null;
            }
        // Hancurkan rak Soal6 yang renderer-nya disembunyikan
        if (rakTersembunyi != null)
        {
            // Pisahkan semua anak dulu (rig/tabung) supaya tidak ikut terhapus
            while (rakTersembunyi.childCount > 0)
                rakTersembunyi.GetChild(0).SetParent(null, true);
            Destroy(rakTersembunyi.gameObject);
            rakTersembunyi = null;
        }
        // Kosongkan semua slot tabung (tabung di-destroy oleh Kosongkan)
        if (semuaTabung != null)
        {
            for (int i = 0; i < semuaTabung.Length; i++)
            {
                if (semuaTabung[i] != null) semuaTabung[i].Kosongkan();
            }
        }
        // Hancurkan sisa anak wadah Model3D_Spawn yang mungkin tertinggal
        GameObject wadah = GameObject.Find("Model3D_Spawn");
        if (wadah != null)
        {
            while (wadah.transform.childCount > 0)
            {
                Transform anak = wadah.transform.GetChild(0);
                anak.SetParent(null);
                Destroy(anak.gameObject);
            }
        }
    }

    // ---------- Dipakai CardTahanGeser3D ----------

    public void Sorot(Vector2 posisiLayar, bool aktif, Camera cam) { }

    /// <summary>Dipanggil saat kartu bahan dilepas. True = model dipakai di sini (jangan dihapus oleh kartu).</summary>
    public bool CobaTaruh(Vector2 posisiLayar, GameObject model, Camera cam, float kedalaman, GameObject kartu)
    {
        if (model == null || cam == null) return false;
        LangkahTakaran langkahSumber = CariLangkahSumber(kartu);
        if (langkahSumber != null)
        {
            RectTransform targetSumber = langkahSumber.targetSumber;
            if (targetSumber == null || !RectTransformUtility.RectangleContainsScreenPoint(targetSumber, posisiLayar, KameraUI(targetSumber, cam)))
                return false;

            if (langkahSumber.modelSumber != null && langkahSumber.modelSumber != model)
                Destroy(langkahSumber.modelSumber);

            Vector2 titikLayar = RectTransformUtility.WorldToScreenPoint(KameraUI(targetSumber, cam), targetSumber.position);
            Vector3 pusatKotak = cam.ScreenToWorldPoint(new Vector3(titikLayar.x, titikLayar.y, kedalaman));
            Renderer[] renderersSumber = model.GetComponentsInChildren<Renderer>(true);
            if (renderersSumber.Length > 0)
            {
                Bounds boundsVisual = renderersSumber[0].bounds;
                for (int i = 1; i < renderersSumber.Length; i++) boundsVisual.Encapsulate(renderersSumber[i].bounds);
                model.transform.position = pusatKotak - (boundsVisual.center - model.transform.position);
            }
            else model.transform.position = pusatKotak;
            model.transform.rotation = cam.transform.rotation;
            langkahSumber.modelSumber = model;
            // Soal yang memakai sumber kimia sama berbagi satu model labu yang diletakkan.
            for (int i = 0; i < langkah.Count; i++)
            {
                LangkahTakaran terkait = langkah[i];
                if (terkait != null && terkait != langkahSumber && terkait.kartuSumber == kartu && terkait.panelSoal == langkahSumber.panelSoal && !terkait.selesai)
                    terkait.modelSumber = model;
            }
            return true;
        }

        LangkahTakaran l = CariLangkah(kartu);
        if (l == null) return false;
        if (l.kartuSumber != null && l.modelSumber == null)
        {
            if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();
            return false;
        }
        SlotTabung target = l.tabungTarget;
        if (target == null || !target.SelesaiMasuk || target.Model == null) return false;

        if (!target.Mengandung(posisiLayar, cam))
        {
            if (semuaTabung != null)
            {
                for (int i = 0; i < semuaTabung.Length; i++)
                {
                    SlotTabung s = semuaTabung[i];
                    if (s != null && s != target && s.Mengandung(posisiLayar, cam))
                    {
                        if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();
                        return false;
                    }
                }
            }
            return false;
        }

        // Terima drop kedua yang benar ketika animasi tetesan pertama masih berlangsung.
        l.sedangDiproses = true;
        PermintaanSesi permintaan = new PermintaanSesi { langkah = l, model = model, kamera = cam };
        if (sesiAktif || memprosesAntrean)
        {
            antreanSesi.Enqueue(permintaan);
            return true;
        }
        StartCoroutine(Sesi(l, model, cam));
        return true;
    }

    private LangkahTakaran CariLangkah(GameObject kartu)
    {
        for (int i = 0; i < langkah.Count; i++)
        {
            LangkahTakaran l = langkah[i];
            if (l == null || l.selesai || l.sedangDiproses || l.kartu != kartu) continue;
            if (l.panelSoal != null && !l.panelSoal.activeInHierarchy) continue;
            return l;
        }
        return null;
    }

    private LangkahTakaran CariLangkahSumber(GameObject kartu)
    {
        for (int i = 0; i < langkah.Count; i++)
        {
            LangkahTakaran l = langkah[i];
            if (l == null || l.kartuSumber != kartu || l.selesai || l.sedangDiproses) continue;
            if (l.panelSoal != null && !l.panelSoal.activeInHierarchy) continue;
            return l;
        }
        return null;
    }

    private static Camera KameraUI(RectTransform target, Camera fallback)
    {
        Canvas canvas = target != null ? target.GetComponentInParent<Canvas>() : null;
        if (canvas == null) return fallback;
        Canvas root = canvas.rootCanvas;
        if (root.renderMode == RenderMode.ScreenSpaceCamera) return root.worldCamera;
        if (root.renderMode == RenderMode.WorldSpace) return root.worldCamera != null ? root.worldCamera : fallback;
        return null;
    }

    // ---------- Sesi takaran ----------

    private static float EaseInOut(float k)
    {
        k = Mathf.Clamp01(k);
        return k * k * (3f - 2f * k);
    }

    private void TampilkanPanel(bool tampil, LangkahTakaran l)
    {
        if (panelTakaran == null) return;
        panelTakaran.SetActive(tampil);
        if (!tampil) return;
        bool tuang = l.jenis == Jenis.Tuang;
        if (grupVolume != null) grupVolume.SetActive(tuang);
        if (grupTetes != null) grupTetes.SetActive(!tuang);
        string tabung = l.tabungTarget != null ? l.tabungTarget.namaSlot : "?";
        if (teksJudul != null)
        {
            teksJudul.text = tuang
                ? "Berapa ml " + l.namaBahan + " yang dituang ke tabung " + tabung + "?"
                : "Teteskan " + l.namaBahan + " ke tabung " + tabung + ", lalu tekan Selesai";
        }
    }

    private void UpdateTeksTetes(int hitung)
    {
        if (teksTetes != null) teksTetes.text = "Tetes: " + hitung;
    }

    private IEnumerator Sesi(LangkahTakaran l, GameObject model, Camera cam)
    {
        sesiAktif = true;
        batal = false;
        pilihanVolume = -1;
        permintaanTetes = 0;
        tekanSelesai = false;
        tetesAktif = 0;

        SlotTabung slot = l.tabungTarget;
        Transform rak = slot.Model != null ? slot.Model.transform.parent : null;
        if (rak == null)
        {
            Destroy(model);
            l.sedangDiproses = false;
            sesiAktif = false;
            yield break;
        }

        InfoModel3D im = model.GetComponent<InfoModel3D>();
        InfoModel3D ir = rak.GetComponent<InfoModel3D>();
        Vector3 skalaAsset = im != null ? im.skalaAsset : Vector3.one;
        float fRak = ir != null ? ir.faktorSkala : 1f;

        Vector3 tipMulai = model.transform.TransformPoint(l.titikKeluar);
        float skalaMulai = Mathf.Abs(model.transform.lossyScale.x);
        float lambdaMulai = skalaMulai / Mathf.Max(0.0001f, fRak * Mathf.Abs(skalaAsset.x));

        // Rig: titik pusat = ujung keluar cairan. Model jadi anak rig, jadi diputar mengelilingi ujungnya.
        GameObject rigGO = new GameObject("Rig_" + l.nama);
        rigAktif = rigGO;
        Transform root = rigGO.transform;
        root.SetParent(rak, false);
        root.position = tipMulai;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one * lambdaMulai;

        model.transform.SetParent(root, false);
        model.transform.localScale = skalaAsset;
        Quaternion rotModel = Quaternion.Euler(l.rotasiModel);
        model.transform.localRotation = rotModel;
        model.transform.localPosition = -(rotModel * Vector3.Scale(l.titikKeluar, skalaAsset));
        GameObject cairanPipet = null;
            if (materialKaca != null && l.jenis == Jenis.Tetes)
            {
                // Hanya pipet tetes yang dibuat tembus pandang; botol akuades mempertahankan warna aslinya
                Renderer[] rsModel = model.GetComponentsInChildren<Renderer>(true);
                for (int iR = 0; iR < rsModel.Length; iR++) rsModel[iR].sharedMaterial = materialKaca;
            }

        try
        {
            // Untuk langkah yang memakai sumber cairan, pipet bergerak ke labu,
            // masuk ke mulut labu, lalu memperlihatkan cairan yang tersedot.
            if (l.modelSumber != null && l.jenis == Jenis.Tetes)
            {
                Renderer[] rendererSumber = l.modelSumber.GetComponentsInChildren<Renderer>();
                if (rendererSumber.Length > 0)
                {
                    Bounds boundsSumber = rendererSumber[0].bounds;
                    for (int i = 1; i < rendererSumber.Length; i++) boundsSumber.Encapsulate(rendererSumber[i].bounds);
                    Vector3 mulutSumber = new Vector3(boundsSumber.center.x, boundsSumber.max.y, boundsSumber.center.z);
                    Vector3 diAtasLabu = rak.InverseTransformPoint(mulutSumber + Vector3.up * 0.35f);
                    // Masukkan ujung pipet sedikit ke dalam bukaan leher labu.
                    Vector3 diMulutLabu = rak.InverseTransformPoint(mulutSumber + Vector3.down * 0.015f);

                    yield return GerakRig(root, diAtasLabu, 0f, 1.05f);
                    yield return GerakRig(root, diMulutLabu, 0f, 0.75f);
                    cairanPipet = BuatCairanPipet(root, l.warnaCairan);
                    yield return IsiPipet(cairanPipet, 0.9f);
                    yield return GerakRig(root, diAtasLabu, 0f, 0.75f);
                }
            }

            // 1. Terbang ke atas mulut tabung (botol/pipet dinaikkan sebesar tinggiDiAtasTabung)
            bool pipetDariSumber = l.jenis == Jenis.Tetes && l.modelSumber != null;
            float sudutAwal = pipetDariSumber ? 0f : (l.jenis == Jenis.Tetes ? l.sudutMiring : l.sudutMiring * 0.85f);
            Vector3 pourLokal = new Vector3(slot.lubangLokal.x, slot.lubangLokal.y + panjangTabung + l.tinggiDiAtasTabung, slot.lubangLokal.z);
            Vector3 awalTerbang = root.position;
            float t = 0f;
            float durTerbang = pipetDariSumber ? 1.1f : 0.5f;
            while (t < durTerbang)
            {
                if (rak == null) yield break;
                t += Time.deltaTime;
                float e = EaseInOut(t / durTerbang);
                root.position = Vector3.Lerp(awalTerbang, rak.TransformPoint(pourLokal), e);
                root.localRotation = Quaternion.Euler(0f, 0f, sudutAwal * e);
                root.localScale = Vector3.one * Mathf.Lerp(lambdaMulai, 1f, e);
                yield return null;
            }
            root.localPosition = pourLokal;
            root.localRotation = Quaternion.Euler(0f, 0f, sudutAwal);
            root.localScale = Vector3.one;
            if (pipetDariSumber) yield return GerakRig(root, pourLokal, pourLokal + Vector3.down * 0.025f, 0f, 0f, 0.75f);

            if (l.jenis == Jenis.Tuang)
            {
                // 2a. Pilih volume
                TampilkanPanel(true, l);
                while (pilihanVolume < 0 && !batal)
                {
                    if (rak == null) yield break;
                    yield return null;
                }
                if (batal) yield break;
                int p = pilihanVolume;
                pilihanVolume = -1;
                TampilkanPanel(false, l);
                if (p != l.jumlahBenar)
                {
                    // Takaran salah: batalkan, kembali ke ScrollBahan untuk ambil lagi
                    if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();
                    yield return Hilang(root, pipetDariSumber ? 0.65f : 0.3f);
                    yield break;
                }

                // 3a. Miring lalu menetes lewat ujung botol
                yield return Putar(root, sudutAwal, l.sudutMiring, 0.5f);
                CairanTabung c = AmbilCairan(slot, rak, l.warnaCairan);
                int jumlah = Mathf.Max(1, l.jumlahBenar * tetesPerMlAnimasi);
                float tambah = (l.jumlahBenar * tinggiPerMl) / jumlah;
                for (int i = 0; i < jumlah; i++)
                {
                    if (rak == null) yield break;
                    LepasTetes(root.position, slot, rak, c, tambah, l.warnaCairan, 1f, false);
                    yield return new WaitForSeconds(0.07f);
                }
                while (tetesAktif > 0) yield return null;
                yield return Putar(root, l.sudutMiring, 0f, 0.4f);
                yield return Hilang(root, 0.3f);
                l.selesai = true;
            }

            else
            {
                // 2b. Tekan Tetes sebanyak yang diinginkan, lalu Selesai
                int hitung = 0;
                CairanTabung c = AmbilCairan(slot, rak, l.warnaCairan);
                float tinggiSnap = c.tinggi;
                Color warnaSnap = c.warna;
                float bobotSnap = c.bobot;
                TampilkanPanel(true, l);
                UpdateTeksTetes(hitung);
                while (true)
                {
                    if (rak == null || batal) yield break;

                    if (permintaanTetes > 0)
                    {
                        permintaanTetes--;
                        if (hitung < maksKlikTetes)
                        {
                            hitung++;
                            UpdateTeksTetes(hitung);
                            LepasTetes(root.position, slot, rak, c, tinggiPerTetes, l.warnaCairan, kekuatanWarnaTetes, l.efekCampur);
                            if (cairanPipet != null)
                            {
                                float sisa = Mathf.Clamp01(1f - (float)hitung / Mathf.Max(1, l.jumlahBenar));
                                cairanPipet.transform.localScale = new Vector3(0.045f, 0.14f * sisa, 0.045f);
                            }
                            StartCoroutine(Denyut(root));
                        }
                    }

                    if (tekanSelesai)
                    {
                        tekanSelesai = false;
                        while (tetesAktif > 0) yield return null;
                        if (hitung == l.jumlahBenar) break;

                        // Takaran tidak sesuai: kartu salah, tetesan ditarik kembali, batalkan sesi (ambil ulang)
                        if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();
                        c.tinggi = tinggiSnap;
                        c.warna = warnaSnap;
                        c.bobot = bobotSnap;
                        c.bobotTunda = 0f;
                        TampilkanWarna(c, c.warna, c.alphaTampil);
                        TerapkanCairan(slot, c);
                        TampilkanPanel(false, l);
                        yield return Hilang(root, 0.3f);
                        yield break;
                    }
                    yield return null;
                }
                TampilkanPanel(false, l);
                if (pipetDariSumber) yield return GerakRig(root, pourLokal + Vector3.down * 0.025f, pourLokal, 0f, 0f, 0.6f);
                yield return Hilang(root, 0.3f);

                // 3b. Efek reaksi (hanya untuk langkah yang menyalakannya)
                if (l.efekCampur) yield return Campur(slot, rak, c, l.warnaCairan);
                if (l.bentukEndapan) yield return Reaksi(slot, rak, c);
                if (l.tuangKeGelas) yield return TuangKeGelas(slot, rak, c, cam);
                l.selesai = true;
            }
        }
        finally
        {
            if (panelTakaran != null) panelTakaran.SetActive(false);
            if (rigGO != null) Destroy(rigGO);
            if (rigAktif == rigGO) rigAktif = null;
            if (l.selesai && l.modelSumber != null)
            {
                GameObject modelSumberSelesai = l.modelSumber;
                bool masihDibutuhkan = false;
                for (int i = 0; i < langkah.Count; i++)
                    if (langkah[i] != null && langkah[i] != l && langkah[i].modelSumber == modelSumberSelesai && !langkah[i].selesai) masihDibutuhkan = true;
                if (!masihDibutuhkan)
                {
                    Destroy(modelSumberSelesai);
                    for (int i = 0; i < langkah.Count; i++) if (langkah[i] != null && langkah[i].modelSumber == modelSumberSelesai) langkah[i].modelSumber = null;
                }
            }
            l.sedangDiproses = false;
            sesiAktif = false;
            if (antreanSesi.Count > 0 && !memprosesAntrean)
                StartCoroutine(ProsesAntrean());
        }
    }

    private IEnumerator GerakRig(Transform rig, Vector3 posisiTujuanLokal, float sudutTujuan, float durasi)
    {
        if (rig == null) yield break;
        Vector3 posisiAwal = rig.localPosition;
        float sudutAwal = rig.localEulerAngles.z;
        float waktu = 0f;
        while (waktu < durasi)
        {
            if (rig == null) yield break;
            waktu += Time.deltaTime;
            float e = EaseInOut(waktu / Mathf.Max(0.001f, durasi));
            rig.localPosition = Vector3.Lerp(posisiAwal, posisiTujuanLokal, e);
            rig.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(sudutAwal, sudutTujuan, e));
            yield return null;
        }
        if (rig != null)
        {
            rig.localPosition = posisiTujuanLokal;
            rig.localRotation = Quaternion.Euler(0f, 0f, sudutTujuan);
        }
    }

    private GameObject BuatCairanPipet(Transform induk, Color warna)
    {
        GameObject cairanPipet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cairanPipet.name = "CairanTerisap_NaCl";
        Collider collider = cairanPipet.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        cairanPipet.transform.SetParent(induk, false);
        cairanPipet.transform.localPosition = Vector3.up * 0.16f;
        cairanPipet.transform.localScale = new Vector3(0.045f, 0.001f, 0.045f);
        Renderer r = cairanPipet.GetComponent<Renderer>();
        if (r != null)
        {
            if (materialTetes != null) r.material = new Material(materialTetes);
            r.material.color = warna;
            if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", warna);
        }
        return cairanPipet;
    }

    private IEnumerator IsiPipet(GameObject cairanPipet, float durasi)
    {
        float waktu = 0f;
        while (waktu < durasi)
        {
            if (cairanPipet == null) yield break;
            waktu += Time.deltaTime;
            float e = EaseInOut(waktu / Mathf.Max(0.001f, durasi));
            cairanPipet.transform.localScale = new Vector3(0.045f, Mathf.Lerp(0.001f, 0.14f, e), 0.045f);
            yield return null;
        }
        if (cairanPipet != null) cairanPipet.transform.localScale = new Vector3(0.045f, 0.14f, 0.045f);
    }

    private IEnumerator ProsesAntrean()
    {
        if (memprosesAntrean) yield break;
        memprosesAntrean = true;
        while (antreanSesi.Count > 0)
        {
            PermintaanSesi berikut = antreanSesi.Dequeue();
            if (berikut.model == null || berikut.langkah == null) continue;
            if (berikut.langkah.panelSoal != null && !berikut.langkah.panelSoal.activeInHierarchy)
            {
                berikut.langkah.sedangDiproses = false;
                Destroy(berikut.model);
                continue;
            }
            yield return StartCoroutine(Sesi(berikut.langkah, berikut.model, berikut.kamera));
        }
        memprosesAntrean = false;
    }
    private IEnumerator Putar(Transform root, float dari, float ke, float durasi)
    {
        float t = 0f;
        while (t < durasi)
        {
            if (root == null) yield break;
            t += Time.deltaTime;
            root.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(dari, ke, EaseInOut(t / durasi)));
            yield return null;
        }
        if (root != null) root.localRotation = Quaternion.Euler(0f, 0f, ke);
    }

    private IEnumerator Hilang(Transform root, float durasi)
    {
        float t = 0f;
        while (t < durasi)
        {
            if (root == null) yield break;
            t += Time.deltaTime;
            root.localScale = Vector3.one * Mathf.Max(0.0001f, 1f - EaseInOut(t / durasi));
            yield return null;
        }
    }

    private IEnumerator Denyut(Transform root)
    {
        float t = 0f;
        const float dur = 0.14f;
        while (t < dur)
        {
            if (root == null) yield break;
            t += Time.deltaTime;
            float s = 1f - 0.06f * Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI);
            root.localScale = Vector3.one * s;
            yield return null;
        }
        if (root != null) root.localScale = Vector3.one;
    }

    // ---------- Cairan dan tetesan ----------

    private static void SetWarna(Material m, Color c)
    {
        if (m == null) return;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    private void TampilkanWarna(CairanTabung c, Color rgb, float alpha)
    {
        if (c == null) return;
        c.alphaTampil = alpha;
        if (c.mat == null) return;
        Color w = rgb;
        w.a = alpha;
        SetWarna(c.mat, w);
    }

    private Color HasilCampur(CairanTabung c)
    {
        float total = c.bobot + c.bobotTunda;
        if (total <= 0.0001f) return c.warna;
        if (c.bobot <= 0.0001f) return c.warnaTunda;
        if (c.bobotTunda <= 0.0001f) return c.warna;
        return (c.warna * c.bobot + c.warnaTunda * c.bobotTunda) / total;
    }

    private GameObject BuatBola(Transform induk, float ukuran, Color warna)
    {
        GameObject d = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider col = d.GetComponent<Collider>();
        if (col != null) Destroy(col);
        Renderer r = d.GetComponent<Renderer>();
        if (materialTetes != null)
        {
            r.sharedMaterial = materialTetes;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            Color dasar = materialTetes.HasProperty("_BaseColor") ? materialTetes.GetColor("_BaseColor") : Color.white;
            Color w = warna;
            w.a = dasar.a;
            mpb.SetColor("_BaseColor", w);
            mpb.SetColor("_Color", w);
            r.SetPropertyBlock(mpb);
        }
        d.transform.SetParent(induk, false);
        d.transform.localScale = Vector3.one * ukuran;
        return d;
    }

    private static void HancurkanSemua(GameObject[] daftar)
    {
        if (daftar == null) return;
        for (int i = 0; i < daftar.Length; i++)
            if (daftar[i] != null) Destroy(daftar[i]);
    }

    private CairanTabung AmbilCairan(SlotTabung slot, Transform rak, Color warna)
    {
        CairanTabung c;
        if (cairan.TryGetValue(slot, out c) && c != null && c.silinder != null) return c;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Cairan_" + slot.namaSlot;
        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        go.transform.SetParent(rak, false);
        go.transform.localRotation = Quaternion.identity;

        c = new CairanTabung();
        c.silinder = go.transform;
        c.warna = warna;
        c.bobot = 0f;
        c.bobotTunda = 0f;
        Renderer r = go.GetComponent<Renderer>();
        if (materialCairan != null)
        {
            c.mat = new Material(materialCairan);
            c.alphaDasar = c.mat.HasProperty("_BaseColor") ? c.mat.GetColor("_BaseColor").a : 0.6f;
            r.sharedMaterial = c.mat;
        }
        c.alphaTampil = c.alphaDasar;
        TampilkanWarna(c, warna, c.alphaDasar);
        cairan[slot] = c;

        // Kaca tabung dibuat transparan supaya cairan terlihat
        if (materialKaca != null && slot.Model != null)
        {
            Renderer rt = slot.Model.GetComponentInChildren<Renderer>();
            if (rt != null) rt.sharedMaterial = materialKaca;
        }

        TerapkanCairan(slot, c);
        return c;
    }

    private void TerapkanCairan(SlotTabung slot, CairanTabung c)
    {
        if (c == null || c.silinder == null) return;
        float h = Mathf.Clamp(c.tinggi, 0f, maksTinggiCairan);
        PerbaruiBayangan(slot, h);
        c.silinder.gameObject.SetActive(h > 0.001f);
        c.silinder.localScale = new Vector3(radiusCairan * 2f, Mathf.Max(h * 0.5f, 0.0001f), radiusCairan * 2f);
        c.silinder.localPosition = new Vector3(slot.lubangLokal.x, slot.lubangLokal.y + jarakDasarKaca + h * 0.5f, slot.lubangLokal.z);
    }

    private void PerbaruiBayangan(SlotTabung slot, float tinggiCairan)
    {
        if (slot == null) return;
        RectTransform bayangan;
        if (!bayanganCairan.TryGetValue(slot, out bayangan) || bayangan == null)
        {
            Transform anak = slot.transform.Find("Bayangan");
            if (anak == null) return;
            bayangan = anak as RectTransform;
            if (bayangan == null) return;
            bayanganCairan[slot] = bayangan;
            ukuranAwalBayangan[slot] = bayangan.sizeDelta;
            bayangan.pivot = new Vector2(bayangan.pivot.x, 0f);
            bayangan.anchoredPosition = new Vector2(bayangan.anchoredPosition.x, bayangan.anchoredPosition.y - bayangan.sizeDelta.y * 0.5f);
        }

        Vector2 ukuran = ukuranAwalBayangan[slot];
        float fraksi = Mathf.Clamp01(tinggiCairan / Mathf.Max(0.001f, maksTinggiCairan));
        bayangan.sizeDelta = new Vector2(ukuran.x, ukuran.y * fraksi);
        bayangan.gameObject.SetActive(fraksi > 0.001f);
    }

    private void TambahkanTetesLangsung(SlotTabung slot, Transform rak, Color warna)
    {
        CairanTabung c = AmbilCairan(slot, rak, warna);
        float bobot = tinggiPerTetes * kekuatanWarnaTetes;
        c.warna = c.bobot > 0.0001f ? (c.warna * c.bobot + warna * bobot) / (c.bobot + bobot) : warna;
        c.bobot += bobot;
        c.tinggi += tinggiPerTetes;
        TampilkanWarna(c, c.warna, c.alphaTampil);
        TerapkanCairan(slot, c);
    }

    private void LepasTetes(Vector3 dariDunia, SlotTabung slot, Transform rak, CairanTabung c, float tambahTinggi, Color warna, float kekuatan, bool tundaWarna)
    {
        StartCoroutine(Jatuh(dariDunia, slot, rak, c, tambahTinggi, warna, kekuatan, tundaWarna));
    }

    private IEnumerator Jatuh(Vector3 dariDunia, SlotTabung slot, Transform rak, CairanTabung c, float tambahTinggi, Color warna, float kekuatan, bool tundaWarna)
    {
        tetesAktif++;
        GameObject d = BuatBola(rak, ukuranTetes, warna);
        Vector3 awal = rak.InverseTransformPoint(dariDunia);
        d.transform.localPosition = awal;

        float permukaan = slot.lubangLokal.y + jarakDasarKaca + Mathf.Clamp(c.tinggi, 0f, maksTinggiCairan);
        Vector3 akhir = new Vector3(slot.lubangLokal.x, permukaan, slot.lubangLokal.z);
        float jarak = Mathf.Max(0.05f, awal.y - akhir.y);
        float dur = Mathf.Max(0.12f, Mathf.Sqrt(2f * jarak / 40f));

        float t = 0f;
        while (t < dur)
        {
            if (rak == null || d == null) break;
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            d.transform.localPosition = new Vector3(Mathf.Lerp(awal.x, akhir.x, k), Mathf.Lerp(awal.y, akhir.y, k * k), Mathf.Lerp(awal.z, akhir.z, k));
            yield return null;
        }

        if (d != null) Destroy(d);
        if (c != null && c.silinder != null)
        {
            float bobot = tambahTinggi * kekuatan;
            if (tundaWarna)
            {
                // Warna baru ditahan dulu, ditampilkan oleh animasi Campur
                c.warnaTunda = c.bobotTunda > 0.0001f ? (c.warnaTunda * c.bobotTunda + warna * bobot) / (c.bobotTunda + bobot) : warna;
                c.bobotTunda += bobot;
            }
            else
            {
                c.warna = c.bobot > 0.0001f ? (c.warna * c.bobot + warna * bobot) / (c.bobot + bobot) : warna;
                c.bobot += bobot;
                TampilkanWarna(c, c.warna, c.alphaTampil);
            }
            c.tinggi += tambahTinggi;
            TerapkanCairan(slot, c);
        }
        tetesAktif = Mathf.Max(0, tetesAktif - 1);
    }

    // ---------- Animasi tercampur ----------

    private IEnumerator Campur(SlotTabung slot, Transform rak, CairanTabung c, Color warnaTetesan)
    {
        if (c == null || c.silinder == null || rak == null) yield break;

        Color akhir = HasilCampur(c);
        Color awal = c.bobot > 0.0001f ? c.warna : akhir;
        const int n = 9;
        const float dur = 1.3f;
        GameObject[] bola = new GameObject[n];
        float[] fase = new float[n];
        float[] tinggiRel = new float[n];
        for (int i = 0; i < n; i++)
        {
            bola[i] = BuatBola(rak, ukuranTetes * 1.1f, warnaTetesan);
            fase[i] = i * (Mathf.PI * 2f / n);
            tinggiRel[i] = (i % 3) / 3f + Random.value * 0.1f;
        }

        float dasar = slot.lubangLokal.y + jarakDasarKaca;
        float t = 0f;
        while (t < dur)
        {
            if (rak == null || c.silinder == null) { HancurkanSemua(bola); yield break; }
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            float h = Mathf.Max(c.tinggi, 0.15f);
            for (int i = 0; i < n; i++)
            {
                if (bola[i] == null) continue;
                float sudut = fase[i] + k * 10f;
                float r = radiusCairan * 0.72f * (1f - 0.55f * k);
                float y = dasar + h * (0.12f + 0.76f * Mathf.Repeat(tinggiRel[i] + k * 0.9f, 1f));
                bola[i].transform.localPosition = new Vector3(slot.lubangLokal.x + Mathf.Cos(sudut) * r, y, slot.lubangLokal.z + Mathf.Sin(sudut) * r);
                bola[i].transform.localScale = Vector3.one * (ukuranTetes * 1.1f * (1f - 0.85f * k));
            }
            TampilkanWarna(c, Color.Lerp(awal, akhir, EaseInOut(k)), c.alphaTampil);
            yield return null;
        }

        HancurkanSemua(bola);
        c.warna = akhir;
        c.bobot += c.bobotTunda;
        c.bobotTunda = 0f;
        TampilkanWarna(c, c.warna, c.alphaTampil);
    }

    // ---------- Reaksi: larutan keruh dan endapan putih PbCl2 ----------

    private void TerapkanEndapan(SlotTabung slot, CairanTabung c)
    {
        if (c == null || c.endapan == null) return;
        float h = Mathf.Max(0f, c.tinggiEndapan);
        c.endapan.gameObject.SetActive(h > 0.001f);
        c.endapan.localScale = new Vector3(radiusCairan * 1.96f, Mathf.Max(h * 0.5f, 0.0001f), radiusCairan * 1.96f);
        c.endapan.localPosition = new Vector3(slot.lubangLokal.x, slot.lubangLokal.y + jarakDasarKaca + h * 0.5f, slot.lubangLokal.z);
    }

    private void BuatEndapan(SlotTabung slot, Transform rak, CairanTabung c)
    {
        if (c.endapan != null) return;
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "Endapan_" + slot.namaSlot;
        Collider col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
        go.transform.SetParent(rak, false);
        go.transform.localRotation = Quaternion.identity;
        c.endapan = go.transform;
        Renderer r = go.GetComponent<Renderer>();
        if (materialCairan != null)
        {
            c.matEndapan = new Material(materialCairan);
            Color w = warnaEndapan;
            w.a = 0.97f;
            SetWarna(c.matEndapan, w);
            r.sharedMaterial = c.matEndapan;
        }
        c.tinggiEndapan = 0f;
        TerapkanEndapan(slot, c);
    }

    private IEnumerator Reaksi(SlotTabung slot, Transform rak, CairanTabung c)
    {
        if (c == null || c.silinder == null || rak == null) yield break;

        // 1. Larutan menjadi keruh
        Color dari = c.warna;
        float aDari = c.alphaTampil;
        float aKeruh = Mathf.Min(0.9f, c.alphaDasar + 0.3f);
        float t = 0f;
        const float durKeruh = 0.9f;
        while (t < durKeruh)
        {
            if (rak == null || c.silinder == null) yield break;
            t += Time.deltaTime;
            float e = EaseInOut(t / durKeruh);
            TampilkanWarna(c, Color.Lerp(dari, warnaLarutanKeruh, e), Mathf.Lerp(aDari, aKeruh, e));
            yield return null;
        }
        c.warna = warnaLarutanKeruh;
        TampilkanWarna(c, c.warna, aKeruh);

        // 2. Endapan putih terbentuk: butir-butir jatuh lalu menumpuk di dasar tabung
        BuatEndapan(slot, rak, c);
        int n = Mathf.Max(0, jumlahButirEndapan);
        GameObject[] butir = new GameObject[n];
        float[] sudut = new float[n];
        float[] radius = new float[n];
        float[] tinggiAwal = new float[n];
        float[] tunda = new float[n];
        float[] lama = new float[n];
        float dasar = slot.lubangLokal.y + jarakDasarKaca;
        float tinggiCair = Mathf.Max(0.2f, c.tinggi);
        for (int i = 0; i < n; i++)
        {
            butir[i] = BuatBola(rak, ukuranTetes * 0.45f, warnaEndapan);
            butir[i].SetActive(false);
            sudut[i] = Random.value * Mathf.PI * 2f;
            radius[i] = radiusCairan * 0.85f * Mathf.Sqrt(Random.value);
            tinggiAwal[i] = 0.35f + Random.value * 0.65f;
            tunda[i] = Random.value * 0.6f;
            lama[i] = 0.9f + Random.value * 0.7f;
        }

        const float durEndap = 2f;
        float aAkhir = Mathf.Max(c.alphaDasar, aKeruh - 0.3f);
        t = 0f;
        while (t < durEndap)
        {
            if (rak == null || c.silinder == null) { HancurkanSemua(butir); yield break; }
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / durEndap);
            c.tinggiEndapan = tinggiEndapanTabung * EaseInOut(k);
            TerapkanEndapan(slot, c);
            TampilkanWarna(c, c.warna, Mathf.Lerp(aKeruh, aAkhir, EaseInOut(k)));
            float puncak = dasar + c.tinggiEndapan;
            for (int i = 0; i < n; i++)
            {
                if (butir[i] == null) continue;
                float lokal = t - tunda[i];
                if (lokal < 0f || lokal > lama[i]) { butir[i].SetActive(false); continue; }
                butir[i].SetActive(true);
                float f = lokal / lama[i];
                float y = Mathf.Lerp(dasar + tinggiCair * tinggiAwal[i], puncak, f * f);
                butir[i].transform.localPosition = new Vector3(slot.lubangLokal.x + Mathf.Cos(sudut[i]) * radius[i], y, slot.lubangLokal.z + Mathf.Sin(sudut[i]) * radius[i]);
            }
            yield return null;
        }
        HancurkanSemua(butir);
        c.tinggiEndapan = tinggiEndapanTabung;
        TerapkanEndapan(slot, c);
        TampilkanWarna(c, c.warna, aAkhir);
        yield return new WaitForSeconds(0.8f);
    }

    // ---------- Tuang ke gelas kimia ----------

    private static float ArahZKamera(Transform rak, Camera cam)
    {
        if (cam == null) return -1f;
        Vector3 k = rak.InverseTransformDirection(-cam.transform.forward);
        return k.z >= 0f ? 1f : -1f;
    }

    private static Transform BuatPivotDasar(Transform t, Transform induk)
    {
        Renderer r = t.GetComponent<Renderer>();
        Vector3 dasar = r != null ? new Vector3(r.bounds.center.x, r.bounds.min.y, r.bounds.center.z) : t.position;
        GameObject p = new GameObject("Pivot_" + t.name);
        p.transform.SetParent(induk, false);
        p.transform.position = dasar;
        p.transform.localRotation = Quaternion.identity;
        p.transform.localScale = Vector3.one;
        t.SetParent(p.transform, true);
        return p.transform;
    }

    private static void BeriWarna(Renderer r, Color warna, float alpha)
    {
        if (r == null) return;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        Color w = warna;
        w.a = alpha;
        mpb.SetColor("_BaseColor", w);
        mpb.SetColor("_Color", w);
        r.SetPropertyBlock(mpb);
    }

    private GelasKimia BuatGelas(Transform rak, Vector3 posLokal)
    {
        GameObject go = Instantiate(prefabGelasKimia);
        go.name = "GelasKimia_PbCl2";
        go.SetActive(true);
        Transform root = go.transform;
        root.SetParent(rak, false);
        root.localPosition = posLokal;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;

        // Buang lampu, kamera, dan penanda bawaan file 3D (hasil ekspor Blender)
        foreach (Light lt in go.GetComponentsInChildren<Light>(true)) Destroy(lt);
        foreach (Camera km in go.GetComponentsInChildren<Camera>(true)) Destroy(km);
        foreach (AudioListener al in go.GetComponentsInChildren<AudioListener>(true)) Destroy(al);

        GelasKimia g = new GelasKimia();
        g.root = root;
        Transform tCairan = null;
        Transform tEndapan = null;
        List<Transform> buang = new List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            Transform ch = root.GetChild(i);
            string nama = ch.name;
            if (nama == "PbCl2_Liquid") tCairan = ch;
            else if (nama == "PbCl2_Sediment") tEndapan = ch;
            else if (nama.StartsWith("Icosphere")) { g.butir.Add(ch.gameObject); ch.gameObject.SetActive(false); }
            else if (nama.StartsWith("Area") || nama == "Camera" || nama == "Empty") buang.Add(ch);
        }
        for (int i = 0; i < buang.Count; i++) Destroy(buang[i].gameObject);

        if (tCairan == null)
        {
            Debug.LogWarning("ManajerTakaran: model gelas kimia tidak punya anak PbCl2_Liquid.");
            Destroy(go);
            return null;
        }

        g.pivotCairan = BuatPivotDasar(tCairan, root);
        g.pivotCairan.localScale = new Vector3(1f, 0.0001f, 1f);
        BeriWarna(tCairan.GetComponent<Renderer>(), warnaLarutanGelas, 0.9f);

        if (tEndapan != null)
        {
            g.pivotEndapan = BuatPivotDasar(tEndapan, root);
            g.pivotEndapan.localScale = Vector3.one * 0.0001f;
            BeriWarna(tEndapan.GetComponent<Renderer>(), warnaLarutanGelas * 0.92f, 1f);
        }

        root.localScale = Vector3.one * 0.0001f;
        return g;
    }

    private IEnumerator PopIn(Transform t, float durasi)
    {
        float w = 0f;
        while (w < durasi)
        {
            if (t == null) yield break;
            w += Time.deltaTime;
            t.localScale = Vector3.one * Mathf.Max(0.0001f, EaseInOut(w / durasi));
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }

    private IEnumerator GerakRig(Transform rig, Vector3 dari, Vector3 ke, float sudutDari, float sudutKe, float durasi)
    {
        float t = 0f;
        while (t < durasi)
        {
            if (rig == null) yield break;
            t += Time.deltaTime;
            float e = EaseInOut(t / durasi);
            rig.localPosition = Vector3.Lerp(dari, ke, e);
            rig.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(sudutDari, sudutKe, e));
            yield return null;
        }
        if (rig != null)
        {
            rig.localPosition = ke;
            rig.localRotation = Quaternion.Euler(0f, 0f, sudutKe);
        }
    }

    private static void KecilkanSilinder(Transform sil, Vector3 skalaAwal, Vector3 posAwal, float faktor)
    {
        if (sil == null) return;
        float h0 = skalaAwal.y;
        float h = Mathf.Max(0.0001f, h0 * Mathf.Clamp01(faktor));
        sil.localScale = new Vector3(skalaAwal.x, h, skalaAwal.z);
        sil.localPosition = new Vector3(posAwal.x, posAwal.y - h0 + h, posAwal.z);
    }

    private IEnumerator Aliran(Vector3 dariDunia, Transform rak, Vector3 tujuanLokal, Color warna)
    {
        if (rak == null) yield break;
        GameObject d = BuatBola(rak, ukuranTetes * 0.8f, warna);
        Vector3 awal = rak.InverseTransformPoint(dariDunia);
        d.transform.localPosition = awal;
        float jarak = Mathf.Max(0.05f, awal.y - tujuanLokal.y);
        float dur = Mathf.Max(0.12f, Mathf.Sqrt(2f * jarak / 40f));
        float t = 0f;
        while (t < dur)
        {
            if (rak == null || d == null) break;
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dur);
            d.transform.localPosition = new Vector3(Mathf.Lerp(awal.x, tujuanLokal.x, k), Mathf.Lerp(awal.y, tujuanLokal.y, k * k), Mathf.Lerp(awal.z, tujuanLokal.z, k));
            yield return null;
        }
        if (d != null) Destroy(d);
    }

    private IEnumerator TuangKeGelas(SlotTabung slot, Transform rak, CairanTabung c, Camera cam)
    {
        if (prefabGelasKimia == null)
        {
            Debug.LogWarning("ManajerTakaran: prefabGelasKimia belum diisi, animasi tuang ke gelas kimia dilewati.");
            yield break;
        }
        if (rak == null || slot == null || slot.Model == null || c == null || c.silinder == null) yield break;

        // Gelas ditaruh di samping rak (sisi tempat tabung berada) dan sedikit ke depan rak
        float sisi = slot.lubangLokal.x <= 0f ? -1f : 1f;
        float zs = ArahZKamera(rak, cam);
        Vector3 posGelas = new Vector3(sisi * (setengahLebarRak + jarakGelasDariRak), 0f, zs * jarakDepanRak);

        GelasKimia g = BuatGelas(rak, posGelas);
        if (g == null) yield break;
        gelasAktif = g.root.gameObject;
        yield return PopIn(g.root, 0.4f);

        // Rig di mulut tabung: tabung (beserta cairannya) diputar mengelilingi mulutnya
        Transform tabung = slot.Model.transform;
        Vector3 mulutAwal = slot.lubangLokal + Vector3.up * panjangTabung;
        GameObject rigGO = new GameObject("RigTuang_" + slot.namaSlot);
        rigTuang = rigGO;
        Transform rig = rigGO.transform;
        rig.SetParent(rak, false);
        rig.localPosition = mulutAwal;
        rig.localRotation = Quaternion.identity;
        rig.localScale = Vector3.one;
        tabung.SetParent(rig, true);
        c.silinder.SetParent(rig, true);
        if (c.endapan != null) c.endapan.SetParent(rig, true);

        Vector3 skalaCair = c.silinder.localScale;
        Vector3 posCair = c.silinder.localPosition;
        bool adaEndapan = c.endapan != null;
        Vector3 skalaEnd = adaEndapan ? c.endapan.localScale : Vector3.one;
        Vector3 posEnd = adaEndapan ? c.endapan.localPosition : Vector3.zero;

        float angkat = Mathf.Max(slot.tinggiAngkat + 0.3f, tinggiRak + 0.4f - slot.lubangLokal.y);
        Vector3 p1 = mulutAwal + Vector3.up * angkat;
        Vector3 p2 = new Vector3(p1.x, p1.y, posGelas.z);
        Vector3 pTuang = new Vector3(posGelas.x - sisi * 0.45f, tinggiGelas + mulutDiAtasGelas + 1.5f, posGelas.z);
        float sudut = -sisi * sudutTuangGelas;

        // Angkat keluar dari lubang rak, geser ke depan rak, lalu miring di atas gelas
        yield return GerakRig(rig, mulutAwal, p1, 0f, 0f, 0.5f);
        yield return GerakRig(rig, p1, p2, 0f, 0f, 0.3f);
        yield return GerakRig(rig, p2, pTuang, 0f, sudut, 0.9f);

        // Tuang: cairan di tabung menyusut, cairan di gelas naik, aliran jatuh dari mulut tabung
        float waktu = 0f;
        float jeda = 0f;
        int butirAktif = 0;
        while (waktu < lamaTuang)
        {
            if (rak == null || rig == null || g.root == null) yield break;
            waktu += Time.deltaTime;
            float u = Mathf.Clamp01(waktu / lamaTuang);
            KecilkanSilinder(c.silinder, skalaCair, posCair, 1f - u);
            if (adaEndapan) KecilkanSilinder(c.endapan, skalaEnd, posEnd, 1f - u);
            g.pivotCairan.localScale = new Vector3(1f, Mathf.Max(0.0001f, u), 1f);

            int target = Mathf.FloorToInt(u * g.butir.Count);
            while (butirAktif < target)
            {
                g.butir[butirAktif].SetActive(true);
                butirAktif++;
            }

            jeda -= Time.deltaTime;
            while (jeda <= 0f)
            {
                jeda += 0.045f;
                Vector3 tujuan = new Vector3(posGelas.x - sisi * 0.25f, posGelas.y + dasarCairanGelas + tinggiCairanGelas * u, posGelas.z);
                StartCoroutine(Aliran(rig.position, rak, tujuan, warnaLarutanGelas));
            }
            yield return null;
        }

        if (c.silinder != null) c.silinder.gameObject.SetActive(false);
        if (adaEndapan && c.endapan != null) c.endapan.gameObject.SetActive(false);
        g.pivotCairan.localScale = Vector3.one;
        for (int i = butirAktif; i < g.butir.Count; i++) g.butir[i].SetActive(true);
        if (g.pivotEndapan != null) yield return PopIn(g.pivotEndapan, 0.5f);
        yield return new WaitForSeconds(0.5f);

        // Tegakkan lagi dan kembalikan tabung ke lubang rak
        yield return GerakRig(rig, pTuang, p2, sudut, 0f, 0.8f);
        yield return GerakRig(rig, p2, p1, 0f, 0f, 0.3f);
        yield return GerakRig(rig, p1, mulutAwal, 0f, 0f, 0.5f);

        if (tabung != null) tabung.SetParent(rak, true);
        if (rigGO != null) Destroy(rigGO);
        rigTuang = null;
        if (c.silinder != null) Destroy(c.silinder.gameObject);
        if (c.endapan != null) Destroy(c.endapan.gameObject);
        cairan.Remove(slot);
    }

    /// <summary>Dipanggil saat panel Kesimpulan tampil (tombol Data diklik). Menyembunyikan gelas reaksi.</summary>
    public void SembunyikanGelasReaksi()
    {
        StartCoroutine(TutupGelasReaksi());
    }

    private IEnumerator TutupGelasReaksi()
    {
        if (gelasAktif != null)
        {
            Transform g = gelasAktif.transform;
            yield return Hilang(g, 0.35f);
            if (gelasAktif != null) Destroy(gelasAktif);
        }
        gelasAktif = null;
        if (rakTersembunyi != null)
        {
            Renderer[] rendRak = rakTersembunyi.GetComponentsInChildren<Renderer>(true);
            for (int iR = 0; iR < rendRak.Length; iR++) rendRak[iR].enabled = true;
            rakTersembunyi = null;
        }
    }

    // ---------- Soal6: tuang hasil tabung A dan B bersamaan saat soal ini mulai ----------

    /// <summary>Dipanggil PemicuTuangGelasSoal6 saat panel Soal6 aktif. Menuang isi tabung A lalu B ke satu gelas kimia yang sama.</summary>
    public void MulaiTuangDuaTabung(SlotTabung a, SlotTabung b, Camera cam)
    {
        StartCoroutine(TuangDuaTabungKeGelas(a, b, cam != null ? cam : Camera.main));
    }

    private class RigTabung
    {
        public Transform rig;
        public Vector3 mulutAwal, p1, p2, pTuang;
        public float sudut, sisi;
        public SlotTabung slot;
        public CairanTabung c;
        public Vector3 skalaCair, posCair, skalaEnd, posEnd;
        public bool adaEndapan;
    }

    private RigTabung AngkatTabung(SlotTabung slot, Transform rak, Vector3 posGelas)
    {
        CairanTabung c;
        if (slot == null || slot.Model == null || !cairan.TryGetValue(slot, out c) || c == null || c.silinder == null) return null;

        Transform tabung = slot.Model.transform;
        Vector3 mulutAwal = slot.lubangLokal + Vector3.up * panjangTabung;
        GameObject rigGO = new GameObject("RigTuang_" + slot.namaSlot);
        rigTuang = rigGO;
        Transform rig = rigGO.transform;
        rig.SetParent(rak, false);
        rig.localPosition = mulutAwal;
        rig.localRotation = Quaternion.identity;
        rig.localScale = Vector3.one;
        tabung.SetParent(rig, true);
        c.silinder.SetParent(rig, true);
        if (c.endapan != null) c.endapan.SetParent(rig, true);

        RigTabung rt = new RigTabung();
        rt.rig = rig;
        rt.slot = slot;
        rt.c = c;
        rt.skalaCair = c.silinder.localScale;
        rt.posCair = c.silinder.localPosition;
        rt.adaEndapan = c.endapan != null;
        rt.skalaEnd = rt.adaEndapan ? c.endapan.localScale : Vector3.one;
        rt.posEnd = rt.adaEndapan ? c.endapan.localPosition : Vector3.zero;

        rt.sisi = slot.lubangLokal.x <= 0f ? -1f : 1f;
        float angkat = Mathf.Max(slot.tinggiAngkat + 0.3f, tinggiRak + 0.4f - slot.lubangLokal.y);
        rt.mulutAwal = mulutAwal;
        rt.p1 = mulutAwal + Vector3.up * angkat;
        rt.p2 = new Vector3(rt.p1.x, rt.p1.y, posGelas.z);
        rt.pTuang = new Vector3(posGelas.x - rt.sisi * 0.45f, tinggiGelas + mulutDiAtasGelas + 1.5f, posGelas.z);
        rt.sudut = -rt.sisi * sudutTuangGelas;
        return rt;
    }

    private IEnumerator TuangDuaTabungKeGelas(SlotTabung slotA, SlotTabung slotB, Camera cam)
    {
        if (prefabGelasKimia == null)
        {
            Debug.LogWarning("ManajerTakaran: prefabGelasKimia belum diisi, animasi tuang Soal6 dilewati.");
            yield break;
        }

        Transform rak = (slotA != null && slotA.Model != null) ? slotA.Model.transform.parent
                       : (slotB != null && slotB.Model != null) ? slotB.Model.transform.parent : null;
        if (rak == null) yield break;

        bool adaA = slotA != null && cairan.ContainsKey(slotA);
        bool adaB = slotB != null && cairan.ContainsKey(slotB);
        if (!adaA && !adaB) yield break;

        float zs = ArahZKamera(rak, cam);
        Vector3 posGelas = new Vector3(0f, 0f, zs * jarakDepanRak * 0.6f); // di tengah, menggantikan posisi rak

        // 1. Cabut/angkat kedua tabung dulu (rak masih terlihat)
        RigTabung rigA = adaA ? AngkatTabung(slotA, rak, posGelas) : null;
        RigTabung rigB = adaB ? AngkatTabung(slotB, rak, posGelas) : null;
        if (rigA != null) yield return GerakRig(rigA.rig, rigA.mulutAwal, rigA.p1, 0f, 0f, 0.4f);
        if (rigB != null) yield return GerakRig(rigB.rig, rigB.mulutAwal, rigB.p1, 0f, 0f, 0.4f);

        // 2. Sekarang rak (RakTabungReaksi) disembunyikan -- tabung yang sudah terangkat dikecualikan
        rakTersembunyi = rak;
        Renderer[] rendRak = rak.GetComponentsInChildren<Renderer>(true);
        for (int iR = 0; iR < rendRak.Length; iR++)
        {
            Transform rt2 = rendRak[iR].transform;
            if ((rigA != null && rt2.IsChildOf(rigA.rig)) || (rigB != null && rt2.IsChildOf(rigB.rig))) continue;
            rendRak[iR].enabled = false;
        }

        // 3. Gelas kimia kosong muncul di tengah
        GelasKimia g = BuatGelas(rak, posGelas);
        if (g == null) yield break;
        gelasAktif = g.root.gameObject;
        yield return PopIn(g.root, 0.4f);
        yield return BesarkanGelas(g.root, 1f, 1.6f, 0.4f);

        // 4. Lanjutkan tuang untuk tabung yang sudah terangkat
        if (rigA != null) yield return LanjutkanTuang(rigA, rak, g, posGelas, 0f, 0.5f);
        if (rigB != null) yield return LanjutkanTuang(rigB, rak, g, posGelas, 0.5f, 1f);

        if (g.pivotEndapan != null) yield return PopIn(g.pivotEndapan, 0.5f);
        // Pertahankan gelas hasil sampai tombol Data diklik.
        // Panel kesimpulan memanggil SembunyikanGelasReaksi untuk menutup hasil dan memulihkan rak.
    }

    private IEnumerator BesarkanGelas(Transform root, float dari, float ke, float durasi)
    {
        float t2 = 0f;
        while (t2 < durasi)
        {
            if (root == null) yield break;
            t2 += Time.deltaTime;
            root.localScale = Vector3.one * Mathf.Lerp(dari, ke, EaseInOut(t2 / durasi));
            yield return null;
        }
        if (root != null) root.localScale = Vector3.one * ke;
    }

    private IEnumerator LanjutkanTuang(RigTabung rt, Transform rak, GelasKimia g, Vector3 posGelas, float uDari, float uKe)
    {
        Transform rig = rt.rig;
        CairanTabung c = rt.c;
        float sisi = rt.sisi;

        yield return GerakRig(rig, rt.p1, rt.p2, 0f, 0f, 0.25f);
        yield return GerakRig(rig, rt.p2, rt.pTuang, 0f, rt.sudut, 0.7f);

        float waktu = 0f;
        float durasi = Mathf.Max(0.4f, lamaTuang * 0.6f);
        float jeda = 0f;
        int butirDari = Mathf.FloorToInt(uDari * g.butir.Count);
        int butirKe = Mathf.FloorToInt(uKe * g.butir.Count);
        int butirAktif = butirDari;
        while (waktu < durasi)
        {
            if (rak == null || rig == null || g.root == null) yield break;
            waktu += Time.deltaTime;
            float k = Mathf.Clamp01(waktu / durasi);
            KecilkanSilinder(c.silinder, rt.skalaCair, rt.posCair, 1f - k);
            if (rt.adaEndapan) KecilkanSilinder(c.endapan, rt.skalaEnd, rt.posEnd, 1f - k);
            float u = Mathf.Lerp(uDari, uKe, k);
            g.pivotCairan.localScale = new Vector3(1f, Mathf.Max(0.0001f, u), 1f);

            int target = Mathf.FloorToInt(Mathf.Lerp(butirDari, butirKe, k));
            while (butirAktif < target)
            {
                if (butirAktif < g.butir.Count) g.butir[butirAktif].SetActive(true);
                butirAktif++;
            }

            jeda -= Time.deltaTime;
            while (jeda <= 0f)
            {
                jeda += 0.05f;
                Vector3 tujuan = new Vector3(posGelas.x - sisi * 0.25f, posGelas.y + dasarCairanGelas + tinggiCairanGelas * u, posGelas.z);
                StartCoroutine(Aliran(rig.position, rak, tujuan, warnaLarutanGelas));
            }
            yield return null;
        }
        if (c.silinder != null) c.silinder.gameObject.SetActive(false);
        if (rt.adaEndapan && c.endapan != null) c.endapan.gameObject.SetActive(false);
        g.pivotCairan.localScale = new Vector3(1f, uKe, 1f);
        for (int i = butirAktif; i < butirKe && i < g.butir.Count; i++) g.butir[i].SetActive(true);

        // Tabung dicabut: tidak dikembalikan ke rak, cukup diangkat sedikit lalu memudar dan dihapus
        Vector3 pCabut = rt.pTuang + new Vector3(0f, 0.6f, 0f);
        yield return GerakRig(rig, rt.pTuang, pCabut, rt.sudut, rt.sudut * 0.5f, 0.4f);
        yield return Hilang(rig, 0.4f);

        if (rig != null) Destroy(rig.gameObject);
        rigTuang = null;
        if (c.silinder != null) Destroy(c.silinder.gameObject);
        if (c.endapan != null) Destroy(c.endapan.gameObject);
        cairan.Remove(rt.slot);
    }
}
