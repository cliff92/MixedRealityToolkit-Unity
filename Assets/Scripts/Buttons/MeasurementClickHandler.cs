using UnityEngine;
public class MeasurementClickHandler : MonoBehaviour, ICustomClickHandler
{
    public void OnClick()
    {
        MeasurementManager.MeasurementClick();
    }
}
