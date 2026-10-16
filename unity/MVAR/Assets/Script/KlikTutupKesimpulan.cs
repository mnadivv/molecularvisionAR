using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pasang di panel Kesimpulan (yang punya Button). Mengklik panel Kesimpulan sama dengan mengklik latar gelapnya
/// (BackgorundShadow): Kesimpulan tertutup, tombol Percobaan 1 dan Percobaan 2 (yang sudah terbuka) muncul.
/// </summary>
[RequireComponent(typeof(Button))]
public class KlikTutupKesimpulan : MonoBehaviour
{
    [Tooltip("Tombol latar gelap (BackgorundShadow) di percobaan yang sama. Kosong = Kesimpulan cuma disembunyikan.")]
    public Button tombolLatar;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Tutup);
    }

    private void Tutup()
    {
        if (tombolLatar != null) tombolLatar.onClick.Invoke();
        else gameObject.SetActive(false);
    }
}
