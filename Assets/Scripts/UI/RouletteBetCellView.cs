using System;
using UnityEngine;
using UnityEngine.UI;

public class RouletteBetCellView : MonoBehaviour
{
    [Header("Cell Data")]
    [SerializeField] private int number;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private RectTransform chipAnchor;
    [SerializeField] private Graphic highlightGraphic;

    [Header("Highlight")]
    [SerializeField] private bool useHighlightColor = true;
    [SerializeField] private Color normalHighlightColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private Color activeHighlightColor = new Color(1f, 0.92f, 0.45f, 0.42f);

    public event Action<RouletteBetCellView> Clicked;

    public int Number => number;
    public Button Button => button;

    public RectTransform ChipAnchor
    {
        get
        {
            if (chipAnchor != null)
            {
                return chipAnchor;
            }

            return transform as RectTransform;
        }
    }

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (chipAnchor == null)
        {
            chipAnchor = transform as RectTransform;
        }

        if (highlightGraphic == null)
        {
            highlightGraphic = GetComponent<Graphic>();
        }

        SetHighlighted(false);
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void SetHighlighted(bool active)
    {
        if (highlightGraphic == null)
        {
            return;
        }

        if (useHighlightColor)
        {
            highlightGraphic.color = active ? activeHighlightColor : normalHighlightColor;
        }
        else
        {
            highlightGraphic.enabled = active;
        }
    }

    private void HandleClicked()
    {
        Clicked?.Invoke(this);
    }
}
