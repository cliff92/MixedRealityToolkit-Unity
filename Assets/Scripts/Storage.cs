using UnityEngine;
public class Storage : MonoBehaviour
{

    public PrimitiveType primitiveType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Target")
        {
            Target target = other.GetComponent<Target>();
            if (target != null)
            {
                target.InsideStorage = true;
                if(target.State != TargetState.Drag)
                    target.StartTimeInStorage = Time.time;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Target")
        {
            Target target = other.GetComponent<Target>();
            if(target != null)
            {
                target.InsideStorage = true;
                if (target.StartTimeInStorage>0 && target.State != TargetState.Drag)
                {
                    if (Time.time - target.StartTimeInStorage> VariablesManager.TimeUntilStored)
                    {
                        target.Store(primitiveType);
                    }
                }
                else if(target.State != TargetState.Drag)
                {
                    target.StartTimeInStorage = Time.time;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Target")
        {
            Target target = other.GetComponent<Target>();
            if (target != null)
            {
                target.InsideStorage = false;
                target.StartTimeInStorage = -1;
            }
        }
    }
}