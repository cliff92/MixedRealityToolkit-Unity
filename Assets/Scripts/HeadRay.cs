using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.EventSystems;
using HoloToolkit.Unity;
using System;

public class HeadRay : MonoBehaviour, IPointingSource
{
    bool startedRelativeRight;
    bool startedRelativeLeft;

    private BaseRayStabilizer rayStabilizer;

    public BaseRayStabilizer RayStabilizer {
        get {
            return rayStabilizer;
        }
        set
        {
            rayStabilizer = value;
        }
    }

    public bool OwnAllInput {
        get
        {
            return ownAllInput;
        }
        set
        {
            ownAllInput = value;
        }
    }
    private bool ownAllInput = true;

    [Obsolete("Will be removed in a later version. Use Rays instead.")]
    public Ray Ray { get { return Rays[0]; } }

    public RayStep[] Rays
    {
        get
        {
            return rays;
        }
    }

    public PointerResult Result { get; set; }

    public float? ExtentOverride { get; set; }

    public LayerMask[] PrioritizedLayerMasksOverride { get; set; }

    public bool InteractionEnabled
    {
        get
        {
            return true;
        }
    }

    public bool FocusLocked {
        get
        {
            return focusLocked;
        }
        set
        {
            focusLocked = value;
        }
    }

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
        startedRelativeRight = false;
        startedRelativeLeft = false;
    }
   

    private void Update()
    {
        Vector3 angularVelocity;
        Quaternion rotation;
        if (HandManager.Instance.IsLeftControllerTracked)
        {
            if (Input.GetButtonUp("RelativeLeft"))
            {
                startedRelativeLeft = false;
            }
            if (Input.GetButtonDown("RelativeLeft"))
            {
                if (HandManager.Instance.LeftHand.TryGetRotation(out rotation))
                {
                    startRelativeQuat = rotation;
                    if (HandManager.Instance.LeftHand.TryGetAngularVelocity(out angularVelocity))
                    {
                        GainFunction.Instance.ResetFunction(angularVelocity);
                    }
                    else
                    {
                        GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                    }
                    startedRelativeLeft = true;
                }
                else
                {
                    startRelativeQuat = Quaternion.identity;
                    GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                    Debug.LogError("No start rotation data avaiable.");
                    return;
                }
            }

            if (startedRelativeLeft && Input.GetButton("RelativeLeft"))
            {
                if (HandManager.Instance.LeftHand.TryGetAngularVelocity(out angularVelocity))
                {
                    GainFunction.Instance.UpdateFunction(angularVelocity.magnitude);
                }
                else if (HandManager.Instance.LeftHand.TryGetRotation(out rotation))
                {
                    GainFunction.Instance.UpdateFunction(rotation, Time.time);
                }
                else
                {
                    Debug.LogError("No velocity and rotation data avaiable");
                }
            }
        }
        if (HandManager.Instance.IsRightControllerTracked)
        {
            if (Input.GetButtonUp("RelativeRight"))
            {
                startedRelativeRight = false;
            }

            if (Input.GetButtonDown("RelativeRight"))
            {
                if (HandManager.Instance.RightHand.TryGetRotation(out rotation))
                {
                    startRelativeQuat = rotation;
                    if (HandManager.Instance.RightHand.TryGetAngularVelocity(out angularVelocity))
                    {
                        GainFunction.Instance.ResetFunction(angularVelocity);
                    }
                    else
                    {
                        GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                    }
                    startedRelativeRight = true;
                }
                else
                {
                    startRelativeQuat = Quaternion.identity;
                    GainFunction.Instance.ResetFunction(startRelativeQuat, Time.time);
                    Debug.LogError("No start rotation data avaiable.");
                    return;
                }
            }
            if (startedRelativeRight && Input.GetButton("RelativeRight"))
            {
                if (HandManager.Instance.RightHand.TryGetAngularVelocity(out angularVelocity))
                {
                    GainFunction.Instance.UpdateFunction(angularVelocity.magnitude);
                }
                else if (HandManager.Instance.RightHand.TryGetRotation(out rotation))
                {
                    GainFunction.Instance.UpdateFunction(rotation, Time.time);
                }
                else
                {
                    Debug.LogError("No velocity and rotation data avaiable");
                }
            }
        }
    }


    [Obsolete("Will be removed in a later version. Use OnPreRaycast / OnPostRaycast instead.")]
    public void UpdatePointer()
    {

    }

    public virtual void OnPreRaycast()
    {
        if (head == null)
        {
            rays[0] = default(RayStep);
        }
        else
        {
            if (Input.GetButton("RelativeLeft") && HandManager.Instance.IsLeftControllerTracked)
            {
                Quaternion quat;
                if (HandManager.Instance.LeftHand.TryGetRotation(out quat))
                {
                    quat = Quaternion.Inverse(startRelativeQuat) * quat;

                    Vector3 gazeDirection = head.transform.rotation * quat * Vector3.forward;

                    SetRays(gazeDirection);
                }
            }
            else if (Input.GetButton("RelativeRight") && HandManager.Instance.IsRightControllerTracked)
            {
                Quaternion quat;
                if (HandManager.Instance.RightHand.TryGetRotation(out quat))
                {
                    quat = Quaternion.Inverse(startRelativeQuat) * quat;

                    Vector3 gazeDirection = head.transform.rotation * quat * Vector3.forward;
                    SetRays(gazeDirection);
                }
            }
            else {
                SetRays(head.transform.forward);
            }
        }
    }

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
}
