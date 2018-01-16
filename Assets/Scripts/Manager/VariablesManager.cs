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

    //Performance Parameters
    [SerializeField]
    private float trainingsTimePerformance = 300;
    [SerializeField]
    private float measurementTimePerformance = 60;

    //Occlusion Parameters
    [SerializeField]
    private float trainingsTimeOcclusion = 300;
    [SerializeField]
    private float measurementTimeOcclusion = 60;

    //Sorting Parameters
    [SerializeField]
    private float trainingsTimeSorting = 300;
    [SerializeField]
    private float measurementTimeSorting = 120;

    [SerializeField]
    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    private float delayClickTime = 0.1f;
    [SerializeField]
    private float timeRightClickController = 1.0f;
    [SerializeField]
    private float timeRightClickMyo = 1.0f;

    [SerializeField]
    private static InputMode inputMode = InputMode.HeadHybrid;

    [SerializeField]
    private float timeUntilStored = 1.0f;

    [SerializeField]
    private Collider worldCollider;

    [SerializeField]
    private Collider[] invalidSpawingAreas;

    [SerializeField]
    private static Handeness handeness = Handeness.Right;

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
            return inputMode;
        }

        set
        {
            inputMode = value;
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
            if (Instance != null)
                return Instance.worldCollider;
            return null;
        }
    }

    public static Collider[] InvalidSpawingAreas
    {
        get
        {
            if (Instance != null)
                return Instance.invalidSpawingAreas;
            return null;
        }
    }

    public static float TrainingsTimePerformance
    {
        get
        {
            return Instance.trainingsTimePerformance;
        }
    }

    public static float MeasurementTimePerformance
    {
        get
        {
            return Instance.measurementTimePerformance;
        }
    }

    public static float TrainingsTimeOcclusion
    {
        get
        {
            return Instance.trainingsTimeOcclusion;
        }
    }

    public static float MeasurementTimeOcclusion
    {
        get
        {
            return Instance.measurementTimeOcclusion;
        }
    }

    public static float TrainingsTimeSorting
    {
        get
        {
            return Instance.trainingsTimeSorting;
        }
    }

    public static float MeasurementTimeSorting
    {
        get
        {
            return Instance.measurementTimeSorting;
        }
    }

    public static Handeness Handeness
    {
        get
        {
            return handeness;
        }

        set
        {
            handeness = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

}

