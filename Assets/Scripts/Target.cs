using System;
using UnityEngine;
public class Target : MonoBehaviour
{
    private Material material;
    private Color defaultColor;

    private TargetState state;
    private TargetState oldState;

    private GameObject depthMarker;
    private Hand handDidNotClick;

    private float lastTimeInFocus;
    private float startTimeInFocus;

    private float angleBetweenRayObj;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        defaultColor = material.color;
        state = TargetState.Default;
        lastTimeInFocus = 0;
        startTimeInFocus = 0;
    }

    private void Start()
    {
        depthMarker = DepthRayManager.Instance.depthMarker;
    }

    private void Update()
    {
        UpdateTransparancy();
        UpdateMaterial();
        if (state == TargetState.InFocus
            || state == TargetState.Drag
            || state == TargetState.InFocusTransparent)
        {
            lastTimeInFocus = Time.time;
        }
    }

    private void LateUpdate()
    {
        if (state == TargetState.Drag)
        {
            transform.position = depthMarker.transform.position;
            Quaternion quat;
            if (handDidNotClick != null)
            {
                handDidNotClick.TryGetRotation(out quat);
                transform.localRotation = quat;
            }
        }
    }

    public void UpdateTransparancy()
    {
        if (state == TargetState.Drag)
            return;

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
                state = TargetState.InFocusTransparent;
            }
            else
            {
                state = TargetState.Transparent;
            }
        }
        else
        {
            if (ClickManager.Instance.CurrentFocusedObject == gameObject)
            {
                state = TargetState.InFocus;
            }
            else
            {
                state = TargetState.Default;
            }
        }
    }

    internal void LogClick()
    {
        String log = gameObject.name+" was clicked at "+ Time.time;
        Logger.Instance.AppendString(log);
    }

    /// <summary>
    /// This method updates the material of the gameobject based on the targetstate.
    /// </summary>
    private void UpdateMaterial()
    {
        Color color;
        switch (state)
        {
            case TargetState.Default:
                material.color = defaultColor;
                break;
            case TargetState.Transparent:
                color = defaultColor;
                color.a = 0.2f + 0.8f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case TargetState.InFocus:
                color = TargetManager.Instance.targetInFocus.color;
                material.color = color;
                break;
            case TargetState.InFocusTransparent:
                color = TargetManager.Instance.targetInFocus.color;
                color.a = 0.4f + 0.6f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case TargetState.Disabled:
                break;
            case TargetState.Drag:
                color = new Color(1,0,0);
                color.a = 0.4f + 0.6f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
        }
    }

    public TargetState State
    {
        get
        {
            return state;
        }

        set
        {
            oldState = state;
            state = value;
        }
    }

    public TargetState OldState
    {
        get
        {
            return oldState;
        }
    }

    public GameObject DepthMarker
    {
        get
        {
            return depthMarker;
        }

        set
        {
            depthMarker = value;
        }
    }

    public Handeness HandnessDidNotClick
    {
        get
        {
            return handDidNotClick.handeness;
        }

        set
        {
            if(value == Handeness.Left)
                handDidNotClick = HandManager.Instance.LeftHand;
            else if (value == Handeness.Right)
                handDidNotClick = HandManager.Instance.RightHand;
        }
    }

    public float LastTimeInFocus
    {
        get
        {
            return lastTimeInFocus;
        }
    }

    public float StartTimeInFocus
    {
        get
        {
            return startTimeInFocus;
        }

        set
        {
            startTimeInFocus = value;
            lastTimeInFocus = value;
        }
    }
}
public enum TargetState {
    Default,
    InFocus,
    Disabled,
    Drag,
    Transparent,
    InFocusTransparent
}

