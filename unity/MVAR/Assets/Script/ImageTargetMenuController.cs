using UnityEngine;
using Vuforia; // Wajib menggunakan Vuforia

public class ImageTargetMenuController : MonoBehaviour
{
    [Header("=== OBJEK YANG DIMUNCULKAN SAAT SCAN ===")]
    [Tooltip("Drag 'Group Menu' atau objek apapun yang ingin Anda munculkan ke sini.")]
    public GameObject[] objekYangDimunculkan;

    [Header("=== PENGATURAN ===")]
    [Tooltip("Centang jika ingin objek otomatis disembunyikan saat marker hilang.")]
    public bool sembunyikanSaatMarkerHilang = true;

    [Tooltip("Centang jika ingin ada jeda (delay) sebelum objek muncul.")]
    public bool gunakanDelay = false;
    public float delayDetik = 0.5f;

    private ObserverBehaviour mObserverBehaviour;
    private bool sudahMuncul = false;

    void Start()
    {
        // Ambil komponen ObserverBehaviour dari ImageTarget
        mObserverBehaviour = GetComponent<ObserverBehaviour>();

        if (mObserverBehaviour != null)
        {
            // Daftarkan event saat status target berubah (terdeteksi / hilang)
            mObserverBehaviour.OnTargetStatusChanged += OnStatusChanged;
            
            // Sembunyikan objek di awal permainan
            SembunyikanSemua();
        }
        else
        {
            Debug.LogError("[ImageTargetMenuController] Script ini harus dipasang di objek ImageTarget Vuforia!");
        }
    }

    void OnDestroy()
    {
        if (mObserverBehaviour != null)
            mObserverBehaviour.OnTargetStatusChanged -= OnStatusChanged;
    }

    // Fungsi yang dipanggil otomatis oleh Vuforia saat status marker berubah
    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // Cek apakah marker sedang terdeteksi (TRACKED atau EXTENDED_TRACKED)
        bool terdeteksi = (status.Status == Status.TRACKED || 
                           status.Status == Status.EXTENDED_TRACKED);

        if (terdeteksi)
        {
            // Jika terdeteksi dan belum muncul, munculkan objek
            if (!sudahMuncul)
            {
                if (gunakanDelay)
                    Invoke(nameof(MunculkanSemua), delayDetik);
                else
                    MunculkanSemua();
                    
                sudahMuncul = true;
            }
        }
        else
        {
            // Jika marker hilang
            if (sembunyikanSaatMarkerHilang && sudahMuncul)
            {
                SembunyikanSemua();
                sudahMuncul = false;
            }
        }
    }

    void MunculkanSemua()
    {
        foreach (GameObject obj in objekYangDimunculkan)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        Debug.Log("[ImageTargetMenuController] Marker terdeteksi! Menu dimunculkan.");
    }

    void SembunyikanSemua()
    {
        foreach (GameObject obj in objekYangDimunculkan)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}