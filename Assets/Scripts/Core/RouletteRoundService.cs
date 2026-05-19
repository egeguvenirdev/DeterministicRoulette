using System.Collections.Generic;
using UnityEngine;

public sealed class RouletteRoundService
{
    private readonly IOutcomeService outcomeService;
    private readonly IPayoutService payoutService;
    private readonly IBetService betService;
    private readonly IStatisticsService statisticsService;

    public RouletteRoundService(
        IOutcomeService outcomeService,
        IPayoutService payoutService,
        IBetService betService,
        IStatisticsService statisticsService)
    {
        this.outcomeService = outcomeService;
        this.payoutService = payoutService;
        this.betService = betService;
        this.statisticsService = statisticsService;
    }

    public bool CanSpin()
    {
        if (outcomeService == null || payoutService == null || betService == null || statisticsService == null)
        {
            return false;
        }

        int totalStake = betService.GetTotalStake();
        return totalStake > 0 && statisticsService.CanAfford(totalStake);
    }

    public RoundResultData Spin()
    {
        if (!CanSpin())
        {
            return null;
        }

        int selectedNumber = outcomeService.HasSelection() ? outcomeService.GetSelectedNumber() : -1;
        string selectedOutcomeLabel = outcomeService.HasSelection() ? outcomeService.GetSelectionLabel() : string.Empty;
        int resultNumber = outcomeService.GetOutcome();
        List<BetData> currentBets = betService.GetBetSnapshot();
        List<BetData> winningBets = payoutService.GetWinningBets(resultNumber, currentBets);
        int totalStake = betService.GetTotalStake();
        int totalWinnings = payoutService.CalculateWinnings(resultNumber, currentBets);

        RoundResultData roundResult = new RoundResultData
        {
            selectedNumber = selectedNumber,
            selectedOutcomeLabel = selectedOutcomeLabel,
            resultNumber = resultNumber,
            winningBets = winningBets,
            losingBets = GetLosingBets(currentBets, winningBets),
            totalWinnings = totalWinnings,
            totalLosses = Mathf.Max(0, totalStake - totalWinnings)
        };

        statisticsService.ApplyRound(roundResult, totalStake);
        betService.ClearBets();
        return roundResult;
    }

    private static List<BetData> GetLosingBets(List<BetData> allBets, List<BetData> winningBets)
    {
        List<BetData> losingBets = new List<BetData>(allBets);

        for (int i = 0; i < winningBets.Count; i++)
        {
            losingBets.Remove(winningBets[i]);
        }

        return losingBets;
    }
}
