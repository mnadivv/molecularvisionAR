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
    }

    private void Start()
    {
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

    /// <summary>Dipanggil otomatis saat panel Kesimpulan percobaan ke-i tampil.</summary>
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

    /// <summary>Kunci ulang semua percobaan (bisa dipanggil dari tombol reset).</summary>
    public void ResetProgres()
    {
        for (int i = 0; i < percobaan.Count; i++)
            percobaan[i].selesai = false;
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
