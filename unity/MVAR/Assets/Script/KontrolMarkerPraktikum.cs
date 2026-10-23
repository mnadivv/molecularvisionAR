using UnityEngine;
using UnityEngine.Events;
using Vuforia;
using System.Collections.Generic;

[DefaultExecutionOrder(-1000)]
public class KontrolMarkerPraktikum : MonoBehaviour
{
    public GameObject scrollAlat, scrollBahan, tombolAlat, tombolBahan, awalMulai, pilihanPercobaan;
    public ManajerPilihItem manajerItem;
    public ManajerTakaran manajerTakaran;
    public ManajerSlotTabung[] semuaManajerSlot;
    public ManajerSpotPlate manajerSpotPlate;
    private DefaultObserverEventHandler eventHandler;
    private UnityAction ditemukan, hilang;
    private bool markerSebelumnyaTerlihat;
    private ObserverBehaviour observer;
    private float mulaiHilang = -1f;
    private Dictionary<GameObject, bool> statusTombolSebelumHilang;
    private const float toleransiHilangDetik = 0.35f;

    private void Awake()
    {
        eventHandler = GetComponent<DefaultObserverEventHandler>();
        observer = GetComponent<ObserverBehaviour>();
        ditemukan = SaatMarkerDitemukan;
        hilang = SaatMarkerHilang;
        if (eventHandler != null)
        {
            eventHandler.OnTargetFound.AddListener(ditemukan);
            eventHandler.OnTargetLost.AddListener(hilang);
        }
    }
    private void Start()
    {
        if (scrollAlat == null) scrollAlat = GameObject.Find("ScrollAlat");
        if (scrollBahan == null) scrollBahan = GameObject.Find("ScrollBahan");
        if (tombolAlat == null) tombolAlat = GameObject.Find("ButtonAlat");
        if (tombolBahan == null) tombolBahan = GameObject.Find("ButtonBahan");
        if (manajerItem == null) manajerItem = FindAnyObjectByType<ManajerPilihItem>(FindObjectsInactive.Include);
        if (manajerTakaran == null) manajerTakaran = ManajerTakaran.Instance;
        if (manajerSpotPlate == null) manajerSpotPlate = ManajerSpotPlate.Instance;
        if (semuaManajerSlot == null || semuaManajerSlot.Length == 0)
            semuaManajerSlot = FindObjectsByType<ManajerSlotTabung>(FindObjectsInactive.Include);
        TerapkanStatus(MarkerTerlihat());
    }
    private void Update()
    {
        TerapkanStatus(MarkerTerlihat());
    }
    private void OnDestroy()
    {
        if (eventHandler != null)
        {
            eventHandler.OnTargetFound.RemoveListener(ditemukan);
            eventHandler.OnTargetLost.RemoveListener(hilang);
        }
    }
    private void SaatMarkerDitemukan()
    {
        if (MarkerTerlihat()) TerapkanStatus(true);
    }
    private void SaatMarkerHilang()
    {
        TerapkanStatus(false);
    }
    private bool MarkerTerlihat()
    {
        if (observer == null) return false;
        Status status = observer.TargetStatus.Status;
        return status == Status.TRACKED;
    }


    private void TerapkanStatus(bool terlihat)
    {
        if (terlihat)
        {
            mulaiHilang = -1f;
            if (!markerSebelumnyaTerlihat)
                TampilkanAugmentasi();

            markerSebelumnyaTerlihat = true;
            return;
        }

        // Tunggu sebentar setelah marker lepas agar tombol tidak berkedip saat tracking pulih.
        if (markerSebelumnyaTerlihat)
        {
            if (mulaiHilang < 0f) mulaiHilang = Time.unscaledTime;
            if (Time.unscaledTime - mulaiHilang < toleransiHilangDetik) return;
            markerSebelumnyaTerlihat = false;
        }

        // Tetap paksa tombol tersembunyi jika marker belum ada, termasuk bila UI lain
        // sempat mengaktifkannya lagi.
        SembunyikanAugmentasi();
    }

    private void SembunyikanAugmentasi()
    {
        if (statusTombolSebelumHilang == null)
            statusTombolSebelumHilang = new Dictionary<GameObject, bool>();
        SimpanDanSembunyikanTombol(tombolAlat);
        SimpanDanSembunyikanTombol(tombolBahan);
    }
    private void SimpanDanSembunyikanTombol(GameObject tombol)
    {
        if (tombol == null || statusTombolSebelumHilang.ContainsKey(tombol)) return;
        statusTombolSebelumHilang[tombol] = tombol.activeSelf;
        tombol.SetActive(false);
    }

    private void TampilkanAugmentasi()
    {
        if (statusTombolSebelumHilang != null)
            foreach (KeyValuePair<GameObject, bool> item in statusTombolSebelumHilang)
                if (item.Key != null) item.Key.SetActive(item.Value);
        statusTombolSebelumHilang = null;

        // Tombol alat/bahan tersedia setiap kali marker sedang terlacak.
        if (tombolAlat != null) tombolAlat.SetActive(true);
        if (tombolBahan != null) tombolBahan.SetActive(true);
    }    private Transform AmbilPanelPercobaan2()
    {
        Transform menuSoal = awalMulai != null ? awalMulai.transform.parent : null;
        return menuSoal != null ? menuSoal.Find("Percobaan2") : null;
    }
    private bool Percobaan2SedangAktif()
    {
        Transform p2 = AmbilPanelPercobaan2();
        if (p2 == null) return false;
        for (int i = 0; i < p2.childCount; i++)
        {
            Transform panel = p2.GetChild(i);
            if ((panel.name.StartsWith("Soal") || panel.name == "Kesimpulan") && panel.gameObject.activeInHierarchy) return true;
        }
        return false;
    }
    private void ResetSemuaSoal(bool pertahankanPercobaan2 = false)
    {
        Transform p2 = pertahankanPercobaan2 ? AmbilPanelPercobaan2() : null;
        if (manajerItem != null) { if (p2 != null) manajerItem.ResetTahapDiLuar(p2); else manajerItem.ResetSemuaTahap(); manajerItem.SembunyikanSalah(); }
        if (manajerTakaran != null && (p2 != null || manajerItem == null)) manajerTakaran.ResetSemua(p2 != null);
        if (semuaManajerSlot != null)
            for (int i = 0; i < semuaManajerSlot.Length; i++)
                if (semuaManajerSlot[i] != null) semuaManajerSlot[i].Bersihkan();
        if (manajerSpotPlate != null && p2 == null) manajerSpotPlate.ResetSemua();
        Transform menuSoal = awalMulai != null ? awalMulai.transform.parent : null;
        if (menuSoal != null)
        {
            ResetPanelSoal(menuSoal.Find("Percobaan1"));
            if (p2 == null) ResetPanelSoal(menuSoal.Find("Percobaan2"));
        }
        // Panel pilihan dikelola oleh alur UI, jangan timpa saat status marker berubah.
    }
    private static void ResetPanelSoal(Transform percobaan)
    {
        if (percobaan == null) return;
        for (int i = 0; i < percobaan.childCount; i++)
        {
            GameObject panel = percobaan.GetChild(i).gameObject;
            if (panel.name.StartsWith("Soal") || panel.name == "Kesimpulan")
            {
                if (panel.name.StartsWith("Soal"))
                    foreach (Transform child in panel.GetComponentsInChildren<Transform>(true))
                        if (child.name == "PilihanAkuades") child.gameObject.SetActive(false);
                panel.SetActive(false);
            }
        }
    }
}


