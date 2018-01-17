using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logger : MonoBehaviour
{
    public static Logger Instance;
    public string userId = "ck92";
    public string GeneralLog = "generalLog";
    public string ClickLog = "clickLog";
    public string SortingLog = "sortingLog";
    // This contains the name of the file. Don't add the ".txt"
    // Assign in inspector
    private StreamWriter writer; // This is the writer that writes to the file

    private static float amountRayMoved = 0;
    private static float amountDepthMarkerMovedInZRelative = 0;
    private static float amountDepthMarkerMovedInZAbsolute = 0;
    private static int numberOfMissedClicks = 0;
    private static int numberOfCorrectClicks = 0;
    private static int numberOfWrongClicks = 0;

    private static int targetsDetachedOutsideStorage = 0;
    private static int targetsDetachedInsideStorage = 0;

    private static float startTime = 0;

    private Vector3 oldDirectionRay = Vector3.zero;

    private static float numberOfPoseCorrections = 0;
    private static float numberOfClickCorrections = 0;


    private void Awake()
    {
        if(Instance== null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    private void Update()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            if (oldDirectionRay != Vector3.zero)
            {
                amountRayMoved += Vector3.Angle(CustomRay.Instance.Rays[0].Direction, oldDirectionRay);
            }
            oldDirectionRay = CustomRay.Instance.Rays[0].Direction;
        }
    }

    public static void AddStepsizeToDepthMarkerMovementInZ(float stepsize)
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            amountDepthMarkerMovedInZRelative += stepsize;
            amountDepthMarkerMovedInZAbsolute += Mathf.Abs(stepsize);
        }
    }

    public static void IncreaseClickCorrectCount()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            AppendToGeneralLog("Correct Click: " + (Time.time - startTime));
            numberOfCorrectClicks++;
        }
    }
    public static void IncreaseClickWrongCount()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            AppendToGeneralLog("Wrong Click: " + (Time.time - startTime));
            numberOfWrongClicks++;
        }
    }
    public static void IncreaseClickMissCount()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            AppendToGeneralLog("Miss Click: " + (Time.time - startTime));
            numberOfMissedClicks++;
        }
    }

    public static void IncreaseDetachCountInsideStorage()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            AppendToGeneralLog("Detached Inside Storage: " + (Time.time - startTime));
            targetsDetachedInsideStorage++;
        }
    }

    public static void IncreaseDetachCoutOutsideStorage()
    {
        if (MeasurementManager.MeasurementActive || MeasurementManager.TrainingActive)
        {
            AppendToGeneralLog("Detached Outside Storage:  " + (Time.time - startTime));
            targetsDetachedOutsideStorage++;
        }
    }

    public void ChangeUserId(InputField inputfield)
    {
        Instance.userId = inputfield.text;
    }

    private static void AppendToGeneralLog(string appendString)
    {
        Instance.writer = new StreamWriter(Application.persistentDataPath + "/" + Instance.GeneralLog + "_" + Instance.userId + ".txt", true);
        Instance.writer.WriteLine(appendString);
        Instance.writer.Close();
    }
    private static void AppendToClickLog(string appendString)
    {
        Instance.writer = new StreamWriter(Application.persistentDataPath + "/" + Instance.ClickLog + "_" + Instance.userId + ".txt", true);
        Instance.writer.WriteLine(appendString);
        Instance.writer.Close();
    }
    private static void AppendToSortingLog(string appendString)
    {
        Instance.writer = new StreamWriter(Application.persistentDataPath + "/" + Instance.SortingLog + "_" + Instance.userId + ".txt", true);
        Instance.writer.WriteLine(appendString);
        Instance.writer.Close();
    }

    private static void AppendToAllLogs(string appendString)
    {
        AppendToGeneralLog(appendString);

        AppendToClickLog(appendString);

        if (SceneHandler.ScenarioType == ScenarioType.Sorting)
            AppendToSortingLog(appendString);
    }

    public static void StartMeasurement()
    {
        string logStartMeasurement;
        logStartMeasurement = "New Run of User: " + Instance.userId;

        GeneralStart(logStartMeasurement);
    }

    public static void StartTraining()
    {
        string logStartTraining;
        logStartTraining = "New Trainings Run of User: " + Instance.userId;


        GeneralStart(logStartTraining);
    }

    private static void GeneralStart(string logStart)
    {
        Reset();
        logStart += "\n Current Time: " + Time.time;
        logStart += "\n Current Scenario: " + SceneManager.GetActiveScene().name;
        logStart += "\n Current Input method: " + VariablesManager.InputMode;
        logStart += "\n Handeness: " + VariablesManager.Handeness;
        logStart += "\n Delay while Click still counts: " + VariablesManager.DelayClickTime;
        logStart += "\n Time for Right Click: " + VariablesManager.TimeRightClickController;
        logStart += "\n Maximum Angle between two Targets: " + VariablesManager.MaximumAngleBetweenTwoTargets;
        logStart += "\n Minimum Angle between two Targets: " + VariablesManager.MinimumAngleBetweenTwoTargets;
        logStart += "\n Random Range x and y: " + VariablesManager.RandomRangeX + " : " + VariablesManager.RandomRangeY;
        if (SceneHandler.ScenarioType == ScenarioType.Occlusion || SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            logStart += "\n Number of Obstacles: " + ObstacleManager.NumberOfObstacles;
            if(SceneHandler.ScenarioType == ScenarioType.Sorting)
            {
                logStart += "\n Number of Targets To Sort: " + TargetManager.CurrentTargets.Length;
                logStart += "\n Time until Stored: " + VariablesManager.TimeUntilStored;
            }
        }

        AppendToAllLogs(logStart);
        string logTitle = "Name of the gameobject";
        logTitle += "; Time Target was clicked";
        logTitle += "; Click Time";
        logTitle += "; Target Position";
        logTitle += "; Bounding Rect Area";
        logTitle += "; Screen Position";
        logTitle += "; ViewPort Position";
        logTitle += "; Target was in View when activated";
        logTitle += "; Distance from head position";
        logTitle += "; Distance from last Target";
        logTitle += "; Distance from last Target Screen";
        logTitle += "; Angle between last and current Target";
        logTitle += "; Amount ray was moved";

        if (SceneHandler.ScenarioType == ScenarioType.Occlusion || SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            logTitle += "; Amount of obstacles in front of Target (small)";
            logTitle += "; Amount of obstacles in front of Target (big)";
            logTitle += "; Amount of obstacles in back of Target (small)";
            logTitle += "; Amount of obstacles in back of Target (big)";
            logTitle += "; Amount depth marker was moved in Z Absolute";
            logTitle += "; Amount depth marker was moved in Z Relative";
        }
        AppendToClickLog(logTitle);

        if (SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            logTitle = "Name of the gameobject";
            logTitle += "; Target Type";
            logTitle += "; Stored Correctly";
            logTitle += "; Time between attached and stored";
            AppendToSortingLog(logTitle);
        }
    }

    internal static void EndMeasurement()
    {
        string log = "End of Measurement Run";

        GeneralEnd();
        AppendToAllLogs(log);
    }

    internal static void EndTraining()
    {
        string log = "End of Trainings Run";
        
        GeneralEnd();
        AppendToAllLogs(log);
    }

    private static void GeneralEnd()
    {
        string log = "Number of Correct Clicks";
        log += "; Number of Wrong Clicks";
        log += "; Number of Miss Clicks";
        log += "; Number of Click Corrections";

        if(SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            log += "; Number of Detached Targets Inside Storage";
            log += "; Number of Detached Targets Outside Storage";
        }

        if(VariablesManager.InputMode == InputMode.HeadMyoHybrid)
        {
            log += "; Number of times pose correction was used";
        }

        AppendToGeneralLog(log);

        log = numberOfCorrectClicks.ToString()
        + "; " + numberOfWrongClicks 
        + "; " + numberOfMissedClicks
        + "; " + numberOfClickCorrections;
        if (SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            log += "; "+ targetsDetachedInsideStorage;
            log += "; " + targetsDetachedOutsideStorage;
        }

        if (VariablesManager.InputMode == InputMode.HeadMyoHybrid)
        {
            log += "; " + numberOfPoseCorrections;
        }

        AppendToGeneralLog(log);
    }

    internal static void LogClick(Target target, Vector3 posLastTarget, Vector3 directionLastTarget)
    {
        Rect boundingRect = Helper.GUIRectWithObject(target.gameObject);
        float timeSinceMeasurementStarted = Time.time - startTime;
        float timeSinceActive = Time.time - target.startTime;
        Vector3 targetPosition = target.transform.position;
        float boundingRectArea = boundingRect.size.x * boundingRect.size.y;
        Vector3 screenPositionTarget = Camera.main.WorldToScreenPoint(target.transform.position);
        Vector3 viewPortPositionTarget = Camera.main.WorldToViewportPoint(target.transform.position);
        float distanceFromHeadToTarget = Vector3.Distance(target.transform.position, DepthRayManager.Instance.HeadPosition);
        float distanceFromLastTarget = Vector3.Distance(target.transform.position, posLastTarget);
        float distanceFromLastTargetScreen = Vector2.Distance(Helper.WorldToGUIPoint(target.transform.position), Helper.WorldToGUIPoint(posLastTarget));
        float angleBetweenLastAndCurrent = Vector3.Angle(target.transform.position - DepthRayManager.Instance.HeadPosition, directionLastTarget);


        string log = target.gameObject.name;
        log += "; " + timeSinceMeasurementStarted;
        log += "; " + timeSinceActive;
        log += "; " + targetPosition;
        log += "; " + boundingRectArea;
        log += "; " + screenPositionTarget;
        log += "; " + viewPortPositionTarget;
        log += "; " + target.insideCameraViewWhenActivated;
        log += "; " + distanceFromHeadToTarget;
        log += "; " + distanceFromLastTarget;
        log += "; " + distanceFromLastTargetScreen;
        log += "; " + angleBetweenLastAndCurrent;
        log += "; " + amountRayMoved;

        if (SceneHandler.ScenarioType == ScenarioType.Occlusion
            || SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            int obstacleLayerMask = 1 << LayerMask.NameToLayer("ObstacleLayer");
            int innerNumberOfElementsInFront = Physics.OverlapCapsule(DepthRayManager.Instance.HeadPosition, target.transform.position, 0.05f, obstacleLayerMask).Length;
            int outerNumberOfElementsInFront = Physics.OverlapCapsule(DepthRayManager.Instance.HeadPosition, target.transform.position, 0.2f, obstacleLayerMask).Length;

            log += "; " + innerNumberOfElementsInFront;
            log += "; " + outerNumberOfElementsInFront;

            Vector3 point2 = target.transform.position + (target.transform.position - DepthRayManager.Instance.HeadPosition) * 100;
            int innerNumberOfElementsBehind = Physics.OverlapCapsule(point2, target.transform.position, 0.05f, obstacleLayerMask).Length;
            int outerNumberOfElementsBehind = Physics.OverlapCapsule(point2, target.transform.position, 0.2f, obstacleLayerMask).Length;
            log += "; " + innerNumberOfElementsBehind;
            log += "; " + outerNumberOfElementsBehind;
            //Debug.Log("Number of Elements In Front - Back Inner/Outer: " + innerNumberOfElementsInFront + "; "
            //    + outerNumberOfElementsInFront + "; " + innerNumberOfElementsBehind + "; " + outerNumberOfElementsBehind);
            log += "; " + amountDepthMarkerMovedInZAbsolute;
            log += "; " + amountDepthMarkerMovedInZRelative;
        }
        AppendToClickLog(log);
        ResetVariablesClick();
    }

    internal static void LogStoreTarget(Target target, bool storedCorrectly)
    {
        float timeBetweenAttachedAndStore = Time.time - target.StartTimeAttached;

        string log = target.gameObject.name;
        log += "; " + target.PrimitiveType;
        if (storedCorrectly)
            log += "; 1";
        else
            log += "; 0";
        log += "; " + timeBetweenAttachedAndStore;

        AppendToSortingLog(log);
    }

    public static void Reset()
    {
        startTime = Time.time;
        numberOfCorrectClicks = 0;
        numberOfMissedClicks = 0;
        numberOfWrongClicks = 0;
        targetsDetachedInsideStorage = 0;
        targetsDetachedOutsideStorage = 0;
        numberOfPoseCorrections = 0;
        numberOfClickCorrections = 0;
        ResetVariablesClick();
    }


    private static void ResetVariablesClick()
    {
        amountDepthMarkerMovedInZRelative = 0;
        amountDepthMarkerMovedInZAbsolute = 0;
        amountRayMoved = 0;
        Instance.oldDirectionRay = Vector3.zero;
    }

    public static void PoseCorrectionUsed()
    {
        Debug.Log("Pose Correction Used");

        string log = "Pose correction was used: " + Time.time;
        numberOfPoseCorrections++;

        AppendToGeneralLog(log);
    }

    public static void ClickCorrectionUsed(int type)
    {
        Debug.Log("Click Correction Used " + type);

        string log = "Click correction was used (Type "+type+"): " + Time.time;
        numberOfClickCorrections++;

        AppendToGeneralLog(log);
    }
}
