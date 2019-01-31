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
    public float restartDelay = 5f;   // How long it takes before we restart the game
    public enum ZombieLocation { Near, Middle, Far, Left, Right, FarLeft, FarRight }; // Locations to spawn zombies
    public EnemyManager enemManager;  // Reference to enemy manager script
    public bool zombieMov = false;    // Indicates whether the zombie to be spawned can move
    public int maxShots = 5;          // Maximum shots a player can take before going back a stage
    public Text pauseText;
    public Text hitText;                    // Displays "Hit!" when zombie is hit
    public Text statsText;            // Displays user's statistics
    public ZombieLocation loc;        // Where to spawn zombie
    public ZombieLocation resLoc;     // Where to respawn zombie

    public static int Cur_State;    // Indicates current state of tutorial scene
    public static int arrowHits = 0;
    public static int arrowFires = 0;
    public static int bodyShots = 0;
    public static int headShots = 0;
    public static int ZombiesLeft;
    public static int zombiesDestroyed = 0;

    Transform player;

    // Private variables
    private enum State { GameStart, Stage1, Stage2, Stage3, Stage4, FreePlay, GameOver };  // Different states for tutorial 
    private float restartTimer;       // After game is over, time before scene is reloaded
    private float spawnTimer;         // Timer for spawning zombies
    private const float spawnTime = 3f;     // Time between zombie spawns in free play mode

    // Use this for initialization
    void Start()
    {
        // 14 total zombies to kill in tutorial stage
        ZombiesLeft = 14;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        restartTimer = 0f;
        spawnTimer = 0f;

        // Initialize location
        loc = ZombieLocation.Near;
        resLoc = ZombieLocation.Near;
    }

    // Update is called once per frame
    void Update()
    {
    

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
            stageText.text = "Tutorial Stage " + Cur_State + "/4";

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
        if (ZombiesLeft == 14)
        {
            loc = ZombieLocation.Near;
            resLoc = ZombieLocation.Near;
        }
        // Middle zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn near zombie again
        else if (ZombiesLeft == 13)
        {
            loc = ZombieLocation.Middle;
            resLoc = ZombieLocation.Near;
        }
        // Far zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn middle zombie again
        else if (ZombiesLeft == 12)
        {
            loc = ZombieLocation.Far;
            resLoc = ZombieLocation.Middle;
        }
        // If you successfully kill all zombies, call Stage2 function
        else if (ZombiesLeft == 11)
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

        if (ZombiesLeft == 11)
        {
            loc = ZombieLocation.Left;
            resLoc = ZombieLocation.Left;
        }
        else if (ZombiesLeft == 10)
        {
            loc = ZombieLocation.Middle;
            resLoc = ZombieLocation.Left;
        }
        else if (ZombiesLeft == 9)
        {
            loc = ZombieLocation.Right;
            resLoc = ZombieLocation.Middle;
        }
        else if (ZombiesLeft == 8)
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

        if (ZombiesLeft == 8)
        {
            loc = ZombieLocation.FarLeft;
            resLoc = ZombieLocation.FarLeft;
        }
        else if (ZombiesLeft == 7)
        {
            loc = ZombieLocation.Far;
            resLoc = ZombieLocation.FarLeft;
        }
        else if (ZombiesLeft == 6)
        {
            loc = ZombieLocation.FarRight;
            resLoc = ZombieLocation.Far;
        }
        else if (ZombiesLeft == 5)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage3Complete");
            Cur_State = (int)State.Stage4;
            return;
        }
        CheckBowShots();
    }

    // Check if need to respawn zombie
    // If not, spawn new zombie
    void CheckBowShots()
    {
        if (Bow.arrowsShot >= maxShots)
            enemManager.RespawnPrevious(resLoc);
        else
            enemManager.Spawn(loc, zombieMov, false);
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
            enemManager.Spawn(loc, zombieMov, false);
        }
        if (ZombiesLeft == 0)
        {
            Cur_State = (int)State.FreePlay;
        }



        //    anim.SetTrigger("Stage4Complete");


    }

    // Free play mode where zombies get progressively stronger and faster
    void FreePlay()
    {
        // Spawn zombie every 3 seconds
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            enemManager.Spawn(loc, zombieMov, true);

            // Reset timer
            spawnTimer = 0f;
        }

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
        statsText.text = "Hits: " + arrowHits + "\nMisses: " + misses + "\nHit %: " + hit_pct + "\nBody Shots: " + bodyShots + "\nHead Shots: " + headShots;
    }

}
