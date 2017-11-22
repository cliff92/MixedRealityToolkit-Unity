using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.EventSystems;
using UnityEditor;
using HoloToolkit.Unity;
using System;
using UnityEngine.XR.WSA.Input;

public class HeadRay : MonoBehaviour, IPointingSource
{

    private MotionControllerInfo leftControllerModel;
    private MotionControllerInfo rightControllerModel;

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
        MotionControllerVisualizer.Instance.OnControllerModelLoaded += OnControllerModelLoaded;
        MotionControllerVisualizer.Instance.OnControllerModelUnloaded += OnControllerModelUnloaded;
    }

    private void OnDestroy()
    {
        //MotionControllerVisualizer.Instance.OnControllerModelLoaded -= OnControllerModelLoaded;
        //MotionControllerVisualizer.Instance.OnControllerModelUnloaded -= OnControllerModelUnloaded;
    }

    private void OnControllerModelUnloaded(MotionControllerInfo obj)
    {
        if (obj.Handedness == InteractionSourceHandedness.Left)
        {
            leftControllerModel = null;
        }
        if (obj.Handedness == InteractionSourceHandedness.Right)
        {
            rightControllerModel = null;
        }
    }

    private void OnControllerModelLoaded(MotionControllerInfo obj)
    {
        if(obj.Handedness == InteractionSourceHandedness.Left)
        {
            leftControllerModel = obj;
        }
        if(obj.Handedness == InteractionSourceHandedness.Right)
        {
            rightControllerModel = obj;
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("RelativeLeft"))
        {
            if(leftControllerModel!=null)
            {
                startRelativeQuat = leftControllerModel.ControllerParent.transform.rotation;
                GainFunction.Instance.ResetFunction(startRelativeQuat);
            }
        }
        if (Input.GetButtonDown("RelativeRight"))
        {
            if (rightControllerModel != null)
            {
                startRelativeQuat = rightControllerModel.ControllerParent.transform.rotation;
                GainFunction.Instance.ResetFunction(startRelativeQuat);
            }
        }

        if (Input.GetButton("RelativeLeft") && leftControllerModel != null)
        {
            GainFunction.Instance.UpdateFunction(leftControllerModel.ControllerParent.transform.rotation);
        }
        if (Input.GetButton("RelativeRight") && rightControllerModel != null)
        {
            GainFunction.Instance.UpdateFunction(rightControllerModel.ControllerParent.transform.rotation);
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
            Vector3 origin = head.transform.position + offsetDown * Vector3.down;

            if (Input.GetButton("RelativeLeft") && leftControllerModel != null)
            {
                Quaternion quat;
                quat = leftControllerModel.ControllerParent.transform.rotation;
                quat *= Quaternion.Inverse(startRelativeQuat);
                Vector3 gazeDirection = quat * head.transform.forward * GainFunction.Instance.RelativeFactor;
                Ray ray = new Ray(origin, gazeDirection);
                rays[0].CopyRay(ray, FocusManager.Instance.GetPointingExtent(this));
            }
            else if (Input.GetButton("RelativeRight") && rightControllerModel != null)
            {
                Quaternion quat;
                quat = rightControllerModel.ControllerParent.transform.rotation;
                quat *= Quaternion.Inverse(startRelativeQuat);
                Vector3 gazeDirection = quat * head.transform.forward * GainFunction.Instance.RelativeFactor;
                Ray ray = new Ray(origin, gazeDirection);
                rays[0].CopyRay(ray, FocusManager.Instance.GetPointingExtent(this));
            }
            else {
                Ray ray = new Ray(origin, head.transform.forward);
                rays[0].CopyRay(ray, FocusManager.Instance.GetPointingExtent(this));
            }
            
            if (RayStabilizer != null)
            {
                RayStabilizer.UpdateStability(rays[0].origin, rays[0].direction);
                rays[0].CopyRay(RayStabilizer.StableRay, FocusManager.Instance.GetPointingExtent(this));
            }
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
            && ((inputData.InputSource == rightControllerModel) || (inputData.InputSource == leftControllerModel));
    }
}
