using UnityEngine;

public enum UIAudioEventType
{
    ButtonClick,
    DropdownOpen,
    DropdownChanged,
    ToggleOn,
    ToggleOff,
    SliderChanged,
    InputFocus,
    InputSubmit,
    Invalid
}

[CreateAssetMenu(menuName = "DeterministicRoulette/UI Audio Profile", fileName = "UIAudioProfile")]
public class UIAudioProfile : ScriptableObject
{
    [Header("UI Clips")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip dropdownOpen;
    [SerializeField] private AudioClip dropdownChanged;
    [SerializeField] private AudioClip toggleOn;
    [SerializeField] private AudioClip toggleOff;
    [SerializeField] private AudioClip sliderChanged;
    [SerializeField] private AudioClip inputFocus;
    [SerializeField] private AudioClip inputSubmit;
    [SerializeField] private AudioClip invalid;

    public AudioClip GetClip(UIAudioEventType eventType)
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
