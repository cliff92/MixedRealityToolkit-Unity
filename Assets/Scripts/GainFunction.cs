using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.UI;

public class GainFunction : MonoBehaviour
{
    public static GainFunction Instance;
    [Tooltip("Number of samples that you want to iterate on.")]
    [Range(1, 100)]
    public int StoredSamples = 20;

    private AnimationCurve functionCurveController = new AnimationCurve(new Keyframe[] 
    { new Keyframe(0,0.1f), new Keyframe(0.25f, 1f), new Keyframe(0.5f, 0.5f), new Keyframe(0.75f, 0.01f), new Keyframe(1, 0.05f) });
    private AnimationCurve functionCurveMyo = new AnimationCurve(new Keyframe[]
    { new Keyframe(0,0.5f), new Keyframe(0.25f, 3f), new Keyframe(0.5f, 1f), new Keyframe(0.75f, 0.1f), new Keyframe(1, 0.25f) });

    /// <summary>
    /// Calculates standard deviation and averages for the gaze position.
    /// </summary>
    private readonly FloatRollingStatistics velocityRollingStats = new FloatRollingStatistics();

    private MovementState state;
    private float currentVelocity;

    private Quaternion lastRotationData;
    private float lastTimeStep;

    public Text text;

    private int counter;

    private float idleVelocityTH = 0.85f;
    private float idleAvgVelocityTH = 1.5f;
    private float primaryBeginAvgVelTH = 0.85f;
    private float primaryAfterVelTH = 0.85f;
    private float primaryEndVelTH = 0.2f;
    private float primaryEndAvgVelTH = 0.85f;
    private float moveEndAvgVelTH = 0.1f;

    public float CurrentVelocity
    {
        get
        {
            return currentVelocity;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        velocityRollingStats.Init(StoredSamples);
        Reset();
    }

    public void ResetFunction(Vector3 currentAngularVelocity)
    {
        velocityRollingStats.Reset();
        velocityRollingStats.AddSample(currentAngularVelocity.magnitude);
        Reset();
        Debug.Log("Reset Function Velocity");
    }
    public void ResetFunction(Quaternion currentRotation, float time)
    {
        lastRotationData = currentRotation;
        lastTimeStep = time;
        velocityRollingStats.Reset();
        Reset();
        Debug.Log("Reset Function Rotation");
    }

    private void Reset()
    {
        state = MovementState.Idle;
        counter = 0;
    }

    private void SetControllerVariables()
    {
        idleVelocityTH = 0.85f;
        idleAvgVelocityTH = 1.5f;
        primaryBeginAvgVelTH = 0.85f;
        primaryAfterVelTH = 0.85f;
        primaryEndVelTH = 0.2f;
        primaryEndAvgVelTH = 0.85f;
        moveEndAvgVelTH = 0.1f;
    }

    private void SetMyoVariables()
    {
        idleVelocityTH = 0.5f;
        idleAvgVelocityTH = 1.25f;
        primaryBeginAvgVelTH = 0.5f;
        primaryAfterVelTH = 0.5f;
        primaryEndVelTH = 0.2f;
        primaryEndAvgVelTH = 0.85f;
        moveEndAvgVelTH = 0.1f;
    }

    public void UpdateFunction(Quaternion currentRotation, float time)
    {
        float angleDelta = Mathf.Deg2Rad * Quaternion.Angle(currentRotation, lastRotationData);

        float currentAngularVelocity = angleDelta / (time - lastTimeStep);

        UpdateFunction(currentAngularVelocity);

        lastRotationData = currentRotation;
        lastTimeStep = time;
    }

    //Angularvelocity in rad/s
    public void UpdateFunction(float currentAngularVelocity)
    {
        if (MyoPoseManager.Instance.useMyo)
        {
            SetMyoVariables();
        }
        else
        {
            SetControllerVariables();
        }
        currentVelocity = currentAngularVelocity;
        velocityRollingStats.AddSample(currentVelocity);

        float avgVelocity = velocityRollingStats.Average;

        switch (state)
        {
            case MovementState.Idle:
                if((currentVelocity > avgVelocity && currentVelocity > idleVelocityTH) 
                    || avgVelocity > idleAvgVelocityTH)
                {
                    counter++;
                }
                else if (counter > 0)
                {
                    counter--;
                }
                if (counter > 3)
                {
                    state = MovementState.PrimarySubMovBegin;
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovBegin:
                if (currentVelocity < avgVelocity || avgVelocity < primaryBeginAvgVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > 3)
                {
                    state = MovementState.PrimarySubMaxAfterMax;
                    counter = 0;
                }
                else if (counter < -3)
                {
                    state = MovementState.Idle;
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMaxAfterMax:
                if (currentVelocity < primaryAfterVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > 5)
                {
                    state = MovementState.PrimarySubMovEnd;
                    counter = 0;
                }
                else if(counter < -5)
                {
                    state = MovementState.PrimarySubMovBegin;
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovEnd:
                if (avgVelocity < primaryEndVelTH)
                {
                    counter++;
                }
                else if(avgVelocity > primaryEndAvgVelTH)
                {
                    counter--;
                }
                if (counter > 5)
                {
                    Debug.Log("Move End");
                    state = MovementState.MovEnd;
                    counter = 0;
                } 
                else if(counter < -2)
                {
                    state = MovementState.PrimarySubMaxAfterMax;
                    counter = 0;
                }
                break;
            case MovementState.MovEnd:
                if (avgVelocity < moveEndAvgVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > 5)
                {
                    state = MovementState.Idle;
                    counter = 0;
                }
                else if (counter < -10)
                {
                    state = MovementState.PrimarySubMovEnd;
                    counter = 0;
                }
                break;
        }
        text.text = "State: " +state.ToString() +"\n Velocity: "+currentVelocity;
    }

    public float RelativeFactor
    {
        get
        {
            AnimationCurve functionCurve = functionCurveController;
            if (MyoPoseManager.Instance.useMyo)
            {
                functionCurve = functionCurveMyo;
            }
            switch (state)
            {
                case MovementState.Idle:
                    return functionCurve.Evaluate(counter/ 3 * 0.25f);
                case MovementState.PrimarySubMovBegin:
                    return functionCurve.Evaluate(0.25f + counter / 3 * 0.25f);
                case MovementState.PrimarySubMaxAfterMax:
                    return functionCurve.Evaluate(0.5f + counter / 30 * 0.25f);
                case MovementState.PrimarySubMovEnd:
                    return functionCurve.Evaluate(0.75f + counter / 10 * 0.25f);
                case MovementState.MovEnd:
                    return functionCurve.Evaluate(1);
                default:
                    return 0;
            }
        }
    }

    public MovementState State
    {
        get
        {
            return state;
        }
    }
}

public enum MovementState
{
    Idle, PrimarySubMovBegin, PrimarySubMaxAfterMax, PrimarySubMovEnd, MovEnd
}

