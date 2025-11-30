using UnityEngine;

public class StartFlask : MonoBehaviour
{
    private System.Diagnostics.Process flaskProcess;
    private string serverIP = "http://127.0.0.1:5000"; // Default Flask address

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartFlaskServer();
    }

    void StartFlaskServer()
    {
        try
        {
            string flaskPath = "E:/Private/Developer/capstone-project/Assets/CoreFLASK";
            string appPath = flaskPath + "/app.py";
            
            flaskProcess = new System.Diagnostics.Process();
            flaskProcess.StartInfo.FileName = "python";
            flaskProcess.StartInfo.Arguments = appPath;
            flaskProcess.StartInfo.WorkingDirectory = flaskPath; // Set working directory
            flaskProcess.StartInfo.UseShellExecute = false;
            flaskProcess.StartInfo.RedirectStandardOutput = true;
            flaskProcess.StartInfo.RedirectStandardError = true;
            flaskProcess.StartInfo.CreateNoWindow = true;
            
            flaskProcess.OutputDataReceived += (sender, args) => 
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Debug.Log("Flask Output: " + args.Data);
                }
            };
            
            flaskProcess.ErrorDataReceived += (sender, args) => 
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Debug.LogError("Flask Error: " + args.Data);
                }
            };
            
            bool started = flaskProcess.Start();
            
            if (started)
            {
                flaskProcess.BeginOutputReadLine();
                flaskProcess.BeginErrorReadLine();
                Debug.Log("Flask server starting at: " + serverIP);
                Debug.Log("Flask path: " + appPath);
            }
            else
            {
                Debug.LogError("Failed to start Flask process");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to start Flask server: " + e.Message);
            Debug.LogError("Stack trace: " + e.StackTrace);
        }
    }

    void OnApplicationQuit()
    {
        if (flaskProcess != null && !flaskProcess.HasExited)
        {
            flaskProcess.Kill();
            flaskProcess.Dispose();
            Debug.Log("Flask server stopped");
        }
    }
}
