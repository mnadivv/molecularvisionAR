using System;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

/// <summary>
/// Pasang script ini pada GameObject di Scene Unity Vuforia Anda (misal pada ARReceiver atau GameManager).
/// Script ini membaca data topik kimia dari aplikasi Flutter dan mengaktifkan ImageTarget/Model Vuforia yang sesuai.
/// </summary>
public class UnityAndroidReceiver : MonoBehaviour
{
    public static UnityAndroidReceiver Instance { get; private set; }

    [Header("Current Topic Data")]
    public string currentTopicId = "";
    public string currentTopicTitle = "";

    [Header("Vuforia Setup Mode")]
    [Tooltip("PILIHAN 1: Jika Anda menggunakan 1 Kartu Marker untuk SEMUA molekul, masukkan ImageTarget tersebut ke modelRoot.\n" +
             "PILIHAN 2: Jika Anda menggunakan KARTU BERBEDA untuk setiap materi, masukkan parent dari semua ImageTarget ke targetsRoot.")]
    public SetupMode vuforiaMode = SetupMode.SingleMarkerMultipleModels;

    public enum SetupMode
    {
        [Tooltip("1 ImageTarget memiliki banyak anak model molekul")]
        SingleMarkerMultipleModels,
        
        [Tooltip("Setiap materi kimia memiliki GameObject ImageTarget sendiri")]
        MultipleImageTargets
    }

    [Header("Vuforia Targets / Models")]
    [Tooltip("Parent wadah (bisa berupa 1 ImageTarget atau grup ImageTargets)")]
    public Transform targetsOrModelsRoot;

    [Header("UI References (Opsional)")]
    public Text topicTitleText;
    public Text markerHintText;
    public Button backButton;

    private void Awake()
    {
        // 1. Optimasi performa dan frame rate kamera AR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 2. Kaitkan optimasi kamera Vuforia berkecepatan tinggi saat Vuforia Engine dimulai
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToFlutter);
        }
    }

    private void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
        {
            VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        }
    }

    /// <summary>
    /// Mengunci kamera ke mode performa tinggi (60 FPS sensor, latensi minimal)
    /// </summary>
    private void OnVuforiaStarted()
    {
        try
        {
            // Meminta sensor kamera bekerja pada FPS tertinggi dan buffer delay terendah
            VuforiaBehaviour.Instance.CameraDevice.SetCameraMode(CameraMode.MODE_OPTIMIZE_SPEED);

            // Menjaga continuous autofocus aktif agar tidak terjadi shutter hunting delay
            VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

            Debug.Log("[Vuforia-Receiver] Berhasil mengunci CameraMode: MODE_OPTIMIZE_SPEED & FOCUS_MODE_CONTINUOUSAUTO");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[Vuforia-Receiver] Gagal mengatur CameraMode: " + ex.Message);
        }
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToFlutter);
        }
        ReadIntentData();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            ReadIntentData();
        }
    }

    /// <summary>
    /// Membaca parameter intent dari Flutter (topic_id, topic_title)
    /// </summary>
    public void ReadIntentData()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent"))
            {
                if (intent != null)
                {
                    string topicId = intent.Call<string>("getStringExtra", "topic_id");
                    string topicTitle = intent.Call<string>("getStringExtra", "topic_title");

                    Debug.Log($"[Vuforia-Receiver] Diterima dari Flutter: ID={topicId}, Title={topicTitle}");

                    if (!string.IsNullOrEmpty(topicId))
                    {
                        currentTopicId = topicId;
                        currentTopicTitle = string.IsNullOrEmpty(topicTitle) ? topicId : topicTitle;

                        UpdateUI();
                        ApplyVuforiaTopic(currentTopicId);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Vuforia-Receiver] Gagal membaca intent: " + e.Message);
        }
#else
        Debug.Log("[Vuforia-Receiver] Mode Unity Editor. Jika ingin test topik, panggil ApplyVuforiaTopic('ikatan_kimia')");
        UpdateUI();
        if (!string.IsNullOrEmpty(currentTopicId))
        {
            ApplyVuforiaTopic(currentTopicId);
        }
#endif
    }

    private void UpdateUI()
    {
        if (topicTitleText != null && !string.IsNullOrEmpty(currentTopicTitle))
        {
            topicTitleText.text = currentTopicTitle;
        }

        if (markerHintText != null && !string.IsNullOrEmpty(currentTopicTitle))
        {
            markerHintText.text = $"Arahkan kamera ke Kartu Marker: {currentTopicTitle}";
        }
    }

    /// <summary>
    /// Logika aktivasi model atau ImageTarget Vuforia berdasarkan topicId
    /// </summary>
    public void ApplyVuforiaTopic(string topicId)
    {
        if (targetsOrModelsRoot == null)
        {
            Debug.LogWarning("[Vuforia-Receiver] targetsOrModelsRoot belum dihubungkan di Inspector!");
            return;
        }

        string sanitized = topicId.ToLower().Trim().Replace(" ", "_");
        bool anyFound = false;

        foreach (Transform child in targetsOrModelsRoot)
        {
            string childName = child.gameObject.name.ToLower().Replace(" ", "_");
            bool match = childName.Contains(sanitized) || sanitized.Contains(childName);

            // Aktifkan objek/target yang cocok dan matikan yang lain
            child.gameObject.SetActive(match);

            if (match)
            {
                anyFound = true;
                Debug.Log($"[Vuforia-Receiver] Objek Vuforia '{child.name}' DIAKTIFKAN untuk materi '{sanitized}'");
            }
        }

        if (!anyFound)
        {
            Debug.LogWarning($"[Vuforia-Receiver] Tidak ada objek di bawah '{targetsOrModelsRoot.name}' yang cocok dengan topicId '{sanitized}'");
        }
    }

    /// <summary>
    /// Menutup APK Unity Vuforia dan kembali ke aplikasi Flutter
    /// </summary>
    public void BackToFlutter()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            SendToFlutter.Send("onBack");
        }
        catch (Exception e)
        {
            Debug.LogError("[Vuforia-Receiver] Gagal kembali ke Flutter: " + e.Message);
        }
#else
        Debug.Log("[Vuforia-Receiver] Tombol Back ditekan (kirim onBack ke Flutter).");
#endif
    }
}

