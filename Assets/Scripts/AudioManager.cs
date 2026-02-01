using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class WorldMusic
    {
        public int worldNumber = 1;
        public AudioClip music;
    }

    public static AudioManager Instance { get; private set; }

    [Header("World Music")]
    [SerializeField] private List<WorldMusic> worldMusic = new();
    [SerializeField] private float worldTransitionDelay = 2.0f;
    [SerializeField] private float fadeOutTime = 1.0f;
    [SerializeField] private float fadeInTime = 1.0f;
    [SerializeField] private float musicVolume = 1.0f;

    [Header("SFX")]
    [SerializeField] private float sfxVolume = 1.0f;

    private AudioSource musicA;
    private AudioSource musicB;
    private AudioSource sfxSource;

    private bool usingA = true;
    private int currentWorld = 0;
    private Coroutine musicRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicA = gameObject.AddComponent<AudioSource>();
        musicB = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        SetupMusicSource(musicA);
        SetupMusicSource(musicB);

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    private void SetupMusicSource(AudioSource src)
    {
        src.loop = true;
        src.playOnAwake = false;
        src.volume = 0f;
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);

        AudioSource active = usingA ? musicA : musicB;
        if (active != null && active.isPlaying)
            active.volume = musicVolume;
    }

    public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volumeMultiplier));
    }

    public void SetWorldMusic(int worldNumber)
    {
        if (worldNumber == currentWorld) return;

        AudioClip nextClip = GetClipForWorld(worldNumber);
        if (nextClip == null)
        {
            currentWorld = worldNumber;
            return;
        }

        currentWorld = worldNumber;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(CrossfadeTo(nextClip, worldTransitionDelay, fadeOutTime, fadeInTime));
    }

    private AudioClip GetClipForWorld(int worldNumber)
    {
        for (int i = 0; i < worldMusic.Count; i++)
        {
            if (worldMusic[i].worldNumber == worldNumber)
                return worldMusic[i].music;
        }
        return null;
    }

    private IEnumerator CrossfadeTo(AudioClip nextClip, float delay, float fadeOut, float fadeIn)
    {
        AudioSource from = usingA ? musicA : musicB;
        AudioSource to = usingA ? musicB : musicA;

        if (delay > 0f && (usingA ? musicA : musicB).isPlaying)
            yield return new WaitForSeconds(delay);


        if (!from.isPlaying)
        {
            to.clip = nextClip;
            to.volume = 0f;
            to.Play();
            yield return Fade(to, 0f, musicVolume, fadeIn);

            usingA = !usingA;
            yield break;
        }

        to.clip = nextClip;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float fromStart = from.volume;
        float toStart = to.volume;

        while (t < Mathf.Max(fadeOut, fadeIn))
        {
            t += Time.deltaTime;

            float outAlpha = (fadeOut <= 0f) ? 1f : Mathf.Clamp01(t / fadeOut);
            float inAlpha = (fadeIn <= 0f) ? 1f : Mathf.Clamp01(t / fadeIn);

            from.volume = Mathf.Lerp(fromStart, 0f, outAlpha);
            to.volume = Mathf.Lerp(toStart, musicVolume, inAlpha);

            yield return null;
        }

        from.volume = 0f;
        to.volume = musicVolume;

        from.Stop();
        from.clip = null;

        usingA = !usingA;
    }

    private IEnumerator Fade(AudioSource src, float from, float to, float time)
    {
        if (src == null) yield break;

        if (time <= 0f)
        {
            src.volume = to;
            yield break;
        }

        float t = 0f;
        src.volume = from;

        while (t < time)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(from, to, Mathf.Clamp01(t / time));
            yield return null;
        }

        src.volume = to;
    }

    public void ForceStartWorldMusic(int worldNumber)
    {
        AudioClip clip = GetClipForWorld(worldNumber);
        if (clip == null) return;

        currentWorld = worldNumber;

        AudioSource active = usingA ? musicA : musicB;
        active.clip = clip;
        active.volume = musicVolume;
        active.loop = true;

        if (!active.isPlaying)
            active.Play();
    }

}
