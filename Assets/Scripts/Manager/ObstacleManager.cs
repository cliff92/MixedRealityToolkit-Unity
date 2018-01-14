using UnityEngine;
public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance;
    
    public Material obstacleNotInFocusMat;
    public Material objectInFocusMat;

    private GameObject[] obstacleArray;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        obstacleArray = GameObject.FindGameObjectsWithTag("Obstacle");
        DeactivateAllObstacles();
        Debug.Log(obstacleArray.Length);
    }

    public static void MoveObjects()
    {
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        Vector3 newPos = Vector3.zero;

        DeactivateAllObstacles();
        int i = 0;
        foreach(GameObject obstacle in Instance.obstacleArray)
        { 
            do
            {
                float x = Random.Range(-VariablesManager.RandomRangeX, VariablesManager.RandomRangeX);
                float y = Random.Range(-VariablesManager.RandomRangeY, VariablesManager.RandomRangeY);
                //float z = Random.Range(-45, 45);
                float z = 0;
                float distance = Random.Range(5, 20);
                Vector3 newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
                newPos = headPos + newDirection;
                obstacle.transform.position = newPos;
                float size = Random.Range(0.5f, 2f);
                obstacle.transform.localScale = new Vector3(size, size, size);
                obstacle.SetActive(true);
            } while (!CheckPosition(newPos, i, obstacle.GetComponent<Collider>()));
            i++;
        }
    }

    private static bool CheckPosition(Vector3 newPos, int i, Collider collider)
    {
        GameObject[] targets = TargetManager.CurrentTargets;

        if(targets != null)
        {
            foreach(GameObject target in targets)
            {
                if (target != null && Vector3.Distance(target.transform.position, newPos) < 0.05f)
                {
                    return false;
                }
                if (target != null && target.GetComponent<Collider>().bounds.Intersects(collider.bounds))
                {
                    return false;
                }
            }
        }

        for (int j = 0; j < i; j++)
        {
            GameObject obj = Instance.obstacleArray[j];
            if (obj.GetComponent<Collider>().bounds.Intersects(collider.bounds))
            {
                return false;
            }
        }

        if (!VariablesManager.WorldCollider.bounds.Intersects(collider.bounds))
        {
            return false;
        }

        foreach(Collider col in VariablesManager.InvalidSpawingAreas)
        {
            if (col.bounds.Intersects(collider.bounds))
            {
                return false;
            }
        }

        return true;
    }

    public static void DeactivateAllObstacles()
    {
        foreach (GameObject obj in Instance.obstacleArray)
        {
            obj.SetActive(false);
        }
    }

    public static int AmountOfObstacles
    {
        get
        {
            if (Instance != null && Instance.obstacleArray != null)
                return Instance.obstacleArray.Length;
            return 0;
        }
    }
}
