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

    private int totalNumberOfObjectsToSort;
    private int numberOfObjectsSorted;

    private float measurementDuration = -1;

    private float targetsClicked = -1;

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

            if (currentTime > measurementDuration)
            {
                StopMeasurement();
            }
        }
        if(trainingActive)
        {
            currentTime += Time.deltaTime;
            if (currentTime > measurementDuration)
            {
                StopTraining();
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

        Logger.StartMeasurement();

        currentTime = 0;

        StartScenario();
    }

    private void StopMeasurement()
    {
        measurementActive = false;
        UI.SetActive(true);
        Logger.EndMeasurement();

        currentTime = 0;
        TargetManager.DeactivateTargets();
        ObstacleManager.DeactivateAllObstacles();

        StopScenario();
    }

    private void StartTraining()
    {
        trainingActive = true;
        UI.SetActive(false);
        StopTrainingButton.SetActive(true);

        Logger.StartTraining();

        statusText.text = "Training Active";

        StartScenario();
    }

    private void StopTraining()
    {
        trainingActive = false;
        UI.SetActive(true);
        StopTrainingButton.SetActive(false);

        Logger.EndTraining();

        statusText.text = "Menu";

        StopScenario();
    }

    private void StartScenario()
    {
        targetsClicked = 0;
        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                statusText.text = "Menu";
                break;
            case ScenarioType.Performance:
                TargetManager.ActivateSingleTarget(lastTargetDirection);
                if(trainingActive)
                {
                    measurementDuration = VariablesManager.TrainingsTimePerformance;
                }
                if(measurementActive)
                {
                    measurementDuration = VariablesManager.MeasurementTimePerformance;
                }
                statusText.text = "Measurement Active";
                break;
            case ScenarioType.Occlusion:
                TargetManager.ActivateSingleTarget(lastTargetDirection);
                ObstacleManager.MoveObjects();
                if (trainingActive)
                {
                    measurementDuration = VariablesManager.TrainingsTimeOcclusion;
                }
                if (measurementActive)
                {
                    measurementDuration = VariablesManager.MeasurementTimeOcclusion;
                }
                statusText.text = "Measurement Active";
                break;
            case ScenarioType.Sorting:
                TargetManager.MoveAllTargets();
                ObstacleManager.MoveObjects();
                numberOfObjectsSorted = 0;
                totalNumberOfObjectsToSort = TargetManager.CurrentTargets.Length;
                if (trainingActive)
                {
                    measurementDuration = VariablesManager.TrainingsTimeSorting;
                }
                if (measurementActive)
                {
                    measurementDuration = VariablesManager.MeasurementTimeSorting;
                }
                statusText.text = "Measurement Active"
                    + "\n" + numberOfObjectsSorted + " / " + totalNumberOfObjectsToSort;
                break;
        }
    }

    private void StopScenario()
    {
        targetsClicked = -1;
        statusText.text = "Menu";
        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                TargetManager.DeactivateTargets();
                break;
            case ScenarioType.Occlusion:
                ObstacleManager.DeactivateAllObstacles();
                TargetManager.DeactivateTargets();
                break;
            case ScenarioType.Sorting:
                TargetManager.DeactivateTargets();
                ObstacleManager.DeactivateAllObstacles();
                break;
        }
    }


    private void OnTargetClicked(Target target)
    {
        targetsClicked++;
        OnLeftClick(target);
        switch (SceneHandler.ScenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                Instance.statusText.text = "Measurement Active"
            + "\n Targets: " + targetsClicked;
                TargetManager.ActivateSingleTarget(lastTargetDirection);
                break;
            case ScenarioType.Occlusion:
                Instance.statusText.text = "Measurement Active"
            + "\n Targets: " + targetsClicked;
                TargetManager.ActivateSingleTarget(lastTargetDirection);
                ObstacleManager.MoveObjects();
                break;
            case ScenarioType.Sorting:
                break;
        }
    }

    public static void OnLeftClick(Target target)
    {
        Logger.LogClick(target, Instance.lastTargetPosition, Instance.lastTargetDirection);
        Instance.lastTargetDirection = (target.transform.position - DepthRayManager.Instance.HeadPosition).normalized;
        Instance.lastTargetPosition = target.transform.position;
    }

    public static void OnStoreAction(Target target)
    {
        Instance.numberOfObjectsSorted++;
        Instance.statusText.text = "Measurement Active"
            + "\n" + Instance.numberOfObjectsSorted+" / "+ Instance.totalNumberOfObjectsToSort;
        
        if (Instance.numberOfObjectsSorted >= Instance.totalNumberOfObjectsToSort)
        {
            TargetManager.DeactivateTargets();
            ObstacleManager.DeactivateAllObstacles();
            TargetManager.MoveAllTargets();
            ObstacleManager.MoveObjects();
            Instance.numberOfObjectsSorted = 0;
            Instance.totalNumberOfObjectsToSort = TargetManager.CurrentTargets.Length;
            //One run finished
        }
    }

    public static bool MeasurementActive
    {
        get
        {
            if (Instance == null)
                return false;
            return Instance.measurementActive;
        }
    }

    public static bool TrainingActive
    {
        get
        {
            if (Instance == null)
                return false;
            return Instance.trainingActive;
        }
    }
}
