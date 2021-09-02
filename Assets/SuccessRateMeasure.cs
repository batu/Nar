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

    public TextMeshProUGUI text;
    public void UpdateResults(bool success)
    {
        if (success)
        {
            successCount++;
        }
        else
        {
            failureCount++;
        }

        successRate = successCount / (float) (failureCount + successCount);
        text.text = successRate.ToString(CultureInfo.InvariantCulture) + " | " + (failureCount + successCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
