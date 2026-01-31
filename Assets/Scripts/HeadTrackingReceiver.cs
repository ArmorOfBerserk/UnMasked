using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

public class HeadTrackingReceiver : MonoBehaviour
{
    [Header("Network")]
    public int port = 5065;

    [Header("Setup")]
    public Transform objectToMove;   // L'oggetto da muovere (es. Sfera)
    public Transform cameraTransform; // La Main Camera
    public float distanceFromCamera = 50f; // Distanza in profondità (asse Z locale)

    [Header("Configuration")]
    // L'offset statico dal centro dello schermo (se vuoi che di base stia spostato)
    public Vector2 baseOffset = new Vector2(8f, -5f);

    // Quanto il movimento della testa influenza lo spostamento (moltiplicatore)
    public Vector2 sensitivity = new Vector2(30f, 40f);

    [Header("Smoothing")]
    public float smoothTime = 0.1f; // Tempo per raggiungere la posizione (più basso = più reattivo)

    [Header("Invert Axis")]
    public bool invertX = false;
    public bool invertY = true; // True perché in CV la Y è invertita rispetto a Unity

    // Variabili interne
    private Thread receiveThread;
    private UdpClient client;
    private bool isRunning = true;

    private float rawX = 0f;
    private float rawY = 0f;
    private Vector3 currentVelocity; // Per SmoothDamp

    void Start()
    {
        if (objectToMove == null) objectToMove = this.transform;
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        // Avvio Thread UDP
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // 1. Calcola il punto centrale davanti alla camera
        Vector3 centerPoint = cameraTransform.position + (cameraTransform.forward * distanceFromCamera);

        // 2. Calcola l'input della testa processato
        float headX = rawX * sensitivity.x * (invertX ? -1 : 1);
        float headY = rawY * sensitivity.y * (invertY ? -1 : 1);

        // 3. Somma Offset Base (fisso) + Input Testa (dinamico)
        float finalOffsetX = baseOffset.x + headX;
        float finalOffsetY = baseOffset.y + headY;

        // 4. Calcola la posizione target nello spazio 3D usando i vettori locali della camera
        //    (Right = X locale, Up = Y locale)
        Vector3 targetPosition = centerPoint
                               + (cameraTransform.right * finalOffsetX)
                               + (cameraTransform.up * finalOffsetY);

        // 5. Applica movimento fluido
        objectToMove.position = Vector3.SmoothDamp(objectToMove.position, targetPosition, ref currentVelocity, smoothTime);

        // Opzionale: Ruota l'oggetto per guardare la camera (stile UI)
        objectToMove.LookAt(cameraTransform);
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

        while (isRunning)
        {
            try
            {
                // Ricezione dati (blocca finché non arriva pacchetto)
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data); // "x,y"

                string[] parts = text.Split(',');
                if (parts.Length == 2)
                {
                    // Parsing culture-invariant (per gestire il punto decimale)
                    rawX = float.Parse(parts[0], CultureInfo.InvariantCulture);
                    rawY = float.Parse(parts[1], CultureInfo.InvariantCulture);
                }
            }
            catch (System.Exception e)
            {
                // Ignora errori di timeout o chiusura socket
                Debug.LogWarning("UDP Receive Error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (client != null) client.Close();
        if (receiveThread != null) receiveThread.Abort();
    }
}