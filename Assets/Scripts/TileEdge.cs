using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileEdge : MonoBehaviour
{
    public Tile tileSource { get; set; }
    public Tile tileTarget { get; set; }
    public MazeDirection direction { get; set; }

    public void Initialize(Tile tileSource, Tile tileTarget, MazeDirection direction)
    {
        this.tileSource = tileSource;
        this.tileTarget = tileTarget;
        this.direction = direction;
        tileSource.SetEdge(direction, this);
        transform.parent = tileSource.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = direction.ToRotation();
    }
}
