using UnityEngine;

public class HandSwitcher : MonoBehaviour, ICustomClickHandler
{
    public static HandSwitcher Instance;

    public TextMesh statusText;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateText();
    }

    public void OnClick()
    {
        SwitchHand();
    }

    private void SwitchHand()
    {
        switch (VariablesManager.Handeness)
        {
            case Handeness.Left:
                VariablesManager.Handeness = Handeness.Right;
                break;
            case Handeness.Right:
                VariablesManager.Handeness = Handeness.Left;
                break;
        }

    }

    private void UpdateText()
    {
        if (statusText != null)
            statusText.text = VariablesManager.Handeness.ToString();
    }
}

