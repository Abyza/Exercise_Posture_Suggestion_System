using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class frameRateLimit : MonoBehaviour
{
    public int targetFrameRate = 60;
    // Start is called before the first frame update
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;  // Disable v-sync
        Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = 1f / targetFrameRate; // Adjust physics frame rate
    }




}
