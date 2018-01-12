using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;

    private static ScenarioType scenarioType = ScenarioType.Menu;

    private bool useDepthMarker = false;

    private GameObject depthmarker;

    public static ScenarioType ScenarioType
    {
        get
        {
            return scenarioType;
        }

        set
        {
            scenarioType = value;
        }
    }

    public static bool UseDepthMarker
    {
        get
        {
            return Instance.useDepthMarker;
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            UpdateScenarioType(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        useDepthMarker = false;
        depthmarker = GameObject.Find("DepthMarker");
        switch (scenarioType)
        {
            case ScenarioType.Menu:
                break;
            case ScenarioType.Performance:
                break;
            case ScenarioType.Occlusion:
                useDepthMarker = true;
                break;
            case ScenarioType.Sorting:
                useDepthMarker = true;
                break;
        }
        if(useDepthMarker)
        {
            depthmarker.SetActive(true);
        }
        else
        {
            depthmarker.SetActive(false);
        }
    }

    public static void UpdateScenarioType(int sceneIndex)
    {
        switch (sceneIndex)
        {
            case 0:
                ScenarioType = ScenarioType.Menu;
                break;
            case 1:
                ScenarioType = ScenarioType.Performance;
                break;
            case 2:
                ScenarioType = ScenarioType.Occlusion;
                break;
            case 3:
                ScenarioType = ScenarioType.Sorting;
                break;
        }
    }
}

public enum ScenarioType
{
    Menu, Performance, Occlusion, Sorting
}
