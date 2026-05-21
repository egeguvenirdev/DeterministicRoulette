using System.Collections.Generic;

/// <summary>
/// Handles the "tap an existing chip to remove the bet" flow.
/// Resolves which origin cell owns the tapped cell, then removes
/// the corresponding bet and all associated chip visuals.
/// </summary>
public sealed class BetBoardRemoveStrategy
{
    private readonly BetBoardSelectionState selectionState;
    private readonly BetBoardChipVisualService chipVisualService;
    private readonly System.Func<RouletteBetCellView, bool> tryRemoveBetForCell;
    private readonly System.Func<RouletteNeighborCalculator.BetOption, bool> tryRemoveBetFromOption;

    public BetBoardRemoveStrategy(
        BetBoardSelectionState selectionState,
        BetBoardChipVisualService chipVisualService,
        System.Func<RouletteBetCellView, bool> tryRemoveBetForCell,
        System.Func<RouletteNeighborCalculator.BetOption, bool> tryRemoveBetFromOption)
    {
        this.selectionState = selectionState;
        this.chipVisualService = chipVisualService;
        this.tryRemoveBetForCell = tryRemoveBetForCell;
        this.tryRemoveBetFromOption = tryRemoveBetFromOption;
    }

    /// <summary>
    /// Returns the origin cell that owns a chip covering the given cell,
    /// or null if no chip exists there.
    /// </summary>
    public RouletteBetCellView GetOriginCell(RouletteBetCellView cell)
    {
        RouletteBetCellView origin = selectionState?.GetOriginForCell(cell);
        if (origin != null)
        {
            return origin;
        }

        // Fallback: outside bets placed before selection tracking was active.
        if (chipVisualService != null && chipVisualService.HasChip(cell))
        {
            return cell;
        }

        return null;
    }

    /// <summary>Removes the bet from the model and the chip visuals for the given origin.</summary>
    public void RemoveBetAndChips(RouletteBetCellView origin)
    {
        if (selectionState != null && selectionState.TryGetOptionForOrigin(origin, out RouletteNeighborCalculator.BetOption option))
        {
            tryRemoveBetFromOption(option);
        }
        else
        {
            tryRemoveBetForCell(origin);
        }

        List<RouletteBetCellView> covered = selectionState != null
            ? selectionState.GetCoveredCellsForOrigin(origin)
            : new List<RouletteBetCellView>();

        foreach (RouletteBetCellView c in covered)
        {
            chipVisualService?.RemoveChipForCell(c);
            selectionState?.RemoveCoveredCell(c);
        }

        selectionState?.RemoveOrigin(origin);
    }
}
