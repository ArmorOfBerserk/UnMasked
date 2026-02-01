using UnityEngine;

public class AutoScrollCamera : MonoBehaviour
{
    public static AutoScrollCamera Instance { get; private set; }

    [Header("Settings")]
    public float scrollSpeed = 2.8f; 
    public bool isScrolling = false;
    public Transform playerTransform; 
    
    [Header("Effetti")]
    // TRASCINA QUI IL TUO OGGETTO CON IL PARTICLE SYSTEM (Il DeathWall)
    public ParticleSystem deathParticles; 

    private Vector3 startPosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (playerTransform != null)
        {
            transform.position = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
        }

        startPosition = transform.position;
    }

    void OnEnable() => EventMessageManager.OnStartCameraScroll += ResumeScrolling;
    void OnDisable() => EventMessageManager.OnStartCameraScroll -= ResumeScrolling;

    void Update()
    {
        if (isScrolling)
        {
            transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);
        }
    }

    public void ResetCamera()
    {
        transform.position = startPosition;

        // --- PULIZIA PARTICELLE ---
        if (deathParticles != null)
        {
            // Clear() cancella tutte le particelle vive in questo istante
            deathParticles.Clear();
            
            // Assicuriamoci che continui a emetterne di nuove
            if (!deathParticles.isPlaying) deathParticles.Play();
        }
        // --------------------------
    }

    public void StopScrolling() => isScrolling = false;
    
    public void ResumeScrolling()
    {
        isScrolling = true;
    }
}