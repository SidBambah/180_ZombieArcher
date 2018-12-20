using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: Change arrowSpeed based on value of force
public class Bow : MonoBehaviour {

    // Variables
    public int arrowSpeed;

    public static int arrowsShot = 0;

    private GameObject arrow;
    Camera cam;
    PositionManipulate positionManipulate;


    void SpawnArrow() {
        GameObject a;
        //Instantiate an arrow facing the direction of the camera
        a = Instantiate(arrow, transform.position + new Vector3(0, 2, 0), transform.rotation  * Quaternion.Euler(0, 90, 0));
        //float x = Screen.width / 2;
        //float y = Screen.height / 2;

        a.gameObject.tag = "Arrow";
        a.GetComponent<Rigidbody>().AddForce(transform.forward * arrowSpeed);
        Debug.Log(transform.forward);

    }


	// Use this for initialization
    void Start () { 
        arrow = GameObject.Find("Arrow");
        positionManipulate = GetComponent<PositionManipulate>();
        // Used to check whether arrow has been fired
        

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Fire1") || PositionManipulate.isFired)
        {
            SpawnArrow();
            arrowsShot += 1;

            // Set boolean back to 0
            PositionManipulate.isFired = false;
        }
	}
}
