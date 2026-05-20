using UnityEngine;
using System;

public class StatisticsManager : MonoBehaviour, IStatisticsService
{
    [SerializeField] private int startingChips = 1000;

    private GameStateData gameState;

    public event Action<int> ChipsChanged;

    public GameStateData CurrentState
    {
        get
        {
            EnsureState();
            return gameState;
        }
    }

    private void Awake()
    {
        EnsureState();
    }

    public bool CanAfford(int amount)
    {
        EnsureState();
        return amount >= 0 && gameState.totalChips >= amount;
    }

    public void ApplyRound(RoundResultData roundResult, int totalStake)
    {
        EnsureState();

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
        ChipsChanged?.Invoke(gameState.totalChips);
    }

    public void AddChips(int amount)
    {
        EnsureState();

        if (amount <= 0)
        {
            return;
        }

        gameState.totalChips += amount;
        ChipsChanged?.Invoke(gameState.totalChips);
    }

    public void ResetState()
    {
        gameState = new GameStateData
        {
            totalChips = startingChips
        };

        ChipsChanged?.Invoke(gameState.totalChips);
    }

    public void SetState(GameStateData state)
    {
        if (state == null)
        {
            ResetState();
            return;
        }

        gameState = new GameStateData
        {
            totalChips = Mathf.Max(0, state.totalChips),
            spinsPlayed = Mathf.Max(0, state.spinsPlayed),
            totalWins = Mathf.Max(0, state.totalWins),
            totalLosses = Mathf.Max(0, state.totalLosses),
            netProfit = state.netProfit,
            rouletteType = state.rouletteType,
            roundHistory = state.roundHistory != null ? new System.Collections.Generic.List<RoundResultData>(state.roundHistory) : new System.Collections.Generic.List<RoundResultData>()
        };

        ChipsChanged?.Invoke(gameState.totalChips);
    }

    private void EnsureState()
    {
        if (gameState != null)
        {
            return;
        }

        gameState = new GameStateData
        {
            totalChips = startingChips
        };
    }
}