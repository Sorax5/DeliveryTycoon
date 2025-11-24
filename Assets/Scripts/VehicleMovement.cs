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

    public Vector3Int currentPos;
    public Vector3Int nextPos;
    public List<Vector3Int> path;
    private SpriteRenderer visual;

    [SerializeField] private Vehicle vehicle;

    [SerializeField] private LineRenderer pathLineRenderer;
    [SerializeField] private bool showReturnPath = false;
    [SerializeField] private float pathYOffset = 0.5f;
    [SerializeField] private float pathLineWidth = 0.08f;
    [SerializeField] private Color pathColor = new Color(1f, 0.85f, 0.2f, 0.8f);

    private void Awake()
    {
        visual = GetComponentInChildren<SpriteRenderer>();
        if (visual != null) visual.enabled = false;
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
        pathLineRenderer.sortingOrder = 2;
    }

    // Appelé par VehicleManager après calcul du chemin (coroutines FIFO côté manager)
    public void StartAdventureWithComputedPath(Vector3Int start, List<Vector3Int> computedPath)
    {
        if (computedPath == null || computedPath.Count == 0)
        {
            Vehicle.IsAvailable = true;
            return;
        }
        Vehicle.IsAvailable = false;
        transform.position = WorldTilemap.CellToWorld(start) + new Vector3(0, pathYOffset, 0);
        path = computedPath;
        ShowPathLine(path);
        StartCoroutine(MoveAdventure());
    }

    private IEnumerator MoveAdventure()
    {
        OnAdventureStarted?.Invoke(Vehicle);
        if (visual != null) visual.enabled = true;
        yield return MoveAlongPath();

        OnAdventureEnded?.Invoke(Vehicle);
        OnAdventureBackUpStarted?.Invoke(Vehicle);

        path.Reverse();
        if (showReturnPath)
        {
            ShowPathLine(path);
        }
        else
        {
            ClearPathVisualization();
        }
        yield return MoveAlongPath();

        OnAdventureBackUpEnded?.Invoke(Vehicle);
        path = null;
        Vehicle.IsAvailable = true;
        if (visual != null) visual.enabled = false;
        ClearPathVisualization();
    }

    public IEnumerator MoveAlongPath()
    {
        currentPos = path[0];
        foreach (var point in path)
        {
            var floor = World.FloorMap[point];
            var speed = floor.Speed;
            var targetPosition = WorldTilemap.CellToWorld(point) + new Vector3(0, pathYOffset, 0);
            while ((transform.position - targetPosition).sqrMagnitude > 0.01f)
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
    }

    private void ShowPathLine(List<Vector3Int> pathToShow)
    {
        if (pathToShow == null || pathToShow.Count == 0) { ClearPathVisualization(); return; }
        EnsureLineRenderer();
        pathLineRenderer.enabled = true;
        pathLineRenderer.positionCount = pathToShow.Count;
        var worldPositions = new Vector3[pathToShow.Count];
        for (int i = 0; i < pathToShow.Count; i++)
            worldPositions[i] = WorldTilemap.CellToWorld(pathToShow[i]) + new Vector3(0, pathYOffset, 0);
        pathLineRenderer.SetPositions(worldPositions);
    }

    private void ClearPathVisualization()
    {
        if (pathLineRenderer == null) return;
        pathLineRenderer.positionCount = 0;
        pathLineRenderer.enabled = false;
    }
}
