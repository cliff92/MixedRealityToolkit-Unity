using UnityEngine;
public class Target : MonoBehaviour
{
    private Material defaultMat;

    private TargetState state;
    private TargetState oldState;

    private GameObject depthMarker;
    private Hand handDidNotClick;

    private void Awake()
    {
        defaultMat = gameObject.GetComponent<Renderer>().material;
        state = TargetState.Default;
    }

    private void Start()
    {
        depthMarker = DepthRayManager.Instance.depthMarker;
    }

    private void Update()
    {
        UpdateMaterial();
    }

    /// <summary>
    /// This method updates the material of the gameobject based on the targetstate.
    /// </summary>
    private void UpdateMaterial()
    {
        switch (state)
        {
            case TargetState.Default:
                GetComponent<Renderer>().material = defaultMat;
                break;
            case TargetState.Transparent:
                GetComponent<Renderer>().material = TargetManager.Instance.transparentMat;
                break;
            case TargetState.InFocus:
                GetComponent<Renderer>().material = TargetManager.Instance.targetInFocus;
                break;
            case TargetState.InFocusTransparent:
                GetComponent<Renderer>().material = TargetManager.Instance.targetInFocusTransparent;
                break;
            case TargetState.Disabled:
                break;
            case TargetState.Drag:
                GetComponent<Renderer>().material = TargetManager.Instance.targetInFocus;
                break;
        }
    }

    private void LateUpdate()
    {
        if (state == TargetState.Drag)
        {
            transform.position = depthMarker.transform.position;
            Quaternion quat;
            if (handDidNotClick != null)
            {
                handDidNotClick.TryGetRotation(out quat);
                transform.localRotation = quat;
            }
        }
    }

    public Material DefaultMat
    {
        get
        {
            return defaultMat;
        }
    }

    public TargetState State
    {
        get
        {
            return state;
        }

        set
        {
            oldState = state;
            state = value;
        }
    }

    public TargetState OldState
    {
        get
        {
            return oldState;
        }
    }

    public GameObject DepthMarker
    {
        get
        {
            return depthMarker;
        }

        set
        {
            depthMarker = value;
        }
    }

    public Handeness HandnessDidNotClick
    {
        get
        {
            return handDidNotClick.handeness;
        }

        set
        {
            if(value == Handeness.Left)
                handDidNotClick = HandManager.Instance.LeftHand;
            else if (value == Handeness.Right)
                handDidNotClick = HandManager.Instance.RightHand;
        }
    }
}
public enum TargetState {
    Default,
    InFocus,
    Disabled,
    Drag,
    Transparent,
    InFocusTransparent
}

