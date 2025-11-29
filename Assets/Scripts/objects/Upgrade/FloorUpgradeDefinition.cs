using UnityEngine;

[CreateAssetMenu(fileName = "FloorUpgradeDefinition", menuName = "delivery/FloorUpgradeDefinition")]
public class FloorUpgradeDefinition : UpgradeDefinition
{
    public string FloorType;

    public override void ApplyUpgrade(Vehicle vehicle)
    {
        vehicle.ByPassFloor.Add(FloorType);
    }
}
