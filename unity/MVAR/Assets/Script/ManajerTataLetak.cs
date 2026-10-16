using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pasang di satu objek yang selalu aktif (mis. ScriptLogic/ManajerLatihan).
/// Menampilkan titik penempatan (tata letak) HANYA selama soal yang bersangkutan tampil.
/// Contoh: titik rak tampil di langkah Ambil Rak Tabung Reaksi,
/// titik tabung A dan B tampil di langkah Ambil 2 tabung reaksi.
/// Model 3D yang sudah ditaruh tidak ikut hilang, hanya penanda titiknya.
/// </summary>
public class ManajerTataLetak : MonoBehaviour
{
    public static ManajerTataLetak Instance { get; private set; }
    [System.Serializable]
    public class AturanTampil
    {
        public string nama;
        [Tooltip("Objek penanda yang ditampilkan (mis. SlotA, SlotB, SlotRak).")]
        public List<GameObject> objek = new List<GameObject>();
        [Tooltip("Objek di atas tampil hanya selama salah satu panel soal ini tampil.")]
        public List<GameObject> panelSoal = new List<GameObject>();
    }

    public List<AturanTampil> aturan = new List<AturanTampil>();

    private void Awake()
    {
        Instance = this;
        Terapkan();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        Terapkan();
    }

    public void Terapkan()
    {
        for (int i = 0; i < aturan.Count; i++)
        {
            AturanTampil a = aturan[i];
            bool tampil = false;
            for (int k = 0; k < a.panelSoal.Count; k++)
            {
                GameObject p = a.panelSoal[k];
                if (p != null && p.activeInHierarchy) { tampil = true; break; }
            }
            for (int k = 0; k < a.objek.Count; k++)
            {
                GameObject o = a.objek[k];
                if (o != null && o.activeSelf != tampil) o.SetActive(tampil);
            }
        }
    }
}
