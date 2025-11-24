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

    // Remplacement des multiples prefabs par un seul LineRenderer pour réduire le lag
    [SerializeField] private LineRenderer pathLineRenderer; // Assigner ou laisser null pour création auto
    [SerializeField] private bool showReturnPath = false;
    [SerializeField] private float pathYOffset = 0.5f; // Décalage vertical
    [SerializeField] private float pathLineWidth = 0.08f;
    [SerializeField] private Color pathColor = new Color(1f, 0.85f, 0.2f, 0.8f);

    private void Awake()
    {
        visual = GetComponentInChildren<SpriteRenderer>();
        visual.enabled = false;
        EnsureLineRenderer();
    }

    private void EnsureLineRenderer()
    {
        if (pathLineRenderer != null) return;
        pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        pathLineRenderer.useWorldSpace = true;
        pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathLineRenderer.widthMultiplier = pathLineWidth;
        pathLineRenderer.positionCount = 0;
        pathLineRenderer.startColor = pathColor;
        pathLineRenderer.endColor = pathColor;
        pathLineRenderer.sortingOrder = 10; // Devant le sol
    }

    public void StartAdventure(Vector3Int start, Vector3Int end)
    {
        pathCts?.Cancel();
        pathCts = new CancellationTokenSource();
        ClearPathVisualization();

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

        transform.position = WorldTilemap.CellToWorld(start) + new Vector3(0, pathYOffset, 0);
        this.path = computedPath;
        ShowPathLine(this.path);
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
        if (showReturnPath)
        {
            ShowPathLine(this.path); // Met à jour pour le retour
        }
        else
        {
            ClearPathVisualization();
        }
        yield return MoveAlongPath();

        OnAdventureBackUpEnded?.Invoke(Vehicle);
        this.path = null;
        Vehicle.IsAvailable = true;
        visual.enabled = false;
        ClearPathVisualization();
        yield return null;
    }

    public IEnumerator MoveAlongPath()
    {
        currentPos = path[0];
        foreach (var point in path)
        {
            var floor = World.FloorMap[point];
            var speed = floor.Speed;
            var targetPosition = WorldTilemap.CellToWorld(point) + new Vector3(0, pathYOffset, 0);
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                nextPos = point;
                var direction = (targetPosition - transform.position).normalized;
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
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

    // Affiche le chemin via LineRenderer (un seul draw call)
    private void ShowPathLine(List<Vector3Int> pathToShow)
    {
        if (pathToShow == null || pathToShow.Count == 0) { ClearPathVisualization(); return; }
        EnsureLineRenderer();
        pathLineRenderer.enabled = true;
        pathLineRenderer.positionCount = pathToShow.Count;
        for (int i = 0; i < pathToShow.Count; i++)
        {
            var cell = pathToShow[i];
            pathLineRenderer.SetPosition(i, WorldTilemap.CellToWorld(cell) + new Vector3(0, pathYOffset, 0));
        }
    }

    private void ClearPathVisualization()
    {
        if (pathLineRenderer == null) return;
        pathLineRenderer.positionCount = 0;
        pathLineRenderer.enabled = false;
    }
}
