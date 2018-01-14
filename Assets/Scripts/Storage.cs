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
                if(target.StartTimeInStorage>0)
                {
                    if (Time.time - target.StartTimeInStorage> VariablesManager.TimeUntilStored)
                    {
                        target.Store(primitiveType);
                    }
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
                target.StartTimeInStorage = -1;
            }
        }
    }
}