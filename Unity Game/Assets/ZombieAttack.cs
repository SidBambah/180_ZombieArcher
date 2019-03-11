using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Description:
// Checks if zombie is in range of player
// If so, it deals damage to the player
public class ZombieAttack : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    private float timeBetweenAttacks = 1f; // Time to wait between attacks for zombie
    private int attackDamage = 5;           // How much damage a zombie deals to the player
    public AudioClip attackClip;


    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private GameObject player;         // Reference to player
    private PlayerHealth playerHealth; // Player's health
    private ZombieHealth zombieHealth; // Zombie's health
    private ZombieMovement zombieMovement;
    private bool playerInRange;        // True if the player is in the zombie's range
    private float timer;               // Ensures zombie attacks every timeBetweenAttacks

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Awake () {
        player = GameObject.FindGameObjectWithTag("Player"); // Get reference to player gameobject
        playerHealth = player.GetComponent<PlayerHealth>();  // Get player's health
        zombieHealth = GetComponent<ZombieHealth>();         // Get zombie's health
        zombieMovement = GetComponent<ZombieMovement>();
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {

        // Increment timer on each frame
        timer += Time.deltaTime;

        // If waited long enough, the player is in range, and the zombie is alive
        if (timer >= timeBetweenAttacks && playerInRange && zombieHealth.currentHealth > 0)
        {


            Attack();
            GetComponent<Animator>().SetTrigger("Attack");
            zombieMovement.DisableMovement();
        }

        // If the player is dead
        if (playerHealth.currentHealth <= 0)
        {

        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Zombie is in Range of Collider
    //////////////////////////////////////////////////////////////////////////////////
    void OnTriggerEnter(Collider other)
    {
        // If the player is in the range of the zombie
        if (other.gameObject.tag == "Player")
        {
            if (other.GetType() == typeof(CapsuleCollider))
            {

                playerInRange = true;

            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Zombie No Longer in range of Collider
    //////////////////////////////////////////////////////////////////////////////////
    void OnTriggerExit(Collider other)
    {

        /*// If the player leaves the range of the zombie
        if (other.gameObject == player)
        {
            playerInRange = false;
            GetComponent<Animator>().SetTrigger("Walk");
        }*/

    }


    ////////////////////////////////////////////////////////////////////////////////// 
    // Attack Player
    //////////////////////////////////////////////////////////////////////////////////
    void Attack()
    {
        // Reset timer
        timer = 0f;

        // If the player is still alive, deal damage to the player
        if (playerHealth.currentHealth > 0)
        {

            //GetComponent<AudioSource>().clip = attackClip;
            //GetComponent<AudioSource>().time = 1f;
            //GetComponent<AudioSource>().Play();
            GetComponent<AudioSource>().PlayOneShot(attackClip);

            playerHealth.TakeDamage(attackDamage);
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }
}
