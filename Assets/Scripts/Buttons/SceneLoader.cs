using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ICustomClickHandler
{
    public GameObject[] gameObjects;
    public int sceneNumber;

    public void OnClick()
    {
        LoadScene();
    }

    private void LoadScene()
    {
        for(int i=0;i< gameObjects.Length;i++)
        {
            Destroy(gameObjects[i]);
        }

        SceneHandler.UpdateScenarioType(sceneNumber);
        SceneManager.LoadScene(sceneNumber);
    }
}

