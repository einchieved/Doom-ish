using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainControl : MonoBehaviour
{
    public GameObject player;
    public GameObject playerHUD;
    public GameObject menuHUD;
    public UnityEngine.UI.Text menuHUDGameOverText;

    public GameObject ethanPrefab;
    public GameObject[] ethans;
    private Vector3[] ethansPosition;

    private Vector3 playerSpawn;

    private bool firstPlaythrough = true;
    private int initialEthansCnt;
    private int currentEthanCnt;

    // Start is called before the first frame update
    void Start()
    {
        ethansPosition = new Vector3[ethans.Length];
        initialEthansCnt = currentEthanCnt = ethans.Length;
        playerSpawn = player.transform.position;

        menuHUD.SetActive(false);
        playerHUD.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEthanCnt < 1)
        {
            GameOver(true);
        }

        if (player.GetComponent<PlayerMovement>().GetHealth() <= 0f)
        {
            GameOver(false);
        }
    }

    public void Play()
    {
        menuHUD.SetActive(false);
        playerHUD.SetActive(true);

        if (firstPlaythrough)
        {
            int i = 0;
            foreach (GameObject ethan in ethans)
            {
                ethansPosition[i++] = ethan.transform.position;
            }
        }
        else
        {
            // create new ethans
            foreach (Vector3 vector in ethansPosition)
            {
                Instantiate(ethanPrefab, vector, Quaternion.identity);
            }

            player.transform.position = playerSpawn;
        }

        player.GetComponent<PlayerMovement>().Revive();
        currentEthanCnt = initialEthansCnt;
    }

    public void GameOver(bool victory)
    {
        if (victory)
        {
            menuHUDGameOverText.text = "Victory";
        }
        else
        {
            menuHUDGameOverText.text = "Game Over";
        }

        menuHUDGameOverText.gameObject.SetActive(true);
        firstPlaythrough = false;

        menuHUD.SetActive(true);
        playerHUD.SetActive(false);
    }

    public void ReportDestruction()
    {
        currentEthanCnt--;
    }
}
