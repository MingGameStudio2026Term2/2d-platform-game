using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class timemanager : MonoBehaviour
{
    public TMP_Text timeText;         // assign in Inspector (TextMeshPro - Text (UI))
    public GameObject player;         // assign in Inspector or will try to find by tag "Player"
    public float startTime = 60f;     // starting time in seconds

    private float timeRemaining;
    private bool isRunning = true;

    void Start()
    {
        timeRemaining = Mathf.Max(0f, startTime);
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        UpdateTimeText();
    }

    void Update()
    {
        if (!isRunning || timeRemaining <= 0f) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            OnTimeUp();
        }

        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        if (timeText != null)
        {
            timeText.text = $"time : {seconds:00}";
        }
    }

    private void OnTimeUp()
    {
        if (player != null)
        {
            Destroy(player);
        }
        else
        {
            Debug.LogWarning("timemanager: Player not assigned and no object with tag 'Player' found to destroy.");
        }

        // Load the "time up" scene additively
        SceneManager.LoadScene("time up", LoadSceneMode.Additive);
    }
}                               