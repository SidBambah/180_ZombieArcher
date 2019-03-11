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
    public Text streakText;             // Displays kill streak
    public Text arrowsLeftText;
    public Text feedbackText;
    public Text nukeText;
    public Text tutorialText;
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
    private float zombieSpeed = 1f;   // Speed of the zombies
    public PlayerHealth playerHealth;   // Get player's health
    public Transform player;            // Reference to transform of the player
    public MachineLearning ML;
    public int maxArrows = 3;
    public static int arrowsLeft = 3;   // Number of arrows
    public RawImage[] Arrows;
    public RawImage[] Nukes;
    public AudioClip nukeClip;
    public AudioClip meleeClip;
    public AudioClip reloadClip;
    public Demos demos;

    public static int Cur_State;        // Indicates current state of tutorial scene
    public static int arrowHits = 0;    // Number of arrow hits
    public static int ZombiesLeft;      // Number of zombies left for tutorial stage
    public static int zombiesDestroyed = 0; // Total number of zombies destroyed
    public static int tutArrowFires = 0;    // Number of arrow fires in tutorial
    public static int killStreak = 0;   // Number of zombies killed in survival mode

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private enum State { GameStart, Stage1, Stage2, Stage3, Stage4,
                         Stage5, Stage6, Stage7, Multiplayer, FreePlay, GameOver };  // Different states for gameplay
    private float restartTimer;         // After game is over, time before scene is reloaded
    private float spawnTimer;           // Timer for spawning zombies
    private float spawnTime = 4f;       // Time between zombie spawns in free play mode
    private float startTimer;           // Timer for staying in gamestart state
    private float startTime = 3f;       // Time to stay in gamestart state, needed to fade screen
    private float narrativeTimer;       // Timer for staying in narrative function
    private float narrativeTime = 34f;  // Time to stay in narrative function
    private float MLTimer;              // Timer for when to call machine learning function
    private float MLTime = 30f;         // How often to call machine learning function
    private float refGlobalTime;        // Taking the difference between this and curGlobalTime yields time elapsed
    private bool start = false;         // Ensures that game does not start until the player presses return
    private bool narrativeDone = false; // Indicates the narrative is done playing
    private int textYPos = -600;        // Start position of narrative text
    private int killStreakThres = 3;    // Player's kill streak in survival mode
    private int killStreakDefault = 3;  // Default kill streak threshold
    private int powerupsAvailable = 0;  // Number of nukes player has
    private int maxPowerups = 3;
    private int saveCSV = 60;
    private int numReloads = 0;
    private int numMelee = 0;
    private int numNukesTut = 0;
    private bool tutorialStage = false;
    private int startState;
    

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start()
    {
        // Start game in gamestart state
        Cur_State = (int)State.GameStart;

        // 9 total zombies to kill in tutorial stage
        ZombiesLeft = 9;

        // Get reference to machine learning script
        ML = GameObject.FindWithTag("MachineLearning").GetComponent<MachineLearning>();

        // Get reference to the player
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Initialize all timers to 0
        restartTimer = 0f;
        spawnTimer = 0f;
        startTimer = 0f;
        narrativeTimer = 0f;
        MLTimer = 0f;

        // Initialize location
        loc = ZombieLocation.Near;
        resLoc = ZombieLocation.Near;


        // Display statistics
        //DisplayStats();

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
            case (int)State.Stage4:
                Stage4();
                break;
            case (int)State.Stage5:
                Stage5();
                break;
            case (int)State.Stage6:
                Stage6();
                break;
            case (int)State.Stage7:
                Stage7();
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

        // Display Text Boxes
        DisplayUIText();

        // Check if paused
        if (Cur_State != (int)State.GameStart)
        {
            CheckIfPaused();
        }

        // Check if in tutorial
        if (Cur_State == (int)State.Stage1 || Cur_State == (int)State.Stage2 || Cur_State == (int)State.Stage3
                 || Cur_State == (int)State.Stage4 || Cur_State == (int)State.Stage5 || Cur_State == (int)State.Stage6
                 || Cur_State == (int)State.Stage7)
        {
            tutorialStage = true;
        }
        else 
        {
            tutorialStage = false;
        }

        // Can skip tutorial stages by pressing 'g' key (for testing)
        if (tutorialStage == true)
        {
            if (Input.GetKeyDown("g"))
            {
                enemManager.DestroyAllZombies();
                refGlobalTime = Time.time;
                killStreak = 0;
                killStreakThres = killStreakDefault;
                Cur_State = (int)State.FreePlay;
            }
        }

        // Can go back to tutorial stage by pressing 'l' key (for testing)
        if (Cur_State == (int)State.FreePlay)
        {
            if (Input.GetKeyDown("h"))
            {
                enemManager.DestroyAllZombies();
                refGlobalTime = Time.time;
                Cur_State = (int)State.Stage1;
            }
        }


        // Reload the arrows if the player reloads
        if (UDPInterface.reload || Input.GetKeyDown("r"))
        {
            GetComponent<AudioSource>().PlayOneShot(reloadClip);
            arrowsLeft = maxArrows;
            numReloads += 1;
        }

        // Perform melee 
        if (UDPInterface.melee || Input.GetKeyDown("n"))
        {
            GetComponent<AudioSource>().PlayOneShot(meleeClip);
            numMelee += 1;
            enemManager.Melee();
        }



        // FOR TESTING
        if (UDPInterface.melee)
             DisplayHit("MELEE");

         if (UDPInterface.reload)
             DisplayHit("RELOAD");

        if (UDPInterface.speech == "play" && UDPInterface.validSpeech == true)
            DisplayHit("PLAY");

        if (UDPInterface.speech == "pause" && UDPInterface.validSpeech == true)
            DisplayHit("PAUSE");

        if (UDPInterface.speech == "kill" && UDPInterface.validSpeech == true)
            DisplayHit("KILL ZOMBIES");
        if ((UDPInterface.speech == "show" && UDPInterface.validSpeech == true) || Input.GetKeyDown("1"))
        {
            DisplayHit("SHOW STATS");
            DisplayStats();
        }

        /*if (UDPInterface.validQuadrant)
        {
            if (UDPInterface.spawnQuadrant == "Q1" || Input.GetKeyDown("u"))
                DisplayHit("Q1");
            else if (UDPInterface.spawnQuadrant == "Q2" || Input.GetKeyDown("i"))
                DisplayHit("Q2");
            else if (UDPInterface.spawnQuadrant == "Q3" || Input.GetKeyDown("j"))
                DisplayHit("Q3");
            else if (UDPInterface.spawnQuadrant == "Q4" || Input.GetKeyDown("k"))
                DisplayHit("Q4");
            Debug.Log("Quadrant " + UDPInterface.spawnQuadrant);
        }*/

    }

    //////////////////////////////////////////////////////////////////////////////////
    // Game Start Screen and Auxillary Animation Functions
    //////////////////////////////////////////////////////////////////////////////////
    void GameStart()
    {
        // Begins narrative when return button is pressed or the user says "start"
        if ((UDPInterface.speech == "play" && UDPInterface.validSpeech == true) || Input.GetKeyDown("return"))
        {
            start = true;
            startState = (int)State.Stage1;
            
        }
        if (Input.GetKeyDown("m"))
        {
            start = true;
            startState = (int)State.Multiplayer;
        }


        // Narrative ends narrativeTime later or when the return button is pressed
        // Calls narrativeAnims to display narrative
        if (start == true && narrativeDone == false)
        {
            NarrativeAnims();
            narrativeTimer += Time.deltaTime;
            if (narrativeTimer >= narrativeTime || Input.GetKeyDown("l"))
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
                Cur_State = startState;
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
        /*temp = statsText.color;
        temp.a = 1f;
        statsText.color = Color.Lerp(statsText.color, temp, Time.deltaTime);*/

        // Turn on time text
        temp = timeText.color;
        temp.a = 1f;
        timeText.color = Color.Lerp(timeText.color, temp, Time.deltaTime);

        // Turn on streak text
        temp = streakText.color;
        temp.a = 1f;
        streakText.color = Color.Lerp(streakText.color, temp, Time.deltaTime);

        // Turn on arrows text
        temp = arrowsLeftText.color;
        temp.a = 1f;
        arrowsLeftText.color = Color.Lerp(arrowsLeftText.color, temp, Time.deltaTime);

        // Turn on feedback text
        temp = feedbackText.color;
        temp.a = 1f;
        feedbackText.color = Color.Lerp(feedbackText.color, temp, Time.deltaTime);

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Tutorial Stages
    //////////////////////////////////////////////////////////////////////////////////
     
    // Practice holding the arrow, aiming, and shooting
    void Stage1()
    {
        DisplayTutorialText("Launch three arrows to advance to the next stage. ");
        if (arrowsLeft <= 0)
            Cur_State = (int)State.Stage2;

    }
    // Practice gestures
    void Stage2()
    {
        DisplayTutorialText("Perform three reload gestures to advance to the next stage. ");
        if (numReloads >= 3)
            Cur_State = (int)State.Stage3;
    }
    void Stage3()
    {
        DisplayTutorialText("Perform three melee gestures to advance to the next stage. ");
        if (numMelee >= 3)
            Cur_State = (int)State.Stage4;

    }
    // Practice saying keywords
    void Stage4()
    {
        DisplayTutorialText("Say the phrase \"kill zombies\" three times to advance to the next stage. ");
        if ((UDPInterface.speech == "kill" && UDPInterface.validSpeech == true) || Input.GetKeyDown("p"))
            numNukesTut += 1;

        if (numNukesTut >= 3)
            Cur_State = (int)State.Stage5;

    }

    // Spawn zombie one at a time at varying (increasing) distances
    void Stage5()
    {
        DisplayTutorialText("Destroy zombies at various locations to hone your craft. ");
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
            Cur_State = (int)State.Stage6;
            return;
        }

        CheckBowShots();
    }

    // Same logic as Stage1, except zombies spawned horizontally
    void Stage6()
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
            Cur_State = (int)State.Stage7;
            return;
        }

        CheckBowShots();

    }

    // Spawn zombies horizontally and allow them to move
    void Stage7()
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
            killStreak = 0;
            killStreakThres = killStreakDefault;
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
            enemManager.Spawn(loc, zombieMov, 0, zombieSpeed);
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Multiplayer Mode
    //////////////////////////////////////////////////////////////////////////////////
    // Spawns zombies in 4 different locations
    void Multiplayer()
    {
        enemManager.maxActiveZombies = 5;
        zombieMov = true;
        bool a = UDPInterface.testing;
        if (UDPInterface.validQuadrant || a)
        {
            if ((UDPInterface.spawnQuadrant == "Q1" && !a) || Input.GetKeyDown("u"))
            {
                loc = ZombieLocation.FarLeft;
                enemManager.Spawn(loc, zombieMov, 2, zombieSpeed);
            }
            else if ((UDPInterface.spawnQuadrant == "Q2" && !a) || Input.GetKeyDown("i"))
            {
                loc = ZombieLocation.FarRight;
                enemManager.Spawn(loc, zombieMov, 2, zombieSpeed);
            }
            else if ((UDPInterface.spawnQuadrant == "Q3" && !a) || Input.GetKeyDown("j"))
            {
                loc = ZombieLocation.Left;
                enemManager.Spawn(loc, zombieMov, 2, zombieSpeed);
            }
            else if ((UDPInterface.spawnQuadrant == "Q4" && !a) || Input.GetKeyDown("k"))
            {
                loc = ZombieLocation.Right;
                enemManager.Spawn(loc, zombieMov, 2, zombieSpeed);
            }

        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Survival Mode
    //////////////////////////////////////////////////////////////////////////////////
    // Free play mode where zombies speed, spawn frequency, and the number of active 
    // zombies is varied according to the player's skill
    // Player can get demoted back to tutorial stage
    void FreePlay()
    {
        DisplayTutorialText("");
        // Allow zombies to move
        zombieMov = true;

        // Spawn zombie every spawnTime seconds
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {

            enemManager.Spawn(loc, zombieMov, 1, zombieSpeed);

            // Reset timer
            spawnTimer = 0f;
        }


        // Set back to false
        bool repeatTutorial = false;

        // Every MLTimer seconds, make a decision about the player's skill
        MLTimer += Time.deltaTime;
        if (MLTimer >= MLTime)
        {
            repeatTutorial = ML.statsReact(ML.name, ML.dbPath);

            //Reset timer
            MLTimer = 0f;
        }

        // Check if we should go back to tutorial stage
        if (repeatTutorial == true)
        {
            enemManager.DestroyAllZombies();
            refGlobalTime = Time.time;
            Cur_State = (int)State.Stage1;
            ZombiesLeft = 9;
        }

        // Check if you unlocked a powerup
        if (killStreak >= killStreakThres)
        {
            // Unlock a nuke and increment threshold
            if (powerupsAvailable < maxPowerups)
            {
                powerupsAvailable += 1;
                killStreakThres += 3;
                DisplayPowerUpUnlock("You unlocked a nuke!\n Say \"Kill Zombies\" to use it!");
            }
            else
            {
                killStreakThres += 1;
            }

        }

        // Check if the player wants to use the a power up
        if ((UDPInterface.speech == "kill" && UDPInterface.validSpeech == true) || Input.GetKeyDown("p"))
        {
            if (powerupsAvailable >= 1)
            {

                // Play sound effect
                GetComponent<AudioSource>().PlayOneShot(nukeClip);

                // Play animations for the explosions
                enemManager.BlowUpZombies();

                // Destroy all zombie game objects
                enemManager.DestroyAllZombies();

                // Decrement number of power ups
                powerupsAvailable -= 1;

                // Increment zombie kill total, but don't increment killstreak
                zombiesDestroyed += EnemyManager.activeZombies;

            }
        }

    }


    ////////////////////////////////////////////////////////////////////////////////// 
    // Change Game Parameters
    //////////////////////////////////////////////////////////////////////////////////
    /// Change each zombie's speed
    public void AdjustZombieSpeed(float amt)
    {
        // Ensure zombie speed does not fall below 0
        if (zombieSpeed + amt <= 0)
        {
            return;
        }
        // Increment zombie speed
        zombieSpeed += amt;

        // Set all current zombie speeds
        enemManager.SetSpeed(zombieSpeed);

    }

    // Change zombie spawn frequency
    public void AdjustSpawnTime(float amt)
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
        if ((UDPInterface.speech == "pause" && UDPInterface.validSpeech == true) || Input.GetKeyDown("t"))
        {
            UDPInterface.moveBowValid = false;
            pauseText.enabled = true;
        }
        else if ((UDPInterface.speech == "play" && UDPInterface.validSpeech == true) || Input.GetKeyDown("y"))
        {
            UDPInterface.moveBowValid = true;
            pauseText.enabled = false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Calls Display Functions
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayUIText()
    {
        // Display stage text and zombies left text
        DisplayStage();

        // Display kill streak
        DisplayStreak();

        // Display current time
        DisplayTime();

        // Display arrows left
        //DisplayItemsLeft();

        // Display to reload arrow
        DisplayFeedback();

        // Display arrow textures
        DisplayArrowUI(Arrows, arrowsLeft);

        // Display nuke textures
        DisplayArrowUI(Nukes, powerupsAvailable);

    }
    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Current Stage and Zombies 
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayStage()
    {
        if (tutorialStage == true)
        {
            stageText.text = "Tutorial Stage " + Cur_State + "/7";
            zombiesLeftText.text = "Zombies Left: " + ZombiesLeft;
        }
        else if (Cur_State == (int)State.FreePlay)
        {
            stageText.text = "Survival Mode";
            zombiesLeftText.text = "Active Zombies: " + EnemyManager.activeZombies;
        }
        else if (Cur_State == (int)State.Multiplayer)
        {
            stageText.text = "Multiplayer Mode";
            zombiesLeftText.text = "Active Zombies " + EnemyManager.activeZombies;
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Kill Streak
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayStreak()
    {
        int streak = 0;

        if (tutorialStage == true)
        {
            streak = 0;
        }
        else if (Cur_State == (int)State.FreePlay)
        {
            streak = killStreak;
        }
        else if (Cur_State == (int)State.Multiplayer)
        {
            streak = 0;
        }

        streakText.text = "Kill Streak: " + streak;

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Survival Time
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayTime()
    {
        // Compute time elapsed
        int timeElapsed = (int)(Time.time - refGlobalTime);

        // Holds game mode
        string mode = "";

        if (tutorialStage == true)
        {
            mode = "Tutorial Time: ";
            timeText.text = mode + timeElapsed + " seconds";
        }
        else if (Cur_State == (int)State.FreePlay)
        {
            mode = "Survival Time: ";
            timeText.text = mode + timeElapsed + " seconds";
        }
        else if (Cur_State == (int)State.Multiplayer)
        {
            mode = "Multiplayer Time: ";
            timeText.text = mode + timeElapsed + " seconds";
        }




        // TODO: Adjust when to save decision history to textfile
        if (timeElapsed >= saveCSV)
        {
            ML.Save();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Items Left
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayItemsLeft()
    {
        arrowsLeftText.text = "Arrows Available: " + arrowsLeft + "\nNukes Available: " + powerupsAvailable;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays to Reload
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayFeedback()
    {
        string t;
        if (arrowsLeft <= 0)
        {
            t = "Reload your quiver!";
        }
        else
        {
            t = "";
        }
        feedbackText.text = t;

    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays arrow textures
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayArrowUI(RawImage[] im, int num)
    {
        Color tmp = im[0].color;

        if (num == 0)
        {
            tmp.a = 0;
            for (int k = 0; k < 3; k++)
                im[k].color = tmp;

        }
        else if (num == 1)
        {
            tmp.a = 1;
            im[0].color = tmp;
            tmp.a = 0;
            im[1].color = tmp;
            im[2].color = tmp;


        
        }
        else if (num == 2)
        {
            tmp.a = 1;
            im[0].color = tmp;
            im[1].color = tmp;
            tmp.a = 0;
            im[2].color = tmp;
        }
        else if (num == 3)
        {
            tmp.a = 1;
            for (int k = 0; k < 3; k++)
                im[k].color = tmp;
        }

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

        // Allow hit text to show for 1 second
        Invoke("HitTextOff", 1f);
    }

    private void HitTextOff()
    {
        Color temp = hitText.color;
        temp.a = 0;
        hitText.color = temp;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Nuke Unlocked
    //////////////////////////////////////////////////////////////////////////////////
    public void DisplayPowerUpUnlock(string t)
    {
        nukeText.text = t;
        Color temp = nukeText.color;
        temp.a = 1f;
        temp.g = 0f;
        temp.b = 0f;
        nukeText.color = temp;

        // Allow hit text to show for 1 second
        Invoke("NukeTextOff", 4f);
    }

    private void NukeTextOff()
    {
        Color temp = nukeText.color;
        temp.a = 0;
        nukeText.color = temp;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Player Statistics
    //////////////////////////////////////////////////////////////////////////////////
    public void DisplayStats()
    {
        double[] stats = ML.getPercents(ML.playerName, ML.dbPath);

        double hits = stats[4] + stats[5];
        string hit_pct = (stats[0] * 100f).ToString("F2");
        string headshot_pct = (stats[2] * 100f).ToString("F2");

        statsText.text = "Statistics for " + ML.playerName + ":\nHits: " + hits + "\nMisses: " + stats[3] + "\nHit %: " + hit_pct + "\nBody Shots: " + stats[5] + "\nHead Shots: " + stats[4]
            + "\nHeadshot %: " + headshot_pct + "\nZombies Destroyed: " + zombiesDestroyed;


        Color temp = statsText.color;
        temp.a = 1;
        statsText.color = temp;
        Invoke("StatsTextOff", 4f);
    }

    private void StatsTextOff()
    {
        Color temp = statsText.color;
        temp.a = 0;
        statsText.color = temp;
    }


    ////////////////////////////////////////////////////////////////////////////////// 
    // Displays Tutorial Instructions
    //////////////////////////////////////////////////////////////////////////////////
    private void DisplayTutorialText(string s)
    {
        tutorialText.text = s;
    }


    public int GetState()
    {
        return Cur_State;
    }
}

