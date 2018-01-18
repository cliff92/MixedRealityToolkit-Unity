using UnityEngine;

public class GainFunction : MonoBehaviour
{
    public static GainFunction Instance;
    [Tooltip("Number of samples that you want to iterate on.")]
    [Range(1, 100)]
    public int StoredSamples = 20;

    [SerializeField]
    private AnimationCurve functionCurveController = new AnimationCurve(new Keyframe[] 
    { new Keyframe(0,0.6f), new Keyframe(0.25f, 1.2f), new Keyframe(0.5f, 1f), new Keyframe(0.75f, 0.5f), new Keyframe(1, 0.6f) });

    [SerializeField]
    private AnimationCurve functionCurveMyo = new AnimationCurve(new Keyframe[]
    { new Keyframe(0,0.7f), new Keyframe(0.25f, 1.2f), new Keyframe(0.5f, 1f), new Keyframe(0.75f, 0.5f), new Keyframe(1, 0.7f) });

    /// <summary>
    /// Calculates standard deviation and averages for the gaze position.
    /// </summary>
    private readonly FloatRollingStatistics velocityRollingStats = new FloatRollingStatistics();

    private MovementState state;
    private float curVel;

    private Quaternion lastRotationData;
    private float lastTimeStep;

    private int counter;

    private float idleVelTH = 50;
    private float idleAvgVelTH = 85;
    private float primaryBeginAvgVelTH = 50;
    private float primaryAfterVelTH = 50;
    private float primaryEndVelTH = 12;
    private float primaryEndAvgVelTH = 50;
    private float moveEndAvgVelTH = 6;

    private int idleToPrimarySubMovBegin = 3;
    private int subMovBeginToSubMovAfterMax = 5;
    private int subMovAfterMaxToSubMoveEnd = 5;
    private int subMoveEndToMoveEnd = 5;
    private int moveEndToIdle = 5;

    private int subMovBeginToIdle = -3;
    private int subMovAfterMaxToSubMovBegin = -5;
    private int subMovEndToSubMovAfterMax = -2;
    private int moveEndToSubMoveEnd = -10;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        velocityRollingStats.Init(StoredSamples);
        Reset();
    }

    private void Reset()
    {
        state = MovementState.Idle;
        counter = 0;
    }

    private void SetControllerVariables()
    {
        idleVelTH = 50;
        idleAvgVelTH = 85;
        primaryBeginAvgVelTH = 50;
        primaryAfterVelTH = 50;
        primaryEndVelTH = 12;
        primaryEndAvgVelTH = 50;
        moveEndAvgVelTH = 6;

        idleToPrimarySubMovBegin = 3;
        subMovBeginToSubMovAfterMax = 5;
        subMovAfterMaxToSubMoveEnd = 5;
        subMoveEndToMoveEnd = 5;
        moveEndToIdle = 5;

        subMovBeginToIdle = -3;
        subMovAfterMaxToSubMovBegin = -5;
        subMovEndToSubMovAfterMax = -2;
        moveEndToSubMoveEnd = -10;
}

    private void SetMyoVariables()
    {
        idleVelTH = 30;
        idleAvgVelTH = 70;
        primaryBeginAvgVelTH = 30;
        primaryAfterVelTH = 30;
        primaryEndVelTH = 12;
        primaryEndAvgVelTH = 50;
        moveEndAvgVelTH = 6;

        idleToPrimarySubMovBegin = 3;
        subMovBeginToSubMovAfterMax = 5;
        subMovAfterMaxToSubMoveEnd = 5;
        subMoveEndToMoveEnd = 5;
        moveEndToIdle = 5;

        subMovBeginToIdle = -3;
        subMovAfterMaxToSubMovBegin = -5;
        subMovEndToSubMovAfterMax = -2;
        moveEndToSubMoveEnd = -10;
    }

    public void UpdateFunction(Quaternion currentRotation, float time)
    {
        float angleDelta = Quaternion.Angle(currentRotation, lastRotationData);

        float currentAngularVelocity = angleDelta / (time - lastTimeStep);

        UpdateFunction(currentAngularVelocity);

        lastRotationData = currentRotation;
        lastTimeStep = time;
    }

    //Angularvelocity in degree/s
    public void UpdateFunction(float currentAngularVelocity)
    {
        if (VariablesManager.InputMode == InputMode.HeadMyoHybrid)
        {
            SetMyoVariables();
        }
        else
        {
            SetControllerVariables();
        }
        curVel = currentAngularVelocity;
        velocityRollingStats.AddSample(curVel);

        float avgVel = velocityRollingStats.Average;

        switch (state)
        {
            case MovementState.Idle:
                if((curVel > avgVel && curVel > idleVelTH) 
                    || avgVel > idleAvgVelTH)
                {
                    counter++;
                }
                else if (counter > 0)
                {
                    counter--;
                }
                if (counter > idleToPrimarySubMovBegin)
                {
                    state = MovementState.PrimarySubMovBegin;
                    Debug.Log("Movement Started "+RelativeFactor);
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovBegin:
                if (curVel < avgVel || avgVel < primaryBeginAvgVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > subMovBeginToSubMovAfterMax)
                {
                    state = MovementState.PrimarySubMovAfterMax;
                    counter = 0;
                }
                else if (counter < subMovBeginToIdle)
                {
                    state = MovementState.Idle;
                    Debug.Log("Back to Idle" + RelativeFactor);
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovAfterMax:
                if (curVel < primaryAfterVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > subMovAfterMaxToSubMoveEnd)
                {
                    state = MovementState.PrimarySubMovEnd;
                    counter = 0;
                }
                else if(counter < subMovAfterMaxToSubMovBegin)
                {
                    state = MovementState.PrimarySubMovBegin;
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovEnd:
                if (avgVel < primaryEndVelTH)
                {
                    counter++;
                }
                else if(avgVel > primaryEndAvgVelTH)
                {
                    counter--;
                }
                if (counter > subMoveEndToMoveEnd)
                {
                    state = MovementState.MovEnd;
                    Debug.LogWarning("Move End");
                    counter = 0;
                } 
                else if(counter < subMovEndToSubMovAfterMax)
                {
                    state = MovementState.PrimarySubMovAfterMax;
                    counter = 0;
                }
                break;
            case MovementState.MovEnd:
                if (avgVel < moveEndAvgVelTH)
                {
                    counter++;
                }
                else
                {
                    counter--;
                }
                if (counter > moveEndToIdle)
                {
                    state = MovementState.Idle;
                    Debug.Log("Complete Cycle" + RelativeFactor);
                    counter = 0;
                }
                else if (counter < moveEndToSubMoveEnd)
                {
                    state = MovementState.PrimarySubMovEnd;
                    counter = 0;
                }
                break;
        }
    }

    public static void ResetFunction(Vector3 currentAngularVelocity)
    {
        Instance.velocityRollingStats.Reset();
        Instance.velocityRollingStats.AddSample(currentAngularVelocity.magnitude);
        Instance.Reset();
    }
    public static void ResetFunction(Quaternion currentRotation, float time)
    {
        Instance.lastRotationData = currentRotation;
        Instance.lastTimeStep = time;
        Instance.velocityRollingStats.Reset();
        Instance.Reset();
    }

    public static float RelativeFactor
    {
        get
        {
            AnimationCurve functionCurve; 
            if (VariablesManager.InputMode == InputMode.HeadMyoHybrid)
            {
                functionCurve = Instance.functionCurveMyo;
            }
            else
            {
                functionCurve = Instance.functionCurveController;
            }
            switch (Instance.state)
            {
                case MovementState.Idle:
                    return functionCurve.Evaluate(Instance.counter / Instance.idleToPrimarySubMovBegin * 0.25f);
                case MovementState.PrimarySubMovBegin:
                    return functionCurve.Evaluate(0.25f + Instance.counter / Instance.subMovBeginToSubMovAfterMax * 0.25f);
                case MovementState.PrimarySubMovAfterMax:
                    return functionCurve.Evaluate(0.5f + Instance.counter / Instance.subMovAfterMaxToSubMoveEnd * 0.25f);
                case MovementState.PrimarySubMovEnd:
                    return functionCurve.Evaluate(0.75f + Instance.counter / Instance.subMoveEndToMoveEnd * 0.25f);
                case MovementState.MovEnd:
                    return functionCurve.Evaluate(1);
                default:
                    return 0;
            }
        }
    }

    public static MovementState State
    {
        get
        {
            return Instance.state;
        }
    }

    public static float CurrentVelocity
    {
        get
        {
            return Instance.curVel;
        }
    }
}

