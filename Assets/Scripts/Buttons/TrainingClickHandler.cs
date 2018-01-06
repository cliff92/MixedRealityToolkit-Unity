using UnityEngine;
public class TrainingClickHandler : MonoBehaviour, ICustomClickHandler
{
    public void OnClick()
    {
        MeasurementManager.TrainingClick();
    }
}
