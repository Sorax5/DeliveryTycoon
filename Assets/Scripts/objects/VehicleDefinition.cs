using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "VehicleDefinition", menuName = "delivery/VehicleDefinition")]
public class VehicleDefinition : ScriptableObject
{
    public Texture2D Sprite;
    public string Name;
    public AlgorithmeEnum algorithm;
    public GameObject vehiclePrefab;
    public float speed = 1.0f;
    public VehicleMovement CreateVehicle(Tilemap tilemap, World world, Vehicle vehicle)
    {
        GameObject vehicleObject = Instantiate(vehiclePrefab);
        var carMovement = vehicleObject.GetComponent<VehicleMovement>();
        carMovement.World = world;
        carMovement.WorldTilemap = tilemap;
        carMovement.Vehicle = vehicle;
        return carMovement;
    }
}
