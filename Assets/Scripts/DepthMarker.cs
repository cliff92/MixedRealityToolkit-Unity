﻿using UnityEngine;
public class DepthMarker : MonoBehaviour
{
    public static DepthMarker Instance;
    public GameObject depthMarkerObj;
    public GameObject rayVisual;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MoveAndScaleDepthMarker();
        MoveRayVisual();
    }

    private void Update()
    {
        float rotationAngle;
        if (HandManager.IsMyoTracked)
        {
            if (HandManager.MyoHand.TryGetRotationAroundZ(out rotationAngle))
            {
                MoveDepthRayRelZ(Mathf.RoundToInt(rotationAngle));
            }
        }
        else
        {
            if (HandManager.RightHand.TryGetRotationAroundZ(out rotationAngle))
            {
                MoveDepthRayRelZ(Mathf.RoundToInt(rotationAngle));
            }
        }


        if (Input.GetButtonUp("JumpToFocus") || MyoPoseManager.Jump)
        {
            MoveDepthMarkerToFocus();
        }

        MoveAndScaleDepthMarker();
        MoveRayVisual();
    }

    private void MoveAndScaleDepthMarker()
    {
        Vector3 origin = CustomRay.Instance.Rays[0].origin;
        Vector3 direction = CustomRay.Instance.Rays[0].direction;
        float distance = Vector3.Distance(depthMarkerObj.transform.position, origin);
        Vector3 newPos = origin + direction * distance;

        depthMarkerObj.transform.position = newPos;
        depthMarkerObj.transform.rotation = Quaternion.LookRotation(direction);

        //Scale depthmarker based on distance to origin
        depthMarkerObj.transform.localScale = new Vector3(1, 1, 1) * Mathf.Sqrt(Vector3.Distance(newPos, origin));
    }

    private void MoveRayVisual()
    {
        Vector3 origin = CustomRay.Instance.Rays[0].origin;
        Vector3 direction = CustomRay.Instance.Rays[0].direction;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rayVisual.transform.rotation = rotation;
        rayVisual.transform.position = origin;
    }

    private void MoveDepthRayRelZ(int rotationAngle)
    {
        float factor = 20;
        if (ClickManager.IsClick)
        {
            factor = 5;
        }
        //change position
        Vector3 origin = DepthRayManager.StartPoint;

        float stepsize = 0;
        if (VariablesManager.InputMode == InputMode.HeadMyoHybrid)
        {
            if (rotationAngle > 0 && rotationAngle < 180)
            {
                stepsize = factor * ((rotationAngle + 80) / 180.0f) * Time.deltaTime;
            }
            else if (rotationAngle < -50 && rotationAngle > -180)
            {
                stepsize = factor * (rotationAngle / 180.0f) * Time.deltaTime;
            }
        }
        else
        {
            if (rotationAngle > 30 && rotationAngle < 180)
            {
                stepsize = factor * (rotationAngle / 180.0f) * Time.deltaTime;
            }
            else if (rotationAngle < -30 && rotationAngle > -180)
            {
                stepsize = factor * (rotationAngle / 180.0f) * Time.deltaTime;
            }
        }
        Logger.AddStepsizeToDepthMarkerMovementInZ(stepsize);

        Vector3 newPos = Vector3.MoveTowards(depthMarkerObj.transform.position, origin, stepsize);

        depthMarkerObj.transform.position = CheckDepthMarkerPos(newPos, origin);
    }

    private Vector3 CheckDepthMarkerPos(Vector3 newPos, Vector3 origin)
    {
        if (Vector3.Distance(newPos, origin) < Vector3.Distance(depthMarkerObj.transform.position, origin)
            && Vector3.Distance(newPos, origin) < 0.5f)
        {
            return depthMarkerObj.transform.position;
        }
        RaycastHit hit;
        int layerMask = 1 << LayerMask.NameToLayer("WorldLayer");
        if (Physics.Raycast(origin, newPos - origin, out hit, Vector3.Distance(origin, newPos), layerMask))
        {
            newPos = hit.point - (newPos - origin).normalized * 0.05f;
        }

        return newPos;
    }

    public void MoveDepthMarkerToFocus()
    {
        if(DepthRayManager.EndPoint == Vector3.zero)
            return;
        Vector3 origin = CustomRay.Instance.Rays[0].origin;
        Vector3 direction = CustomRay.Instance.Rays[0].direction;
        float distance = Vector3.Distance(DepthRayManager.EndPoint, origin);
        depthMarkerObj.transform.position = origin + direction * distance;
    }

    public void MoveDepthMarkerToUser()
    {
        Vector3 origin = CustomRay.Instance.Rays[0].origin;
        Vector3 direction = CustomRay.Instance.Rays[0].direction;
        depthMarkerObj.transform.position = origin + direction * 0.5f;
    }

    public static GameObject DepthMarkerObj
    {
        get{
            if (Instance == null)
                return null;
            return Instance.depthMarkerObj;
        }
    }

    public static Vector3 Position
    {
        get
        {
            if (Instance == null || Instance.depthMarkerObj == null)
                return Vector3.zero;
            return Instance.depthMarkerObj.transform.position;
        }
    }
}

