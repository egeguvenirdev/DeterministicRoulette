using System.Collections.Generic;

/// <summary>
/// Handles game flow orchestration: bet management, spin execution, outcome tracking.
/// Keeps game logic separate from UI concerns.
/// </summary>
public class RouletteGameFlowService
{
    private readonly BetManager betManager;
    private readonly RouletteGameManager gameManager;
    private readonly OutcomeSelector outcomeSelector;
    private readonly StatisticsManager statisticsManager;

    public RouletteGameFlowService(
        BetManager betManager,
        RouletteGameManager gameManager,
        OutcomeSelector outcomeSelector,
        StatisticsManager statisticsManager)
    {
        this.betManager = betManager;
        this.gameManager = gameManager;
        this.outcomeSelector = outcomeSelector;
        this.statisticsManager = statisticsManager;
    }

    /// <summary>
    /// Attempts to add a straight bet with validation.
    /// Returns true if bet was successfully added.
    /// </summary>
    public bool TryAddStraightBet(int targetNumber, int stake)
    {
        return TryAddBet(BetType.Straight, stake, targetNumber);
    }

    public bool TryAddBet(BetType betType, int stake, int targetNumber = -1, List<int> targetNumbers = null)
    {
        if (stake <= 0)
        {
            return false;
        }

        BetData bet = new BetData
        {
            betType = betType,
            targetNumber = targetNumber,
            amount = stake,
            targetNumbers = targetNumbers != null ? new List<int>(targetNumbers) : new List<int>()
        };

        return betManager.TryAddBet(bet);
    }

    public bool TryRemoveBet(BetType betType, int targetNumber = -1, List<int> targetNumbers = null)
    {
        for (int i = betManager.ActiveBets.Count - 1; i >= 0; i--)
        {
            BetData bet = betManager.ActiveBets[i];
            if (bet == null)
            {
                continue;
            }

            if (bet.betType != betType)
            {
                continue;
            }

            if (targetNumbers != null && targetNumbers.Count > 0)
            {
                if (bet.targetNumbers != null && bet.targetNumbers.Count == targetNumbers.Count
                    && bet.targetNumbers.TrueForAll(n => targetNumbers.Contains(n)))
                {
                    return betManager.RemoveBet(bet);
                }
            }
            else if (bet.targetNumber == targetNumber)
            {
                return betManager.RemoveBet(bet);
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the deterministic outcome for the next spin.
    /// </summary>
    public void SetOutcomeSelection(int targetNumber)
    {
        if (targetNumber < 0 || targetNumber > 36)
        {
            return;
        }

        outcomeSelector.SetSelectedNumber(targetNumber);
    }

    public void SetOutcomeSelectionPreset(OutcomeSelectionPreset preset)
    {
        outcomeSelector.SetSelectionPreset(preset);
    }

    /// <summary>
    /// Clears the deterministic outcome selection (resets to random).
    /// </summary>
    public void ClearOutcomeSelection()
    {
        outcomeSelector.ClearSelection();
    }

    /// <summary>
    /// Clears all active bets.
    /// </summary>
    public void ClearAllBets()
    {
        betManager.ClearBets();
    }

    public void AddChips(int amount)
    {
        statisticsManager.AddChips(amount);
    }

    /// <summary>
    /// Executes a spin with current bets and outcome selection.
    /// Returns null if spin cannot execute (insufficient funds, no bets, etc).
    /// Fires RoundCompleted event on gameManager.
    /// </summary>
    public RoundResultData ExecuteSpin()
    {
        if (!gameManager.CanSpin())
        {
            return null;
        }

        return gameManager.Spin();
    }

    /// <summary>
    /// Gets current active bets count.
    /// </summary>
    public int GetActiveBetCount()
    {
        return betManager.ActiveBets.Count;
    }

    /// <summary>
    /// Gets total stake across all active bets.
    /// </summary>
    public int GetTotalStake()
    {
        return betManager.GetTotalStake();
    }

    public List<BetData> GetActiveBetSnapshot()
    {
        return betManager.GetBetSnapshot();
    }

    /// <summary>
    /// Checks if outcome selection is active.
    /// </summary>
    public bool HasOutcomeSelection()
    {
        return outcomeSelector.HasSelection();
    }

    /// <summary>
    /// Gets current game state (chips, spins, wins/losses).
    /// </summary>
    public GameStateData GetGameState()
    {
        return statisticsManager.CurrentState;
    }
}
