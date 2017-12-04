﻿using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.EventSystems;
using HoloToolkit.Unity;
using System;

public class HeadRay : MonoBehaviour, IPointingSource
{

    private BaseRayStabilizer rayStabilizer;

    private bool ownAllInput = true;

    [Obsolete("Will be removed in a later version. Use Rays instead.")]
    public Ray Ray { get { return Rays[0]; } }

    public PointerResult Result { get; set; }

    public float? ExtentOverride { get; set; }

    public LayerMask[] PrioritizedLayerMasksOverride { get; set; }

    private bool focusLocked = false;

    private RayStep[] rays = new RayStep[1] { new RayStep(Vector3.zero, Vector3.forward) };

    public static HeadRay Instance;

    public float offsetDown = 0.05f;
    public float relativeFactor = 1.0f;

    public GameObject head;

    private Quaternion startRelativeQuat = Quaternion.identity;

    // Use this for initialization
    void Start()
    {
        RayStabilizer = gameObject.GetComponent<BaseRayStabilizer>();
        OwnAllInput = true;
        PrioritizedLayerMasksOverride = null;
    }

    private void Awake()
    {
        Instance = this;
    }
   

    private void Update()
    {
        CheckStartOfRelativeMovement();
    }


    /// <summary>
    /// This method checks if a relative movement was started by the user.
    /// If started, the relative quaternion start value is set and the gain function reseted.
    /// </summary>
    private void CheckStartOfRelativeMovement()
    {
        Vector3 angularVelocity;
        Quaternion rotation;

        if (MyoPoseManager.Instance.ClickUp ||
            ((Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight"))&& !MyoPoseManager.Instance.useMyo))
        {
            DepthRayManager.Instance.UpdateTransparencyRay(10);
        }

        Hand hand = null;
        bool clickDown = false;
        bool click = false;

        if (MyoPoseManager.Instance.ClickDown)
        {
            hand = HandManager.Instance.MyoHand;
            clickDown = true;
        }
        else if (Input.GetButtonDown("RelativeLeft") && !MyoPoseManager.Instance.useMyo)
        {
            hand = HandManager.Instance.LeftHand;
            clickDown = true;
        }
        else if(Input.GetButtonDown("RelativeRight") && !MyoPoseManager.Instance.useMyo)
        {
            hand = HandManager.Instance.RightHand;
            clickDown = true;
        }
        if(clickDown && hand != null)
        {
            if (hand.TryGetRotation(out rotation))
            {
                DepthRayManager.Instance.UpdateTransparencyRay(180);
                startRelativeQuat = rotation;
                if (hand.TryGetAngularVelocity(out angularVelocity))
                {
                    GainFunction.Instance.ResetFunction(angularVelocity);
                }
                else
                {
                    GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                }
            }
            else
            {
                startRelativeQuat = Quaternion.identity;
                GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                Debug.LogError("No start rotation data avaiable.");
                return;
            }
        }

        if (MyoPoseManager.Instance.Click)
        {
            hand = HandManager.Instance.MyoHand;
            click = true;
        }
        else if (Input.GetButton("RelativeLeft") && !MyoPoseManager.Instance.useMyo)
        {
            hand = HandManager.Instance.LeftHand;
            click = true;
        }
        else if (Input.GetButton("RelativeRight") && !MyoPoseManager.Instance.useMyo)
        {
            hand = HandManager.Instance.RightHand;
            click = true;
        }

        if (click && hand != null)
        {
            if (hand.TryGetAngularVelocity(out angularVelocity))
            {
                GainFunction.Instance.UpdateFunction(angularVelocity.magnitude);
            }
            else if (hand.TryGetRotation(out rotation))
            {
                GainFunction.Instance.UpdateFunction(rotation, Time.time);
            }
            else
            {
                Debug.LogError("No velocity and rotation data avaiable");
            }
        }
    }


    [Obsolete("Will be removed in a later version. Use OnPreRaycast / OnPostRaycast instead.")]
    public void UpdatePointer()
    {

    }

    /// <summary>
    /// This method sets and updates the rays from the head into the scene.
    /// The gaze direction is changed when the relative movement is active.
    /// Then the direction is steered by the controller or myo armband.
    /// </summary>
    public virtual void OnPreRaycast()
    {
        if (head == null)
        {
            rays[0] = default(RayStep);
        }
        else
        {
            Hand hand = null;
            bool click = false;
            if (MyoPoseManager.Instance.Click)
            {
                hand = HandManager.Instance.MyoHand;
                click = true;
            }
            else if (Input.GetButton("RelativeLeft") && !MyoPoseManager.Instance.useMyo)
            {
                hand = HandManager.Instance.LeftHand;
                click = true;
            }
            else if (Input.GetButton("RelativeRight") && !MyoPoseManager.Instance.useMyo)
            {
                hand = HandManager.Instance.RightHand;
                click = true;
            }
            if (click)
            {
                Quaternion quat;
                if (hand.TryGetRotation(out quat))
                {
                    SetRays(quat);
                }
            }
            else {
                SetRays(head.transform.forward);
            }
        }
    }
    private void SetRays(Quaternion roation)
    {
        roation = Quaternion.Inverse(startRelativeQuat) * roation;
        Vector3 gazeDirection = head.transform.rotation * roation * Vector3.forward * GainFunction.Instance.RelativeFactor;
        SetRays(gazeDirection);
    }

    /// <summary>
    /// This method creates five rays to make pointing easier than with just one.
    /// </summary>
    /// <param name="direction"></param>
    private void SetRays(Vector3 direction)
    {
        float spreadFactor = 0.02f;
        Vector3 origin = head.transform.position + offsetDown * Vector3.down;

        Ray ray = new Ray(origin, direction);
        Ray rayUp = new Ray(origin + head.transform.up * spreadFactor, direction);
        Ray rayDown = new Ray(origin - head.transform.up * spreadFactor, direction);
        Ray rayRight = new Ray(origin + head.transform.right * spreadFactor, direction);
        Ray rayLeft = new Ray(origin - head.transform.right * spreadFactor, direction);

        rays = new RayStep[5];
        rays[0].CopyRay(ray, FocusManager.Instance.GetPointingExtent(this));
        rays[1].CopyRay(rayUp, FocusManager.Instance.GetPointingExtent(this));
        rays[2].CopyRay(rayDown, FocusManager.Instance.GetPointingExtent(this));
        rays[3].CopyRay(rayRight, FocusManager.Instance.GetPointingExtent(this));
        rays[4].CopyRay(rayLeft, FocusManager.Instance.GetPointingExtent(this));

        if (RayStabilizer != null)
        {
            RayStabilizer.UpdateStability(rays[0].origin, rays[0].direction);
            rays[0].CopyRay(RayStabilizer.StableRay, FocusManager.Instance.GetPointingExtent(this));
        }
    }

    public virtual void OnPostRaycast()
    {
        // Nothing needed
    }

    public bool OwnsInput(BaseEventData eventData)
    {
        return (OwnAllInput || InputIsFromSource(eventData));
    }

    public bool InputIsFromSource(BaseEventData eventData)
    {
        var inputData = (eventData as IInputSourceInfoProvider);

        return (inputData != null);
            //&& ((inputData.InputSource == rightController) || (inputData.InputSource == leftController));
    }

    public BaseRayStabilizer RayStabilizer
    {
        get
        {
            return rayStabilizer;
        }
        set
        {
            rayStabilizer = value;
        }
    }

    public bool OwnAllInput
    {
        get
        {
            return ownAllInput;
        }
        set
        {
            ownAllInput = value;
        }
    }

    public RayStep[] Rays
    {
        get
        {
            return rays;
        }
    }

    public bool InteractionEnabled
    {
        get
        {
            return true;
        }
    }

    public bool FocusLocked
    {
        get
        {
            return focusLocked;
        }
        set
        {
            focusLocked = value;
        }
    }
}
