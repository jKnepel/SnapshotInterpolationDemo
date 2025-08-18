using jKnepel.ProteusNet.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text numberOfObjects;
    [SerializeField] private UIButton upButton;
    [SerializeField] private UIButton leftButton;
    [SerializeField] private UIButton downButton;
    [SerializeField] private UIButton rightButton;
    [SerializeField] private Toggle useInterpolation;
    [SerializeField] private TMP_InputField interpolationInterval;
    [SerializeField] private Toggle useExtrapolation;
    [SerializeField] private TMP_InputField extrapolationInterval;

    private int _numberOfObjects;
    public int NumberOfObjects
    {
        get => _numberOfObjects;
        set
        {
            _numberOfObjects = value;
            numberOfObjects.text = $"Collected: {value}";
        }
    }

    public Vector2 MenuInput { get; private set; }

    private void Awake()
    {
        upButton.ClickStart += () => SetInputVertical(1f);
        upButton.ClickEnd   += () => ResetInputVertical(1f);

        downButton.ClickStart += () => SetInputVertical(-1f);
        downButton.ClickEnd   += () => ResetInputVertical(-1f);

        rightButton.ClickStart += () => SetInputHorizontal(1f);
        rightButton.ClickEnd   += () => ResetInputHorizontal(1f);

        leftButton.ClickStart += () => SetInputHorizontal(-1f);
        leftButton.ClickEnd   += () => ResetInputHorizontal(-1f);
        
        useInterpolation.onValueChanged.AddListener(val =>
        {
            foreach (NetworkTransform networkTransform in FindObjectsByType<NetworkTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                networkTransform.UseInterpolation = val;
        });
        interpolationInterval.onValueChanged.AddListener(val =>
        {
            if (float.TryParse(val, out float interval))
                foreach (NetworkTransform networkTransform in FindObjectsByType<NetworkTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    networkTransform.InterpolationInterval = interval;
        });
        useExtrapolation.onValueChanged.AddListener(val =>
        {
            foreach (NetworkTransform networkTransform in FindObjectsByType<NetworkTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                networkTransform.UseExtrapolation = val;
        });
        extrapolationInterval.onValueChanged.AddListener(val =>
        {
            if (float.TryParse(val, out float interval))
                foreach (NetworkTransform networkTransform in FindObjectsByType<NetworkTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    networkTransform.ExtrapolationInterval = interval;
        });
    }

    private void SetInputVertical(float input) => MenuInput = new Vector2(MenuInput.x, input);
    private void SetInputHorizontal(float input) => MenuInput = new Vector2(input, MenuInput.y);

    private void ResetInputVertical(float releasedInput)
    {
        // Only reset if the released input matches current state
        if (Mathf.Approximately(MenuInput.y, releasedInput))
            MenuInput = new Vector2(MenuInput.x, 0f);
    }

    private void ResetInputHorizontal(float releasedInput)
    {
        if (Mathf.Approximately(MenuInput.x, releasedInput))
            MenuInput = new Vector2(0f, MenuInput.y);
    }
}
