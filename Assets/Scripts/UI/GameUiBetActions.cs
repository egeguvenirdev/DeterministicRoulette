using UnityEngine;

public sealed class GameUiBetActions
{
    private readonly GameUiFacade gameFacade;
    private readonly StakeInputHandler stakeInputHandler;
    private readonly Object logContext;

    public GameUiBetActions(GameUiFacade gameFacade, StakeInputHandler stakeInputHandler, Object logContext)
    {
        this.gameFacade = gameFacade;
        this.stakeInputHandler = stakeInputHandler;
        this.logContext = logContext;
    }

    public bool TryAddBetForCell(bool initialized, RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return false;
        }

        if (!IsReadyForBetActions(initialized))
        {
            return false;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("[GameUIController] Invalid stake.");
            return false;
        }

        if (cell.BetType == BetType.Straight && (cell.Number < 0 || cell.Number > 36))
        {
            Debug.LogWarning("[GameUIController] Target number out of range.");
            return false;
        }

        if (!gameFacade.TryAddBet(cell.BetType, stake, cell.Number, cell.TargetNumbers))
        {
            Debug.LogWarning("[GameUIController] Bet rejected.");
            return false;
        }

        return true;
    }

    public bool TryRemoveBetForCell(bool initialized, RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return false;
        }

        if (!IsReadyForBetActions(initialized))
        {
            return false;
        }

        return gameFacade.TryRemoveBet(cell.BetType, cell.Number, cell.TargetNumbers);
    }

    public bool TryAddBetFromOption(bool initialized, RouletteNeighborCalculator.BetOption option)
    {
        if (option == null || option.targetNumbers == null || option.targetNumbers.Count == 0)
        {
            return false;
        }

        if (!IsReadyForBetActions(initialized))
        {
            return false;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("[GameUIController] Invalid stake.");
            return false;
        }

        int targetNumber = option.betType == BetType.Straight ? option.targetNumbers[0] : -1;

        if (!gameFacade.TryAddBet(option.betType, stake, targetNumber, option.targetNumbers))
        {
            Debug.LogWarning("[GameUIController] Bet from option rejected.");
            return false;
        }

        return true;
    }

    public bool TryRemoveBetFromOption(bool initialized, RouletteNeighborCalculator.BetOption option)
    {
        if (option == null || option.targetNumbers == null || option.targetNumbers.Count == 0)
        {
            return false;
        }

        if (!IsReadyForBetActions(initialized))
        {
            return false;
        }

        return gameFacade.TryRemoveBet(option.betType, -1, option.targetNumbers);
    }

    private bool TryGetStake(out int stake)
    {
        if (stakeInputHandler != null)
        {
            return stakeInputHandler.TryGetStake(out stake);
        }

        stake = 0;
        return false;
    }

    private bool IsReadyForBetActions(bool initialized)
    {
        if (initialized && gameFacade != null && gameFacade.IsReady)
        {
            return true;
        }

        Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", logContext);
        return false;
    }
}
