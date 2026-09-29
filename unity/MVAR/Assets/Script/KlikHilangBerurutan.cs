using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Script: Klik komponen → hilang, komponen berikutnya muncul.
/// Drop ke parent "Content", lalu drag anak-anak komponen ke list.
/// </summary>
public class KlikHilangBerurutan : MonoBehaviour
{
    [Header("=== DAFTAR KOMPONEN ===")]
    [Tooltip("Isi dengan anak-anak komponen (Image / Card). Urut dari atas ke bawah.")]
    public List<GameObject> daftarKomponen = new List<GameObject>();

    [Header("=== PENGATURAN HILANG ===")]
    [Tooltip("Berapa komponen yang hilang setiap kali diklik.")]
    [Range(1, 10)]
    public int jumlahHilangPerKlik = 1;

    [Tooltip("Kalau true, komponen yang hilang akan di-DISABLE (bisa dimunculkan lagi).\n" +
             "Kalau false, komponen akan di-DESTROY (hilang permanen).")]
    public bool disableSajaTidakDestroy = true;

    [Header("=== EFEK SUARA ===")]
    [Tooltip("AudioSource untuk memutar suara klik.")]
    public AudioSource audioSource;

    [Tooltip("Clip suara klik. Kalau kosong, pakai audioSource.clip.")]
    public AudioClip suaraKlik;

    [Header("=== EFEK VISUAL (opsional) ===")]
    [Tooltip("Durasi animasi fade-out (detik). 0 = tanpa animasi.")]
    [Range(0f, 1f)]
    public float durasiFade = 0.15f;

    // === Variabel internal ===
    private int indexSekarang = 0;
    private bool sedangProses = false;

    void Start()
    {
        // Sembunyikan semua komponen kecuali yang pertama
        for (int i = 0; i < daftarKomponen.Count; i++)
        {
            if (daftarKomponen[i] == null) continue;

            bool aktif = (i == 0);
            daftarKomponen[i].SetActive(aktif);

            // Tambahkan Button component otomatis kalau belum ada
            Button btn = daftarKomponen[i].GetComponent<Button>();
            if (btn == null)
                btn = daftarKomponen[i].AddComponent<Button>();

            // Tambahkan EventTrigger supaya klik di area mana saja terdeteksi
            btn.onClick.RemoveAllListeners();
            int capturedIndex = i; // capture untuk closure
            btn.onClick.AddListener(() => OnKomponenDiklik(capturedIndex));
        }

        // Pastikan index mulai dari 0
        indexSekarang = 0;

        // Kalau audioSource belum di-set, cari otomatis
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Dipanggil saat salah satu komponen diklik.
    /// </summary>
    void OnKomponenDiklik(int indexYangDiklik)
    {
        // Cegah double-click saat animasi berjalan
        if (sedangProses) return;

        // Cegah klik komponen yang bukan urutannya
        if (indexYangDiklik != indexSekarang) return;

        // Cegah klik kalau sudah di akhir
        if (indexSekarang >= daftarKomponen.Count) return;

        // Mainkan suara klik
        MainkanSuaraKlik();

        // Mulai proses menghilang
        StartCoroutine(ProsesHilang());
    }

    /// <summary>
    /// Proses menghilangkan komponen satu per satu.
    /// </summary>
    System.Collections.IEnumerator ProsesHilang()
    {
        sedangProses = true;

        int sisa = jumlahHilangPerKlik;

        for (int i = 0; i < jumlahHilangPerKlik; i++)
        {
            // Kalau index sudah melewati daftar, berhenti
            if (indexSekarang >= daftarKomponen.Count) break;

            GameObject target = daftarKomponen[indexSekarang];
            if (target == null) { indexSekarang++; continue; }

            // Animasi fade-out (opsional)
            if (durasiFade > 0f)
            {
                CanvasGroup cg = target.GetComponent<CanvasGroup>();
                if (cg == null) cg = target.AddComponent<CanvasGroup>();

                float t = 0f;
                float alphaAwal = cg.alpha;
                while (t < durasiFade)
                {
                    t += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(alphaAwal, 0f, t / durasiFade);
                    yield return null;
                }
                cg.alpha = 1f; // reset setelah disable
            }

            // Hilangkan komponen
            if (disableSajaTidakDestroy)
                target.SetActive(false);
            else
                Destroy(target);

            indexSekarang++;
            sisa--;

            // Munculkan komponen selanjutnya
            if (indexSekarang < daftarKomponen.Count)
            {
                GameObject berikutnya = daftarKomponen[indexSekarang];
                if (berikutnya != null)
                    berikutnya.SetActive(true);
            }

            // Jeda kecil antar komponen (biar terasa natural)
            yield return new WaitForSeconds(0.05f);
        }

        sedangProses = false;

        // Kalau sudah semua habis, panggil event (bisa di-override)
        if (indexSekarang >= daftarKomponen.Count)
        {
            Debug.Log("✅ Semua komponen sudah di-klik dan hilang!");
        }
    }

    /// <summary>
    /// Mainkan suara klik.
    /// </summary>
    void MainkanSuaraKlik()
    {
        if (audioSource == null) return;

        if (suaraKlik != null)
            audioSource.PlayOneShot(suaraKlik);
        else if (audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }

    /// <summary>
    /// Fungsi publik untuk reset (kalau perlu dimainkan ulang).
    /// </summary>
    public void ResetSemua()
    {
        indexSekarang = 0;
        for (int i = 0; i < daftarKomponen.Count; i++)
        {
            if (daftarKomponen[i] != null)
                daftarKomponen[i].SetActive(i == 0);
        }
    }
}