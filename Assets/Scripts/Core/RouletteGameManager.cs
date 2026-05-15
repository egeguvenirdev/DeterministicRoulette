using System;
using System.Collections.Generic;
using UnityEngine;

public class RouletteGameManager : MonoBehaviour
{
    [SerializeField] private OutcomeSelector outcomeSelector;
    [SerializeField] private PayoutCalculator payoutCalculator;
    [SerializeField] private BetManager betManager;
    [SerializeField] private StatisticsManager statisticsManager;

    public event Action<RoundResultData> RoundCompleted;

    public bool CanSpin()
    {
        if (outcomeSelector == null || payoutCalculator == null || betManager == null || statisticsManager == null)
        {
            return false;
        }

        int totalStake = betManager.GetTotalStake();
        return totalStake > 0 && statisticsManager.CanAfford(totalStake);
    }

    public RoundResultData Spin()
    {
        if (!CanSpin())
        {
            return null;
        }

        int selectedNumber = outcomeSelector.HasSelection() ? outcomeSelector.GetSelectedNumber() : -1;
        int resultNumber = outcomeSelector.GetOutcome();
        List<BetData> currentBets = betManager.GetBetSnapshot();
        List<BetData> winningBets = payoutCalculator.GetWinningBets(resultNumber, currentBets);
        int totalStake = betManager.GetTotalStake();
        int totalWinnings = payoutCalculator.CalculateWinnings(resultNumber, currentBets);

        RoundResultData roundResult = new RoundResultData
        {
            selectedNumber = selectedNumber,
            resultNumber = resultNumber,
            winningBets = winningBets,
            losingBets = GetLosingBets(currentBets, winningBets),
            totalWinnings = totalWinnings,
            totalLosses = Mathf.Max(0, totalStake - totalWinnings)
        };

        statisticsManager.ApplyRound(roundResult, totalStake);
        betManager.ClearBets();
        RoundCompleted?.Invoke(roundResult);
        return roundResult;
    }

    private List<BetData> GetLosingBets(List<BetData> allBets, List<BetData> winningBets)
    {
        List<BetData> losingBets = new List<BetData>(allBets);

        for (int i = 0; i < winningBets.Count; i++)
        {
            losingBets.Remove(winningBets[i]);
        }

        return losingBets;
    }
}