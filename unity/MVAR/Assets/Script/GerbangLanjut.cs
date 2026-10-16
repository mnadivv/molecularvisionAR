using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang otomatis di setiap ButtonLanjut (langsung anak dari panel Soal-nya).
/// Mengunci tombol (interactable = false) selama soal yang bersangkutan belum dikerjakan
/// (ManajerPilihItem.TahapSelesai). Begitu item jawaban soal itu sudah diambil, tombol terbuka.
/// Soal yang belum dikonfigurasi di ManajerPilihItem (itemBenar kosong) tidak dikunci.
/// </summary>
[RequireComponent(typeof(Button))]
public class GerbangLanjut : MonoBehaviour
{
    private Button tombol;
    private GameObject panelSoal;

    private void Awake()
    {
        tombol = GetComponent<Button>();
        panelSoal = transform.parent != null ? transform.parent.gameObject : null;
    }

    private void OnEnable()
    {
        Terapkan();
    }

    private void Update()
    {
        Terapkan();
    }

    private void Terapkan()
    {
        if (tombol == null || panelSoal == null) return;
        bool selesai = ManajerPilihItem.Instance == null || ManajerPilihItem.Instance.TahapSelesai(panelSoal);
        if (tombol.interactable != selesai)
            tombol.interactable = selesai;
    }
}
