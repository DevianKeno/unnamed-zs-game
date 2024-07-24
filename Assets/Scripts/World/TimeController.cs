using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;


namespace UZSG.WorldBuilder
{
    public class TimeController : MonoBehaviour
{
    public Gradient DayColors;
    public Light Sun;
    public Light Moon;
    public float CurrentTime;
    public int CurrentDay;

    public int DayLength = 2160;

    public void Initialize()
    {
        Game.Tick.OnTick += OnTick;
        CurrentTime = 0;
        CurrentDay = 0;
    }

    private void OnTick(TickInfo info)
    {
        float tickThreshold = Game.Tick.TPS / 64f;

        CurrentTime += ((Game.Tick.SecondsPerTick * (Game.Tick.CurrentTick / 32f)) * tickThreshold);
        CalculateDay();
        UpdateCelestialBodies();
    }

    public void CalculateDay()
    {
        if (CurrentTime >= DayLength)
        {
            CurrentTime = 0;
            CurrentDay++;
        }
    }

    private void OnValidate()
    {
        if (CurrentTime > DayLength)
        {
            CurrentTime = 0;
            CurrentDay++;
        }

        UpdateCelestialBodies();
    }

    public void UpdateCelestialBodies()
    {
        float timeOfDay = CurrentTime / (float)DayLength;
        float sunAngle = timeOfDay * 360f;
        float moonAngle = (timeOfDay + 0.5f) % 1f * 360f;

        // Update Sun
        Vector3 sunRotation = new Vector3(sunAngle, 0, 0);
        Sun.transform.rotation = Quaternion.Euler(sunRotation);

        // Update Moon
        Vector3 moonRotation = new Vector3(moonAngle, 0, 0);
        Moon.transform.rotation = Quaternion.Euler(moonRotation);

        // Sample the color from the gradient for the Sun
        Color sunColor = DayColors.Evaluate(timeOfDay);
        Sun.color = sunColor;

        // Adjust Sun and Moon intensity based on their position
        Sun.intensity = Mathf.Max(0, Mathf.Sin(sunAngle * Mathf.Deg2Rad));
        Moon.intensity = Mathf.Max(0, Mathf.Sin(moonAngle * Mathf.Deg2Rad)) * 0.05f; // Moon is dimmer

        // Enable/disable Sun and Moon based on their position
        Sun.enabled = Sun.intensity > 0;
        Moon.enabled = Moon.intensity > 0;
    }
}

}