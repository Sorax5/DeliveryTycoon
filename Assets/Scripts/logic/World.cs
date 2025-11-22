using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class World
{
    private Dictionary<Vector3Int, Floor> floorMap;
    private readonly MapDefinition mapDefinition;

    public Dictionary<Vector3Int, Floor> FloorMap => floorMap;

    public World(MapDefinition mapDefinition)
    {
        this.mapDefinition = mapDefinition;
        this.floorMap = new Dictionary<Vector3Int, Floor>();
    }

    public void LoadPosition()
    {
        var width = mapDefinition.Width;
        var depth = mapDefinition.Height;

        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        FastNoiseLite noiseMaker = mapDefinition.NoiseDefinition.CreateNoiseMaker();
        noiseMaker.SetSeed(seed);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < depth; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                float noise = noiseMaker.GetNoise(x, y);

                FloorRange range = mapDefinition.GetFloorRangeByNoise(noise);
                if (range == null)
                {
                    Debug.Log($"Not any floor find for Noise value {noise}");
                    continue;
                }

                floorMap[position] = new Floor(position, range.FloorDefinition);
            }
        }
    }

    public Vector3Int GetRandomPosition()
    {
        var keys = new List<Vector3Int>(floorMap.Keys);
        int randomIndex = UnityEngine.Random.Range(0, keys.Count);
        return keys[randomIndex];
    }

    public Vector3Int GetRandomPositionAt(Vector3Int positon, int distance)
    {
        var position = positon;

        if (floorMap == null || floorMap.Count == 0)
        {
            return position;
        }
            

        if (distance <= 0)
        {
            return position;
        }

        int maxDistSq = distance * distance;
        var possiblePositions = new List<Vector3Int>();

        foreach (var kvp in floorMap)
        {
            var pos = kvp.Key;
            if (pos == position) continue;

            int dx = pos.x - position.x;
            int dy = pos.y - position.y;
            int distSq = dx * dx + dy * dy;

            if (distSq <= maxDistSq)
                possiblePositions.Add(pos);
        }

        if (possiblePositions.Count == 0)
        {
            Vector3Int nearest = position;
            int nearestSq = int.MaxValue;
            foreach (var kvp in floorMap)
            {
                var pos = kvp.Key;
                if (pos == position) continue;

                int dx = pos.x - position.x;
                int dy = pos.y - position.y;
                int dsq = dx * dx + dy * dy;
                if (dsq < nearestSq)
                {
                    nearestSq = dsq;
                    nearest = pos;
                }
            }

            return nearestSq == int.MaxValue ? position : nearest;
        }

        int randomIndex = UnityEngine.Random.Range(0, possiblePositions.Count);
        return possiblePositions[randomIndex];
    } 

    public List<Vector3Int> Dijkstra(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Dictionary<Vector3Int, float> distances = new Dictionary<Vector3Int, float>();
        Dictionary<Vector3Int, Vector3Int?> previous = new Dictionary<Vector3Int, Vector3Int?>();

        foreach (var (position, floor) in floorMap)
        {
            distances[position] = float.MaxValue;
            previous[position] = null;
        }

        distances[start] = 0;

        var unvisited = new HashSet<Vector3Int>(floorMap.Keys);

        while (unvisited.Count > 0)
        {
            var current = GetClosestNode(unvisited, distances);
            unvisited.Remove(current);

            List<Vector3Int> neighbors = GetNeighbors(current);
            foreach (var neighbor in neighbors)
            {
                var cost = distances[current] + floorMap[neighbor].Poids;
                if (cost < distances[neighbor])
                {
                    distances[neighbor] = cost;
                    previous[neighbor] = current;
                }
            }
        }

        Vector3Int? step = end;
        while (step != null)
        {
            path.Insert(0, step.Value);
            step = previous[step.Value];
        }
        return path;
    }

    public List<Vector3Int> AStar(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Dictionary<Vector3Int, float> distances = new Dictionary<Vector3Int, float>();
        Dictionary<Vector3Int, Vector3Int?> previous = new Dictionary<Vector3Int, Vector3Int?>();
        Dictionary<Vector3Int, float> fScore = new Dictionary<Vector3Int, float>();

        foreach (var (position, floor) in floorMap)
        {
            distances[position] = float.MaxValue;
            previous[position] = null;
            fScore[position] = float.MaxValue;
        }

        distances[start] = 0;
        fScore[start] = Heuristic(start, end);

        var openSet = new HashSet<Vector3Int> { start };
        while (openSet.Count > 0)
        {
            var current = GetClosestNode(openSet, fScore);
            if (current == end)
            {
                Vector3Int? step = end;
                while (step != null)
                {
                    path.Insert(0, step.Value);
                    step = previous[step.Value];
                }
                return path;
            }
            openSet.Remove(current);
            List<Vector3Int> neighbors = GetNeighbors(current);
            foreach (var neighbor in neighbors)
            {
                var tentativeGScore = distances[current] + floorMap[neighbor].Poids;
                if (tentativeGScore < distances[neighbor])
                {
                    previous[neighbor] = current;
                    distances[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);
                    openSet.Add(neighbor);
                }
            }
        }

        return path;
    }

    public IEnumerator DijkstraCoroutine(Vector3Int start, Vector3Int end, Action<List<Vector3Int>> onComplete, Func<bool> isCanceled = null, int yieldEvery = 100)
    {
        var nodes = new List<Vector3Int>(floorMap.Keys);
        var weights = new Dictionary<Vector3Int, float>(nodes.Count);
        foreach (var kv in floorMap)
        {
            weights[kv.Key] = kv.Value.Poids;
        }

        var path = new List<Vector3Int>();
        var distances = new Dictionary<Vector3Int, float>(nodes.Count);
        var previous = new Dictionary<Vector3Int, Vector3Int?>(nodes.Count);

        foreach (var position in nodes)
        {
            distances[position] = float.MaxValue;
            previous[position] = null;
        }

        if (!distances.ContainsKey(start) || !distances.ContainsKey(end))
        {
            onComplete?.Invoke(path);
            yield break;
        }

        distances[start] = 0;
        var unvisited = new HashSet<Vector3Int>(nodes);
        int iter = 0;

        while (unvisited.Count > 0)
        {
            if (isCanceled != null && isCanceled())
            {
                onComplete?.Invoke(new List<Vector3Int>()); yield break;
            }

            Vector3Int current = GetClosestNode(unvisited, distances);
            unvisited.Remove(current);

            if (current.Equals(end))
            {
                break;
            }

            var neighbors = GetNeighbors(current);

            foreach (var neighbor in neighbors)
            {
                if (!weights.ContainsKey(neighbor))
                {
                    continue;
                }

                var cost = distances[current] + weights[neighbor];
                if (cost < distances[neighbor])
                {
                    distances[neighbor] = cost;
                    previous[neighbor] = current;
                }
            }

            iter++;
            if (iter % yieldEvery == 0)
                yield return null;
        }

        if (!distances.ContainsKey(end) || distances[end] >= float.MaxValue)
        {
            onComplete?.Invoke(path);
            yield break;
        }

        Vector3Int? step = end;
        while (step.HasValue)
        {
            path.Insert(0, step.Value);
            step = previous[step.Value];

            if (path.Count % yieldEvery == 0)
            {
                yield return null;
            }
        }

        onComplete?.Invoke(path);
    }

    public IEnumerator AStarCoroutine(Vector3Int start, Vector3Int end, Action<List<Vector3Int>> onComplete, Func<bool> isCanceled = null, int yieldEvery = 100)
    {
        var nodes = new List<Vector3Int>(floorMap.Keys);
        var weights = new Dictionary<Vector3Int, float>(nodes.Count);
        foreach (var kv in floorMap)
        {
            weights[kv.Key] = kv.Value.Poids;
        }

        var path = new List<Vector3Int>();
        var distances = new Dictionary<Vector3Int, float>(nodes.Count);
        var previous = new Dictionary<Vector3Int, Vector3Int?>(nodes.Count);
        var fScore = new Dictionary<Vector3Int, float>(nodes.Count);

        foreach (var position in nodes)
        {
            distances[position] = float.MaxValue;
            previous[position] = null;
            fScore[position] = float.MaxValue;
        }

        if (!distances.ContainsKey(start) || !distances.ContainsKey(end))
        {
            onComplete?.Invoke(path);
            yield break;
        }

        distances[start] = 0;
        fScore[start] = Heuristic(start, end);

        var openSet = new HashSet<Vector3Int> { start };
        var iter = 0;

        while (openSet.Count > 0)
        {
            if (isCanceled != null && isCanceled())
            {
                onComplete?.Invoke(new List<Vector3Int>()); yield break;
            }

            Vector3Int current = GetClosestNode(openSet, fScore);
            if (current.Equals(end))
            {
                Vector3Int? step = end;
                while (step.HasValue)
                {
                    path.Insert(0, step.Value);
                    step = previous[step.Value];
                }
                onComplete?.Invoke(path);
                yield break;
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!weights.ContainsKey(neighbor))
                {
                    continue;
                }

                var tentativeGScore = distances[current] + weights[neighbor];

                if (tentativeGScore >= distances[neighbor])
                {
                    continue;
                }

                previous[neighbor] = current;
                distances[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);
                openSet.Add(neighbor);
            }

            iter++;
            if (iter % yieldEvery == 0)
                yield return null;
        }

        onComplete?.Invoke(path);
    }

    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector3Int GetClosestNode(HashSet<Vector3Int> unvisited, Dictionary<Vector3Int, float> distances)
    {
        Vector3Int closestNode = default;
        float closestDistance = float.MaxValue;
        foreach (var node in unvisited)
        {
            if (distances[node] < closestDistance)
            {
                closestDistance = distances[node];
                closestNode = node;
            }
        }
        return closestNode;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int node)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };
        foreach (var direction in directions)
        {
            Vector3Int neighbor = node + direction;
            if (floorMap.ContainsKey(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
}
