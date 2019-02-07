using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

// Description: 
// 
public class GameController : MonoBehaviour
{

    // Public variables
    public Animator anim;    // Reference to UI animator
    public Text stageText;   // Displays current stage of tutorial stage
    public Text zombiesLeftText; // Displays number of zombies left
    public float restartDelay = 2f;   // How long it takes before we restart the game
    public enum ZombieLocation { Near, Middle, Far, Left, Right, FarLeft, FarRight }; // Locations to spawn zombies
    public EnemyManager enemManager;  // Reference to enemy manager script
    public bool zombieMov = false;    // Indicates whether the zombie to be spawned can move
    public int maxShots = 5;          // Maximum shots a player can take before going back a stage
    public Text pauseText;
    public Text hitText;                    // Displays "Hit!" when zombie is hit
    public Text statsText;            // Displays user's statistics
    public ZombieLocation loc;        // Where to spawn zombie
    public ZombieLocation resLoc;     // Where to respawn zombie
    public float zombieSpeed = 0.001f;         // Speed of the zombies
    public PlayerHealth playerHealth; // Get player's health
    public Transform player; // Reference to transform of the player

    public static int Cur_State;    // Indicates current state of tutorial scene
    public static int arrowHits = 0;   // Number of arrow hits
    public static int arrowFires = 0;  // Number of arrow fires
    public static int bodyShots = 0;   // Number of body shots
    public static int headShots = 0;   // Number of head shots
    public static int ZombiesLeft;     // Number of zombies left for tutorial stage
    public static int zombiesDestroyed = 0; // Total number of zombies destroyed
    public static int tutArrowFires = 0;    // Number of arrow fires in tutorial


    // Private variables
    private enum State { GameStart, Stage1, Stage2, Stage3, Stage4, FreePlay, GameOver };  // Different states for tutorial 
    private float restartTimer;       // After game is over, time before scene is reloaded
    private float spawnTimer;         // Timer for spawning zombies
    private float spawnTime = 4f;     // Time between zombie spawns in free play mode
    private float capacityTimer;      // Timer for increasing capacity of zombies in scene
    private float capacityTime = 5f;  // Time between increasing capacity of zombies


    // Use this for initialization
    void Start()
    {
        // 9 total zombies to kill in tutorial stage
        ZombiesLeft = 9;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        restartTimer = 0f;
        spawnTimer = 0f;
        capacityTimer = 0f;

        // Initialize location
        loc = ZombieLocation.Near;
        resLoc = ZombieLocation.Near;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerHealth.currentHealth <= 0)
        {
            Cur_State = (int)State.GameOver;
        }


        // Enter stage based on current state
        switch (Cur_State)
        {
            case (int)State.GameStart:
                GameStart();
                break;
            case (int)State.Stage1:
                Stage1();
                break;
            case (int)State.Stage2:
                Stage2();
                break;
            case (int)State.Stage3:
                Stage3();
                break;
            case (int)State.Stage4:
                Stage4();
                break;
            case (int)State.FreePlay:
                FreePlay();
                break;
            case (int)State.GameOver:
                GameOver();
                break;
        }

        // Check if game is paused
        if (Cur_State != (int)State.GameStart)
        {
            CheckIfPaused();
        }

        // Display text
        if (Cur_State != (int)State.FreePlay)
        {
            // Display stage number
            stageText.text = "Tutorial Stage " + Cur_State + "/3";

            // Display number of zombies left
            zombiesLeftText.text = "Zombies Left: " + ZombiesLeft;
        }
        else
        {
            stageText.text = "Survival Mode";
            zombiesLeftText.text = "Active Zombies: " + EnemyManager.activeZombies;
        }
    }


    // Game start screen
    // Begins stage 1 when the return button is pressed or the user says "start"
    void GameStart()
    {
        if (Input.GetKeyDown("return") || !UDPInterface.isPaused)
        {
            // Cause transition in UI animator
            anim.SetTrigger("GameStart");
            Cur_State = (int)State.Stage1;
        }
    }

    // Spawn zombie one at a time at varying (increasing) distances
    void Stage1()
    {
        // Zombies cannot move in this stage
        zombieMov = false;

        // Close zombie: Unlimited tries
        if (ZombiesLeft == 9)
        {
            loc = ZombieLocation.Near;
            resLoc = ZombieLocation.Near;
        }
        // Middle zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn near zombie again
        else if (ZombiesLeft == 8)
        {
            loc = ZombieLocation.Middle;
            resLoc = ZombieLocation.Near;
        }
        // Far zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn middle zombie again
        else if (ZombiesLeft == 7)
        {
            loc = ZombieLocation.Far;
            resLoc = ZombieLocation.Middle;
        }
        // If you successfully kill all zombies, call Stage2 function
        else if (ZombiesLeft == 6)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage1Complete");
            Cur_State = (int)State.Stage2;
            return;
        }

        CheckBowShots();
    }

    // Same logic as Stage1, except zombies spawned horizontally
    void Stage2()
    {
        zombieMov = false;

        if (ZombiesLeft == 6)
        {
            loc = ZombieLocation.Left;
            resLoc = ZombieLocation.Left;
        }
        else if (ZombiesLeft == 5)
        {
            loc = ZombieLocation.Middle;
            resLoc = ZombieLocation.Left;
        }
        else if (ZombiesLeft == 4)
        {
            loc = ZombieLocation.Right;
            resLoc = ZombieLocation.Middle;
        }
        else if (ZombiesLeft == 3)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage2Complete");
            Cur_State = (int)State.Stage3;
            return;
        }

        CheckBowShots();

    }

    // Spawn zombies horizontally and allow them to move
    void Stage3()
    {
        // Zombies can move in this stage
        zombieMov = true;

        if (ZombiesLeft == 3)
        {
            loc = ZombieLocation.FarLeft;
            resLoc = ZombieLocation.FarLeft;
        }
        else if (ZombiesLeft == 2)
        {
            loc = ZombieLocation.Far;
            resLoc = ZombieLocation.FarLeft;
        }
        else if (ZombiesLeft == 1)
        {
            loc = ZombieLocation.FarRight;
            resLoc = ZombieLocation.Far;
        }
        else if (ZombiesLeft == 0)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage3Complete");
            Cur_State = (int)State.FreePlay;
            return;
        }
        CheckBowShots();
    }

    // Check if need to respawn zombie
    // If not, spawn new zombie
    void CheckBowShots()
    {
        if (tutArrowFires >= maxShots)
            enemManager.RespawnPrevious(resLoc);
        else
            enemManager.Spawn(loc, zombieMov, false, zombieSpeed);
    }

    // Spawns zombies in 4 different locations
    void Stage4()
    {
        zombieMov = true;
        if (UDPInterface.isValidQuadrant)
        {
            if (UDPInterface.spawnQuadrant == 1 || Input.GetKeyDown("u"))
                loc = ZombieLocation.FarLeft;
            else if (UDPInterface.spawnQuadrant == 2 || Input.GetKeyDown("i"))
                loc = ZombieLocation.FarRight;
            else if (UDPInterface.spawnQuadrant == 3 || Input.GetKeyDown("j"))
                loc = ZombieLocation.Left;
            else if (UDPInterface.spawnQuadrant == 4 || Input.GetKeyDown("k"))
                loc = ZombieLocation.Right;
            // UDPInterface.isValidQuadrant = false;
            enemManager.Spawn(loc, zombieMov, false, zombieSpeed);
        }
        if (ZombiesLeft == 0)
        {
            Cur_State = (int)State.FreePlay;
            zombiesDestroyed = 0;
        }



        //    anim.SetTrigger("Stage4Complete");


    }

    // Free play mode where zombies get progressively stronger and faster
    void FreePlay()
    {
        // Spawn zombie every spawnTime seconds
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            adjustZombieSpeed(0.001f);
            adjustSpawnTime(0f);
            enemManager.Spawn(loc, zombieMov, true, zombieSpeed);

            // Reset timer
            spawnTimer = 0f;
        }


        // Change the speed of each zombie active in the scene
        enemManager.setSpeed(zombieSpeed);

        capacityTimer += Time.deltaTime;
        if (capacityTimer >= capacityTime)
        {
            enemManager.incMaxActiveZombies();
            capacityTimer = 0f;
        }

        /*// Check if we should go back to tutorial stage
        if (condition)
        {
            Cur_State = (int)State.Stage1;
            ZombiesLeft = 9;
        }*/

    }


    // Reloads the game after restartDelay has elapsed
    void GameOver()
    {
        // Begin Timer
        restartTimer += Time.deltaTime;

        // Once restartDelay has elapsed
        if (restartTimer >= restartDelay)
        {
            // Load currently loaded level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void adjustZombieSpeed(float amt)
    {
        // Ensure zombie speed does not fall below 0
        if (zombieSpeed + amt <= 0)
        {
            return;
        }
        // Increment zombie speed
        zombieSpeed += amt;

    }

    void adjustSpawnTime(float amt)
    {
        if (spawnTime + amt <= 0)
        {
            return;
        }
        spawnTime += amt;

    }


    void CheckIfPaused()
    {
        if (UDPInterface.isPaused || Input.GetKeyDown("y"))
        {
            UDPInterface.moveBowValid = false;
            pauseText.enabled = true;
        } else{
            pauseText.enabled = false;
            UDPInterface.moveBowValid = true;
        }
    }

    public void DisplayHit(string t)
    {
        hitText.text = t;
        Color temp = hitText.color;
        temp.a = 1;
        hitText.color = temp;
        Invoke("HitTextOff", 1f);
    }
    void HitTextOff()
    {
        Color temp = hitText.color;
        temp.a = 0;
        hitText.color = temp;

    }

   public void DisplayStats()
    {

        // Display statistics
        int misses = arrowFires - arrowHits;
        float hit_pct;
        float body_pct;
        float head_pct;

        // Compute hit percentage
        if (arrowFires == 0)
        {
            hit_pct = 100f;
            body_pct = 100f;
            head_pct = 100f;
        }
        else
        {
            hit_pct = 100f * ((float)arrowHits / (float)arrowFires);
            body_pct = 100f * ((float)bodyShots / (float)arrowFires);
            head_pct = 100f * ((float)headShots / (float)arrowFires);
        }
        statsText.text = "Hits: " + arrowHits + "\nMisses: " + misses + "\nHit %: " + hit_pct + "\nBody Shots: " + bodyShots + "\nHead Shots: " + headShots
            + "\nZombies Destroyed: " + zombiesDestroyed;
    }

}
