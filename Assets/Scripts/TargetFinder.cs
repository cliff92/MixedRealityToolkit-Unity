using UnityEngine;
public class TargetFinder : MonoBehaviour
{
    public Material targetFinderMaterial;

    private void Update()
    {
        UpdateTargetMarker();
    }

    private void UpdateTargetMarker()
    {
        GameObject target = TargetManager.CurrentTarget;
        if(target == null || !target.activeSelf)
        {
            DeactivateChildren();
        }
        else
        {
            Vector3 headPos = DepthRayManager.Instance.HeadPosition;
            Vector3 targetPos = target.transform.position;
            Vector3 rayDirection = CustomRay.RayDirection();
            Vector3 targetDirection = (targetPos - headPos).normalized;

            float angleBetween = Vector3.Angle(rayDirection, targetDirection);

            if (angleBetween>5)
            {
                Color color = targetFinderMaterial.color;
                color.a = Mathf.Pow(Mathf.Abs(angleBetween) / 180f, 0.25f);
                targetFinderMaterial.color = color;
                ActivateChildren();
                transform.forward = targetPos - transform.position;
            }
            else
            {
                DeactivateChildren();
            }
        }
    }

    private void DeactivateChildren()
    {
        foreach(Transform transform in transform)
        {
            transform.gameObject.SetActive(false);
        }
    }

    private void ActivateChildren()
    {
        foreach (Transform transform in transform)
        {
            transform.gameObject.SetActive(true);
        }
    }
}
