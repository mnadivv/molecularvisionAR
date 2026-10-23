using UnityEngine;

/// <summary>
/// Otomatis dipasang ke tabung yang dilepas di SlotA / SlotB.
/// Animasi: tabung terbang dari titik lepas ke atas lubang rak (sambil menyesuaikan ukuran dengan rak),
/// lalu turun masuk ke lubang dan sedikit memantul saat menyentuh dasar rak.
/// Setelah selesai tabung menjadi anak dari rak, jadi ikut bergerak bersama rak.
/// </summary>
[DefaultExecutionOrder(200)]
public class TabungMasukRak : MonoBehaviour
{
    private SlotTabung slot;
    private Transform rak;
    private Camera cam;
    private Vector3 lubang;
    private float angkat;
    private float durasiPindah;
    private float durasiTurun;

    private Vector3 awalLokalKamera;
    private Quaternion rotasiRelatifKamera;
    private Vector3 skalaAwal;
    private Vector3 skalaAkhir;
    private float waktu;
    private bool jalan;

    public void Mulai(SlotTabung s, Transform rakTransform, Camera kamera, Vector3 posisiLubang,
                      float tinggiAngkat, float dPindah, float dTurun, Quaternion rotRelatif)
    {
        slot = s;
        rak = rakTransform;
        cam = kamera;
        lubang = posisiLubang;
        angkat = tinggiAngkat;
        durasiPindah = Mathf.Max(0.01f, dPindah);
        durasiTurun = Mathf.Max(0.01f, dTurun);

        awalLokalKamera = cam.transform.InverseTransformPoint(transform.position);
        rotasiRelatifKamera = rotRelatif;
        skalaAwal = transform.localScale;
        skalaAkhir = skalaAwal;

        InfoModel3D infoTabung = GetComponent<InfoModel3D>();
        InfoModel3D infoRak = rak != null ? rak.GetComponent<InfoModel3D>() : null;
        if (infoTabung != null && infoRak != null)
            skalaAkhir = infoTabung.skalaAsset * infoRak.faktorSkala;

        KacaSaatMulai();

        waktu = 0f;
        jalan = true;
    }

    // Tabung langsung dibuat tembus pandang (kaca) begitu diambil, tidak menunggu bahan pertama dituang
    private void KacaSaatMulai()
    {
        ManajerTakaran m = ManajerTakaran.Instance;
        if (m == null || m.materialKaca == null) return;
        Renderer[] rs = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rs.Length; i++)
        {
            Material[] materials = rs[i].sharedMaterials;
            bool changed = false;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] == null) continue;
                string nama = materials[j].name.ToLowerInvariant();
                if (!nama.Contains("kaca") && !nama.Contains("glass")) continue;
                materials[j] = m.materialKaca;
                changed = true;
            }
            if (changed) rs[i].sharedMaterials = materials;
        }
    }

    private static float EaseInOut(float k)
    {
        k = Mathf.Clamp01(k);
        return k * k * (3f - 2f * k);
    }

    private void LateUpdate()
    {
        if (!jalan) return;
        if (rak == null || cam == null)
        {
            jalan = false;
            if (slot != null) slot.TandaiSelesaiMasuk();
            Destroy(this);
            return;
        }

        waktu += Time.deltaTime;

        Vector3 dasar = rak.TransformPoint(lubang);
        Vector3 diAtas = rak.TransformPoint(lubang + Vector3.up * angkat);
        Vector3 mulai = cam.transform.TransformPoint(awalLokalKamera);

        Vector3 pos;
        Vector3 skala;
        if (waktu < durasiPindah)
        {
            // Tahap 1: terbang ke atas lubang
            float e = EaseInOut(waktu / durasiPindah);
            pos = Vector3.Lerp(mulai, diAtas, e);
            skala = Vector3.Lerp(skalaAwal, skalaAkhir, e);
        }
        else
        {
            // Tahap 2: turun masuk lubang, 80% waktu untuk turun, 20% untuk memantul
            float k = Mathf.Clamp01((waktu - durasiPindah) / durasiTurun);
            float kt = Mathf.Clamp01(k / 0.8f);
            pos = Vector3.Lerp(diAtas, dasar, kt * kt);
            if (k > 0.8f)
            {
                float m = (k - 0.8f) / 0.2f;
                pos += rak.TransformVector(Vector3.up * (Mathf.Sin(m * Mathf.PI) * 0.12f));
            }
            skala = skalaAkhir;
        }

        transform.rotation = cam.transform.rotation * rotasiRelatifKamera;
        transform.localScale = skala;
        transform.position = pos;

        if (waktu >= durasiPindah + durasiTurun)
        {
            transform.rotation = cam.transform.rotation * rotasiRelatifKamera;
            transform.localScale = skalaAkhir;
            transform.position = dasar;
            transform.SetParent(rak, true);
            jalan = false;
            if (slot != null) slot.TandaiSelesaiMasuk();
            Destroy(this);
        }
    }
}
