using UnityEngine;
using UnityEngine.UI;

public class ImageQuizManager : MonoBehaviour
{
    [Header("=== GAMBAR SALAH ===")]
    [Tooltip("Isi jumlah gambar salah. Slot akan muncul otomatis.")]
    public int jumlahGambarSalah = 3;
    [Tooltip("Drag gambar salah ke sini")]
    public GameObject[] gambarSalah;

    [Header("=== PESAN SALAH ===")]
    [Tooltip("Drag 1 gambar pop-up pesan salah ke sini.")]
    public GameObject gambarPesanSalah;

    [Header("=== GAMBAR BENAR ===")]
    [Tooltip("Isi jumlah gambar benar. Slot akan muncul otomatis.")]
    public int jumlahGambarBenar = 2;
    [Tooltip("Drag gambar benar ke sini")]
    public GameObject[] gambarBenar;

    [Header("=== TOMBOL LANJUT ===")]
    public Button tombolLanjut;

    [Header("=== AKSI SAAT LANJUT DIKLIK ===")]
    public GameObject[] gambarYangDisembunyikan;
    public GameObject[] gambarYangDitampilkan;

    // Internal
    private int counterBenar = 0;
    private bool selesai = false;

    void Start()
    {
        if (gambarPesanSalah != null)
            gambarPesanSalah.SetActive(false);

        if (tombolLanjut != null)
            tombolLanjut.interactable = false;

        SetupGambarSalah();
        SetupGambarBenar();
    }

    void SetupGambarSalah()
    {
        foreach (GameObject g in gambarSalah)
        {
            if (g == null) continue;
            Button btn = g.GetComponent<Button>();
            if (btn == null) btn = g.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(KlikGambarSalah);
        }
    }

    void SetupGambarBenar()
    {
        foreach (GameObject g in gambarBenar)
        {
            if (g == null) continue;
            Button btn = g.GetComponent<Button>();
            if (btn == null) btn = g.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(KlikGambarBenar);
        }
    }

    public void KlikGambarSalah()
    {
        if (selesai) return;

        if (gambarPesanSalah != null)
        {
            gambarPesanSalah.SetActive(true);
            CancelInvoke("SembunyikanPesanSalah");
            Invoke("SembunyikanPesanSalah", 2f);
        }
    }

    void SembunyikanPesanSalah()
    {
        if (gambarPesanSalah != null)
            gambarPesanSalah.SetActive(false);
    }

    public void KlikGambarBenar()
    {
        if (selesai) return;

        counterBenar++;
        Debug.Log("Gambar benar diklik: " + counterBenar + "/" + gambarBenar.Length);

        if (counterBenar >= gambarBenar.Length)
        {
            selesai = true;
            if (tombolLanjut != null)
                tombolLanjut.interactable = true;
        }
    }

    public void KlikTombolLanjut()
    {
        if (!selesai) return;

        foreach (GameObject g in gambarYangDisembunyikan)
            if (g != null) g.SetActive(false);

        foreach (GameObject g in gambarYangDitampilkan)
            if (g != null) g.SetActive(true);
    }
}