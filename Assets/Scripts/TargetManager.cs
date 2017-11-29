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
            currentFocusedObject.SetActive(false);
            Debug.Log("LeftClick");
            HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
            HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            correctSound.Play();
        }
        else
        {
            DetachTargetFromDepthMarker(currentFocusedObject);
        }
    }

    public void AttachTargetToDepthMarker(GameObject currentFocusedObject, Handeness handenessDidNotClicked)
    {
        if (currentlyAttachedObj == currentFocusedObject)
            return;
        else if(currentlyAttachedObj!=null)
        {
            DetachTargetFromDepthMarker(currentFocusedObject);
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
            HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
            HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
            correctSound.Play();
        }
    }
    public void DetachTargetFromDepthMarker(GameObject currentFocusedObject)
    {
        if (currentlyAttachedObj != null)
        {
            Target target = currentlyAttachedObj.GetComponent<Target>();

            if (target.State == TargetState.Drag)
            {
                if(currentlyAttachedObj == currentFocusedObject)
                {
                    target.State = TargetState.InFocus;
                }
                else
                {
                    target.State = TargetState.Default;
                }
                HandManager.Instance.LeftHand.Virbrate(0.5f, 0.5f);
                HandManager.Instance.RightHand.Virbrate(0.5f, 0.5f);
                correctSound.Play();
            }
            currentlyAttachedObj = null;
        }
    }
  

    public void UpdateTransparency(Vector3 markerPos, Vector3 headPos, Vector3 rayDirection)
    {
        float distanceMarkerHead = Vector3.Distance(markerPos, headPos);
        
        foreach (GameObject obj in TargetArray)
        {
            UpdateTransparancy(headPos,rayDirection,obj,distanceMarkerHead);
        }
    }

    public void UpdateTransparancy(Vector3 headPos, Vector3 rayDirection, GameObject obj, float distanceMarkerHead)
    {
        Vector3 dirHeadMarker = obj.transform.position - headPos;
        float angle = Vector3.Angle(rayDirection, dirHeadMarker);
        Target target = obj.GetComponent<Target>();
        float distanceObjHead = Vector3.Distance(obj.transform.position, headPos);

        if (angle < 10 && angle > -10 && distanceMarkerHead > distanceObjHead-0.05)
        {
            if (target.State == TargetState.Default)
            {
                obj.GetComponent<Renderer>().material = transparentMat;
            }
            else if (target.State == TargetState.InFocus)
            {
                obj.GetComponent<Renderer>().material = targetInFocusTransparent;
            }
        }
        else
        {
            if (target.State == TargetState.Default)
            {
                obj.GetComponent<Renderer>().material = target.DefaultMat;
                target.State = TargetState.Default;
            }
            else if (target.State == TargetState.InFocus)
            {
                obj.GetComponent<Renderer>().material = targetInFocus;
                target.State = TargetState.InFocus;
            }
        }
    }
}

