using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VehicleMovement : MonoBehaviour
{
    public World World { get; set; }
    public Tilemap WorldTilemap { get; set; }

    public Vehicle Vehicle
    {
        get => vehicle;
        set => vehicle = value;
    }

    public event Action<Vehicle> OnAdventureStarted;
    public event Action<Vehicle> OnAdventureEnded;
    public event Action<Vehicle> OnAdventureBackUpStarted;
    public event Action<Vehicle> OnAdventureBackUpEnded;

    private Vector3Int currentPos;
    private Vector3Int nextPos;
    private List<Vector3Int> path;
    private SpriteRenderer visual;

    [SerializeField] private Vehicle vehicle;

    private void Awake()
    {
        visual = GetComponentInChildren<SpriteRenderer>();
        visual.enabled = false;
    }

    public void StartAdventure(Vector3Int start, Vector3Int end)
    {
        Vehicle.IsAvailable = false;
        List<Vector3Int> path = new List<Vector3Int>();
        switch (Vehicle.Algorithme)
        {
            case AlgorithmeEnum.DIJKSTRA:
                path = World.Dijkstra(start, end);
                break;
            case AlgorithmeEnum.ASTAR:
                path = World.AStar(start, end);
                break;
        }

        transform.position = WorldTilemap.CellToWorld(start) + new Vector3(0, 0.5f, 0);
        this.path = path;
        StartCoroutine(MoveAdventure());
    }

    private IEnumerator MoveAdventure()
    {
        OnAdventureStarted?.Invoke(Vehicle);
        visual.enabled = true;
        yield return MoveAlongPath();

        OnAdventureEnded?.Invoke(Vehicle);
        OnAdventureBackUpStarted?.Invoke(Vehicle);

        this.path.Reverse();
        yield return MoveAlongPath();

        OnAdventureBackUpEnded?.Invoke(Vehicle);
        this.path = null;
        Vehicle.IsAvailable = true;
        visual.enabled = false;
        yield return null;
    }

    public IEnumerator MoveAlongPath()
    {
        currentPos = path[0];
        foreach (var point in path)
        {
            var floor = World.FloorMap[point];
            float speed = floor.Speed;
            Vector3 targetPosition = WorldTilemap.CellToWorld(point) + new Vector3(0, 0.5f, 0);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                nextPos = point;
                Vector3 direction = (targetPosition - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Vehicle.Speed * speed * Time.deltaTime);
                yield return null;
            }

            nextPos = point;
            currentPos = point;
            yield return null;
        }

        yield break;
    }

    private void OnDrawGizmos()
    {
        if (this.path == null)
        {
            return;
        }

        for (var i = 0; i < path.Count; i++)
        {
            var fp = path[i];
            var np = (i + 1 < path.Count) ? path[i + 1] : fp;
            Vector3 fromWorldPos = WorldTilemap.CellToWorld(fp) + new Vector3(0, 0.5f, 0);
            Vector3 toWorldPos = WorldTilemap.CellToWorld(np) + new Vector3(0, 0.5f, 0);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(fromWorldPos, toWorldPos);
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(fromWorldPos, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(toWorldPos, 0.05f);
        }

        Vector3 currentWorldPos = WorldTilemap.CellToWorld(currentPos) + new Vector3(0, 0.5f, 0);
        Vector3 nextWorldPos = WorldTilemap.CellToWorld(nextPos) + new Vector3(0, 0.5f, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentWorldPos, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(nextWorldPos, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(currentWorldPos, nextWorldPos);
    }
}
