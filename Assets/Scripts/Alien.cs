using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Alien : MonoBehaviour
{   
    // gives the alien position
    public Vector2 pos;

    public SpriteRenderer sr;
    
    // chooses the next step for the alien 
    public void computeNextStep(ShipManager ship, bool front = true) {
        //compute the valid neighbors
        List<Node> neighbors = ship.GetValidNeighborNodes(pos);
        // if there are no valid neighbors, don't do anything
        if(neighbors.Count == 0) {
            return;
        }

        // choose a random valid neighbor to move to 
        Node chosen = neighbors[ThreadSafeRandom.Next(neighbors.Count)];
        chosen.occupied = true;
        ship.GetNode(pos).occupied = false;
        pos = chosen.pos;
        if(front)
            transform.position = new Vector3(chosen.pos.x, chosen.pos.y, 0);
    }
}
