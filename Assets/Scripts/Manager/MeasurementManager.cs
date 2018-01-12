using UnityEngine;
using UnityEngine.SceneManagement;

public class MeasurementManager : MonoBehaviour
{
    public static MeasurementManager Instance;
    public GameObject UI;
    public GameObject StopTrainingButton;
    public TextMesh statusText;
    public float measurementDuration = 60;

    private float currentTime = 0;

    private bool measurementActive = false;
    private bool trainingActive = false;

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
        StopTrainingButton.SetActive(false);
        statusText.text = "Menu";
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
        TargetManager.DeactivateAllObstacles();
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

        TargetManager.Reset();
        statusText.text = "Training Active";
    }

    private void StopTraining()
    {
        trainingActive = false;
        UI.SetActive(true);
        StopTrainingButton.SetActive(false);

        Logger.AppendString("End of Trainings Run of User: " + Logger.Instance.userId);
        Logger.AddCurrentTime();

        TargetManager.DestroyCurrentTarget();
        TargetManager.DeactivateAllObstacles();
        statusText.text = "Menu";
    }
}
