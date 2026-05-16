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
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text statsText;

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
            Transform button = transform.Find("Button_TogglePanel");
            if (button != null)
            {
                togglePanelButton = button.GetComponent<Button>();
            }
        }

        if (togglePanelButtonText == null && togglePanelButton != null)
        {
            togglePanelButtonText = togglePanelButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (targetNumberDropdown == null)
        {
            Transform dropdown = transform.Find("Panel_OutcomeControls/Dropdown_TargetOutcome");
            if (dropdown != null)
            {
                targetNumberDropdown = dropdown.GetComponent<TMP_Dropdown>();
            }
        }

        if (straightBetDropdown == null)
        {
            Transform dropdown = transform.Find("Panel_BetControls/Dropdown_StraightBetNumber");
            if (dropdown != null)
            {
                straightBetDropdown = dropdown.GetComponent<TMP_Dropdown>();
            }
        }

        if (stakeInput == null)
        {
            Transform input = transform.Find("Panel_BetControls/Input_StakeAmount");
            if (input != null)
            {
                stakeInput = input.GetComponent<TMP_InputField>();
            }
        }

        if (addStraightBetButton == null)
        {
            Transform button = transform.Find("Panel_BetControls/Row_BetButtons/Button_AddStraightBet");
            if (button != null)
            {
                addStraightBetButton = button.GetComponent<Button>();
            }
        }

        if (clearBetsButton == null)
        {
            Transform button = transform.Find("Panel_BetControls/Row_BetButtons/Button_ClearBets");
            if (button != null)
            {
                clearBetsButton = button.GetComponent<Button>();
            }
        }

        if (clearSelectionButton == null)
        {
            Transform button = transform.Find("Panel_OutcomeControls/Row_OutcomeButtons/Button_ClearSelection");
            if (button != null)
            {
                clearSelectionButton = button.GetComponent<Button>();
            }
        }

        if (spinButton == null)
        {
            Transform button = transform.Find("Panel_OutcomeControls/Row_OutcomeButtons/Button_Spin");
            if (button != null)
            {
                spinButton = button.GetComponent<Button>();
            }
        }

        if (chipsText == null)
        {
            Transform label = transform.Find("Panel_TopStatus/Text_Chips");
            if (label != null)
            {
                chipsText = label.GetComponent<TMP_Text>();
            }
        }

        if (statsText == null)
        {
            Transform label = transform.Find("Panel_TopStatus/Text_Stats");
            if (label != null)
            {
                statsText = label.GetComponent<TMP_Text>();
            }
        }

        if (betsText == null)
        {
            Transform label = transform.Find("Panel_Result/Text_BetsSummary");
            if (label != null)
            {
                betsText = label.GetComponent<TMP_Text>();
            }
        }

        if (resultText == null)
        {
            Transform label = transform.Find("Panel_Result/Text_Result");
            if (label != null)
            {
                resultText = label.GetComponent<TMP_Text>();
            }
        }
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
            SetResultText("Invalid stake");
            return;
        }

        int targetNumber = straightBetDropdown != null ? straightBetDropdown.value : 0;

        if (!flowService.TryAddStraightBet(targetNumber, stake))
        {
            SetResultText("Bet rejected");
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

        if (!gameManager.CanSpin())
        {
            SetResultText("Cannot spin");
            RefreshView();
            return;
        }

        RoundResultData roundResult = flowService.ExecuteSpin();

        if (roundResult == null)
        {
            SetResultText("Spin failed");
            return;
        }

        RefreshView();
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
        SetResultText("Result: " + roundResult.resultNumber + "  Win: " + roundResult.totalWinnings);
        RefreshView();
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

    private void SetResultText(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
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