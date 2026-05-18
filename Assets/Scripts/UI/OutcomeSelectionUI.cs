using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages target number outcome selection via dropdown.
/// Handles dropdown setup, selection apply, and selection clear.
/// </summary>
public class OutcomeSelectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetNumberDropdown;
    [SerializeField] private GameUiFacade gameFacade;

    private readonly List<SelectionEntry> selectionEntries = new List<SelectionEntry>();

    private struct SelectionEntry
    {
        public OutcomeSelectionPreset preset;
        public int number;
    }

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

        int selectedIndex = Mathf.Clamp(targetNumberDropdown.value, 0, selectionEntries.Count - 1);
        if (selectionEntries.Count == 0)
        {
            gameFacade.ClearOutcomeSelection();
            return;
        }

        SelectionEntry entry = selectionEntries[selectedIndex];
        if (entry.preset == OutcomeSelectionPreset.Random)
        {
            gameFacade.ClearOutcomeSelection();
            return;
        }

        if (entry.preset == OutcomeSelectionPreset.ExactNumber)
        {
            gameFacade.SetOutcomeSelection(entry.number);
            return;
        }

        gameFacade.SetOutcomeSelectionPreset(entry.preset);
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

        selectionEntries.Clear();
        targetNumberDropdown.ClearOptions();
        targetNumberDropdown.AddOptions(BuildOptions());
        targetNumberDropdown.SetValueWithoutNotify(0);
        targetNumberDropdown.RefreshShownValue();

        if (targetNumberDropdown.captionText != null && targetNumberDropdown.options.Count > 0)
        {
            int selectedIndex = Mathf.Clamp(targetNumberDropdown.value, 0, targetNumberDropdown.options.Count - 1);
            targetNumberDropdown.captionText.text = targetNumberDropdown.options[selectedIndex].text;
        }
    }

    private List<TMP_Dropdown.OptionData> BuildOptions()
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        AddOption(options, "Random", OutcomeSelectionPreset.Random, -1);
        AddOption(options, "Red", OutcomeSelectionPreset.Red, -1);
        AddOption(options, "Black", OutcomeSelectionPreset.Black, -1);
        AddOption(options, "Even", OutcomeSelectionPreset.Even, -1);
        AddOption(options, "Odd", OutcomeSelectionPreset.Odd, -1);
        AddOption(options, "Low (1-18)", OutcomeSelectionPreset.Low, -1);
        AddOption(options, "High (19-36)", OutcomeSelectionPreset.High, -1);

        for (int number = 0; number <= 36; number++)
        {
            AddOption(options, number.ToString(), OutcomeSelectionPreset.ExactNumber, number);
        }

        return options;
    }

    private void AddOption(List<TMP_Dropdown.OptionData> options, string label, OutcomeSelectionPreset preset, int number)
    {
        options.Add(new TMP_Dropdown.OptionData(label));
        selectionEntries.Add(new SelectionEntry
        {
            preset = preset,
            number = number
        });
    }
}
