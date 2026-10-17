using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di satu objek yang selalu aktif (mis. ScriptLogic/ManajerLatihan).
/// Mengatur alat/bahan mana yang BOLEH diambil di setiap soal.
/// - Soal yang sedang tampil dikenali dari panel soal yang aktif.
/// - Alat/bahan yang bukan jawaban soal itu (atau diambil saat belum ada soal) memunculkan gambar salah,
///   lalu hilang sendiri setelah durasiSalah detik.
/// - Alat/bahan yang benar memakai logika yang sudah ada (mis. CardTahanGeser3D), DAN tahap soal itu
///   otomatis ditandai selesai begitu semua item benarnya sudah diambil (tahan sampai waktuTahan).
/// - Tahap yang belum selesai mengunci ButtonLanjut soal itu (lihat GerbangLanjut).
/// </summary>
public class ManajerPilihItem : MonoBehaviour
{
    public static ManajerPilihItem Instance { get; private set; }

    [System.Serializable]
    public class TahapSoal
    {
        public string nama;
        [Tooltip("Panel soal (Soal1, Soal2, ...). Tahap ini berlaku selama panel ini tampil.")]
        public GameObject panelSoal;
        [Tooltip("Kartu alat/bahan (di ScrollAlat / ScrollBahan) yang BOLEH diambil di soal ini.")]
        public List<GameObject> itemBenar = new List<GameObject>();
        [Tooltip("Opsional. Kalau diisi, soal ini baru dianggap selesai setelah SEMUA slot di pengelola ini terisi dan animasi masuknya selesai (mis. tabung A dan B sudah masuk rak).")]
        public ManajerSlotTabung syaratSlot;
        [Tooltip("Otomatis diisi saat main: true kalau semua itemBenar di atas sudah diambil (tahan).\n" +
                 "Kosongkan itemBenar kalau soal ini belum dikonfigurasi -> ButtonLanjut-nya tidak dikunci.")]
        [HideInInspector] public bool selesai;
    }

    [Header("=== ATURAN PER SOAL ===")]
    public List<TahapSoal> tahap = new List<TahapSoal>();

    [Header("=== PESAN SALAH ===")]
    [Tooltip("Gambar pesan salah. Pasang sprite buatan sendiri di Image-nya. Disembunyikan di awal.")]
    public GameObject gambarSalah;
    [Tooltip("Gambar saat belum masuk soal (AwalMulai/PilihanPercobaan).")]
    public GameObject gambarBelumMulai;
    [Tooltip("Panel yang dianggap belum di soal: AwalMulai, PilihanPercobaan.")]
    public GameObject[] panelBelumMulai;
    [Tooltip("Lama gambar salah tampil (detik).")]
    public float durasiSalah = 2f;
    [Tooltip("Kalau aktif, mengambil alat/bahan saat belum ada soal yang tampil (mis. sebelum Mulai) juga dianggap salah.")]
    public bool salahSaatBelumMulai = true;

    [Header("=== SUARA SALAH (opsional) ===")]
    public AudioSource audioSource;
    public AudioClip suaraSalah;
    [Range(0f, 1f)] public float volumeSuara = 1f;

    private Coroutine timer;

    // index tahap -> item yang sudah diambil (benar) untuk tahap itu
    private readonly Dictionary<int, HashSet<GameObject>> dipakai = new Dictionary<int, HashSet<GameObject>>();

    private void Awake()
    {
        Instance = this;
        if (gambarSalah != null) gambarSalah.SetActive(false);
        if (gambarBelumMulai != null) gambarBelumMulai.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Soal yang sedang tampil, atau null kalau tidak ada.</summary>
    public TahapSoal TahapAktif()
    {
        for (int i = 0; i < tahap.Count; i++)
        {
            TahapSoal t = tahap[i];
            if (t != null && t.panelSoal != null && t.panelSoal.activeInHierarchy) return t;
        }
        return null;
    }

    /// <summary>True kalau alat/bahan ini jawaban untuk soal yang sedang tampil.</summary>
    public bool BolehDipakai(GameObject item)
    {
        TahapSoal t = TahapAktif();
        return t != null && t.itemBenar != null && t.itemBenar.Contains(item);
    }

    /// <summary>Dipanggil ItemPilihGate saat item yang BENAR ditahan cukup lama (dianggap sudah diambil).</summary>
    public void TandaiDipakai(GameObject item)
    {
        for (int i = 0; i < tahap.Count; i++)
        {
            TahapSoal t = tahap[i];
            if (t == null || t.itemBenar == null || !t.itemBenar.Contains(item)) continue;

            if (!dipakai.TryGetValue(i, out HashSet<GameObject> set))
            {
                set = new HashSet<GameObject>();
                dipakai[i] = set;
            }
            set.Add(item);

            if (set.Count >= t.itemBenar.Count)
                t.selesai = true;
        }
    }

    /// <summary>
    /// True kalau soal (panelSoal) ini sudah dikerjakan (semua item benarnya diambil),
    /// atau soal ini belum dikonfigurasi (itemBenar kosong) sehingga tidak perlu dikunci,
    /// atau panelSoal tidak terdaftar di sini sama sekali (bukan tanggung jawab manajer ini).
    /// </summary>
    public bool TahapSelesai(GameObject panelSoal)
    {
        for (int i = 0; i < tahap.Count; i++)
        {
            TahapSoal t = tahap[i];
            if (t == null || t.panelSoal != panelSoal) continue;
            bool takaranOk = ManajerTakaran.Instance == null || ManajerTakaran.Instance.TakaranSelesai(panelSoal);
            bool itemOk = t.itemBenar == null || t.itemBenar.Count == 0 || t.selesai;
            if (t.syaratSlot != null) return itemOk && takaranOk && t.syaratSlot.SemuaSelesai();
            return itemOk && takaranOk;
        }
        return true;
    }

    /// <summary>Kunci ulang semua tahap (dipanggil saat progres di-reset).</summary>
    public void ResetSemuaTahap()
    {
        dipakai.Clear();
        for (int i = 0; i < tahap.Count; i++)
            if (tahap[i] != null) tahap[i].selesai = false;

        // Bersihkan model 3D yang sudah ditaruh (rak dan tabung)
        ManajerSlotTabung[] semua = FindObjectsByType<ManajerSlotTabung>(FindObjectsInactive.Include);
        for (int i = 0; i < semua.Length; i++) semua[i].Bersihkan();
        if (ManajerTakaran.Instance != null) ManajerTakaran.Instance.ResetSemua();
    }

    /// <summary>Munculkan gambar salah, lalu sembunyikan otomatis setelah durasiSalah.</summary>
    public bool SedangBelumMulai()
    {
        if (panelBelumMulai == null) return false;
        for (int i = 0; i < panelBelumMulai.Length; i++)
            if (panelBelumMulai[i] != null && panelBelumMulai[i].activeInHierarchy) return true;
        return false;
    }

    public void TampilkanSalah()
    {
        if (TahapAktif() == null && !salahSaatBelumMulai) return;

        if (gambarSalah == null)
        {
            Debug.LogWarning("ManajerPilihItem: gambarSalah belum diisi.");
            return;
        }

        Transform sementara = gambarSalah.transform.Find("TeksSementara");
        if (sementara != null)
        {
            Image img = gambarSalah.GetComponent<Image>();
            sementara.gameObject.SetActive(img == null || img.sprite == null);
        }

        gambarSalah.transform.SetAsLastSibling();
        bool bm = SedangBelumMulai();
        GameObject tampil = (bm && gambarBelumMulai != null) ? gambarBelumMulai : gambarSalah;
        GameObject tutup  = (bm && gambarBelumMulai != null) ? gambarSalah : gambarBelumMulai;
        if (tampil != null) tampil.SetActive(true);
        if (tutup  != null) tutup.SetActive(false);
        MainkanSuara();

        if (timer != null) StopCoroutine(timer);
        timer = StartCoroutine(SembunyikanSetelah(durasiSalah));
    }

    public void SembunyikanSalah()
    {
        if (timer != null) { StopCoroutine(timer); timer = null; }
        if (gambarSalah != null) gambarSalah.SetActive(false);
        if (gambarBelumMulai != null) gambarBelumMulai.SetActive(false);
    }

    private IEnumerator SembunyikanSetelah(float detik)
    {
        yield return new WaitForSecondsRealtime(detik);
        timer = null;
        if (gambarSalah != null) gambarSalah.SetActive(false);
        if (gambarBelumMulai != null) gambarBelumMulai.SetActive(false);
    }

    private void MainkanSuara()
    {
        if (audioSource == null || suaraSalah == null || !audioSource.gameObject.activeInHierarchy) return;
        audioSource.PlayOneShot(suaraSalah, volumeSuara);
    }
}
