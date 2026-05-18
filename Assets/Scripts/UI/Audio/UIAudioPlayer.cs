using UnityEngine;

[DisallowMultipleComponent]
public class UIAudioPlayer : MonoBehaviour
{
    [SerializeField] private UIAudioProfile profile;
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField, Min(0f)] private float minIntervalSeconds = 0.03f;

    private float lastPlayTime = -999f;

    private void Awake()
    {
        if (targetAudioSource == null)
        {
            targetAudioSource = GetComponent<AudioSource>();
        }
    }

    public void Play(UIAudioEventType eventType)
    {
        Play(eventType, null, 1f);
    }

    public void Play(UIAudioEventType eventType, AudioClip overrideClip, float volumeScale)
    {
        if (targetAudioSource == null)
        {
            return;
        }

        if (Time.unscaledTime - lastPlayTime < minIntervalSeconds)
        {
            return;
        }

        AudioClip clip = overrideClip;
        if (clip == null && profile != null)
        {
            clip = profile.GetClip(eventType);

            // Fallback so first-click interactions still produce sound when dedicated clips are not assigned yet.
            if (clip == null && (eventType == UIAudioEventType.DropdownOpen || eventType == UIAudioEventType.InputFocus))
            {
                clip = profile.GetClip(UIAudioEventType.ButtonClick);
            }
        }

        if (clip == null)
        {
            return;
        }

        float finalVolume = Mathf.Clamp01(masterVolume * Mathf.Max(0f, volumeScale));
        if (finalVolume <= 0f)
        {
            return;
        }

        targetAudioSource.PlayOneShot(clip, finalVolume);
        lastPlayTime = Time.unscaledTime;
    }
}
