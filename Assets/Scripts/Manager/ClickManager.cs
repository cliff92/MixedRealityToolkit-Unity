using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The click manager handles all interaction like left and right click.
/// A events will be send to all listeners.
/// </summary>
public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance;

    //different events that can be used
    public delegate void LeftClickMethod(GameObject gameObject);
    public event LeftClickMethod LeftClick;

    public delegate void RightClickMethod(GameObject gameObject);
    public event RightClickMethod RightClick;

    public delegate void ResetMethod();
    public event ResetMethod Reset;

    private GameObject currentFocusedObject;
    private GameObject currentlyAttachedObj;

    private List<Target> targetsInFoucsSinceLastClickDown;
    private VelocityHandler velocityHandler;

    public GameObject rightClickIndicator;
    public GameObject depthMarkerVisual;
    private Vector3 scaleRCIndicatorDefault;
    private Vector3 differenceRCIandDM;

    private float timeTargetInFocusAndButtonDown;

    private bool isClick;

    public GameObject CurrentFocusedObject
    {
        get
        {
            return currentFocusedObject;
        }
    }

    private void Awake()
    {
        Instance = this;
        targetsInFoucsSinceLastClickDown = new List<Target>();
        velocityHandler = new VelocityHandler(VariablesManager.DelayClickTime*2);
    }

    private void Start()
    {
        currentFocusedObject = null;

        DepthRayManager.Instance.RayUpdateEvent += OnUpdatePointer;
        InputManager.Instance.AddGlobalListener(gameObject);

        scaleRCIndicatorDefault = rightClickIndicator.transform.localScale;
        differenceRCIandDM = depthMarkerVisual.transform.localScale - scaleRCIndicatorDefault;
    }

    private void Update()
    {
        velocityHandler.UpdateLists();
        CheckReset();
        CheckClick();
    }

    private void OnDestroy()
    {
        DepthRayManager.Instance.RayUpdateEvent -= OnUpdatePointer;
    }

    private void CheckReset()
    {
        if (Input.GetButtonUp("Reset") || MyoPoseManager.DoubleTapUp)
        {
            OnReset();
            DepthRayManager.Instance.MoveDepthMarkerToUser();
        }
    }

    private void CheckClick()
    {
        if (!CheckRightClick())
        {
            CheckLeftClick();
        }
    }

    /// <summary>
    /// This method evalutes if the button was released and which object was clicked.
    /// A delay is included to avoid false clicks due to movement while perfoming the click.
    /// </summary>
    private void CheckLeftClick()
    {
        if (Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight") || MyoPoseManager.ClickUp)
        {
            if(TargetManager.IsAnyObjectAttached())
            {
                TargetManager.DetachTargetFromDepthMarker();
                targetsInFoucsSinceLastClickDown = new List<Target>();
                return;
            }

            //If current object in focus is not null just make a left click on this object
            // else check if there is some older object in the list of targets since last click down
            // There we also check if the velocity was over a threshold and if so no click is made
            if (currentFocusedObject != null)
            {
                OnLeftClick(currentFocusedObject);
            }
            else if(targetsInFoucsSinceLastClickDown.Count==1 && targetsInFoucsSinceLastClickDown[0] != null)
            {
                Target target = targetsInFoucsSinceLastClickDown[0];
                if (target.LastTimeInFocus > Time.time - VariablesManager.DelayClickTime)
                {
                    bool click = false;
                    if (MyoPoseManager.ClickUp 
                        && !velocityHandler.VelocityWasOverThSinceTimeStempMyo(target.LastTimeInFocus))
                    {
                        click = true;
                    }
                    else if (Input.GetButtonUp("RelativeLeft")
                        && !velocityHandler.VelocityWasOverThSinceTimeStempLeftController(target.LastTimeInFocus))
                    {
                        click = true;
                    }
                    else if(!velocityHandler.VelocityWasOverThSinceTimeStempRightController(target.LastTimeInFocus))
                    {
                        click = true;
                    }
                    if(click)
                    {
                        OnLeftClick(target.gameObject);
                    }
                }
            }
            else if(targetsInFoucsSinceLastClickDown.Count > 1)
            {
                float timeStempWithMinVel = -1;
                if(MyoPoseManager.ClickUp)
                {
                    timeStempWithMinVel = velocityHandler.FindTimeStepWithMinVelMyo();
                }
                else if(Input.GetButtonUp("RelativeLeft"))
                {
                    timeStempWithMinVel = velocityHandler.FindTimeStepWithMinVelLeftController();
                }
                else
                {
                    timeStempWithMinVel = velocityHandler.FindTimeStepWithMinVelRightController();
                }

                Target targetClosestToTimeStemp = null;
                float timeDifference = float.MaxValue;
                foreach (Target target in targetsInFoucsSinceLastClickDown)
                {
                    if(Mathf.Abs(target.LastTimeInFocus-timeStempWithMinVel)<timeDifference && target.LastTimeInFocus > Time.time - VariablesManager.DelayClickTime)
                    {
                        timeDifference = Mathf.Abs(target.LastTimeInFocus - timeStempWithMinVel);
                        targetClosestToTimeStemp = target;
                    }
                }
                if(targetClosestToTimeStemp!=null)
                {
                    OnLeftClick(targetClosestToTimeStemp.gameObject);
                }
            }
            //Reset list
            targetsInFoucsSinceLastClickDown = new List<Target>();
        }
    }

    /// <summary>
    /// This method checks if the current focused object was more than a certain time in focus.
    /// If this is the case, a right click is triggered and true is returned.
    /// </summary>
    private bool CheckRightClick()
    {
        if (!SceneHandler.UseRightClick || currentFocusedObject == null || TargetManager.IsAnyObjectAttached())
            return false;

        if (Input.GetButton("RelativeLeft") || Input.GetButton("RelativeRight") || MyoPoseManager.Click)
        {
            //Change time between Myo and Controller
            float timeRightClick = VariablesManager.TimeRightClickController;
            if (InputSwitcher.InputMode == InputMode.HeadMyoHybrid)
            {
                timeRightClick = VariablesManager.TimeRightClickMyo;
            }

            isClick = true;
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
            if (timeTargetInFocusAndButtonDown >= 0)
            {
                timeTargetInFocusAndButtonDown += Time.deltaTime;
                rightClickIndicator.transform.localScale = scaleRCIndicatorDefault + Mathf.Min(1f,timeTargetInFocusAndButtonDown / timeRightClick) * differenceRCIandDM;
                if (timeTargetInFocusAndButtonDown > timeRightClick)
                {
                    OnRightClick(currentFocusedObject);
                    timeTargetInFocusAndButtonDown = -1;
                    return true;
                }
            }
        }
        else
        {
            isClick = false;
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
        }
        return false;
    }

    /// <summary>
    /// This is a listener that is called from the depth ray manager every time a ray is shot.
    /// It updates the state of the object and resets timer
    /// </summary>
    public void OnUpdatePointer(GameObject newFocusedObject)
    {
        if(currentFocusedObject == newFocusedObject)
        {
            return;
        }
        timeTargetInFocusAndButtonDown = -1f;

        if (newFocusedObject == null || TargetManager.IsAnyObjectAttached())
        {
            currentFocusedObject = null;
        }
        else
        {
            //check if update should be happen 
            // velocity under a threshold and click
            bool update = false;
            Vector3 angularVelocity = Vector3.zero;
            switch (CustomRay.Instance.DeviceType)
            {
                case RayInputDevice.Unknown:
                    break;
                case RayInputDevice.Myo:
                    if (HandManager.MyoHand.TryGetAngularVelocity(out angularVelocity)
                        && angularVelocity.magnitude < 0.5f && MyoPoseManager.Click)
                    {
                        update = true;
                    }
                    break;
                case RayInputDevice.ControllerLeft:
                    if (HandManager.LeftHand.TryGetAngularVelocity(out angularVelocity)
                        && angularVelocity.magnitude < 0.5f && Input.GetButton("RelativeLeft"))
                    {
                        update = true;
                    }
                    break;
                case RayInputDevice.ControllerRight:
                    if (HandManager.RightHand.TryGetAngularVelocity(out angularVelocity)
                        && angularVelocity.magnitude < 0.5f && Input.GetButton("RelativeRight"))
                    {
                        update = true;
                    }
                    break;
            }
            if (!update)
            {
                currentFocusedObject = null;
            }
            else
            {
                switch (newFocusedObject.tag)
                {
                    case "Target":
                        Target target = newFocusedObject.GetComponent<Target>();
                        target.StartTimeInFocus = Time.time;
                        targetsInFoucsSinceLastClickDown.Add(target);
                        timeTargetInFocusAndButtonDown = 0;
                        currentFocusedObject = newFocusedObject;
                        break;
                    case "Obstacle":
                        currentFocusedObject = newFocusedObject;
                        break;
                    case "UI":
                        currentFocusedObject = newFocusedObject;
                        break;
                    default:
                        currentFocusedObject = null;
                        break;
                }
            }
        }
        //CheckLeftClick();
    }


    private void OnLeftClick(GameObject target)
    {
        if (LeftClick != null)
        {
            LeftClick(target);
        }
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


    public static bool IsClick
    {
        get
        {
            return Instance.isClick;
        }
    }

    /*private void CheckHandTwist()
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
    }*/
}
