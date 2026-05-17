using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RouletteRulesDatabase : MonoBehaviour
{
    private static RouletteRulesDatabase instance;
    
    private Dictionary<int, RouletteNumberData> numberDatabase = new Dictionary<int, RouletteNumberData>();
    private Dictionary<BetType, float> payoutRates = new Dictionary<BetType, float>();
    
    public RouletteType currentRouletteType = RouletteType.European;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRoulette();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeRoulette()
    {
        InitializeEuropeanRoulette();
        InitializePayoutRates();
    }
    
    private void InitializeEuropeanRoulette()
    {
        // European roulette: 0-36
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        var redSet = new HashSet<int>(redNumbers);
        
        // 0 is green
        numberDatabase[0] = new RouletteNumberData(0, NumberColor.Green, false, false, 0, 0);
        
        // 1-36
        for (int i = 1; i <= 36; i++)
        {
            NumberColor color = redSet.Contains(i) ? NumberColor.Red : NumberColor.Black;
            bool isEven = i % 2 == 0;
            bool isHigh = i > 18;
            int column = (i - 1) % 3;
            int dozen = (i - 1) / 12;
            
            numberDatabase[i] = new RouletteNumberData(i, color, isEven, isHigh, column, dozen);
        }
    }
    
    private void InitializePayoutRates()
    {
        payoutRates[BetType.Straight] = 35;
        payoutRates[BetType.Split] = 17;
        payoutRates[BetType.Street] = 11;
        payoutRates[BetType.Corner] = 8;
        payoutRates[BetType.SixLine] = 5;
        payoutRates[BetType.Red] = 1;
        payoutRates[BetType.Black] = 1;
        payoutRates[BetType.Even] = 1;
        payoutRates[BetType.Odd] = 1;
        payoutRates[BetType.High] = 1;
        payoutRates[BetType.Low] = 1;
        payoutRates[BetType.Dozen1] = 2;
        payoutRates[BetType.Dozen2] = 2;
        payoutRates[BetType.Dozen3] = 2;
        payoutRates[BetType.Column1] = 2;
        payoutRates[BetType.Column2] = 2;
        payoutRates[BetType.Column3] = 2;
    }
    
    public RouletteNumberData GetNumberData(int number)
    {
        return numberDatabase.ContainsKey(number) ? numberDatabase[number] : null;
    }
    
    public List<int> GetAllNumbers()
    {
        return numberDatabase.Keys.ToList();
    }
    
    public bool IsNumberRed(int number)
    {
        return numberDatabase.ContainsKey(number) && numberDatabase[number].color == NumberColor.Red;
    }
    
    public bool IsNumberBlack(int number)
    {
        return numberDatabase.ContainsKey(number) && numberDatabase[number].color == NumberColor.Black;
    }
    
    public float GetPayoutRate(BetType betType)
    {
        return payoutRates.ContainsKey(betType) ? payoutRates[betType] : 0;
    }
    
    public bool IsValidBet(BetData bet)
    {
        if (bet == null || bet.amount <= 0) return false;
        
        switch (bet.betType)
        {
            case BetType.Straight:
                return bet.targetNumber >= 0 && bet.targetNumber <= 36;
            case BetType.Red:
            case BetType.Black:
            case BetType.Even:
            case BetType.Odd:
            case BetType.High:
            case BetType.Low:
                return true;
            case BetType.Dozen1:
            case BetType.Dozen2:
            case BetType.Dozen3:
            case BetType.Column1:
            case BetType.Column2:
            case BetType.Column3:
                return true;
            case BetType.Split:
                return bet.targetNumbers != null && bet.targetNumbers.Count == 2
                    && bet.targetNumbers.TrueForAll(n => n >= 0 && n <= 36);
            case BetType.Street:
                return bet.targetNumbers != null && bet.targetNumbers.Count == 3
                    && bet.targetNumbers.TrueForAll(n => n >= 0 && n <= 36);
            case BetType.Corner:
                return bet.targetNumbers != null && bet.targetNumbers.Count == 4
                    && bet.targetNumbers.TrueForAll(n => n >= 0 && n <= 36);
            case BetType.SixLine:
                return bet.targetNumbers != null && bet.targetNumbers.Count == 6
                    && bet.targetNumbers.TrueForAll(n => n >= 0 && n <= 36);
            default:
                return false;
        }
    }
    
    public static RouletteRulesDatabase Instance => instance;
}
