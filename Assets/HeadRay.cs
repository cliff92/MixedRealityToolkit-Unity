using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.EventSystems;
using UnityEditor;
using HoloToolkit.Unity;
using System;

public class HeadRay : MonoBehaviour, IPointingSource
{
    public IInputSource InputSource { get; set; }

    public uint InputSourceId { get; set; }

    public BaseRayStabilizer RayStabilizer { get; set; }

    public bool OwnAllInput { get; set; }

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

    public bool FocusLocked { get; set; }

    private RayStep[] rays = new RayStep[1] { new RayStep(Vector3.zero, Vector3.forward) };

    public static HeadRay Instance;

    public float offsetDown = 0.05f;
    public float relativeFactor = 1.0f;

    public GameObject head;

    private Quaternion startRelativeQuat;

    // Use this for initialization
    void Start()
    {
        RayStabilizer = gameObject.GetComponent<BaseRayStabilizer>();
    }

    private void Awake()
    {
        Instance = this;
        InputManager.Instance.AddGlobalListener(gameObject);
    }

    private void Update()
    {
        if (Input.GetButtonDown("RelativeLeft"))
        {
            Quaternion quat;
            if(InputSource.TryGetGripRotation(InputSourceId, out quat))
            {
                startRelativeQuat = quat;
            }
        }
        if (Input.GetButtonDown("RelativeRight"))
        {
            Quaternion quat;
            if (InputSource.TryGetGripRotation(InputSourceId, out quat))
            {
                startRelativeQuat = quat;
            }
        }
    }


    [Obsolete("Will be removed in a later version. Use OnPreRaycast / OnPostRaycast instead.")]
    public void UpdatePointer()
    {
        if (head != null)
        {
            if (Input.GetButton("RelativeLeft"))
            {
                if (MotionControllerVisualizer.Instance.handLeft != null)
                {
                    GameObject handLeft = MotionControllerVisualizer.Instance.handLeft;
                    Quaternion tmp = handLeft.transform.rotation * Quaternion.Inverse(startRelativeQuat);
                    Vector3 gazeDirection = tmp * head.transform.forward * relativeFactor;
                    RayStabilizer.UpdateStability(head.transform.position + Vector3.down * offsetDown, gazeDirection);
                }
            }
            else if (Input.GetButton("RelativeRight"))
            {
                if (MotionControllerVisualizer.Instance.handRight != null)
                {
                    GameObject handRight = MotionControllerVisualizer.Instance.handRight;
                    Quaternion tmp = handRight.transform.rotation * Quaternion.Inverse(startRelativeQuat);
                    Vector3 gazeDirection = tmp * head.transform.forward * relativeFactor;
                    RayStabilizer.UpdateStability(head.transform.position + Vector3.down * offsetDown, gazeDirection);
                }
            }
            else
            {
                RayStabilizer.UpdateStability(head.transform.position + Vector3.down * offsetDown, head.transform.forward);
            }
        }
    }

    public virtual void OnPreRaycast()
    {
        if (InputSource == null)
        {
            rays[0] = default(RayStep);
        }
        else
        {
            Debug.Assert(InputSource.SupportsInputInfo(InputSourceId, SupportedInputInfo.Pointing), string.Format("{0} with id {1} does not support pointing!", InputSource, InputSourceId));

            Ray pointingRay;
            if (InputSource.TryGetPointingRay(InputSourceId, out pointingRay))
            {
                rays[0].CopyRay(pointingRay, FocusManager.Instance.GetPointingExtent(this));
            }
        }

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

        return (inputData != null)
            && (inputData.InputSource == InputSource)
            && (inputData.SourceId == InputSourceId);
    }
}
