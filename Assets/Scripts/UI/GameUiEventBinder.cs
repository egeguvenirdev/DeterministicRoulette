using System;

public sealed class GameUiEventBinder
{
    private readonly GameUiFacade gameFacade;
    private readonly SpinLifecycleController spinLifecycleController;
    private readonly Action<RoundResultData> onRoundCompleted;
    private readonly Action<int> onChipsChanged;
    private readonly Action<bool> onControlsInteractableRequested;
    private readonly Action<RoundResultData> onResultPresented;

    private bool roundCompletedBound;
    private bool chipsChangedBound;
    private bool controlsInteractableBound;
    private bool resultPresentedBound;

    public GameUiEventBinder(
        GameUiFacade gameFacade,
        SpinLifecycleController spinLifecycleController,
        Action<RoundResultData> onRoundCompleted,
        Action<int> onChipsChanged,
        Action<bool> onControlsInteractableRequested,
        Action<RoundResultData> onResultPresented)
    {
        this.gameFacade = gameFacade;
        this.spinLifecycleController = spinLifecycleController;
        this.onRoundCompleted = onRoundCompleted;
        this.onChipsChanged = onChipsChanged;
        this.onControlsInteractableRequested = onControlsInteractableRequested;
        this.onResultPresented = onResultPresented;
    }

    public void Bind()
    {
        if (gameFacade != null)
        {
            if (!roundCompletedBound)
            {
                gameFacade.RoundCompleted -= onRoundCompleted;
                gameFacade.RoundCompleted += onRoundCompleted;
                roundCompletedBound = true;
            }

            if (!chipsChangedBound)
            {
                gameFacade.ChipsChanged -= onChipsChanged;
                gameFacade.ChipsChanged += onChipsChanged;
                chipsChangedBound = true;
            }
        }

        if (spinLifecycleController != null)
        {
            if (!controlsInteractableBound)
            {
                spinLifecycleController.ControlsInteractableRequested -= onControlsInteractableRequested;
                spinLifecycleController.ControlsInteractableRequested += onControlsInteractableRequested;
                controlsInteractableBound = true;
            }

            if (!resultPresentedBound)
            {
                spinLifecycleController.ResultPresented -= onResultPresented;
                spinLifecycleController.ResultPresented += onResultPresented;
                resultPresentedBound = true;
            }

            spinLifecycleController.OnEnable();
        }
    }

    public void Unbind()
    {
        if (gameFacade != null)
        {
            if (roundCompletedBound)
            {
                gameFacade.RoundCompleted -= onRoundCompleted;
                roundCompletedBound = false;
            }

            if (chipsChangedBound)
            {
                gameFacade.ChipsChanged -= onChipsChanged;
                chipsChangedBound = false;
            }
        }

        if (spinLifecycleController != null)
        {
            if (controlsInteractableBound)
            {
                spinLifecycleController.ControlsInteractableRequested -= onControlsInteractableRequested;
                controlsInteractableBound = false;
            }

            if (resultPresentedBound)
            {
                spinLifecycleController.ResultPresented -= onResultPresented;
                resultPresentedBound = false;
            }

            spinLifecycleController.OnDisable();
        }
    }
}
