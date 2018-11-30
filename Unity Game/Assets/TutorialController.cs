using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Description: 
// 
public class TutorialController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Start in Stage1, i.e. call function Stage1
	}
	
	// Update is called once per frame
	void Update () {
		
        // If you get a pause signal, pause the game
	}






    void Stage1() 
    {
        // Spawn zombie one at a time at varying (increasing) distances

        // Close zombie: 
        // Unlimited tries

        // Next zombie: 
        // 5 tries
        // If you fail to kill the zombie, destroy current zombie 
        // and spawn close zombie again


        // Far zombie: 
        // 5 tries
        // If you fail to kill the zombie, destroy current zombie
        // and spawn middle zombie

        // If you successfully kill all zombies, call Stage2 function



    }


    void Stage2()
    {
        // Same logic as Stage1, except zombies spawned horizontally

    }

    void Stage3()
    {
        // Ducking under flying object
    }

    void Stage4()
    {
        // Random motion of zombies
        // Move zombies in a circle
    }


    // Create method to destroy all zombies in scene

    // For pause method, get reference to zombies
    // Disable movement for each of them
    // In update method, continuosly listen for 







}
