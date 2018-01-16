using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

/// <summary>
/// This class handles the left and right hand. Data can be either from the motions controller or the myo armband.
/// Can be used from all classes to get informations of the hand.
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private bool isMyoTracked;
    private bool isLeftControllerTracked;
    private bool isRightControllerTracked;

    private Hand myoHand;
    private Hand leftHand;
    private Hand rightHand;

    private Hand currentHand;

    private void Awake()
    {
        Instance = this;
        leftHand = new Hand(Handeness.Left, RayInputDevice.ControllerLeft);
        rightHand = new Hand(Handeness.Right, RayInputDevice.ControllerRight);
        myoHand = new Hand(Handeness.Unknown, RayInputDevice.Myo);
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        UpdateControllers();
    }

    private void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
    }

    private void Start()
    {
        UpdateCurrentHand();
    }

    private void Update()
    {
        UpdateMyo();
        UpdateControllers();
        UpdateCurrentHand();
    }

    private void UpdateCurrentHand()
    {
        if(VariablesManager.InputMode == InputMode.HeadMyoHybrid)
        {
            currentHand = myoHand;
        }
        else
        {
            if(VariablesManager.Handeness == Handeness.Left)
            {
                currentHand = leftHand;
            }
            else
            {
                currentHand = rightHand;
            }
        }
    }


    /// <summary>
    /// This method checks whether the left and right controller is connected 
    /// and delegates the update to the UpdateHand method.
    /// </summary>
    private void UpdateControllers()
    {
        isRightControllerTracked = false;
        isLeftControllerTracked = false;
        foreach (var sourceState in InteractionManager.GetCurrentReading())
        {
            if (sourceState.source.kind == InteractionSourceKind.Controller)
            {
                if (sourceState.source.handedness == InteractionSourceHandedness.Left)
                {
                    isLeftControllerTracked = true;
                    UpdateHandViaController(leftHand, sourceState);
                }
                if (sourceState.source.handedness == InteractionSourceHandedness.Right)
                {
                    isRightControllerTracked = true;
                    UpdateHandViaController(rightHand, sourceState);
                }
            }
        }
    }

    private void UpdateHandViaController(Hand hand, InteractionSourceState sourceState)
    {
        hand.source = sourceState.source;
        hand.isPosAvailable = sourceState.sourcePose.TryGetPosition(out hand.pos);
        hand.isRotAvailable = sourceState.sourcePose.TryGetRotation(out hand.rotation, InteractionSourceNode.Pointer);
        hand.isForwardAvailable = sourceState.sourcePose.TryGetForward(out hand.forward, InteractionSourceNode.Pointer);
        hand.isAngularVelAvailable = sourceState.sourcePose.TryGetAngularVelocity(out hand.angularVelocity);
    }

    /// <summary>
    /// This method checks the arm of the myo armband
    /// and delegates the update to the UpdateHandWithMyo method.
    /// </summary>
    private void UpdateMyo()
    {
        myoHand.Reset();
        switch (MyoPoseManager.Arm)
        {
            case Thalmic.Myo.Arm.Right:
                myoHand.handeness = Handeness.Right;
                break;
            case Thalmic.Myo.Arm.Left:
                myoHand.handeness = Handeness.Left;
                break;
            case Thalmic.Myo.Arm.Unknown:
                myoHand.handeness = Handeness.Unknown;
                break;
        }
        myoHand.rotation = MyoPoseManager.Rotation;
        myoHand.isRotAvailable = true;
        myoHand.angularVelocity = MyoPoseManager.AngularVelocity;
        myoHand.isAngularVelAvailable = true;
        myoHand.rollAroundZ = MyoPoseManager.RelativeRoll;
        myoHand.isRollAroundZ = true;
        isMyoTracked = (VariablesManager.InputMode == InputMode.HeadMyoHybrid);
    }

    private void InteractionManager_InteractionSourceLost(InteractionSourceLostEventArgs obj)
    {
        UpdateControllers();
    }

    private void InteractionManager_InteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
    {
        InteractionSourceState sourceState = obj.state;
        if (sourceState.source.kind == InteractionSourceKind.Controller)
        {
            if (sourceState.source.handedness == InteractionSourceHandedness.Left)
            {
                isLeftControllerTracked = true;
                UpdateHandViaController(leftHand, sourceState);
            }
            if (sourceState.source.handedness == InteractionSourceHandedness.Right)
            {
                isRightControllerTracked = true;
                UpdateHandViaController(rightHand, sourceState);
            }
        }
    }

    public static bool IsRayRelativeDown()
    {
        if (Instance.currentHand == null)
            return false;
        if(Instance.currentHand == Instance.leftHand)
        {
            return Input.GetButtonDown("RelativeLeft");
        }

        if (Instance.currentHand == Instance.rightHand)
        {
            return Input.GetButtonDown("RelativeRight");
        }

        if(Instance.currentHand == Instance.myoHand)
        {
            return MyoPoseManager.ClickDown;
        }

        return false;
    }

    public static bool IsRayRelativeUp()
    {
        if (Instance.currentHand == null)
            return false;
        if (Instance.currentHand == Instance.leftHand)
        {
            return Input.GetButtonUp("RelativeLeft");
        }

        if (Instance.currentHand == Instance.rightHand)
        {
            return Input.GetButtonUp("RelativeRight");
        }

        if (Instance.currentHand == Instance.myoHand)
        {
            return MyoPoseManager.ClickUp;
        }

        return false;
    }

    public static bool IsRayRelative()
    {
        if (Instance.currentHand == null)
            return false;
        if (Instance.currentHand == Instance.leftHand)
        {
            return Input.GetButton("RelativeLeft");
        }

        if (Instance.currentHand == Instance.rightHand)
        {
            return Input.GetButton("RelativeRight");
        }

        if (Instance.currentHand == Instance.myoHand)
        {
            return MyoPoseManager.Click;
        }

        return false;
    }

    public static bool IsLeftControllerTracked
    {
        get
        {
            return Instance.isLeftControllerTracked;
        }
    }

    public static bool IsRightControllerTracked
    {
        get
        {
            return Instance.isRightControllerTracked;
        }
    }

    public static Hand LeftHand
    {
        get
        {
            return Instance.leftHand;
        }
    }

    public static Hand RightHand
    {
        get
        {
            return Instance.rightHand;
        }
    }

    public static Hand MyoHand
    {
        get
        {
            return Instance.myoHand;
        }
    }

    public static bool IsMyoTracked
    {
        get
        {
            return Instance.isMyoTracked;
        }
    }

    public static Hand CurrentHand
    {
        get
        {
            return Instance.currentHand;
        }
    }
}

public class Hand
{
    internal InteractionSource source;
    internal Vector3 pos;
    internal Quaternion rotation;
    internal Vector3 angularVelocity;
    internal Vector3 forward;
    internal float rollAroundZ;
    internal bool isPosAvailable;
    internal bool isRotAvailable;
    internal bool isAngularVelAvailable;
    internal bool isRollAroundZ;
    internal bool isForwardAvailable;
    internal Handeness handeness;
    internal RayInputDevice device;

    public Hand(Handeness handeness, RayInputDevice device)
    {
        this.handeness = handeness;
    }

    public bool TryGetPos(out Vector3 position)
    {
        position = pos;
        return isPosAvailable;
    }
    public bool TryGetRotation(out Quaternion rotation)
    {
        rotation = this.rotation;
        return isRotAvailable;
    }
    public bool TryGetAngularVelocity(out Vector3 angularVelocity)
    {
        angularVelocity = this.angularVelocity;
        return isAngularVelAvailable;
    }

    public bool TryGetForward(out Vector3 forward)
    {
        forward = this.forward;
        return isForwardAvailable;
    }

    public bool TryGetRotationAroundZ(out float angle)
    {
        if (isRollAroundZ)
        {
            angle = rollAroundZ;
            return true;
        }
        Quaternion quat;
        if(TryGetRotation(out quat))
        {
            angle = MyoPoseManager.NormalizeAngle(quat.eulerAngles.z);
            return true;
        }
        angle = 0;
        return false;
    }

    public void Vibrate(float intensity, float durationInSeconds)
    {
        //InteractionSourceExtensions.StopHaptics(source);
        //InteractionSourceExtensions.StartHaptics(source, intensity, durationInSeconds);
    }

    public void Reset()
    {
        isPosAvailable = false;
        isRotAvailable = false;
        isAngularVelAvailable = false;
        isRollAroundZ = false;
        isForwardAvailable = false;
    }
}

