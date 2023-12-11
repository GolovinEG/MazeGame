using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates;

    public int stepsFromStart { get; set; }
    public bool blocked { get; set; } = false;
    public Key key { get; set; } = null;
    public bool isExit { get; set; } = false;
    private int initializedEdgeCount;
    private TileEdge[] edges = new TileEdge[MazeDirections.Count];

    public bool IsFullyInitialized
    {
        get
        {
            return initializedEdgeCount == MazeDirections.Count;
        }
    }

    public int PassageCount
    {
        get
        {
            int passageCount = 0;
            foreach (TileEdge edge in edges)
                if (edge.GetComponent<EdgePassage>() != null)
                    passageCount++;
            return passageCount;
        }
    }

    public MazeDirection RandomUninitializedDirection
    {
        get
        {
            int skips = Random.Range(0, MazeDirections.Count - initializedEdgeCount);
            for(int i = 0; i < MazeDirections.Count; i++)
                if (edges[i] == null)
                {
                    if (skips == 0)
                        return (MazeDirection)i;
                    skips--;
                }
            throw new System.InvalidOperationException("No uninitialized directions left.");
        }
    }

    public TileEdge GetEdge(MazeDirection direction) 
    {
        return edges[(int)direction];
    }

    public void SetEdge(MazeDirection direction, TileEdge edge)
    {
        edges[(int)direction] = edge;
        initializedEdgeCount++;
    }
}
