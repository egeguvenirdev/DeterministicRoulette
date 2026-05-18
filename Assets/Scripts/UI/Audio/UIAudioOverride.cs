using UnityEngine;

[DisallowMultipleComponent]
public class UIAudioOverride : MonoBehaviour
{
    [SerializeField, Range(0f, 2f)] private float volumeScale = 1f;

    [Header("Optional Overrides")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip dropdownOpen;
    [SerializeField] private AudioClip dropdownChanged;
    [SerializeField] private AudioClip toggleOn;
    [SerializeField] private AudioClip toggleOff;
    [SerializeField] private AudioClip sliderChanged;
    [SerializeField] private AudioClip inputFocus;
    [SerializeField] private AudioClip inputSubmit;
    [SerializeField] private AudioClip invalid;

    public float VolumeScale => volumeScale;

    public bool TryGetClip(UIAudioEventType eventType, out AudioClip clip)
    {
        clip = GetClip(eventType);
        return clip != null;
    }

    private AudioClip GetClip(UIAudioEventType eventType)
    {
        switch (eventType)
        {
            case UIAudioEventType.ButtonClick:
                return buttonClick;
            case UIAudioEventType.DropdownOpen:
                return dropdownOpen;
            case UIAudioEventType.DropdownChanged:
                return dropdownChanged;
            case UIAudioEventType.ToggleOn:
                return toggleOn;
            case UIAudioEventType.ToggleOff:
                return toggleOff;
            case UIAudioEventType.SliderChanged:
                return sliderChanged;
            case UIAudioEventType.InputFocus:
                return inputFocus;
            case UIAudioEventType.InputSubmit:
                return inputSubmit;
            case UIAudioEventType.Invalid:
                return invalid;
            default:
                return null;
        }
    }
}
