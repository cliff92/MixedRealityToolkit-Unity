﻿using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance;

    public delegate void LeftClickMethod(GameObject gameObject);
    public event LeftClickMethod LeftClick;

    public delegate void RightClickMethod(GameObject gameObject);
    public event RightClickMethod RightClick;

    public delegate void ResetMethod();
    public event ResetMethod Reset;

    private GameObject currentFocusedObject;
    private GameObject oldFocusedObject;

    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    public float delayClickTime = 0.1f;

    public float timeRightClick = 1.0f;

    public GameObject rightClickIndicator;
    public GameObject depthMarker;
    private Vector3 scaleRCIndicatorDefault;
    private Vector3 differenceRCIandDM;

    private float timeSinceOldTargetInFocus;
    private float timeTargetInFocusAndButtonDown;

    private TwistState twistState;
    private float timeTwistStarted;
    private GameObject currentlyAttachedObj;

    private void Awake()
    {
        Instance = this;
        twistState = TwistState.Idle;
    }

    private void Start()
    {
        timeSinceOldTargetInFocus = 0;
        currentFocusedObject = null;
        oldFocusedObject = null;

        DepthRayManager.Instance.PointerSpecificFocusChanged += OnPointerSpecificFocusChanged;
        InputManager.Instance.AddGlobalListener(gameObject);

        scaleRCIndicatorDefault = rightClickIndicator.transform.localScale;
        differenceRCIandDM = depthMarker.transform.localScale - scaleRCIndicatorDefault;
    }

    private void Update()
    {
        if (timeSinceOldTargetInFocus >= 0)
        {
            timeSinceOldTargetInFocus += Time.deltaTime;
        }
        CheckReset();
        CheckLeftClick();
        CheckRightClick();
    }

    private void OnDestroy()
    {
        DepthRayManager.Instance.PointerSpecificFocusChanged -= OnPointerSpecificFocusChanged;
    }

    private void CheckReset()
    {
        if (Input.GetButtonUp("Reset"))
        {
            OnReset();
        }
    }

    private void CheckLeftClick()
    {
        if (Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight"))
        {
            timeTargetInFocusAndButtonDown = 0;
            if (timeSinceOldTargetInFocus > 0 && timeSinceOldTargetInFocus < delayClickTime)
            {
                if (oldFocusedObject != null && oldFocusedObject.tag.Equals("Target"))
                {
                    OnLeftClick(oldFocusedObject);
                }
            }
            else
            {
                if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
                {
                    OnLeftClick(currentFocusedObject);
                }
            }
        }
    }

    private void CheckRightClick()
    {
        if (Input.GetButton("RelativeLeft") || Input.GetButton("RelativeRight"))
        {
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
            if (timeTargetInFocusAndButtonDown >= 0)
            {
                timeTargetInFocusAndButtonDown += Time.deltaTime;
                rightClickIndicator.transform.localScale = scaleRCIndicatorDefault + Mathf.Min(1f,timeTargetInFocusAndButtonDown / timeRightClick) * differenceRCIandDM;
                if (timeTargetInFocusAndButtonDown > timeRightClick)
                {
                    OnRightClick(currentFocusedObject);
                    if (Input.GetButton("RelativeLeft"))
                    {
                        TargetManager.Instance.AttachTargetToDepthMarker(currentFocusedObject, Handeness.Right);
                    }
                    else
                    {
                        TargetManager.Instance.AttachTargetToDepthMarker(currentFocusedObject, Handeness.Left);
                    }
                    timeTargetInFocusAndButtonDown = -1;
                }
            }
        }
        else
        {
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
        }
    }


    protected virtual void OnPointerSpecificFocusChanged(IPointingSource pointer, GameObject oldFocusedObject, GameObject newFocusedObject)
    {
        timeTargetInFocusAndButtonDown = -1f;

        this.oldFocusedObject = oldFocusedObject;
        currentFocusedObject = newFocusedObject;

        if (oldFocusedObject != null)
        {
            switch (oldFocusedObject.tag)
            {
                case "Target":
                    Target target = oldFocusedObject.GetComponent<Target>();
                    if (target.State != TargetState.Drag)
                        target.State = TargetState.Default;
                    timeSinceOldTargetInFocus = 0;
                    Vector3 headPos = pointer.Result.StartPoint;
                    Vector3 rayDirection = pointer.Result.End.Point - headPos;
                    TargetManager.Instance.UpdateTransparancy(headPos, rayDirection, oldFocusedObject, DepthRayManager.Instance.DistanceHeadDepthMarker);
                    break;
                case "Object":
                    timeSinceOldTargetInFocus = -1;
                    //oldFocusedObject.GetComponent<Renderer>().material = objectNotInFocus;
                    break;
            }
        }

        if (newFocusedObject != null)
        {
            switch (newFocusedObject.tag)
            {
                case "Target":
                    timeTargetInFocusAndButtonDown = 0;
                    Target target = newFocusedObject.GetComponent<Target>();
                    if (target.State != TargetState.Drag)
                        target.State = TargetState.InFocus;
                    Vector3 headPos = pointer.Result.StartPoint;
                    Vector3 rayDirection = pointer.Result.End.Point - headPos;
                    TargetManager.Instance.UpdateTransparancy(headPos, rayDirection, newFocusedObject, DepthRayManager.Instance.DistanceHeadDepthMarker);
                    Debug.Log("Target {0} in Foucs", target.gameObject);
                    break;
                case "Object":
                    //newFocusedObject.GetComponent<Renderer>().material = objectInFocus;
                    break;
            }
        }
    }

    private void OnLeftClick(GameObject target)
    {
        if (LeftClick != null)
            LeftClick(target);
    }

    private void OnRightClick(GameObject target)
    {
        if (RightClick != null)
            RightClick(target);
    }

    private void OnReset()
    {
        if (Reset != null)
            Reset();
    }

    private void CheckHandTwist()
    {
        float currentAngleHand;
        if (HandManager.Instance.RightHand.TryGetRotationAroundZ(out currentAngleHand))
        {
            switch (twistState)
            {
                case TwistState.Idle:
                    if (currentAngleHand > 35 && currentAngleHand < 180)
                    {
                        timeTwistStarted = Time.time;
                        twistState = TwistState.Started;
                    }
                    break;
                case TwistState.Started:
                    if (Time.time - timeTwistStarted < 0.5f)
                    {
                        if (currentAngleHand > 180 && currentAngleHand < 355)
                        {
                            twistState = TwistState.Idle;
                            TargetManager.Instance.AttachTargetToDepthMarker(currentFocusedObject, Handeness.Left);
                            TargetManager.Instance.DetachTargetFromDepthMarker(currentFocusedObject);
                        }
                    }
                    else
                    {
                        twistState = TwistState.Idle;
                    }
                    break;
            }
        }
    }

    private enum TwistState
    {
        Idle, Started
    }
}
