using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHit : MonoBehaviour
{
    // Manipulated by zombie health function
    // Attached to each arrow object
    // Can use this variable to check whether the arrow has dealt 
    // damage to a zombie

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public bool arrowHit = false;
    public int damagePerShot = 50;          // How much damage each arrow deals to zombie
    public GameObject tutCont;      // Reference to the tutorial controller
    public MachineLearning ML;

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
        tutCont = GameObject.FindWithTag("GameController");
        ML = GameObject.FindWithTag("MachineLearning").GetComponent<MachineLearning>();
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {
    }


    ////////////////////////////////////////////////////////////////////////////////// 
    // Handles Collision with Zombie
    //////////////////////////////////////////////////////////////////////////////////
    void OnTriggerEnter(Collider other)
    {
   
        // If an arrow is in the capsule collider
        if (other.gameObject.tag == "Enemy")
        {
            if (other.GetType() == typeof(CapsuleCollider))
            {
                // Deal damage if arrow has not dealt damage before
                if (!arrowHit)
                {

                    if (transform.position.y > other.gameObject.GetComponent<ZombieHealth>().zombieNeckHeight)
                    {
                        // Headshot has occurred
                        other.gameObject.GetComponent<ZombieHealth>().ZombieTakeDamage(2 * damagePerShot);

                        // Display hit text
                        tutCont.GetComponent<GameController>().DisplayHit("Headshot!");

                        // Call machine learning headshot function
                        ML.headShot(ML.playerName, ML.dbPath);

                    }
                    else
                    {
                        // Deal damage to zombie
                        other.gameObject.GetComponent<ZombieHealth>().ZombieTakeDamage(damagePerShot);

                        // Display hit text
                        tutCont.GetComponent<GameController>().DisplayHit("Body Hit!");

                        // Call machine learning headshot function
                        ML.bodyShot(ML.playerName, ML.dbPath);

                    }
                    arrowHit = true;
                    GameController.arrowHits += 1;
                    tutCont.GetComponent<GameController>().DisplayStats();

                }
            }
        }
  
    }



}