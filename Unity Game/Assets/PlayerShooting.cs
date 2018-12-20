using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour {

    // Public variables
    public int arrowSpeed = 100;            // Defines how fast the arrow movesn
    public float timeBetweenArrows = 0.2f;  // Creates delay between shots
    public float range = 100f;              // 
    public AudioSource arrowAudio;
    

    float timer;
    Ray shootRay;
    RaycastHit shootHit;
    int shootableMask;
    

	// Use this for initialization
	void Awake () {
        shootableMask = LayerMask.GetMask("Shootable");
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;

        if (Input.GetButton("Fire1") && timer >= timeBetweenArrows)
        {
            Shoot();
        }
	}

    void Shoot()
    {
        timer = 0f;
        arrowAudio.Play();
        
       /* if ()
        {
            ZombieHealth zombieHealth = ;
            zombieHealth.TakeDamage(damagePerShot, shootHit.point)

        }*/


    }





}
