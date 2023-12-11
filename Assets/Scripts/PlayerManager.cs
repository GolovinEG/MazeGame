using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Maze maze { get; set; }
    public Tile tile { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
            ProccessMovement(MazeDirection.North);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ProccessMovement(MazeDirection.East);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ProccessMovement(MazeDirection.South);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ProccessMovement(MazeDirection.West);
    }

    private void ProccessMovement(MazeDirection direction)
    {
        if (tile.GetEdge(direction).GetComponent<EdgePassage>() != null && !maze.GetTile(tile.coordinates + direction.ToVector()).blocked)
        {
            tile = maze.GetTile(tile.coordinates + direction.ToVector());
            transform.position = tile.transform.position;
            if (tile.key != null)
                tile.key.UseKey();
            else if (tile.isExit)
                maze.Victory();
        }
    }
}
