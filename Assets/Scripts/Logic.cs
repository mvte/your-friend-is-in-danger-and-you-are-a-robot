using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Logic : MonoBehaviour
{
    public ShipManager ship;
    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        ship.Init();
        cam.transform.position = new Vector3(1, ship.dim/2 - 0.5f, -10);
        cam.GetComponent<Camera>().orthographicSize = ship.dim * 9 / 16;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    
}
