using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocus;
    public Material targetInFocusTransparent;
    public Material targetNotInFocus;
    public Material objectNotInFocus;
    public Material objectInFocus;
    public Material transparentMat;

    public AudioSource correctSound;

    private GameObject[] targetArray;

    private GameObject currentlyAttachedObj;

    public GameObject targets;
    public GameObject targetPrefab;
    private int targetId = 0;

    public static GameObject[] TargetArray
    {
        get
        {
            return Instance.targetArray;
        }
    }

    private static int TargetId
    {
        get
        {
            return Instance.targetId++;
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

        targetArray = GameObject.FindGameObjectsWithTag("Target");

        currentlyAttachedObj = null;
    }

    private void OnDestroy()
    {
        ClickManager.Instance.LeftClick -= LeftClick;
        ClickManager.Instance.Reset -= Reset;
    }

    private void Reset()
    {
        foreach (GameObject obj in targetArray)
        {
            obj.SetActive(true);
        }
    }

    public static void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
            return;
        Target target = currentFocusedObject.GetComponent<Target>();
        if (target.State != TargetState.Drag)
        {
            target.State = TargetState.Disabled;
            target.LogClick();
            currentFocusedObject.SetActive(false);
            if (InputSwitcher.InputMode == InputMode.Myo)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.RightHand.Virbrate(0.5f, 0.5f);
            }
            Instance.correctSound.Play();
            SpawnTarget(currentFocusedObject.transform.position);
        }
        else
        {
            DetachTargetFromDepthMarker();
        }
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
            if (InputSwitcher.InputMode == InputMode.Myo)
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
        if (Instance.currentlyAttachedObj != null)
        {
            Target target = Instance.currentlyAttachedObj.GetComponent<Target>();
            target.State = TargetState.Default;
            target.UpdateTransparancy();
            if (InputSwitcher.InputMode == InputMode.Myo)
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

    public static void SpawnTarget(Vector3 posLastTarget)
    {
        Vector3 headPos = HeadRay.Instance.head.transform.position;
        Vector3 headForward = HeadRay.Instance.head.transform.forward;

        GameObject newTarget = Instantiate(Instance.targetPrefab, Instance.targets.transform);
        string id = string.Format("{0,3:000}", TargetId);
        newTarget.name = "Target_"+ id;
        newTarget.GetComponent<Target>().PosLastTarget = posLastTarget;

        bool correctPos = false;
        Vector3 newPos = Vector3.zero;
        while(!correctPos)
        {
            float x = Random.Range(-45, 45);
            float y = Random.Range(-45, 45);
            float z = Random.Range(-45, 45);
            float distance = Random.Range(3, 20);
            Vector3 newDirection = Quaternion.Euler(x, y, z) * Vector3.forward * distance;
            newPos = headPos + newDirection;

            correctPos = !Physics.Raycast(headPos, newDirection, distance);
        }

        newTarget.transform.position = newPos;
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

