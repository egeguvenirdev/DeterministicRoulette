using UnityEngine;

/// <summary>
/// Manages spin lifecycle: execution, wheel animation sync, and result handling.
/// Decouples spin logic from general UI management.
/// </summary>
public class SpinLifecycleController : MonoBehaviour
{
    [SerializeField] private RouletteWheelAnimator wheelAnimator;
    [SerializeField] private bool waitForWheelAnimation = true;
    [SerializeField] private GameUiFacade gameFacade;
    [SerializeField] private RoundResultPresenter roundResultPresenter;

    public System.Action<bool> ControlsInteractableRequested;

    private bool wheelAnimatorBound;
    private RoundResultData pendingRoundResult;

    public bool IsWaitingForAnimation => waitForWheelAnimation && wheelAnimator != null && wheelAnimator.enabled;

    public void Initialize(GameUiFacade facade, RoundResultPresenter presenter)
    {
        gameFacade = facade;
        roundResultPresenter = presenter;
    }

    public void OnEnable()
    {
        BindWheelAnimator();
    }

    public void OnDisable()
    {
        UnbindWheelAnimator();
    }

    public void ExecuteSpin(System.Action<RoundResultData> onSpinComplete)
    {
        if (!CanSpin())
        {
            return;
        }

        if (IsWaitingForAnimation)
        {
            ControlsInteractableRequested?.Invoke(false);
        }

        if (!gameFacade.CanSpin())
        {
            Debug.LogWarning("Cannot spin");
            onSpinComplete?.Invoke(null);
            ControlsInteractableRequested?.Invoke(true);
            return;
        }

        RoundResultData roundResult = gameFacade.ExecuteSpin();

        if (roundResult == null)
        {
            Debug.LogWarning("Spin failed");
            ControlsInteractableRequested?.Invoke(true);
            onSpinComplete?.Invoke(null);
            return;
        }

        if (!IsWaitingForAnimation)
        {
            onSpinComplete?.Invoke(roundResult);
        }
    }

    public void HandleRoundCompleted(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        if (IsWaitingForAnimation)
        {
            pendingRoundResult = roundResult;
            return;
        }

        PresentResult(roundResult);
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
        if (IsWaitingForAnimation)
        {
            ControlsInteractableRequested?.Invoke(false);
        }
    }

    private void HandleSpinAnimationCompleted(RoundResultData roundResult)
    {
        RoundResultData finalResult = pendingRoundResult ?? roundResult;
        pendingRoundResult = null;

        PresentResult(finalResult);
        ControlsInteractableRequested?.Invoke(true);
    }

    private void PresentResult(RoundResultData roundResult)
    {
        roundResultPresenter?.PresentRoundResult(roundResult);
    }

    private bool CanSpin()
    {
        return gameFacade != null && gameFacade.IsReady;
    }
}
