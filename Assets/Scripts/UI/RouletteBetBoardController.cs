using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouletteBetBoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameUIController gameUIController;
    [SerializeField] private Canvas boardCanvas;
    [SerializeField] private RectTransform chipLayerParent;
    [SerializeField] private GameObject chipVisualPrefab;
    [SerializeField] private RouletteBetCellView[] cells;

    [Header("Popup")]
    [SerializeField] private BetTypePickerPopup betTypePickerPopup;

    private BetBoardChipVisualService chipVisualService;
    private BetBoardSelectionState selectionState;
    private bool isBoardInteractable = true;

    private void Awake()
    {
        chipVisualService = new BetBoardChipVisualService(boardCanvas, chipLayerParent, chipVisualPrefab, this);
        selectionState = new BetBoardSelectionState();
    }

    private void OnEnable()
    {
        BindCells();
        if (betTypePickerPopup != null)
            betTypePickerPopup.OptionSelected += OnBetOptionSelected;
    }

    private void OnDisable()
    {
        UnbindCells();
        if (betTypePickerPopup != null)
            betTypePickerPopup.OptionSelected -= OnBetOptionSelected;
    }

    public void Initialize(GameUIController controller)
    {
        gameUIController = controller;
    }

    public void SetBoardInteractable(bool interactable)
    {
        isBoardInteractable = interactable;

        if (cells == null)
        {
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            RouletteBetCellView cell = cells[i];
            if (cell == null || cell.Button == null)
            {
                continue;
            }

            cell.Button.interactable = interactable;
        }
    }

    public void ClearChipVisuals()
    {
        chipVisualService?.ClearChipVisuals();
        selectionState?.Clear();
    }

    private void BindCells()
    {
        if (cells == null)
        {
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            RouletteBetCellView cell = cells[i];
            if (cell == null)
            {
                continue;
            }

            cell.Clicked -= HandleCellClicked;
            cell.Clicked += HandleCellClicked;
        }
    }

    private void UnbindCells()
    {
        if (cells == null)
        {
            return;
        }

        for (int i = 0; i < cells.Length; i++)
        {
            RouletteBetCellView cell = cells[i];
            if (cell == null)
            {
                continue;
            }

            cell.Clicked -= HandleCellClicked;
        }
    }

    private void HandleCellClicked(RouletteBetCellView cell)
    {
        if (!isBoardInteractable || cell == null)
        {
            return;
        }

        if (gameUIController == null)
        {
            Debug.LogWarning("[RouletteBetBoardController] GameUIController reference is missing.", this);
            return;
        }

        // Check if this cell (or any cell it belongs to) is covered by an existing bet
        RouletteBetCellView origin = GetOriginCell(cell);
        if (origin != null)
        {
            RemoveBetAndChips(origin);
            return;
        }

        // For number cells (Straight), show popup to let the player choose bet type
        if (cell.BetType == BetType.Straight && betTypePickerPopup != null)
        {
            selectionState?.SetPendingOrigin(cell);
            betTypePickerPopup.Show(cell.Number);
            return;
        }

        // Outside bets (Red, Black, Dozen, Column, etc.) are placed directly
        if (!gameUIController.TryAddBetForCell(cell))
        {
            return;
        }

        chipVisualService?.EnsureChipForCell(cell);
        selectionState?.SetCoveredCellOrigin(cell, cell);
    }

    // ── Popup integration ────────────────────────────────────────────────────

    private void OnBetOptionSelected(RouletteNeighborCalculator.BetOption option)
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
        if (!gameUIController.TryAddBetFromOption(option))
        {
            return;
        }

        // Place chips on every number cell covered by this bet
        foreach (int number in option.targetNumbers)
        {
            RouletteBetCellView coveredCell = FindNumberCell(number);
            if (coveredCell == null)
            {
                continue;
            }

            chipVisualService?.EnsureChipForCell(coveredCell);
            selectionState?.SetCoveredCellOrigin(coveredCell, originCell);
        }

        selectionState?.SetOptionForOrigin(originCell, option);
    }

    /// <summary>Returns the origin cell for the given cell if a bet chip exists on it, otherwise null.</summary>
    private RouletteBetCellView GetOriginCell(RouletteBetCellView cell)
    {
        RouletteBetCellView origin = selectionState?.GetOriginForCell(cell);
        if (origin != null)
        {
            return origin;
        }

        // Fallback for directly placed chips (outside bets added before tracking was wired)
        if (chipVisualService != null && chipVisualService.HasChip(cell))
        {
            return cell;
        }

        return null;
    }

    private void RemoveBetAndChips(RouletteBetCellView origin)
    {
        // Remove the bet from the game model
        if (selectionState != null && selectionState.TryGetOptionForOrigin(origin, out RouletteNeighborCalculator.BetOption option))
        {
            gameUIController.TryRemoveBetFromOption(option);
        }
        else
        {
            gameUIController.TryRemoveBetForCell(origin);
        }

        // Remove chip visuals from all cells covered by this origin
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

    private RouletteBetCellView FindNumberCell(int number)
    {
        if (cells == null)
        {
            return null;
        }

        foreach (RouletteBetCellView cell in cells)
        {
            if (cell != null && cell.BetType == BetType.Straight && cell.Number == number)
            {
                return cell;
            }
        }

        return null;
    }

}
