using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles all read-only display updates: round results, game state labels, bet summary, and table type.
/// Attach to the same GameObject as GameUIController and wire fields in the Inspector.
/// </summary>
public class RoundResultPresenter : MonoBehaviour
{
    [Header("Result Labels")]
    [SerializeField] private TMP_Text winningNumberText;
    [SerializeField] private TMP_Text winningsAmountText;

    [Header("State Labels")]
    [SerializeField] private TMP_Text chipsText;
    [SerializeField] private TMP_Text betsText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text tableTypeText;

    public void PresentRoundResult(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        string numberColor = IsRedNumber(roundResult.resultNumber) ? "#FF0000" : "#000000";
        if (winningNumberText != null)
        {
            winningNumberText.text = $"<color={numberColor}>{roundResult.resultNumber}</color>";
        }

        bool hasWinningBet = roundResult.winningBets != null && roundResult.winningBets.Count > 0;
        if (winningsAmountText != null && hasWinningBet && roundResult.totalWinnings > 0)
        {
            string winningTypes = BuildWinningBetTypesLabel(roundResult.winningBets);
            winningsAmountText.text = $"<color=#00FF00>+ {roundResult.totalWinnings} ({winningTypes})</color>";
        }
        else if (winningsAmountText != null)
        {
            winningsAmountText.text = $"<color=#FF0000>- {roundResult.totalLosses}</color>";
        }
    }

    public void PresentGameState(GameStateData state)
    {
        if (state == null)
        {
            return;
        }

        if (chipsText != null)
        {
            chipsText.text = "Chips: " + state.totalChips;
        }

        if (statsText != null)
        {
            statsText.text = "Spins: " + state.spinsPlayed + "  W: " + state.totalWins + "  L: " + state.totalLosses;
        }
    }

    public void PresentBets(int betCount, int totalStake)
    {
        if (betsText != null)
        {
            betsText.text = "Bets: " + betCount + "  Stake: " + totalStake;
        }
    }

    public void UpdateTableTypeLabel(List<BetData> activeBets)
    {
        if (tableTypeText == null)
        {
            return;
        }

        if (activeBets == null || activeBets.Count == 0)
        {
            tableTypeText.text = "Table Type: None";
            return;
        }

        List<string> typeNames = new List<string>();
        for (int i = 0; i < activeBets.Count; i++)
        {
            string typeName = GetBetTypeLabel(activeBets[i].betType);
            if (!typeNames.Contains(typeName))
            {
                typeNames.Add(typeName);
            }
        }

        tableTypeText.text = typeNames.Count == 1
            ? "Table Type: " + typeNames[0]
            : "Table Type: Multiple Bets";
    }

    private static string BuildWinningBetTypesLabel(List<BetData> winningBets)
    {
        List<string> labels = new List<string>();
        for (int i = 0; i < winningBets.Count; i++)
        {
            string label = GetBetTypeLabel(winningBets[i].betType);
            if (!labels.Contains(label))
            {
                labels.Add(label);
            }
        }
        return string.Join(", ", labels);
    }

    public static string GetBetTypeLabel(BetType betType)
    {
        switch (betType)
        {
            case BetType.Column1:
            case BetType.Column2:
            case BetType.Column3:
                return "Column";

            case BetType.Dozen1:
            case BetType.Dozen2:
            case BetType.Dozen3:
                return "Dozen";

            default:
                return betType.ToString();
        }
    }

    private static bool IsRedNumber(int number)
    {
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        return System.Array.Exists(redNumbers, element => element == number);
    }
}
