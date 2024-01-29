using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public Tile tile;
    public int dim;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init() {
        GenerateGrid();
    }

    void GenerateGrid() {
        for (int x = 0; x < dim; x++) {
            for (int y = 0; y < dim; y++) {
                Tile spawnedTile = Instantiate(tile, new Vector3(x, y, 0), Quaternion.identity);
                spawnedTile.transform.parent = this.transform;
                spawnedTile.name = $"Tile {x} {y}";
            }
        }
    }
}
