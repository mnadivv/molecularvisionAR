using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Script HINT SEQUENCE:
/// - Klik tombol Hint → tampilkan petunjuk berurutan + shadow + suara
/// - Klik di AREA petunjuk juga bisa lanjut ke berikutnya
/// - Klik di SHADOW juga bisa lanjut
/// - Klik tombol LAIN (Settings, Download, dll) → otomatis tutup semua
/// </summary>
public class HintSequence : MonoBehaviour
{
    [Header("=== DAFTAR PETUNJUK (URUT) ===")]
    [Tooltip("Drag Petunjuk1, Petunjuk2, ... ke sini. Urutan sesuai Element.")]
    public GameObject[] daftarPetunjuk;

    [Header("=== BACKGROUND SHADOW ===")]
    [Tooltip("Shadow yang muncul bersamaan dengan petunjuk.")]
    public GameObject backgroundShadow;

    [Header("=== AUDIO KLIK ===")]
    public AudioSource audioSource;
    public AudioClip suaraKlik;
    [Range(0f, 1f)]
    public float volumeSuara = 1f;

    [Header("=== KLIK DI SHADOW = LANJUT? ===")]
    [Tooltip("Kalau ✅, klik di shadow = lanjut ke petunjuk berikutnya.\n" +
             "Kalau ❌, klik di shadow = tutup semua petunjuk.")]
    public bool klikShadowLanjut = true;

    [Header("=== TOMBOL LAIN YANG MENUTUP HINT ===")]
    [Tooltip("Drag tombol lain (Settings, Download, ButtonAlat, ButtonBahan, dll).\n" +
             "Kalau tombol-tombol ini diklik, hint otomatis tertutup.")]
    public Button[] tombolYangMenutupHint;

    // === Internal ===
    private int indexSekarang = -1;
    private bool hintAktif = false;

    void Start()
    {
        // 1. Setup tombol Hint utama
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnHintDiklik);

        // 2. Setup AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 3. Setup klik di area petunjuk (EventTrigger)
        SetupKlikPadaPetunjuk();

        // 4. Setup klik di shadow
        SetupKlikPadaShadow();

        // 5. Setup tombol lain yang menutup hint
        SetupTombolPenutup();

        // 6. Sembunyikan semua petunjuk + shadow di awal
        foreach (GameObject p in daftarPetunjuk)
        {
            if (p != null) p.SetActive(false);
        }
        if (backgroundShadow != null)
            backgroundShadow.SetActive(false);
    }

    /// <summary>
    /// Tambahkan EventTrigger ke setiap petunjuk, supaya klik di area petunjuk juga lanjut.
    /// </summary>
    void SetupKlikPadaPetunjuk()
    {
        foreach (GameObject p in daftarPetunjuk)
        {
            if (p == null) continue;

            // Pastikan ada komponen Image dengan Raycast Target
            Image img = p.GetComponent<Image>();
            if (img == null) img = p.AddComponent<Image>();
            img.raycastTarget = true;

            // Setup EventTrigger
            EventTrigger trigger = p.GetComponent<EventTrigger>();
            if (trigger == null) trigger = p.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnHintDiklik(); });
            trigger.triggers.Add(entry);
        }
    }

    /// <summary>
    /// Setup klik pada shadow.
    /// </summary>
    void SetupKlikPadaShadow()
    {
        if (backgroundShadow == null) return;

        // Pastikan ada Image dengan Raycast Target
        Image img = backgroundShadow.GetComponent<Image>();
        if (img == null) img = backgroundShadow.AddComponent<Image>();
        img.raycastTarget = true;

        // Setup Button component
        Button btn = backgroundShadow.GetComponent<Button>();
        if (btn == null) btn = backgroundShadow.AddComponent<Button>();

        btn.onClick.RemoveAllListeners();

        if (klikShadowLanjut)
        {
            // Klik shadow = lanjut
            btn.onClick.AddListener(OnHintDiklik);
        }
        else
        {
            // Klik shadow = tutup semua
            btn.onClick.AddListener(TutupSemuaHint);
        }
    }

    /// <summary>
    /// Setup tombol-tombol lain yang menutup hint saat diklik.
    /// </summary>
    void SetupTombolPenutup()
    {
        foreach (Button b in tombolYangMenutupHint)
        {
            if (b == null) continue;

            b.onClick.AddListener(() =>
            {
                if (hintAktif)
                    TutupSemuaHint();
            });
        }
    }

    /// <summary>
    /// Dipanggil saat Hint diklik (dari tombol Hint, dari area petunjuk, atau dari shadow).
    /// </summary>
    void OnHintDiklik()
    {
        // Mainkan suara
        MainkanSuara();

        // Cek apakah masih ada petunjuk berikutnya
        int indexBerikutnya = indexSekarang + 1;

        if (indexBerikutnya < daftarPetunjuk.Length)
        {
            // === TAMPILKAN PETUNJUK BERIKUTNYA ===

            // Sembunyikan petunjuk sebelumnya
            if (indexSekarang >= 0 && daftarPetunjuk[indexSekarang] != null)
                daftarPetunjuk[indexSekarang].SetActive(false);

            // Tampilkan petunjuk berikutnya
            if (daftarPetunjuk[indexBerikutnya] != null)
                daftarPetunjuk[indexBerikutnya].SetActive(true);

            // Tampilkan shadow
            if (backgroundShadow != null && !backgroundShadow.activeSelf)
                backgroundShadow.SetActive(true);

            indexSekarang = indexBerikutnya;
            hintAktif = true;

            Debug.Log($"[HintSequence] Tampilkan petunjuk ke-{indexSekarang + 1}");
        }
        else
        {
            // === TUTUP SEMUA (tidak ada lanjutan) ===
            TutupSemuaHint();
        }
    }

    /// <summary>
    /// Tutup semua petunjuk + shadow.
    /// </summary>
    public void TutupSemuaHint()
    {
        // Sembunyikan semua petunjuk
        for (int i = 0; i < daftarPetunjuk.Length; i++)
        {
            if (daftarPetunjuk[i] != null)
                daftarPetunjuk[i].SetActive(false);
        }

        // Sembunyikan shadow
        if (backgroundShadow != null)
            backgroundShadow.SetActive(false);

        indexSekarang = -1;
        hintAktif = false;

        Debug.Log("[HintSequence] Semua petunjuk ditutup.");
    }

    /// <summary>
    /// Mainkan suara klik.
    /// </summary>
    void MainkanSuara()
    {
        if (audioSource == null) return;
        if (!audioSource.gameObject.activeInHierarchy) return;

        AudioClip clip = suaraKlik != null ? suaraKlik : audioSource.clip;
        if (clip == null) return;

        audioSource.PlayOneShot(clip, volumeSuara);
    }

    /// <summary>
    /// Fungsi publik untuk reset dari luar.
    /// </summary>
    public void ResetHint()
    {
        TutupSemuaHint();
    }
}