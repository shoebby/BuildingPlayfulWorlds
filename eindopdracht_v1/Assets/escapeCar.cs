using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class escapeCar : MonoBehaviour
{
    private void OnTriggerEnter(Collider c)
    {
        SceneManager.LoadScene(4);
    }
}
