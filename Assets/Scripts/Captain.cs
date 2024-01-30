using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Captain : MonoBehaviour
{
    public void Move(Vector2 pos) {
        transform.position = new Vector3(pos.x, pos.y, 0);
    }
}
