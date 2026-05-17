using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GameUIController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private TMP_Dropdown targetNumberDropdown;
    [SerializeField] private TMP_InputField stakeInput;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button clearSelectionButton;
    [SerializeField] private Button clearBetsButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text chipsText;
    [SerializeField] private TMP_Text betsText;
    [SerializeField] private TMP_Text winningNumberText;
    [SerializeField] private TMP_Text winningsAmountText;
    [SerializeField] private TMP_Text statsText;

    [Header("Animation Sync")]
    [SerializeField] private RouletteWheelAnimator wheelAnimator;
    [SerializeField] private bool waitForWheelAnimation = true;

    [Header("Bet Board")]
    [SerializeField] private RouletteBetBoardController betBoardController;

    private OutcomeSelector outcomeSelector;
    private BetManager betManager;
    private RouletteGameManager gameManager;
    private StatisticsManager statisticsManager;
    private RouletteGameFlowService flowService;
    private bool initialized;
    private bool roundCompletedBound;
    private bool wheelAnimatorBound;
    private RoundResultData pendingRoundResult;

    private void Awake()
    {
        SetupDropdowns();
    }

    private void Start()
    {
        if (initialized)
        {
            return;
        }

        ResolveGameplayReferences();

        if (HasDependencies())
        {
            Initialize(outcomeSelector, betManager, gameManager, statisticsManager);
        }
    }

    private void OnEnable()
    {
        WireButtons();
    }

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

        if (!HasDependencies())
        {
            Debug.LogError("[GameUIController] Missing gameplay dependencies during initialization.", this);
            initialized = false;
            return;
        }

        flowService = new RouletteGameFlowService(betManager, gameManager, outcomeSelector, statisticsManager);

        WireButtons();
        BindRoundCompleted();
        BindWheelAnimator();
        BindBetBoardController();

        initialized = true;
        RefreshView();
    }

    private void OnDisable()
    {
        if (gameManager != null && roundCompletedBound)
        {
            gameManager.RoundCompleted -= HandleRoundCompleted;
            roundCompletedBound = false;
        }

        UnbindWheelAnimator();
        UnbindBetBoardController();

        UnwireButtons();
    }

    private void SetupDropdowns()
    {
        SetupDropdown(targetNumberDropdown, true);
    }

    private void SetupDropdown(TMP_Dropdown dropdown, bool includeRandom)
    {
        if (dropdown == null)
        {
            return;
        }

        EnsureDropdownBindings(dropdown);

        dropdown.ClearOptions();
        dropdown.AddOptions(BuildNumberOptions(includeRandom));
        dropdown.SetValueWithoutNotify(0);
        dropdown.RefreshShownValue();

        if (dropdown.captionText != null && dropdown.options.Count > 0)
        {
            int selectedIndex = Mathf.Clamp(dropdown.value, 0, dropdown.options.Count - 1);
            dropdown.captionText.text = dropdown.options[selectedIndex].text;
        }
    }

    private void EnsureDropdownBindings(TMP_Dropdown dropdown)
    {
        if (dropdown.captionText == null)
        {
            Transform label = dropdown.transform.Find("Label");
            if (label != null)
            {
                dropdown.captionText = label.GetComponent<TMP_Text>();
            }
        }

        if (dropdown.template == null)
        {
            Transform template = dropdown.transform.Find("Template");
            if (template != null)
            {
                dropdown.template = template as RectTransform;
            }
        }

        if (dropdown.itemText == null)
        {
            Transform itemLabel = dropdown.transform.Find("Template/Viewport/Content/Item/Item Label");
            if (itemLabel != null)
            {
                dropdown.itemText = itemLabel.GetComponent<TMP_Text>();
            }
        }
    }

    private void WireButtons()
    {
        BindButton(spinButton, Spin);
        BindButton(clearSelectionButton, ClearSelection);
        BindButton(clearBetsButton, ClearBets);
    }

    private void UnwireButtons()
    {
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

    private void BindButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);

        if (button.onClick.GetPersistentEventCount() == 0)
        {
            button.onClick.AddListener(action);
        }
    }


    private List<TMP_Dropdown.OptionData> BuildNumberOptions(bool includeRandom)
    {
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        if (includeRandom)
        {
            options.Add(new TMP_Dropdown.OptionData("Random"));
        }

        for (int number = 0; number <= 36; number++)
        {
            options.Add(new TMP_Dropdown.OptionData(number.ToString()));
        }

        return options;
    }

    private void Spin()
    {
        if (!initialized || flowService == null)
        {
            return;
        }

        ApplySelection();

        if (ShouldWaitForWheelAnimation())
        {
            SetControlsInteractable(false);
        }

        if (!gameManager.CanSpin())
        {
            Debug.LogWarning("Cannot spin");
            RefreshView();
            SetControlsInteractable(true);
            return;
        }

        RoundResultData roundResult = flowService.ExecuteSpin();

        if (roundResult == null)
        {
            Debug.LogWarning("Spin failed");
            SetControlsInteractable(true);
            return;
        }

        if (!ShouldWaitForWheelAnimation())
        {
            RefreshView();
        }
    }

    private void ApplySelection()
    {
        if (targetNumberDropdown == null || flowService == null)
        {
            return;
        }

        if (targetNumberDropdown.value == 0)
        {
            flowService.ClearOutcomeSelection();
            return;
        }

        flowService.SetOutcomeSelection(targetNumberDropdown.value - 1);
    }

    private void ClearSelection()
    {
        if (flowService != null)
        {
            flowService.ClearOutcomeSelection();
        }

        if (targetNumberDropdown != null)
        {
            targetNumberDropdown.SetValueWithoutNotify(0);
            targetNumberDropdown.RefreshShownValue();
        }

        RefreshView();
    }

    private void ClearBets()
    {
        if (flowService != null)
        {
            flowService.ClearAllBets();
        }

        if (betBoardController != null)
        {
            betBoardController.ClearChipVisuals();
        }

        RefreshView();
    }

    public bool TryAddStraightBetForNumber(int targetNumber)
    {
        if (!initialized || flowService == null)
        {
            return false;
        }

        if (targetNumber < 0 || targetNumber > 36)
        {
            Debug.LogWarning("Target number out of range.");
            return false;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("Invalid stake");
            return false;
        }

        if (!flowService.TryAddStraightBet(targetNumber, stake))
        {
            Debug.LogWarning("Bet rejected");
            return false;
        }

        RefreshView();
        return true;
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
        if (roundResult == null)
        {
            return;
        }

        if (ShouldWaitForWheelAnimation())
        {
            pendingRoundResult = roundResult;
            return;
        }

        ApplyRoundResultToView(roundResult);
        RefreshView();
    }

    private void ApplyRoundResultToView(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        // Winning number with red/black color
        string numberColor = IsRedNumber(roundResult.resultNumber) ? "#FF0000" : "#000000";
        if (winningNumberText != null)
        {
            winningNumberText.text = $"<color={numberColor}>{roundResult.resultNumber}</color>";
        }
        
        // Winnings/Loss amount with green/red color
        bool hasWinningBet = roundResult.winningBets != null && roundResult.winningBets.Count > 0;
        if (winningsAmountText != null && hasWinningBet && roundResult.totalWinnings > 0)
        {
            winningsAmountText.text = $"<color=#00FF00>+ {roundResult.totalWinnings}</color>";
        }
        else if (winningsAmountText != null)
        {
            winningsAmountText.text = $"<color=#FF0000>- {roundResult.totalLosses}</color>";
        }
    }

    private void BindWheelAnimator()
    {
        if (wheelAnimator == null || wheelAnimatorBound)
        {
            return;
        }

        wheelAnimator.SpinAnimationStarted -= HandleSpinAnimationStarted;
        wheelAnimator.SpinAnimationStarted += HandleSpinAnimationStarted;

        wheelAnimator.SpinAnimationCompleted -= HandleSpinAnimationCompleted;
        wheelAnimator.SpinAnimationCompleted += HandleSpinAnimationCompleted;

        wheelAnimatorBound = true;
    }

    private void UnbindWheelAnimator()
    {
        if (wheelAnimator == null || !wheelAnimatorBound)
        {
            return;
        }

        wheelAnimator.SpinAnimationStarted -= HandleSpinAnimationStarted;
        wheelAnimator.SpinAnimationCompleted -= HandleSpinAnimationCompleted;
        wheelAnimatorBound = false;
    }

    private void HandleSpinAnimationStarted()
    {
        if (ShouldWaitForWheelAnimation())
        {
            SetControlsInteractable(false);
        }
    }

    private void HandleSpinAnimationCompleted(RoundResultData roundResult)
    {
        RoundResultData finalResult = pendingRoundResult ?? roundResult;
        pendingRoundResult = null;

        ApplyRoundResultToView(finalResult);
        RefreshView();
        SetControlsInteractable(true);
    }

    private bool ShouldWaitForWheelAnimation()
    {
        return waitForWheelAnimation && wheelAnimator != null && wheelAnimator.enabled;
    }

    private void SetControlsInteractable(bool interactable)
    {
        if (targetNumberDropdown != null)
        {
            targetNumberDropdown.interactable = interactable;
        }

        if (stakeInput != null)
        {
            stakeInput.interactable = interactable;
        }

        if (spinButton != null)
        {
            spinButton.interactable = interactable;
        }

        if (clearSelectionButton != null)
        {
            clearSelectionButton.interactable = interactable;
        }

        if (clearBetsButton != null)
        {
            clearBetsButton.interactable = interactable;
        }


        if (betBoardController != null)
        {
            betBoardController.SetBoardInteractable(interactable);
        }
    }
    
    private bool IsRedNumber(int number)
    {
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        return System.Array.Exists(redNumbers, element => element == number);
    }

    private void RefreshView()
    {
        if (flowService != null)
        {
            GameStateData state = flowService.GetGameState();

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

        if (betsText != null && flowService != null)
        {
            betsText.text = "Bets: " + flowService.GetActiveBetCount() + "  Stake: " + flowService.GetTotalStake();
        }

        if (betBoardController != null && flowService != null && flowService.GetActiveBetCount() == 0)
        {
            betBoardController.ClearChipVisuals();
        }
    }

    private bool HasDependencies()
    {
        return outcomeSelector != null &&
               betManager != null &&
               gameManager != null &&
               statisticsManager != null;
    }

    private void ResolveGameplayReferences()
    {
        if (outcomeSelector == null)
        {
            outcomeSelector = FindFirstObjectByType<OutcomeSelector>();
        }

        if (betManager == null)
        {
            betManager = FindFirstObjectByType<BetManager>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<RouletteGameManager>();
        }

        if (statisticsManager == null)
        {
            statisticsManager = FindFirstObjectByType<StatisticsManager>();
        }

        if (wheelAnimator == null)
        {
            wheelAnimator = FindFirstObjectByType<RouletteWheelAnimator>();
        }

        if (betBoardController == null)
        {
            betBoardController = FindFirstObjectByType<RouletteBetBoardController>();
        }
    }

    private void BindBetBoardController()
    {
        if (betBoardController == null)
        {
            return;
        }

        betBoardController.Initialize(this);
    }

    private void UnbindBetBoardController()
    {
        if (betBoardController == null)
        {
            return;
        }

        betBoardController.Initialize(null);
    }

    private void BindRoundCompleted()
    {
        if (gameManager == null || roundCompletedBound)
        {
            return;
        }

        gameManager.RoundCompleted -= HandleRoundCompleted;
        gameManager.RoundCompleted += HandleRoundCompleted;
        roundCompletedBound = true;
    }
}