using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elevatorDownController : MonoBehaviour
{
    public GameObject elevator;
    public bool elevatorDown;

    void Update()
    {
        if (elevatorDown == true)
        {
            elevator.transform.Translate(Vector3.down * Time.deltaTime * 5);
        }

        if (elevator.transform.position.y <= -17.5f)
        {
            elevatorDown = false;
        }
    }

    void OnMouseDown()
    {
        elevatorDown = true;
    }
}
