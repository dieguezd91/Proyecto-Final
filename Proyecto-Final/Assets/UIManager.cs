using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI wavesText;
    [SerializeField] TextMeshProUGUI enemiesRemainingText;
    private WaveSpawner waveSpawner;

    void Start()
    {
        waveSpawner = FindObjectOfType<WaveSpawner>();
    }

    void Update()
    {
        if (waveSpawner != null)
        {
            wavesText.text = "OLEADA: " + waveSpawner.GetCurrentWaveIndex().ToString() + " / " + waveSpawner.totalWaves.ToString();
            enemiesRemainingText.text = "ENEMIGOS RESTANTES: " + waveSpawner.GetRemainingEnemies().ToString() + " / " + waveSpawner.GetEnemiesPerWave().ToString();
        }
    }
}
