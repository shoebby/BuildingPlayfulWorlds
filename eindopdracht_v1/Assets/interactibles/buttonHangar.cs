﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonHangar : MonoBehaviour
{
    public GameObject door;
    public bool doorIsOpening;
    public GameObject button;

    void Update()
    {
        if (doorIsOpening == true)
        {
            door.transform.Translate(Vector3.up * Time.deltaTime * 5);
        }

        if (door.transform.position.y > 0.25f)
        {
            doorIsOpening = false;
        }
    }

    void OnMouseDown()
    {
        doorIsOpening = true;
        button.transform.Translate(Vector3.down * Time.deltaTime * 10);
    }
}
