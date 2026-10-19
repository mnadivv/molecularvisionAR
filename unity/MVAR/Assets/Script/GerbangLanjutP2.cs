using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di setiap ButtonLanjut Percobaan2.
/// Mengunci tombol sampai kondisi yang sesuai soalnya terpenuhi di ManajerSpotPlate.
/// </summary>
[RequireComponent(typeof(Button))]
public class GerbangLanjutP2 : MonoBehaviour
{
    public enum KondisiSoal { PlateSiap, Kolom1, Kolom2, Kolom3, Baris1, Baris2, Baris3 }
    public KondisiSoal kondisi;

    private Button tombol;

    private void Awake() { tombol = GetComponent<Button>(); }
    private void OnEnable() { Terapkan(); }
    private void Update() { Terapkan(); }

    private void Terapkan()
    {
        if (tombol == null) return;
        var m = ManajerSpotPlate.Instance;
        bool buka = m == null || Cek(m);
        if (tombol.interactable != buka) tombol.interactable = buka;
    }

    private bool Cek(ManajerSpotPlate m)
    {
        switch (kondisi)
        {
            case KondisiSoal.PlateSiap: return m.PlateSudahAda();
            case KondisiSoal.Kolom1:    return m.Kolom1Selesai();
            case KondisiSoal.Kolom2:    return m.Kolom2Selesai();
            case KondisiSoal.Kolom3:    return m.Kolom3Selesai();
            case KondisiSoal.Baris1:    return m.Baris1Selesai();
            case KondisiSoal.Baris2:    return m.Baris2Selesai();
            case KondisiSoal.Baris3:    return m.Baris3Selesai();
        }
        return true;
    }
}
