using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Script SIMPLE: Klik tombol → sembunyikan tombol lain → tampilkan BANYAK panel
/// + suara klik + tombol close opsional + background shadow opsional (klik untuk tutup).
/// </summary>
public class SimpleTogglePanel : MonoBehaviour
{
    [Header("=== TOMBOL YANG DIHILANGKAN SAAT DIKLIK ===")]
    [Tooltip("Drag tombol-tombol yang mau HILANG saat tombol ini diklik.")]
    public GameObject[] tombolYangDihilangkan;

    [Header("=== PANEL YANG DITAMPILKAN (bisa banyak) ===")]
    [Tooltip("Drag semua panel yang mau DITAMPILKAN saat tombol ini diklik.")]
    public GameObject[] panelYangDitampilkan;

    [Header("=== AUDIO KLIK ===")]
    [Tooltip("Audio Source. Taruh di objek yang selalu aktif (misal AudioManager).")]
    public AudioSource audioSource;
    [Tooltip("File suara klik (.mp3 / .wav).")]
    public AudioClip suaraKlik;
    [Range(0f, 1f)]
    public float volumeSuara = 1f;

    [Header("=== BACKGROUND SHADOW (opsional) ===")]
    [Tooltip("Panel gelap transparan di belakang panel utama.\n" +
             "Kalau diisi, otomatis muncul saat panel dibuka.\n" +
             "Bisa diklik untuk tutup panel (seperti modal).")]
    public GameObject backgroundShadow;

    [Header("=== TOMBOL CLOSE DI PANEL (opsional) ===")]
    [Tooltip("Tombol close (bisa Button / Image). Drag ButtonClose ke sini.")]
    public Button tombolClose;

    [Header("=== YANG DIMUNCULKAN KEMBALI SAAT CLOSE (opsional) ===")]
    [Tooltip("Objek yang dimunculkan kembali saat tombol close diklik.")]
    public GameObject[] objekYangDimunculkanSaatClose;

    void Start()
    {
        // 1. Setup tombol utama
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnTombolDiklik);

        // 2. Setup tombol close
        if (tombolClose != null)
        {
            tombolClose.onClick.RemoveAllListeners();
            tombolClose.onClick.AddListener(OnTombolCloseDiklik);
        }

        // 3. Setup background shadow (klik untuk tutup)
        if (backgroundShadow != null)
        {
            Button shadowBtn = backgroundShadow.GetComponent<Button>();
            if (shadowBtn == null)
                shadowBtn = backgroundShadow.AddComponent<Button>();

            shadowBtn.onClick.RemoveAllListeners();
            shadowBtn.onClick.AddListener(OnTombolCloseDiklik);

            // Sembunyikan shadow di awal
            backgroundShadow.SetActive(false);
        }

        // 4. Cari AudioSource kalau belum diisi
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTombolDiklik()
    {
        // 1. Mainkan suara klik
        MainkanSuara();

        // 2. Sembunyikan tombol-tombol
        if (tombolYangDihilangkan != null)
        {
            foreach (GameObject tombol in tombolYangDihilangkan)
            {
                if (tombol != null)
                    tombol.SetActive(false);
            }
        }

        gameObject.SetActive(false);

        // 3. Tampilkan background shadow (kalau diisi)
        if (backgroundShadow != null)
            backgroundShadow.SetActive(true);

        // 4. Tampilkan SEMUA panel di list (bisa banyak)
        foreach (GameObject panel in panelYangDitampilkan)
        {
            if (panel == null) continue;

            panel.SetActive(true);

            // Reset item di dalam panel (kalau ada KlikHilangBerurutan)
            KlikHilangBerurutan scriptKlik = panel.GetComponentInChildren<KlikHilangBerurutan>();
            if (scriptKlik != null)
                scriptKlik.ResetSemua();
        }
    }

    void OnTombolCloseDiklik()
    {
        // 1. Mainkan suara klik
        MainkanSuara();

        // 2. Sembunyikan SEMUA panel di list
        foreach (GameObject panel in panelYangDitampilkan)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // 3. Sembunyikan background shadow
        if (backgroundShadow != null)
            backgroundShadow.SetActive(false);

        // 4. Munculkan kembali objek yang perlu dimunculkan
        foreach (GameObject obj in objekYangDimunculkanSaatClose)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    /// <summary>
    /// Mainkan suara klik dengan aman.
    /// </summary>
    void MainkanSuara()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[SimpleTogglePanel] AudioSource belum di-drag!");
            return;
        }

        if (!audioSource.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[SimpleTogglePanel] AudioSource di '{audioSource.gameObject.name}' nonaktif! " +
                             "Pindahkan ke objek yang selalu aktif (misal AudioManager).");
            return;
        }

        AudioClip clip = suaraKlik != null ? suaraKlik : audioSource.clip;
        if (clip == null)
        {
            Debug.LogWarning("[SimpleTogglePanel] Tidak ada clip suara yang dipasang!");
            return;
        }

        audioSource.PlayOneShot(clip, volumeSuara);
        Debug.Log($"[SimpleTogglePanel] Putar suara: {clip.name}");
    }
}