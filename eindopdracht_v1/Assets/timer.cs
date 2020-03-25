using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class timer : MonoBehaviour
{
  
    public Text counterText;
    public float counter = 10f;
    private float deathTime = 0f;

    void Start()
    {
        counterText = GetComponent<Text>() as Text;
    }
    void Update()
    {
        counter -= Time.deltaTime;
        counterText.text = counter.ToString("0");

        if (counter <= deathTime)
        {
            SceneManager.LoadScene(3);
        }
    }
}
