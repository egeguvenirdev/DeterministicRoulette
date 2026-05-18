using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIDropdownPointerSfxForwarder : MonoBehaviour, IPointerClickHandler
{
    private UIAudioPlayer audioPlayer;
    private bool isActive;
    private UIAudioEventType pointerEventType = UIAudioEventType.ButtonClick;

    public void Configure(UIAudioPlayer player, UIAudioEventType eventType)
    {
        audioPlayer = player;
        pointerEventType = eventType;
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive || audioPlayer == null)
        {
            return;
        }

        if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        audioPlayer.Play(pointerEventType);
    }
}
