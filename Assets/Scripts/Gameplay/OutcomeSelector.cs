using UnityEngine;
using System.Collections.Generic;

public class OutcomeSelector : MonoBehaviour, IOutcomeService
{
    private RouletteRulesDatabase rulesDb;
    private int selectedNumber = -1;
    private OutcomeSelectionPreset selectionPreset = OutcomeSelectionPreset.Random;

    public void SetRulesDatabase(RouletteRulesDatabase db)
    {
        if (db == null)
        {
            Debug.LogWarning("[OutcomeSelector] Attempted to set null rules database.", this);
            return;
        }
        rulesDb = db;
    }
    
    public void SetSelectedNumber(int number)
    {
        if (number >= 0 && number <= 36)
        {
            selectedNumber = number;
            selectionPreset = OutcomeSelectionPreset.ExactNumber;
        }
    }

    public void SetSelectionPreset(OutcomeSelectionPreset preset)
    {
        if (preset == OutcomeSelectionPreset.ExactNumber)
        {
            return;
        }

        selectionPreset = preset;
        if (preset != OutcomeSelectionPreset.ExactNumber)
        {
            selectedNumber = -1;
        }
    }
    
    public void ClearSelection()
    {
        selectedNumber = -1;
        selectionPreset = OutcomeSelectionPreset.Random;
    }
    
    public int GetOutcome()
    {
        if (rulesDb == null)
        {
            Debug.LogError("[OutcomeSelector] Rules database not initialized. Call SetRulesDatabase first.", this);
            return 0;
        }

        if (selectedNumber >= 0)
        {
            return selectedNumber;
        }

        List<int> allNumbers = rulesDb.GetAllNumbers();
        List<int> candidates = BuildCandidatesFromPreset(allNumbers);
        if (candidates.Count == 0)
        {
            return allNumbers[Random.Range(0, allNumbers.Count)];
        }

        return candidates[Random.Range(0, candidates.Count)];
    }
    
    public int GetSelectedNumber()
    {
        return selectedNumber;
    }

    public string GetSelectionLabel()
    {
        if (selectionPreset == OutcomeSelectionPreset.ExactNumber)
        {
            return selectedNumber >= 0 ? selectedNumber.ToString() : string.Empty;
        }

        switch (selectionPreset)
        {
            case OutcomeSelectionPreset.Red:
                return "Red";
            case OutcomeSelectionPreset.Black:
                return "Black";
            case OutcomeSelectionPreset.Even:
                return "Even";
            case OutcomeSelectionPreset.Odd:
                return "Odd";
            case OutcomeSelectionPreset.Low:
                return "Low";
            case OutcomeSelectionPreset.High:
                return "High";
            default:
                return string.Empty;
        }
    }
    
    public bool HasSelection()
    {
        return selectedNumber >= 0 || selectionPreset != OutcomeSelectionPreset.Random;
    }

    private List<int> BuildCandidatesFromPreset(List<int> allNumbers)
    {
        List<int> candidates = new List<int>();

        switch (selectionPreset)
        {
            case OutcomeSelectionPreset.Red:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (rulesDb.IsNumberRed(number))
                    {
                        candidates.Add(number);
                    }
                }
                break;

            case OutcomeSelectionPreset.Black:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (rulesDb.IsNumberBlack(number))
                    {
                        candidates.Add(number);
                    }
                }
                break;

            case OutcomeSelectionPreset.Even:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (number != 0 && number % 2 == 0)
                    {
                        candidates.Add(number);
                    }
                }
                break;

            case OutcomeSelectionPreset.Odd:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (number % 2 != 0)
                    {
                        candidates.Add(number);
                    }
                }
                break;

            case OutcomeSelectionPreset.Low:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (number >= 1 && number <= 18)
                    {
                        candidates.Add(number);
                    }
                }
                break;

            case OutcomeSelectionPreset.High:
                for (int i = 0; i < allNumbers.Count; i++)
                {
                    int number = allNumbers[i];
                    if (number >= 19 && number <= 36)
                    {
                        candidates.Add(number);
                    }
                }
                break;

            default:
                candidates.AddRange(allNumbers);
                break;
        }

        return candidates;
    }
}
