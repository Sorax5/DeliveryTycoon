using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private List<UpgradeDefinition> upgrades;

    public Dictionary<Guid, List<Upgrade>> UpgradeByVehicle = new Dictionary<Guid, List<Upgrade>>();

    public List<Upgrade> GetUpgradesForVehicle(Vehicle vehicle)
    {
        if (UpgradeByVehicle.TryGetValue(vehicle.UniqueId, out var upgradesList))
        {
            return upgradesList;
        }

        upgradesList = new List<Upgrade>();
        foreach (var upgrade in upgrades)
        {
            var upgradeInstance = new Upgrade(upgrade);
            upgradesList.Add(upgradeInstance);
        }

        UpgradeByVehicle[vehicle.UniqueId] = upgradesList;
        return upgradesList;
    }
}
