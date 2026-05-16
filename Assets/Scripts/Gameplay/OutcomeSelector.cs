using UnityEngine;

public class OutcomeSelector : MonoBehaviour, IOutcomeService
{
    private RouletteRulesDatabase rulesDb;
    private int selectedNumber = -1;
    
    private void Start()
    {
        rulesDb = RouletteRulesDatabase.Instance;
    }
    
    public void SetSelectedNumber(int number)
    {
        if (number >= 0 && number <= 36)
        {
            selectedNumber = number;
        }
    }
    
    public void ClearSelection()
    {
        selectedNumber = -1;
    }
    
    public int GetOutcome()
    {
        EnsureRulesDatabase();

        if (rulesDb == null)
        {
            return 0;
        }

        if (selectedNumber >= 0)
        {
            return selectedNumber;
        }
        
        // random if not selected
        var allNumbers = rulesDb.GetAllNumbers();
        return allNumbers[Random.Range(0, allNumbers.Count)];
    }
    
    public int GetSelectedNumber()
    {
        return selectedNumber;
    }
    
    public bool HasSelection()
    {
        return selectedNumber >= 0;
    }

    private void EnsureRulesDatabase()
    {
        if (rulesDb == null)
        {
            rulesDb = RouletteRulesDatabase.Instance;
        }
    }
}
