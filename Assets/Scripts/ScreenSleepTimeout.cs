using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSleepTimeout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
