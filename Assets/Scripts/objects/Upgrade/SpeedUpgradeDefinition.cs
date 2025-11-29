using UnityEngine;

[CreateAssetMenu(fileName = "SpeedUpgradeDefinition", menuName = "delivery/SpeedUpgradeDefinition")]
public class SpeedUpgradeDefinition : UpgradeDefinition
{
    public float SpeedByBuy = 0.2f;

    public override void ApplyUpgrade(Vehicle vehicle)
    {
        vehicle.SpeedMultiplier += SpeedByBuy;
    }
}
