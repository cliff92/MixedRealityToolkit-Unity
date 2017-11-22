using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;

    public Material targetInFocus;
    public Material targetNotInFocus;
    public Material objectNotInFocus;
    public Material objectInFocus;
    public Material transparentMat;

    public float transparencyFactor = 0.5f;

    private GameObject currentFocusedObject;
    private GameObject oldFocusedObject;

    [Tooltip("Time where a click is still counted even when the object is not in focus anymore")]
    public float DelayClickTime;

    private float timeSinceOldTargettInFocus;

    private GameObject[] targetArray;

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
        timeSinceOldTargettInFocus = 0;
        currentFocusedObject = null;
        oldFocusedObject = null;

        InputManager.Instance.AddGlobalListener(gameObject);
        DepthRayManager.Instance.PointerSpecificFocusChanged += OnPointerSpecificFocusChanged;

        targetArray = GameObject.FindGameObjectsWithTag("Target");
    }

    private void Update()
    {
        if (timeSinceOldTargettInFocus >= 0)
        {
            timeSinceOldTargettInFocus += Time.deltaTime;
        }

        if (Input.GetButtonUp("RelativeLeft") || Input.GetButtonUp("RelativeRight"))
        {
            if (timeSinceOldTargettInFocus > 0 && timeSinceOldTargettInFocus < 0.05f)
            {
                oldFocusedObject.SetActive(false);
                Debug.Log("HitOld");
            }
            else
            {
                if (currentFocusedObject != null && currentFocusedObject.tag.Equals("Target"))
                {
                    currentFocusedObject.SetActive(false);
                    Debug.Log("HitNew");
                }
            }
        }
    }
    protected virtual void OnPointerSpecificFocusChanged(IPointingSource pointer, GameObject oldFocusedObject, GameObject newFocusedObject)
    {
        
        this.oldFocusedObject = oldFocusedObject;
        currentFocusedObject = newFocusedObject;

        if (oldFocusedObject != null)
        {
            switch (oldFocusedObject.tag)
            {
                case "Target":
                    Target target = oldFocusedObject.GetComponent<Target>();
                    target.State = target.OldState;
                    timeSinceOldTargettInFocus = 0;
                    switch (target.State)
                    {
                        case TargetState.Default:
                            oldFocusedObject.GetComponent<Renderer>().material = targetNotInFocus;
                            break;
                        case TargetState.Transparent:
                            oldFocusedObject.GetComponent<Renderer>().material = transparentMat;
                            break;
                    }
                    break;
                case "Object":
                    timeSinceOldTargettInFocus = -1;
                    oldFocusedObject.GetComponent<Renderer>().material = objectNotInFocus;
                    break;
            }
        }

        if (newFocusedObject != null)
        {
            switch (newFocusedObject.tag)
            {
                case "Target":
                    Target target = newFocusedObject.GetComponent<Target>();
                    target.State = TargetState.InFocus;
                    newFocusedObject.GetComponent<Renderer>().material = targetInFocus;
                    Debug.Log("Target {0} in Foucs",target.gameObject);
                    break;
                case "Object":
                    newFocusedObject.GetComponent<Renderer>().material = objectInFocus;
                    break;
            }
        }
    }

    public void UpdateTransparency(Vector3 markerPos, Vector3 headPos)
    {
        float distanceBH = Vector3.Distance(markerPos, headPos);
        foreach (GameObject obj in TargetArray)
        {
            Target target = obj.GetComponent<Target>();
            float distanceHO = Vector3.Distance(obj.transform.position, headPos);
            if(distanceBH < distanceHO)
            {
                if (target.State == TargetState.Transparent)
                {
                    obj.GetComponent<Renderer>().material = target.DefaultMat;
                    target.State = TargetState.Default;
                }
            }
            else if (target.State == TargetState.Default) { 
                obj.GetComponent<Renderer>().material = transparentMat;
                target.State = TargetState.Transparent;
            }
        }
    }
}

