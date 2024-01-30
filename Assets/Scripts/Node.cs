using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool open = false;
    public bool occupied = false;
    public Color openColor;
    public Color closedColor;
    public SpriteRenderer sr;
    public Vector2 pos;

    [ContextMenu("Open")]
    public void Open() {
        open = true;
        sr.color = openColor;
    }

    public void Close() {
        open = false;
        sr.color = closedColor;
    }
}
