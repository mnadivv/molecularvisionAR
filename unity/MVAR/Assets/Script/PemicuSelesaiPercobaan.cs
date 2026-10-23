using UnityEngine;

/// <summary>
/// Pasang di panel hasil tunggal yang langsung menuntaskan percobaan saat ditampilkan.
/// Untuk halaman kesimpulan berurutan, matikan selesaikanSaatAktif dan panggil
/// ManajerPercobaan.PercobaanSelesai setelah halaman terakhir ditutup.
/// </summary>
public class PemicuSelesaiPercobaan : MonoBehaviour
{
    [Tooltip("Nomor percobaan: 0 = Percobaan1, 1 = Percobaan2.")]
    public int indexPercobaan = 0;

    [Tooltip("Matikan untuk panel Kesimpulan yang merupakan bagian dari rangkaian halaman.")]
    public bool selesaikanSaatAktif = true;

    private void OnEnable()
    {
        if (selesaikanSaatAktif && ManajerPercobaan.Instance != null)
            ManajerPercobaan.Instance.PercobaanSelesai(indexPercobaan);
    }
}
