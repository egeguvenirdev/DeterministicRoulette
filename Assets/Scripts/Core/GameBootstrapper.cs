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
        if (!HasRequiredReferences())
        {
            Debug.LogWarning($"[Bootstrapper] Missing references. Bootstrap skipped (manual inspector wiring mode). Missing: {GetMissingReferences()}", this);
            return;
        }

        if (!WireDependencies())
        {
            Debug.LogWarning("[Bootstrapper] Failed to wire dependencies. Bootstrap skipped (manual inspector wiring mode).", this);
            return;
        }

        gameUIController.Initialize(outcomeSelector, betManager, gameManager, statisticsManager);
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

    private string GetMissingReferences()
    {
        System.Collections.Generic.List<string> missing = new System.Collections.Generic.List<string>();

        if (rulesDatabase == null)
        {
            missing.Add(nameof(rulesDatabase));
        }

        if (outcomeSelector == null)
        {
            missing.Add(nameof(outcomeSelector));
        }

        if (payoutCalculator == null)
        {
            missing.Add(nameof(payoutCalculator));
        }

        if (betManager == null)
        {
            missing.Add(nameof(betManager));
        }

        if (statisticsManager == null)
        {
            missing.Add(nameof(statisticsManager));
        }

        if (gameManager == null)
        {
            missing.Add(nameof(gameManager));
        }

        if (gameUIController == null)
        {
            missing.Add(nameof(gameUIController));
        }

        return string.Join(", ", missing);
    }
}