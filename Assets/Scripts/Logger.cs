using UnityEngine;
using System.IO;
using System.Text;

public class Logger : MonoBehaviour
{
    public static Logger Instance;
    public string FileName = "log"; 
    // This contains the name of the file. Don't add the ".txt"
    // Assign in inspector
    private StreamWriter writer; // This is the writer that writes to the file
    internal static void AppendString(string appendString)
    {
        Instance.writer = new StreamWriter(Application.dataPath + "/Resources/" + Instance.FileName + ".txt", true);
        Instance.writer.WriteLine(appendString + "\n");
        Instance.writer.Close();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AppendString("New Run");
    }
    internal static void AddCurrentTime()
    {
        AppendString("Current Time: " + Time.time);
    }
}
