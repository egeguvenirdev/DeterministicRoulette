using UnityEngine;
using System.Collections.Generic;

public class PayoutCalculator : MonoBehaviour, IPayoutService
{
    private RouletteRulesDatabase rulesDb;

    public void SetRulesDatabase(RouletteRulesDatabase db)
    {
        if (db == null)
        {
            Debug.LogWarning("[PayoutCalculator] Attempted to set null rules database.", this);
            return;
        }
        rulesDb = db;
    }
    
    public int CalculateWinnings(int resultNumber, List<BetData> bets)
    {
        if (rulesDb == null)
        {
            Debug.LogError("[PayoutCalculator] Rules database not initialized. Call SetRulesDatabase first.", this);
            return 0;
        }

        int totalWinnings = 0;
        var resultData = rulesDb.GetNumberData(resultNumber);
        
        foreach (var bet in bets)
        {
            if (IsBetWinning(bet, resultNumber, resultData))
            {
                int betWinnings = (int)(bet.amount * (1 + rulesDb.GetPayoutRate(bet.betType)));
                totalWinnings += betWinnings;
            }
        }
        
        return totalWinnings;
    }
    
    public List<BetData> GetWinningBets(int resultNumber, List<BetData> bets)
    {
        if (rulesDb == null)
        {
            Debug.LogError("[PayoutCalculator] Rules database not initialized. Call SetRulesDatabase first.", this);
            return new List<BetData>();
        }

        var winning = new List<BetData>();
        var resultData = rulesDb.GetNumberData(resultNumber);
        
        foreach (var bet in bets)
        {
            if (IsBetWinning(bet, resultNumber, resultData))
            {
                winning.Add(bet);
            }
        }
        
        return winning;
    }
    
    private bool IsBetWinning(BetData bet, int resultNumber, RouletteNumberData resultData)
    {
        if (resultData == null) return false;
        
        switch (bet.betType)
        {
            case BetType.Straight:
                return resultNumber == bet.targetNumber;
            
            case BetType.Red:
                return resultData.color == NumberColor.Red;
            
            case BetType.Black:
                return resultData.color == NumberColor.Black;
            
            case BetType.Even:
                return resultData.isEven && resultNumber != 0;
            
            case BetType.Odd:
                return !resultData.isEven && resultNumber != 0;
            
            case BetType.High:
                return resultData.isHigh;
            
            case BetType.Low:
                return !resultData.isHigh && resultNumber != 0;
            
            case BetType.Dozen1:
                return resultData.dozen == 0 && resultNumber != 0;
            
            case BetType.Dozen2:
                return resultData.dozen == 1;
            
            case BetType.Dozen3:
                return resultData.dozen == 2;
            
            case BetType.Column1:
                return resultData.column == 0 && resultNumber != 0;
            
            case BetType.Column2:
                return resultData.column == 1 && resultNumber != 0;
            
            case BetType.Column3:
                return resultData.column == 2 && resultNumber != 0;

            case BetType.Split:
            case BetType.Street:
            case BetType.Corner:
            case BetType.SixLine:
                return bet.targetNumbers != null && bet.targetNumbers.Contains(resultNumber);
            
            default:
                return false;
        }
    }
}
