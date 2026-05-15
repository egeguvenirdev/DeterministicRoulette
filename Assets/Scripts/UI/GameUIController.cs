using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Dropdown targetNumberDropdown;
    [SerializeField] private Dropdown straightBetDropdown;
    [SerializeField] private InputField stakeInput;
    [SerializeField] private Button addStraightBetButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button clearSelectionButton;
    [SerializeField] private Button clearBetsButton;

    [Header("Labels")]
    [SerializeField] private Text chipsText;
    [SerializeField] private Text betsText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text statsText;

    private OutcomeSelector outcomeSelector;
    private BetManager betManager;
    private RouletteGameManager gameManager;
    private StatisticsManager statisticsManager;
    private bool initialized;

    public void Initialize(
        OutcomeSelector selector,
        BetManager manager,
        RouletteGameManager rouletteGameManager,
        StatisticsManager stats)
    {
        outcomeSelector = selector;
        betManager = manager;
        gameManager = rouletteGameManager;
        statisticsManager = stats;

        SetupDropdowns();
        WireButtons();

        if (gameManager != null)
        {
            gameManager.RoundCompleted -= HandleRoundCompleted;
            gameManager.RoundCompleted += HandleRoundCompleted;
        }

        initialized = true;
        RefreshView();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.RoundCompleted -= HandleRoundCompleted;
        }

        UnwireButtons();
    }

    private void SetupDropdowns()
    {
        if (targetNumberDropdown != null && targetNumberDropdown.options.Count == 0)
        {
            targetNumberDropdown.options = BuildNumberOptions(true);
            targetNumberDropdown.value = 0;
        }

        if (straightBetDropdown != null && straightBetDropdown.options.Count == 0)
        {
            straightBetDropdown.options = BuildNumberOptions(false);
            straightBetDropdown.value = 0;
        }
    }

    private void WireButtons()
    {
        if (addStraightBetButton != null)
        {
            addStraightBetButton.onClick.RemoveListener(AddStraightBet);
            addStraightBetButton.onClick.AddListener(AddStraightBet);
        }

        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(Spin);
            spinButton.onClick.AddListener(Spin);
        }

        if (clearSelectionButton != null)
        {
            clearSelectionButton.onClick.RemoveListener(ClearSelection);
            clearSelectionButton.onClick.AddListener(ClearSelection);
        }

        if (clearBetsButton != null)
        {
            clearBetsButton.onClick.RemoveListener(ClearBets);
            clearBetsButton.onClick.AddListener(ClearBets);
        }
    }

    private void UnwireButtons()
    {
        if (addStraightBetButton != null)
        {
            addStraightBetButton.onClick.RemoveListener(AddStraightBet);
        }

        if (spinButton != null)
        {
            spinButton.onClick.RemoveListener(Spin);
        }

        if (clearSelectionButton != null)
        {
            clearSelectionButton.onClick.RemoveListener(ClearSelection);
        }

        if (clearBetsButton != null)
        {
            clearBetsButton.onClick.RemoveListener(ClearBets);
        }
    }

    private List<Dropdown.OptionData> BuildNumberOptions(bool includeRandom)
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

        if (includeRandom)
        {
            options.Add(new Dropdown.OptionData("Random"));
        }

        for (int number = 0; number <= 36; number++)
        {
            options.Add(new Dropdown.OptionData(number.ToString()));
        }

        return options;
    }

    private void AddStraightBet()
    {
        if (!initialized || betManager == null)
        {
            return;
        }

        if (!TryGetStake(out int stake))
        {
            SetResultText("Invalid stake");
            return;
        }

        int targetNumber = straightBetDropdown != null ? straightBetDropdown.value : 0;
        BetData bet = new BetData
        {
            betType = BetType.Straight,
            targetNumber = targetNumber,
            amount = stake
        };

        if (!betManager.TryAddBet(bet))
        {
            SetResultText("Bet rejected");
            return;
        }

        RefreshView();
    }

    private void Spin()
    {
        if (!initialized || gameManager == null || outcomeSelector == null)
        {
            return;
        }

        ApplySelection();

        if (!gameManager.CanSpin())
        {
            SetResultText("Cannot spin");
            RefreshView();
            return;
        }

        RoundResultData roundResult = gameManager.Spin();

        if (roundResult == null)
        {
            SetResultText("Spin failed");
            return;
        }

        RefreshView();
    }

    private void ApplySelection()
    {
        if (targetNumberDropdown == null || outcomeSelector == null)
        {
            return;
        }

        if (targetNumberDropdown.value == 0)
        {
            outcomeSelector.ClearSelection();
            return;
        }

        outcomeSelector.SetSelectedNumber(targetNumberDropdown.value - 1);
    }

    private void ClearSelection()
    {
        if (outcomeSelector != null)
        {
            outcomeSelector.ClearSelection();
        }

        if (targetNumberDropdown != null)
        {
            targetNumberDropdown.value = 0;
        }

        RefreshView();
    }

    private void ClearBets()
    {
        if (betManager != null)
        {
            betManager.ClearBets();
        }

        RefreshView();
    }

    private bool TryGetStake(out int stake)
    {
        stake = 0;

        if (stakeInput == null)
        {
            return false;
        }

        return int.TryParse(stakeInput.text, out stake) && stake > 0;
    }

    private void HandleRoundCompleted(RoundResultData roundResult)
    {
        SetResultText("Result: " + roundResult.resultNumber + "  Win: " + roundResult.totalWinnings);
        RefreshView();
    }

    private void RefreshView()
    {
        if (statisticsManager != null)
        {
            GameStateData state = statisticsManager.CurrentState;

            if (chipsText != null)
            {
                chipsText.text = "Chips: " + state.totalChips;
            }

            if (statsText != null)
            {
                statsText.text = "Spins: " + state.spinsPlayed + "  W: " + state.totalWins + "  L: " + state.totalLosses;
            }
        }

        if (betsText != null && betManager != null)
        {
            betsText.text = "Bets: " + betManager.ActiveBets.Count + "  Stake: " + betManager.GetTotalStake();
        }
    }

    private void SetResultText(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }
    }
}