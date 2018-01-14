using UnityEngine;
public class VariablesManager : MonoBehaviour
{
    public static VariablesManager Instance;

    [SerializeField]
    private int randomRangeX = 45;
    [SerializeField]
    private int randomRangeY = 100;
    [SerializeField]
    private int maximumAngleBetweenTwoTargets = 90;
    [SerializeField]
    private int minimumAngleBetweenTwoTargets = 10;

    [SerializeField]
    private float measurementDuration = 60;
    [SerializeField]
    private float trainingsDuration = 60;

    [SerializeField]
    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    private float delayClickTime = 0.1f;
    [SerializeField]
    private float timeRightClickController = 1.0f;
    [SerializeField]
    private float timeRightClickMyo = 1.0f;

    [SerializeField]
    private InputMode inputMode = InputMode.HeadHybrid;

    [SerializeField]
    private float timeUntilStored = 2.0f;

    [SerializeField]
    private Collider worldCollider;

    [SerializeField]
    private Collider[] invalidSpawingAreas;

    public static int RandomRangeX
    {
        get
        {
            if(Instance!=null)
                return Instance.randomRangeX;
            return -1;
        }
    }

    public static int RandomRangeY
    {
        get
        {
            if (Instance != null)
                return Instance.randomRangeY;
            return -1;
        }
    }

    public static int MaximumAngleBetweenTwoTargets
    {
        get
        {
            if (Instance != null)
                return Instance.maximumAngleBetweenTwoTargets;
            return -1;
        }
    }

    public static int MinimumAngleBetweenTwoTargets
    {
        get
        {
            if (Instance != null)
                return Instance.minimumAngleBetweenTwoTargets;
            return -1;
        }
    }

    public static float MeasurementDuration
    {
        get
        {
            if (Instance != null)
                return Instance.measurementDuration;
            return -1;
        }
    }

    public static float TrainingsDuration
    {
        get
        {
            if (Instance != null)
                return Instance.trainingsDuration;
            return -1;
        }
    }

    public static float DelayClickTime
    {
        get
        {
            if (Instance != null)
                return Instance.delayClickTime;
            return -1;
        }
    }

    public static float TimeRightClickController
    {
        get
        {
            if (Instance != null)
                return Instance.timeRightClickController;
            return -1;
        }
    }

    public static float TimeRightClickMyo
    {
        get
        {
            if (Instance != null)
                return Instance.timeRightClickMyo;
            return -1;
        }
    }

    public static InputMode InputMode
    {
        get
        {
            if (Instance != null)
                return Instance.inputMode;
            return InputMode.HeadHybrid;
        }

        set
        {
            if(Instance != null)
            {
                Instance.inputMode = value;
            }
        }
    }

    public static float TimeUntilStored
    {
        get
        {
            if (Instance != null)
                return Instance.timeUntilStored;
            return -1;
        }
    }

    public static Collider WorldCollider
    {
        get
        {
            return Instance.worldCollider;
        }
    }

    public static Collider[] InvalidSpawingAreas
    {
        get
        {
            return Instance.invalidSpawingAreas;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

}

