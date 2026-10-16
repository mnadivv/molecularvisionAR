using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pasang di SettingTampil (objek yang selalu aktif, menaungi panel pengaturan).
/// - SliderMusic mengatur volume musik latar (AudioSource musik, clip-nya di-drop sendiri).
/// - SliderSound mengatur volume efek suara klik (AudioSource yang sama dipakai tombol-tombol lain, mis. AudioClick).
/// - ButtonMusic / ButtonSound: tombol mute/unmute cepat (klik untuk 0% / kembali ke volume sebelumnya).
/// - TextMusicPresent / TextSoundPresent: menampilkan persentase volume.
/// Volume disimpan di PlayerPrefs supaya tersimpan walau aplikasi ditutup.
/// </summary>
public class PengaturanAudio : MonoBehaviour
{
    [Header("=== AUDIO SOURCE ===")]
    [Tooltip("AudioSource untuk musik latar. Clip-nya tinggal di-drop sendiri ke sini (atau ke AudioSource-nya) kapan saja, volume tetap mengikuti slider.\nKalau kosong, otomatis dibuat AudioSource baru bernama AudioMusik (loop, belum ada clip).")]
    public AudioSource sumberMusik;
    [Tooltip("AudioSource yang dipakai untuk efek suara klik (mis. AudioClick yang sudah dipakai tombol-tombol lain).")]
    public AudioSource sumberSuara;

    [Header("=== UI ===")]
    public Slider sliderMusik;
    public Slider sliderSuara;
    public Button tombolMusik;
    public Button tombolSuara;
    public TMP_Text teksPersenMusik;
    public TMP_Text teksPersenSuara;

    [Header("=== PANEL (opsional, untuk tombol close) ===")]
    public Button tombolTutup;
    public GameObject[] panelYangDitutup;

    private const string KeyMusik = "volume_musik";
    private const string KeySuara = "volume_suara";

    private float musikSebelumMute = 1f;
    private float suaraSebelumMute = 1f;

    private void Awake()
    {
        if (sumberMusik == null)
        {
            GameObject g = GameObject.Find("AudioMusik");
            if (g == null)
            {
                g = new GameObject("AudioMusik");
                g.transform.SetParent(transform, false);
            }
            sumberMusik = g.GetComponent<AudioSource>();
            if (sumberMusik == null) sumberMusik = g.AddComponent<AudioSource>();
            sumberMusik.loop = true;
            sumberMusik.playOnAwake = false;
        }
    }

    private void Start()
    {
        float volMusik = PlayerPrefs.GetFloat(KeyMusik, sliderMusik != null ? sliderMusik.value : 0.5f);
        float volSuara = PlayerPrefs.GetFloat(KeySuara, sliderSuara != null ? sliderSuara.value : 0.5f);

        TerapkanMusik(volMusik);
        TerapkanSuara(volSuara);

        if (sliderMusik != null)
        {
            sliderMusik.SetValueWithoutNotify(volMusik);
            sliderMusik.onValueChanged.RemoveListener(TerapkanMusik);
            sliderMusik.onValueChanged.AddListener(TerapkanMusik);
        }
        if (sliderSuara != null)
        {
            sliderSuara.SetValueWithoutNotify(volSuara);
            sliderSuara.onValueChanged.RemoveListener(TerapkanSuara);
            sliderSuara.onValueChanged.AddListener(TerapkanSuara);
        }

        if (tombolMusik != null)
        {
            tombolMusik.onClick.RemoveListener(ToggleMusik);
            tombolMusik.onClick.AddListener(ToggleMusik);
        }
        if (tombolSuara != null)
        {
            tombolSuara.onClick.RemoveListener(ToggleSuara);
            tombolSuara.onClick.AddListener(ToggleSuara);
        }

        if (tombolTutup != null)
        {
            tombolTutup.onClick.RemoveListener(TutupPanel);
            tombolTutup.onClick.AddListener(TutupPanel);
        }

        if (sumberMusik != null && sumberMusik.clip != null && !sumberMusik.isPlaying && sumberMusik.volume > 0f)
            sumberMusik.Play();
    }

    private void Update()
    {
        // Kalau user men-drop clip musik saat Play / lewat Inspector belakangan, langsung mulai main.
        if (sumberMusik != null && sumberMusik.clip != null && !sumberMusik.isPlaying && sumberMusik.volume > 0f)
            sumberMusik.Play();
    }

    public void TerapkanMusik(float nilai)
    {
        nilai = Mathf.Clamp01(nilai);
        if (sumberMusik != null) sumberMusik.volume = nilai;
        if (teksPersenMusik != null) teksPersenMusik.text = Mathf.RoundToInt(nilai * 100f) + "%";
        PlayerPrefs.SetFloat(KeyMusik, nilai);
    }

    public void TerapkanSuara(float nilai)
    {
        nilai = Mathf.Clamp01(nilai);
        if (sumberSuara != null) sumberSuara.volume = nilai;
        if (teksPersenSuara != null) teksPersenSuara.text = Mathf.RoundToInt(nilai * 100f) + "%";
        PlayerPrefs.SetFloat(KeySuara, nilai);
    }

    private void ToggleMusik()
    {
        if (sliderMusik == null) return;
        if (sliderMusik.value > 0.0001f)
        {
            musikSebelumMute = sliderMusik.value;
            sliderMusik.value = 0f;
        }
        else
        {
            sliderMusik.value = musikSebelumMute > 0.0001f ? musikSebelumMute : 1f;
        }
    }

    private void ToggleSuara()
    {
        if (sliderSuara == null) return;
        if (sliderSuara.value > 0.0001f)
        {
            suaraSebelumMute = sliderSuara.value;
            sliderSuara.value = 0f;
        }
        else
        {
            sliderSuara.value = suaraSebelumMute > 0.0001f ? suaraSebelumMute : 1f;
        }
    }

    private void TutupPanel()
    {
        if (sumberSuara != null) sumberSuara.Play();
        if (panelYangDitutup != null)
            foreach (GameObject p in panelYangDitutup)
                if (p != null) p.SetActive(false);
    }
}
