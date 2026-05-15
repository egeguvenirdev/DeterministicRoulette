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

    private void Awake()
    {
        ResolveReferences();

        if (gameManager != null)
        {
            gameManager.Configure(outcomeSelector, payoutCalculator, betManager, statisticsManager);
        }

        if (gameUIController != null)
        {
            gameUIController.Initialize(outcomeSelector, betManager, gameManager, statisticsManager);
        }
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
}