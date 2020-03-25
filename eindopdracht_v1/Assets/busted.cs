using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class busted : MonoBehaviour
{
    public void ReturnMain()
    {
        SceneManager.LoadScene(0);
    }
}
