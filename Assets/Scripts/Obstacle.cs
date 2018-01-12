using UnityEngine;
public class Obstacle : MonoBehaviour
{
    private Material material;
    private Color defaultColor;

    private ObjectState state;
    private ObjectState oldState;

    private float angleBetweenRayObj;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
        defaultColor = material.color;
        state = ObjectState.Default;
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
                state = ObjectState.InFocusTransparent;
            }
            else
            {
                state = ObjectState.Transparent;
            }
        }
        else
        {
            if (ClickManager.Instance.CurrentFocusedObject == gameObject)
            {
                state = ObjectState.InFocus;
            }
            else
            {
                state = ObjectState.Default;
            }
        }
    }

    private void UpdateMaterial()
    {
        Color color;
        switch (state)
        {
            case ObjectState.Default:
                material.color = defaultColor;
                break;
            case ObjectState.Transparent:
                color = defaultColor;
                color.a = 0.2f + 0.8f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case ObjectState.InFocus:
                color = defaultColor;
                material.color = color;
                break;
            case ObjectState.InFocusTransparent:
                color = defaultColor;
                color.a = 0.2f + 0.8f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case ObjectState.Disabled:
                break;
        }
    }
}

public enum ObjectState
{
    Default,
    InFocus,
    Disabled,
    Transparent,
    InFocusTransparent
}
