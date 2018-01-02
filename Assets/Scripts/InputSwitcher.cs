using UnityEngine;
public class InputSwitcher : MonoBehaviour
{
    public static InputSwitcher Instance;

    private InputMode inputMode = InputMode.HeadHybrid;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetButtonUp("Switch"))
        {
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
        }
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
