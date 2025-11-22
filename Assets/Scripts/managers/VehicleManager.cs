using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VehicleManager : MonoBehaviour
{
    public event Action<Vehicle> OnVehicleAdded;

    [SerializeField] private WorldManager worldManager;
    [SerializeField] private List<VehicleDefinition> vehicles;

    public List<Vehicle> Vehicles { get; private set; } = new List<Vehicle>();
    public List<VehicleMovement> VehicleMovements { get; private set; } = new List<VehicleMovement>();

    public Vehicle CreateVehicle(int index)
    {
        var definition = vehicles[index];
        Vehicle vehicle = new Vehicle(definition);
        var movement = definition.CreateVehicle(worldManager.WorldTilemap, worldManager.World, vehicle);
        
        Vehicles.Add(vehicle);
        VehicleMovements.Add(movement);

        movement.OnAdventureStarted += onDeliveryStarted;
        movement.OnAdventureEnded += onDeliveryEnded;
        movement.OnAdventureBackUpStarted += onDeliveryBackupStarted;
        movement.OnAdventureBackUpEnded += onDeliveryBackupEnded;

        OnVehicleAdded?.Invoke(vehicle);

        return vehicle; 
    }

    public bool StartDelivery(Factory factory, Store store)
    {
        if (!HasAvailableVehicle())
        {
            return false;
        }
        var vehicle = GetAvailableVehicle();
        var movement = GetVehicleMovement(vehicle);

        movement.StartAdventure(factory.Position, store.Position);
        return true;
    }
    
    public bool HasAvailableVehicle()
    {
        foreach (var vehicle in Vehicles)
        {
            if (vehicle.IsAvailable)
            {
                return true;
            }
        }
        return false;
    }

    public Vehicle GetAvailableVehicle()
    {
        return Vehicles.FirstOrDefault(v => v.IsAvailable);
    }

    public VehicleMovement GetVehicleMovement(Vehicle vehicle)
    {
        return VehicleMovements.FirstOrDefault(vm => vm.Vehicle.UniqueId == vehicle.UniqueId);
    }

    private void onDeliveryStarted(Vehicle vehicle)
    {
    }

    private void onDeliveryEnded(Vehicle vehicle)
    {
    }

    private void onDeliveryBackupStarted(Vehicle vehicle)
    {

    }

    private void onDeliveryBackupEnded(Vehicle vehicle)
    {

    }


}
