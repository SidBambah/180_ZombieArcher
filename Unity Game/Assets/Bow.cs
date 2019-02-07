using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Change arrowSpeed based on value of force
public class Bow : MonoBehaviour {

    // Variables
    public int arrowSpeed;

    public static int arrowsShot = 0;

    UDPInterface udpInterface;





	// Use this for initialization
    void Start () { 
        udpInterface = GetComponent<UDPInterface>();
        // Used to check whether arrow has been fired
        

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1") || UDPInterface.isFired)
        {
            arrowsShot += 1;

            // Set boolean back to 0
            UDPInterface.isFired = false;
        }
	}
}
