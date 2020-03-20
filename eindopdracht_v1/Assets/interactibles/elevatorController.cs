using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elevatorController : MonoBehaviour
{
    public GameObject elevator;
    public bool elevatorUp;

    void Update()
    {
        if (elevatorUp == true)
        {
            elevator.transform.Translate(Vector3.up * Time.deltaTime * 5);
        }

        if (elevator.transform.position.y >= -6f)
        {
            elevatorUp = false;
        }
    }

    void OnMouseDown()
    {
        elevatorUp = true;
    }
}
