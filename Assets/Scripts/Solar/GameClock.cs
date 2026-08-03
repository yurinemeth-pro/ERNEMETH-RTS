using System;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [Header("Data de início da simulação")]
    public int startYear = 1989;
    public int startMonth = 8;
    public int startDay = 25; // sobrevoo real da Voyager 2 por Netuno

    [Header("Time Warp")]
    public float timeScale = 1f; // 1 = tempo real, 1000 = 1000x mais rápido, etc.

    public DateTime CurrentDate { get; private set; }

    private DateTime simulationStart;
    private double secondsElapsedSimTime;

    void Awake()
    {
        simulationStart = new DateTime(startYear, startMonth, startDay);
        CurrentDate = simulationStart;
    }

    void Update()
    {
        secondsElapsedSimTime += Time.deltaTime * timeScale;
        CurrentDate = simulationStart.AddSeconds(secondsElapsedSimTime);

        Debug.Log(CurrentDate.ToString("dd/MM/yyyy HH:mm:ss"));
    }

    public void SetTimeScale(float newScale)
    {
        timeScale = newScale;
    }
}