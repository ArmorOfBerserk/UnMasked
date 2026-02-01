using UnityEngine;
using UnityEngine.Tilemaps;

// QUESTO SCRIPT VA MESSO SULL'OGGETTO CHE HA IL COMPONENTE TILEMAP
public class DisappearingTilemap : MonoBehaviour
{
    [Header("Effetti")]
    [SerializeField] private GameObject particleEffectPrefab; 

    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;
    private CompositeCollider2D compositeCollider; // A volte si usa questo

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
        compositeCollider = GetComponent<CompositeCollider2D>();
    }

    private void Start()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskChanged += UpdateVisibility;
            // Setta stato iniziale senza effetti
            SetState(MaskManager.Instance.IsMaskActive, false);
        }
    }

    private void OnDestroy()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskChanged -= UpdateVisibility;
        }
    }

    private void UpdateVisibility(bool isMaskActive)
    {
        // Se la maschera è attiva, NON deve esistere.
        // Se la maschera non è attiva, deve esistere.
        SetState(isMaskActive, true);
    }

    private void SetState(bool isMaskActive, bool spawnEffects)
    {
        bool shouldExist = !isMaskActive;

        // 1. Gestione Collider (fisica)
        if (tilemapCollider != null) tilemapCollider.enabled = shouldExist;
        if (compositeCollider != null) compositeCollider.enabled = shouldExist;

        // 2. Gestione Renderer (visibilità)
        if (tilemapRenderer != null) tilemapRenderer.enabled = shouldExist;

        // 3. Spawn Effetti su OGNI mattonella
        if (spawnEffects && particleEffectPrefab != null && tilemap != null)
        {
            SpawnParticlesOnEveryTile();
        }
    }

    private void SpawnParticlesOnEveryTile()
    {
        // Itera attraverso i limiti della tilemap per trovare dove ci sono mattonelle
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    // Abbiamo trovato una mattonella!
                    // Calcoliamo la sua posizione nel mondo reale
                    Vector3Int cellPosition = new Vector3Int(bounds.xMin + x, bounds.yMin + y, bounds.z);
                    Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);

                    // Istanzia l'effetto al centro di QUESTA specifica mattonella
                    GameObject p = Instantiate(particleEffectPrefab, worldPosition, Quaternion.identity);
                    Destroy(p, 2f); // Pulizia
                }
            }
        }
    }
}