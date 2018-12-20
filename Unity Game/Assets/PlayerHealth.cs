using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Description: 
// Keeps track of and updates player's health
// Allows player to take damage and die
public class PlayerHealth : MonoBehaviour
{
    // Public variables
    public int startingHealth = 100;    // Player's starting health
    public int currentHealth;           // Player's current health
    public Slider healthSlider;         // Reference to health slider on GUI
    public Image damageImage;           // Reference to Image on GUI so can change the flashing color
    public AudioClip deathClip;         // Sound to make when player dies
    public float flashSpeed = 5f;       // Indicates how fast for the flash to occur
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f); // Color of the flash

    
    // Private variables
    AudioSource playerAudio;        // Sound to play when player takes damage
    PlayerShooting playerShooting;  // Reference to PlayerShooting script
    bool isDead;                    // Indicates if the player is dead
    bool damaged;                   // Indicates if the player has taken damage within interval
    //Animator anim;                // Do not currently have animator

    // Use this for initialization
    void Start()
    {
        playerAudio = GetComponent<AudioSource>(); // Get Audio Source attacked to Player
        playerShooting = GetComponentInChildren<PlayerShooting>(); // Get reference to PlayerShooting script
        currentHealth = startingHealth;            // Initialize current health
    }

    // Update is called once per frame
    void Update()
    {
        
        if (damaged)
        {
            damageImage.color = flashColor; // Set the GUI Image to the flash color
        }
        else
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime); // Slowly fade away the flash color
        }
        damaged = false; // Reset damaged to false
    }


    public void TakeDamage(int amount)
    {
        // Mark the player as damaged
        damaged = true;

        // Decrement current health
        currentHealth -= amount;

        // Update health slider
        healthSlider.value = currentHealth;

        //Play audio for player taking damage
        playerAudio.Play();

        // Call Death function
        if (currentHealth <= 0 && !isDead)
        {
            Death();
        }
    }

    void Death()
    {
        // Mark the player as dead
        isDead = true;

        // Play the death clip when player dies
        playerAudio.clip = deathClip;
        playerAudio.Play();

        // Disable player shooting functionality
        playerShooting.enabled = false;

    }
}
