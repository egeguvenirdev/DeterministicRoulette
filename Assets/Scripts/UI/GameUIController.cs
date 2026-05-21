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
    private GameUiBetActions betActions;
    private GameUiPersistenceCoordinator persistenceCoordinator;
    private GameUiViewSync viewSync;
    private GameUiRoundFlowCoordinator roundFlowCoordinator;
    private GameUiEventBinder eventBinder;


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

        eventBinder?.Bind();
        BindBetBoardController();
        SaveGameState();
        RefreshView();
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
        viewSync = new GameUiViewSync(gameFacade, spinButton, clearBetsButton, stakeInputHandler, betBoardController, roundResultPresenter);
        roundFlowCoordinator = new GameUiRoundFlowCoordinator(
            gameFacade,
            outcomeSelectionUI,
            spinLifecycleController,
            roundHistoryListPresenter,
            persistenceCoordinator,
            RefreshView);

        eventBinder = new GameUiEventBinder(
            gameFacade,
            spinLifecycleController,
            roundFlowCoordinator.HandleRoundCompleted,
            _ => viewSync.HandleChipsChanged(),
            viewSync.SetControlsInteractable,
            roundFlowCoordinator.HandleSpinResultPresented);

        eventBinder.Bind();
        BindBetBoardController();

        if (outcomeSelectionUI != null)
        {
            outcomeSelectionUI.Initialize(gameFacade);
        }

        spinLifecycleController?.Initialize(gameFacade, roundResultPresenter);

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
        eventBinder?.Unbind();

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
        roundFlowCoordinator?.Spin(initialized);
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

        viewSync?.RefreshView();
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

    private void RefreshView()
    {
        viewSync?.RefreshView();
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

}