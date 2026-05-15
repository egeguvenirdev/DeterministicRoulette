using UnityEngine;
using System;
using System.Collections.Generic;

public enum RouletteType
{
    European,   // 0-36
    American    // 0, 00, 1-36
}

public enum BetType
{
    // Inside Bets
    Straight,
    Split,
    Street,
    Corner,
    SixLine,
    
    // Outside Bets
    Red,
    Black,
    Even,
    Odd,
    High,
    Low,
    Dozen1,
    Dozen2,
    Dozen3,
    Column1,
    Column2,
    Column3
}

public enum NumberColor
{
    Red,
    Black,
    Green
}

[System.Serializable]
public class RouletteNumberData
{
    public int number;
    public NumberColor color;
    public bool isEven;
    public bool isHigh;
    public int column;
    public int dozen;
    
    public RouletteNumberData(int num, NumberColor col, bool even, bool high, int col_idx, int dozen_idx)
    {
        number = num;
        color = col;
        isEven = even;
        isHigh = high;
        column = col_idx;
        dozen = dozen_idx;
    }
}

[System.Serializable]
public class BetData
{
    public BetType betType;
    public int targetNumber;
    public List<int> targetNumbers;
    public int chipValue;
    public int amount;
    public float payout;
    
    public BetData()
    {
        targetNumbers = new List<int>();
    }
}

[System.Serializable]
public class RoundResultData
{
    public int selectedNumber;
    public int resultNumber;
    public List<BetData> winningBets;
    public List<BetData> losingBets;
    public int totalWinnings;
    public int totalLosses;
    public DateTime timestamp;
    
    public RoundResultData()
    {
        winningBets = new List<BetData>();
        losingBets = new List<BetData>();
        timestamp = DateTime.Now;
    }
}

[System.Serializable]
public class GameStateData
{
    public int totalChips;
    public int spinsPlayed;
    public int totalWins;
    public int totalLosses;
    public int netProfit;
    public RouletteType rouletteType;
    public List<RoundResultData> roundHistory;
    
    public GameStateData()
    {
        totalChips = 1000;
        spinsPlayed = 0;
        totalWins = 0;
        totalLosses = 0;
        netProfit = 0;
        rouletteType = RouletteType.European;
        roundHistory = new List<RoundResultData>();
    }
}
