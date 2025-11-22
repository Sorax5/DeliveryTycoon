using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    public Vector3Int currentPos;
    public Vector3Int nextPos;
    public List<Vector3Int> path;
    private SpriteRenderer visual;

    private CancellationTokenSource pathCts;

    [SerializeField] private Vehicle vehicle;

    private void Awake()
    {
        visual = GetComponentInChildren<SpriteRenderer>();
        visual.enabled = false;
    }

    public void StartAdventure(Vector3Int start, Vector3Int end)
    {
        pathCts?.Cancel();
        pathCts = new CancellationTokenSource();

        Vehicle.IsAvailable = false;

        bool useDijkstra = Vehicle.Algorithme == AlgorithmeEnum.DIJKSTRA;
        StartCoroutine(ComputePathAndStartCoroutine(start, end, useDijkstra, pathCts.Token));
    }

    private IEnumerator ComputePathAndStartCoroutine(Vector3Int start, Vector3Int end, bool useDijkstra, CancellationToken cancellationToken)
    {
        List<Vector3Int> computedPath = null;

        if (useDijkstra)
        {
            yield return StartCoroutine(World.DijkstraCoroutine(start, end, result => computedPath = result, () => cancellationToken.IsCancellationRequested));
        }
        else
        {
            yield return StartCoroutine(World.AStarCoroutine(start, end, result => computedPath = result, () => cancellationToken.IsCancellationRequested));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            Vehicle.IsAvailable = true;
            yield break;
        }

        if (computedPath == null || computedPath.Count == 0)
        {
            Vehicle.IsAvailable = true;
            yield break;
        }

        transform.position = WorldTilemap.CellToWorld(start) + new Vector3(0, 0.5f, 0);
        this.path = computedPath;
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
}
