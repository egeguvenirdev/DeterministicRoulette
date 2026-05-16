using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GameUIController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform slidingPanel;
    [SerializeField] private Button togglePanelButton;
    [SerializeField] private TMP_Text togglePanelButtonText;
    [SerializeField] private bool startCollapsed = true;
    [SerializeField] private float panelClosedX = 1120f;
    [SerializeField] private float panelSlideDuration = 0.22f;

    [Header("Controls")]
    [SerializeField] private TMP_Dropdown targetNumberDropdown;
    [SerializeField] private TMP_Dropdown straightBetDropdown;
    [SerializeField] private TMP_InputField stakeInput;
    [SerializeField] private Button addStraightBetButton;
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

    private OutcomeSelector outcomeSelector;
    private BetManager betManager;
    private RouletteGameManager gameManager;
    private StatisticsManager statisticsManager;
    private RouletteGameFlowService flowService;
    private bool initialized;
    private bool panelOpen;
    private Coroutine panelSlideRoutine;
    private bool roundCompletedBound;
    private bool panelStateInitialized;
    private bool wheelAnimatorBound;
    private RoundResultData pendingRoundResult;

    private void Awake()
    {
        ResolveViewReferences();
        SetupDropdowns();
        SetupPanelState();
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
        ResolveViewReferences();
        WireButtons();
        SetupPanelState();
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

        ResolveViewReferences();
        WireButtons();
        BindRoundCompleted();
        BindWheelAnimator();

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

        UnwireButtons();
    }

    private void SetupDropdowns()
    {
        SetupDropdown(targetNumberDropdown, true);
        SetupDropdown(straightBetDropdown, false);
    }

    private void ResolveViewReferences()
    {
        if (slidingPanel == null)
        {
            slidingPanel = transform as RectTransform;
        }

        if (togglePanelButton == null)
        {
            Button button = FindByPathOrName<Button>("Button_TogglePanel", "TogglePanel", "Toggle");
            if (button != null)
            {
                togglePanelButton = button;
            }
        }

        if (togglePanelButtonText == null && togglePanelButton != null)
        {
            togglePanelButtonText = togglePanelButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (targetNumberDropdown == null)
        {
            TMP_Dropdown dropdown = FindByPathOrName<TMP_Dropdown>("Panel_OutcomeControls/Dropdown_TargetOutcome", "Dropdown_TargetOutcome", "TargetOutcome");
            if (dropdown != null)
            {
                targetNumberDropdown = dropdown;
            }
        }

        if (straightBetDropdown == null)
        {
            TMP_Dropdown dropdown = FindByPathOrName<TMP_Dropdown>("Panel_BetControls/Dropdown_StraightBetNumber", "Dropdown_StraightBetNumber", "StraightBetNumber");
            if (dropdown != null)
            {
                straightBetDropdown = dropdown;
            }
        }

        if (stakeInput == null)
        {
            TMP_InputField input = FindByPathOrName<TMP_InputField>("Panel_BetControls/Input_StakeAmount", "Input_StakeAmount", "StakeAmount");
            if (input != null)
            {
                stakeInput = input;
            }
        }

        if (addStraightBetButton == null)
        {
            Button button = FindByPathOrName<Button>("Panel_BetControls/Row_BetButtons/Button_AddStraightBet", "Button_AddStraightBet", "AddStraightBet");
            if (button != null)
            {
                addStraightBetButton = button;
            }
        }

        if (clearBetsButton == null)
        {
            Button button = FindByPathOrName<Button>("Panel_BetControls/Row_BetButtons/Button_ClearBets", "Button_ClearBets", "ClearBets");
            if (button != null)
            {
                clearBetsButton = button;
            }
        }

        if (clearSelectionButton == null)
        {
            Button button = FindByPathOrName<Button>("Panel_OutcomeControls/Row_OutcomeButtons/Button_ClearSelection", "Button_ClearSelection", "ClearSelection");
            if (button != null)
            {
                clearSelectionButton = button;
            }
        }

        if (spinButton == null)
        {
            Button button = FindByPathOrName<Button>("Panel_OutcomeControls/Row_OutcomeButtons/Button_Spin", "Button_Spin", "Spin");
            if (button != null)
            {
                spinButton = button;
            }
        }

        if (chipsText == null)
        {
            TMP_Text label = FindByPathOrName<TMP_Text>("Panel_TopStatus/Text_Chips", "Text_Chips", "Chips");
            if (label != null)
            {
                chipsText = label;
            }
        }

        if (statsText == null)
        {
            TMP_Text label = FindByPathOrName<TMP_Text>("Panel_TopStatus/Text_Stats", "Text_Stats", "Stats");
            if (label != null)
            {
                statsText = label;
            }
        }

        if (betsText == null)
        {
            TMP_Text label = FindByPathOrName<TMP_Text>("Panel_Result/Text_BetsSummary", "Text_BetsSummary", "BetsSummary");
            if (label != null)
            {
                betsText = label;
            }
        }

        if (winningNumberText == null)
        {
            TMP_Text label = FindByPathOrName<TMP_Text>("Panel_Result/Text_WinningNumber", "Text_WinningNumber", "WinningNumber");
            if (label != null)
            {
                winningNumberText = label;
            }
        }

        if (winningsAmountText == null)
        {
            TMP_Text label = FindByPathOrName<TMP_Text>("Panel_Result/Text_WinningsAmount", "Text_WinningsAmount", "WinningsAmount");
            if (label != null)
            {
                winningsAmountText = label;
            }
        }
    }

    private T FindByPathOrName<T>(string path, params string[] nameHints) where T : Component
    {
        Transform byPath = transform.Find(path);
        if (byPath != null)
        {
            T byPathComponent = byPath.GetComponent<T>();
            if (byPathComponent != null)
            {
                return byPathComponent;
            }
        }

        T[] all = GetComponentsInChildren<T>(true);
        for (int i = 0; i < all.Length; i++)
        {
            string candidateName = all[i].name;
            for (int j = 0; j < nameHints.Length; j++)
            {
                if (candidateName.IndexOf(nameHints[j], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return all[i];
                }
            }
        }

        return null;
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
        BindButton(addStraightBetButton, AddStraightBet);
        BindButton(spinButton, Spin);
        BindButton(clearSelectionButton, ClearSelection);
        BindButton(clearBetsButton, ClearBets);
        BindButton(togglePanelButton, TogglePanel);
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

        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.RemoveListener(TogglePanel);
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

    private void SetupPanelState()
    {
        if (slidingPanel == null)
        {
            slidingPanel = transform as RectTransform;
        }

        if (slidingPanel == null || panelStateInitialized)
        {
            return;
        }

        panelOpen = !startCollapsed;
        Vector2 pos = slidingPanel.anchoredPosition;
        pos.x = panelOpen ? GetOpenX() : GetClosedX();
        slidingPanel.anchoredPosition = pos;
        UpdateToggleText();
        panelStateInitialized = true;
    }

    public void TogglePanel()
    {
        if (slidingPanel == null)
        {
            return;
        }

        panelOpen = !panelOpen;

        if (panelSlideRoutine != null)
        {
            StopCoroutine(panelSlideRoutine);
        }

        float targetX = panelOpen ? GetOpenX() : GetClosedX();
        panelSlideRoutine = StartCoroutine(SlidePanelTo(targetX));
        UpdateToggleText();
    }

    private float GetOpenX()
    {
        return 0f;
    }

    private IEnumerator SlidePanelTo(float targetX)
    {
        Vector2 start = slidingPanel.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, panelSlideDuration);
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            slidingPanel.anchoredPosition = Vector2.LerpUnclamped(start, end, eased);
            yield return null;
        }

        slidingPanel.anchoredPosition = end;
        panelSlideRoutine = null;
    }

    private float GetClosedX()
    {
        return panelClosedX;
    }

    private void UpdateToggleText()
    {
        if (togglePanelButtonText != null)
        {
            togglePanelButtonText.text = panelOpen ? ">" : "<";
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

    private void AddStraightBet()
    {
        if (!initialized || flowService == null)
        {
            return;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("Invalid stake");
            return;
        }

        int targetNumber = straightBetDropdown != null ? straightBetDropdown.value : 0;

        if (!flowService.TryAddStraightBet(targetNumber, stake))
        {
            Debug.LogWarning("Bet rejected");
            return;
        }

        RefreshView();
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

        if (straightBetDropdown != null)
        {
            straightBetDropdown.interactable = interactable;
        }

        if (stakeInput != null)
        {
            stakeInput.interactable = interactable;
        }

        if (addStraightBetButton != null)
        {
            addStraightBetButton.interactable = interactable;
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

        if (togglePanelButton != null)
        {
            togglePanelButton.interactable = interactable;
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