using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.UI;

public class GainFunction : MonoBehaviour
{
    public static GainFunction Instance;
    [Tooltip("Number of samples that you want to iterate on.")]
    [Range(1, 100)]
    public int StoredSamples = 20;

    private AnimationCurve functionCurve = new AnimationCurve(new Keyframe[] 
    { new Keyframe(0,0.25f), new Keyframe(0.25f, 1.5f), new Keyframe(0.5f, 0.5f), new Keyframe(0.75f, 0.05f), new Keyframe(1, 0.25f) });

    /// <summary>
    /// Calculates standard deviation and averages for the gaze position.
    /// </summary>
    private readonly FloatRollingStatistics velocityRollingStats = new FloatRollingStatistics();
    private readonly FloatRollingStatistics timeRollingStats = new FloatRollingStatistics();

    private MovementState state;
    private float currentVelocity;
    private float oldVelocity;
    private float currentAcceleration;

    private Quaternion oldRotation;
    private Quaternion currentRotation;

    private float currentTime;
    private float oldTime;

    public Text text;

    private int counter;
    private int resetCounter;

    public float CurrentVelocity
    {
        get
        {
            return currentVelocity;
        }
    }

    public float CurrentAcceleration
    {
        get
        {
            return currentAcceleration;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        velocityRollingStats.Init(StoredSamples);
        timeRollingStats.Init(StoredSamples);
        Reset();
    }

    public void ResetFunction(Quaternion startRotationValue)
    {
        velocityRollingStats.Reset();
        timeRollingStats.Reset();
        timeRollingStats.AddSample(Time.time);
        currentRotation = oldRotation = startRotationValue;
        oldTime = Time.time;
        oldVelocity = 0;
        Reset();
        Debug.Log("Reset Function");
    }

    private void Reset()
    {
        state = MovementState.Idle;
        counter = 0;
        resetCounter = 0;
    }

    public void UpdateFunction(Quaternion rotationValue)
    {
        currentRotation = rotationValue;
        currentTime = Time.time;
        timeRollingStats.AddSample(Time.time);
        if (currentTime != oldTime)
        {
            currentVelocity = Mathf.Abs(Quaternion.Angle(currentRotation, oldRotation)) / (currentTime - oldTime);
            velocityRollingStats.AddSample(currentVelocity);
        }
        float std = velocityRollingStats.CurrentStandardDeviation;
        float avgVelocity = velocityRollingStats.Average;

        switch (state)
        {
            case MovementState.Idle:
                if((currentVelocity > avgVelocity && currentVelocity >50)|| avgVelocity > 90)
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
                    Debug.Log("1. Change state from idle to movbegin");
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovBegin:
                if (currentVelocity < avgVelocity || avgVelocity < 50)
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
                    Debug.Log("2. Change state from primary begin to primary after max");
                    counter = 0;
                }
                else if (counter < -3)
                {
                    state = MovementState.Idle;
                    Debug.Log("2.1 Change state back from primary begin to idle");
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMaxAfterMax:
                if (currentVelocity < 50)
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
                    Debug.Log("3. Change state from primary after max to primary end");
                    counter = 0;
                }
                else if(counter < -5)
                {
                    state = MovementState.PrimarySubMovBegin;
                    Debug.Log("3.1 Change state back from primary after max to primary begin");
                    counter = 0;
                }
                break;
            case MovementState.PrimarySubMovEnd:
                if (avgVelocity < 10)
                {
                    counter++;
                }
                else if(avgVelocity > 50)
                {
                    counter--;
                }
                if (counter > 30)
                {
                    state = MovementState.MovEnd;
                    Debug.Log("4. Change state from primary primary end to secondary end");
                    counter = 0;
                } 
                else if(counter < -10)
                {
                    state = MovementState.PrimarySubMaxAfterMax;
                    Debug.Log("4.1 Change state from primary primary end to primary after max");
                    counter = 0;
                }
                break;
            case MovementState.MovEnd:
                if (avgVelocity < 5)
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
                    Debug.Log("5. Change state from secondary end to idle");
                    counter = 0;
                }
                else if (counter < -10)
                {
                    state = MovementState.PrimarySubMovEnd;
                    Debug.Log("5.1 Change state from secondary end to primary after PrimarySubMovEnd");
                    counter = 0;
                }
                break;
        }
        oldVelocity = currentVelocity;
        oldTime = currentTime;
        oldRotation = currentRotation;
        text.text = "State: " +state.ToString();
        //Debug.Log("Current: " + CurrentVelocity + " vs avg: " + avgVelocity+" Std: "+std);
    }

    public float RelativeFactor
    {
        get
        {
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
}

public enum MovementState
{
    Idle, PrimarySubMovBegin, PrimarySubMaxAfterMax, PrimarySubMovEnd, MovEnd
}

