using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    // Start is called before the first frame update
    
    public void Move(Vector2 pos) {
        transform.position = new Vector3(pos.x, pos.y, 0);
    }
}
