﻿using HoloToolkit.Unity;
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
        hand.source = sourceState.source;
        hand.isPosAvaiable = sourceState.sourcePose.TryGetPosition(out hand.pos);
        hand.isRotAvaiable = sourceState.sourcePose.TryGetRotation(out hand.rotation,InteractionSourceNode.Pointer);
        hand.isAngularVelAvaiable = sourceState.sourcePose.TryGetAngularVelocity(out hand.angularVelocity);
    }
}

public class Hand
{
    internal InteractionSource source;
    internal Vector3 pos;
    internal Quaternion rotation;
    internal Vector3 angularVelocity;
    internal bool isPosAvaiable;
    internal bool isRotAvaiable;
    internal bool isAngularVelAvaiable;


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
        Quaternion quat;
        if(TryGetRotation(out quat))
        {
            angle = quat.eulerAngles.z;
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
}

