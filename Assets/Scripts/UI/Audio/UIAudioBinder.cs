using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIAudioBinder : MonoBehaviour
{
    [SerializeField] private UIAudioPlayer audioPlayer;
    [SerializeField] private bool autoBindOnEnable = true;
    [SerializeField] private bool includeInactive = true;
    [SerializeField] private bool bindLegacyDropdown = true;
    [SerializeField] private bool bindLegacyInputField = false;

    private readonly Dictionary<Button, UnityAction> buttonHandlers = new Dictionary<Button, UnityAction>();
    private readonly Dictionary<TMP_Dropdown, UnityAction<int>> tmpDropdownHandlers = new Dictionary<TMP_Dropdown, UnityAction<int>>();
    private readonly Dictionary<Dropdown, UnityAction<int>> dropdownHandlers = new Dictionary<Dropdown, UnityAction<int>>();
    private readonly Dictionary<Toggle, UnityAction<bool>> toggleHandlers = new Dictionary<Toggle, UnityAction<bool>>();
    private readonly Dictionary<Slider, UnityAction<float>> sliderHandlers = new Dictionary<Slider, UnityAction<float>>();
    private readonly List<UIDropdownPointerSfxForwarder> pointerForwarders = new List<UIDropdownPointerSfxForwarder>();

    private void OnEnable()
    {
        if (autoBindOnEnable)
        {
            BindAll();
        }
    }

    private void OnDisable()
    {
        UnbindAll();
    }

    public void BindAll()
    {
        UnbindAll();

        if (audioPlayer == null)
        {
            audioPlayer = GetComponent<UIAudioPlayer>();
        }

        BindButtons();
        BindTMPDropdowns();

        if (bindLegacyDropdown)
        {
            BindDropdowns();
        }

        BindToggles();
        BindSliders();
        BindTMPInputFields();

        if (bindLegacyInputField)
        {
            BindInputFields();
        }
    }

    public void UnbindAll()
    {
        foreach (KeyValuePair<Button, UnityAction> pair in buttonHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.onClick.RemoveListener(pair.Value);
            }
        }
        buttonHandlers.Clear();

        foreach (KeyValuePair<TMP_Dropdown, UnityAction<int>> pair in tmpDropdownHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.onValueChanged.RemoveListener(pair.Value);
            }
        }
        tmpDropdownHandlers.Clear();

        foreach (KeyValuePair<Dropdown, UnityAction<int>> pair in dropdownHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.onValueChanged.RemoveListener(pair.Value);
            }
        }
        dropdownHandlers.Clear();

        foreach (KeyValuePair<Toggle, UnityAction<bool>> pair in toggleHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.onValueChanged.RemoveListener(pair.Value);
            }
        }
        toggleHandlers.Clear();

        foreach (KeyValuePair<Slider, UnityAction<float>> pair in sliderHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.onValueChanged.RemoveListener(pair.Value);
            }
        }
        sliderHandlers.Clear();

        for (int i = 0; i < pointerForwarders.Count; i++)
        {
            if (pointerForwarders[i] != null)
            {
                pointerForwarders[i].Deactivate();
            }
        }
        pointerForwarders.Clear();
    }

    private void BindButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(includeInactive);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button component = buttons[i];
            UnityAction action = () => Play(component, UIAudioEventType.ButtonClick);
            component.onClick.RemoveListener(action);
            component.onClick.AddListener(action);
            buttonHandlers[component] = action;
        }
    }

    private void BindTMPDropdowns()
    {
        TMP_Dropdown[] dropdowns = GetComponentsInChildren<TMP_Dropdown>(includeInactive);
        for (int i = 0; i < dropdowns.Length; i++)
        {
            TMP_Dropdown component = dropdowns[i];
            UnityAction<int> action = _ => Play(component, UIAudioEventType.DropdownChanged);
            component.onValueChanged.RemoveListener(action);
            component.onValueChanged.AddListener(action);
            tmpDropdownHandlers[component] = action;

            BindPointerForwarder(component.gameObject, UIAudioEventType.DropdownOpen);
        }
    }

    private void BindDropdowns()
    {
        Dropdown[] dropdowns = GetComponentsInChildren<Dropdown>(includeInactive);
        for (int i = 0; i < dropdowns.Length; i++)
        {
            Dropdown component = dropdowns[i];
            UnityAction<int> action = _ => Play(component, UIAudioEventType.DropdownChanged);
            component.onValueChanged.RemoveListener(action);
            component.onValueChanged.AddListener(action);
            dropdownHandlers[component] = action;

            BindPointerForwarder(component.gameObject, UIAudioEventType.DropdownOpen);
        }
    }

    private void BindPointerForwarder(GameObject target, UIAudioEventType eventType)
    {
        if (target == null || audioPlayer == null)
        {
            return;
        }

        UIDropdownPointerSfxForwarder forwarder = target.GetComponent<UIDropdownPointerSfxForwarder>();
        if (forwarder == null)
        {
            forwarder = target.AddComponent<UIDropdownPointerSfxForwarder>();
        }

        forwarder.Configure(audioPlayer, eventType);
        if (!pointerForwarders.Contains(forwarder))
        {
            pointerForwarders.Add(forwarder);
        }
    }

    private void BindToggles()
    {
        Toggle[] toggles = GetComponentsInChildren<Toggle>(includeInactive);
        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle component = toggles[i];
            UnityAction<bool> action = isOn => Play(component, isOn ? UIAudioEventType.ToggleOn : UIAudioEventType.ToggleOff);
            component.onValueChanged.RemoveListener(action);
            component.onValueChanged.AddListener(action);
            toggleHandlers[component] = action;
        }
    }

    private void BindSliders()
    {
        Slider[] sliders = GetComponentsInChildren<Slider>(includeInactive);
        for (int i = 0; i < sliders.Length; i++)
        {
            Slider component = sliders[i];
            UnityAction<float> action = _ => Play(component, UIAudioEventType.SliderChanged);
            component.onValueChanged.RemoveListener(action);
            component.onValueChanged.AddListener(action);
            sliderHandlers[component] = action;
        }
    }

    private void BindTMPInputFields()
    {
        TMP_InputField[] inputFields = GetComponentsInChildren<TMP_InputField>(includeInactive);
        for (int i = 0; i < inputFields.Length; i++)
        {
            TMP_InputField component = inputFields[i];
            BindPointerForwarder(component.gameObject, UIAudioEventType.InputFocus);
        }
    }

    private void BindInputFields()
    {
        InputField[] inputFields = GetComponentsInChildren<InputField>(includeInactive);
        for (int i = 0; i < inputFields.Length; i++)
        {
            InputField component = inputFields[i];
            BindPointerForwarder(component.gameObject, UIAudioEventType.InputFocus);
        }
    }

    private void Play(Component _, UIAudioEventType eventType)
    {
        if (audioPlayer == null)
        {
            return;
        }

        audioPlayer.Play(eventType);
    }
}
