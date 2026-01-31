using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    
    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void TransitionToLevel(string levelName)
    {
        StartCoroutine(TransitionRoutine(levelName));
    }
    
    IEnumerator TransitionRoutine(string levelName)
    {
        // Fade OUT (schermo diventa nero)
        yield return StartCoroutine(Fade(0f, 1f));
        
        // Carica scena
        LevelsManager.Instance.NextLevel();
        
        // Fade IN (schermo torna visibile)
        yield return StartCoroutine(Fade(1f, 0f));
    }
    
    IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, end);
    }
}