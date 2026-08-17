using System;
using UnityEngine;

public class GameClock : MonoBehaviour
{
    [Header("Data de início da simulação")]
    public int startYear = 1989;
    public int startMonth = 8;
    public int startDay = 25;

    [Header("Controle de tempo")]
    [Range(1f, 100f)] public float timeScale = 1f; // intensidade (sempre positiva)
    public bool isPaused = false;
    public int direction = 1; // 1 = avançando, -1 = retrocedendo

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
        if (isPaused) return;

        secondsElapsedSimTime += Time.deltaTime * timeScale * direction;
        CurrentDate = simulationStart.AddSeconds(secondsElapsedSimTime);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetDirectionForward()
    {
        direction = 1;
        isPaused = false;
    }

    public void SetDirectionBackward()
    {
        direction = -1;
        isPaused = false;
    }

    public void SetSpeed(float value)
    {
        timeScale = Mathf.Clamp(value, 1f, 100f);
    }
}