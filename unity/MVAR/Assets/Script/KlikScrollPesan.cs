using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script untuk:
/// - Klik Image di ScrollAlat → memunculkan PESAN ERROR (auto-hide 2 detik).
/// - Kalau TOMBOL SYARAT sudah diklik → klik Image TIDAK memunculkan pesan error.
/// </summary>
public class KlikScrollPesan : MonoBehaviour
{
    [Header("=== TOMBOL SYARAT (yang harus diklik dulu) ===")]
    [Tooltip("Drag tombol yang harus diklik dulu. Setelah salah satu diklik, klik Image tidak akan memunculkan pesan error lagi.")]
    public List<Button> tombolSyarat = new List<Button>();

    [Header("=== DAFTAR IMAGE YANG BISA DIKLIK ===")]
    [Tooltip("Drag semua Image di ScrollAlat yang mau bisa diklik.")]
    public List<Image> daftarTombolAlat = new List<Image>();

    [Header("=== GAMBAR PESAN ERROR ===")]
    [Tooltip("Gambar pesan error yang muncul saat Image diklik (sebelum tombol syarat diklik).")]
    public GameObject gambarPesanError;

    [Tooltip("Berapa detik pesan error tampil sebelum hilang otomatis.")]
    public float durasiPesanError = 2f;

    [Header("=== AUDIO KLIK (SATU UNTUK SEMUA) ===")]
    [Tooltip("AudioSource untuk memutar suara. Taruh di objek yang selalu aktif.")]
    public AudioSource audioSource;

    [Tooltip("File suara klik yang diputar saat Image diklik.")]
    public AudioClip suaraKlik;

    [Range(0f, 1f)]
    public float volumeSuara = 1f;

    // Status apakah tombol syarat sudah diklik
    private bool syaratTerpenuhi = false;

    // Coroutine timer untuk pesan error
    private Coroutine timerError;

    void Start()
    {
        // 1. Cari AudioSource kalau belum diisi
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 2. Sembunyikan gambar pesan error di awal
        if (gambarPesanError != null)
            gambarPesanError.SetActive(false);

        // 3. Setup klik pada tiap Image di ScrollAlat
        foreach (Image img in daftarTombolAlat)
        {
            if (img == null) continue;

            Button btn = img.GetComponent<Button>();
            if (btn == null)
                btn = img.gameObject.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnAlatDiklik);
        }

        // 4. Setup tombol syarat
        foreach (Button btnSyarat in tombolSyarat)
        {
            if (btnSyarat == null) continue;
            btnSyarat.onClick.RemoveAllListeners();
            btnSyarat.onClick.AddListener(OnTombolSyaratDiklik);
        }
    }

    /// <summary>
    /// Dipanggil saat tombol syarat diklik.
    /// Setelah ini, klik Image tidak akan memunculkan pesan error.
    /// </summary>
    void OnTombolSyaratDiklik()
    {
        syaratTerpenuhi = true;
        Debug.Log("[KlikScrollPesan] ✅ Tombol syarat diklik → syarat terpenuhi.");
    }

    void OnAlatDiklik()
    {
        // 1. Mainkan suara klik
        MainkanSuara();

        // 2. Kalau syarat BELUM terpenuhi → tampilkan pesan error
        if (!syaratTerpenuhi)
        {
            Debug.Log("[KlikScrollPesan] ⚠️ Tampilkan pesan error (syarat belum terpenuhi).");
            TampilkanPesanError();
            return;
        }

        // 3. Kalau syarat sudah terpenuhi → tidak ada pesan error
        Debug.Log("[KlikScrollPesan] ✅ Syarat terpenuhi. Tidak ada pesan error.");
    }

    /// <summary>
    /// Tampilkan pesan error dengan timer auto-hide.
    /// Kalau diklik lagi, timer di-reset.
    /// </summary>
    void TampilkanPesanError()
    {
        if (gambarPesanError == null)
        {
            Debug.LogWarning("[KlikScrollPesan] Gambar Pesan Error belum diisi!");
            return;
        }

        gambarPesanError.SetActive(true);

        if (timerError != null)
            StopCoroutine(timerError);

        timerError = StartCoroutine(HideErrorAfterDelay(durasiPesanError));
    }

    IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gambarPesanError != null)
            gambarPesanError.SetActive(false);

        timerError = null;
        Debug.Log("[KlikScrollPesan] ⏱️ Pesan error hilang otomatis.");
    }

    void MainkanSuara()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[KlikScrollPesan] AudioSource belum diisi!");
            return;
        }

        if (!audioSource.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[KlikScrollPesan] AudioSource di '{audioSource.gameObject.name}' nonaktif!");
            return;
        }

        AudioClip clip = suaraKlik != null ? suaraKlik : audioSource.clip;
        if (clip == null)
        {
            Debug.LogWarning("[KlikScrollPesan] Tidak ada clip suara!");
            return;
        }

        audioSource.PlayOneShot(clip, volumeSuara);
    }
}