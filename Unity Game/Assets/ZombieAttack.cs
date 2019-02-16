using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Description:
// Checks if zombie is in range of player
// If so, it deals damage to the player
public class ZombieAttack : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public float timeBetweenAttacks = 0.5f; // Time to wait between attacks for zombie
    public int attackDamage = 10;           // How much damage a zombie deals to the player

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private GameObject player;         // Reference to player
    private PlayerHealth playerHealth; // Player's health
    private ZombieHealth zombieHealth; // Zombie's health
    private bool playerInRange;        // True if the player is in the zombie's range
    private float timer;               // Ensures zombie attacks every timeBetweenAttacks
    //Animator anim; // Do not currently have animator

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Awake () {
        player = GameObject.FindGameObjectWithTag("Player"); // Get reference to player gameobject
        playerHealth = player.GetComponent<PlayerHealth>();  // Get player's health
        zombieHealth = GetComponent<ZombieHealth>();         // Get zombie's health
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {

        // Increment timer on each frame
        timer += Time.deltaTime;
        //Debug.Log("Player health: " + playerHealth.currentHealth);
        //Debug.Log("PlayerInRange: " + playerInRange);

        // If waited long enough, the player is in range, and the zombie is alive
        if (timer >= timeBetweenAttacks && playerInRange && zombieHealth.currentHealth > 0)
        {

            Attack();
            GetComponent<Animator>().SetTrigger("Attack");
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
        if (other.gameObject == player)
        {
            playerInRange = true;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Zombie No Longer in range of Collider
    //////////////////////////////////////////////////////////////////////////////////
    void OnTriggerExit(Collider other)
    {

        // If the player leaves the range of the zombie
        if (other.gameObject == player)
        {
            playerInRange = false;
            GetComponent<Animator>().SetTrigger("Walk");
        }

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
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
