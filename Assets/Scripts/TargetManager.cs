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

    public GameObject[] TargetArray
    {
        get
        {
            return targetArray;
        }
    }

    private int TargetId
    {
        get
        {
            return targetId++;
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

    public void LeftClick(GameObject currentFocusedObject)
    {
        if (currentFocusedObject == null)
            return;
        Target target = currentFocusedObject.GetComponent<Target>();
        if (target.State != TargetState.Drag)
        {
            target.State = TargetState.Disabled;
            target.LogClick();
            currentFocusedObject.SetActive(false);
            if (MyoPoseManager.Instance.useMyo)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            }
            correctSound.Play();
            SpawnTarget(currentFocusedObject.transform.position);
        }
        else
        {
            DetachTargetFromDepthMarker();
        }
    }

    public void AttachTargetToDepthMarker(GameObject currentFocusedObject, Handeness handenessDidNotClicked)
    {
        if (currentlyAttachedObj == currentFocusedObject)
            return;
        else if(currentlyAttachedObj!=null)
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
                currentlyAttachedObj = currentFocusedObject;
            }
            if (MyoPoseManager.Instance.useMyo)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            }
            correctSound.Play();
        }
    }
    public void DetachTargetFromDepthMarker()
    {
        if (currentlyAttachedObj != null)
        {
            Target target = currentlyAttachedObj.GetComponent<Target>();
            target.State = TargetState.Default;
            target.UpdateTransparancy();
            if (MyoPoseManager.Instance.useMyo)
            {
                MyoPoseManager.Instance.Vibrate();
            }
            else
            {
                HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            }
            correctSound.Play();
            currentlyAttachedObj = null;
        }
    }

    public void SpawnTarget(Vector3 posLastTarget)
    {
        Vector3 headPos = HeadRay.Instance.head.transform.position;
        Vector3 headForward = HeadRay.Instance.head.transform.forward;

        GameObject newTarget = Instantiate(targetPrefab, targets.transform);
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

