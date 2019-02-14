using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public Text stageText;              // Displays current stage of tutorial stage
    public Text gameStartText;          // Displays start message
    public Text gameOverText;           // Displays game over 
    public Text zombiesLeftText;        // Displays number of zombies left
    public Text timeText;               // Displays survival time
    public Image startImage;            // Background image
    public float restartDelay = 2f;     // How long it takes before we restart the game
    public enum ZombieLocation { Near, Middle, Far, Left, Right, FarLeft, FarRight }; // Locations to spawn zombies
    public EnemyManager enemManager;    // Reference to enemy manager script
    public bool zombieMov = false;      // Indicates whether the zombie to be spawned can move
    public int maxShots = 5;            // Maximum shots a player can take before going back a stage
    public Text pauseText;              // Indicates game is paused
    public Text hitText;                // Displays "Hit!" when zombie is hit
    public Text statsText;              // Displays user's statistics
    public ZombieLocation loc;          // Where to spawn zombie
    public ZombieLocation resLoc;       // Where to respawn zombie
    public float zombieSpeed = 0.001f;  // Speed of the zombies
    public PlayerHealth playerHealth;   // Get player's health
    public Transform player;            // Reference to transform of the player

    public static int Cur_State;        // Indicates current state of tutorial scene
    public static int arrowHits = 0;    // Number of arrow hits
    public static int arrowFires = 0;   // Number of arrow fires
    public static int bodyShots = 0;    // Number of body shots
    public static int headShots = 0;    // Number of head shots
    public static int ZombiesLeft;      // Number of zombies left for tutorial stage
    public static int zombiesDestroyed = 0; // Total number of zombies destroyed
    public static int tutArrowFires = 0;    // Number of arrow fires in tutorial

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private enum State { GameStart, Stage1, Stage2, Stage3, Multiplayer, FreePlay, GameOver };  // Different states for gameplay
    private float restartTimer;         // After game is over, time before scene is reloaded
    private float spawnTimer;           // Timer for spawning zombies
    private float spawnTime = 4f;       // Time between zombie spawns in free play mode
    private float capacityTimer;        // Timer for increasing capacity of zombies in scene
    private float capacityTime = 5f;    // Time between increasing capacity of zombies
    private float startTimer;           // Timer for staying in gamestart state
    private float startTime = 3f;       // Time to stay in gamestart state, needed to fade screen
    private float narrativeTimer;       // Timer for staying in narrative function
    private float narrativeTime = 34f;  // Time to stay in narrative function
    private float MLTimer;              // Timer for when to call machine learning function
    private float MLTime = 20f;         // How often to call machine learning function
    private float refGlobalTime;        // Taking the difference between this and curGlobalTime yields time elapsed
    private bool start = false;         // Ensures that game does not start until the player presses return
    private bool narrativeDone = false; // Indicates the narrative is done playing
    private int textYPos = -600;        // Start position of narrative text

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
        // Start game in gamestart state
        Cur_State = (int)State.GameStart;

        // 9 total zombies to kill in tutorial stage
        ZombiesLeft = 9;

        // Get reference to the player
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Initialize all timers to 0
        restartTimer = 0f;
        spawnTimer = 0f;
        capacityTimer = 0f;
        startTimer = 0f;
        narrativeTimer = 0f;
        MLTimer = 0f;

        // Initialize location
        loc = ZombieLocation.Near;
        resLoc = ZombieLocation.Near;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {

        // If player is dead, end game
        if (playerHealth.currentHealth <= 0)
        {
            Cur_State = (int)State.GameOver;
        }


        // Enter game mode based on current state
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
            case (int)State.Multiplayer:
                Multiplayer();
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

        // Display current time
        if (Cur_State != (int)State.GameStart)
        {
            DisplayTime();
        }

        // Can skip tutorial stages by pressing 'k' key
        if (Cur_State == (int)State.Stage1 || Cur_State == (int)State.Stage2 || Cur_State == (int)State.Stage3)
        {
            if (Input.GetKeyDown("k"))
            {
                enemManager.DestroyAllZombies();
                refGlobalTime = Time.time;
                Cur_State = (int)State.FreePlay; 
            }
        }

        // Can go back to tutorial stage by pressing 'k' key
        if (Cur_State == (int)State.FreePlay)
        {
            if (Input.GetKeyDown("l"))
            {
                enemManager.DestroyAllZombies();
                refGlobalTime = Time.time;
                Cur_State = (int)State.Stage1;
            }
        }

        // Display stage text and zombies left text
        DisplayStage();

    }

    //////////////////////////////////////////////////////////////////////////////////
    // Game Start Screen and Auxillary Animation Functions
    //////////////////////////////////////////////////////////////////////////////////
    void GameStart()
    {
        // Begins narrative when return button is pressed or the user says "start"
        if (Input.GetKeyDown("return") || !UDPInterface.isPaused)
        {
            start = true;
        }

        // Narrative ends narrativeTime later or when the return button is pressed
        if (start == true && narrativeDone == false)
        {
            NarrativeAnims();
            narrativeTimer += Time.deltaTime;
            if (narrativeTimer >= narrativeTime || Input.GetKeyDown("return"))
            {
                narrativeDone = true;
            }
        }

        // When narrative ends, turn off graphics by calling GameStartAnims
        if (narrativeDone == true)
        {

            GameStartAnims();
            startTimer += Time.deltaTime;
            if (startTimer >= startTime)
            {
                refGlobalTime = Time.time;
                Cur_State = (int)State.Stage1;
                // Reset timer and start
                startTimer = 0f;
                start = false;
            }
        }
    }

    void NarrativeAnims()
    {
        gameStartText.fontSize = 40;
        gameStartText.lineSpacing = 1.5f;

        Vector3 temp = gameStartText.rectTransform.position;
        temp.y = textYPos;
        gameStartText.rectTransform.position = temp;

        gameStartText.alignment = TextAnchor.MiddleLeft;
        gameStartText.text = "The year is 3019 and the CDC developed a new superbug for biological warfare." +
            " Unfortunately, the antidote to this superbug is extremely hard to develop and is therefore protected" +
            " in a secret underground vault in Area 51 making it notoriously hard to access.\n\n" +

            "One day, an evil scientist who had enough of humanity’s wickedness decided to release the superbug to the populace. It instantly infected" +
            " anyone who came into contact with it and rapidly spread across the globe. To the scientist’s surprise, however," +
            " everyone survived this superbug and turned into zombies who had a craving for human flesh.\n\n" +

            "Only a handful of humans survived the zombie onslaught. Now it is up to them to travel to the vault and acquire the antidote to revert" +
            " all the zombies back to humans. But they best beware; hordes of zombies will stop at nothing to keep them from the cure! " +
            "The fate of humanity is in their hands…\n";

        textYPos += 1;

    }
    void GameStartAnims()
    {
        // Turn off start messsage
        gameStartText.color = Color.Lerp(gameStartText.color, Color.clear, Time.deltaTime);

        // Turn off start image
        startImage.color = Color.Lerp(startImage.color, Color.clear, Time.deltaTime);

        // Turn on stage text
        Color temp = stageText.color;
        temp.a = 1f;
        stageText.color = Color.Lerp(stageText.color, temp, Time.deltaTime);

        // Turn on zombies left text
        temp = zombiesLeftText.color;
        temp.a = 1f;
        zombiesLeftText.color = Color.Lerp(zombiesLeftText.color, temp, Time.deltaTime);

        // Turn on stats text
        temp = statsText.color;
        temp.a = 1f;
        statsText.color = Color.Lerp(statsText.color, temp, Time.deltaTime);

        // Turn on time text
        temp = timeText.color;
        temp.a = 1f;
        timeText.color = Color.Lerp(timeText.color, temp, Time.deltaTime);

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Tutorial Stages
    //////////////////////////////////////////////////////////////////////////////////
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
            refGlobalTime = Time.time;
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

    ////////////////////////////////////////////////////////////////////////////////// 
    // Multiplayer Mode
    //////////////////////////////////////////////////////////////////////////////////
    // Spawns zombies in 4 different locations
    void Multiplayer()
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
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Survival Mode
    //////////////////////////////////////////////////////////////////////////////////
    // Free play mode where zombies get progressively stronger and faster
    void FreePlay()
    {
        zombieMov = true;

        // Spawn zombie every spawnTime seconds
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {

            enemManager.Spawn(loc, zombieMov, true, zombieSpeed);

            // Reset timer
            spawnTimer = 0f;
        }


        // Change the speed of each zombie active in the scene
        //enemManager.setSpeed(zombieSpeed);

        /*capacityTimer += Time.deltaTime;
        if (capacityTimer >= capacityTime)
        {
            enemManager.incMaxActiveZombies();
            capacityTimer = 0f;
        }*/

        MLTimer += Time.deltaTime;
        if (MLTimer >= MLTime)
        {
            AdjustZombieSpeed(0.001f);
            AdjustSpawnTime(0f);
            enemManager.incMaxActiveZombies();

            //Reset timer
            MLTimer = 0f;
        }
        // Check if we should go back to tutorial stage
        /*if (ML Condition)
        {
            enemManager.DestroyAllZombies();
            refGlobalTime = Time.time;
            Cur_State = (int)State.Stage1;
            ZombiesLeft = 9;
        }*/

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Change Game Parameters
    //////////////////////////////////////////////////////////////////////////////////
    void AdjustZombieSpeed(float amt)
    {
        // Ensure zombie speed does not fall below 0
        if (zombieSpeed + amt <= 0)
        {
            return;
        }
        // Increment zombie speed
        zombieSpeed += amt;

    }

    void AdjustSpawnTime(float amt)
    {
        // Ensure spawn time does not fall below 0
        if (spawnTime + amt <= 0)
        {
            return;
        }
        spawnTime += amt;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Game Over Screen and Auxillary Animation Functions
    //////////////////////////////////////////////////////////////////////////////////
    // Reloads the game after restartDelay has elapsed
    void GameOver()
    {
        // Begin Timer
        restartTimer += Time.deltaTime;
        GameOverAnims();
        // Once restartDelay has elapsed
        if (restartTimer >= restartDelay)
        {
            // Load currently loaded level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void GameOverAnims()
    {
        // Turn off zombies left text
        zombiesLeftText.color = Color.Lerp(zombiesLeftText.color, Color.clear, Time.deltaTime);
        stageText.color = Color.Lerp(stageText.color, Color.clear, Time.deltaTime);
       
        Color temp = gameOverText.color;
        temp.a = 1f;
        gameOverText.color = Color.Lerp(gameOverText.color, temp, Time.deltaTime);

        temp = startImage.color;
        temp.a = 1f;
        startImage.color = Color.Lerp(startImage.color, temp, Time.deltaTime);

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Check if Player Pauses Game
    //////////////////////////////////////////////////////////////////////////////////
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

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Survival Time
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayStage()
    {
        if (Cur_State != (int)State.FreePlay)
        {
            stageText.text = "Tutorial Stage " + Cur_State + "/3";
            zombiesLeftText.text = "Zombies Left: " + ZombiesLeft;
        }
        else
        {
            stageText.text = "Survival Mode";
            zombiesLeftText.text = "Active Zombies: " + EnemyManager.activeZombies;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Survival Time
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayTime()
    {
        // Compute time elapsed
        int timeElapsed = (int)(Time.time - refGlobalTime);

        // Holds game mode
        string mode;

        // Get game mode
        if (Cur_State == (int)State.FreePlay)
        {
            mode = "Survival Time: ";

        }
        else 
        {
            mode = "Tutorial Time: ";
        }

        // Display current time
        timeText.text = mode + timeElapsed + " seconds";
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Hit Text
    //////////////////////////////////////////////////////////////////////////////////
    public void DisplayHit(string t)
    {
        hitText.text = t;
        Color temp = hitText.color;
        temp.a = 1;
        hitText.color = temp;
        Invoke("HitTextOff", 1f);
    }

    private void HitTextOff()
    {
        Color temp = hitText.color;
        temp.a = 0;
        hitText.color = temp;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Player Statistics
    //////////////////////////////////////////////////////////////////////////////////
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
