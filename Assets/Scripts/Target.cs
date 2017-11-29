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
            else if (value == Handeness.Left)
                handDidNotClick = HandManager.Instance.RightHand;
        }
    }
}
public enum TargetState { Default, InFocus, Disabled, Drag}

