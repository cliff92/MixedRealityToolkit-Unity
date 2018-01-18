using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocusMat;
    public Material targetNotInFocusMat;

    private GameObject currentlyAttachedObj;
    private GameObject[] targetArray;

    public GameObject targets;
    private int targetId = 0;

    public delegate void TargetClickedMethod(Target target);
    public event TargetClickedMethod TargetClicked;

    public delegate void TargetAttachedMethod(Target target);
    public event TargetAttachedMethod TargetAttached;

    public delegate void TargetDetachedMethod(Target target);
    public event TargetDetachedMethod TargetDetached;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClickManager.Instance.LeftClick += LeftClick;
        ClickManager.Instance.RightClick += RightClick;
        ClickManager.Instance.Reset += Reset;
        targetArray = GameObject.FindGameObjectsWithTag("Target");
        DeactivateTargets();
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
        ClickManager.Instance.RightClick -= RightClick;
        ClickManager.Instance.Reset -= Reset;
    }

    public static void Reset()
    {
        MoveAllTargets();
        ObstacleManager.MoveObjects();
    }

    private void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
        {
            Logger.IncreaseClickMissCount();
            return;
        }
        Target target = currentFocusedObject.GetComponent<Target>();
        if (target == null || target.State == TargetState.Disabled)
        {
            if(currentFocusedObject.tag != "UI")
                Logger.IncreaseClickWrongCount();
            return;
        }
        Logger.IncreaseClickCorrectCount();

        if (!SceneHandler.UseLeftClick)
            return;

        target.Deactivate();
        currentFocusedObject.SetActive(false);

        HandManager.CurrentHand.Vibrate(Thalmic.Myo.VibrationType.Short);

        AudioManager.PlayCorrectSound();
        if (Instance.TargetClicked != null)
            Instance.TargetClicked(target);
        Debug.LogError("Click "+ Time.time);
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

    public static void ActivateSingleTarget(Vector3 lastTargetDirection)
    {
        DeactivateTargets();
        if (Instance.targetArray != null)
        {
            MoveTarget(Instance.targetArray[0], lastTargetDirection);
        }
        else
        {
            Debug.LogError("No Targets Found");
        }
    }

    public static void MoveAllTargets()
    {
        DeactivateTargets();

        foreach(GameObject target in Instance.targetArray)
        {
            MoveTarget(target, Vector3.zero);
        }
    }

    private static void MoveTarget(GameObject target, Vector3 lastTargetDirection)
    {
        Vector3 headPos = CustomRay.Instance.head.transform.position;
        Vector3 headForward = CustomRay.Instance.head.transform.forward;

        target.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        string id = string.Format("{0,3:000}", TargetId);
        target.name = "Target_"+ id;

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
            target.transform.position = newPos;
            target.SetActive(true);
        }
        while (!CorrectPosition(headPos, newDirection, lastTargetDirection, distance, target.GetComponent<Collider>()));
        target.GetComponent<Target>().Activate();
    }

    private static bool CorrectPosition(Vector3 headPos, Vector3 newDirection, Vector3 lastTargetDirection, float distance, Collider collider)
    {
        if(!VariablesManager.WorldCollider.bounds.Intersects(collider.bounds))
        {
            return false;
        }

        if(lastTargetDirection != Vector3.zero)
        {
            float angleBetweenLastAndCurrent = Vector3.Angle(newDirection, lastTargetDirection);
            if (angleBetweenLastAndCurrent > VariablesManager.MaximumAngleBetweenTwoTargets
                || angleBetweenLastAndCurrent < VariablesManager.MinimumAngleBetweenTwoTargets)
                return false;
        }

        foreach(GameObject target in Instance.targetArray)
        {
            if (target == collider.gameObject)
                continue;
            if(target.activeSelf)
            {
                if (target != null && target.GetComponent<Collider>().bounds.Intersects(collider.bounds))
                {
                    return false;
                }
            }
        }

        foreach (Collider col in VariablesManager.InvalidSpawingAreas)
        {
            if (col.bounds.Intersects(collider.bounds))
            {
                return false;
            }
        }

        return true;
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
                DepthMarker.Instance.MoveDepthMarkerToFocus();
                target.State = TargetState.Drag;
                target.HandnessDidNotClick = handenessDidNotClicked;
                Instance.currentlyAttachedObj = currentFocusedObject;
            }

            HandManager.CurrentHand.Vibrate(Thalmic.Myo.VibrationType.Short);

            AudioManager.PlayCorrectSound();
            target.StartTimeAttached = Time.time;
            MeasurementManager.OnLeftClick(target);
        }
    }

    public static void DetachTargetFromDepthMarker(GameObject target)
    {
        if(Instance.currentlyAttachedObj == target)
        {
            DetachTargetFromDepthMarker();
        }
    }

    public static void DetachTargetFromDepthMarker()
    {
        if (Instance != null && Instance.currentlyAttachedObj != null)
        {
            Target target = Instance.currentlyAttachedObj.GetComponent<Target>();
            target.State = TargetState.Default;

            HandManager.CurrentHand.Vibrate(Thalmic.Myo.VibrationType.Short);

            AudioManager.PlayCorrectSound();
            Instance.currentlyAttachedObj = null;

            if (target.insideStorage)
            {
                Logger.IncreaseDetachCountInsideStorage();
            }
            else
            {
                Logger.IncreaseDetachCoutOutsideStorage();
            }
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


    private static int TargetId
    {
        get
        {
            if (Instance == null)
                return 0;
            return Instance.targetId++;
        }
    }

    public static GameObject[] CurrentTargets
    {
        get
        {
            if (Instance != null && Instance.targetArray != null)
            {
                return Instance.targetArray;
            }
            return null;
        }
    }

    public static GameObject CurrentTarget
    {
        get
        {
            if (Instance != null && Instance.targetArray != null && Instance.targetArray.Length == 1)
            {
                return Instance.targetArray[0];
            }
            return null;
        }
    }

    public static GameObject CurrentlyAttachedObj
    {
        get
        {
            return Instance.currentlyAttachedObj;
        }
    }

    public static void DeactivateTargets()
    {
        if (Instance.targetArray != null)
        {
            for (int i = 0; i < Instance.targetArray.Length; i++)
            {
                if (Instance.targetArray[i] != null)
                {
                    Instance.targetArray[i].SetActive(false);
                }
            }
        }
    }
}

