using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Maze mazePrefab;
    public TMP_Text victoryText;
    private Maze maze;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            RestartGame();
    }

    private void StartGame()
    {
        maze = Instantiate<Maze>(mazePrefab);
        maze.victoryText = victoryText;
        StartCoroutine(maze.Generate());
    }

    private void RestartGame()
    {
        StopAllCoroutines();
        maze.victoryText.enabled = false;
        Destroy(maze.gameObject);
        StartGame();
    }
}
