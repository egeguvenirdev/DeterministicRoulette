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

    private readonly Dictionary<RouletteBetCellView, Image> chipsByCell = new Dictionary<RouletteBetCellView, Image>();
    private bool isBoardInteractable = true;

    private void OnEnable()
    {
        BindCells();
    }

    private void OnDisable()
    {
        UnbindCells();
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

        if (!gameUIController.TryAddStraightBetForNumber(cell.Number))
        {
            return;
        }

        EnsureChipForCell(cell);
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
