using HoloToolkit.Unity.InputModule;
using UnityEngine;

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
    private GameObject oldFocusedObject;
    private GameObject currentlyAttachedObj;

    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    public float delayClickTime = 0.1f;

    public float timeRightClickController = 1.0f;
    public float timeRightClickMyo = 1.5f;

    public GameObject rightClickIndicator;
    public GameObject depthMarker;
    private Vector3 scaleRCIndicatorDefault;
    private Vector3 differenceRCIandDM;

    private float timeSinceOldTargetInFocus;
    private float timeTargetInFocusAndButtonDown;

    private void Awake()
    {
        Instance = this;
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
        if (Input.GetButtonUp("Reset") || MyoPoseManager.Instance.DoubleTapUp)
        {
            OnReset();
        }
    }

    /// <summary>
    /// This method evalutes if the button was released and which object was clicked.
    /// A delay is included to avoid false clicks due to movement while perfoming the click.
    /// </summary>
    private void CheckLeftClick()
    {
        if (Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight") || MyoPoseManager.Instance.ClickUp)
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

    /// <summary>
    /// This method checks if the current focused object was more than a certain time in focus.
    /// If this is the case, a right click is triggered.
    /// </summary>
    private void CheckRightClick()
    {
        float timeRightClick = timeRightClickController;
        if (MyoPoseManager.Instance.useMyo)
        {
            timeRightClick = timeRightClickMyo;
        }
        if (Input.GetButton("RelativeLeft") || Input.GetButton("RelativeRight") || MyoPoseManager.Instance.Click)
        {
            rightClickIndicator.transform.localScale = scaleRCIndicatorDefault;
            if (timeTargetInFocusAndButtonDown >= 0 && currentFocusedObject != null)
            {
                timeTargetInFocusAndButtonDown += Time.deltaTime;
                rightClickIndicator.transform.localScale = scaleRCIndicatorDefault + Mathf.Min(1f,timeTargetInFocusAndButtonDown / timeRightClick) * differenceRCIandDM;
                if (timeTargetInFocusAndButtonDown > timeRightClick)
                {
                    OnRightClick(currentFocusedObject);
                    if (Input.GetButton("RelativeLeft") || MyoPoseManager.Instance.Arm == Thalmic.Myo.Arm.Left)
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

    /// <summary>
    /// This is a listener that is called from the input manager, when the focus of the pointer changes.
    /// It updates the state of the object and resets timer
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="oldFocusedObject"></param>
    /// <param name="newFocusedObject"></param>
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
