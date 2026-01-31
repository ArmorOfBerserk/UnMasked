using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics; // Necessario per avviare .exe
using System.IO;          // Necessario per i percorsi file

public class HeadTrackingReceiver : MonoBehaviour
{
    [Header("Executable Configuration")]
    public string executableName = "tracker.exe"; // Il nome del tuo file exe
    public bool startServerOnPlay = true;         // Vuoi che Unity avvii il server?
    public bool showConsoleWindow = false;        // Se true, mostra la finestra nera (solo se compilato senza --noconsole)

    [Header("Network")]
    public int port = 5065;

    [Header("Setup")]
    public Transform objectToMove;
    public Transform cameraTransform;
    public float distanceFromCamera = 50f;

    [Header("Configuration")]
    public Vector2 baseOffset = new Vector2(8f, -5f);
    public Vector2 sensitivity = new Vector2(30f, 40f);

    [Header("Smoothing")]
    public float smoothTime = 0.1f;

    [Header("Invert Axis")]
    public bool invertX = false;
    public bool invertY = true;

    // --- Variabili interne ---
    private Thread receiveThread;
    private UdpClient client;
    private bool isRunning = true;
    private Process serverProcess; // Riferimento al processo esterno

    private float rawX = 0f;
    private float rawY = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        if (objectToMove == null) objectToMove = this.transform;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        // 1. Avvia il server Python (exe)
        if (startServerOnPlay)
        {
            StartExternalServer();
        }

        // 2. Avvia Thread UDP
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        Vector3 centerPoint = cameraTransform.position + (cameraTransform.forward * distanceFromCamera);

        float headX = rawX * sensitivity.x * (invertX ? -1 : 1);
        float headY = rawY * sensitivity.y * (invertY ? -1 : 1);

        float finalOffsetX = baseOffset.x + headX;
        float finalOffsetY = baseOffset.y + headY;

        Vector3 targetPosition = centerPoint
                               + (cameraTransform.right * finalOffsetX)
                               + (cameraTransform.up * finalOffsetY);

        objectToMove.position = Vector3.SmoothDamp(objectToMove.position, targetPosition, ref currentVelocity, smoothTime);
        objectToMove.LookAt(cameraTransform);
    }

    // --- GESTIONE PROCESSO ESTERNO ---
    void StartExternalServer()
    {
        // Calcola il percorso in base a se siamo nell'Editor o nella Build
        string basePath = Application.streamingAssetsPath;
        string fullPath = Path.Combine(basePath, executableName);

        if (!File.Exists(fullPath))
        {
            UnityEngine.Debug.LogError("Server EXE non trovato in: " + fullPath);
            return;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fullPath);
            
            // Impostazioni per nascondere o mostrare la finestra
            startInfo.CreateNoWindow = !showConsoleWindow;
            startInfo.UseShellExecute = showConsoleWindow; 
            startInfo.WindowStyle = showConsoleWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;

            serverProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log("Server Python avviato: " + fullPath);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Impossibile avviare il server: " + e.Message);
        }
    }

    void KillExternalServer()
    {
        if (serverProcess != null && !serverProcess.HasExited)
        {
            try
            {
                serverProcess.Kill();
                serverProcess = null;
                UnityEngine.Debug.Log("Server Python terminato.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("Errore chiusura server: " + e.Message);
            }
        }
    }
    // ---------------------------------

    private void ReceiveData()
    {
        try 
        {
            client = new UdpClient(port);
            // Timeout corto per non bloccare troppo alla chiusura
            client.Client.ReceiveTimeout = 1000; 
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] data = client.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);

                    string[] parts = text.Split(',');
                    if (parts.Length == 2)
                    {
                        rawX = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        rawY = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    }
                }
                catch (SocketException) 
                { 
                    // Timeout, continua il loop
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning("UDP Error: " + e.Message);
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Errore avvio UDP: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        
        // Chiudi UDP
        if (client != null) client.Close();
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Abort();

        // Uccidi il processo Python
        if (startServerOnPlay)
        {
            KillExternalServer();
        }
    }
}