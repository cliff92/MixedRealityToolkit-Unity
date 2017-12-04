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

    private void Awake()
    {
        Instance = this;
        leftHand = new Hand(Handeness.Left);
        rightHand = new Hand(Handeness.Right);
        myoHand = new Hand(Handeness.Unknown);
        InteractionManager.InteractionSourceDetected += InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost += InteractionManager_InteractionSourceLost;
        UpdateControllers();
    }

    private void OnDestroy()
    {
        InteractionManager.InteractionSourceDetected -= InteractionManager_InteractionSourceDetected;
        InteractionManager.InteractionSourceLost -= InteractionManager_InteractionSourceLost;
    }

    private void Update()
    {
        UpdateMyo();
        UpdateControllers();
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
        hand.isPosAvaiable = sourceState.sourcePose.TryGetPosition(out hand.pos);
        hand.isRotAvaiable = sourceState.sourcePose.TryGetRotation(out hand.rotation, InteractionSourceNode.Pointer);
        hand.isAngularVelAvaiable = sourceState.sourcePose.TryGetAngularVelocity(out hand.angularVelocity);
    }

    /// <summary>
    /// This method checks the arm of the myo armband
    /// and delegates the update to the UpdateHandWithMyo method.
    /// </summary>
    private void UpdateMyo()
    {
        myoHand.Reset();
        switch (MyoPoseManager.Instance.Arm)
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
        myoHand.rotation = MyoPoseManager.Instance.Rotation;
        myoHand.isRotAvaiable = true;
        myoHand.angularVelocity = MyoPoseManager.Instance.AngularVelocity;
        myoHand.isAngularVelAvaiable = true;
        myoHand.rollAroundZ = MyoPoseManager.Instance.RollFromZero();
        myoHand.isRollAroundZ = true;
        isMyoTracked = MyoPoseManager.Instance.useMyo;
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


    public bool IsLeftControllerTracked
    {
        get
        {
            return isLeftControllerTracked;
        }
    }

    public bool IsRightControllerTracked
    {
        get
        {
            return isRightControllerTracked;
        }
    }

    public Hand LeftHand
    {
        get
        {
            return leftHand;
        }
    }

    public Hand RightHand
    {
        get
        {
            return rightHand;
        }
    }

    public Hand MyoHand
    {
        get
        {
            return myoHand;
        }
    }

    public bool IsMyoTracked
    {
        get
        {
            return isMyoTracked;
        }
    }
}

public class Hand
{
    internal InteractionSource source;
    internal Vector3 pos;
    internal Quaternion rotation;
    internal Vector3 angularVelocity;
    internal float rollAroundZ;
    internal bool isPosAvaiable;
    internal bool isRotAvaiable;
    internal bool isAngularVelAvaiable;
    internal bool isRollAroundZ;
    internal Handeness handeness;

    public Hand(Handeness handeness)
    {
        this.handeness = handeness;
    }

    public bool TryGetPos(out Vector3 position)
    {
        position = pos;
        return isPosAvaiable;
    }
    public bool TryGetRotation(out Quaternion rotation)
    {
        rotation = this.rotation;
        return isRotAvaiable;
    }
    public bool TryGetAngularVelocity(out Vector3 angularVelocity)
    {
        angularVelocity = this.angularVelocity;
        return isAngularVelAvaiable;
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

    public void Virbrate(float intensity, float durationInSeconds)
    {
        InteractionSourceExtensions.StopHaptics(source);
        InteractionSourceExtensions.StartHaptics(source, intensity, durationInSeconds);
    }

    public void Reset()
    {
        isPosAvaiable = false;
        isRotAvaiable = false;
        isAngularVelAvaiable = false;
        isRollAroundZ = false;
    }
}

public enum Handeness
{
    Left, Right, Unknown
}

