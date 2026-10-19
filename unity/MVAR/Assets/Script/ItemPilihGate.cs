using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pasang di setiap kartu alat/bahan (ScrollAlat dan ScrollBahan).
/// - Kalau kartu ini BUKAN jawaban soal yang sedang tampil, menahan (atau mengetuk singkat) kartu
///   memunculkan gambar salah lewat ManajerPilihItem.
/// - Kalau kartu ini ADALAH jawaban soal yang sedang tampil, menahannya sampai waktuTahan menandai
///   soal itu sudah dikerjakan (ManajerPilihItem.TandaiDipakai), yang membuka ButtonLanjut soal itu.
/// Menggeser daftar untuk scroll tidak dianggap memilih (berlaku untuk kedua kasus di atas).
/// Kartu yang benar tidak diapa-apakan selain itu: logika yang sudah ada (mis. CardTahanGeser3D) tetap berjalan.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ItemPilihGate : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Lama menahan (detik) sebelum dianggap mengambil. Samakan dengan Waktu Tahan di CardTahanGeser3D.")]
    public float waktuTahan = 0.25f;
    [Tooltip("Batas geser daftar (unit canvas). Lebih dari ini dianggap scroll, bukan memilih.")]
    public float toleransiScroll = 6f;
    [Tooltip("Ketukan singkat pada kartu yang salah juga memunculkan gambar salah.")]
    public bool tapSingkatJugaSalah = true;

    private ScrollRect scrollRect;

    private bool memantauSalah;
    private bool sudahLaporSalah;
    private bool memantauBenar;
    private bool sudahLaporBenar;

    private float waktuMulai;
    private Vector2 posisiAwalKonten;

    private void Awake()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
    }

    private void OnDisable()
    {
        memantauSalah = false;
        memantauBenar = false;
    }

    public void OnPointerDown(PointerEventData e)
    {
        ManajerPilihItem m = ManajerPilihItem.Instance;
        ManajerSpotPlate p2 = ManajerSpotPlate.Instance;
        bool plateUntukP2 = p2 != null && p2.BolehAmbilKartuPlateTetes(gameObject);
        bool boleh = plateUntukP2 || (p2 != null && p2.AturanKartuSoal2Aktif
            ? p2.BolehAmbilKartuSoal2(gameObject)
            : m != null && m.BolehDipakai(gameObject));

        memantauSalah = m != null && !boleh;
        sudahLaporSalah = false;

        // Penempatan plate P2 disahkan oleh DropPlate, jangan tandai melalui kuis P1.
        memantauBenar = boleh && !plateUntukP2;
        sudahLaporBenar = false;

        waktuMulai = Time.unscaledTime;
        posisiAwalKonten = PosisiKonten();
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (memantauSalah && !sudahLaporSalah && tapSingkatJugaSalah && !SedangScroll())
            LaporSalah();
        memantauSalah = false;
        memantauBenar = false;
    }

    private void Update()
    {
        if (memantauSalah && !sudahLaporSalah)
        {
            if (SedangScroll())
                memantauSalah = false;
            else if (Time.unscaledTime - waktuMulai >= waktuTahan)
                LaporSalah();
        }

        if (memantauBenar && !sudahLaporBenar)
        {
            if (SedangScroll())
                memantauBenar = false;
            else if (Time.unscaledTime - waktuMulai >= waktuTahan)
                LaporBenar();
        }
    }

    private void LaporSalah()
    {
        sudahLaporSalah = true;
        if (ManajerPilihItem.Instance != null)
            ManajerPilihItem.Instance.TampilkanSalah();
    }

    private void LaporBenar()
    {
        sudahLaporBenar = true;
        if (ManajerPilihItem.Instance != null)
            ManajerPilihItem.Instance.TandaiDipakai(gameObject);
    }

    private Vector2 PosisiKonten()
    {
        if (scrollRect != null && scrollRect.content != null)
            return scrollRect.content.anchoredPosition;
        return Vector2.zero;
    }

    private bool SedangScroll()
    {
        return (PosisiKonten() - posisiAwalKonten).sqrMagnitude > toleransiScroll * toleransiScroll;
    }
}
