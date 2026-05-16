using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private RouletteRulesDatabase rulesDatabase;
    [SerializeField] private OutcomeSelector outcomeSelector;
    [SerializeField] private PayoutCalculator payoutCalculator;
    [SerializeField] private BetManager betManager;
    [SerializeField] private StatisticsManager statisticsManager;
    [SerializeField] private RouletteGameManager gameManager;

    [Header("UI")]
    [SerializeField] private GameUIController gameUIController;

    private void Start()
    {
        ResolveReferences();

        if (!HasRequiredReferences())
        {
            Debug.LogError("[Bootstrapper] Missing required scene references. Initialization aborted.", this);
            enabled = false;
            return;
        }

        if (!WireDependencies())
        {
            Debug.LogError("[Bootstrapper] Failed to wire dependencies. Initialization aborted.", this);
            enabled = false;
            return;
        }

        gameUIController.Initialize(outcomeSelector, betManager, gameManager, statisticsManager);
    }

    private void ResolveReferences()
    {
        if (rulesDatabase == null)
        {
            rulesDatabase = FindFirstObjectByType<RouletteRulesDatabase>();
        }

        if (outcomeSelector == null)
        {
            outcomeSelector = FindFirstObjectByType<OutcomeSelector>();
        }

        if (payoutCalculator == null)
        {
            payoutCalculator = FindFirstObjectByType<PayoutCalculator>();
        }

        if (betManager == null)
        {
            betManager = FindFirstObjectByType<BetManager>();
        }

        if (statisticsManager == null)
        {
            statisticsManager = FindFirstObjectByType<StatisticsManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<RouletteGameManager>();
        }

        if (gameUIController == null)
        {
            gameUIController = FindFirstObjectByType<GameUIController>();
        }
    }

    private bool HasRequiredReferences()
    {
        return rulesDatabase != null &&
               outcomeSelector != null &&
               payoutCalculator != null &&
               betManager != null &&
               statisticsManager != null &&
               gameManager != null &&
               gameUIController != null;
    }

    private bool WireDependencies()
    {
        IOutcomeService outcomeService = outcomeSelector;
        IPayoutService payoutService = payoutCalculator;
        IBetService betService = betManager;
        IStatisticsService statsService = statisticsManager;

        if (outcomeService == null || payoutService == null || betService == null || statsService == null)
        {
            return false;
        }

        gameManager.Configure(outcomeService, payoutService, betService, statsService);
        return true;
    }
}