using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public static SceneHandler Instance;

    private static ScenarioType scenarioType = ScenarioType.Menu;

    private bool useDepthMarker = false;
    private bool useRightClick = false;
    private bool useLeftClick = false;

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

    public static bool UseRightClick
    {
        get
        {
            return Instance.useRightClick;
        }
    }

    public static bool UseLeftClick
    {
        get
        {
            return Instance.useLeftClick;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScenarioType(SceneManager.GetActiveScene().buildIndex);
        useDepthMarker = false;
        useRightClick = false;
        useLeftClick = false;
        depthmarker = GameObject.Find("DepthMarker");
        switch (scenarioType)
        {
            case ScenarioType.Menu:
                useLeftClick = true;
                break;
            case ScenarioType.Performance:
                useLeftClick = true;
                break;
            case ScenarioType.Occlusion:
                useDepthMarker = true;
                useLeftClick = true;
                break;
            case ScenarioType.Sorting:
                useDepthMarker = true;
                useRightClick = true;
                break;
        }
        if (useDepthMarker)
        {
            foreach (Transform child in depthmarker.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform child in depthmarker.transform)
            {
                child.gameObject.SetActive(false);
            }
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
