using UnityEngine;

public abstract class UpgradeDefinition : ScriptableObject
{
    public string Name;
    public string Description;

    public bool AvailableOnce = true;
    public int Cost;

    public abstract void ApplyUpgrade(Vehicle vehicle);
}
