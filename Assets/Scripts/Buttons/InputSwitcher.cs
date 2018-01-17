using UnityEngine;

public class InputSwitcher : MonoBehaviour, ICustomClickHandler
{
    public static InputSwitcher Instance;

    public TextMesh statusText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if(SceneHandler.ScenarioType == ScenarioType.Menu)
        {
            if (Input.GetButtonUp("Switch") || MyoPoseManager.DoubleTapUp)
            {
                SwitchInput();
            }
        }

        UpdateText();
    }

    public void OnClick()
    {
        SwitchInput();
    }

    private void SwitchInput(){
        switch (VariablesManager.InputMode)
        {
            case InputMode.HeadMyoHybrid:
                VariablesManager.InputMode = InputMode.HeadHybrid;
                break;
            case InputMode.HeadHybrid:
                VariablesManager.InputMode = InputMode.RayControllerOrigin;
                break;
            case InputMode.RayControllerOrigin:
                VariablesManager.InputMode = InputMode.HeadMyoHybrid;
                break;
        }
        
    }

    private void UpdateText()
    {
        if(statusText!=null)
            statusText.text = VariablesManager.InputMode.ToString();
    }
}
