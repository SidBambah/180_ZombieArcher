using UnityEngine;
using System.Collections;

public class LongbowShoot : MonoBehaviour
{

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
    public MachineLearning ML;      // ADDED

    private bool shooting = false;
    private float shootingTimer;       
    private float shootingTime;       


    // Use this for initialization
    void Start()
    {
        tutCont = GameObject.FindWithTag("GameController");
        ML = GameObject.FindWithTag("MachineLearning").GetComponent<MachineLearning>();
        shootingTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {

        if (GameController.arrowsLeft > 0)
        {
            if (UDPInterface.testing)
            {
                MouseShoot();
            }
            else
            {
                ControllerShoot();
            }

        }
    }

    void MouseShoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartShot();

            //reset power to 0
            power = 0;
        }

        if (Input.GetMouseButton(0))
        {
            //Increase power of shot
            if (power < maxPower)
            { //power will not exceed 2000
                power += maxPower * Time.deltaTime; //add 2000 to power every second
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndShot();
            InstantiateArrow(power);


        }
    }

    void ControllerShoot()
    {
        if (UDPInterface.validForce)
        {
            StartShot();

            // set power
            power = ((float)maxPower) * UDPInterface.force;

            shooting = true;
            shootingTime = UDPInterface.force;
        }

        if (shooting == true)
        {
            if (shootingTimer >= shootingTime)
            {
                EndShot();
                InstantiateArrow(power);
                shootingTimer = 0f;
                shooting = false;
            }
            else
            {
                shootingTimer += Time.deltaTime;
            }

        }

    }

    void StartShot()
    {
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

    }

    void EndShot()
    {
        //Calculate Shoot animation Time
        //This way, if the player lets go of the mouse before the entire Draw animation is complete, 
        //the Shoot animation isn't played from the begining (where the the string is drawn all the way back)
        float percent = GetComponent<Animation>()["Draw"].time / GetComponent<Animation>()["Draw"].length;
        float shootTime = 1 * percent;

        //play shootSound
        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().time = 0.2f;
        GetComponent<AudioSource>().clip = shootSound;
        if (percent == 0)
        {
            GetComponent<AudioSource>().volume = 1;
        }
        else
        {
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

    }

    void InstantiateArrow(float power)
    {
        //Instantiated projectile (arrow)
        arrow = Instantiate(projectile, arrowSpawn.transform.position, transform.rotation) as Transform;

        //ADDED
        arrow.gameObject.tag = "Arrow";
        flare = Instantiate(Flare, arrowSpawn.transform.position, transform.rotation, arrow.transform) as GameObject;
        //ADDED
        GameController.tutArrowFires += 1;
        GameController.arrowsLeft -= 1;

        //Add force to projectile, based off power
        arrow.transform.GetComponent<Rigidbody>().AddForce(transform.forward * power);

        if (destroyArrows == true)
        {

            //ADDED 
            //Invoke("DestroyObjects", destroyTime);
            StartCoroutine(DestroyObjects(arrow, destroyTime));

        }

    }

    //ADDED
    IEnumerator DestroyObjects(Transform a, float desTime)
    {

        yield return new WaitForSeconds(desTime);

        // Call machine learning miss function if the arrow did not deal damage to a zombie
        if (a.gameObject.GetComponent<ArrowHit>().arrowHit == false)
            ML.missShot(ML.playerName, ML.dbPath);

        tutCont.GetComponent<GameController>().DisplayStats();

        //ADDED
        Destroy(flare, 0);
        //Destroy instantiated arrow, after given time
        Destroy(a.gameObject, 0); //< you can change the amount of time until the arrow is destroyed by chaning destroyTime on the script, in the editor

    }



}
