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

    [Header("Audio")]
    [SerializeField] private GameAudioService gameAudioService;
    [SerializeField] private GameAudioEventId chipDropAudioEvent = GameAudioEventId.ChipDrop;

    private BetBoardChipVisualService chipVisualService;
    private BetBoardSelectionState selectionState;
    private BetBoardPopupFlow popupFlow;
    private BetBoardRemoveStrategy removeStrategy;
    private bool isBoardInteractable = true;

    private void Awake()
    {
        chipVisualService = new BetBoardChipVisualService(boardCanvas, chipLayerParent, chipVisualPrefab, this);
        selectionState = new BetBoardSelectionState();
        popupFlow = new BetBoardPopupFlow(selectionState, chipVisualService, FindNumberCell, c => gameUIController != null && gameUIController.TryAddBetFromOption(c), TriggerChipDrop);
        removeStrategy = new BetBoardRemoveStrategy(selectionState, chipVisualService, c => gameUIController != null && gameUIController.TryRemoveBetForCell(c), c => gameUIController != null && gameUIController.TryRemoveBetFromOption(c));
    }

    private void OnEnable()
    {
        BindCells();
        if (betTypePickerPopup != null)
            betTypePickerPopup.OptionSelected += popupFlow.OnOptionSelected;
    }

    private void OnDisable()
    {
        UnbindCells();
        if (betTypePickerPopup != null)
            betTypePickerPopup.OptionSelected -= popupFlow.OnOptionSelected;
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

        RouletteBetCellView origin = removeStrategy?.GetOriginCell(cell);
        if (origin != null)
        {
            removeStrategy.RemoveBetAndChips(origin);
            return;
        }

        // For number cells (Straight), show popup to let the player choose bet type.
        if (cell.BetType == BetType.Straight && betTypePickerPopup != null)
        {
            selectionState?.SetPendingOrigin(cell);
            betTypePickerPopup.Show(cell.Number);
            return;
        }

        // Outside bets (Red, Black, Dozen, Column, etc.) are placed directly.
        if (!gameUIController.TryAddBetForCell(cell))
        {
            return;
        }

        chipVisualService?.EnsureChipForCell(cell);
        selectionState?.SetCoveredCellOrigin(cell, cell);
        TriggerChipDrop();
    }

    private void TriggerChipDrop()
    {
        if (gameAudioService == null || chipDropAudioEvent == GameAudioEventId.None)
        {
            return;
        }

        gameAudioService.Play(chipDropAudioEvent);
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
