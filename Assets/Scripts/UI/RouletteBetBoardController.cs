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

    private readonly Dictionary<RouletteBetCellView, Image> chipsByCell = new Dictionary<RouletteBetCellView, Image>();
    // Maps every chip-covered cell to the cell that was originally clicked to place that bet.
    private readonly Dictionary<RouletteBetCellView, RouletteBetCellView> originCellByCell = new Dictionary<RouletteBetCellView, RouletteBetCellView>();
    // Maps each origin cell to the bet option that was chosen (for removal).
    private readonly Dictionary<RouletteBetCellView, RouletteNeighborCalculator.BetOption> optionByOriginCell = new Dictionary<RouletteBetCellView, RouletteNeighborCalculator.BetOption>();
    // The number cell that is waiting for the popup selection.
    private RouletteBetCellView pendingOriginCell;
    private bool isBoardInteractable = true;

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
        foreach (KeyValuePair<RouletteBetCellView, Image> pair in chipsByCell)
        {
            if (pair.Value != null)
            {
                SafeDestroy(pair.Value.gameObject);
            }

            if (pair.Key != null)
            {
                pair.Key.SetHighlighted(false);
            }
        }

        chipsByCell.Clear();
        originCellByCell.Clear();
        optionByOriginCell.Clear();
        pendingOriginCell = null;
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
            pendingOriginCell = cell;
            betTypePickerPopup.Show(cell.Number);
            return;
        }

        // Outside bets (Red, Black, Dozen, Column, etc.) are placed directly
        if (!gameUIController.TryAddBetForCell(cell))
        {
            return;
        }

        EnsureChipForCell(cell);
        originCellByCell[cell] = cell;  // self-origin so removal works uniformly
    }

    private void EnsureChipForCell(RouletteBetCellView cell)
    {
        if (chipVisualPrefab == null)
        {
            return;
        }

        if (chipsByCell.ContainsKey(cell) && chipsByCell[cell] != null)
        {
            PositionChip(chipsByCell[cell], cell.ChipAnchor);
            cell.SetHighlighted(true);
            return;
        }

        RectTransform parent = chipLayerParent != null ? chipLayerParent : cell.ChipAnchor.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject chipObject = Instantiate(chipVisualPrefab, parent, false);
        Image chip = chipObject.GetComponent<Image>();
        if (chip == null)
        {
            Debug.LogWarning("[RouletteBetBoardController] Chip prefab must have an Image component on root.", this);
            SafeDestroy(chipObject);
            return;
        }

        PositionChip(chip, cell.ChipAnchor);
        chipsByCell[cell] = chip;
        cell.SetHighlighted(true);
    }

    private void RemoveChipForCell(RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return;
        }

        if (chipsByCell.TryGetValue(cell, out Image chip) && chip != null)
        {
            SafeDestroy(chip.gameObject);
        }

        chipsByCell.Remove(cell);
        cell.SetHighlighted(false);
    }

    // ── Popup integration ────────────────────────────────────────────────────

    private void OnBetOptionSelected(RouletteNeighborCalculator.BetOption option)
    {
        if (pendingOriginCell == null)
        {
            return;
        }

        RouletteBetCellView origin = pendingOriginCell;
        pendingOriginCell = null;
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

            EnsureChipForCell(coveredCell);
            originCellByCell[coveredCell] = originCell;
        }

        optionByOriginCell[originCell] = option;
    }

    /// <summary>Returns the origin cell for the given cell if a bet chip exists on it, otherwise null.</summary>
    private RouletteBetCellView GetOriginCell(RouletteBetCellView cell)
    {
        if (originCellByCell.TryGetValue(cell, out RouletteBetCellView origin))
        {
            return origin;
        }

        // Fallback for directly placed chips (outside bets added before tracking was wired)
        if (chipsByCell.ContainsKey(cell) && chipsByCell[cell] != null)
        {
            return cell;
        }

        return null;
    }

    private void RemoveBetAndChips(RouletteBetCellView origin)
    {
        // Remove the bet from the game model
        if (optionByOriginCell.TryGetValue(origin, out RouletteNeighborCalculator.BetOption option))
        {
            gameUIController.TryRemoveBetFromOption(option);
        }
        else
        {
            gameUIController.TryRemoveBetForCell(origin);
        }

        // Remove chip visuals from all cells covered by this origin
        var covered = new List<RouletteBetCellView>();
        foreach (KeyValuePair<RouletteBetCellView, RouletteBetCellView> kvp in originCellByCell)
        {
            if (kvp.Value == origin)
            {
                covered.Add(kvp.Key);
            }
        }

        foreach (RouletteBetCellView c in covered)
        {
            RemoveChipForCell(c);
            originCellByCell.Remove(c);
        }

        optionByOriginCell.Remove(origin);
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

    // ── Private helpers ──────────────────────────────────────────────────────

    private static void SafeDestroy(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
            return;
        }

        DestroyImmediate(target);
    }

    private void PositionChip(Image chip, RectTransform anchor)
    {
        if (chip == null || anchor == null)
        {
            return;
        }

        RectTransform chipRect = chip.rectTransform;
        RectTransform parentRect = chipRect.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        Canvas canvas = boardCanvas;
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = canvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, anchor.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out Vector2 localPoint))
        {
            chipRect.anchoredPosition = localPoint;
        }

        chipRect.localRotation = Quaternion.identity;
        chipRect.localScale = Vector3.one;
    }
}
