using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    private bool initialized;
    private bool panelOpen;
    private Coroutine panelSlideRoutine;
    private bool roundCompletedBound;

    private void Awake()
    {
        AutoResolveUIReferences();
        EnsureGameplayReferences();
        SetupDropdowns();
        WireButtons();
        SetupPanelState();
        RefreshView();
    }

    private void OnEnable()
    {
        AutoResolveUIReferences();
        EnsureGameplayReferences();
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

        SetupDropdowns();
        WireButtons();

        EnsureGameplayReferences();

        SetupPanelState();

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

    private void EnsureGameplayReferences()
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

        if (gameManager != null && !roundCompletedBound)
        {
            gameManager.RoundCompleted -= HandleRoundCompleted;
            gameManager.RoundCompleted += HandleRoundCompleted;
            roundCompletedBound = true;
        }

        if (!initialized && (betManager != null || gameManager != null || outcomeSelector != null || statisticsManager != null))
        {
            initialized = true;
            Debug.Log("[UI] Auto-initialized gameplay references: gameManager=" + (gameManager != null) + ", outcomeSelector=" + (outcomeSelector != null) + ", betManager=" + (betManager != null) + ", statisticsManager=" + (statisticsManager != null));
        }
    }

    private void SetupDropdowns()
    {
        SetupDropdown(targetNumberDropdown, true);
        SetupDropdown(straightBetDropdown, false);
    }

    private void AutoResolveUIReferences()
    {
        if (slidingPanel == null)
        {
            slidingPanel = transform as RectTransform;
        }

        if (targetNumberDropdown == null)
        {
            Transform t = transform.Find("Panel_OutcomeControls/Dropdown_TargetOutcome");
            if (t != null)
            {
                targetNumberDropdown = t.GetComponent<TMP_Dropdown>();
            }
        }

        if (straightBetDropdown == null)
        {
            Transform t = transform.Find("Panel_BetControls/Dropdown_StraightBetNumber");
            if (t != null)
            {
                straightBetDropdown = t.GetComponent<TMP_Dropdown>();
            }
        }

        if (stakeInput == null)
        {
            Transform t = transform.Find("Panel_BetControls/Input_StakeAmount");
            if (t != null)
            {
                stakeInput = t.GetComponent<TMP_InputField>();
            }
        }

        if (addStraightBetButton == null)
        {
            Transform t = transform.Find("Panel_BetControls/Row_BetButtons/Button_AddStraightBet");
            if (t != null)
            {
                addStraightBetButton = t.GetComponent<Button>();
            }
        }

        if (clearBetsButton == null)
        {
            Transform t = transform.Find("Panel_BetControls/Row_BetButtons/Button_ClearBets");
            if (t != null)
            {
                clearBetsButton = t.GetComponent<Button>();
            }
        }

        if (clearSelectionButton == null)
        {
            Transform t = transform.Find("Panel_OutcomeControls/Row_OutcomeButtons/Button_ClearSelection");
            if (t != null)
            {
                clearSelectionButton = t.GetComponent<Button>();
            }
        }

        if (spinButton == null)
        {
            Transform t = transform.Find("Panel_OutcomeControls/Row_OutcomeButtons/Button_Spin");
            if (t != null)
            {
                spinButton = t.GetComponent<Button>();
            }
        }

        if (togglePanelButton == null)
        {
            Transform t = transform.Find("Button_TogglePanel");
            if (t != null)
            {
                togglePanelButton = t.GetComponent<Button>();
            }
        }

        if (togglePanelButtonText == null && togglePanelButton != null)
        {
            togglePanelButtonText = togglePanelButton.GetComponentInChildren<TMP_Text>(true);
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

        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.RemoveListener(TogglePanel);
            togglePanelButton.onClick.AddListener(TogglePanel);
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

        if (togglePanelButton != null)
        {
            togglePanelButton.onClick.RemoveListener(TogglePanel);
        }
    }

    private void SetupPanelState()
    {
        if (slidingPanel == null)
        {
            slidingPanel = transform as RectTransform;
        }

        panelOpen = !startCollapsed;
        Vector2 pos = slidingPanel.anchoredPosition;
        pos.x = panelOpen ? GetOpenX() : GetClosedX();
        slidingPanel.anchoredPosition = pos;
        UpdateToggleText();
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
        Debug.Log("[UI] AddStraightBet clicked");

        EnsureGameplayReferences();

        if (betManager == null)
        {
            Debug.Log("[UI] AddStraightBet ignored: betManager=False");
            return;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.Log("[UI] AddStraightBet invalid stake input");
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
            Debug.Log("[UI] AddStraightBet rejected: target=" + targetNumber + ", stake=" + stake);
            SetResultText("Bet rejected");
            return;
        }

        Debug.Log("[UI] AddStraightBet success: target=" + targetNumber + ", stake=" + stake + ", totalBets=" + betManager.ActiveBets.Count);

        RefreshView();
    }

    private void Spin()
    {
        Debug.Log("[UI] Spin clicked");

        EnsureGameplayReferences();

        if (gameManager == null || outcomeSelector == null)
        {
            Debug.Log("[UI] Spin ignored: gameManager=" + (gameManager != null) + ", outcomeSelector=" + (outcomeSelector != null));
            return;
        }

        ApplySelection();

        if (!gameManager.CanSpin())
        {
            Debug.Log("[UI] Spin blocked: CanSpin returned false");
            SetResultText("Cannot spin");
            RefreshView();
            return;
        }

        RoundResultData roundResult = gameManager.Spin();

        if (roundResult == null)
        {
            Debug.Log("[UI] Spin failed: gameManager returned null result");
            SetResultText("Spin failed");
            return;
        }

        Debug.Log("[UI] Spin success: result=" + roundResult.resultNumber + ", winnings=" + roundResult.totalWinnings + ", losses=" + roundResult.totalLosses);

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
        Debug.Log("[UI] ClearSelection clicked");

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
        Debug.Log("[UI] ClearBets clicked");

        if (betManager != null)
        {
            Debug.Log("[UI] ClearBets before clear: count=" + betManager.ActiveBets.Count + ", stake=" + betManager.GetTotalStake());
            betManager.ClearBets();
            Debug.Log("[UI] ClearBets after clear: count=" + betManager.ActiveBets.Count + ", stake=" + betManager.GetTotalStake());
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
        Debug.Log("[UI] RoundCompleted: result=" + roundResult.resultNumber + ", selected=" + roundResult.selectedNumber + ", winnings=" + roundResult.totalWinnings + ", losses=" + roundResult.totalLosses);
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