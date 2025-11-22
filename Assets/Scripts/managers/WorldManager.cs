using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [Header("Parameters")] 
    [SerializeField] private MapDefinition mapDefinition;
    [SerializeField] private List<VehicleDefinition> vehicles;

    [Header("References")]
    [SerializeField] private PolygonCollider2D playerBoundsCollider;
    [SerializeField] private GameObject player;

    [Header("Tilemaps")] 
    [SerializeField] private Tilemap worldTilemap;

    public World World { get; private set; }
    public Tilemap WorldTilemap => worldTilemap;

    private void Start()
    {
        this.World = new World(mapDefinition);
        this.World.LoadPosition();

        foreach (var (position, floor) in this.World.FloorMap)
        {
            worldTilemap.SetTile(position, floor.Definition.Tile);
        }

        int width = mapDefinition.Width / 2;
        int depth = mapDefinition.Height / 2;

        Vector3 bottomLeft = worldTilemap.CellToWorld(new Vector3Int(-width, -depth, 0));
        Vector3 topLeft = worldTilemap.CellToWorld(new Vector3Int(-width, depth, 0));
        Vector3 topRight = worldTilemap.CellToWorld(new Vector3Int(width, depth, 0));
        Vector3 bottomRight = worldTilemap.CellToWorld(new Vector3Int(width, -depth, 0));

        Vector3 center = (bottomLeft + topLeft + topRight + bottomRight) * 0.25f;

        playerBoundsCollider.points = new Vector2[]
        {
            new Vector2(bottomLeft.x - center.x, bottomLeft.y - center.y),
            new Vector2(topLeft.x - center.x, topLeft.y - center.y),
            new Vector2(topRight.x - center.x, topRight.y - center.y),
            new Vector2(bottomRight.x - center.x, bottomRight.y - center.y)
        };

        playerBoundsCollider.transform.position = new Vector2(center.x - bottomLeft.x, center.y - bottomLeft.y);
        player.transform.position = new Vector3(0, depth, 0) + worldTilemap.cellSize * 0.5f;
    }

    public Vector3 CellToWorld(Vector3Int cellPosition)
    {
        return worldTilemap.CellToWorld(cellPosition);
    }
}