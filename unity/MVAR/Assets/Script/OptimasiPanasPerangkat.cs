using UnityEngine;

/// <summary>
/// Membatasi render pada perangkat mobile agar beban GPU/CPU dan panas berkurang.
/// Dibuat otomatis sebelum scene pertama, jadi tidak perlu dipasang manual di Inspector.
/// </summary>
[DisallowMultipleComponent]
public sealed class OptimasiPanasPerangkat : MonoBehaviour
{
    private static OptimasiPanasPerangkat instance;
    private const int TargetFrameRateMobile = 30;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        GameObject host = new GameObject(nameof(OptimasiPanasPerangkat));
        DontDestroyOnLoad(host);
        host.AddComponent<OptimasiPanasPerangkat>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        TerapkanBatasRender();
    }

    private void OnApplicationFocus(bool fokus)
    {
        if (fokus) TerapkanBatasRender();
    }

    private void OnApplicationPause(bool jeda)
    {
        if (!jeda) TerapkanBatasRender();
    }

    private static void TerapkanBatasRender()
    {
        if (!Application.isMobilePlatform) return;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRateMobile;
    }
}