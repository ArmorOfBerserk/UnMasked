using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using UnityEngine.InputSystem;

public class HeadTrackingReceiver : MonoBehaviour
{
    // --- SINGLETON (Nuovo) ---
    public static HeadTrackingReceiver Instance { get; private set; }

    [Header("Executable Configuration")]
    public string executableName = "eyetracking.exe";
    public bool startServerOnPlay = true;
    public bool showConsoleWindow = false;

    [Header("Network")]
    public int port = 5065;

    [Header("Setup")]
    public Transform objectToMove;
    public Transform cameraTransform;
    public float distanceFromCamera = 50f; 

    [Header("Calibration")]
    public Vector2 screenCenterOffset = new Vector2(8f, 0f);
    public Key recenterKey = Key.C; 
    
    private float calibrationOffsetX = 0f;
    private float calibrationOffsetY = 0f;

    [Header("Movement Configuration")]
    public Vector2 sensitivity = new Vector2(60f, 60f); 
    public Vector2 movementLimits = new Vector2(8.5f, 8f); 

    [Header("Smoothing")]
    public float smoothTime = 0.05f; 

    [Header("Invert Axis")]
    public bool invertX = false; 
    public bool invertY = true;

    // --- Variabili interne ---
    private Thread receiveThread;
    private UdpClient client;
    private bool isRunning = true;
    private Process serverProcess;

    private float rawX = 0f;
    private float rawY = 0f;
    private Vector3 currentVelocity;

    void Awake()
    {
        // Setup Singleton
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (objectToMove == null) objectToMove = this.transform;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        if (startServerOnPlay) StartExternalServer();

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if (Keyboard.current[recenterKey].wasPressedThisFrame) CalibrateCenter();
        if (cameraTransform == null) return;

        // Calcolo posizione target (uguale a prima)
        Vector3 targetPosition = CalculateTargetPosition();

        // Movimento fluido
        objectToMove.position = Vector3.SmoothDamp(objectToMove.position, targetPosition, ref currentVelocity, smoothTime);
    }

    // Ho estratto il calcolo in una funzione per usarlo anche nel Reset
    private Vector3 CalculateTargetPosition()
    {
        Vector3 cameraPlanePos = cameraTransform.position + (cameraTransform.forward * distanceFromCamera);
        
        Vector3 neutralPosition = cameraPlanePos 
                                + (cameraTransform.right * screenCenterOffset.x) 
                                + (cameraTransform.up * screenCenterOffset.y);

        float calibratedRawX = rawX - calibrationOffsetX;
        float calibratedRawY = rawY - calibrationOffsetY;

        float inputX = calibratedRawX * sensitivity.x * (invertX ? -1 : 1);
        float inputY = calibratedRawY * sensitivity.y * (invertY ? -1 : 1);

        float clampedX = Mathf.Clamp(inputX, -movementLimits.x, movementLimits.x);
        float clampedY = Mathf.Clamp(inputY, -movementLimits.y, movementLimits.y);

        return neutralPosition 
               + (cameraTransform.right * clampedX) 
               + (cameraTransform.up * clampedY);
    }

    // --- NUOVA FUNZIONE: RESET IMMEDIATO ---
    public void ForceResetPosition()
    {
        if (cameraTransform == null || objectToMove == null) return;

        // 1. Calcola dove dovrebbe essere ORA
        Vector3 targetPos = CalculateTargetPosition();

        // 2. Teletrasporta l'oggetto lì istantaneamente
        objectToMove.position = targetPos;

        // 3. Azzera la velocità del SmoothDamp (altrimenti continua a spingere)
        currentVelocity = Vector3.zero;
    }

    public void CalibrateCenter()
    {
        calibrationOffsetX = rawX;
        calibrationOffsetY = rawY;
        UnityEngine.Debug.Log($"Calibrazione effettuata!");
    }

    // --- (Il resto delle funzioni UDP/Server rimane identico a prima) ---
    void StartExternalServer()
    {
        string basePath = Application.streamingAssetsPath;
        string fullPath = Path.Combine(basePath, executableName);
        if (!File.Exists(fullPath)) return;
        try {
            ProcessStartInfo startInfo = new ProcessStartInfo(fullPath);
            startInfo.CreateNoWindow = !showConsoleWindow;
            startInfo.UseShellExecute = showConsoleWindow; 
            startInfo.WindowStyle = showConsoleWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            serverProcess = Process.Start(startInfo);
        } catch {}
    }

    void KillExternalServer() { if (serverProcess != null && !serverProcess.HasExited) try { serverProcess.Kill(); } catch {} }

    private void ReceiveData() {
        try {
            client = new UdpClient(port); client.Client.ReceiveTimeout = 1000; IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            while (isRunning) { try {
                    byte[] data = client.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    string[] parts = text.Split(',');
                    if (parts.Length == 2) {
                        rawX = float.Parse(parts[0], CultureInfo.InvariantCulture);
                        rawY = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    }
            } catch (SocketException) {} }
        } catch {}
    }

    void OnApplicationQuit() {
        isRunning = false; if (client != null) client.Close(); if (receiveThread != null) receiveThread.Abort(); if (startServerOnPlay) KillExternalServer();
    }
}