using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    [SerializeField] private int startingChips = 1000;

    private GameStateData gameState;

    public GameStateData CurrentState => gameState;

    private void Awake()
    {
        gameState = new GameStateData
        {
            totalChips = startingChips
        };
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && gameState.totalChips >= amount;
    }

    public void ApplyRound(RoundResultData roundResult, int totalStake)
    {
        if (roundResult == null)
        {
            return;
        }

        gameState.spinsPlayed++;
        gameState.totalChips -= totalStake;
        gameState.totalChips += roundResult.totalWinnings;
        gameState.netProfit += roundResult.totalWinnings - totalStake;

        if (roundResult.totalWinnings > totalStake)
        {
            gameState.totalWins++;
        }
        else
        {
            gameState.totalLosses++;
        }

        gameState.roundHistory.Add(roundResult);
    }
}