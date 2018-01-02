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
                case InputMode.Myo:
                    inputMode = InputMode.HeadHybrid;
                    break;
                case InputMode.HeadHybrid:
                    inputMode = InputMode.Ray;
                    break;
                case InputMode.Ray:
                    inputMode = InputMode.Myo;
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
    Myo,
    HeadHybrid,
    Ray
}
