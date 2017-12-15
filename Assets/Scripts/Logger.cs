using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static Logger Instance;
    public string FileName = "log"; // This contains the name of the file. Don't add the ".txt"
                            // Assign in inspector
    private TextAsset asset; // Gets assigned through code. Reads the file.
    private StreamWriter writer; // This is the writer that writes to the file
    internal void AppendString(string appendString)
    {
        asset = Resources.Load(FileName + ".txt") as TextAsset;
        writer = new StreamWriter(Application.dataPath+"/Resources/" + FileName + ".txt",true);
        writer.WriteLine(appendString+"\n");
        writer.Close();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        AppendString("New Run");
    }
}
