using UnityEngine;

/// <summary>
/// Pasang di panel Soal6 (observasi). Begitu panel ini aktif, langsung memunculkan gelas kimia
/// dan menuangkan isi tabung A dan B (apa pun hasilnya) ke gelas yang sama, menjadi larutan
/// putih keabu-abuan (PbCl2). Tidak mengunci apa pun; ButtonData tetap mengikuti GerbangLanjut biasa.
/// </summary>
public class PemicuTuangGelasSoal6 : MonoBehaviour
{
    public ManajerTakaran manajer;
    public SlotTabung tabungA;
    public SlotTabung tabungB;
    public Camera kamera;

    private void OnEnable()
    {
        ManajerTakaran m = manajer != null ? manajer : ManajerTakaran.Instance;
        if (m != null) m.MulaiTuangDuaTabung(tabungA, tabungB, kamera != null ? kamera : Camera.main);
    }
}
