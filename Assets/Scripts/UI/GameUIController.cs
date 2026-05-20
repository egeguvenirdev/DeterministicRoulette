using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameUIController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button clearBetsButton;

    [Header("Bet Board")]
    [SerializeField] private RouletteBetBoardController betBoardController;

    [Header("Gameplay Facade")]
    [SerializeField] private GameUiFacade gameFacade;

    [Header("Sub-controllers")]
    [SerializeField] private StakeInputHandler stakeInputHandler;
    [SerializeField] private RoundResultPresenter roundResultPresenter;
    [SerializeField] private RoundHistoryListPresenter roundHistoryListPresenter;
    [SerializeField] private OutcomeSelectionUI outcomeSelectionUI;
    [SerializeField] private SpinLifecycleController spinLifecycleController;

    private bool initialized;
    private bool roundCompletedBound;
    private bool chipsChangedBound;
    private bool controlsLockedByLifecycle;
    private GameUiBetActions betActions;
    private GameUiPersistenceCoordinator persistenceCoordinator;


    private void Start()
    {
        Initialize();
        LoadGameState();
    }


    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        BindRoundCompleted();
        BindChipsChanged();
        spinLifecycleController?.OnEnable();
        BindBetBoardController();
        SaveGameState();
        UpdateSpinButtonInteractable();
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

        betActions = new GameUiBetActions(gameFacade, stakeInputHandler, this);
        persistenceCoordinator = new GameUiPersistenceCoordinator(gameFacade, roundHistoryListPresenter, roundResultPresenter);

        BindRoundCompleted();
        BindChipsChanged();
        spinLifecycleController?.OnEnable();
        BindBetBoardController();

        if (outcomeSelectionUI != null)
        {
            outcomeSelectionUI.Initialize(gameFacade);
        }

        if (spinLifecycleController != null)
        {
            spinLifecycleController.Initialize(gameFacade, roundResultPresenter);
            spinLifecycleController.ControlsInteractableRequested += SetControlsInteractable;
            spinLifecycleController.ResultPresented += HandleSpinResultPresented;
        }

        initialized = true;
        roundHistoryListPresenter?.RebuildFromState(gameFacade.GetGameState());
        RefreshView();
    }

    // --- SAVE/LOAD ---
    private void SaveGameState()
    {
        persistenceCoordinator?.SaveGameState();
    }

    private void LoadGameState()
    {
        persistenceCoordinator?.LoadGameState();
        RefreshView();
    }

    public void ResetGame()
    {
        persistenceCoordinator?.ResetGameState();
        RefreshView();
    }

    private void OnDisable()
    {
        if (gameFacade != null && roundCompletedBound)
        {
            gameFacade.RoundCompleted -= HandleRoundCompleted;
            roundCompletedBound = false;
        }

        if (gameFacade != null && chipsChangedBound)
        {
            gameFacade.ChipsChanged -= HandleChipsChanged;
            chipsChangedBound = false;
        }

        if (spinLifecycleController != null)
        {
            spinLifecycleController.ControlsInteractableRequested -= SetControlsInteractable;
            spinLifecycleController.ResultPresented -= HandleSpinResultPresented;

            spinLifecycleController.OnDisable();
        }

        UnbindBetBoardController();
    }

    public void OnSpinButtonClicked()
    {
        Spin();
    }

    public void OnClearBetsButtonClicked()
    {
        ClearBets();
    }



    private void Spin()
    {
        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            return;
        }

        outcomeSelectionUI?.ApplySelection();
        spinLifecycleController?.ExecuteSpin(result => RefreshView());
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

    public bool TryAddBetForCell(RouletteBetCellView cell)
    {
        if (betActions == null || !betActions.TryAddBetForCell(initialized, cell))
        {
            return false;
        }

        RefreshView();
        return true;
    }

    public bool TryRemoveBetForCell(RouletteBetCellView cell)
    {
        if (betActions == null || !betActions.TryRemoveBetForCell(initialized, cell))
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
        if (betActions == null || !betActions.TryAddBetFromOption(initialized, option))
        {
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
        if (betActions == null || !betActions.TryRemoveBetFromOption(initialized, option))
        {
            return false;
        }

        RefreshView();
        return true;
    }

    private void HandleRoundCompleted(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        spinLifecycleController?.HandleRoundCompleted(roundResult);

        if (spinLifecycleController == null)
        {
            HandleSpinResultPresented(roundResult);
        }
    }

    private void HandleSpinResultPresented(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        if (roundHistoryListPresenter != null)
        {
            int spinIndex = 0;
            GameStateData state = gameFacade != null ? gameFacade.GetGameState() : null;
            if (state != null)
            {
                spinIndex = Mathf.Max(1, state.spinsPlayed);
            }

            roundHistoryListPresenter.AddRound(roundResult, spinIndex);
        }

        RefreshView();
        SaveGameState();
    }

    private void SetControlsInteractable(bool interactable)
    {
        controlsLockedByLifecycle = !interactable;

        if (spinButton != null)
        {
            UpdateSpinButtonInteractable();
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

        roundResultPresenter?.PresentLastRoundFromState(state);
        roundResultPresenter?.UpdateTableTypeLabel(gameFacade.GetActiveBetSnapshot());
        UpdateSpinButtonInteractable();
    }

    private void UpdateSpinButtonInteractable()
    {
        if (spinButton == null)
        {
            return;
        }

        if (controlsLockedByLifecycle)
        {
            spinButton.interactable = false;
            return;
        }

        spinButton.interactable = gameFacade != null && gameFacade.IsReady && gameFacade.CanSpin();
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

    private void BindChipsChanged()
    {
        if (gameFacade == null || chipsChangedBound)
        {
            return;
        }

        gameFacade.ChipsChanged -= HandleChipsChanged;
        gameFacade.ChipsChanged += HandleChipsChanged;
        chipsChangedBound = true;
    }

    private void HandleChipsChanged(int _)
    {
        if (gameFacade == null)
        {
            return;
        }

        if (controlsLockedByLifecycle)
        {
            return;
        }

        GameStateData state = gameFacade.GetGameState();
        roundResultPresenter?.PresentGameState(state);
        UpdateSpinButtonInteractable();
    }
}