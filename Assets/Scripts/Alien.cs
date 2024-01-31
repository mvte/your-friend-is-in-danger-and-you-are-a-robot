using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{   
    public Vector2 pos;
    
    public void computeNextStep(ShipManager ship) {
        List<Node> neighbors = ship.GetValidNeighborNodes(pos);
        if(neighbors.Count == 0) {
            return;
        }

        Node chosen = neighbors[Random.Range(0, neighbors.Count)];
        chosen.occupied = true;
        ship.GetNode(pos).occupied = false;
        pos = chosen.pos;
        transform.position = new Vector3(chosen.pos.x, chosen.pos.y, 0);
    }
}
