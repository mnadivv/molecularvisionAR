using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(1000)]
public class RotasiObjekSoal : MonoBehaviour
{
    private sealed class Grup
    {
        public Quaternion rotasi = Quaternion.identity;
        public int frameTerakhir = -1;
        public Vector3 pusat;
        public bool pusatTersimpan;
        public readonly List<RotasiObjekSoal> akar = new List<RotasiObjekSoal>();
    }
    private static readonly List<RotasiObjekSoal> aktif = new List<RotasiObjekSoal>();
    private static readonly Dictionary<GameObject, Grup> grupPerPanel = new Dictionary<GameObject, Grup>();
    private static RotasiObjekSoal dipilih;
    private static Vector2 posisiInput;
    private static int idInput = -999;
    private static int frameInput = -1;

    public GameObject panelSoal;
    public float sensitivitas = 0.45f;
    private Renderer[] renderers;
    private Vector3 posisiAwal;
    private Quaternion rotasiAwal;
    private bool siap;

    private void OnEnable()
    {
        if (!aktif.Contains(this)) aktif.Add(this);
        renderers = GetComponentsInChildren<Renderer>(true);
    }
    private void OnDisable()
    {
        aktif.Remove(this);
        if (dipilih == this) dipilih = null;
    }
    public void AturPanel(GameObject panel) { panelSoal = panel; }

    private void Update()
    {
        if (!siap)
        {
            if (GetComponent<TabungMasukRak>() != null) return;
            posisiAwal = transform.position;
            rotasiAwal = transform.rotation;
            siap = true;
        }
        if (panelSoal != null && !panelSoal.activeInHierarchy)
        {
            ResetPanel(panelSoal);
            return;
        }
        ProsesInput();
    }

    private void LateUpdate()
    {
        if (!siap || panelSoal == null || !panelSoal.activeInHierarchy) return;
        Grup grup = AmbilGrup(panelSoal);
        if (grup.frameTerakhir == Time.frameCount) return;
        grup.frameTerakhir = Time.frameCount;
        grup.akar.Clear();

        for (int i = aktif.Count - 1; i >= 0; i--)
        {
            RotasiObjekSoal item = aktif[i];
            if (item == null) { aktif.RemoveAt(i); continue; }
            if (!item.isActiveAndEnabled || !item.siap || item.panelSoal != panelSoal ||
                !item.panelSoal.activeInHierarchy || item.GetComponent<TabungMasukRak>() != null ||
                item.PunyaIndukDalamGrup(panelSoal)) continue;
            grup.akar.Add(item);
        }
        if (grup.akar.Count == 0) return;

        if (!grup.pusatTersimpan || Quaternion.Angle(grup.rotasi, Quaternion.identity) < 0.01f)
        {
            Bounds gabungan = new Bounds(grup.akar[0].transform.position, Vector3.zero);
            bool adaRenderer = false;
            for (int i = 0; i < grup.akar.Count; i++)
            {
                RotasiObjekSoal item = grup.akar[i];
                if (item.GetComponent<TabungTertempel>() != null)
                {
                    item.posisiAwal = item.transform.position;
                    item.rotasiAwal = item.transform.rotation;
                }
                foreach (Renderer r in item.GetComponentsInChildren<Renderer>(true))
                {
                    if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                    if (!adaRenderer) { gabungan = r.bounds; adaRenderer = true; }
                    else gabungan.Encapsulate(r.bounds);
                }
            }
            if (adaRenderer) grup.pusat = gabungan.center;
            else
            {
                grup.pusat = Vector3.zero;
                for (int i = 0; i < grup.akar.Count; i++) grup.pusat += grup.akar[i].posisiAwal;
                grup.pusat /= grup.akar.Count;
            }
            grup.pusatTersimpan = true;
        }
        else
        {
            // Screen anchored models restore their anchor pose each frame before this script runs.
            for (int i = 0; i < grup.akar.Count; i++)
            {
                RotasiObjekSoal item = grup.akar[i];
                if (item.GetComponent<TabungTertempel>() != null)
                {
                    item.posisiAwal = item.transform.position;
                    item.rotasiAwal = item.transform.rotation;
                }
            }
        }

        for (int i = 0; i < grup.akar.Count; i++)
        {
            RotasiObjekSoal item = grup.akar[i];
            item.transform.position = grup.pusat + grup.rotasi * (item.posisiAwal - grup.pusat);
            item.transform.rotation = grup.rotasi * item.rotasiAwal;
        }
    }

    private bool PunyaIndukDalamGrup(GameObject panel)
    {
        Transform induk = transform.parent;
        while (induk != null)
        {
            RotasiObjekSoal rotator = induk.GetComponent<RotasiObjekSoal>();
            if (rotator != null && rotator.panelSoal == panel) return true;
            induk = induk.parent;
        }
        return false;
    }

    private static Grup AmbilGrup(GameObject panel)
    {
        Grup grup;
        if (!grupPerPanel.TryGetValue(panel, out grup))
        {
            grup = new Grup();
            grupPerPanel.Add(panel, grup);
        }
        return grup;
    }

    private static void ProsesInput()
    {
        if (frameInput == Time.frameCount) return;
        frameInput = Time.frameCount;
        bool turun = false, tahan = false, lepas = false;
        Vector2 posisi;
        int id;
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            posisi = t.position; id = t.fingerId;
            turun = t.phase == TouchPhase.Began;
            tahan = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
            lepas = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }
        else
        {
            posisi = Input.mousePosition; id = -999;
            turun = Input.GetMouseButtonDown(0);
            tahan = Input.GetMouseButton(0);
            lepas = Input.GetMouseButtonUp(0);
        }
        if (turun)
        {
            if (EventSystem.current != null &&
                (Input.touchCount > 0 ? EventSystem.current.IsPointerOverGameObject(id) : EventSystem.current.IsPointerOverGameObject()))
                return;
            RotasiObjekSoal kandidat = null;
            float jarakTerdekat = float.MaxValue;
            for (int i = aktif.Count - 1; i >= 0; i--)
            {
                RotasiObjekSoal item = aktif[i];
                if (item == null || !item.isActiveAndEnabled || !item.siap || item.panelSoal == null ||
                    !item.panelSoal.activeInHierarchy || item.GetComponent<TabungMasukRak>() != null) continue;
                float jarak;
                if (item.DalamLayar(posisi, out jarak) && jarak < jarakTerdekat)
                { kandidat = item; jarakTerdekat = jarak; }
            }
            dipilih = kandidat;
            if (dipilih != null) { posisiInput = posisi; idInput = id; }
        }
        if (dipilih != null && tahan && idInput == id)
        {
            Vector2 delta = posisi - posisiInput;
            posisiInput = posisi;
            Camera cam = Camera.main;
            if (cam != null)
            {
                Grup grup = AmbilGrup(dipilih.panelSoal);
                Quaternion horizontal = Quaternion.AngleAxis(delta.x * dipilih.sensitivitas, cam.transform.up);
                Quaternion vertikal = Quaternion.AngleAxis(-delta.y * dipilih.sensitivitas, cam.transform.right);
                grup.rotasi = horizontal * vertikal * grup.rotasi;
            }
        }
        if (lepas && dipilih != null && idInput == id) { dipilih = null; idInput = -999; }
    }

    private bool DalamLayar(Vector2 titik, out float jarak)
    {
        jarak = float.MaxValue;
        Camera cam = Camera.main;
        if (cam == null || renderers == null || renderers.Length == 0) return false;
        bool ada = false;
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
            Bounds b = r.bounds;
            for (int k = 0; k < 8; k++)
            {
                Vector3 p = b.center + Vector3.Scale(b.extents, new Vector3(
                    (k & 1) == 0 ? -1f : 1f, (k & 2) == 0 ? -1f : 1f, (k & 4) == 0 ? -1f : 1f));
                Vector3 screen = cam.WorldToScreenPoint(p);
                if (screen.z <= 0f) continue;
                min = Vector2.Min(min, screen); max = Vector2.Max(max, screen); ada = true;
            }
            float d = Vector3.Distance(cam.transform.position, b.center);
            if (d < jarak) jarak = d;
        }
        return ada && titik.x >= min.x && titik.x <= max.x && titik.y >= min.y && titik.y <= max.y;
    }

    public static bool SudahDiputar(GameObject panel)
    {
        if (panel == null) return false;
        Grup grup;
        return grupPerPanel.TryGetValue(panel, out grup) && Quaternion.Angle(Quaternion.identity, grup.rotasi) > 0.25f;
    }
    public void ResetRotasi() { ResetPanel(panelSoal); }
    public static void ResetPanel(GameObject panel)
    {
        if (panel == null) { ResetSemua(); return; }
        Grup grup;
        if (grupPerPanel.TryGetValue(panel, out grup))
        {
            grup.rotasi = Quaternion.identity;
            grup.pusatTersimpan = false;
        }
        if (dipilih != null && dipilih.panelSoal == panel) dipilih = null;
    }
    public static void ResetSemua()
    {
        foreach (var pasangan in grupPerPanel)
        {
            pasangan.Value.rotasi = Quaternion.identity;
            pasangan.Value.pusatTersimpan = false;
        }
        dipilih = null;
    }
}