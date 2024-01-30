using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{   
    public Vector2 pos;
    
    public void computeNextStep(ShipManager ship) {
        //TODO: implement this
        pos = new Vector2(pos.x, pos.y + 1);

        transform.position = new Vector3(pos.x, pos.y, 0);
    }
}
