using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class SuccessRateMeasure : MonoBehaviour
{
    public int successCount = 0;
    public int failureCount = 0;
    public float successRate = 0f;
    // Start is called before the first frame update

    public int fellCount = 0;
    public int timeoutCount = 0;
    
    public TextMeshProUGUI text;

    public enum EpisodeResult
    {
        Success, Failed, Timedout
    }
    public void UpdateResults(EpisodeResult episodeResult)
    {
        switch (episodeResult)
        {
            case EpisodeResult.Success:
                successCount++;
                break;
            case EpisodeResult.Failed:
                fellCount++;
                failureCount++;
                break;
            case EpisodeResult.Timedout:
                timeoutCount++;
                failureCount++;
                break;
        }

        successRate = successCount / (float) (failureCount + successCount);
        text.text = successRate.ToString(CultureInfo.InvariantCulture) + " | " + (failureCount + successCount) + $" | Fell:{fellCount} - Timeout:{timeoutCount}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
