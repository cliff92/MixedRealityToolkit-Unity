using UnityEngine;
public class Target : MonoBehaviour
{
    private Material defaultMat;

    private TargetState state;
    private TargetState oldState;

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

    private void Awake()
    {
        defaultMat = gameObject.GetComponent<Renderer>().material;
        state = TargetState.Default;
    }
}
public enum TargetState { Default, InFocus, Transparent, Disabled }

