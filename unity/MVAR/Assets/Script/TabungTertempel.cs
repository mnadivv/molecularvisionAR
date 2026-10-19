using UnityEngine;

/// <summary>Menjaga alat tetap pada titik layar yang ditetapkan saat marker terlacak.</summary>
public class TabungTertempel : MonoBehaviour
{
    private SlotTabung slot;
    private Camera cam;
    private float kedalaman;
    private Vector3 offsetPusatLokal;
    private Quaternion rotasiRelatif;

    public void Pasang(SlotTabung s, Camera c, float z, Vector3 offset, Quaternion rotRel)
    {
        slot = s; cam = c; kedalaman = z; offsetPusatLokal = offset; rotasiRelatif = rotRel;
    }

    private void LateUpdate()
    {
        if (slot == null || cam == null) return;
        Vector2 min, max, pusat;
        slot.AmbilRectLayar(cam, out min, out max, out pusat);
        Vector3 target = cam.ScreenToWorldPoint(new Vector3(pusat.x, pusat.y, kedalaman));
        transform.rotation = cam.transform.rotation * rotasiRelatif;
        transform.position = target + cam.transform.TransformVector(offsetPusatLokal);
    }
}
