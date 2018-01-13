using UnityEngine;
public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance;

    private GameObject obstaclePrefab;
    
    public Material obstacleNotInFocusMat;
    public Material objectInFocusMat;

    private GameObject[] obstacleArray;

    private void Awake()
    {
        Instance = this;
        obstaclePrefab = Resources.Load("ObstaclePrefab", typeof(GameObject)) as GameObject;
    }

    private void Start()
    {
        InstantiateObstacles();
    }

    private void InstantiateObstacles()
    {
        obstacleArray = new GameObject[VariablesManager.AmountOfObstacles];
        for (int i = 0; i < VariablesManager.AmountOfObstacles; i++)
        {
            //GameObject newObject = Instantiate(Instance.obstaclePrefab, TargetManager.Instance.targets.transform);

            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //newObject.transform.parent = Instance.targets.transform;
            newObject.GetComponent<Renderer>().material = Instance.obstacleNotInFocusMat;
            newObject.AddComponent<Obstacle>();
            newObject.tag = "Obstacle";
            newObject.layer = LayerMask.NameToLayer("ObstacleLayer");

            newObject.SetActive(false);
            obstacleArray[i] = newObject;
        }
    }

    public static void MoveObjects()
    {
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        bool newPosFound = false;
        Vector3 newPos = Vector3.zero;

        for (int i = 0; i < Instance.obstacleArray.Length; i++)
        {
            GameObject obj = Instance.obstacleArray[i];
            newPosFound = false;
            while (!newPosFound)
            {
                float x = Random.Range(-VariablesManager.RandomRangeX, VariablesManager.RandomRangeX);
                float y = Random.Range(-VariablesManager.RandomRangeY, VariablesManager.RandomRangeY);
                //float z = Random.Range(-45, 45);
                float z = 0;
                float distance = Random.Range(5, 20);
                Vector3 newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
                newPos = headPos + newDirection;
                obj.transform.position = newPos;
                float size = Random.Range(0.5f, 2f);
                obj.transform.localScale = new Vector3(size, size, size);
                obj.SetActive(true);
                newPosFound = CheckPosition(newPos, i, obj.GetComponent<Collider>());
            }
        }
    }

    private static bool CheckPosition(Vector3 newPos, int i, Collider collider)
    {
        if (Vector3.Distance(TargetManager.CurrentTarget.transform.position, newPos) < 0.05f)
        {
            return false;
        }

        if (TargetManager.CurrentTarget.GetComponent<Collider>().bounds.Intersects(collider.bounds))
        {
            return false;
        }

        for (int j = 0; j < i; j++)
        {
            GameObject obj = Instance.obstacleArray[j];
            if (obj.GetComponent<Collider>().bounds.Intersects(collider.bounds))
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
}
