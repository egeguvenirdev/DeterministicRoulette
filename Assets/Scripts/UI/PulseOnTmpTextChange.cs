using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Plays a short pulse animation whenever the target TMP text value changes.
/// If changes are spammed, animation resets to base scale and restarts.
/// </summary>
public class PulseOnTmpTextChange : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private RectTransform targetTransform;

    [Header("Pulse")]
    [SerializeField, Min(0.01f)] private float duration = 0.2f;
    [SerializeField, Range(1f, 2f)] private float maxScaleMultiplier = 1.2f;

    private Vector3 baseScale = Vector3.one;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }

        if (targetTransform == null)
        {
            targetTransform = GetComponent<RectTransform>();
        }

        if (targetTransform != null)
        {
            baseScale = targetTransform.localScale;
        }
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        ResetToBaseScale();
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        ResetToBaseScale();
    }

    private void OnTextChanged(Object changedObject)
    {
        if (targetText == null || targetTransform == null)
        {
            return;
        }

        if (changedObject != targetText)
        {
            return;
        }

        RestartPulse();
    }

    private void RestartPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        ResetToBaseScale();
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float triangle = t < 0.5f ? t * 2f : (1f - t) * 2f;
            float multiplier = Mathf.Lerp(1f, maxScaleMultiplier, triangle);

            targetTransform.localScale = baseScale * multiplier;
            yield return null;
        }

        ResetToBaseScale();
        pulseRoutine = null;
    }

    private void ResetToBaseScale()
    {
        if (targetTransform != null)
        {
            targetTransform.localScale = baseScale;
        }
    }
}
