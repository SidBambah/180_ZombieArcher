using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour {

    // Public variables
    public PlayerHealth playerHealth; // Get player's health
    public float restartDelay = 5f;   // How long it takes before we restart the game

    public static int zombiesKilled;

    Animator anim;
    float restartTimer;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        zombiesKilled = 0;
	}
	
	// Update is called once per frame
	void Update () {

        // If the player is dead
		if (playerHealth.currentHealth <= 0 || zombiesKilled >= 10)
        {

            // Set GameOver trigger to transition to GameOver image
            anim.SetTrigger("Stage3Complete");

            // Begin Timer
            restartTimer += Time.deltaTime;

            // Once restartDelay has elapsed
            if (restartTimer >= restartDelay) 
            {
                // Load currently loaded level
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

        }
	}
}
