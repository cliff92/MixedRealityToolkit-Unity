using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocus;
    public Material targetInFocusTransparent;
    public Material targetNotInFocus;
    public Material objectNotInFocus;
    public Material objectInFocus;
    public Material transparentMat;

    public AudioSource correctSound;

    private GameObject currentFocusedObject;
    private GameObject oldFocusedObject;

    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    public float delayClickTime;

    public float timeRightClick = 1.0f;

    private float timeSinceOldTargettInFocus;
    private float timeTargetInFocus;

    private TwistState twistState;
    private float timeTwistStarted;
    private GameObject currentlyAttachedObj;

    private GameObject[] targetArray;

    public GameObject[] TargetArray
    {
        get
        {
            return targetArray;
        }
    }

    private void Awake()
    {
        Instance = this;
        twistState = TwistState.Idle;
    }

    private void Start()
    {
        timeSinceOldTargettInFocus = 0;
        currentFocusedObject = null;
        oldFocusedObject = null;

        InputManager.Instance.AddGlobalListener(gameObject);
        DepthRayManager.Instance.PointerSpecificFocusChanged += OnPointerSpecificFocusChanged;

        targetArray = GameObject.FindGameObjectsWithTag("Target");
    }

    private void Update()
    {
        if (timeSinceOldTargettInFocus >= 0)
        {
            timeSinceOldTargettInFocus += Time.deltaTime;
        }

        if (Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight"))
        {
            if (timeSinceOldTargettInFocus > 0 && timeSinceOldTargettInFocus < delayClickTime)
            {
                Target target = oldFocusedObject.GetComponent<Target>();
                if (target.State != TargetState.Drag)
                {
                    target.State = TargetState.Disabled;
                    oldFocusedObject.SetActive(false);
                    Debug.Log("HitOld");
                    HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                    HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
                    correctSound.Play();
                }
            }
            else
            {
                if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
                {
                    Target target = currentFocusedObject.GetComponent<Target>();
                    if (target.State != TargetState.Drag)
                    {
                        target.State = TargetState.Disabled;
                        currentFocusedObject.SetActive(false);
                        Debug.Log("HitNew");
                        HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                        HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
                        correctSound.Play();
                    }
                }
            }
        }

        if(Input.GetButtonUp("Reset"))
        {
            foreach(GameObject obj in targetArray)
            {
                obj.SetActive(true);
            }
        }
        CheckRightClick();
        CheckHandTwist();
    }

    private void CheckRightClick()
    {
        if(timeTargetInFocus>0)
        {
            if(Time.time-timeTargetInFocus>timeRightClick)
            {
                AttachTargetToDepthMarker();
                timeTargetInFocus = -1;
            }
        }
    }

    private void CheckHandTwist()
    {
        float currentAngleHand;
        if(HandManager.Instance.RightHand.TryGetRotationAroundZ(out currentAngleHand))
        {
            switch (twistState)
            {
                case TwistState.Idle:
                    if(currentAngleHand>35 && currentAngleHand < 180)
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
                            AttachTargetToDepthMarker();
                            DetachTargetFromDepthMarker();
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

    private void AttachTargetToDepthMarker()
    {
        if (currentlyAttachedObj == currentFocusedObject)
            return;
        else if(currentlyAttachedObj!=null)
        {
            DetachTargetFromDepthMarker();
        }
        if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
        {
            Target target = currentFocusedObject.GetComponent<Target>();
            if (target.State!= TargetState.Drag)
            {
                target.State = TargetState.Drag;
                currentFocusedObject.transform.parent = DepthRayManager.Instance.depthMarker.transform;
                currentlyAttachedObj = currentFocusedObject;
                Debug.Log("Atached");
            }
            HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
            HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            correctSound.Play();
        }
    }
    private void DetachTargetFromDepthMarker()
    {
        if (currentlyAttachedObj != null)
        {
            Target target = currentlyAttachedObj.GetComponent<Target>();

            if (target.State == TargetState.Drag)
            {
                if(currentlyAttachedObj == currentFocusedObject)
                {
                    target.State = TargetState.InFocus;
                }
                else
                {
                    target.State = TargetState.Default;
                }
                currentlyAttachedObj.transform.parent = target.Parent;
                Debug.Log("Detached");
                HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
                correctSound.Play();
            }
            currentlyAttachedObj = null;
        }
    }

    private enum TwistState
    {
        Idle,Started
    }

    protected virtual void OnPointerSpecificFocusChanged(IPointingSource pointer, GameObject oldFocusedObject, GameObject newFocusedObject)
    {
        timeTargetInFocus = -1;
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
                    timeSinceOldTargettInFocus = 0;
                    Vector3 headPos = pointer.Result.StartPoint;
                    Vector3 rayDirection = pointer.Result.End.Point - headPos;
                    UpdateTransparancy(headPos, rayDirection, oldFocusedObject, DepthRayManager.Instance.DistanceHeadDepthMarker); 
                    break;
                case "Object":
                    timeSinceOldTargettInFocus = -1;
                    oldFocusedObject.GetComponent<Renderer>().material = objectNotInFocus;
                    break;
            }
        }

        if (newFocusedObject != null)
        {
            switch (newFocusedObject.tag)
            {
                case "Target":
                    Target target = newFocusedObject.GetComponent<Target>();
                    if(target.State != TargetState.Drag)
                        target.State = TargetState.InFocus;
                    Vector3 headPos = pointer.Result.StartPoint;
                    Vector3 rayDirection = pointer.Result.End.Point - headPos;
                    UpdateTransparancy(headPos, rayDirection, newFocusedObject, DepthRayManager.Instance.DistanceHeadDepthMarker);
                    Debug.Log("Target {0} in Foucs",target.gameObject);
                    timeTargetInFocus = Time.time;
                    break;
                case "Object":
                    newFocusedObject.GetComponent<Renderer>().material = objectInFocus;
                    break;
            }
        }
    }

    public void UpdateTransparency(Vector3 markerPos, Vector3 headPos, Vector3 rayDirection)
    {
        float distanceMarkerHead = Vector3.Distance(markerPos, headPos);
        
        foreach (GameObject obj in TargetArray)
        {
            UpdateTransparancy(headPos,rayDirection,obj,distanceMarkerHead);
        }
    }

    private void UpdateTransparancy(Vector3 headPos, Vector3 rayDirection, GameObject obj, float distanceMarkerHead)
    {
        Vector3 dirHeadMarker = obj.transform.position - headPos;
        float angle = Vector3.Angle(rayDirection, dirHeadMarker);
        Target target = obj.GetComponent<Target>();
        float distanceHeadObj = Vector3.Distance(obj.transform.position, headPos);

        if (angle < 10 && angle > -10 && distanceMarkerHead > distanceHeadObj)
        {
            if (target.State == TargetState.Default)
            {
                obj.GetComponent<Renderer>().material = transparentMat;
            }
            else if (target.State == TargetState.InFocus)
            {
                obj.GetComponent<Renderer>().material = targetInFocusTransparent;
            }
        }
        else
        {
            if (target.State == TargetState.Default)
            {
                obj.GetComponent<Renderer>().material = target.DefaultMat;
                target.State = TargetState.Default;
            }
            else if (target.State == TargetState.InFocus)
            {
                obj.GetComponent<Renderer>().material = targetInFocus;
                target.State = TargetState.InFocus;
            }
        }
    }
}

