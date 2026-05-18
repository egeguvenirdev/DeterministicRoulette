using UnityEngine;
using TMPro;

/// <summary>
/// Manages target number outcome selection via dropdown.
/// Handles dropdown setup, selection apply, and selection clear.
/// </summary>
public class OutcomeSelectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetNumberDropdown;
    [SerializeField] private GameUiFacade gameFacade;

    private void Awake()
    {
        SetupDropdown();
    }

    public void Initialize(GameUiFacade facade)
    {
        gameFacade = facade;
    }

    public void ApplySelection()
    {
        if (targetNumberDropdown == null || gameFacade == null)
        {
            return;
        }

        if (targetNumberDropdown.value == 0)
        {
            gameFacade.ClearOutcomeSelection();
            return;
        }

        gameFacade.SetOutcomeSelection(targetNumberDropdown.value - 1);
    }

    public void ClearSelection()
    {
        if (gameFacade != null)
        {
            gameFacade.ClearOutcomeSelection();
        }

        if (targetNumberDropdown != null)
        {
            targetNumberDropdown.SetValueWithoutNotify(0);
            targetNumberDropdown.RefreshShownValue();
        }
    }

    private void SetupDropdown()
    {
        if (targetNumberDropdown == null)
        {
            return;
        }

        targetNumberDropdown.ClearOptions();
        targetNumberDropdown.AddOptions(BuildNumberOptions(true));
        targetNumberDropdown.SetValueWithoutNotify(0);
        targetNumberDropdown.RefreshShownValue();

        if (targetNumberDropdown.captionText != null && targetNumberDropdown.options.Count > 0)
        {
            int selectedIndex = Mathf.Clamp(targetNumberDropdown.value, 0, targetNumberDropdown.options.Count - 1);
            targetNumberDropdown.captionText.text = targetNumberDropdown.options[selectedIndex].text;
        }
    }

    private static System.Collections.Generic.List<TMP_Dropdown.OptionData> BuildNumberOptions(bool includeRandom)
    {
        System.Collections.Generic.List<TMP_Dropdown.OptionData> options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

        if (includeRandom)
        {
            options.Add(new TMP_Dropdown.OptionData("Random"));
        }

        for (int number = 0; number <= 36; number++)
        {
            options.Add(new TMP_Dropdown.OptionData(number.ToString()));
        }

        return options;
    }
}
