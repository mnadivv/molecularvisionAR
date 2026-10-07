using UnityEngine;

/// <summary>
/// Pasang di objek SlotTabung (induk SlotA dan SlotB).
/// Mengatur ke slot mana tabung ditaruh saat klik tahan dilepas.
/// Urutan isi: tabung pertama ke slot pertama (A), tabung kedua ke slot kedua (B).
/// </summary>
public class ManajerSlotTabung : MonoBehaviour
{
    public static ManajerSlotTabung Instance { get; private set; }

    [Tooltip("Daftar slot berurutan: A lalu B")]
    public SlotTabung[] slot;
    [Tooltip("Area cadangan (TargetDrop). Drop di sini tapi bukan tepat di slot = masuk ke slot kosong berikutnya")]
    public RectTransform areaCadangan;

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    private SlotTabung SlotKosongPertama()
    {
        if (slot == null) return null;
        for (int i = 0; i < slot.Length; i++)
            if (slot[i] != null && !slot[i].Terisi) return slot[i];
        return null;
    }

    /// <summary>Dipanggil saat tabung dilepas. True = tabung ditaruh di slot (jangan dihapus).</summary>
    public bool CobaTaruh(Vector2 posisiLayar, GameObject model, Camera cam, float kedalaman)
    {
        if (slot == null || slot.Length == 0 || model == null || cam == null) return false;

        SlotTabung target = null;
        for (int i = 0; i < slot.Length; i++)
        {
            if (slot[i] != null && !slot[i].Terisi && slot[i].Mengandung(posisiLayar, cam))
            {
                target = slot[i];
                break;
            }
        }

        if (target == null && areaCadangan != null && slot[0] != null &&
            RectTransformUtility.RectangleContainsScreenPoint(areaCadangan, posisiLayar, slot[0].KameraUI(cam)))
        {
            target = SlotKosongPertama();
        }

        if (target == null) return false;
        target.Isi(model, cam, kedalaman);
        return true;
    }

    /// <summary>Beri efek sorot pada slot yang sedang ditunjuk saat menggeser tabung.</summary>
    public void Sorot(Vector2 posisiLayar, bool aktif, Camera cam)
    {
        if (slot == null) return;
        for (int i = 0; i < slot.Length; i++)
            if (slot[i] != null) slot[i].SetSorot(aktif && slot[i].Mengandung(posisiLayar, cam));
    }

    /// <summary>Kosongkan semua slot (bisa dipanggil dari tombol reset).</summary>
    public void Bersihkan()
    {
        if (slot == null) return;
        for (int i = 0; i < slot.Length; i++)
            if (slot[i] != null) slot[i].Kosongkan();
    }
}
