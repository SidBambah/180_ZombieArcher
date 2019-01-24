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
    public Transform[] spawnPoints;   // Location to spawn enemies


    // Static variables
    public static int ZombiesLeft;
    public static bool[] positionTaken;   // Element is true if that position is taken by zombie
    public static GameObject currentZombie; // reference to current zombie (Lazy way of doing it)

    // Private variables
    private int maxPositions = 7;

	// Use this for initialization
	void Start () {

        // 9 total zombies to kill in tutorial stage
        ZombiesLeft = 14; 

        // For now, we will have 3 spawn points
        positionTaken = new bool[maxPositions];

        // Indicate that there are no zombies occupying positions
        for (int k = 0; k < maxPositions; k++)
        {
            positionTaken[k] = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    // Takes in location to spawn zombie and whether the zombie can move
    public void Spawn(TutorialController.ZombieLocation loc, bool zombieMove)
    {

        // If the player is dead
        if (playerHealth.currentHealth <= 0f)
        {
            return;
        }

        // If there is currently a zombie on the screen, do not spawn new zombie
        for (int k = 0; k < maxPositions; k++)
        {
            if (positionTaken[(int)loc])
                return;
        }

        // Create gameobject for newZombie
        GameObject newZombie;

        // Instantiate zombie in spawnpoint
        newZombie = Instantiate(enemy, spawnPoints[(int)loc].position, spawnPoints[(int)loc].rotation);

        // Give zombie its unique spawn point index
        newZombie.GetComponent<ZombieHealth>().positionTakenIndex = (int)loc;

        // Indicate the position is now occupied
        positionTaken[(int)loc] = true;

        if (zombieMove)
        {
            // Enable zombie to move
            newZombie.GetComponent<ZombieMovement>().enabled = true;
            newZombie.GetComponent<Animator>().SetTrigger("Walk");
        }
        // Keep reference of current zombie in the level
        currentZombie = newZombie;


    }

    // Destroys the current zombie and allows the TutorialController to go back to previous zombie
    public void RespawnPrevious(TutorialController.ZombieLocation loc)
    {
        // Increment number of zombies left, implements respawning previous zombies
        ZombiesLeft += 1;

        //Mark this position as false in EnemyManager positionTaken array
        positionTaken[(int)loc] = false;

        // Destroy current zombie
        Destroy(currentZombie, 0);

        // Reset the number of arrows shot to 0
        Bow.arrowsShot = 0;

    }
}
