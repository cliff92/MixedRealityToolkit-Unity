using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using System.Linq;
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
            DepthMarker.Instance.MoveDepthMarkerToUser();
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
        if (HandManager.IsRayRelativeUp())
        {
            if (TargetManager.IsAnyObjectAttached())
            {
                TargetManager.DetachTargetFromDepthMarker();
                targetsInFoucsSinceLastClickDown = new List<Target>();
                return;
            }
            targetsInFoucsSinceLastClickDown = targetsInFoucsSinceLastClickDown.Distinct().ToList();

            GameObject clickedObj = currentFocusedObject;

            // If current object in focus is null 
            // check if there is some older object in the list of targets since last click down
            // There we also check if the velocity was over a threshold and if so no click is made
            // Afterwards send event OnLeftClick
            if(currentFocusedObject == null && 
                targetsInFoucsSinceLastClickDown.Count==1 && targetsInFoucsSinceLastClickDown[0] != null)
            {
                Target target = targetsInFoucsSinceLastClickDown[0];
                if (target.LastTimeInFocus > Time.time - VariablesManager.DelayClickTime)
                {
                    if(!velocityHandler.VelocityWasOverThSinceTimeStemp(target.LastTimeInFocus))
                    {
                        clickedObj = target.gameObject;
                        Logger.ClickCorrectionUsed(1);
                    }
                }
            }
            else if(currentFocusedObject == null && targetsInFoucsSinceLastClickDown.Count > 1)
            {
                float timeStempWithMinVel = velocityHandler.FindTimeStepWithMinVel();

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
                    clickedObj = targetClosestToTimeStemp.gameObject;
                    Logger.ClickCorrectionUsed(2);
                }
            }

            OnLeftClick(clickedObj);
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

        if (HandManager.IsRayRelative())
        {
            //Change time between Myo and Controller
            float timeRightClick = VariablesManager.TimeRightClickController;
            if (VariablesManager.InputMode == InputMode.HeadMyoHybrid)
            {
                timeRightClick = VariablesManager.TimeRightClickMyo;
            }

            isClick = true;
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
            if (timeTargetInFocusAndButtonDown >= 0)
            {
                timeTargetInFocusAndButtonDown += Time.deltaTime;
                rightClickIndicator.transform.localScale = scaleRCIndicatorDefault + Mathf.Min(1f,timeTargetInFocusAndButtonDown / timeRightClick) * differenceRCIandDM;
                HandManager.CurrentHand.Vibrate(Thalmic.Myo.VibrationType.Short);
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
        if (currentFocusedObject == newFocusedObject)
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
            Vector3 angularVelocity = Vector3.zero;

            if(HandManager.IsRayRelative() && HandManager.CurrentHand.TryGetAngularVelocity(out angularVelocity)
                        && angularVelocity.magnitude < 0.5f)
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
            else
            {
                currentFocusedObject = null;
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
