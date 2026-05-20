public sealed class GameUiPersistenceCoordinator
{
    private readonly GameUiFacade gameFacade;
    private readonly RoundHistoryListPresenter roundHistoryListPresenter;
    private readonly RoundResultPresenter roundResultPresenter;

    public GameUiPersistenceCoordinator(
        GameUiFacade gameFacade,
        RoundHistoryListPresenter roundHistoryListPresenter,
        RoundResultPresenter roundResultPresenter)
    {
        this.gameFacade = gameFacade;
        this.roundHistoryListPresenter = roundHistoryListPresenter;
        this.roundResultPresenter = roundResultPresenter;
    }

    public void SaveGameState()
    {
        if (gameFacade == null)
        {
            return;
        }

        GameStateData state = gameFacade.GetGameState();
        if (state == null)
        {
            return;
        }

        GameSaveManager.SaveGame(state);
    }

    public void LoadGameState()
    {
        if (gameFacade == null)
        {
            return;
        }

        if (GameSaveManager.TryLoadGame(out GameStateData loadedState))
        {
            gameFacade.ApplyLoadedState(loadedState);
            GameStateData stateAfterLoad = gameFacade.GetGameState();
            roundHistoryListPresenter?.RebuildFromState(stateAfterLoad);
            roundResultPresenter?.PresentLastRoundFromState(stateAfterLoad);
            return;
        }

        roundResultPresenter?.ClearRoundResult();
    }

    public void ResetGameState()
    {
        GameSaveManager.ResetGame();

        if (gameFacade != null)
        {
            gameFacade.ResetGameState();
            roundHistoryListPresenter?.RebuildFromState(gameFacade.GetGameState());
        }

        roundResultPresenter?.ClearRoundResult();
    }
}
