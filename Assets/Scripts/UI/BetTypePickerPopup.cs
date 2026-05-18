using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Popup panel that appears when a number cell is clicked on the bet board.
/// Shows valid bet type options (Straight, Split, Street, Corner, SixLine) for that number.
/// </summary>
public class BetTypePickerPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button cancelButton;

    public event Action<RouletteNeighborCalculator.BetOption> OptionSelected;

    private readonly List<Button> optionButtons = new List<Button>();
    private readonly Dictionary<Button, UnityAction> popupButtonHandlers = new Dictionary<Button, UnityAction>();

    private void Awake()
    {
        CacheOptionButtons();
        SetOverlayVisible(false);

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Hide);
        }
    }

    /// <summary>
    /// Shows the popup for the given roulette number with all valid bet options.
    /// </summary>
    public void Show(int number)
    {
        List<RouletteNeighborCalculator.BetOption> options = RouletteNeighborCalculator.GetOptions(number);
        Show(number, options);
    }

    public void Show(int number, List<RouletteNeighborCalculator.BetOption> options)
    {
        CacheOptionButtons();

        if (titleText != null)
        {
            titleText.text = number == 0 ? "Sayı 0 için bahis tipi seç" : $"Sayı {number} için bahis tipi seç";
        }

        int optionCount = options != null ? options.Count : 0;
        for (int i = 0; i < optionButtons.Count; i++)
        {
            Button button = optionButtons[i];
            if (button == null)
            {
                continue;
            }

            if (i < optionCount && options[i] != null)
            {
                RouletteNeighborCalculator.BetOption option = options[i];
                button.gameObject.SetActive(true);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = option.label;
                }

                SetPopupButtonHandler(button, () =>
                {
                    Hide();
                    OptionSelected?.Invoke(option);
                });
            }
            else
            {
                ClearPopupButtonHandler(button);
                button.gameObject.SetActive(false);
            }
        }

        SetOverlayVisible(true);
    }

    public void Hide()
    {
        SetOverlayVisible(false);
    }

    private void CacheOptionButtons()
    {
        if (optionsContainer == null)
        {
            return;
        }

        optionButtons.Clear();
        for (int i = 0; i < optionsContainer.childCount; i++)
        {
            Button button = optionsContainer.GetChild(i).GetComponent<Button>();
            if (button != null && !optionButtons.Contains(button))
            {
                optionButtons.Add(button);
            }
        }
    }

    private void SetOverlayVisible(bool visible)
    {
        GameObject target = overlayRoot;
        if (target == null && transform.childCount > 0)
        {
            target = transform.GetChild(0).gameObject;
        }

        if (target != null)
        {
            target.SetActive(visible);
        }
    }

    private void SetPopupButtonHandler(Button button, UnityAction handler)
    {
        if (button == null)
        {
            return;
        }

        ClearPopupButtonHandler(button);
        button.onClick.AddListener(handler);
        popupButtonHandlers[button] = handler;
    }

    private void ClearPopupButtonHandler(Button button)
    {
        if (button == null)
        {
            return;
        }

        if (popupButtonHandlers.TryGetValue(button, out UnityAction oldHandler))
        {
            button.onClick.RemoveListener(oldHandler);
            popupButtonHandlers.Remove(button);
        }
    }
}
