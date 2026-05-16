using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Visual-only deterministic roulette animation driver.
/// Strict manual mode: all required references are assigned from Inspector.
/// </summary>
public class RouletteWheelAnimator : MonoBehaviour
{
    private static readonly int[] EuropeanOrder =
    {
        0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
        5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };

    [Header("Manual References")]
    [SerializeField] private RouletteGameManager gameManager;
    [SerializeField] private Transform wheelSpin;
    [SerializeField] private Transform ballContainer;
    [SerializeField] private Transform ball;

    [Header("Direction (-1 left, +1 right)")]
    [SerializeField] private int wheelDirection = 1;
    [SerializeField] private int ballDirection = -1;

    [Header("Turns")]
    [SerializeField] private float wheelTurns = 5f;
    [SerializeField] private float ballTurns = 9f;

    [Header("Duration Per Turn (seconds)")]
    [SerializeField] private float wheelSecondsPerTurn = 0.45f;
    [SerializeField] private float ballSecondsPerTurn = 0.5f;
    [SerializeField] private float settleDuration = 0.42f;

    [Header("Computed (read-only)")]
    [SerializeField] private float computedWheelDuration;
    [SerializeField] private float computedBallDuration;
    [SerializeField] private float computedTotalDuration;

    [Header("Pocket Mapping")]
    [SerializeField] private float pocketAngleOffset = 0f;
    [SerializeField] private float settleBallAngle = 90f;

    [Header("Ball Spiral")]
    [SerializeField] private float outerOrbitRadius = 0.5f;
    [SerializeField] private float innerPocketRadius = 0.35f;
    [SerializeField] private float finalRadius = 0.33f;
    [SerializeField] private float orbitHeight = 0.08f;
    [SerializeField] private float pocketHeight = 0.02f;
    [SerializeField] private float settleDropFromHeightOffset = 0.012f;

    [Header("Ball State Timing (normalized)")]
    [SerializeField] private float ballFastPhaseDuration = 0.38f;
    [SerializeField] private float ballSlowPhaseDuration = 0.42f;

    [Header("Ball State Speeds (relative)")]
    [SerializeField] private float ballFastPhaseSpeed = 1.0f;
    [SerializeField] private float ballSlowPhaseSpeed = 0.55f;
    [SerializeField] private float ballFinalPhaseSpeed = 0.22f;

    [Header("Settle Jitter")]
    [SerializeField] private float settleAngleJitter = 2.2f;
    [SerializeField] private float settleRadiusJitter = 0.008f;
    [SerializeField] private float settleHeightJitter = 0.004f;

    public event Action SpinAnimationStarted;
    public event Action<RoundResultData> SpinAnimationCompleted;

    public bool IsAnimating { get; private set; }
    public float TotalAnimationDuration => computedTotalDuration;

    private Coroutine activeSpinRoutine;
    private float wheelAngle;
    private float ballAngle;
    private bool bound;
    private Quaternion wheelBaseLocalRotation;
    private Quaternion ballContainerBaseLocalRotation;

    private void Awake()
    {
        RecalculateDurations();
        if (wheelSpin != null)
        {
            wheelBaseLocalRotation = wheelSpin.localRotation;
        }

        if (ballContainer != null)
        {
            ballContainerBaseLocalRotation = ballContainer.localRotation;
        }

        if (ballContainer != null && ball != null)
        {
            PlaceBall(ballAngle, outerOrbitRadius, orbitHeight);
        }
    }

    private void OnEnable()
    {
        BindToGameEvents();
    }

    private void OnDisable()
    {
        UnbindFromGameEvents();
    }

    private void OnValidate()
    {
        if (wheelDirection == 0)
        {
            wheelDirection = 1;
        }

        if (ballDirection == 0)
        {
            ballDirection = -1;
        }

        wheelDirection = wheelDirection > 0 ? 1 : -1;
        ballDirection = ballDirection > 0 ? 1 : -1;

        wheelTurns = Mathf.Max(0f, wheelTurns);
        ballTurns = Mathf.Max(0f, ballTurns);
        wheelSecondsPerTurn = Mathf.Max(0.01f, wheelSecondsPerTurn);
        ballSecondsPerTurn = Mathf.Max(0.01f, ballSecondsPerTurn);
        settleDuration = Mathf.Max(0.01f, settleDuration);
        finalRadius = Mathf.Max(0f, finalRadius);
        settleDropFromHeightOffset = Mathf.Max(0f, settleDropFromHeightOffset);

        ballFastPhaseDuration = Mathf.Clamp(ballFastPhaseDuration, 0.05f, 0.9f);
        ballSlowPhaseDuration = Mathf.Clamp(ballSlowPhaseDuration, 0.05f, 0.9f - ballFastPhaseDuration);
        if (1f - (ballFastPhaseDuration + ballSlowPhaseDuration) < 0.05f)
        {
            ballSlowPhaseDuration = Mathf.Max(0.05f, 0.95f - ballFastPhaseDuration);
        }

        ballFastPhaseSpeed = Mathf.Max(0.01f, ballFastPhaseSpeed);
        ballSlowPhaseSpeed = Mathf.Max(0.01f, ballSlowPhaseSpeed);
        ballFinalPhaseSpeed = Mathf.Max(0.01f, ballFinalPhaseSpeed);
        RecalculateDurations();
    }

    public void BindToGameEvents()
    {
        if (gameManager == null || bound)
        {
            return;
        }

        gameManager.RoundCompleted -= HandleRoundCompleted;
        gameManager.RoundCompleted += HandleRoundCompleted;
        bound = true;
    }

    public void UnbindFromGameEvents()
    {
        if (gameManager == null || !bound)
        {
            return;
        }

        gameManager.RoundCompleted -= HandleRoundCompleted;
        bound = false;
    }

    public void PlayForNumber(int winningNumber)
    {
        PlayForRound(new RoundResultData { resultNumber = winningNumber });
    }

    public void PlayForRound(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        if (wheelSpin == null || ballContainer == null || ball == null)
        {
            Debug.LogWarning("[RouletteWheelAnimator] gameManager, wheelSpin, ballContainer and ball references are required.", this);
            return;
        }

        wheelBaseLocalRotation = wheelSpin.localRotation;
        ballContainerBaseLocalRotation = ballContainer.localRotation;

        if (activeSpinRoutine != null)
        {
            StopCoroutine(activeSpinRoutine);
        }

        activeSpinRoutine = StartCoroutine(PlaySpinAnimation(roundResult));
    }

    private void HandleRoundCompleted(RoundResultData roundResult)
    {
        if (roundResult == null)
        {
            return;
        }

        PlayForRound(roundResult);
    }

    private IEnumerator PlaySpinAnimation(RoundResultData roundResult)
    {
        int pocketIndex = GetPocketIndex(roundResult.resultNumber);
        if (pocketIndex < 0)
        {
            yield break;
        }

        RecalculateDurations();
        IsAnimating = true;
        SpinAnimationStarted?.Invoke();

        float wheelSign = wheelDirection > 0 ? 1f : -1f;
        float ballSign = ballDirection > 0 ? 1f : -1f;
        float startBallAngle = ballAngle;

        float pocketStep = 360f / EuropeanOrder.Length;
        float pocketAngle = pocketAngleOffset + pocketIndex * pocketStep;

        float desiredWheelModulo = NormalizeAngle(settleBallAngle - pocketAngle);
        float targetWheelAngle = ComputeTargetWheelAngleByTurns(wheelAngle, desiredWheelModulo, wheelTurns, wheelSign);
        float targetBallAngle = ComputeTargetWheelAngleByTurns(startBallAngle, settleBallAngle, ballTurns, ballSign);
        float totalBallDistance = Mathf.Abs(targetBallAngle - startBallAngle);

        float motionDuration = Mathf.Max(computedWheelDuration, computedBallDuration);
        motionDuration = Mathf.Max(0.01f, motionDuration);

        float elapsed = 0f;
        float startWheelAngle = wheelAngle;

        while (elapsed < motionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / motionDuration);

            float wheelT = computedWheelDuration <= 0.0001f ? 1f : Mathf.Clamp01(elapsed / computedWheelDuration);
            float ballTRaw = computedBallDuration <= 0.0001f ? 1f : Mathf.Clamp01(elapsed / computedBallDuration);

            float nextWheelAngle = Mathf.Lerp(startWheelAngle, targetWheelAngle, EaseOutCubic(wheelT));
            ApplyWheelAngle(nextWheelAngle);
            wheelAngle = nextWheelAngle;

            float traveledDistance = EvaluateBallDistanceByStates(ballTRaw, totalBallDistance);
            ballAngle = startBallAngle + ballSign * traveledDistance;

            float finalPhaseStart = ballFastPhaseDuration + ballSlowPhaseDuration;
            float preFinalProgress = Mathf.Clamp01(ballTRaw / Mathf.Max(0.001f, finalPhaseStart));
            float finalProgress = Mathf.Clamp01((ballTRaw - finalPhaseStart) / Mathf.Max(0.001f, 1f - finalPhaseStart));

            float preRadius = Mathf.Lerp(outerOrbitRadius, innerPocketRadius, EaseOutCubic(preFinalProgress));
            float radius = Mathf.Lerp(preRadius, finalRadius, EaseInOutCubic(finalProgress));

            float preHeight = Mathf.Lerp(orbitHeight, pocketHeight + settleDropFromHeightOffset, EaseOutCubic(preFinalProgress));
            float height = Mathf.Lerp(preHeight, pocketHeight, EaseOutCubic(finalProgress));

            PlaceBall(ballAngle, radius, height);
            yield return null;
        }

        wheelAngle = targetWheelAngle;
        ballAngle = targetBallAngle;
        PlaceBall(ballAngle, finalRadius, pocketHeight);

        yield return StartCoroutine(PlaySettleJitter());

        PlaceBall(ballAngle, finalRadius, pocketHeight);
        activeSpinRoutine = null;
        IsAnimating = false;
        SpinAnimationCompleted?.Invoke(roundResult);
    }

    private IEnumerator PlaySettleJitter()
    {
        float elapsed = 0f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            float damping = 1f - t;

            float angleNoise = Mathf.Sin(t * Mathf.PI * 8f) * settleAngleJitter * damping;
            float radiusNoise = Mathf.Sin(t * Mathf.PI * 10f + 0.6f) * settleRadiusJitter * damping;
            float heightNoise = Mathf.Sin(t * Mathf.PI * 12f + 1.2f) * settleHeightJitter * damping;

            PlaceBall(
                settleBallAngle + angleNoise,
                finalRadius + radiusNoise,
                pocketHeight + heightNoise);

            yield return null;
        }
    }

    private void RecalculateDurations()
    {
        computedWheelDuration = Mathf.Max(0f, wheelTurns) * Mathf.Max(0.01f, wheelSecondsPerTurn);
        computedBallDuration = Mathf.Max(0f, ballTurns) * Mathf.Max(0.01f, ballSecondsPerTurn);
        computedTotalDuration = Mathf.Max(computedWheelDuration, computedBallDuration) + Mathf.Max(0.01f, settleDuration);
    }

    private void ApplyWheelAngle(float absoluteAngle)
    {
        if (wheelSpin == null)
        {
            return;
        }

        wheelSpin.localRotation = wheelBaseLocalRotation * Quaternion.AngleAxis(absoluteAngle, Vector3.forward);
    }

    private void PlaceBall(float localAngleDeg, float radius, float localHeight)
    {
        if (ballContainer == null || ball == null)
        {
            return;
        }

        ballContainer.localRotation = ballContainerBaseLocalRotation * Quaternion.AngleAxis(localAngleDeg, Vector3.forward);
        ball.localPosition = new Vector3(radius, 0f, localHeight);
    }

    private static float ComputeTargetWheelAngleByTurns(float currentAngle, float desiredModulo, float turns, float directionSign)
    {
        float signedTurns = Mathf.Max(0f, turns) * 360f * directionSign;
        float minTarget = currentAngle + signedTurns;
        float target = currentAngle;

        if (directionSign >= 0f)
        {
            target = Mathf.Floor(currentAngle / 360f) * 360f + desiredModulo;
            while (target < minTarget)
            {
                target += 360f;
            }
        }
        else
        {
            target = Mathf.Ceil(currentAngle / 360f) * 360f + desiredModulo;
            while (target > minTarget)
            {
                target -= 360f;
            }
        }

        return target;
    }

    private float EvaluateBallDistanceByStates(float normalizedTime, float totalDistance)
    {
        float t = Mathf.Clamp01(normalizedTime);
        float d1 = ballFastPhaseDuration;
        float d2 = ballSlowPhaseDuration;
        float d3 = Mathf.Max(0.001f, 1f - d1 - d2);

        float s1 = ballFastPhaseSpeed;
        float s2 = ballSlowPhaseSpeed;
        float s3 = ballFinalPhaseSpeed;

        float seg1 = s1 * d1;
        float seg2 = ((s1 + s2) * 0.5f) * d2;
        float seg3 = ((s2 + s3) * 0.5f) * d3;
        float baseTotal = Mathf.Max(0.0001f, seg1 + seg2 + seg3);
        float distanceScale = totalDistance / baseTotal;

        float baseDistance;
        if (t <= d1)
        {
            baseDistance = s1 * t;
        }
        else if (t <= d1 + d2)
        {
            float u = t - d1;
            float a = (s2 - s1) / Mathf.Max(0.0001f, d2);
            baseDistance = seg1 + (s1 * u + 0.5f * a * u * u);
        }
        else
        {
            float u = t - d1 - d2;
            float a = (s3 - s2) / Mathf.Max(0.0001f, d3);
            baseDistance = seg1 + seg2 + (s2 * u + 0.5f * a * u * u);
        }

        return baseDistance * distanceScale;
    }

    private static float NormalizeAngle(float angle)
    {
        float wrapped = angle % 360f;
        return wrapped < 0f ? wrapped + 360f : wrapped;
    }

    private static float EaseOutCubic(float t)
    {
        float inv = 1f - Mathf.Clamp01(t);
        return 1f - inv * inv * inv;
    }

    private static float EaseInOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        if (t < 0.5f)
        {
            return 4f * t * t * t;
        }

        float f = -2f * t + 2f;
        return 1f - (f * f * f) / 2f;
    }

    private static int GetPocketIndex(int number)
    {
        for (int i = 0; i < EuropeanOrder.Length; i++)
        {
            if (EuropeanOrder[i] == number)
            {
                return i;
            }
        }

        return -1;
    }
}
