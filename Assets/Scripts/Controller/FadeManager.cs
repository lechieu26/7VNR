using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    public GameObject loadingPrefab;
    private GameObject currentLoading;
    private CanvasGroup canvasGroup;

    private void Awake()
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

    public IEnumerator FadeOut(float time)
    {
        if (currentLoading == null)
        {
            currentLoading = Instantiate(loadingPrefab);
            canvasGroup = currentLoading.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentLoading.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / time);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    public IEnumerator FadeIn(float time)
    {
        if (currentLoading == null) yield break;

        float elapsedTime = 0f;
        
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / time);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(currentLoading);
        currentLoading = null;
    }
}