using UnityEngine;

/// <summary>
/// Pasang di panel Kesimpulan tiap percobaan.
/// Begitu panel ini tampil, percobaan dianggap selesai: PilihanPercobaan muncul
/// dan percobaan berikutnya terbuka (diatur oleh ManajerPercobaan).
/// </summary>
public class PemicuSelesaiPercobaan : MonoBehaviour
{
    [Tooltip("Nomor percobaan: 0 = Percobaan1, 1 = Percobaan2.")]
    public int indexPercobaan = 0;

    private void OnEnable()
    {
        if (ManajerPercobaan.Instance != null)
            ManajerPercobaan.Instance.PercobaanSelesai(indexPercobaan);
    }
}
