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

    public GameObject[] TargetArray
    {
        get
        {
            return targetArray;
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

    public static bool IsAnyObjectAttached()
    {
        if(Instance.currentlyAttachedObj!=null)
        {
            return true;
        }
        return false;
    }
}

