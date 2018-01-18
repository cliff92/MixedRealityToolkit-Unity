using System;
using UnityEngine;
public class Target : MonoBehaviour
{
    private Material material;
    private Color defaultColor;

    private TargetState state;
    private TargetState oldState;

    private Hand handDidNotClick;

    private float lastTimeInFocus;
    private float startTimeInFocus;

    private float angleBetweenRayObj;

    internal float startTime;

    [SerializeField]
    private PrimitiveType primitiveType;

    private float startTimeInStorage = -1;
    private float startTimeAttached = -1;

    internal bool insideStorage = false;

    internal bool insideCameraViewWhenActivated = false;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        defaultColor = material.color;
        State = TargetState.Default;
        lastTimeInFocus = 0;
        startTimeInFocus = 0;
    }

    private void Update()
    {
        UpdateTransparancy();
        UpdateMaterial();
        if (state == TargetState.InFocus
            || state == TargetState.InFocusTransparent)
        {
            lastTimeInFocus = Time.time;
        }
        if (state == TargetState.Drag)
        {
            transform.position = DepthMarker.Position;
        }
    }

    public void Activate()
    {
        State = TargetState.Default;
        startTime = Time.time;
        insideStorage = false;
        Vector3 screenPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPosition.x > 0 && screenPosition.x < 1
            && screenPosition.y > 0 && screenPosition.y < 1
            && screenPosition.z > 0)
        {
            insideCameraViewWhenActivated = true;
        }
        else
        {
            insideCameraViewWhenActivated = false;
        }
    }

    public void Deactivate()
    {
        State = TargetState.Disabled;
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
                State = TargetState.InFocusTransparent;
            }
            else
            {
                State = TargetState.Transparent;
            }
        }
        else
        {
            if (ClickManager.Instance.CurrentFocusedObject == gameObject)
            {
                State = TargetState.InFocus;
            }
            else
            {
                State = TargetState.Default;
            }
        }
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
            case TargetState.Drag:
                color = new Color(1, 1, 0, 1);
                //color.a = 0.4f + 0.6f * Mathf.Abs(angleBetweenRayObj) / 30f;
                material.color = color;
                break;
            case TargetState.Disabled:
                break;
        }
        if(insideStorage)
        {
            color = new Color(1, 0, 1, 1);
            material.color = color;
        }
    }

    public void Store(PrimitiveType primitiveType)
    {
        MeasurementManager.OnStoreAction(this);
        if (primitiveType==this.primitiveType)
        {
            //correct stored
            // Time when stored
            // Correct or Incorrect stored
            // Information about the manipulation phase
            Logger.LogStoreTarget(this, true);
            TargetManager.DetachTargetFromDepthMarker(gameObject);
            State = TargetState.Disabled;
            gameObject.SetActive(false);

        }
        else
        {
            //incorrect stored
            Logger.LogStoreTarget(this, false);
            State = TargetState.Disabled;
            TargetManager.DetachTargetFromDepthMarker(gameObject);
            gameObject.SetActive(false);
        }
        gameObject.transform.position = Vector3.zero;
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

    public PrimitiveType PrimitiveType
    {
        get
        {
            return primitiveType;
        }

        set
        {
            primitiveType = value;
        }
    }

    public float StartTimeInStorage
    {
        get
        {
            return startTimeInStorage;
        }

        set
        {
            startTimeInStorage = value;
        }
    }

    public bool InsideStorage
    {
        set
        {
            insideStorage = value;
        }
    }

    public float StartTimeAttached
    {
        get
        {
            return startTimeAttached;
        }
        set
        {
            startTimeAttached = value;
        }
    }
}

