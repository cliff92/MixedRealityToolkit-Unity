using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocusMat;
    public Material targetNotInFocusMat;

    public AudioSource correctSound;

    private GameObject currentTarget;
    private GameObject currentlyAttachedObj;
    private GameObject[] targetArray;

    public GameObject targets;
    private GameObject targetPrefab;
    private int targetId = 0;

    public delegate void TargetClickedMethod(Target target);
    public event TargetClickedMethod TargetClicked;

    public delegate void TargetAttachedMethod(Target target);
    public event TargetAttachedMethod TargetAttached;

    public delegate void TargetDetachedMethod(Target target);
    public event TargetDetachedMethod TargetDetached;

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
        targetPrefab = Resources.Load("TargetPrefab", typeof(GameObject)) as GameObject;
    }

    private void Start()
    {
        ClickManager.Instance.LeftClick += LeftClick;
        ClickManager.Instance.RightClick += RightClick;
        ClickManager.Instance.Reset += Reset;
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
        ClickManager.Instance.RightClick -= RightClick;
        ClickManager.Instance.Reset -= Reset;
    }

    public static void Reset()
    {
        SpawnTarget(Vector3.zero,PrimitiveType.Cube);
        ObstacleManager.MoveObjects();
    }

    private void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
            return;
        Target target = currentFocusedObject.GetComponent<Target>();
        if (target == null)
            return;

        target.State = TargetState.Disabled;
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
        if (Instance.TargetClicked != null)
            Instance.TargetClicked(target);
    }

    private void RightClick(GameObject currentFocusedObject)
    {
        if (Input.GetButton("RelativeLeft") || MyoPoseManager.Arm == Thalmic.Myo.Arm.Left)
        {
            AttachTargetToDepthMarker(currentFocusedObject, Handeness.Right);
        }
        else
        {
            AttachTargetToDepthMarker(currentFocusedObject, Handeness.Left);
        }
    }

    public static void SpawnSingleTarget(Vector3 lastTargetDirection, PrimitiveType primitiveType)
    {
        DestroyTargets();
        Instance.currentTarget = SpawnTarget(lastTargetDirection, primitiveType);
    }

    public static void SpawnTwoTypeTargets(int amount, PrimitiveType primitiveType, PrimitiveType primitiveType2)
    {
        DestroyTargets();
        Instance.targetArray = new GameObject[2*amount];

        for(int i=0;i<amount; i++)
        {
            Instance.targetArray[i] = SpawnTarget(Vector3.zero, primitiveType);
            Instance.targetArray[i+amount] = SpawnTarget(Vector3.zero, primitiveType2);
        }
    }

    private static GameObject SpawnTarget(Vector3 lastTargetDirection, PrimitiveType primitiveType)
    {
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        Vector3 headForward = CustomRay.Instance.head.transform.forward;

        GameObject newTarget = GameObject.CreatePrimitive(primitiveType);
        newTarget.transform.parent = Instance.targets.transform;
        newTarget.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        newTarget.GetComponent<Renderer>().material = Instance.targetNotInFocusMat;
        newTarget.AddComponent<Target>();
        newTarget.tag = "Target";
        newTarget.layer = LayerMask.NameToLayer("TargetLayer");

        string id = string.Format("{0,3:000}", TargetId);
        newTarget.name = "Target_"+ id;

        Vector3 newPos = Vector3.zero;
        Vector3 newDirection = Vector3.zero;
        float distance = 0;
        do
        {
            float x = Random.Range(-VariablesManager.RandomRangeX, VariablesManager.RandomRangeX);
            float y = Random.Range(-VariablesManager.RandomRangeY, VariablesManager.RandomRangeY);
            //float z = Random.Range(-160, 160);
            float z = 0;
            distance = Random.Range(3, 20);
            newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
            newPos = headPos + newDirection;
        }
        while (!CorrectPosition(headPos, newDirection, lastTargetDirection, distance));

        newTarget.transform.position = newPos;

        return newTarget;
    }

    private static bool CorrectPosition(Vector3 headPos, Vector3 newDirection, Vector3 lastTargetDirection, float distance)
    {
        bool correct = true;

        correct = !Physics.Raycast(headPos, newDirection, distance);
        if(lastTargetDirection != Vector3.zero)
        {
            float angleBetweenLastAndCurrent = Vector3.Angle(newDirection, lastTargetDirection);
            if (angleBetweenLastAndCurrent > VariablesManager.MaximumAngleBetweenTwoTargets)
                correct = false;
        }

        return correct;
    }

    public static void AttachTargetToDepthMarker(GameObject currentFocusedObject, Handeness handenessDidNotClicked)
    {
        if (Instance.currentlyAttachedObj == currentFocusedObject)
            return;
        else if (Instance.currentlyAttachedObj != null)
        {
            DetachTargetFromDepthMarker();
        }
        if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
        {
            Target target = currentFocusedObject.GetComponent<Target>();
            if (target.State != TargetState.Drag)
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
        if (Instance != null && Instance.currentlyAttachedObj != null)
        {
            Target target = Instance.currentlyAttachedObj.GetComponent<Target>();
            target.State = TargetState.Default;
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

    public static bool IsAnyObjectAttached()
    {
        if (Instance != null && Instance.currentlyAttachedObj != null)
        {
            return true;
        }
        return false;
    }

    public static void DestroyTarget(GameObject target)
    {
        if (target != null)
        {
            Destroy(target);
        }
    }

    public static void DestroyCurrentTarget()
    {
        if (Instance.currentTarget != null)
        {
            Destroy(Instance.currentTarget);
            Instance.currentTarget = null;
        }
    }

    public static void DestroyTargets()
    {
        DestroyCurrentTarget();
        if (Instance.targetArray != null)
        {
            for (int i = 0; i < Instance.targetArray.Length; i++)
            {
                if (Instance.targetArray[i] != null)
                {
                    Destroy(Instance.targetArray[i]);
                    Instance.targetArray[i] = null;
                }
            }
        }
    }
}

