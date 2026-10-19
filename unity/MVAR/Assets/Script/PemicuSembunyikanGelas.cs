using UnityEngine;

/// <summary>
/// Pasang di panel Kesimpulan. Begitu panel ini aktif (tombol Data diklik dan tugas Soal6 beres),
/// gelas kimia hasil reaksi disembunyikan karena sudah digantikan oleh Kesimpulan/Percobaan1 selesai.
/// </summary>
public class PemicuSembunyikanGelas : MonoBehaviour
{
    private void OnEnable()
    {
        if (ManajerTakaran.Instance != null) ManajerTakaran.Instance.SembunyikanGelasReaksi();
    }
}
