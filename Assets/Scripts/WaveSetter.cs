using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveSetter : MonoBehaviour
{
    public TextMeshProUGUI waveText;

    private void Start()
    {
        if (waveText) waveText.SetText("Current Wave: " + PlayerPrefs.GetInt("currentWave") + 
            "\n Highest Wave: " + PlayerPrefs.GetInt("highestWave"));
    }
}
