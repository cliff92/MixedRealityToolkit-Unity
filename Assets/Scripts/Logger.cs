using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class Logger : MonoBehaviour
{
    public static Logger Instance;
    public string userId = "ck92";
    public string FileName = "log"; 
    // This contains the name of the file. Don't add the ".txt"
    // Assign in inspector
    private StreamWriter writer; // This is the writer that writes to the file
    internal static void AppendString(string appendString)
    {
        Instance.writer = new StreamWriter(Application.dataPath + "/Resources/" + Instance.FileName + "_" + Instance.userId + ".txt", true);
        Instance.writer.WriteLine(appendString);
        Instance.writer.Close();
    }

    private void Awake()
    {
        if(Instance== null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(this);
        }
        
    }

    private void Start()
    {
        AppendString("New Run");
    }
    internal static void AddCurrentTime()
    {
        AppendString("Current Time: " + Time.time);
    }

    public void ChangeUserId(InputField inputfield)
    {
        Instance.userId = inputfield.text;
    }
}
