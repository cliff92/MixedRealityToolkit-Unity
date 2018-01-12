using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocusMat;
    public Material targetNotInFocusMat;
    public Material objectNotInFocusMat;
    public Material objectInFocusMat;
    

    public AudioSource correctSound;

    private GameObject currentTarget;
    private GameObject[] objectArray;

    private GameObject currentlyAttachedObj;

    public GameObject targets;
    public GameObject targetPrefab;
    public GameObject objectPrefab;
    public int AmountOfObjects = 100;
    private int targetId = 0;

    public int randomRangeX = 45;
    public int randomRangeY = 100;
    public int maximumAngleBetweenTwoTargets = 90;

    private Vector3 lastTargetPosition = Vector3.zero;
    private Vector3 lastTargetDirection = Vector3.zero;

    private static int TargetId
    {
        get
        {
            if (Instance == null)
                return 0;
            return Instance.targetId++;
        }
    }

    public static GameObject CurrentTarget
    {
        get
        {
            if (Instance == null)
                return null;
            return Instance.currentTarget;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClickManager.Instance.LeftClick += LeftClick;
        ClickManager.Instance.Reset += Reset;

        currentTarget = GameObject.FindGameObjectWithTag("Target");

        currentlyAttachedObj = null;

        InstantiateObjects();
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
        ClickManager.Instance.Reset -= Reset;
    }

    private void InstantiateObjects()
    {
        objectArray = new GameObject[AmountOfObjects];
        for (int i = 0; i < AmountOfObjects; i++)
        {
            GameObject newObject = Instantiate(Instance.objectPrefab, Instance.targets.transform);
            newObject.SetActive(false);
            objectArray[i] = newObject;
        }
    }

    public static void Reset()
    {
        SpawnTarget();
        MoveObjects();
    }

    public static void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
            return;
        Target target = currentFocusedObject.GetComponent<Target>();
        if (target == null)
            return;
        if (target.State != TargetState.Drag)
        {
            target.State = TargetState.Disabled;
            LogLeftClick(target);
            currentFocusedObject.SetActive(false);
            if (InputSwitcher.InputMode == InputMode.HeadMyoHybrid)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.RightHand.Virbrate(0.5f, 0.5f);
            }
            Instance.correctSound.Play();
            SpawnTarget();
            MoveObjects();
        }
        else
        {
            DetachTargetFromDepthMarker();
        }
    }

    public static void LogLeftClick(Target target)
    {
        target.LogClick(Instance.lastTargetPosition, Instance.lastTargetDirection);
        Instance.lastTargetDirection = (target.transform.position - DepthRayManager.Instance.HeadPosition).normalized;
        Instance.lastTargetPosition = target.transform.position;
    }

    public static void AttachTargetToDepthMarker(GameObject currentFocusedObject, Handeness handenessDidNotClicked)
    {
        if (Instance.currentlyAttachedObj == currentFocusedObject)
            return;
        else if(Instance.currentlyAttachedObj !=null)
        {
            DetachTargetFromDepthMarker();
        }
        if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
        {
            Target target = currentFocusedObject.GetComponent<Target>();
            if (target.State!= TargetState.Drag)
            {
                DepthRayManager.Instance.MoveDepthMarkerToFocus();
                target.State = TargetState.Drag;
                target.HandnessDidNotClick = handenessDidNotClicked;
                Instance.currentlyAttachedObj = currentFocusedObject;
            }
            if (InputSwitcher.InputMode == InputMode.HeadMyoHybrid)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.RightHand.Virbrate(0.5f, 0.5f);
            }
            Instance.correctSound.Play();
        }
    }
    public static void DetachTargetFromDepthMarker()
    {
        if (Instance != null && Instance.currentlyAttachedObj != null && Instance.currentlyAttachedObj.tag == "Target")
        {
            Target target = Instance.currentlyAttachedObj.GetComponent<Target>();
            target.State = TargetState.Default;
            target.UpdateTransparancy();
            if (InputSwitcher.InputMode == InputMode.HeadMyoHybrid)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.RightHand.Virbrate(0.5f, 0.5f);
            }
            Instance.correctSound.Play();
            Instance.currentlyAttachedObj = null;
        }
    }

    public static void SpawnTarget()
    {
        DestroyCurrentTarget();
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        Vector3 headForward = CustomRay.Instance.head.transform.forward;

        GameObject newTarget = Instantiate(Instance.targetPrefab, Instance.targets.transform);
        string id = string.Format("{0,3:000}", TargetId);
        newTarget.name = "Target_"+ id;

        Vector3 newPos = Vector3.zero;
        Vector3 newDirection = Vector3.zero;
        float distance = 0;
        do
        {
            float x = Random.Range(-Instance.randomRangeX, Instance.randomRangeX);
            float y = Random.Range(-Instance.randomRangeY, Instance.randomRangeY);
            //float z = Random.Range(-160, 160);
            float z = 0;
            distance = Random.Range(3, 20);
            newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
            newPos = headPos + newDirection;
        }
        while (!CorrectPosition(headPos, newDirection, distance));

        newTarget.transform.position = newPos;
        Instance.currentTarget = newTarget;
    }

    private static bool CorrectPosition(Vector3 headPos, Vector3 newDirection, float distance)
    {
        bool correct = true;

        correct = !Physics.Raycast(headPos, newDirection, distance);
        if(Instance.lastTargetDirection != Vector3.zero)
        {
            float angleBetweenLastAndCurrent = Vector3.Angle(newDirection, Instance.lastTargetDirection);
            if (angleBetweenLastAndCurrent > Instance.maximumAngleBetweenTwoTargets)
                correct = false;
        }

        return correct;
    }

    public static void MoveObjects()
    {
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        bool newPosFound = false;
        Vector3 newPos = Vector3.zero;

        for(int i=0;i< Instance.objectArray.Length;i++)
        {
            GameObject obj = Instance.objectArray[i];
            newPosFound = false;
            while(!newPosFound)
            {
                float x = Random.Range(-Instance.randomRangeX, Instance.randomRangeX);
                float y = Random.Range(-Instance.randomRangeY, Instance.randomRangeY);
                //float z = Random.Range(-45, 45);
                float z = 0;
                float distance = Random.Range(5, 20);
                Vector3 newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
                newPos = headPos + newDirection;
                obj.transform.position = newPos;
                obj.transform.localScale = new Vector3(Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f), Random.Range(0.5f, 1.5f));
                obj.SetActive(true);
                newPosFound = CheckPosition(newPos, i, obj.GetComponent<Collider>());
            }
        }
    }

    private static bool CheckPosition(Vector3 newPos, int i, Collider collider)
    {
        if(Vector3.Distance(Instance.currentTarget.transform.position, newPos) < 0.05f)
        {
            return false;
        }

        if(CurrentTarget.GetComponent<Collider>().bounds.Intersects(collider.bounds))
        {
            return false;
        }

        for(int j = 0;j<i;j++)
        {
            GameObject obj = Instance.objectArray[j];
            if (obj.GetComponent<Collider>().bounds.Intersects(collider.bounds))
            {
                return false;
            }
        }
        return true;
    }

    public static void DeactivateAllObstacles()
    {
        foreach (GameObject obj in Instance.objectArray)
        {
            obj.SetActive(false);
        }
    }

    public static void DestroyCurrentTarget()
    {
        if (Instance.currentTarget != null)
        {
            Destroy(Instance.currentTarget);
        }
    }

    public static bool IsAnyObjectAttached()
    {
        if(Instance.currentlyAttachedObj!=null)
        {
            return true;
        }
        return false;
    }
}

