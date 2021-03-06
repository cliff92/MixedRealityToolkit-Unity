﻿using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.EventSystems;
using HoloToolkit.Unity;
using System;

public class CustomRay : MonoBehaviour, IPointingSource
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

    public static CustomRay Instance;

    public float offsetDown = 0.05f;
    public float relativeFactor = 1.0f;

    public GameObject head;
    private GameObject[] partsVisualRay;
    public Material rayInactiveMaterial;
    public Material rayActiveMaterial;

    private Quaternion startRelativeQuat = Quaternion.identity;
    private Quaternion currentRelativeRotation = Quaternion.identity;
    private Quaternion lastRelativeRotation = Quaternion.identity;

    private Vector3 currentDirection = Vector3.forward;

    // Use this for initialization
    void Start()
    {
        RayStabilizer = gameObject.GetComponent<BaseRayStabilizer>();
        OwnAllInput = true;
        //PrioritizedLayerMasksOverride = null;
        partsVisualRay = GameObject.FindGameObjectsWithTag("PartVisualRay");
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
    public void CheckStartOfRelativeMovement()
    {
        Vector3 angularVelocity;
        Quaternion rotation;

        if (HandManager.IsRayRelativeUp())
        {
            foreach(GameObject part in partsVisualRay)
            {
                part.GetComponent<Renderer>().material = rayInactiveMaterial;
            }
        }

        if(HandManager.IsRayRelativeDown())
        {
            if (HandManager.CurrentHand.TryGetRotation(out rotation))
            {
                currentRelativeRotation = Quaternion.identity;
                lastRelativeRotation = rotation;
                startRelativeQuat = rotation;
                foreach (GameObject part in partsVisualRay)
                {
                    part.GetComponent<Renderer>().material = rayActiveMaterial;
                }
                if (HandManager.CurrentHand.TryGetAngularVelocity(out angularVelocity))
                {
                    GainFunction.ResetFunction(angularVelocity);
                }
                else
                {
                    GainFunction.ResetFunction(startRelativeQuat, Time.time);
                }
            }
            else
            {
                startRelativeQuat = Quaternion.identity;
                GainFunction.ResetFunction(startRelativeQuat, Time.time);
                Debug.LogError("No start rotation data avaiable.");
                return;
            }
        }

        if (HandManager.IsRayRelative() || HandManager.IsRayRelativeUp())
        {
            if (HandManager.CurrentHand.TryGetAngularVelocity(out angularVelocity))
            {
                GainFunction.Instance.UpdateFunction(Mathf.Rad2Deg*angularVelocity.magnitude);
            }
            else if (HandManager.CurrentHand.TryGetRotation(out rotation))
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
        Vector3 forward;
        Vector3 position;
        switch (VariablesManager.InputMode)
        {
            case InputMode.HeadMyoHybrid:
            case InputMode.HeadHybrid:
                if (head == null)
                {
                    rays[0] = default(RayStep);
                }
                else if (HandManager.IsRayRelative() || HandManager.IsRayRelativeUp())
                    {
                        Quaternion quat;
                        if (HandManager.CurrentHand.TryGetRotation(out quat))
                        {
                            SetRays(quat);
                        }
                    }
                else
                {
                    SetRaysWithHeadAsOrigin(head.transform.forward);
                }
                break;
            /*case InputMode.RayHeadOrigin:
                if (HandManager.RayHand.TryGetForward(out forward))
                {
                    SetRaysWithHeadAsOrigin(forward);
                }
                break;*/
            case InputMode.RayControllerOrigin:
                if(HandManager.CurrentHand.TryGetForward(out forward) && HandManager.CurrentHand.TryGetPos(out position))
                {
                    SetRays(forward, position);
                }
                break;
        }
        
    }
    private void SetRays(Quaternion rotation)
    {
        Quaternion deltaRotation = Quaternion.Inverse(lastRelativeRotation) * rotation;
        deltaRotation = Quaternion.SlerpUnclamped(Quaternion.identity, deltaRotation, GainFunction.RelativeFactor);
        currentRelativeRotation *= deltaRotation;

        lastRelativeRotation = rotation;
        
        Vector3 gazeDirection = head.transform.rotation * currentRelativeRotation * Vector3.forward;
        SetRaysWithHeadAsOrigin(gazeDirection);
    }

    private void SetRaysWithHeadAsOrigin(Vector3 direction)
    {
        Vector3 origin = head.transform.position + offsetDown * Vector3.down;
        SetRays(direction, origin);
    }

    /// <summary>
    /// This method creates five rays to make pointing easier than with just one.
    /// </summary>
    /// <param name="direction"></param>
    private void SetRays(Vector3 direction, Vector3 origin)
    {
        currentDirection = direction;
        float spreadFactor = 0.02f;
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
            RayStabilizer.UpdateStability(rays[0].Origin, rays[0].Direction);
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

    public static Vector3 RayDirection()
    {
        return Instance.currentDirection;
    }
}
