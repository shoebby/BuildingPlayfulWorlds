using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class checkpointTimer : MonoBehaviour
{
    public Text counterText;
    public float counter = 60f;
    private float deathTime = 0f;

    public GameObject player;

    private void Start()
    {
        counterText.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (counterText.gameObject.activeSelf)
        {
            counter -= Time.deltaTime;
            counterText.text = counter.ToString("0");

            if (counter <= deathTime)
            {
                SceneManager.LoadScene(3);
            }
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        counterText.gameObject.SetActive(true);
    }
}
