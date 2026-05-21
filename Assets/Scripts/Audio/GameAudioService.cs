using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameAudioService : MonoBehaviour
{
    [SerializeField] private GameAudioEventBank eventBank;

    [Header("Bus Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Debug")]
    [SerializeField] private bool logMissingEvents;

    [Header("Startup Music")]
    [SerializeField] private GameAudioEventId startupMusicEvent = GameAudioEventId.None;

    private readonly Dictionary<GameAudioEventId, float> lastPlayTimeByEvent = new Dictionary<GameAudioEventId, float>();
    private readonly Dictionary<GameAudioEventId, int> activeVoicesByEvent = new Dictionary<GameAudioEventId, int>();

    private void Start()
    {
        if (startupMusicEvent != GameAudioEventId.None)
        {
            PlayMusic(startupMusicEvent);
        }
    }

    public void Play(GameAudioEventId eventId)
    {
        PlayInternal(eventId, null);
    }

    public void PlayAt(GameAudioEventId eventId, Vector3 worldPosition)
    {
        PlayInternal(eventId, worldPosition);
    }

    /// <summary>Plays the given event on the music source with looping enabled.</summary>
    public void PlayMusic(GameAudioEventId eventId)
    {
        if (musicSource == null || eventBank == null)
        {
            return;
        }

        if (!eventBank.TryGet(eventId, out GameAudioEventDefinition definition) || definition == null)
        {
            if (logMissingEvents)
            {
                Debug.LogWarning($"[GameAudioService] Missing audio event definition: {eventId}", this);
            }
            return;
        }

        AudioClip clip = definition.PickClip();
        if (clip == null)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.volume = Mathf.Clamp01(definition.baseVolume);
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>Stops the music source.</summary>
    public void StopMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.Stop();
    }

    private void PlayInternal(GameAudioEventId eventId, Vector3? worldPosition)
    {
        if (eventBank == null)
        {
            return;
        }

        if (!eventBank.TryGet(eventId, out GameAudioEventDefinition definition) || definition == null)
        {
            if (logMissingEvents)
            {
                Debug.LogWarning($"[GameAudioService] Missing audio event definition: {eventId}", this);
            }
            return;
        }

        if (!CanPlay(definition, eventId))
        {
            return;
        }

        AudioClip clip = definition.PickClip();
        if (clip == null)
        {
            return;
        }

        float finalVolume = Mathf.Clamp01(definition.baseVolume);

        if (definition.spatialize3D || worldPosition.HasValue)
        {
            Play3D(eventId, clip, finalVolume, worldPosition ?? transform.position, definition.bus);
        }
        else
        {
            Play2D(eventId, clip, finalVolume, definition.bus);
        }

        lastPlayTimeByEvent[eventId] = Time.unscaledTime;
    }

    private bool CanPlay(GameAudioEventDefinition definition, GameAudioEventId eventId)
    {
        if (lastPlayTimeByEvent.TryGetValue(eventId, out float lastPlay))
        {
            if (Time.unscaledTime - lastPlay < definition.cooldownSeconds)
            {
                return false;
            }
        }

        if (activeVoicesByEvent.TryGetValue(eventId, out int activeVoices))
        {
            if (activeVoices >= Mathf.Max(1, definition.maxSimultaneous))
            {
                return false;
            }
        }

        return true;
    }

    private void Play2D(GameAudioEventId eventId, AudioClip clip, float volume, GameAudioBus bus)
    {
        AudioSource source = GetBusSource(bus);
        if (source == null)
        {
            return;
        }

        source.PlayOneShot(clip, volume);

        StartCoroutine(TrackVoiceLifetime(eventId, clip.length));
    }

    private void Play3D(GameAudioEventId eventId, AudioClip clip, float volume, Vector3 worldPosition, GameAudioBus bus)
    {
        AudioSource busSource = GetBusSource(bus);

        GameObject temp = new GameObject($"Audio_{eventId}");
        temp.transform.position = worldPosition;
        temp.transform.SetParent(transform, false);

        AudioSource source = temp.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = busSource != null ? busSource.outputAudioMixerGroup : null;
        source.spatialBlend = 1f;
        source.PlayOneShot(clip, volume);

        StartCoroutine(DestroyAfterPlay(temp, clip.length));
        StartCoroutine(TrackVoiceLifetime(eventId, clip.length));
    }

    private AudioSource GetBusSource(GameAudioBus bus)
    {
        switch (bus)
        {
            case GameAudioBus.UI:
                return uiSource;
            case GameAudioBus.SFX:
                return sfxSource;
            case GameAudioBus.Music:
                return musicSource;
            default:
                return sfxSource;
        }
    }

    private IEnumerator DestroyAfterPlay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (target != null)
        {
            Destroy(target);
        }
    }

    private IEnumerator TrackVoiceLifetime(GameAudioEventId eventId, float duration)
    {
        if (!activeVoicesByEvent.ContainsKey(eventId))
        {
            activeVoicesByEvent[eventId] = 0;
        }

        activeVoicesByEvent[eventId]++;
        yield return new WaitForSeconds(duration);

        if (!activeVoicesByEvent.ContainsKey(eventId))
        {
            yield break;
        }

        activeVoicesByEvent[eventId] = Mathf.Max(0, activeVoicesByEvent[eventId] - 1);
    }
}
