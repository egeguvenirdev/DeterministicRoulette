using UnityEngine;

/// <summary>
/// Manages spin lifecycle and delays result presentation until wheel animation completes.
/// </summary>
public class SpinLifecycleController : MonoBehaviour
{
	[SerializeField] private RouletteWheelAnimator wheelAnimator;
	[SerializeField] private bool waitForWheelAnimation = true;
	[SerializeField] private GameUiFacade gameFacade;
	[SerializeField] private RoundResultPresenter roundResultPresenter;

	public System.Action<bool> ControlsInteractableRequested;
	public System.Action<RoundResultData> ResultPresented;

	private bool wheelAnimatorBound;
	private RoundResultData pendingRoundResult;

	private bool IsWaitingForAnimation => waitForWheelAnimation && wheelAnimator != null && wheelAnimator.enabled;

	public void Initialize(GameUiFacade facade, RoundResultPresenter presenter)
	{
		gameFacade = facade;
		roundResultPresenter = presenter;
		EnsureWheelAnimatorReference();
	}

	public void OnEnable()
	{
		EnsureWheelAnimatorReference();
		BindWheelAnimator();
	}

	public void OnDisable()
	{
		UnbindWheelAnimator();
		pendingRoundResult = null;
	}

	public void ExecuteSpin(System.Action<RoundResultData> onSpinComplete)
	{
		if (gameFacade == null || !gameFacade.IsReady)
		{
			return;
		}

		EnsureWheelAnimatorReference();

		if (IsWaitingForAnimation)
		{
			ControlsInteractableRequested?.Invoke(false);
		}

		if (!gameFacade.CanSpin())
		{
			onSpinComplete?.Invoke(null);
			ControlsInteractableRequested?.Invoke(true);
			return;
		}

		RoundResultData roundResult = gameFacade.ExecuteSpin();
		if (roundResult == null)
		{
			onSpinComplete?.Invoke(null);
			ControlsInteractableRequested?.Invoke(true);
			return;
		}

		if (!IsWaitingForAnimation)
		{
			PresentResult(roundResult);
			ControlsInteractableRequested?.Invoke(true);
			onSpinComplete?.Invoke(roundResult);
		}
	}

	public void HandleRoundCompleted(RoundResultData roundResult)
	{
		if (roundResult == null)
		{
			return;
		}

		EnsureWheelAnimatorReference();

		if (IsWaitingForAnimation)
		{
			pendingRoundResult = roundResult;
			return;
		}
	}

	private void HandleSpinAnimationCompleted(RoundResultData roundResult)
	{
		RoundResultData finalResult = pendingRoundResult ?? roundResult;
		pendingRoundResult = null;

		if (finalResult != null)
		{
			PresentResult(finalResult);
		}

		ControlsInteractableRequested?.Invoke(true);
	}

	private void EnsureWheelAnimatorReference()
	{
		if (wheelAnimator == null)
		{
			wheelAnimator = Object.FindFirstObjectByType<RouletteWheelAnimator>();
		}
	}

	private void BindWheelAnimator()
	{
		if (wheelAnimator == null || wheelAnimatorBound)
		{
			return;
		}

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

		wheelAnimator.SpinAnimationCompleted -= HandleSpinAnimationCompleted;
		wheelAnimatorBound = false;
	}

	private void PresentResult(RoundResultData roundResult)
	{
		roundResultPresenter?.PresentRoundResult(roundResult);
		ResultPresented?.Invoke(roundResult);
	}
}
