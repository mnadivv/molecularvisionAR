using System;
using UnityEngine;

#if FLUTTER_UNITY_INTEGRATION
using FlutterUnityIntegration;
#endif

/// <summary>
/// Pasang script ini pada GameObject bernama "FlutterUnityBridge" di Scene Unity utama Anda.
/// Script ini menangani komunikasi 2 arah antara Flutter dan Unity.
/// </summary>
public class FlutterUnityBridge : MonoBehaviour
{
    public static FlutterUnityBridge Instance { get; private set; }

    [System.Serializable]
    public class TopicData
    {
        public string topic;
        public string topicId;
        public string subtitle;
        public long timestamp;
    }

    [Header("Current Topic")]
    public string currentTopic = "";
    public string currentTopicId = "";

    [Header("Model Container")]
    [Tooltip("Transform parent tempat molekul/objek AR ditempatkan")]
    public Transform modelRoot;

    [Header("UI / Labels")]
    public GameObject labelContainer;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (modelRoot != null)
        {
            initialPosition = modelRoot.localPosition;
            initialRotation = modelRoot.localRotation;
            initialScale = modelRoot.localScale;
        }
    }

    private void Start()
    {
        // Beritahu Flutter bahwa Unity Scene sudah siap
        SendToFlutter("SceneReady");
    }

    /// <summary>
    /// Dipanggil dari Flutter: _unityWidgetController.postMessage('FlutterUnityBridge', 'LoadTopic', payloadJson);
    /// </summary>
    public void LoadTopic(string jsonPayload)
    {
        Debug.Log("[FlutterUnityBridge] Received LoadTopic: " + jsonPayload);

        try
        {
            TopicData data = JsonUtility.FromJson<TopicData>(jsonPayload);
            currentTopic = data.topic;
            currentTopicId = data.topicId;

            // Logika untuk mengaktifkan model 3D yang sesuai
            ActivateTopicModel(currentTopicId);

            SendToFlutter("ModelLoaded: " + currentTopic);
        }
        catch (Exception e)
        {
            Debug.LogError("[FlutterUnityBridge] Gagal parse JSON topik: " + e.Message);
            // Fallback jika dikirimkan plain string bukan JSON
            currentTopic = jsonPayload;
            ActivateTopicModel(jsonPayload.ToLower().Replace(" ", "_"));
            SendToFlutter("ModelLoaded: " + currentTopic);
        }
    }

    /// <summary>
    /// Mengaktifkan objek 3D molekul berdasarkan topicId
    /// Misalnya anak dari modelRoot memiliki nama sesuai topik (misal: "ikatan_kimia", "laju_reaksi", dll)
    /// </summary>
    private void ActivateTopicModel(string topicId)
    {
        if (modelRoot == null) return;

        bool found = false;
        foreach (Transform child in modelRoot)
        {
            string childName = child.gameObject.name.ToLower().Replace(" ", "_");
            bool match = childName.Contains(topicId) || topicId.Contains(childName);
            child.gameObject.SetActive(match);
            if (match) found = true;
        }

        if (!found)
        {
            Debug.LogWarning("[FlutterUnityBridge] Tidak ditemukan objek model dengan ID: " + topicId);
        }

        ResetModel("auto");
    }

    /// <summary>
    /// Dipanggil dari Flutter: _unityWidgetController.postMessage('FlutterUnityBridge', 'ResetModel', 'reset');
    /// </summary>
    public void ResetModel(string msg)
    {
        if (modelRoot != null)
        {
            modelRoot.localPosition = initialPosition;
            modelRoot.localRotation = initialRotation;
            modelRoot.localScale = initialScale;
            Debug.Log("[FlutterUnityBridge] Model transform direset");
        }
    }

    /// <summary>
    /// Dipanggil dari Flutter: _unityWidgetController.postMessage('FlutterUnityBridge', 'ToggleLabels', 'true'/'false');
    /// </summary>
    public void ToggleLabels(string isVisibleStr)
    {
        bool isVisible = isVisibleStr.ToLower() == "true";
        if (labelContainer != null)
        {
            labelContainer.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Mengirim pesan kembali ke Flutter
    /// </summary>
    public void SendToFlutter(string message)
    {
#if FLUTTER_UNITY_INTEGRATION
        UnityMessageManager.Instance.SendMessageToFlutter(message);
#else
        Debug.Log("[FlutterUnityBridge -> Flutter] " + message);
#endif
    }
}
