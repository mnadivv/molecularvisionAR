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
        if (manajerItem == null) manajerItem = FindFirstObjectByType<ManajerPilihItem>(FindObjectsInactive.Include);
        if (manajerTakaran == null) manajerTakaran = ManajerTakaran.Instance;
        if (manajerSpotPlate == null) manajerSpotPlate = ManajerSpotPlate.Instance;
        if (semuaManajerSlot == null || semuaManajerSlot.Length == 0)
            semuaManajerSlot = FindObjectsByType<ManajerSlotTabung>(FindObjectsInactive.Include);
        bool markerValid = observer != null && observer.TargetStatus.Status == Status.TRACKED;
        if (!markerValid) ResetSemuaSoal();
        TerapkanStatus(markerValid);
    }
    private void Update()
    {
        TerapkanStatus(observer != null && observer.TargetStatus.Status == Status.TRACKED);
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
        if (observer != null && observer.TargetStatus.Status == Status.TRACKED) TerapkanStatus(true);
    }
    private void SaatMarkerHilang()
    {
        ResetSemuaSoal();
        TerapkanStatus(false);
    }
    private void TerapkanStatus(bool terlihat)
    {
        if (terlihat)
        {
            if (!markerSebelumnyaTerlihat)
            {
                if (tombolAlat != null) tombolAlat.SetActive(true);
                if (tombolBahan != null) tombolBahan.SetActive(true);
                if (scrollAlat != null) scrollAlat.SetActive(false);
                if (scrollBahan != null) scrollBahan.SetActive(false);
                if (awalMulai != null) awalMulai.SetActive(true);
            }
            markerSebelumnyaTerlihat = true;
            return;
        }
        if (markerSebelumnyaTerlihat) ResetSemuaSoal();
        if (tombolAlat != null) tombolAlat.SetActive(false);
        if (tombolBahan != null) tombolBahan.SetActive(false);
        if (scrollAlat != null) scrollAlat.SetActive(false);
        if (scrollBahan != null) scrollBahan.SetActive(false);
        if (awalMulai != null) awalMulai.SetActive(false);
        if (pilihanPercobaan != null) pilihanPercobaan.SetActive(false);
        markerSebelumnyaTerlihat = false;
    }
    private Transform AmbilPanelPercobaan2()
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
        if (pilihanPercobaan != null) pilihanPercobaan.SetActive(false);
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