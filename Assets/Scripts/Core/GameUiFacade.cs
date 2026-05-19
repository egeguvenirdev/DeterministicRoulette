using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUiFacade : MonoBehaviour
{
    [Header("Gameplay Dependencies")]
    [SerializeField] private OutcomeSelector outcomeSelector;
    [SerializeField] private BetManager betManager;
    [SerializeField] private RouletteGameManager gameManager;
    [SerializeField] private StatisticsManager statisticsManager;
    [SerializeField] private RouletteRulesDatabase rulesDatabase;

    private RouletteGameFlowService flowService;
    private bool roundCompletedBound;
    private bool statisticsChipsBound;

    public event Action<RoundResultData> RoundCompleted;
    public event Action<int> ChipsChanged;

    public bool IsReady => flowService != null && gameManager != null;

    private void Awake()
    {
        BuildFlowService();
    }

    private void OnEnable()
    {
        BuildFlowService();
        BindRoundCompleted();
        BindStatisticsEvents();
    }

    private void OnDisable()
    {
        UnbindRoundCompleted();
        UnbindStatisticsEvents();
    }

    public bool CanSpin()
    {
        return IsReady && gameManager.CanSpin();
    }

    public RoundResultData ExecuteSpin()
    {
        if (!CanSpin())
        {
            return null;
        }

        return flowService.ExecuteSpin();
    }

    public bool TryAddStraightBet(int targetNumber, int stake)
    {
        if (!IsReady)
        {
            return false;
        }

        return flowService.TryAddStraightBet(targetNumber, stake);
    }

    public bool TryAddBet(BetType betType, int stake, int targetNumber = -1, List<int> targetNumbers = null)
    {
        if (!IsReady)
        {
            return false;
        }

        return flowService.TryAddBet(betType, stake, targetNumber, targetNumbers);
    }

    public bool TryRemoveBet(BetType betType, int targetNumber = -1, List<int> targetNumbers = null)
    {
        if (!IsReady)
        {
            return false;
        }

        return flowService.TryRemoveBet(betType, targetNumber, targetNumbers);
    }

    public void SetOutcomeSelection(int targetNumber)
    {
        if (!IsReady)
        {
            return;
        }

        flowService.SetOutcomeSelection(targetNumber);
    }

    public void SetOutcomeSelectionPreset(OutcomeSelectionPreset preset)
    {
        if (!IsReady)
        {
            return;
        }

        flowService.SetOutcomeSelectionPreset(preset);
    }

    public void ClearOutcomeSelection()
    {
        if (!IsReady)
        {
            return;
        }

        flowService.ClearOutcomeSelection();
    }

    public void ClearAllBets()
    {
        if (!IsReady)
        {
            return;
        }

        flowService.ClearAllBets();
    }

    public void AddChips(int amount)
    {
        if (!IsReady)
        {
            return;
        }

        flowService.AddChips(amount);
    }

    public int GetActiveBetCount()
    {
        if (!IsReady)
        {
            return 0;
        }

        return flowService.GetActiveBetCount();
    }

    public int GetTotalStake()
    {
        if (!IsReady)
        {
            return 0;
        }

        return flowService.GetTotalStake();
    }

    public List<BetData> GetActiveBetSnapshot()
    {
        if (!IsReady)
        {
            return new List<BetData>();
        }

        return flowService.GetActiveBetSnapshot();
    }

    public GameStateData GetGameState()
    {
        if (!IsReady)
        {
            return null;
        }

        return flowService.GetGameState();
    }

    public string GetMissingDependencies()
    {
        List<string> missing = new List<string>();

        if (outcomeSelector == null)
        {
            missing.Add(nameof(outcomeSelector));
        }

        if (betManager == null)
        {
            missing.Add(nameof(betManager));
        }

        if (gameManager == null)
        {
            missing.Add(nameof(gameManager));
        }

        if (statisticsManager == null)
        {
            missing.Add(nameof(statisticsManager));
        }

        if (rulesDatabase == null)
        {
            missing.Add(nameof(rulesDatabase));
        }

        return string.Join(", ", missing);
    }

    private void BuildFlowService()
    {
        if (outcomeSelector == null || betManager == null || gameManager == null || statisticsManager == null || rulesDatabase == null)
        {
            flowService = null;
            return;
        }

        // Inject rules database into services before building flow service
        outcomeSelector.SetRulesDatabase(rulesDatabase);
        betManager.SetRulesDatabase(rulesDatabase);

        flowService = new RouletteGameFlowService(betManager, gameManager, outcomeSelector, statisticsManager);
    }

    private void BindRoundCompleted()
    {
        if (gameManager == null || roundCompletedBound)
        {
            return;
        }

        gameManager.RoundCompleted -= HandleRoundCompleted;
        gameManager.RoundCompleted += HandleRoundCompleted;
        roundCompletedBound = true;
    }

    private void UnbindRoundCompleted()
    {
        if (gameManager == null || !roundCompletedBound)
        {
            return;
        }

        gameManager.RoundCompleted -= HandleRoundCompleted;
        roundCompletedBound = false;
    }

    private void HandleRoundCompleted(RoundResultData roundResult)
    {
        RoundCompleted?.Invoke(roundResult);
    }

    private void BindStatisticsEvents()
    {
        if (statisticsManager == null || statisticsChipsBound)
        {
            return;
        }

        statisticsManager.ChipsChanged -= HandleStatisticsChipsChanged;
        statisticsManager.ChipsChanged += HandleStatisticsChipsChanged;
        statisticsChipsBound = true;
    }

    private void UnbindStatisticsEvents()
    {
        if (statisticsManager == null || !statisticsChipsBound)
        {
            return;
        }

        statisticsManager.ChipsChanged -= HandleStatisticsChipsChanged;
        statisticsChipsBound = false;
    }

    private void HandleStatisticsChipsChanged(int totalChips)
    {
        ChipsChanged?.Invoke(totalChips);
    }
}
