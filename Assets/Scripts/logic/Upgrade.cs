using System;
using System.Collections.Generic;
using System.Text;

public class Upgrade
{
    public bool IsAlreadyUnlock { get; private set; } = false;
    public UpgradeDefinition Definition { get; private set; }

    public Upgrade(UpgradeDefinition definition)
    {
        this.Definition = definition;
    }

    public void ApplyUpgrade(Vehicle vehicle)
    {
        if (IsAlreadyUnlock)
        {
            return;
        }
        Definition.ApplyUpgrade(vehicle);
        IsAlreadyUnlock = true;
    }
}