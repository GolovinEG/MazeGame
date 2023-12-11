using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Maze : MonoBehaviour
{
    private Vector3 bottomLeft = new Vector3(-12.5f, -7, 6);
    public Vector2Int size = new Vector2Int(18, 12);
    public float stepSize = 1.5f;
    public float generationDelay = 0.1f;
    public float baseChance = 40;
    public float chanceStep = 20;
    public Tile tilePrefab;
    public EdgePassage passagePrefab;
    public EdgeWall wallPrefab;
    public Exit exitPrefab;
    public Key[] keyPrefabs = new Key[3];
    public EdgeDoor[] doorPrefabs = new EdgeDoor[3];
    public PlayerManager playerPrefab;
    public TMP_Text victoryText { get; set; }
    private Tile[,] tiles;
    public Tile start { get; set; }
    private List<Tile> deadends = new List<Tile>();
    private List<Tile>[] paths = new List<Tile>[4];
    private int goalCount = 0;
    private Tile[] goals = new Tile[4];
    private List<Tile> blockers = new List<Tile>();
    private MazeDirection[] blockerDirection = new MazeDirection[3];

    public Vector2Int RandomCoordinates
    {
        get
        {
            return new Vector2Int(Random.Range(0, size.x), Random.Range(0, size.y));
        }
    }

    public Tile GetTile(Vector2Int coordinates)
    {
        return tiles[coordinates.x,coordinates.y];
    }

    public IEnumerator Generate()
    {
        WaitForSeconds delay = new WaitForSeconds(generationDelay);
        tiles = new Tile[size.x, size.y];
        List<Tile> activeTiles = new List<Tile>();
        GenerationStep(activeTiles);
        while (activeTiles.Count > 0)
        {
            yield return delay;
            GenerationStep(activeTiles);
        }
        for (int i = 0; i < 4; i++)
            paths[i] = new List<Tile>();
        while (deadends.Count > 0 && goalCount < 4)
            GenerateGoal(deadends[Random.Range(0, deadends.Count)]);
        for (int i = 0; i < goalCount; i++)
            AddGoal(i);
        PlayerManager player = Instantiate<PlayerManager>(playerPrefab);
        player.transform.parent = transform;
        player.transform.position = start.transform.position;
        player.maze = this;
        player.tile = start;
    }

    private void GenerationStep(List<Tile> activeTiles)
    {
        if (activeTiles.Count == 0)
        {
            activeTiles.Add(SpawnTile(RandomCoordinates));
            start = activeTiles[0];
        }
        else
        {
            int currentIndex = activeTiles.Count - 1;
            Tile currentTile = activeTiles[currentIndex];
            if (currentTile.IsFullyInitialized)
            {
                if (currentTile.PassageCount == 1 && !currentTile.Equals(start)) 
                    deadends.Add(currentTile);
                activeTiles.RemoveAt(currentIndex);
                return;
            }
            MazeDirection direction = currentTile.RandomUninitializedDirection;
            Vector2Int coordinates = currentTile.coordinates + direction.ToVector();
            if (ContainsCoordinates(coordinates))
            {
                Tile tileTarget = GetTile(coordinates);
                if (tileTarget == null)
                {
                    tileTarget = SpawnTile(coordinates, currentTile.stepsFromStart);
                    SpawnPassage(currentTile, tileTarget, direction);
                    activeTiles.Add(tileTarget);
                }
                else
                    SpawnWall(currentTile, tileTarget, direction);
            }
            else
                SpawnWall(currentTile, null, direction);
        }
    }

    private void GenerateGoal(Tile firstTile)
    {
        Tile currentTile = firstTile;
        Vector2Int coordinates;
        MazeDirection direction;
        while (!currentTile.Equals(start))
        {
            paths[goalCount].Add(currentTile);
            direction = FindCloserDirection(currentTile);
            coordinates = currentTile.coordinates + direction.ToVector();
            currentTile = GetTile(coordinates);
            for (int i = 0; i < goalCount; i++)
                if (paths[i].Contains(currentTile))
                    CutPath(i, currentTile);
        }
        currentTile = paths[goalCount][Random.Range(0, paths[goalCount].Count)];
        if (goalCount < 3)
        {
            blockers.Add(currentTile);
            blockerDirection[goalCount] = FindCloserDirection(currentTile);
        }
        goals[goalCount] = firstTile;
        goalCount++;
        deadends.Remove(firstTile);
    }

    private void AddGoal(int goalIndex)
    {
        if (goalIndex == 0)
        {
            Exit exit = Instantiate<Exit>(exitPrefab);
            exit.transform.parent = goals[goalIndex].transform;
            exit.transform.localPosition = Vector3.zero;
            goals[goalIndex].isExit = true;
        }
        else
        {
            Key key = Instantiate<Key>(keyPrefabs[goalIndex - 1]);
            key.transform.parent = goals[goalIndex].transform;
            key.transform.localPosition = Vector3.zero;
            goals[goalIndex].key = key;
            EdgeDoor door = Instantiate<EdgeDoor>(doorPrefabs[goalIndex - 1]);
            key.door = door;
            Tile tile = blockers[goalIndex - 1];
            door.transform.parent = tile.transform;
            door.transform.localPosition = Vector3.zero;
            door.transform.localRotation = blockerDirection[goalIndex - 1].ToRotation();
            door.tile = tile;
            tile.blocked = true;
        }
    }

    public void Victory()
    {
        victoryText.enabled = true;
    }

    private bool ContainsCoordinates(Vector2Int coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.y >= 0 && coordinate.y < size.y;
    }

    private MazeDirection FindCloserDirection(Tile tile)
    {
        MazeDirection direction = MazeDirection.North;
        Vector2Int coordinates;
        for (int i = 0; i < MazeDirections.Count; i++)
        {
            direction = (MazeDirection)i;
            coordinates = tile.coordinates + direction.ToVector();
            if (tile.GetEdge(direction).GetComponent<EdgePassage>() != null && GetTile(coordinates).stepsFromStart < tile.stepsFromStart)
                break;
        }
        return direction;
    }

    private void CutPath(int pathIndex, Tile cutTile)
    {
        List<Tile> path = paths[pathIndex];
        while (path.Contains(cutTile))
            path.RemoveAt(path.Count - 1);
        blockers[pathIndex] = path[path.Count - 1];
        blockerDirection[pathIndex] = FindCloserDirection(blockers[pathIndex]);
        paths[pathIndex] = path;
    }

    private Tile SpawnTile(Vector2Int coordinates, int lastNumber = -1)
    {
        Tile tile = Instantiate<Tile>(tilePrefab);
        tiles[coordinates.x, coordinates.y] = tile;
        tile.transform.parent = transform;
        tile.transform.localPosition = bottomLeft + new Vector3(coordinates.x * stepSize, coordinates.y * stepSize, 0);
        tile.coordinates = coordinates;
        tile.stepsFromStart = lastNumber + 1;
        return tile;
    }

    private void SpawnPassage(Tile tileSource, Tile tileTarget, MazeDirection direction)
    {
        EdgePassage passage = Instantiate<EdgePassage>(passagePrefab);
        passage.Initialize(tileSource, tileTarget, direction);
        passage = Instantiate<EdgePassage>(passagePrefab);
        passage.Initialize(tileTarget, tileSource, direction.GetOpposite());
    }

    private void SpawnWall(Tile tileSource, Tile tileTarget, MazeDirection direction)
    {
        EdgeWall wall = Instantiate<EdgeWall>(wallPrefab);
        wall.Initialize(tileSource, tileTarget, direction);
        if (tileTarget != null)
        {
            wall = Instantiate<EdgeWall>(wallPrefab);
            wall.Initialize(tileTarget, tileSource, direction.GetOpposite());
        }
    }
}