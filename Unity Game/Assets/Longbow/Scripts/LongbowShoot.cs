using UnityEngine;
using System.Collections;

public class LongbowShoot : MonoBehaviour {

	public Transform arrowSpawn; //the arrow's spawn point
	public Transform projectile; //the arrow to be instantiated
	public AudioClip drawSound; //sound to be played when drawing the bow (when the string is pulled back)
	public AudioClip shootSound; //sound to play when arrow is shot
	int maxPower = 2000; //max force applied to the arrow
	float power; //how far the arrow will be shoot
	public float destroyTime = 10; //destroy the instantiated arrow, after this many seconds | if destroyArrows is unchecked, this time will have no effect
	public bool destroyArrows = false; //destroy arrows shortly after they've been shot, or not
    public GameObject Flare; //ADDED
    public GameObject tutCont;      // ADDED Reference to the tutorial controller
    public GameObject flare;
    public Transform arrow;
    


    // Use this for initialization
    void Start () {
        tutCont = GameObject.FindWithTag("GameController");
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) {
			//play drawSound
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().clip = drawSound;
			GetComponent<AudioSource>().volume = 1;
			GetComponent<AudioSource>().Play();
			
			//Play Draw animation On Mouse Down
			GetComponent<Animation>().Play("Draw");
			GetComponent<Animation>()["Draw"].speed = 1;
			GetComponent<Animation>()["Draw"].wrapMode = WrapMode.Once;
			
			//Enable arrowSpawn MeshRenderer
			arrowSpawn.transform.GetComponent<Renderer>().enabled = true;

			//reset power to 0
			power = 0;
		}
		if(Input.GetMouseButton(0)) {
			//Increase power of shot
			if(power < maxPower) { //power will not exceed 2000
				power += maxPower * Time.deltaTime; //add 2000 to power every second
			}
		}
		if(Input.GetMouseButtonUp(0)) {
			//Calculate Shoot animation Time
			//This way, if the player lets go of the mouse before the entire Draw animation is complete, 
			//the Shoot animation isn't played from the begining (where the the string is drawn all the way back)
			float percent = GetComponent<Animation>()["Draw"].time / GetComponent<Animation>()["Draw"].length;
			float shootTime = 1 * percent;
			
			//play shootSound
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().time = 0.2f;
			GetComponent<AudioSource>().clip = shootSound;
			if(percent == 0) {
				GetComponent<AudioSource>().volume = 1;
			}
			else {
				//volume of sound will be determined by arrow's power
				GetComponent<AudioSource>().volume = 1 * percent;
			}
			GetComponent<AudioSource>().Play();
			
			//Play Shoot animation On Mouse Up
			GetComponent<Animation>().Play("Shoot");
			GetComponent<Animation>()["Shoot"].speed = 1;
			GetComponent<Animation>()["Shoot"].time = shootTime;
			GetComponent<Animation>()["Shoot"].wrapMode = WrapMode.Once;
			
			//Disable arrow Spawn MeshRenderer
			arrowSpawn.transform.GetComponent<Renderer>().enabled = false;
			
			//Instantiated projectile (arrow)
		    arrow = Instantiate(projectile, arrowSpawn.transform.position, transform.rotation) as Transform;

            //ADDED
            arrow.gameObject.tag = "Arrow";
            flare = Instantiate(Flare, arrowSpawn.transform.position, transform.rotation, arrow.transform) as GameObject;
            //ADDED
            GameController.arrowFires += 1;

            //Add force to projectile, based off power
            arrow.transform.GetComponent<Rigidbody>().AddForce(transform.forward * power);
			
			if(destroyArrows == true) {

                //ADDED 
                Invoke("DestroyObjects", destroyTime);

			}
		}
	}

    //ADDED
    void DestroyObjects()
    {
        tutCont.GetComponent<GameController>().DisplayStats();
        //ADDED
        Destroy(flare, 0);
        //Destroy instantiated arrow, after given time
        Destroy(arrow.gameObject, 0); //< you can change the amount of time until the arrow is destroyed by chaning destroyTime on the script, in the editor

    }



}
