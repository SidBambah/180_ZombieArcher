using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Description: 
// Every 7 seconds, an attempt to spawn a zombie occurs
// If the player is dead or there are 3 zombies, then a zombie does not spawn
// Otherwise, a zombie is spawned randomly out of 3 positions, and in a location 
// where there is not a zombie

public class EnemyManager : MonoBehaviour
{

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public PlayerHealth playerHealth;  // Reference to player's health
    public GameObject enemy;           // Reference to enemy to make duplicate of
    public GameObject explosion;       // Reference to explosion prefab
    public Transform[] spawnPoints;    // Location to spawn enemies
    public GameObject[] currentZombie; // reference to current zombie
    public float xmin;                 // Min x value where zombie can spawn
    public float xmax;                 // Max x value where zombie can spawn
    public float zmin;                 // Min z value where zombie can spawn
    public float zmax;                 // Max z value where zombie can spawn

    ////////////////////////////////////////////////////////////////////////////////// 
    // Static Variables
    //////////////////////////////////////////////////////////////////////////////////
    public static GameObject[] zombiesAlive; // reference to each zombie alive
    public static GameObject[] powerups;
    public static bool[] spotTaken;
    public static int activeZombies;          // Number of zombies in the scene

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private int maxPositions = 7;
    private int maxZombies = 32;
    public int maxActiveZombies = 2;

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start()
    {

        zombiesAlive = new GameObject[maxZombies];
        spotTaken = new bool[maxZombies];
        powerups = new GameObject[maxZombies];

        activeZombies = 0;

        for (int k = 0; k < maxZombies; k++)
        {
            spotTaken[k] = false;
        }

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {

    }



    ////////////////////////////////////////////////////////////////////////////////// 
    // Spawn Zombie Function
    //////////////////////////////////////////////////////////////////////////////////
    public void Spawn(GameController.ZombieLocation loc, bool zombieMove, bool freePlay, float speed)
    {

        // If the player is dead
        if (playerHealth.currentHealth <= 0f)
        {
            return;
        }


        // In tutorial mode, only allow one zombie on the screen
        if (!freePlay)
        {
            for (int k = 0; k < maxZombies; k++)
            {
                if (spotTaken[k] == true)
                    return;
            }
        }

        int openIndex = -1;

        // Look for open position in zombie array
        for (int k = 0; k < maxActiveZombies; k++)
        {
            if (spotTaken[k] == false)
            {
                openIndex = k;
                break;
            }

        }

        // Cannot hold more zombies in the scene
        if (openIndex == -1)
            return;

        Vector3 zombiePos;
        // Choose where to spawn zombie
        if (freePlay)
        {
            zombiePos.x = Random.Range(xmin, xmax);
            zombiePos.y = 7.525024e-07f;
            zombiePos.z = Random.Range(zmin, zmax);
        }
        else
        {
            zombiePos = spawnPoints[(int)loc].position;

        }

        // Create gameobject for newZombie
        GameObject newZombie;

        // Instantiate zombie in spawnpoint
        newZombie = Instantiate(enemy, zombiePos, spawnPoints[0].rotation);


        // Let zombie know its index into zombie array manager
        newZombie.GetComponent<ZombieHealth>().openIndex = openIndex;

        if (zombieMove)
        {
            // Enable zombie to move
            newZombie.GetComponent<ZombieMovement>().enabled = true;
            newZombie.GetComponent<Animator>().SetTrigger("Walk");
            newZombie.GetComponent<NavMeshAgent>().speed = speed;
            newZombie.GetComponent<Animator>().speed = speed + 1.0f; // Correcting factor
        }

        // Store new zombie
        zombiesAlive[openIndex] = newZombie;

        // Indicate spot is taken by new zombie
        spotTaken[openIndex] = true;

        // Increment number of active zombies
        activeZombies += 1;


    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Respond Previous Zombie (Used for Tutorial Stage)
    //////////////////////////////////////////////////////////////////////////////////
    public void RespawnPrevious(GameController.ZombieLocation loc)
    {
        // Increment number of zombies left, implements respawning previous zombies
        GameController.ZombiesLeft += 1;

        DestroyAllZombies();

        // Reset the number of arrows shot to 0
        GameController.tutArrowFires = 0;

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Remove all Zombie Gameobjects from the Scene
    //////////////////////////////////////////////////////////////////////////////////
    public void DestroyAllZombies()
    {
        for (int k = 0; k < maxZombies; k++)
        {
            if (spotTaken[k] == true)
            {
                Destroy(zombiesAlive[k], 0);
                spotTaken[k] = false;

            }
        }
        activeZombies = 0;
    }



    ////////////////////////////////////////////////////////////////////////////////// 
    // Set Speed of All Zombies
    //////////////////////////////////////////////////////////////////////////////////
    public void SetSpeed(float speed)
    {
        for (int k = 0; k < maxZombies; k++)
        {
            if (spotTaken[k] == true)
            {
                zombiesAlive[k].GetComponent<NavMeshAgent>().speed = speed;
                zombiesAlive[k].GetComponent<Animator>().speed = speed;
            }
        }


    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Change Maximum Active Zombies
    //////////////////////////////////////////////////////////////////////////////////
    // If incrementing, increase array size
    public void IncMaxActiveZombies()
    {
        maxActiveZombies += 1;
    }

    // Before decrementing, destroy zombie in the last array spot
    public void DecMaxActiveZombies()
    {
        if (spotTaken[maxActiveZombies - 1] == true)
        {
            Destroy(zombiesAlive[maxActiveZombies - 1], 0);
            spotTaken[maxActiveZombies - 1] = false;
        }
        maxActiveZombies -= 1;

    }


    ////////////////////////////////////////////////////////////////////////////////// 
    // Blow Up Zombies
    //////////////////////////////////////////////////////////////////////////////////
    public void BlowUpZombies()
    {
        for (int k = 0; k < maxZombies; k++)
        {
            if (spotTaken[k] == true)
            {
                // Instantiate an explosion at each zombie
                Transform temp = zombiesAlive[k].transform;
                powerups[k] = Instantiate(explosion, temp.position, temp.rotation);
                Destroy(powerups[k], 3f);
            }


        }
    }



}
