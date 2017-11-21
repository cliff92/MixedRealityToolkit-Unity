using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;

public class DepthRayManager : MonoBehaviour
{
    public static DepthRayManager Instance;

    public GameObject rayVisual;
    public GameObject depthMarker;

    private HeadRay headRay;
    public float pointingExtent = 100;

    public delegate void FocusEnteredMethod(GameObject focusedObject);
    public event FocusEnteredMethod FocusEntered;

    public delegate void FocusExitedMethod(GameObject unfocusedObject);
    public event FocusExitedMethod FocusExited;

    public delegate void PointerSpecificFocusChangedMethod(IPointingSource pointer, GameObject oldFocusedObject, GameObject newFocusedObject);
    public event PointerSpecificFocusChangedMethod PointerSpecificFocusChanged;

    private PointerData pointer;

    [SerializeField]
    [Tooltip("The LayerMasks, in prioritized order, that are used to determine the HitObject when raycasting.")]
    private LayerMask[] pointingRaycastLayerMasks;

    private void Awake()
    {
        Instance = this;
        pointingRaycastLayerMasks = new LayerMask[]{ LayerMask.GetMask("TargetLayer"),Physics.DefaultRaycastLayers};
    }

    private void Start()
    {
        headRay = HeadRay.Instance;
        pointer = new PointerData(headRay);
        StartUp();
    }

    private void StartUp()
    {
        headRay.OnPreRaycast();
        MoveDepthMarker();
        MoveRayVisual();
        SelectObject();
        UpdateFocusedObjects();
        UpdateTransparency();
    }

    private void Update()
    {
        headRay.OnPreRaycast();

        float stepSizeLeft = Input.GetAxis("TouchPadYLeft");
        float stepSizeRight = Input.GetAxis("TouchPadYRight");
        if (stepSizeLeft > 0.5f || stepSizeLeft < -0.5f)
        {
            MoveDepthRayRelZ(stepSizeLeft * Time.deltaTime);
        }
        if (stepSizeRight > 0.5f || stepSizeRight < -0.5f)
        {
            MoveDepthRayRelZ(stepSizeRight * Time.deltaTime);
        }
        MoveDepthMarker();
        MoveRayVisual();
        SelectObject();
        UpdateFocusedObjects();
    }

    private void MoveDepthMarker()
    {
        Vector3 origin = headRay.Rays[0].origin;
        Vector3 direction = headRay.Rays[0].direction;
        float distance = Vector3.Distance(depthMarker.transform.position, origin);
        Vector3 newPos = origin + direction * distance;
        depthMarker.transform.position = newPos;
    }

    private void MoveRayVisual()
    {
        Vector3 origin = headRay.Rays[0].origin;
        Vector3 direction = headRay.Rays[0].direction;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rayVisual.transform.rotation = rotation;
        rayVisual.transform.position = origin;
    }

    private void MoveDepthRayRelZ(float stepSize)
    {
        Vector3 origin = headRay.Rays[0].origin;
        depthMarker.transform.position = Vector3.MoveTowards(depthMarker.transform.position, origin, stepSize);
        UpdateTransparency();
    }

    private void UpdateTransparency()
    {
        Vector3 bubblePos = depthMarker.transform.position;
        Vector3 headPos = headRay.Rays[0].origin;
        TargetManager.Instance.UpdateTransparency(bubblePos,headPos);
    }

    private void SelectObject()
    {
        // Call the pointer's OnPreRaycast function
        // This will give it a chance to prepare itself for raycasts
        // eg, by building its Rays array
        pointer.PointingSource.OnPreRaycast();

        // If pointer interaction isn't enabled, clear its result object and return
        if (!pointer.PointingSource.InteractionEnabled)
        {
            // Don't clear the previous focused object since we still want to trigger FocusExit events
            pointer.ResetFocusedObjects(false);
        }
        else
        {
            // If the pointer is locked
            // Keep the focus objects the same
            // This will ensure that we execute events on those objects
            // even if the pointer isn't pointing at them
            if (!pointer.PointingSource.FocusLocked)
            {
                // Otherwise, continue
                var prioritizedLayerMasks = (pointer.PointingSource.PrioritizedLayerMasksOverride ?? pointingRaycastLayerMasks);

                // Perform raycast to determine focused object
                RaycastPhysics(pointer, prioritizedLayerMasks);

                // Set the pointer's result last
                pointer.PointingSource.Result = pointer;
            }
        }

        // Call the pointer's OnPostRaycast function
        // This will give it a chance to respond to raycast results
        // eg by updating its appearance
        pointer.PointingSource.OnPostRaycast();
    }


    /// <summary>
    /// Perform a Unity physics Raycast to determine which scene objects with a collider is currently being gazed at, if any.
    /// </summary>
    private void RaycastPhysics(PointerData pointer, LayerMask[] prioritizedLayerMasks)
    {
        bool isHit = false;
        int rayStepIndex = 0;
        RayStep rayStep = default(RayStep);
        RaycastHit physicsHit = default(RaycastHit);

        Debug.Assert(pointer.PointingSource.Rays != null, "No valid rays for " + pointer.GetType());
        Debug.Assert(pointer.PointingSource.Rays.Length > 0, "No valid rays for " + pointer.GetType());

        // Check raycast for each step in the pointing source
        for (int i = 0; i < pointer.PointingSource.Rays.Length; i++)
        {
            if (RaycastPhysicsStep(pointer.PointingSource.Rays[i], prioritizedLayerMasks, out physicsHit))
            {
                // Set the pointer source's origin ray to this step
                isHit = true;
                rayStep = pointer.PointingSource.Rays[i];
                rayStepIndex = i;
                // No need to continue once we've hit something
                break;
            }
        }

        if (isHit)
        {
            pointer.UpdateHit(physicsHit, rayStep, rayStepIndex);
        }
        else
        {
            pointer.UpdateHit(GetPointingExtent(pointer.PointingSource));
        }
    }
    public float GetPointingExtent(IPointingSource pointingSource)
    {
        return pointingSource.ExtentOverride ?? pointingExtent;
    }


    private bool RaycastPhysicsStep(RayStep step, LayerMask[] prioritizedLayerMasks, out RaycastHit physicsHit)
    {
        bool isHit = false;
        physicsHit = default(RaycastHit);

        // If there is only one priority, don't prioritize
        if (prioritizedLayerMasks.Length == 1)
        {
            isHit = Physics.Raycast(step.origin, step.direction, out physicsHit, step.length, prioritizedLayerMasks[0]);
        }
        else
        {
            // Raycast across all layers and prioritize
            RaycastHit? hit = PrioritizeHits(Physics.RaycastAll(step.origin, step.direction, step.length, Physics.AllLayers), prioritizedLayerMasks);
            isHit = hit.HasValue;

            if (isHit)
            {
                physicsHit = hit.Value;
            }
        }

        return isHit;
    }

    private RaycastHit? PrioritizeHits(RaycastHit[] hits, LayerMask[] layerMasks)
    {
        if (hits.Length == 0)
        {
            return null;
        }

        // Return the minimum distance hit within the first layer that has hits.
        // In other words, sort all hit objects first by layerMask, then by distance.
        for (int layerMaskIdx = 0; layerMaskIdx < layerMasks.Length; layerMaskIdx++)
        {
            RaycastHit? minHit = null;

            for (int hitIdx = 0; hitIdx < hits.Length; hitIdx++)
            {
                RaycastHit hit = hits[hitIdx];
                if (hit.transform.gameObject.layer.IsInLayerMask(layerMasks[layerMaskIdx]))
                {
                    if(minHit == null)
                    {
                        minHit = hit;
                    }
                    else
                    {
                        float newDist = Vector3.Distance(hit.point, depthMarker.transform.position);
                        float minDist = Vector3.Distance(minHit.Value.point, depthMarker.transform.position);
                        if(newDist<minDist)
                        {
                            minHit = hit;
                        }
                    }
                }
            }

            if (minHit != null)
            {
                return minHit;
            }
        }

        return null;
    }

    private void UpdateFocusedObjects()
    {
        // NOTE: We compute the set of events to send before sending the first event
        //       just in case someone responds to the event by adding/removing a
        //       pointer which would change the structures we're iterating over.

        if (pointer.PreviousEndObject != pointer.End.Object)
        {
            RaisePointerSpecificFocusChangedEvents(pointer.PointingSource, pointer.PreviousEndObject, pointer.End.Object);

            if (pointer.PreviousEndObject != null)
            {
                RaiseFocusExitedEvents(pointer.PreviousEndObject);
            }

            if (pointer.End.Object != null)
            {
                RaiseFocusEnteredEvents(pointer.End.Object);
            }
        }
    }

    #region events
    private void RaiseFocusExitedEvents(GameObject unfocusedObject)
    {
        InputManager.Instance.RaiseFocusExit(unfocusedObject);
        //Debug.Log("Focus Exit: " + unfocusedObject.name);
        if (FocusExited != null)
        {
            FocusExited(unfocusedObject);
        }
    }

    private void RaiseFocusEnteredEvents(GameObject focusedObject)
    {
        InputManager.Instance.RaiseFocusEnter(focusedObject);
        //Debug.Log("Focus Enter: " + focusedObject.name);
        if (FocusEntered != null)
        {
            FocusEntered(focusedObject);
        }
    }

    private void RaisePointerSpecificFocusChangedEvents(IPointingSource pointer, GameObject oldFocusedObject, GameObject newFocusedObject)
    {
        
        InputManager.Instance.RaisePointerSpecificFocusChangedEvents(pointer, oldFocusedObject, newFocusedObject);

        if (PointerSpecificFocusChanged != null)
        {
            PointerSpecificFocusChanged(pointer, oldFocusedObject, newFocusedObject);
        }
    }
    #endregion

    private class PointerData : PointerResult
    {
        public readonly IPointingSource PointingSource;

        private PointerInputEventData pointerData;
        public PointerInputEventData UnityUIPointerData
        {
            get
            {
                if (pointerData == null)
                {
                    pointerData = new PointerInputEventData(EventSystem.current);
                }

                return pointerData;
            }
        }

        public PointerData(IPointingSource pointingSource)
        {
            PointingSource = pointingSource;
        }

        public void UpdateHit(RaycastHit hit, RayStep sourceRay, int rayStepIndex)
        {
            LastRaycastHit = hit;
            PreviousEndObject = End.Object;
            RayStepIndex = rayStepIndex;

            StartPoint = sourceRay.origin;
            End = new FocusDetails
            {
                Point = hit.point,
                Normal = hit.normal,
                Object = hit.transform.gameObject
            };
        }

        public void UpdateHit(RaycastResult result, RaycastHit hit, RayStep sourceRay, int rayStepIndex)
        {
            // We do not update the PreviousEndObject here because
            // it's already been updated in the first physics raycast.

            RayStepIndex = rayStepIndex;
            StartPoint = sourceRay.origin;
            End = new FocusDetails
            {
                Point = hit.point,
                Normal = hit.normal,
                Object = result.gameObject
            };
        }

        public void UpdateHit(float extent)
        {
            PreviousEndObject = End.Object;

            RayStep firstStep = PointingSource.Rays[0];
            RayStep finalStep = PointingSource.Rays[PointingSource.Rays.Length - 1];
            RayStepIndex = 0;

            StartPoint = firstStep.origin;
            End = new FocusDetails
            {
                Point = finalStep.terminus,
                Normal = (-finalStep.direction),
                Object = null
            };
        }

        public void ResetFocusedObjects(bool clearPreviousObject = true)
        {
            if (clearPreviousObject)
            {
                PreviousEndObject = null;
            }

            End = new FocusDetails
            {
                Point = End.Point,
                Normal = End.Normal,
                Object = null
            };
        }
    }
}

