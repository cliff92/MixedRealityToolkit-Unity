using UnityEngine;

public class CustomButton : MonoBehaviour
{
    private ICustomClickHandler customClickHandler;
    public Material focusMaterial;

    private Material defaultMaterial;

    private void Start()
    {
        customClickHandler = GetComponent<ICustomClickHandler>();
        ClickManager.Instance.LeftClick += LeftClick;
        DepthRayManager.Instance.FocusEntered += FocusEntered;
        defaultMaterial = GetComponent<Renderer>().material;
    }

    private void FocusEntered(GameObject focusedObject)
    {
        if (focusedObject == gameObject)
        {
            GetComponent<Renderer>().material = focusMaterial;
        }
        else
        {
            GetComponent<Renderer>().material = defaultMaterial;
        }
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
        DepthRayManager.Instance.FocusEntered -= FocusEntered;
    }

    public void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == gameObject)
        {
            AudioManager.PlayCorrectSound();
            customClickHandler.OnClick();
        }
    }
}
