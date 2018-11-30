using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Description: 
// Every 7 seconds, an attempt to spawn a zombie occurs
// If the player is dead or there are 3 zombies, then a zombie does not spawn
// Otherwise, a zombie is spawned randomly out of 3 positions, and in a location 
// where there is not a zombie

public class EnemyManager : MonoBehaviour {

    // Public variables
    public PlayerHealth playerHealth; // Reference to player's health
    public GameObject enemy;          // Reference to enemy to make duplicate of
    public float spawnTime = 10f;      // How often to spawn enemies
    public Transform[] spawnPoints;   // Location to spawn enemies


    // Static variables
    public static int numberZombies;        // Number of zombies in scene
    public static bool[] positionTaken;   // Element is true if that position is taken by zombie

    // Private variables
    private int spawnPointIndex;    //Indexes into spawnPoint array 
    private int maxZombies = 3;


	// Use this for initialization
	void Start () {
        // Calls Spawn function every spawnTime seconds 
        // starting at time spawnTime
        InvokeRepeating("Spawn", spawnTime, spawnTime);

        // Initially, one zombie in scene
        numberZombies = 0;

        // For now, we will have 3 spawn points
        positionTaken = new bool[maxZombies];

        // Indicate that there are no zombies occupying positions
        for (int k = 0; k < maxZombies; k++)
        {
            positionTaken[k] = false;
        }


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Spawn()
    {
        // If the player is dead
        if (playerHealth.currentHealth <= 0f)
        {
            return;
        }

        // If there are 3 zombies in the scene
        if (numberZombies == maxZombies)
        {
            return;
        }
        //spawnPointIndex = Random.Range(0, spawnPoints.Length);

        // Get random index to a spawn point where there is currently not a zombie
        while (true)
        {
            spawnPointIndex = Random.Range(0, spawnPoints.Length);
            if (positionTaken[spawnPointIndex] == false)
            {
                break;
            }
        }

        GameObject newZombie;

        // Instantiate zombie in spawnpoint
        newZombie = Instantiate(enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

        // Give zombie its unique spawn point index
        newZombie.GetComponent<ZombieHealth>().positionTakenIndex = spawnPointIndex;

        // Indicate the position is now occupied
        positionTaken[spawnPointIndex] = true;

        // Increment number of zombies in scene
        numberZombies += 1;
    }


}
