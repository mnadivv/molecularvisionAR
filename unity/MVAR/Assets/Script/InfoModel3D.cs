using UnityEngine;

/// <summary>
/// Otomatis dipasang ke model 3D yang dimunculkan CardTahanGeser3D.
/// Menyimpan skala asli aset dan faktor pembesaran/pengecilannya,
/// supaya benda lain (mis. tabung) bisa memakai skala yang sama dengan rak.
/// </summary>
public class InfoModel3D : MonoBehaviour
{
    public Vector3 skalaAsset = Vector3.one;
    public float faktorSkala = 1f;

    public void Perbarui()
    {
        if (Mathf.Abs(skalaAsset.x) > 0.000001f)
            faktorSkala = transform.localScale.x / skalaAsset.x;
    }
}
