using UnityEngine;

public class InputSwitcher : MonoBehaviour, ICustomClickHandler
{
    public static InputSwitcher Instance;

    public TextMesh statusText;

    private InputMode inputMode = InputMode.HeadHybrid;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetButtonUp("Switch") && !MeasurementManager.MeasurementActive)
        {
            SwitchInput();
        }
    }

    public void OnClick()
    {
        SwitchInput();
    }

    private void SwitchInput(){
        switch (inputMode)
        {
            case InputMode.HeadMyoHybrid:
                inputMode = InputMode.HeadHybrid;
                break;
            case InputMode.HeadHybrid:
                inputMode = InputMode.RayHeadOrigin;
                break;
            case InputMode.RayHeadOrigin:
                inputMode = InputMode.RayControllerOrigin;
                break;
            case InputMode.RayControllerOrigin:
                inputMode = InputMode.HeadMyoHybrid;
                break;
        }
        statusText.text = "Current Technique " + InputMode;
    }

    public static InputMode InputMode
    {
        get
        {
            return Instance.inputMode;
        }
    }
}

public enum InputMode
{
    HeadMyoHybrid,
    HeadHybrid,
    RayControllerOrigin,
    RayHeadOrigin
}
