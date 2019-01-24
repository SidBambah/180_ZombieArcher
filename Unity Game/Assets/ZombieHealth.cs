using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Description: 
// Decrements the zombies health when an arrow contacts the zombie
// Removes the zombie from the game environment when the zombie runs out of health
public class ZombieHealth : MonoBehaviour {

    // Public variables
    public int damagePerShot = 50;          // How much damage each arrow deals to zombie
    public int startingHealth = 100;        // Starting health of zombie
    public int currentHealth;               // Current health of zombie
    public int scoreValue = 50;             // Score increase for each zombie hit
    public int positionTakenIndex;          // Position index into EnemyManager's positionTaken array
    public float zombieNeckHeight = 1.5f;   // Measured location of neck to determine head shots
    public GameObject tutCont;      // Reference to the tutorial controller

    //public AudioClip deathClip;           // TODO: Sound zombie makes when dies

    // Private variables
    //AudioSource zombieAudio;            // Audio source for zombie
    //ParticleSystem hitParticles;        // TODO: Will contain blood
    //CapsuleCollider capsuleCollider;    // Capsule collider for zombie
    

	// Use this for initialization
	void Awake () {
        //zombieAudio = GetComponent<AudioSource>();
        //hitParticles = GetComponentInChildren<ParticleSystem>(); // TODO: Find particle system
        //capsuleCollider = GetComponent<CapsuleCollider>();         // Get reference to capsule collider
        currentHealth = startingHealth;                            // Initialize current health
        tutCont = GameObject.FindWithTag("GameController");
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("ZOMBIE HEALTH: " + currentHealth);
	}

    void OnTriggerEnter(Collider other)
    {
        // If an arrow is in the capsule collider
        if (other.gameObject.tag == "Arrow")
        {
            // Find out whether arrow has dealt damage
            bool arrowhit = other.gameObject.GetComponent<ArrowHit>().arrowHit;

            // Deal damage if arrow has not dealt damage before
            if (!arrowhit)
            {
                float yPos = other.gameObject.transform.position.y;
                if (other.gameObject.transform.position.y > zombieNeckHeight)
                {
                    // Headshot has occurred
                    ZombieTakeDamage(2 * damagePerShot);

                    // Display hit text
                    tutCont.GetComponent<TutorialController>().DisplayHit("Headshot!");
                }
                else
                {
                    // Deal damage to zombie
                    ZombieTakeDamage(damagePerShot);

                    // Display hit text
                    tutCont.GetComponent<TutorialController>().DisplayHit("Body Hit!");
                }
                other.gameObject.GetComponent<ArrowHit>().arrowHit = true;

               
            }
        }
    }

    void ZombieTakeDamage(int amount)
    {
        // TODO: Play zombie audio when zombie is damaged
        //zombieAudio.Play();

        //Decrement zombie health
        currentHealth -= amount;

        // Increase the player's score
        ScoreManager.score += amount;

        // TODO: Display blood
        //hitParticles.Play();

        // If the current health is less than 0, zombie is dead
        if(currentHealth <= 0)
        {
            Death();
        }

    }

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

        Invoke("Helper", 2f);

        


    }
    public void Helper()
    {
        // Turn off mesh agent component to get zombie to stop following player
        GetComponent<NavMeshAgent>().enabled = false;

        // Decrement number of zombies from the scene
        EnemyManager.ZombiesLeft -= 1;

        // Allow that position to be taken by a new zombie
        EnemyManager.positionTaken[positionTakenIndex] = false;

        // Play death animation
        GetComponent<Animator>().SetTrigger("Death");

        // Remove zombie from game environment, allow time for animation
        Destroy(gameObject, 0f);

        // Reset arrows shot to 0
        Bow.arrowsShot = 0;
    }

}
