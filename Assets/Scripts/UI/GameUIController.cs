using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class GameUIController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private TMP_Dropdown targetNumberDropdown;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button clearSelectionButton;
    [SerializeField] private Button clearBetsButton;

    [Header("Animation Sync")]
    [SerializeField] private RouletteWheelAnimator wheelAnimator;
    [SerializeField] private bool waitForWheelAnimation = true;

    [Header("Bet Board")]
    [SerializeField] private RouletteBetBoardController betBoardController;

    [Header("Gameplay Facade")]
    [SerializeField] private GameUiFacade gameFacade;

    [Header("Sub-controllers")]
    [SerializeField] private StakeInputHandler stakeInputHandler;
    [SerializeField] private RoundResultPresenter roundResultPresenter;

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
        Initialize();
    }

    private void OnEnable()
    {
        WireButtons();

        if (!initialized)
        {
            return;
        }

        BindRoundCompleted();
        BindWheelAnimator();
        BindBetBoardController();
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        if (!HasDependencies())
        {
            Debug.LogError($"[GameUIController] Missing gameplay facade in Inspector: {GetMissingGameplayDependencies()}", this);
            initialized = false;
            return;
        }

        if (!gameFacade.IsReady)
        {
            Debug.LogError($"[GameUIController] Game facade is not ready. Missing: {gameFacade.GetMissingDependencies()}", this);
            initialized = false;
            return;
        }

        WireButtons();
        BindRoundCompleted();
        BindWheelAnimator();
        BindBetBoardController();

        initialized = true;
        RefreshView();
    }

    private void OnDisable()
    {
        if (gameFacade != null && roundCompletedBound)
        {
            gameFacade.RoundCompleted -= HandleRoundCompleted;
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
        button.onClick.AddListener(action);
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
        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            return;
        }

        ApplySelection();

        if (ShouldWaitForWheelAnimation())
        {
            SetControlsInteractable(false);
        }

        if (!gameFacade.CanSpin())
        {
            Debug.LogWarning("Cannot spin");
            RefreshView();
            SetControlsInteractable(true);
            return;
        }

        RoundResultData roundResult = gameFacade.ExecuteSpin();

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
        if (targetNumberDropdown == null || gameFacade == null)
        {
            return;
        }

        if (targetNumberDropdown.value == 0)
        {
            gameFacade.ClearOutcomeSelection();
            return;
        }

        gameFacade.SetOutcomeSelection(targetNumberDropdown.value - 1);
    }

    private void ClearSelection()
    {
        if (gameFacade != null)
        {
            gameFacade.ClearOutcomeSelection();
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
        if (gameFacade != null)
        {
            gameFacade.ClearAllBets();
        }

        if (betBoardController != null)
        {
            betBoardController.ClearChipVisuals();
        }

        RefreshView();
    }

    public bool TryAddStraightBetForNumber(int targetNumber)
    {
        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", this);
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

        if (!gameFacade.TryAddStraightBet(targetNumber, stake))
        {
            Debug.LogWarning("Bet rejected");
            return false;
        }

        RefreshView();
        return true;
    }

    public bool TryAddBetForCell(RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return false;
        }

        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", this);
            return false;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("Invalid stake");
            return false;
        }

        if (cell.BetType == BetType.Straight && (cell.Number < 0 || cell.Number > 36))
        {
            Debug.LogWarning("Target number out of range.");
            return false;
        }

        if (!gameFacade.TryAddBet(cell.BetType, stake, cell.Number, cell.TargetNumbers))
        {
            Debug.LogWarning("Bet rejected");
            return false;
        }

        RefreshView();
        return true;
    }

    public bool TryRemoveBetForCell(RouletteBetCellView cell)
    {
        if (cell == null)
        {
            return false;
        }

        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", this);
            return false;
        }

        if (!gameFacade.TryRemoveBet(cell.BetType, cell.Number, cell.TargetNumbers))
        {
            return false;
        }

        RefreshView();
        return true;
    }

    /// <summary>
    /// Places a bet based on a bet option chosen from the popup (handles Straight through SixLine).
    /// </summary>
    public bool TryAddBetFromOption(RouletteNeighborCalculator.BetOption option)
    {
        if (option == null || option.targetNumbers == null || option.targetNumbers.Count == 0)
        {
            return false;
        }

        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", this);
            return false;
        }

        if (!TryGetStake(out int stake))
        {
            Debug.LogWarning("[GameUIController] Invalid stake.");
            return false;
        }

        // For Straight bets use targetNumbers[0] as the specific target; multi-number bets use -1.
        int targetNumber = option.betType == BetType.Straight ? option.targetNumbers[0] : -1;

        if (!gameFacade.TryAddBet(option.betType, stake, targetNumber, option.targetNumbers))
        {
            Debug.LogWarning("[GameUIController] Bet from option rejected.");
            return false;
        }

        RefreshView();
        return true;
    }

    /// <summary>
    /// Removes a bet that was placed via a popup option.
    /// Matches by bet type and targetNumbers list.
    /// </summary>
    public bool TryRemoveBetFromOption(RouletteNeighborCalculator.BetOption option)
    {
        if (option == null || option.targetNumbers == null || option.targetNumbers.Count == 0)
        {
            return false;
        }

        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            Debug.LogWarning("[GameUIController] UI is not initialized. Check serialized gameplay facade wiring.", this);
            return false;
        }

        if (!gameFacade.TryRemoveBet(option.betType, -1, option.targetNumbers))
        {
            return false;
        }

        RefreshView();
        return true;
    }

    private bool TryGetStake(out int stake)
    {
        if (stakeInputHandler != null)
        {
            return stakeInputHandler.TryGetStake(out stake);
        }

        stake = 0;
        return false;
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
        roundResultPresenter?.PresentRoundResult(roundResult);
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

        stakeInputHandler?.SetInteractable(interactable);

        if (betBoardController != null)
        {
            betBoardController.SetBoardInteractable(interactable);
        }
    }

    private void RefreshView()
    {
        if (gameFacade == null)
        {
            return;
        }

        GameStateData state = gameFacade.GetGameState();
        roundResultPresenter?.PresentGameState(state);

        int betCount = gameFacade.GetActiveBetCount();
        roundResultPresenter?.PresentBets(betCount, gameFacade.GetTotalStake());

        if (betBoardController != null && betCount == 0)
        {
            betBoardController.ClearChipVisuals();
        }

        roundResultPresenter?.UpdateTableTypeLabel(gameFacade.GetActiveBetSnapshot());
    }

    private bool HasDependencies()
    {
        return gameFacade != null;
    }

    private string GetMissingGameplayDependencies()
    {
        List<string> missing = new List<string>();

        if (gameFacade == null)
        {
            missing.Add(nameof(gameFacade));
        }

        return string.Join(", ", missing);
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
        if (gameFacade == null || roundCompletedBound)
        {
            return;
        }

        gameFacade.RoundCompleted -= HandleRoundCompleted;
        gameFacade.RoundCompleted += HandleRoundCompleted;
        roundCompletedBound = true;
    }
}