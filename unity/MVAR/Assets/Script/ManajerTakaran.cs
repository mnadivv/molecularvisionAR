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



    private bool animasiSoal6Selesai = true;

    public bool AnimasiSoal6Selesai { get { return animasiSoal6Selesai; } }



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

        [Tooltip("Area layar untuk menjatuhkan kartu bahan. Jika kosong, memakai area tabung target.")]

        public RectTransform areaJatuh;

        public Jenis jenis = Jenis.Tuang;

        [Tooltip("Tuang: jumlah ml yang benar. Tetes: jumlah tetes yang benar.")]

        public int jumlahBenar = 1;

        [Tooltip("Warna bahan ini. Warna di tabung = campuran semua bahan menurut volumenya.")]

        public Color warnaCairan = new Color(0.85f, 0.94f, 1f, 1f);

        [Tooltip("Titik ujung tempat cairan keluar, di ruang model asli (sebelum diputar).")]

        public Vector3 titikKeluar;

        [Tooltip("Putaran sumbu Z (derajat) supaya ujung menghadap ke bawah.")]

        public float sudutMiring = 90f;

        [Tooltip("Tinggi ujung di atas mulut tabung (satuan model rak). Naikkan supaya botol tidak tertutup rak.")]

        public float tinggiDiAtasTabung = 0.015f;

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

    public void ResetSemua(bool pertahankanObjekLuar = false)

    {

        StopAllCoroutines();
        pilihanVolume = -1;
        batal = true;
        permintaanTetes = 0;
        tekanSelesai = false;
        if (grupVolume != null) grupVolume.SetActive(false);
        if (grupTetes != null) grupTetes.SetActive(false);

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

                if (langkah[i].panelSoal != null && langkah[i].panelSoal.name == "Soal2" && langkah[i].areaJatuh != null) langkah[i].areaJatuh.gameObject.SetActive(true);

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

        GameObject wadah = pertahankanObjekLuar ? null : GameObject.Find("Model3D_Spawn");

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
            ModelTertempelLayar pengunciLayar = model.GetComponent<ModelTertempelLayar>();
            if (pengunciLayar == null) pengunciLayar = model.AddComponent<ModelTertempelLayar>();
            pengunciLayar.Ikat(targetSumber, cam);

            // Soal yang memakai sumber kimia sama berbagi satu model labu yang diletakkan.

            for (int i = 0; i < langkah.Count; i++)

            {

                LangkahTakaran terkait = langkah[i];

                if (terkait != null && terkait != langkahSumber && terkait.kartuSumber == kartu && terkait.panelSoal == langkahSumber.panelSoal && !terkait.selesai)

                    terkait.modelSumber = model;

            }

            return true;

        }



        // Pada Soal5 pilih langkah berdasarkan tabung yang benar-benar disentuh; A dan B diproses terpisah.

        LangkahTakaran l = CariLangkahSoal5PadaTabung(kartu, posisiLayar, cam);

        if (l == null) l = CariLangkah(kartu);

        if (l == null) return false;

        if (l.kartuSumber != null && l.modelSumber == null)

        {

            if (ManajerPilihItem.Instance != null) ManajerPilihItem.Instance.TampilkanSalah();

            return false;

        }

        SlotTabung target = l.tabungTarget;

        if (target == null || !target.SelesaiMasuk || target.Model == null) return false;



        bool areaDropBenar = l.areaJatuh != null

            ? RectTransformUtility.RectangleContainsScreenPoint(l.areaJatuh, posisiLayar, KameraUI(l.areaJatuh, cam))

            : target.Mengandung(posisiLayar, cam);

        if (!areaDropBenar)

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



        // Sembunyikan penanda sesudah botol akuades Soal2 berhasil diletakkan.

        if (l.panelSoal != null && l.panelSoal.name == "Soal2" && l.areaJatuh != null) l.areaJatuh.gameObject.SetActive(false);



        // Terima drop kedua yang benar ketika animasi tetesan pertama masih berlangsung.

        l.sedangDiproses = true;

        PermintaanSesi permintaan = new PermintaanSesi { langkah = l, model = model, kamera = cam };

        if (sesiAktif || memprosesAntrean)

        {

            antreanSesi.Enqueue(permintaan);

            // Biarkan pipet kedua tetap terlihat saat menunggu panel tetes pertama selesai.

            return true;

        }

        StartCoroutine(Sesi(l, model, cam));

        return true;

    }



    private LangkahTakaran CariLangkahSoal5PadaTabung(GameObject kartu, Vector2 posisiLayar, Camera cam)

    {

        for (int i = 0; i < langkah.Count; i++)

        {

            LangkahTakaran l = langkah[i];

            if (l == null || l.selesai || l.sedangDiproses || l.kartu != kartu) continue;

            if (l.panelSoal == null || l.panelSoal.name != "Soal5" || !l.panelSoal.activeInHierarchy) continue;

            if (l.tabungTarget != null && l.tabungTarget.SelesaiMasuk && l.tabungTarget.Model != null && l.tabungTarget.Mengandung(posisiLayar, cam)) return l;

        }

        return null;

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

        if (!tampil)
        {
            if (grupVolume != null) grupVolume.SetActive(false);
            if (grupTetes != null) grupTetes.SetActive(false);
            return;
        }

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

        bool pertahankanAkuadesSoal2 = l.jenis == Jenis.Tuang && l.panelSoal != null && l.panelSoal.name == "Soal2";

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

                    Vector3 diAtasLabu = rak.InverseTransformPoint(mulutSumber - Vector3.up * Mathf.Min(0.08f, boundsSumber.size.y * 0.08f) + Vector3.up * 0.015f);

                    // Masukkan ujung pipet sedikit ke dalam bukaan leher labu.

                    Vector3 diMulutLabu = rak.InverseTransformPoint(mulutSumber + Vector3.down * 0.015f);



                    yield return GerakRig(root, diAtasLabu, 0f, 0.52f);

                    yield return GerakRig(root, diMulutLabu, 0f, 0.38f);

                    cairanPipet = BuatCairanPipet(root, l.warnaCairan, model.transform);

                    yield return IsiPipet(cairanPipet, 0.52f);

                    yield return GerakRig(root, diAtasLabu, 0f, 0.38f);

                }

            }



            // 1. Terbang ke atas mulut tabung (botol/pipet dinaikkan sebesar tinggiDiAtasTabung)

            bool pipetDariSumber = l.jenis == Jenis.Tetes && l.modelSumber != null;

            bool tuangGelasSoal2 = pertahankanAkuadesSoal2;

            float sudutAwal = l.jenis == Jenis.Tetes || pipetDariSumber || tuangGelasSoal2 ? 0f : l.sudutMiring;

            Vector3 pourLokal = tuangGelasSoal2 ? rak.InverseTransformPoint(root.position) : new Vector3(slot.lubangLokal.x, slot.lubangLokal.y + panjangTabung + Mathf.Min(l.tinggiDiAtasTabung, l.jenis == Jenis.Tetes ? 0.015f : 0.08f), slot.lubangLokal.z);

            if (tuangGelasSoal2)

            {

                // Setelah diletakkan di tanda, ujung sedotan bergerak halus ke atas mulut gelas.

                yield return PosisikanBotolDiSampingGelas(root, slot, cam, 0.42f);

                root.localRotation = Quaternion.identity;

            }

            else

            {

                float durTerbang = pipetDariSumber ? 0.62f : 0.42f;
                if (l.jenis == Jenis.Tetes)
                {
                    // Pipet selalu mendekati lubang dari atas: naik, geser mendatar,
                    // lalu turun lurus. Arah pipet tidak diputar selama lintasan.
                    Vector3 awalLokal = root.localPosition;
                    float tinggiLintasan = Mathf.Max(awalLokal.y, pourLokal.y + 0.12f);
                    Vector3 diAtasAwal = new Vector3(awalLokal.x, tinggiLintasan, awalLokal.z);
                    Vector3 diAtasLubang = new Vector3(pourLokal.x, tinggiLintasan, pourLokal.z);
                    float waktuPipet = 0f;
                    while (waktuPipet < durTerbang)
                    {
                        if (rak == null) yield break;
                        waktuPipet += Time.deltaTime;
                        float k = Mathf.Clamp01(waktuPipet / durTerbang);
                        if (k < 0.18f)
                            root.localPosition = Vector3.Lerp(awalLokal, diAtasAwal, EaseInOut(k / 0.18f));
                        else if (k < 0.78f)
                            root.localPosition = Vector3.Lerp(diAtasAwal, diAtasLubang, EaseInOut((k - 0.18f) / 0.60f));
                        else
                            root.localPosition = Vector3.Lerp(diAtasLubang, pourLokal, EaseInOut((k - 0.78f) / 0.22f));
                        root.localRotation = Quaternion.identity;
                        root.localScale = Vector3.one * Mathf.Lerp(lambdaMulai, 1f, EaseInOut(k));
                        yield return null;
                    }
                    root.localPosition = pourLokal;
                    root.localRotation = Quaternion.identity;
                    root.localScale = Vector3.one;
                    float masukLubang = Mathf.Clamp(panjangTabung * 0.025f, 0.04f, 0.08f);
                    yield return GerakRig(root, pourLokal, pourLokal + Vector3.down * masukLubang, 0f, 0f, 0.38f);
                }
                else
                {
                    Vector3 awalTerbang = root.position;
                    float t = 0f;
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
                }

            }



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

                    if (pertahankanAkuadesSoal2 && l.areaJatuh != null) l.areaJatuh.gameObject.SetActive(true);

                    yield return Hilang(root, pipetDariSumber ? 0.65f : 0.3f);

                    yield break;

                }



                // 3a. Miring lalu menetes lewat ujung botol

                yield return Putar(root, sudutAwal, l.sudutMiring, 0.5f);

                if (tuangGelasSoal2)

                {

                    yield return TuangAkuadesKeGelas(slot, rak, root, l.jumlahBenar, l.warnaCairan);

                }

                else

                {

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

                }

                yield return Putar(root, l.sudutMiring, 0f, 0.4f);

                if (!pertahankanAkuadesSoal2) yield return Hilang(root, 0.3f);

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

                if (l.jenis == Jenis.Tetes) yield return GerakRig(root, root.localPosition, pourLokal, 0f, 0f, 0.6f);

                yield return Hilang(root, 0.3f);



                // Lepaskan panel sesi segera setelah pipet selesai. Efek reaksi Soal 5 berjalan paralel

                // agar pengambilan pipet kedua tidak menunggu animasi campuran/endapan tabung pertama.

                if (l.panelSoal != null && l.panelSoal.name == "Soal5")

                {

                    l.selesai = true;

                    StartCoroutine(EfekLanjutanSoal5(l, slot, rak, c, cam));

                }

                else

                {

                    if (l.efekCampur) yield return Campur(slot, rak, c, l.warnaCairan);

                    if (l.bentukEndapan) yield return Reaksi(slot, rak, c);

                    if (l.tuangKeGelas) yield return TuangKeGelas(slot, rak, c, cam);

                    l.selesai = true;

                }

            }

        }

        finally

        {

            if (panelTakaran != null) panelTakaran.SetActive(false);

            if (pertahankanAkuadesSoal2 && l.selesai && model != null) model.transform.SetParent(rak, true);

            if (pertahankanAkuadesSoal2 && !l.selesai && l.areaJatuh != null) l.areaJatuh.gameObject.SetActive(true);

            if (rigGO != null) Destroy(rigGO);

            if (rigAktif == rigGO) rigAktif = null;

            if (!pertahankanAkuadesSoal2 && l.selesai && l.modelSumber != null)

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



    private IEnumerator EfekLanjutanSoal5(LangkahTakaran l, SlotTabung slot, Transform rak, CairanTabung c, Camera cam)

    {

        if (l.efekCampur) yield return StartCoroutine(Campur(slot, rak, c, l.warnaCairan));

        if (l.bentukEndapan) yield return StartCoroutine(Reaksi(slot, rak, c));

        if (l.tuangKeGelas) yield return StartCoroutine(TuangKeGelas(slot, rak, c, cam));

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



    private GameObject BuatCairanPipet(Transform induk, Color warna, Transform modelPipet)

    {

        GameObject cairanPipet = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        cairanPipet.name = "CairanTerisap_NaCl";

        Collider collider = cairanPipet.GetComponent<Collider>();

        if (collider != null) Destroy(collider);

        cairanPipet.transform.SetParent(induk, false);

        cairanPipet.transform.localPosition = modelPipet != null

            ? modelPipet.localPosition + modelPipet.localRotation * Vector3.Scale(Vector3.up * 0.16f, modelPipet.localScale)

            : Vector3.up * 0.16f;

        if (modelPipet != null) cairanPipet.transform.localRotation = modelPipet.localRotation;

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

            Renderer[] rendererSiap = berikut.model.GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < rendererSiap.Length; i++) rendererSiap[i].enabled = true;

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

        if (materialTetes != null) r.sharedMaterial = materialTetes;

        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        Color w = warna;

        Color dasar = materialTetes != null && materialTetes.HasProperty("_BaseColor") ? materialTetes.GetColor("_BaseColor") : Color.white;

        w.a = dasar.a;

        mpb.SetColor("_BaseColor", w);

        mpb.SetColor("_Color", w);

        r.SetPropertyBlock(mpb);

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



    private IEnumerator PosisikanBotolDiSampingGelas(Transform root, SlotTabung slot, Camera cam, float durasi)

    {

        if (root == null || slot == null || slot.Model == null) yield break;

        if (cam == null) cam = Camera.main;

        Renderer[] gelasRenderer = slot.Model.GetComponentsInChildren<Renderer>(true);

        Renderer[] botolRenderer = root.GetComponentsInChildren<Renderer>(true);

        if (gelasRenderer.Length == 0 || botolRenderer.Length == 0) yield break;

        Bounds gb = new Bounds();
        bool punyaBoundsGelas = false;
        for (int i = 0; i < gelasRenderer.Length; i++)
        {
            Transform tr = gelasRenderer[i].transform;
            bool cairanSebelumnya = false;
            while (tr != null && tr != slot.Model.transform)
            {
                if (tr.name.Contains("Akuades_Liquid_Soal2") || tr.name.Contains("Pivot_Akuades_Soal2") || tr.name.Contains("CairanAkuades_Soal2"))
                {
                    cairanSebelumnya = true;
                    break;
                }
                tr = tr.parent;
            }
            if (cairanSebelumnya) continue;
            if (!punyaBoundsGelas) { gb = gelasRenderer[i].bounds; punyaBoundsGelas = true; }
            else gb.Encapsulate(gelasRenderer[i].bounds);
        }
        if (!punyaBoundsGelas) yield break;

        Bounds bb = botolRenderer[0].bounds;
        for (int i = 1; i < botolRenderer.Length; i++) bb.Encapsulate(botolRenderer[i].bounds);

        Vector3 kanan = cam != null ? cam.transform.right : Vector3.right;

        Vector3 atas = cam != null ? cam.transform.up : Vector3.up;

        float halfGlassRight = Mathf.Abs(kanan.x) * gb.extents.x + Mathf.Abs(kanan.y) * gb.extents.y + Mathf.Abs(kanan.z) * gb.extents.z;



        float halfBottleRight = Mathf.Abs(kanan.x) * bb.extents.x + Mathf.Abs(kanan.y) * bb.extents.y + Mathf.Abs(kanan.z) * bb.extents.z;

        float halfGlassUp = Mathf.Abs(atas.x) * gb.extents.x + Mathf.Abs(atas.y) * gb.extents.y + Mathf.Abs(atas.z) * gb.extents.z;

        float side = 1f;

        if (cam != null && cam.WorldToScreenPoint(gb.center).x > Screen.width * 0.5f) side = -1f;

        Vector3 target = gb.center + atas * (halfGlassUp * 1.12f)

            + kanan * side * (halfGlassRight + halfBottleRight + 0.006f);

        Vector3 start = root.position;

        float t = 0f;

        while (t < durasi)

        {

            if (root == null) yield break;

            t += Time.deltaTime;

            root.position = Vector3.Lerp(start, target, EaseInOut(t / durasi));

            yield return null;

        }

        if (root != null) root.position = target;

    }



    private static Transform BuatPivotDasarSumbu(Transform objek, Transform induk, Vector3 sumbu, out float tinggi)

    {

        Renderer r = objek.GetComponent<Renderer>();

        Vector3 axis = sumbu.normalized;

        Vector3 dasar = objek.position;

        tinggi = 0f;

        if (r != null)

        {

            Bounds local = r.localBounds;

            float min = float.PositiveInfinity, max = float.NegativeInfinity;

            for (int i = 0; i < 8; i++)

            {

                Vector3 tanda = new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f);

                Vector3 titik = r.transform.TransformPoint(local.center + Vector3.Scale(local.extents, tanda));

                float proyeksi = Vector3.Dot(titik, axis);

                min = Mathf.Min(min, proyeksi);

                max = Mathf.Max(max, proyeksi);

            }

            tinggi = Mathf.Max(0f, max - min);

            dasar = r.bounds.center + axis * (min - Vector3.Dot(r.bounds.center, axis));

        }

        GameObject pivotObj = new GameObject("Pivot_Akuades_Soal2");

        pivotObj.transform.SetParent(induk, false);

        pivotObj.transform.position = dasar;

        pivotObj.transform.localRotation = Quaternion.identity;

        pivotObj.transform.localScale = Vector3.one;

        objek.SetParent(pivotObj.transform, true);

        return pivotObj.transform;

    }




    private IEnumerator Campur(SlotTabung slot, Transform rak, CairanTabung c, Color warnaTetesan)
    {
        if (c == null || c.silinder == null) yield break;
        c.bobot += c.bobotTunda;
        c.warna = HasilCampur(c);
        if (c.bobotTunda > 0f) c.warna = Color.Lerp(c.warna, warnaTetesan, Mathf.Clamp01(c.bobotTunda / Mathf.Max(0.0001f, c.bobot)));
        c.bobotTunda = 0f;
        Color awal = c.mat != null && c.mat.HasProperty("_BaseColor") ? c.mat.GetColor("_BaseColor") : c.warna;
        Color akhir = c.warna;
        akhir.a = c.alphaTampil;
        float t = 0f;
        while (t < 0.65f)
        {
            if (c.silinder == null) yield break;
            t += Time.deltaTime;
            Color warna = Color.Lerp(awal, akhir, EaseInOut(t / 0.65f));
            TampilkanWarna(c, warna, Mathf.Lerp(c.alphaTampil, c.alphaDasar, EaseInOut(t / 0.65f)));
            yield return null;
        }
        TampilkanWarna(c, akhir, c.alphaDasar);
    }

    private IEnumerator Reaksi(SlotTabung slot, Transform rak, CairanTabung c)
    {
        if (slot == null || rak == null || c == null || c.silinder == null) yield break;
        Color awal = c.warna;
        float t = 0f;
        while (t < 0.8f)
        {
            if (c.silinder == null) yield break;
            t += Time.deltaTime;
            TampilkanWarna(c, Color.Lerp(awal, warnaLarutanKeruh, EaseInOut(t / 0.8f)), c.alphaDasar);
            yield return null;
        }
        c.warna = warnaLarutanKeruh;
        if (c.endapan == null)
        {
            GameObject endapan = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            endapan.name = "Endapan_" + slot.namaSlot;
            Collider col = endapan.GetComponent<Collider>();
            if (col != null) Destroy(col);
            endapan.transform.SetParent(rak, false);
            c.endapan = endapan.transform;
            Renderer er = endapan.GetComponent<Renderer>();
            if (materialCairan != null) { c.matEndapan = new Material(materialCairan); er.sharedMaterial = c.matEndapan; }
            TampilkanEndapan(c);
        }
        yield return null;
    }

    private void TampilkanEndapan(CairanTabung c)
    {
        if (c == null || c.endapan == null) return;
        c.endapan.gameObject.SetActive(true);
        c.endapan.localPosition = new Vector3(c.endapan.localPosition.x, jarakDasarKaca + c.tinggiEndapan * 0.5f, c.endapan.localPosition.z);
        c.endapan.localScale = new Vector3(radiusCairan * 1.5f, Mathf.Max(0.005f, c.tinggiEndapan * 0.5f), radiusCairan * 1.5f);
        if (c.matEndapan != null) SetWarna(c.matEndapan, warnaEndapan);
    }

    private IEnumerator Jatuh(Vector3 dariDunia, SlotTabung slot, Transform rak, CairanTabung c, float tambahTinggi, Color warna, float kekuatan, bool tundaWarna)
    {
        tetesAktif++;
        GameObject tetes = BuatBola(rak, ukuranTetes, warna);
        Vector3 awal = rak.InverseTransformPoint(dariDunia);
        tetes.transform.localPosition = awal;
        float permukaan = slot.lubangLokal.y + jarakDasarKaca + (c != null ? Mathf.Clamp(c.tinggi, 0f, maksTinggiCairan) : 0f);
        Vector3 akhir = new Vector3(slot.lubangLokal.x, permukaan, slot.lubangLokal.z);
        float durasi = 0.25f;
        float waktu = 0f;
        while (waktu < durasi)
        {
            if (tetes == null || rak == null) break;
            waktu += Time.deltaTime;
            float u = Mathf.Clamp01(waktu / durasi);
            float e = u * u;
            tetes.transform.localPosition = Vector3.Lerp(awal, akhir, e);
            yield return null;
        }
        if (tetes != null) Destroy(tetes);
        if (c != null && c.silinder != null)
        {
            float bobot = tambahTinggi * kekuatan;
            if (tundaWarna) { c.warnaTunda = warna; c.bobotTunda += bobot; }
            else { c.warna = warna; c.bobot += bobot; TampilkanWarna(c, warna, c.alphaDasar); }
            c.tinggi += tambahTinggi;
            TerapkanCairan(slot, c);
        }
        tetesAktif = Mathf.Max(0, tetesAktif - 1);
    }

    private IEnumerator TuangAkuadesKeGelas(SlotTabung slot, Transform rak, Transform ujungBotol, int volumeMl, Color warna)

    {

        if (slot == null || slot.Model == null || rak == null || ujungBotol == null) yield break;

        Transform gelas = slot.Model.transform;

        Transform pivotLama = gelas.Find("Pivot_Akuades_Soal2");

        if (pivotLama != null) Destroy(pivotLama.gameObject);

        Transform silinderLama = gelas.Find("CairanAkuades_Soal2");

        if (silinderLama != null) Destroy(silinderLama.gameObject);

        Transform template = prefabGelasKimia != null ? prefabGelasKimia.transform.Find("PbCl2_Liquid") : null;

        if (template == null)

        {

            Debug.LogWarning("ManajerTakaran: prefabGelasKimia harus memakai model GelasKimiaAkuades yang memiliki PbCl2_Liquid.");

            yield break;

        }

        GameObject isiObj = Instantiate(template.gameObject, gelas, false);

        isiObj.name = "Akuades_Liquid_Soal2";

        Renderer isiRenderer = isiObj.GetComponent<Renderer>();

        BeriWarna(isiRenderer, warna, 0.62f);

        float tinggiPenuh;

        Transform pivot = BuatPivotDasarSumbu(isiObj.transform, gelas, gelas.up, out tinggiPenuh);

        pivot.localScale = new Vector3(1f, 0.0001f, 1f);

        float rasioIsi = 0.12f * Mathf.Clamp01(volumeMl / 10f);

        int jumlahTetes = Mathf.Clamp(volumeMl * 3, 6, 30);

        float durasiTuang = Mathf.Clamp(volumeMl * 0.24f, 1.8f, 3.0f);

        float waktu = 0f, jedaTetes = 0f;

        while (waktu < durasiTuang)

        {

            if (gelas == null || ujungBotol == null || rak == null || pivot == null) yield break;

            waktu += Time.deltaTime;

            float u = EaseInOut(waktu / durasiTuang);

            float rasioSaatIni = rasioIsi * u;

            pivot.localScale = new Vector3(1f, Mathf.Max(0.0001f, rasioSaatIni), 1f);

            jedaTetes -= Time.deltaTime;

            if (jedaTetes <= 0f)

            {

                jedaTetes = durasiTuang / jumlahTetes;

                Vector3 titikPermukaan = pivot.position + pivot.up * Mathf.Max(0.015f, tinggiPenuh * rasioSaatIni);

                StartCoroutine(AliranAkuades(ujungBotol.position, rak, rak.InverseTransformPoint(titikPermukaan), warna, Mathf.Min(tinggiPenuh, 1f) * 0.045f));

            }

            yield return null;

        }

        float tunggu = 0.45f;

        while (tunggu > 0f && gelas != null)

        {

            tunggu -= Time.deltaTime;

            yield return null;

        }

    }



    private static float ArahZKamera(Transform rak, Camera cam)

    {

        if (cam == null) return -1f;

        Vector3 arah = rak.InverseTransformDirection(-cam.transform.forward);

        return arah.z >= 0f ? 1f : -1f;

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

        Color c = warna;

        c.a = alpha;

        mpb.SetColor("_BaseColor", c);

        mpb.SetColor("_Color", c);

        r.SetPropertyBlock(mpb);

    }



    private GelasKimia BuatGelas(Transform rak, Vector3 posLokal)

    {

        if (prefabGelasKimia == null) return null;

        GameObject go = Instantiate(prefabGelasKimia);

        go.name = "GelasKimia_PbCl2";

        go.SetActive(true);

        Transform root = go.transform;

        root.SetParent(rak, false);

        root.localPosition = posLokal;

        root.localRotation = Quaternion.identity;

        root.localScale = Vector3.one;

        foreach (Light lt in go.GetComponentsInChildren<Light>(true)) Destroy(lt);

        foreach (Camera km in go.GetComponentsInChildren<Camera>(true)) Destroy(km);

        foreach (AudioListener al in go.GetComponentsInChildren<AudioListener>(true)) Destroy(al);



        GelasKimia g = new GelasKimia();

        g.root = root;

        Transform tCairan = null, tEndapan = null;

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



    private IEnumerator AliranAkuades(Vector3 dariDunia, Transform rak, Vector3 tujuanLokal, Color warna, float diameterDunia)

    {

        if (rak == null) yield break;

        float skalaRak = (Mathf.Abs(rak.lossyScale.x) + Mathf.Abs(rak.lossyScale.y) + Mathf.Abs(rak.lossyScale.z)) / 3f;

        float ukuranLokal = Mathf.Clamp(diameterDunia / Mathf.Max(0.0001f, skalaRak), 0.004f, 0.06f);

        GameObject d = BuatBola(rak, ukuranLokal, warna);

        Vector3 awal = rak.InverseTransformPoint(dariDunia);

        d.transform.localPosition = awal;

        float jarak = Mathf.Max(0.05f, awal.y - tujuanLokal.y);

        float dur = Mathf.Clamp(Mathf.Sqrt(2f * jarak / 24f), 0.22f, 0.7f);

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

        animasiSoal6Selesai = false;

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

            animasiSoal6Selesai = true;

            yield break;

        }



        Transform rak = (slotA != null && slotA.Model != null) ? slotA.Model.transform.parent

                       : (slotB != null && slotB.Model != null) ? slotB.Model.transform.parent : null;

        if (rak == null) { animasiSoal6Selesai = true; yield break; }



        bool adaA = slotA != null && cairan.ContainsKey(slotA);

        bool adaB = slotB != null && cairan.ContainsKey(slotB);

        if (!adaA && !adaB) { animasiSoal6Selesai = true; yield break; }



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

        if (g == null) { animasiSoal6Selesai = true; yield break; }

        gelasAktif = g.root.gameObject;

        yield return PopIn(g.root, 0.4f);

        yield return BesarkanGelas(g.root, 1f, 1.6f, 0.4f);



        // 4. Lanjutkan tuang untuk tabung yang sudah terangkat

        if (rigA != null) yield return LanjutkanTuang(rigA, rak, g, posGelas, 0f, 0.5f);

        if (rigB != null) yield return LanjutkanTuang(rigB, rak, g, posGelas, 0.5f, 1f);



        if (g.pivotEndapan != null) yield return PopIn(g.pivotEndapan, 0.5f);

        // Pertahankan gelas hasil sampai tombol Data diklik.

        // Panel kesimpulan memanggil SembunyikanGelasReaksi untuk menutup hasil dan memulihkan rak.

        animasiSoal6Selesai = true;

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

