using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Appends one UI item per spin to a scrollable history container.
/// Expected setup: assign a content root and a prefab that contains a TMP_Text.
/// </summary>
public class RoundHistoryListPresenter : MonoBehaviour
{
    [Header("History List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject historyItemPrefab;
    [SerializeField] private ScrollRect historyScrollRect;
    [SerializeField] private bool autoScrollToBottom = true;

    private bool isRebuilding;

    public void RebuildFromState(GameStateData state)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Clear();

        if (state == null || state.roundHistory == null)
        {
            return;
        }

        isRebuilding = true;
        for (int i = 0; i < state.roundHistory.Count; i++)
        {
            AddRound(state.roundHistory[i], i + 1);
        }
        isRebuilding = false;

        ScrollToBottomIfNeeded();
    }

    public void AddRound(RoundResultData roundResult, int spinIndex)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (roundResult == null || contentRoot == null || historyItemPrefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(historyItemPrefab, contentRoot, false);
        TMP_Text label = instance.GetComponent<TMP_Text>();
        if (label == null)
        {
            label = instance.GetComponentInChildren<TMP_Text>();
        }

        if (label != null)
        {
            label.text = BuildHistoryLine(roundResult, spinIndex);
        }

        if (!isRebuilding)
        {
            ScrollToBottomIfNeeded();
        }
    }

    public void Clear()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (contentRoot == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

    }

    private static string BuildHistoryLine(RoundResultData roundResult, int spinIndex)
    {
        string selectedOutcomeText = string.Empty;

        if (!string.IsNullOrWhiteSpace(roundResult.selectedOutcomeLabel))
        {
            selectedOutcomeText = " | Selected: " + roundResult.selectedOutcomeLabel;
        }
        else if (roundResult.selectedNumber >= 0)
        {
            selectedOutcomeText = " | Selected: " + roundResult.selectedNumber;
        }

        return
            "Spin #" + spinIndex +
            " | Result: " + roundResult.resultNumber +
            " | Won: " + roundResult.totalWinnings +
            " | Lost: " + roundResult.totalLosses +
            selectedOutcomeText;
    }

    private void ScrollToBottomIfNeeded()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (!autoScrollToBottom || historyScrollRect == null)
        {
            return;
        }

        RectTransform contentRect = contentRoot as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        Canvas.ForceUpdateCanvases();

        RectTransform viewportRect = historyScrollRect.viewport;

        if (contentRect == null)
        {
            historyScrollRect.verticalNormalizedPosition = 1f;
            return;
        }

        if (viewportRect == null)
        {
            viewportRect = historyScrollRect.GetComponent<RectTransform>();
        }

        float contentHeight = contentRect.rect.height;
        float viewportHeight = viewportRect != null ? viewportRect.rect.height : 0f;

        // If content fits into viewport, keep list at top so first item is visible.
        if (contentHeight <= viewportHeight + 0.1f)
        {
            historyScrollRect.verticalNormalizedPosition = 1f;
        }
        else
        {
            historyScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
