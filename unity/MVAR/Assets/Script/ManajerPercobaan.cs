using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di satu objek yang selalu aktif (mis. ScriptLogic/ManajerLatihan).
/// - Percobaan berikutnya terkunci sampai percobaan sebelumnya selesai (sampai Kesimpulan tampil).
/// - Saat percobaan selesai: PilihanPercobaan muncul lagi dan percobaan berikutnya terbuka.
/// - Percobaan yang sudah selesai TETAP bisa dimasuki lagi: dimulai ulang dari soal pertama dengan meja bersih
///   (kecuali izinkanUlang dimatikan, maka tombolnya hanya menampilkan Kesimpulan).
/// Dihubungkan ke onClick tombol lewat PilihPercobaan(int).
/// </summary>
public class ManajerPercobaan : MonoBehaviour
{
    public static ManajerPercobaan Instance { get; private set; }

    [System.Serializable]
    public class DataPercobaan
    {
        public string nama;
        [Tooltip("Tombol pilihan (ButtonPercobaan1 / ButtonPercobaan2).")]
        public Button tombolPilihan;
        [Tooltip("Soal pertama percobaan ini (Soal1).")]
        public GameObject soalPertama;
        [Tooltip("Panel Kesimpulan percobaan ini.")]
        public GameObject kesimpulan;
        [Tooltip("Gambar tombol setelah percobaan selesai (tombol kesimpulan). Kosong = gambar tombol tidak berubah.")]
        public Sprite spriteTombolKesimpulan;
        [HideInInspector] public Sprite spriteAwal;
        [HideInInspector] public bool selesai;
    }

    [Header("=== PANEL PILIHAN ===")]
    [Tooltip("Container SlotTabung dan SlotRakTabung milik Percobaan1. Disembunyikan saat pindah ke percobaan lain.")]
    public GameObject[] wadahSlotPercobaan1;

    [Tooltip("Objek PilihanPercobaan.")]
    public GameObject panelPilihan;

    [Header("=== DAFTAR PERCOBAAN (URUT) ===")]
    public List<DataPercobaan> percobaan = new List<DataPercobaan>();

    [Tooltip("Percobaan berikutnya terkunci sampai percobaan sebelumnya selesai.")]
    public bool kunciBerurutan = true;

    [Tooltip("Percobaan yang sudah selesai bisa dimasuki lagi dan diulang dari soal pertama.")]
    public bool izinkanUlang = true;

    private GameObject[] halamanKesimpulanP2 = new GameObject[3];
    private GameObject bayanganKesimpulanP2;
    private Button tombolDataP2;
    private GameObject tombolAlat;
    private GameObject tombolBahan;
    private int halamanAktifKesimpulanP2 = -1;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < percobaan.Count; i++)
        {
            DataPercobaan p = percobaan[i];

        // Hapus state/notifikasi Percobaan 2 saat kembali ke Percobaan 1.
        if (i == 0 && ManajerSpotPlate.Instance != null)
        {
            ManajerSpotPlate.Instance.ResetSemua();
            if (ManajerSpotPlate.Instance.slotPlate != null)
                ManajerSpotPlate.Instance.slotPlate.gameObject.SetActive(false);
        }
            if (p.tombolPilihan == null) continue;
            Image img = p.tombolPilihan.GetComponent<Image>();
            if (img != null) p.spriteAwal = img.sprite;
        }

        SiapkanAlurKesimpulanP2();
    }

    private void Start()
    {
        TutupAlurKesimpulanP2();
        TerapkanStatus();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Apakah percobaan ke-i boleh dibuka.</summary>
    public bool Terbuka(int i)
    {
        if (i < 0 || i >= percobaan.Count) return false;
        if (!kunciBerurutan || i == 0) return true;
        return percobaan[i - 1].selesai;
    }

    /// <summary>Dipanggil dari onClick tombol percobaan (0 = Percobaan1, 1 = Percobaan2).</summary>
    public void PilihPercobaan(int i)
    {
        if (!Terbuka(i)) return;
        DataPercobaan p = percobaan[i];

        TutupAlurKesimpulanP2();

        // Hapus state/notifikasi Percobaan 2 saat kembali ke Percobaan 1.
        if (i == 0 && ManajerSpotPlate.Instance != null)
        {
            ManajerSpotPlate.Instance.ResetSemua();
            if (ManajerSpotPlate.Instance.slotPlate != null)
                ManajerSpotPlate.Instance.slotPlate.gameObject.SetActive(false);
        }

        SembunyikanSemuaKesimpulan();

        if (p.selesai && !izinkanUlang)
        {
            // Mode lama: tombol percobaan yang selesai hanya menampilkan kesimpulan
            if (p.kesimpulan != null) p.kesimpulan.SetActive(true);
            return;
        }

        if (p.selesai) MulaiUlang(p);

        if (panelPilihan != null) panelPilihan.SetActive(false);

        // Tutup soal dari kedua percobaan sebelum membuka percobaan yang dipilih.
        // Ini mencegah marker/notifikasi percobaan lain tertinggal saat berpindah.
        for (int iPercobaan = 0; iPercobaan < percobaan.Count; iPercobaan++)
        {
            Transform indukSoal = percobaan[iPercobaan].soalPertama != null
                ? percobaan[iPercobaan].soalPertama.transform.parent
                : null;
            if (indukSoal == null) continue;
            for (int child = 0; child < indukSoal.childCount; child++)
            {
                GameObject panel = indukSoal.GetChild(child).gameObject;
                if (panel.name.StartsWith("Soal") || panel.name == "Kesimpulan")
                    panel.SetActive(false);
            }
        }
        // Sembunyikan objek 3D Percobaan1 saat masuk ke percobaan lain
        bool bukan0 = i != 0;
        if (wadahSlotPercobaan1 != null)
            for (int w = 0; w < wadahSlotPercobaan1.Length; w++)
                if (wadahSlotPercobaan1[w] != null) wadahSlotPercobaan1[w].SetActive(!bukan0);
        if (p.soalPertama != null) p.soalPertama.SetActive(true);

        // Terapkan penanda seketika berdasarkan soal yang baru dibuka.
        if (ManajerTataLetak.Instance != null)
            ManajerTataLetak.Instance.Terapkan();
    }

    /// <summary>Bersihkan meja (rak, tabung, cairan, gelas kimia) dan tutup semua panel soal percobaan ini.</summary>
    private void MulaiUlang(DataPercobaan p)
    {
        // Tampilkan kembali container slot saat percobaan1 diulang
        if (percobaan.Count > 0 && p == percobaan[0] && wadahSlotPercobaan1 != null)
            for (int w = 0; w < wadahSlotPercobaan1.Length; w++)
                if (wadahSlotPercobaan1[w] != null) wadahSlotPercobaan1[w].SetActive(true);
        if (ManajerPilihItem.Instance != null)
        {
            ManajerPilihItem.Instance.ResetSemuaTahap();
            ManajerPilihItem.Instance.SembunyikanSalah();
        }

        if (p.soalPertama != null && p.soalPertama.transform.parent != null)
        {
            Transform induk = p.soalPertama.transform.parent;
            for (int k = 0; k < induk.childCount; k++)
                induk.GetChild(k).gameObject.SetActive(false);
        }
    }

    /// <summary>Tandai percobaan selesai, bersihkan meja, dan tampilkan menu pilihan.</summary>
    public void PercobaanSelesai(int i)
    {
        if (i < 0 || i >= percobaan.Count) return;
        percobaan[i].selesai = true;
        // wadahSlotPercobaan1 saat selesai: sembunyikan objek 3D P1 supaya tidak menggantung di atas PilihanPercobaan
        if (wadahSlotPercobaan1 != null)
            for (int w = 0; w < wadahSlotPercobaan1.Length; w++)
                if (wadahSlotPercobaan1[w] != null) wadahSlotPercobaan1[w].SetActive(false);
        if (ManajerSlotTabung.Instance != null) ManajerSlotTabung.Instance.Bersihkan();
        if (ManajerTakaran.Instance != null) ManajerTakaran.Instance.ResetSemua();
        if (ManajerSpotPlate.Instance != null) ManajerSpotPlate.Instance.ResetSemua();
        if (panelPilihan != null) panelPilihan.SetActive(true);
        TerapkanStatus();
    }

    private void SiapkanAlurKesimpulanP2()
    {
        if (percobaan.Count < 2 || percobaan[1].soalPertama == null) return;

        Transform akarP2 = percobaan[1].soalPertama.transform.parent;
        if (akarP2 == null) return;

        halamanKesimpulanP2[0] = CariAnak(akarP2, "Kesimpulan1")?.gameObject;
        halamanKesimpulanP2[1] = CariAnak(akarP2, "Kesimpulan2")?.gameObject;
        halamanKesimpulanP2[2] = CariAnak(akarP2, "Kesimpulan3")?.gameObject;
        // Gunakan shadow milik Percobaan2, bukan shadow kecil yang ada di dalam panel soal.
        Transform shadow = CariAnakLangsung(akarP2, "BackgroundShadow")
            ?? CariAnakLangsung(akarP2, "BackgorundShadow");
        bayanganKesimpulanP2 = shadow != null ? shadow.gameObject : null;
        tombolAlat = GameObject.Find("ButtonAlat");
        tombolBahan = GameObject.Find("ButtonBahan");

        Transform soal7 = CariAnak(akarP2, "Soal7");
        if (soal7 != null)
        {
            Button[] tombolDalamSoal7 = soal7.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < tombolDalamSoal7.Length; i++)
            {
                if (tombolDalamSoal7[i].gameObject.name == "ButtonData")
                {
                    tombolDataP2 = tombolDalamSoal7[i];
                    tombolDataP2.onClick.RemoveListener(BukaKesimpulanP2);
                    tombolDataP2.onClick.AddListener(BukaKesimpulanP2);
                    break;
                }
            }
        }

        for (int i = 0; i < halamanKesimpulanP2.Length; i++)
        {
            GameObject halaman = halamanKesimpulanP2[i];
            if (halaman == null) continue;

            Button tombolHalaman = halaman.GetComponent<Button>();
            if (tombolHalaman == null) tombolHalaman = halaman.AddComponent<Button>();
            tombolHalaman.targetGraphic = halaman.GetComponent<Graphic>();
            tombolHalaman.onClick.RemoveListener(LanjutKesimpulanP2);
            tombolHalaman.onClick.AddListener(LanjutKesimpulanP2);
        }

        if (bayanganKesimpulanP2 != null)
        {
            Button tombolBayangan = bayanganKesimpulanP2.GetComponent<Button>();
            if (tombolBayangan == null) tombolBayangan = bayanganKesimpulanP2.AddComponent<Button>();
            tombolBayangan.targetGraphic = bayanganKesimpulanP2.GetComponent<Graphic>();
            tombolBayangan.onClick.RemoveListener(LanjutKesimpulanP2);
            tombolBayangan.onClick.AddListener(LanjutKesimpulanP2);
        }
    }

    private void BukaKesimpulanP2()
    {
        if (halamanKesimpulanP2[0] == null) return;
        halamanAktifKesimpulanP2 = 0;
        TampilkanHalamanKesimpulanP2();
    }

    private void LanjutKesimpulanP2()
    {
        if (halamanAktifKesimpulanP2 < 0) return;

        if (halamanAktifKesimpulanP2 < halamanKesimpulanP2.Length - 1)
        {
            halamanAktifKesimpulanP2++;
            TampilkanHalamanKesimpulanP2();
            return;
        }

        TutupAlurKesimpulanP2();
        PercobaanSelesai(1);

        // Tombol alat dan bahan disembunyikan saat hasil dibuka; munculkan lagi di menu.
        if (tombolAlat != null) tombolAlat.SetActive(true);
        if (tombolBahan != null) tombolBahan.SetActive(true);
        if (panelPilihan != null) panelPilihan.SetActive(true);
        for (int i = 0; i < percobaan.Count; i++)
            if (percobaan[i].tombolPilihan != null)
                percobaan[i].tombolPilihan.gameObject.SetActive(true);
    }

    private void TampilkanHalamanKesimpulanP2()
    {
        if (bayanganKesimpulanP2 != null) bayanganKesimpulanP2.SetActive(true);
        for (int i = 0; i < halamanKesimpulanP2.Length; i++)
            if (halamanKesimpulanP2[i] != null)
                halamanKesimpulanP2[i].SetActive(i == halamanAktifKesimpulanP2);
    }

    private void TutupAlurKesimpulanP2()
    {
        halamanAktifKesimpulanP2 = -1;
        if (bayanganKesimpulanP2 != null) bayanganKesimpulanP2.SetActive(false);
        for (int i = 0; i < halamanKesimpulanP2.Length; i++)
            if (halamanKesimpulanP2[i] != null)
                halamanKesimpulanP2[i].SetActive(false);
    }

    private static Transform CariAnak(Transform induk, string nama)
    {
        if (induk == null) return null;
        for (int i = 0; i < induk.childCount; i++)
        {
            Transform anak = induk.GetChild(i);
            if (anak.name == nama) return anak;
            Transform hasil = CariAnak(anak, nama);
            if (hasil != null) return hasil;
        }
        return null;
    }

    private static Transform CariAnakLangsung(Transform induk, string nama)
    {
        if (induk == null) return null;
        for (int i = 0; i < induk.childCount; i++)
        {
            Transform anak = induk.GetChild(i);
            if (anak.name == nama) return anak;
        }
        return null;
    }

    /// <summary>Kunci ulang semua percobaan (bisa dipanggil dari tombol reset).</summary>
    public void ResetProgres()
    {
        for (int i = 0; i < percobaan.Count; i++)
            percobaan[i].selesai = false;
        TutupAlurKesimpulanP2();
        SembunyikanSemuaKesimpulan();
        TerapkanStatus();
        if (ManajerPilihItem.Instance != null)
            ManajerPilihItem.Instance.ResetSemuaTahap();
    }

    public void TerapkanStatus()
    {
        for (int i = 0; i < percobaan.Count; i++)
        {
            DataPercobaan p = percobaan[i];

        // Hapus state/notifikasi Percobaan 2 saat kembali ke Percobaan 1.
        if (i == 0 && ManajerSpotPlate.Instance != null)
        {
            ManajerSpotPlate.Instance.ResetSemua();
            if (ManajerSpotPlate.Instance.slotPlate != null)
                ManajerSpotPlate.Instance.slotPlate.gameObject.SetActive(false);
        }
            if (p.tombolPilihan == null) continue;

            p.tombolPilihan.interactable = Terbuka(i);

            Image img = p.tombolPilihan.GetComponent<Image>();
            if (img != null)
            {
                bool pakaiKesimpulan = !izinkanUlang && p.selesai && p.spriteTombolKesimpulan != null;
                img.sprite = pakaiKesimpulan ? p.spriteTombolKesimpulan : p.spriteAwal;
            }
        }
    }

    private void SembunyikanSemuaKesimpulan()
    {
        for (int i = 0; i < percobaan.Count; i++)
            if (percobaan[i].kesimpulan != null) percobaan[i].kesimpulan.SetActive(false);
    }
}
