using UnityEngine;
public class Obstacle : MonoBehaviour
{
    private Material material;
    private Color defaultColor;

    private ObstacleState state;
    private ObstacleState oldState;

    private float angleBetweenRayObj;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
        defaultColor = material.color;
        state = ObstacleState.Default;
    }

    private void Update()
    {
        UpdateTransparancy();
        UpdateMaterial();
    }

    public void UpdateTransparancy()
    {
        Vector3 headPos = DepthRayManager.Instance.HeadPosition;
        Vector3 rayDirection = DepthRayManager.Instance.RayDirection;
        float distanceMarkerHead = DepthRayManager.Instance.DistanceHeadDepthMarker;

        Vector3 dirHeadObj = transform.position - headPos;
        angleBetweenRayObj = Vector3.Angle(rayDirection, dirHeadObj);

        float distanceObjHead = Vector3.Distance(transform.position, headPos);

        if (angleBetweenRayObj < 30 && angleBetweenRayObj > -30
            && distanceMarkerHead > distanceObjHead - 0.05)
        {
            if (ClickManager.Instance.CurrentFocusedObject == gameObject)
            {
                state = ObstacleState.InFocusTransparent;
            }
            else
            {
                state = ObstacleState.Transparent;
            }
        }
        else
        {
            if (ClickManager.Instance.CurrentFocusedObject == gameObject)
            {
                state = ObstacleState.InFocus;
            }
            else
            {
                state = ObstacleState.Default;
            }
        }
    }

    private void UpdateMaterial()
    {
        Color color;
        switch (state)
        {
            case ObstacleState.Default:
                material.color = defaultColor;
                break;
            case ObstacleState.Transparent:
                color = defaultColor;
                color.a = 0.2f + 0.8f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case ObstacleState.InFocus:
                color = new Color(1,1,0,1);
                material.color = color;
                break;
            case ObstacleState.InFocusTransparent:
                color = new Color(1, 1, 0, 0.2f + 0.8f * Mathf.Abs(angleBetweenRayObj) / 30f);
                material.color = color;
                break;
            case ObstacleState.Disabled:
                break;
        }
    }
}
