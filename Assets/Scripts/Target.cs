using UnityEngine;
public class Target : MonoBehaviour
{
    private Material defaultMat;

    private TargetState state;
    private TargetState oldState;

    private Transform parent;

    private void Start()
    {
        parent = transform.parent;
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

    public Transform Parent
    {
        get
        {
            return parent;
        }
    }

    private void Awake()
    {
        defaultMat = gameObject.GetComponent<Renderer>().material;
        state = TargetState.Default;
    }
}
public enum TargetState { Default, InFocus, Disabled, Drag}

