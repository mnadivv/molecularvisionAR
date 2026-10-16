using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di ButtonLanjut Percobaan2. Syarat lanjut berdasarkan fase ManajerSpotPlate.
/// Hubungkan onClick ke method Lanjut() di inspector.
/// </summary>
public class GerbangSpotPlate : MonoBehaviour
{
    public enum Syarat { PlateSiap, Kolom1, Kolom2, Kolom3, Baris1, Baris2, Baris3 }
    public Syarat syarat;
    public GameObject aktifkan;
    public GameObject nonaktifkan;

    private Button btn;
    private void Awake() { btn = GetComponent<Button>(); }

    private void Update()
    {
        if (btn == null) return;
        var m = ManajerSpotPlate.Instance;
        btn.interactable = m != null && Cek(m);
    }

    bool Cek(ManajerSpotPlate m)
    {
        switch (syarat)
        {
            case Syarat.PlateSiap: return m.PlateSudahAda();
            case Syarat.Kolom1:    return m.Kolom1Selesai();
            case Syarat.Kolom2:    return m.Kolom2Selesai();
            case Syarat.Kolom3:    return m.Kolom3Selesai();
            case Syarat.Baris1:    return m.Baris1Selesai();
            case Syarat.Baris2:    return m.Baris2Selesai();
            default:               return m.Baris3Selesai();
        }
    }

    public void Lanjut()
    {
        var m = ManajerSpotPlate.Instance;
        if (m == null || !Cek(m)) return;
        switch (syarat)
        {
            case Syarat.PlateSiap: m.AktifkanFase(ManajerSpotPlate.FaseP2.IsiKolom1); break;
            case Syarat.Kolom1:    m.AktifkanFase(ManajerSpotPlate.FaseP2.IsiKolom2); break;
            case Syarat.Kolom2:    m.AktifkanFase(ManajerSpotPlate.FaseP2.IsiKolom3); break;
            case Syarat.Kolom3:    m.AktifkanFase(ManajerSpotPlate.FaseP2.CampurBaris1); break;
            case Syarat.Baris1:    m.AktifkanFase(ManajerSpotPlate.FaseP2.CampurBaris2); break;
            case Syarat.Baris2:    m.AktifkanFase(ManajerSpotPlate.FaseP2.CampurBaris3); break;
        }
        if (aktifkan != null) aktifkan.SetActive(true);
        if (nonaktifkan != null) nonaktifkan.SetActive(false);
    }
}
