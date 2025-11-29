using System;
using System.Collections;
using System.Collections.Generic;
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

        var seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        var noiseMaker = mapDefinition.NoiseDefinition.CreateNoiseMaker();
        noiseMaker.SetSeed(seed);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < depth; y++)
            {
                var position = new Vector3Int(x, y, 0);
                var noise = noiseMaker.GetNoise(x, y);

                var range = mapDefinition.GetFloorRangeByNoise(noise);
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
        var randomIndex = UnityEngine.Random.Range(0, keys.Count);
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

        var maxDistSq = distance * distance;
        var possiblePositions = new List<Vector3Int>();

        foreach (var kvp in floorMap)
        {
            var pos = kvp.Key;
            if (pos == position)
            {
                continue;
            }

            var dx = pos.x - position.x;
            var dy = pos.y - position.y;
            var distSq = dx * dx + dy * dy;

            if (distSq <= maxDistSq)
            {
                possiblePositions.Add(pos);
            }
        }

        if (possiblePositions.Count == 0)
        {
            var nearest = position;
            var nearestSq = int.MaxValue;
            foreach (var kvp in floorMap)
            {
                var pos = kvp.Key;
                if (pos == position)
                {
                    continue;
                }

                var dx = pos.x - position.x;
                var dy = pos.y - position.y;
                var dsq = dx * dx + dy * dy;

                if (dsq < nearestSq)
                {
                    nearestSq = dsq;
                    nearest = pos;
                }
            }

            return nearestSq == int.MaxValue ? position : nearest;
        }

        var randomIndex = UnityEngine.Random.Range(0, possiblePositions.Count);
        return possiblePositions[randomIndex];
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
        var iter = 0;

        while (unvisited.Count > 0)
        {
            if (isCanceled != null && isCanceled())
            {
                onComplete?.Invoke(new List<Vector3Int>()); 
                yield break;
            }

            var current = getClosestNode(unvisited, distances);
            unvisited.Remove(current);

            if (current.Equals(end))
            {
                break;
            }

            var neighbors = GetNeighbors(current);

            foreach (var neighbor in neighbors)
            {
                if (weights.TryGetValue(neighbor, out var weight))
                {
                    var cost = distances[current] + weights[neighbor];
                    if (cost >= distances[neighbor])
                    {
                        continue;
                    }

                    distances[neighbor] = cost;
                    previous[neighbor] = current;
                }
                
            }

            iter++;
            if (iter % yieldEvery == 0)
            {
                yield return null;
            }
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
        fScore[start] = heuristic(start, end);

        var openSet = new HashSet<Vector3Int> { start };
        var iter = 0;

        while (openSet.Count > 0)
        {
            if (isCanceled != null && isCanceled())
            {
                onComplete?.Invoke(new List<Vector3Int>()); yield break;
            }

            var current = getClosestNode(openSet, fScore);
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
                if (weights.TryGetValue(neighbor, out var weight))
                {
                    var tentativeGScore = distances[current] + weight;

                    if (tentativeGScore >= distances[neighbor])
                    {
                        continue;
                    }

                    previous[neighbor] = current;
                    distances[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + heuristic(neighbor, end);
                    openSet.Add(neighbor);
                }

            }

            iter++;
            if (iter % yieldEvery == 0)
            {
                yield return null;
            }
        }

        onComplete?.Invoke(path);
    }

    private float heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector3Int getClosestNode(HashSet<Vector3Int> unvisited, Dictionary<Vector3Int, float> distances)
    {
        Vector3Int closestNode = default;
        var closestDistance = float.MaxValue;
        foreach (var node in unvisited)
        {
            if (distances[node] >= closestDistance)
            {
                continue;
            }
            closestDistance = distances[node];
            closestNode = node;
        }
        return closestNode;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int node)
    {
        var neighbors = new List<Vector3Int>();
        var directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (var direction in directions)
        {
            var neighbor = node + direction;
            if (!floorMap.ContainsKey(neighbor))
            {
                continue;
            }
            neighbors.Add(neighbor);
        }
        return neighbors;
    }
}
