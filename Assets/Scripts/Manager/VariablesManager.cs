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
    private float measurementDuration = 60;
    [SerializeField]
    private float trainingsDuration = 60;

    [SerializeField]
    private int amountOfObstacles = 400;

    [SerializeField]
    private int amountOfSortingObjects = 10;

    [SerializeField]
    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    private float delayClickTime = 0.1f;
    [SerializeField]
    private float timeRightClickController = 1.0f;
    [SerializeField]
    private float timeRightClickMyo = 1.0f;

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

    public static int AmountOfObstacles
    {
        get
        {
            if (Instance != null)
                return Instance.amountOfObstacles;
            return -1;
        }
    }

    public static int AmountOfSortingObjects
    {
        get
        {
            if (Instance != null)
                return Instance.amountOfSortingObjects;
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

    private void Awake()
    {
        Instance = this;
    }

}

