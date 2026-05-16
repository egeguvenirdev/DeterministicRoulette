using System;
using UnityEngine;

public class RouletteGameManager : MonoBehaviour
{
    [SerializeField] private OutcomeSelector outcomeSelector;
    [SerializeField] private PayoutCalculator payoutCalculator;
    [SerializeField] private BetManager betManager;
    [SerializeField] private StatisticsManager statisticsManager;

    private IOutcomeService configuredOutcomeService;
    private IPayoutService configuredPayoutService;
    private IBetService configuredBetService;
    private IStatisticsService configuredStatisticsService;
    private RouletteRoundService roundService;

    public event Action<RoundResultData> RoundCompleted;

    private void Awake()
    {
        BuildRoundServiceFromSerializedReferences();
    }

    public void Configure(
        IOutcomeService selector,
        IPayoutService calculator,
        IBetService manager,
        IStatisticsService stats)
    {
        configuredOutcomeService = selector;
        configuredPayoutService = calculator;
        configuredBetService = manager;
        configuredStatisticsService = stats;
        roundService = new RouletteRoundService(configuredOutcomeService, configuredPayoutService, configuredBetService, configuredStatisticsService);
    }

    public bool CanSpin()
    {
        EnsureRoundService();
        return roundService != null && roundService.CanSpin();
    }

    public RoundResultData Spin()
    {
        EnsureRoundService();

        if (roundService == null)
        {
            return null;
        }

        RoundResultData roundResult = roundService.Spin();

        if (roundResult == null)
        {
            return null;
        }

        RoundCompleted?.Invoke(roundResult);
        return roundResult;
    }

    private void EnsureRoundService()
    {
        if (roundService != null)
        {
            return;
        }

        if (configuredOutcomeService != null &&
            configuredPayoutService != null &&
            configuredBetService != null &&
            configuredStatisticsService != null)
        {
            roundService = new RouletteRoundService(configuredOutcomeService, configuredPayoutService, configuredBetService, configuredStatisticsService);
            return;
        }

        BuildRoundServiceFromSerializedReferences();
    }

    private void BuildRoundServiceFromSerializedReferences()
    {
        if (outcomeSelector == null || payoutCalculator == null || betManager == null || statisticsManager == null)
        {
            return;
        }

        configuredOutcomeService = outcomeSelector;
        configuredPayoutService = payoutCalculator;
        configuredBetService = betManager;
        configuredStatisticsService = statisticsManager;
        roundService = new RouletteRoundService(configuredOutcomeService, configuredPayoutService, configuredBetService, configuredStatisticsService);
    }
}