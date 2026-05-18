using UnityEngine;
using TMPro;

/// <summary>
/// Manages chip-value and stake-count input fields: normalization, wiring, and stake calculation.
/// Attach to the same GameObject as GameUIController and wire fields in the Inspector.
/// </summary>
public class StakeInputHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField chipValueInput;
    [SerializeField] private TMP_InputField stakeCountInput;

    private void OnEnable()
    {
        Wire();
        Normalize();
    }

    private void OnDisable()
    {
        Unwire();
    }

    public void Wire()
    {
        if (chipValueInput != null)
        {
            chipValueInput.onEndEdit.RemoveListener(HandleChipValueEndEdit);
            chipValueInput.onEndEdit.AddListener(HandleChipValueEndEdit);
        }

        if (stakeCountInput != null)
        {
            stakeCountInput.onEndEdit.RemoveListener(HandleStakeCountEndEdit);
            stakeCountInput.onEndEdit.AddListener(HandleStakeCountEndEdit);
        }
    }

    public void Unwire()
    {
        if (chipValueInput != null)
        {
            chipValueInput.onEndEdit.RemoveListener(HandleChipValueEndEdit);
        }

        if (stakeCountInput != null)
        {
            stakeCountInput.onEndEdit.RemoveListener(HandleStakeCountEndEdit);
        }
    }

    public void Normalize()
    {
        NormalizeInputField(chipValueInput);
        NormalizeInputField(stakeCountInput);
    }

    public bool TryGetStake(out int stake)
    {
        stake = 0;

        if (chipValueInput == null)
        {
            return false;
        }

        int chipValue = ParsePositiveOrDefault(chipValueInput, 1);
        int stakeCount = ParsePositiveOrDefault(stakeCountInput, 1);

        long totalStake = (long)chipValue * stakeCount;
        if (totalStake <= 0 || totalStake > int.MaxValue)
        {
            return false;
        }

        stake = (int)totalStake;
        return true;
    }

    public void SetInteractable(bool interactable)
    {
        if (chipValueInput != null)
        {
            chipValueInput.interactable = interactable;
        }

        if (stakeCountInput != null)
        {
            stakeCountInput.interactable = interactable;
        }
    }

    private void HandleChipValueEndEdit(string _) => NormalizeInputField(chipValueInput);
    private void HandleStakeCountEndEdit(string _) => NormalizeInputField(stakeCountInput);

    private static void NormalizeInputField(TMP_InputField input)
    {
        if (input == null)
        {
            return;
        }

        if (!int.TryParse(input.text, out int value) || value <= 0)
        {
            input.text = "1";
        }
    }

    private static int ParsePositiveOrDefault(TMP_InputField input, int fallback)
    {
        if (input == null)
        {
            return fallback;
        }

        if (!int.TryParse(input.text, out int value) || value <= 0)
        {
            input.text = fallback.ToString();
            return fallback;
        }

        return value;
    }
}
