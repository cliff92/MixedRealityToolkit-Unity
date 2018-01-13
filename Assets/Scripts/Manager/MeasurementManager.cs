using UnityEngine;
using UnityEngine.SceneManagement;

public class MeasurementManager : MonoBehaviour
{
    public static MeasurementManager Instance;
    public GameObject UI;
    public GameObject StopTrainingButton;
    public TextMesh statusText;

    private float currentTime = 0;

    private bool measurementActive = false;
    private bool trainingActive = false;

    private Vector3 lastTargetPosition = Vector3.zero;
    private Vector3 lastTargetDirection = Vector3.zero;

    private int remainingSortingObjects = 10;

    public static bool MeasurementActive
    {
        get
        {
            return Instance.measurementActive;
        }
    }

    public static bool TrainingActive
    {
        get
        {
            return Instance.trainingActive;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TargetManager.Instance.TargetClicked += OnTargetClicked;
        StopTrainingButton.SetActive(false);
        statusText.text = "Menu";
    }

    private void OnDestroy()
    {
        TargetManager.Instance.TargetClicked -= OnTargetClicked;
    }

    private void Update()
    {
        if (measurementActive)
        {
            currentTime += Time.deltaTime;

            if (currentTime > VariablesManager.MeasurementDuration)
            {
                StopMeasurement();
            }
        }
    }

    public static void MeasurementClick()
    {
        if(Instance.measurementActive)
        {
            Instance.StopMeasurement();
        }
        else
        {
            Instance.StartMeasurement();
        }
    }

    public static void TrainingClick()
    {
        if (!Instance.measurementActive)
        {
            if (TrainingActive)
            {
                Instance.StopTraining();
            }
            else
            {
                Instance.StartTraining();
            }
        }
    }

    private void StartMeasurement()
    {
        trainingActive = false;
        measurementActive = true;
        UI.SetActive(false);

        string logStartMeasurement;
        logStartMeasurement = "New Run of User: " + Logger.Instance.userId;
        logStartMeasurement += "\n Current Time: " + Time.time;
        logStartMeasurement += "\n Current Scenario: " + SceneManager.GetActiveScene().name;
        logStartMeasurement += "\n Current Input method: " + InputSwitcher.InputMode;

        Logger.AppendString(logStartMeasurement);

        string logTitle = "Name of the gameobject";
        logTitle += "; Click Time";
        logTitle += "; Time since Instantiate";
        logTitle += "; Bounding Rect Area";
        logTitle += "; Screen Position";
        logTitle += "; Distance from last Target";
        logTitle += "; Distance from last Target Screen";
        logTitle += "; Angle between last and current Target";

        Logger.AppendString(logTitle);

        currentTime = 0;
        TargetManager.Reset();

        statusText.text = "Measurement Active";
    }

    private void StopMeasurement()
    {
        measurementActive = false;
        UI.SetActive(true);
        Logger.AppendString("End of Run of User: " + Logger.Instance.userId);
        Logger.AddCurrentTime();

        currentTime = 0;
        TargetManager.DestroyCurrentTarget();
        ObstacleManager.DeactivateAllObstacles();
        statusText.text = "Menu";

    }

    private void StartTraining()
    {
        trainingActive = true;
        UI.SetActive(false);
        StopTrainingButton.SetActive(true);

        string logStartTraining;
        logStartTraining = "New Trainings Run of User: " + Logger.Instance.userId;
        logStartTraining += "\n Current Time: " + Time.time;
        logStartTraining += "\n Current Scenario: " + SceneManager.GetActiveScene().name;
        logStartTraining += "\n Current Input method: " + InputSwitcher.InputMode;

        Logger.AppendString(logStartTraining);

        string logTitle = "Name of the gameobject";
        logTitle += "; Click Time";
        logTitle += "; Time since Instantiate";
        logTitle += "; Bounding Rect Area";
        logTitle += "; Screen Position";
        logTitle += "; Distance from last Target";
        logTitle += "; Distance from last Target Screen";
        logTitle += "; Angle between last and current Target";

        if(SceneHandler.ScenarioType == ScenarioType.Occlusion || SceneHandler.ScenarioType == ScenarioType.Sorting)
        {
            logTitle += "; Amount of objects in front of Target (small)";
            logTitle += "; Amount of objects in front of Target (big)";
            logTitle += "; Amount of objects in back of Target (small)";
            logTitle += "; Amount of objects in back of Target (big)";
        }

        Logger.AppendString(logTitle);

        statusText.text = "Training Active";

        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                TargetManager.SpawnSingleTarget(lastTargetDirection, PrimitiveType.Cube);
                break;
            case ScenarioType.Occlusion:
                TargetManager.SpawnSingleTarget(lastTargetDirection, PrimitiveType.Cube);
                ObstacleManager.MoveObjects();
                break;
            case ScenarioType.Sorting:
                TargetManager.SpawnTwoTypeTargets(VariablesManager.AmountOfSortingObjects, PrimitiveType.Cube, PrimitiveType.Capsule);
                break;
        }
    }

    private void StopTraining()
    {
        trainingActive = false;
        UI.SetActive(true);
        StopTrainingButton.SetActive(false);

        Logger.AppendString("End of Trainings Run of User: " + Logger.Instance.userId);
        Logger.AddCurrentTime();

        statusText.text = "Menu";

        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                TargetManager.DestroyCurrentTarget();
                break;
            case ScenarioType.Occlusion:
                ObstacleManager.DeactivateAllObstacles();
                TargetManager.DestroyCurrentTarget();
                break;
            case ScenarioType.Sorting:
                TargetManager.DestroyTargets();
                ObstacleManager.DeactivateAllObstacles();
                break;
        }
    }

    private void OnTargetClicked(Target target)
    {
        LogLeftClick(target);
        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                TargetManager.SpawnSingleTarget(lastTargetDirection,PrimitiveType.Cube);
                break;
            case ScenarioType.Occlusion:
                TargetManager.SpawnSingleTarget(lastTargetDirection, PrimitiveType.Cube);
                ObstacleManager.MoveObjects();
                break;
            case ScenarioType.Sorting:
                //ObstacleManager.MoveObjects();
                break;
        }
    }


    public static void LogLeftClick(Target target)
    {
        target.LogClick(Instance.lastTargetPosition, Instance.lastTargetDirection);
        Instance.lastTargetDirection = (target.transform.position - DepthRayManager.Instance.HeadPosition).normalized;
        Instance.lastTargetPosition = target.transform.position;
    }
}
