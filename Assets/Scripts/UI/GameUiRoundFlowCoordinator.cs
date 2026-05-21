using UnityEngine;

public sealed class GameUiRoundFlowCoordinator
{
    private readonly GameUiFacade gameFacade;
    private readonly OutcomeSelectionUI outcomeSelectionUI;
    private readonly SpinLifecycleController spinLifecycleController;
    private readonly RoundHistoryListPresenter roundHistoryListPresenter;
    private readonly GameUiPersistenceCoordinator persistenceCoordinator;
    private readonly System.Action refreshView;

    public GameUiRoundFlowCoordinator(
        GameUiFacade gameFacade,
        OutcomeSelectionUI outcomeSelectionUI,
        SpinLifecycleController spinLifecycleController,
        RoundHistoryListPresenter roundHistoryListPresenter,
        GameUiPersistenceCoordinator persistenceCoordinator,
        System.Action refreshView)
    {
        this.gameFacade = gameFacade;
        this.outcomeSelectionUI = outcomeSelectionUI;
        this.spinLifecycleController = spinLifecycleController;
        this.roundHistoryListPresenter = roundHistoryListPresenter;
        this.persistenceCoordinator = persistenceCoordinator;
        this.refreshView = refreshView;
    }

    public void Spin(bool initialized)
    {
        if (!initialized || gameFacade == null || !gameFacade.IsReady)
        {
            return;
        }

        outcomeSelectionUI?.ApplySelection();
        spinLifecycleController?.ExecuteSpin(_ => refreshView?.Invoke());
    }

    public void HandleRoundCompleted(RoundResultData roundResult)
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

    public void HandleSpinResultPresented(RoundResultData roundResult)
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

        refreshView?.Invoke();
        persistenceCoordinator?.SaveGameState();
    }
}
