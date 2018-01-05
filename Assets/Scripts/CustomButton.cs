using UnityEngine;

public class CustomButton : MonoBehaviour
{
    private ICustomClickHandler customClickHandler;

    private void Start()
    {
        customClickHandler = GetComponent<ICustomClickHandler>();
        ClickManager.Instance.LeftClick += LeftClick;
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
    }

    public void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
        {
            Debug.Log("Null");
            return;
        }
        if (currentFocusedObject == gameObject)
        {
            customClickHandler.OnClick();
        }
    }
}
