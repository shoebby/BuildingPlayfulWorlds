using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class checkpointTimer : MonoBehaviour
{
    public Text counterText;
    public float counter = 5f;
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
                playerPosLoad();
                counter = 60f;
            }
        }
    }

    private void OnTriggerEnter(Collider c)
    {
        counterText.gameObject.SetActive(true);
        PlayerPosSave();
    }

    public void PlayerPosSave()
    {
        PlayerPrefs.SetFloat("p_x", player.transform.position.x);
        PlayerPrefs.SetFloat("p_y", player.transform.position.y);
        PlayerPrefs.SetFloat("p_z", player.transform.position.z);
        PlayerPrefs.SetInt("Saved", 1);
        PlayerPrefs.Save();
    }
    
    public void playerPosLoad()
    {
        PlayerPrefs.SetInt("TimeToLoad", 1);
        PlayerPrefs.Save();
    }
}
