using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public GameObject[] gameObjects;

    public void LoadTestScene()
    {
        for(int i=0;i< gameObjects.Length;i++)
        {
            Destroy(gameObjects[i]);
        }
        SceneManager.LoadScene(1);
    }
}

