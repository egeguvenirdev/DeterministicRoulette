using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetBoardChipVisualService
{
    private readonly Canvas boardCanvas;
    private readonly RectTransform chipLayerParent;
    private readonly GameObject chipVisualPrefab;
    private readonly Object context;

    private readonly Dictionary<RouletteBetCellView, Image> chipsByCell = new Dictionary<RouletteBetCellView, Image>();

    public BetBoardChipVisualService(Canvas boardCanvas, RectTransform chipLayerParent, GameObject chipVisualPrefab, Object context)
    {
        this.boardCanvas = boardCanvas;
        this.chipLayerParent = chipLayerParent;
        this.chipVisualPrefab = chipVisualPrefab;
        this.context = context;
    }

    public bool HasChip(RouletteBetCellView cell)
    {
        return cell != null && chipsByCell.TryGetValue(cell, out Image chip) && chip != null;
    }

    public void EnsureChipForCell(RouletteBetCellView cell)
    {
        if (cell == null || chipVisualPrefab == null)
        {
            return;
        }

        if (chipsByCell.TryGetValue(cell, out Image existingChip) && existingChip != null)
        {
            PositionChip(existingChip, cell.ChipAnchor);
            cell.SetHighlighted(true);
            return;
        }

        RectTransform parent = chipLayerParent != null ? chipLayerParent : cell.ChipAnchor.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        GameObject chipObject = Object.Instantiate(chipVisualPrefab, parent, false);
        Image chip = chipObject.GetComponent<Image>();
        if (chip == null)
        {
            Debug.LogWarning("[RouletteBetBoardController] Chip prefab must have an Image component on root.", context);
            SafeDestroy(chipObject);
            return;
        }

        PositionChip(chip, cell.ChipAnchor);
        chipsByCell[cell] = chip;
        cell.SetHighlighted(true);
    }

    public void RemoveChipForCell(RouletteBetCellView cell)
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

        Camera cam = null;
        if (boardCanvas != null && boardCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = boardCanvas.worldCamera;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, anchor.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out Vector2 localPoint))
        {
            chipRect.anchoredPosition = localPoint;
        }

        chipRect.localRotation = Quaternion.identity;
        chipRect.localScale = Vector3.one;
    }

    private static void SafeDestroy(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
            return;
        }

        Object.DestroyImmediate(target);
    }
}
