using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public EdgeDoor door { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UseKey()
    {
        door.tile.blocked = false;
        Destroy(door.gameObject);
        Destroy(gameObject);
    }
}
