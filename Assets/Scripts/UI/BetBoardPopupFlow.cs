using System;

/// <summary>
/// Handles the popup-driven bet placement flow.
/// When the player taps a number cell, a popup shows available bet types;
/// this class maps the selected option back to chip visuals and the game model.
/// </summary>
public sealed class BetBoardPopupFlow
{
    private readonly BetBoardSelectionState selectionState;
    private readonly BetBoardChipVisualService chipVisualService;
    private readonly Func<int, RouletteBetCellView> findNumberCell;
    private readonly Func<RouletteNeighborCalculator.BetOption, bool> tryAddBetFromOption;
    private readonly Action onBetPlaced;

    public BetBoardPopupFlow(
        BetBoardSelectionState selectionState,
        BetBoardChipVisualService chipVisualService,
        Func<int, RouletteBetCellView> findNumberCell,
        Func<RouletteNeighborCalculator.BetOption, bool> tryAddBetFromOption,
        Action onBetPlaced = null)
    {
        this.selectionState = selectionState;
        this.chipVisualService = chipVisualService;
        this.findNumberCell = findNumberCell;
        this.tryAddBetFromOption = tryAddBetFromOption;
        this.onBetPlaced = onBetPlaced;
    }

    /// <summary>Called when the popup fires OptionSelected.</summary>
    public void OnOptionSelected(RouletteNeighborCalculator.BetOption option)
    {
        RouletteBetCellView origin = selectionState?.ConsumePendingOrigin();
        if (origin == null)
        {
            return;
        }

        HandleOptionSelected(origin, option);
    }

    private void HandleOptionSelected(RouletteBetCellView originCell, RouletteNeighborCalculator.BetOption option)
    {
        if (!tryAddBetFromOption(option))
        {
            return;
        }

        // Place chips on every number cell covered by this bet.
        foreach (int number in option.targetNumbers)
        {
            RouletteBetCellView coveredCell = findNumberCell(number);
            if (coveredCell == null)
            {
                continue;
            }

            chipVisualService?.EnsureChipForCell(coveredCell);
            selectionState?.SetCoveredCellOrigin(coveredCell, originCell);
        }

        selectionState?.SetOptionForOrigin(originCell, option);
        onBetPlaced?.Invoke();
    }
}
