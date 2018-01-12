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

    private float startTime;

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
        startTime = Time.time;
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
            && distanceMarkerHead > distanceObjHead - 0.05 && SceneHandler.UseDepthMarker)
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

    internal void LogClick(Vector3 posLastTarget , Vector3 directionLastTarget)
    {
        Rect boundingRect = GUIRectWithObject(gameObject);
        float timeClicked = Time.time;
        float timeSinceInstantiate = timeClicked - startTime;
        float boundingRectArea = boundingRect.size.x * boundingRect.size.y;
        Vector3 screenPosition = WorldToGUIPoint(transform.position);
        float distanceFromLastTarget = Vector3.Distance(transform.position, posLastTarget);
        float distanceFromLastTargetScreen = Vector2.Distance(WorldToGUIPoint(transform.position), WorldToGUIPoint(posLastTarget));
        float angleBetweenLastAndCurrent = Vector3.Angle(transform.position - DepthRayManager.Instance.HeadPosition, directionLastTarget);

        String log = gameObject.name;
        log += "; " + timeClicked;
        log += "; " + timeSinceInstantiate;
        log += "; " + boundingRectArea;
        log += "; " + screenPosition;
        log += "; " + distanceFromLastTarget;
        log += "; " + distanceFromLastTargetScreen;
        log += "; " + angleBetweenLastAndCurrent;

        if (SceneHandler.ScenarioType == ScenarioType.Occlusion
            || SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            int obstacleLayerMask = 1 << LayerMask.NameToLayer("ObstacleLayer");
            int innerNumberOfElementsInFront = Physics.OverlapCapsule(DepthRayManager.Instance.HeadPosition, transform.position, 0.05f, obstacleLayerMask).Length;
            int outerNumberOfElementsInFront = Physics.OverlapCapsule(DepthRayManager.Instance.HeadPosition, transform.position, 0.2f, obstacleLayerMask).Length;

            log += "; " + innerNumberOfElementsInFront;
            log += "; " + outerNumberOfElementsInFront;

            Vector3 point2 = transform.position + (transform.position - DepthRayManager.Instance.HeadPosition) * 100;
            int innerNumberOfElementsBehind = Physics.OverlapCapsule(point2, transform.position, 0.05f, obstacleLayerMask).Length;
            int outerNumberOfElementsBehind = Physics.OverlapCapsule(point2, transform.position, 0.2f, obstacleLayerMask).Length;
            log += "; " + innerNumberOfElementsBehind;
            log += "; " + outerNumberOfElementsBehind;
            //Debug.Log("Number of Elements In Front - Back Inner/Outer: " + innerNumberOfElementsInFront + "; "
            //    + outerNumberOfElementsInFront + "; " + innerNumberOfElementsBehind + "; " + outerNumberOfElementsBehind);
        }
        Logger.AppendString(log);
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
                color = TargetManager.Instance.targetInFocusMat.color;
                material.color = color;
                break;
            case TargetState.InFocusTransparent:
                color = TargetManager.Instance.targetInFocusMat.color;
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
                handDidNotClick = HandManager.LeftHand;
            else if (value == Handeness.Right)
                handDidNotClick = HandManager.RightHand;
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

    public static Rect GUIRectWithObject(GameObject go)
    {
        Vector3 cen = go.GetComponent<Renderer>().bounds.center;
        Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
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

