using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Description: 
// Creates NavMeshAgent so that the zombie can follow the player
public class ZombieMovement : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public NavMeshAgent nav;           // Create agent for Unity AI

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private Transform player;           // Get reference to player's position
    private PlayerHealth playerHealth;  // Get player's health
    private ZombieHealth zombieHealth;  // Get zombie's health
   
    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization (Awake for AI)
    //////////////////////////////////////////////////////////////////////////////////
    void Awake () {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Find player's position
        playerHealth = player.GetComponent<PlayerHealth>();            // Get player's health
        zombieHealth = GetComponent<ZombieHealth>();                   // Get zombie's health
        nav = GetComponent<NavMeshAgent>();                            // Create NavMeshAgent
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update () {

        // If the zombie and player are both alive
        if (zombieHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            // Set destination of zombie to the player of the position
            nav.SetDestination(player.position);
        }
        else 
        {
            // Disable the navigation if either are dead
            nav.enabled = false;
        }
	}

    public void DisableMovement()
    {
        nav.speed = 0;
        
        
    }
}
