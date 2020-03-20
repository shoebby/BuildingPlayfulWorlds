using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bridgeController : MonoBehaviour
{
    public GameObject bridge;
    public bool bridgeIsLowering;

    void Update()
    {
        if (bridgeIsLowering == true)
        {
            bridge.transform.Translate(Vector3.down * Time.deltaTime * 5);
        }

        if (bridge.transform.position.y <= 19.5f)
        {
            bridgeIsLowering = false;
        }
    }

    void OnMouseDown()
    {
        bridgeIsLowering = true;
    }
}
