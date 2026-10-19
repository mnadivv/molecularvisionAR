using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Script: Klik komponen pemicu → munculkan gambar/pesan di Content.
/// Tutup dengan klik halaman kosong (background).
/// Jumlah pasangan pemicu → gambar bisa dikustom.
/// </summary>
public class PesanMulaiInvalid : MonoBehaviour
{
    [System.Serializable]
    public class PasanganPemicuPesan
    {
        [Tooltip("Tombol / komponen yang diklik untuk memicu pesan.")]
        public Button pemicu;

        [Tooltip("Gambar / panel pesan yang akan ditampilkan di Content.")]
        public GameObject pesan;

        [Tooltip("Nama opsional untuk identifikasi di Inspector.")]
        public string nama;
    }

    [Header("=== DAFTAR PASANGAN PEMICU → PESAN ===")]
    [Tooltip("Tambahkan pasangan: tombol mana memunculkan gambar mana.\n" +
             "Jumlahnya bebas, sesuai kebutuhan.")]
    public List<PasanganPemicuPesan> daftarPasangan = new List<PasanganPemicuPesan>();

    [Header("=== CONTENT (TEMPAT PESAN MUNCUL) ===")]
    [Tooltip("Parent Content tempat gambar/pesan akan ditampilkan.\n" +
             "Biasanya: ScrollAlat > Viewport > Content")]
    public Transform contentParent;

    [Header("=== AREA KLIK UNTUK MENUTUP ===")]
    [Tooltip("Objek 'halaman kosong' / background yang diklik untuk menutup pesan.\n" +
             "Biasanya: BackgroundShadow atau Panel transparan di belakang.")]
    public GameObject halamanKosong;

    [Tooltip("Kalau true, halamanKosong akan otomatis diberi Button + listener untuk menutup.")]
    public bool autoSetupHalamanKosong = true;

    [Header("=== EFEK SUARA (opsional) ===")]
    public AudioSource audioSource;
    public AudioClip suaraKlik;

    // === Variabel internal ===
    private GameObject pesanAktif = null;
    private bool sedangProses = false;

    void Start()
    {
        // Setup semua tombol pemicu
        for (int i = 0; i < daftarPasangan.Count; i++)
        {
            var pasangan = daftarPasangan[i];
            if (pasangan.pemicu == null) continue;

            int capturedIndex = i;
            pasangan.pemicu.onClick.RemoveAllListeners();
            pasangan.pemicu.onClick.AddListener(() => TampilkanPesan(capturedIndex));
        }

        // Sembunyikan semua pesan di awal
        for (int i = 0; i < daftarPasangan.Count; i++)
        {
            if (daftarPasangan[i].pesan != null)
                daftarPasangan[i].pesan.SetActive(false);
        }

        // Setup halaman kosong untuk menutup
        if (autoSetupHalamanKosong && halamanKosong != null)
        {
            Button btnKosong = halamanKosong.GetComponent<Button>();
            if (btnKosong == null)
                btnKosong = halamanKosong.AddComponent<Button>();

            // Supaya klik di area kosong tetap terdeteksi meski tidak ada Image
            Image img = halamanKosong.GetComponent<Image>();
            if (img == null)
            {
                img = halamanKosong.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0); // transparan
            }
            img.raycastTarget = true;

            btnKosong.onClick.RemoveAllListeners();
            btnKosong.onClick.AddListener(TutupPesan);
        }

        // AudioSource otomatis
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Tampilkan pesan berdasarkan index pasangan.
    /// </summary>
    public void TampilkanPesan(int index)
    {
        if (sedangProses) return;
        if (index < 0 || index >= daftarPasangan.Count) return;

        var pasangan = daftarPasangan[index];
        if (pasangan.pesan == null)
        {
            Debug.LogWarning($"Pesan index {index} belum di-set!");
            return;
        }

        MainkanSuaraKlik();

        // Sembunyikan pesan lama kalau ada
        if (pesanAktif != null)
            pesanAktif.SetActive(false);

        // Pindahkan pesan ke Content kalau belum
        if (contentParent != null && pasangan.pesan.transform.parent != contentParent)
            pasangan.pesan.transform.SetParent(contentParent, false);

        // Tampilkan pesan baru
        pasangan.pesan.SetActive(true);
        pasangan.pesan.transform.SetAsLastSibling(); // tampil paling depan
        pesanAktif = pasangan.pesan;

        // Pastikan halaman kosong aktif & di depan supaya bisa diklik untuk close
        if (halamanKosong != null)
        {
            halamanKosong.SetActive(true);
            halamanKosong.transform.SetAsLastSibling();
        }

        Debug.Log($"✅ Menampilkan pesan: {pasangan.nama}");
    }

    /// <summary>
    /// Tutup pesan yang sedang aktif.
    /// </summary>
    public void TutupPesan()
    {
        if (pesanAktif == null) return;

        MainkanSuaraKlik();
        pesanAktif.SetActive(false);
        pesanAktif = null;

        Debug.Log("❌ Pesan ditutup.");
    }

    /// <summary>
    /// Cek apakah sedang ada pesan aktif.
    /// </summary>
    public bool AdaPesanAktif()
    {
        return pesanAktif != null && pesanAktif.activeSelf;
    }

    void MainkanSuaraKlik()
    {
        if (audioSource == null) return;
        if (suaraKlik != null)
            audioSource.PlayOneShot(suaraKlik);
        else if (audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }
}