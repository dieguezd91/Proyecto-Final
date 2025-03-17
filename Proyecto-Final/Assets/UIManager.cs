using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI wavesText;
    private WaveSpawner waveSpawner;

    void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
    }

    void Update()
    {
        if (waveSpawner != null)
        {
            wavesText.text = "OLEADA: " + WaveSpawner.currentWave.ToString() + " / " + waveSpawner.totalWaves.ToString();
        }
    }
}
