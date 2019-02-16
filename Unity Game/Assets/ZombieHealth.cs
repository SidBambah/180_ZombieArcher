using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Description: 
// Decrements the zombies health when an arrow contacts the zombie
// Removes the zombie from the game environment when the zombie runs out of health
public class ZombieHealth : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public int damagePerShot = 50;          // How much damage each arrow deals to zombie
    public int startingHealth = 100;        // Starting health of zombie
    public int currentHealth;               // Current health of zombie
    public int scoreValue = 50;             // Score increase for each zombie hit
    public int openIndex;                   // Position taken into zombie manager array
    public float zombieNeckHeight = 1.5f;   // Measured location of neck to determine head shots
    public GameObject tutCont;              // Reference to the tutorial controller
    public bool arrowHit = false;          
    public MachineLearning ML;              // Reference to ML script
    //public AudioClip deathClip;           // TODO: Sound zombie makes when dies

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    //AudioSource zombieAudio;            // Audio source for zombie
    //ParticleSystem hitParticles;        // TODO: Will contain blood
    //CapsuleCollider capsuleCollider;    // Capsule collider for zombie


    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Awake () {
        //zombieAudio = GetComponent<AudioSource>();
        //hitParticles = GetComponentInChildren<ParticleSystem>(); // TODO: Find particle system
        //capsuleCollider = GetComponent<CapsuleCollider>();         // Get reference to capsule collider
        currentHealth = startingHealth;                            // Initialize current health
        tutCont = GameObject.FindWithTag("GameController");
        ML = GameObject.FindWithTag("MachineLearning").GetComponent<MachineLearning>();
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update () {
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Zombie Being Hit By Arrow
    //////////////////////////////////////////////////////////////////////////////////
    public void ZombieTakeDamage(int amount)
    {
        // TODO: Play zombie audio when zombie is damaged
        //zombieAudio.Play();

        //Decrement zombie health
        currentHealth -= amount;

        // TODO: Display blood
        //hitParticles.Play();

        // If the current health is less than 0, zombie is dead
        if(currentHealth <= 0)
        {
            Death();
        }

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Zombie Death and Auxillary Functions
    //////////////////////////////////////////////////////////////////////////////////
    public void Death()
    {

        //TODO: Set zombie audio to deathclip
        //zombieAudio.clip = deathClip;

        //TODO: Play zombie death audio
        //zombieAudio.Play();

        // Stop zombie from moving
        GetComponent<ZombieMovement>().enabled = false;

        // Play death animation
        GetComponent<Animator>().SetTrigger("Death");

        // Increment number of zombies killed
        GameController.zombiesDestroyed += 1;

        // Increment killstreak
        GameController.killStreak += 1;

        // Call Helper function 2 seconds later
        Invoke("Helper", 2f);

    }

    // Used to allow delay of destroying zombie after dies
    private void Helper()
    {
        // Turn off mesh agent component to get zombie to stop following player
        GetComponent<NavMeshAgent>().enabled = false;

        // Decrement number of zombies from the scene
        GameController.ZombiesLeft -= 1;

        // Allow position in enemy manager array to be reused
        EnemyManager.spotTaken[openIndex] = false;

        // Play death animation
        GetComponent<Animator>().SetTrigger("Death");

        // Remove zombie from game environment, allow time for animation
        Destroy(EnemyManager.zombiesAlive[openIndex], 0f);

        // Reset arrows shot to 0 since zombie respawned
        GameController.tutArrowFires = 0;

        // Decrement number of active zombies in the scene
        EnemyManager.activeZombies -= 1;


    }

}
