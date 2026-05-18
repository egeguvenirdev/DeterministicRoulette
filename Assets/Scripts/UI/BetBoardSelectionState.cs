using System.Collections.Generic;

public class BetBoardSelectionState
{
    private readonly Dictionary<RouletteBetCellView, RouletteBetCellView> originCellByCell = new Dictionary<RouletteBetCellView, RouletteBetCellView>();
    private readonly Dictionary<RouletteBetCellView, RouletteNeighborCalculator.BetOption> optionByOriginCell = new Dictionary<RouletteBetCellView, RouletteNeighborCalculator.BetOption>();

    public RouletteBetCellView PendingOriginCell { get; private set; }

    public void SetPendingOrigin(RouletteBetCellView cell)
    {
        PendingOriginCell = cell;
    }

    public RouletteBetCellView ConsumePendingOrigin()
    {
        RouletteBetCellView origin = PendingOriginCell;
        PendingOriginCell = null;
        return origin;
    }

    public void SetCoveredCellOrigin(RouletteBetCellView coveredCell, RouletteBetCellView originCell)
    {
        if (coveredCell == null || originCell == null)
        {
            return;
        }

        originCellByCell[coveredCell] = originCell;
    }

    public RouletteBetCellView GetOriginForCell(RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return null;
        }

        if (originCellByCell.TryGetValue(cell, out RouletteBetCellView origin))
        {
            return origin;
        }

        return null;
    }

    public void SetOptionForOrigin(RouletteBetCellView originCell, RouletteNeighborCalculator.BetOption option)
    {
        if (originCell == null || option == null)
        {
            return;
        }

        optionByOriginCell[originCell] = option;
    }

    public bool TryGetOptionForOrigin(RouletteBetCellView originCell, out RouletteNeighborCalculator.BetOption option)
    {
        if (originCell == null)
        {
            option = null;
            return false;
        }

        return optionByOriginCell.TryGetValue(originCell, out option);
    }

    public List<RouletteBetCellView> GetCoveredCellsForOrigin(RouletteBetCellView originCell)
    {
        List<RouletteBetCellView> covered = new List<RouletteBetCellView>();
        if (originCell == null)
        {
            return covered;
        }

        foreach (KeyValuePair<RouletteBetCellView, RouletteBetCellView> kvp in originCellByCell)
        {
            if (kvp.Value == originCell)
            {
                covered.Add(kvp.Key);
            }
        }

        return covered;
    }

    public void RemoveCoveredCell(RouletteBetCellView coveredCell)
    {
        if (coveredCell == null)
        {
            return;
        }

        originCellByCell.Remove(coveredCell);
    }

    public void RemoveOrigin(RouletteBetCellView originCell)
    {
        if (originCell == null)
        {
            return;
        }

        optionByOriginCell.Remove(originCell);
    }

    public void Clear()
    {
        PendingOriginCell = null;
        originCellByCell.Clear();
        optionByOriginCell.Clear();
    }
}
