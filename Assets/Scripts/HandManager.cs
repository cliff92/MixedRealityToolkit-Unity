using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    private bool isLeftControllerTracked;
    private bool isRightControllerTracked;

    private Hand leftHand;
    private Hand rightHand;

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

    private void Awake()
    {
        Instance = this;
        leftHand = new Hand();
        rightHand = new Hand();
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
        UpdateControllers();
    }

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
                    UpdateHand(leftHand, sourceState);
                }
                if (sourceState.source.handedness == InteractionSourceHandedness.Right)
                {
                    isRightControllerTracked = true;
                    UpdateHand(rightHand, sourceState);
                }
            }
        }
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
                UpdateHand(leftHand, sourceState);
            }
            if (sourceState.source.handedness == InteractionSourceHandedness.Right)
            {
                isRightControllerTracked = true;
                UpdateHand(rightHand, sourceState);
            }
        }
    }

    private void UpdateHand(Hand hand, InteractionSourceState sourceState)
    {
        sourceState.sourcePose.TryGetPosition(out hand.pos);
        sourceState.sourcePose.TryGetRotation(out hand.rotation,InteractionSourceNode.Pointer);
        sourceState.sourcePose.TryGetAngularVelocity(out hand.angularVelocity);
    }
}

public class Hand
{
    internal Vector3 pos;
    internal Quaternion rotation;
    internal Vector3 angularVelocity;

    public bool TryGetPos(out Vector3 position)
    {
        position = pos;
        if (pos == null)
        {
            return false;
        }
        return true;
    }
    public bool TryGetRotation(out Quaternion rotation)
    {
        rotation = this.rotation;
        if (pos == null)
        {
            return false;
        }
        return true;
    }
    public bool TryGetAngularVelocity(out Vector3 angularVelocity)
    {
        angularVelocity = this.angularVelocity;
        if (pos == null)
        {
            return false;
        }
        return true;
    }

    public bool TryGetRotationAroundZ(out float angle)
    {
        Quaternion quat;
        if(TryGetRotation(out quat))
        {
            angle = quat.eulerAngles.z;
            return true;
        }
        angle = 0;
        return false;
    }
}

