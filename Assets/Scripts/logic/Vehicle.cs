using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class Vehicle
{
    public event Action OnAvailabilityChanged;
    public event Action OnSpeedMultiplierChanged;

    private bool isAvailable = true;
    private float speedMultiplier = 1.0f;

    public float SpeedMultiplier
    {
        get => speedMultiplier;
        set
        {
            if (Math.Abs(speedMultiplier - value) <= 0.01f)
            {
                return;
            }
            speedMultiplier = value;
            OnSpeedMultiplierChanged?.Invoke();
        }
    }
    public float Speed => speedMultiplier * Definition.speed;
    public Guid UniqueId { get; private set; }
    public bool IsAvailable 
    { 
        get => isAvailable; 
        set
        {
            isAvailable = value;
            OnAvailabilityChanged?.Invoke();
        }
    }
    public AlgorithmeEnum Algorithme { get; set; }

    public VehicleDefinition Definition { get; private set; }

    public Vehicle(VehicleDefinition definition)
    {
        this.Definition = definition;
        this.Algorithme = definition.algorithm;
        this.UniqueId = Guid.NewGuid();
    }
}