using UnityEngine;

public class OutcomeSelector : MonoBehaviour, IOutcomeService
{
    private RouletteRulesDatabase rulesDb;
    private int selectedNumber = -1;

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
        }
    }
    
    public void ClearSelection()
    {
        selectedNumber = -1;
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
}
