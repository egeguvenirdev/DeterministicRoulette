using UnityEngine;
using UnityEngine.UI;

public sealed class GameUiViewSync
{
    private readonly GameUiFacade gameFacade;
    private readonly Button spinButton;
    private readonly Button clearBetsButton;
    private readonly StakeInputHandler stakeInputHandler;
    private readonly RouletteBetBoardController betBoardController;
    private readonly RoundResultPresenter roundResultPresenter;

    private bool controlsLockedByLifecycle;

    public GameUiViewSync(
        GameUiFacade gameFacade,
        Button spinButton,
        Button clearBetsButton,
        StakeInputHandler stakeInputHandler,
        RouletteBetBoardController betBoardController,
        RoundResultPresenter roundResultPresenter)
    {
        this.gameFacade = gameFacade;
        this.spinButton = spinButton;
        this.clearBetsButton = clearBetsButton;
        this.stakeInputHandler = stakeInputHandler;
        this.betBoardController = betBoardController;
        this.roundResultPresenter = roundResultPresenter;
    }

    public void SetControlsInteractable(bool interactable)
    {
        controlsLockedByLifecycle = !interactable;

        UpdateSpinButtonInteractable();

        if (clearBetsButton != null)
        {
            clearBetsButton.interactable = interactable;
        }

        stakeInputHandler?.SetInteractable(interactable);
        betBoardController?.SetBoardInteractable(interactable);
    }

    public void HandleChipsChanged()
    {
        if (gameFacade == null || controlsLockedByLifecycle)
        {
            return;
        }

        GameStateData state = gameFacade.GetGameState();
        roundResultPresenter?.PresentGameState(state);
        UpdateSpinButtonInteractable();
    }

    public void RefreshView()
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
}
