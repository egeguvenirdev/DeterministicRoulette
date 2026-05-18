using UnityEngine;

[DisallowMultipleComponent]
public class UIAudioPlayer : MonoBehaviour
{
    [Header("Event Audio")]
    [SerializeField] private GameAudioService gameAudioService;

    public void Play(UIAudioEventType eventType)
    {
        if (gameAudioService == null)
        {
            return;
        }

        if (!TryMapToGameEvent(eventType, out GameAudioEventId eventId))
        {
            return;
        }

        gameAudioService.Play(eventId);
    }

    private static bool TryMapToGameEvent(UIAudioEventType eventType, out GameAudioEventId eventId)
    {
        switch (eventType)
        {
            case UIAudioEventType.ButtonClick:
                eventId = GameAudioEventId.UiButtonClick;
                return true;
            case UIAudioEventType.DropdownOpen:
                eventId = GameAudioEventId.UiDropdownOpen;
                return true;
            case UIAudioEventType.DropdownChanged:
                eventId = GameAudioEventId.UiDropdownChanged;
                return true;
            case UIAudioEventType.ToggleOn:
                eventId = GameAudioEventId.UiToggleOn;
                return true;
            case UIAudioEventType.ToggleOff:
                eventId = GameAudioEventId.UiToggleOff;
                return true;
            case UIAudioEventType.SliderChanged:
                eventId = GameAudioEventId.UiSliderChanged;
                return true;
            case UIAudioEventType.InputFocus:
                eventId = GameAudioEventId.UiInputFocus;
                return true;
            default:
                eventId = GameAudioEventId.None;
                return false;
        }
    }
}
