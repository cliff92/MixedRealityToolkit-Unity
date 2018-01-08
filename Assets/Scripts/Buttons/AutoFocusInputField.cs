using UnityEngine;
using UnityEngine.UI;

public class AutoFocusInputField : MonoBehaviour
{
    private void Start()
    {
        if(GetComponent<InputField>()!=null)
            GetComponent<InputField>().ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (GetComponent<InputField>() != null)
                GetComponent<InputField>().ActivateInputField();
        }
    }
}
