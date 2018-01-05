using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour, ICustomClickHandler
{
    public GameObject[] gameObjects;

    public void OnClick()
    {
        LoadTestScene();
    }

    private void LoadTestScene()
    {
        for(int i=0;i< gameObjects.Length;i++)
        {
            Destroy(gameObjects[i]);
        }
        SceneManager.LoadScene(2);
    }
}

