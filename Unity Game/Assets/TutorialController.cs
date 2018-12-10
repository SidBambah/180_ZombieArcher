using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

// Description: 
// 
public class TutorialController : MonoBehaviour
{

    // Public variables
    public int Cur_State;    // Indicates current state of tutorial scene
    public Animator anim;    // Reference to UI animator
    public Text stageText;   // Displays current stage of tutorial stage
    public Text zombiesLeftText; // Displays number of zombies left
    public float restartDelay = 5f;   // How long it takes before we restart the game
    public enum ZombieLocation { Near, Middle, Far, Left, Right, FarLeft, FarRight }; // Locations to spawn zombies
    public EnemyManager enemManager;  // Reference to enemy manager script
    public bool zombieMov = false;    // Indicates whether the zombie to be spawned can move
    public int maxShots = 5;          // Maximum shots a player can take before going back a stage
    public Text pauseText;

    Transform player;

    // Private variables
    private enum State { GameStart, Stage1, Stage2, Stage3, Stage4, GameOver };  // Different states for tutorial 
    private float restartTimer;       // After game is over, time before scene is reloaded


    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        restartTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {

        // Display stage number
        stageText.text = "Stage " + Cur_State + "/4";

        // Display number of zombies left
        zombiesLeftText.text = "Zombies Left: " + EnemyManager.ZombiesLeft;

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
            case (int)State.GameOver:
                GameOver();
                break;
        }


        CheckIfPaused();
        CheckIfContinued();
    }


    // Game start screen
    // Begins stage 1 when the return button is pressed or the user says "start"
    void GameStart()
    {
        if (Input.GetKeyDown("return") || PositionManipulate.isStart)
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
        if (EnemyManager.ZombiesLeft == 14)
        {
            enemManager.Spawn(ZombieLocation.Near, zombieMov);
        }
        // Middle zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn near zombie again
        else if (EnemyManager.ZombiesLeft == 13)
        {
            enemManager.Spawn(ZombieLocation.Middle, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Middle);
            }
        }
        // Far zombie: 5 tries, if you fail to kill zombie
        // then destroy current zombie and spawn middle zombie again
        else if (EnemyManager.ZombiesLeft == 12)
        {
            enemManager.Spawn(ZombieLocation.Far, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Far);
            }
        }
        // If you successfully kill all zombies, call Stage2 function
        else if (EnemyManager.ZombiesLeft == 11)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage1Complete");
            Cur_State = (int)State.Stage2;
        }

    }

    // Same logic as Stage1, except zombies spawned horizontally
    void Stage2()
    {
        zombieMov = false;

        if (EnemyManager.ZombiesLeft == 11)
        {
            enemManager.Spawn(ZombieLocation.Left, zombieMov);

        }
        else if (EnemyManager.ZombiesLeft == 10)
        {
            enemManager.Spawn(ZombieLocation.Middle, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Middle);
            }
        }
        else if (EnemyManager.ZombiesLeft == 9)
        {
            enemManager.Spawn(ZombieLocation.Right, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Right);
            }
        }
        else if (EnemyManager.ZombiesLeft == 8)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage2Complete");
            Cur_State = (int)State.Stage3;
        }
    }

    // Spawn zombies horizontally and allow them to move
    void Stage3()
    {
        // Zombies can move in this stage
        zombieMov = true;

        if (EnemyManager.ZombiesLeft == 8)
        {
            enemManager.Spawn(ZombieLocation.Left, zombieMov);

        }
        else if (EnemyManager.ZombiesLeft == 7)
        {
            enemManager.Spawn(ZombieLocation.Middle, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Middle);
            }
        }
        else if (EnemyManager.ZombiesLeft == 6)
        {
            enemManager.Spawn(ZombieLocation.Right, zombieMov);
            if (Bow.arrowsShot == maxShots)
            {
                enemManager.RespawnPrevious(ZombieLocation.Right);
            }
        }
        else if (EnemyManager.ZombiesLeft == 5)
        {
            // Cause transition in UI animator
            anim.SetTrigger("Stage3Complete");
            Cur_State = (int)State.Stage4;
        }
    }

    // Spawns zombies in 4 different locations
    void Stage4()
    {
        zombieMov = true;
        if (PositionManipulate.isValidQuadrant || true)
        {
            if (PositionManipulate.spawnQuadrant == 1 || Input.GetKeyDown("u"))
                enemManager.Spawn(ZombieLocation.FarLeft, zombieMov);
            else if (PositionManipulate.spawnQuadrant == 2 || Input.GetKeyDown("i"))
                enemManager.Spawn(ZombieLocation.FarRight, zombieMov);
            else if (PositionManipulate.spawnQuadrant == 3 || Input.GetKeyDown("j"))
                enemManager.Spawn(ZombieLocation.Left, zombieMov);
            else if (PositionManipulate.spawnQuadrant == 4 || Input.GetKeyDown("k"))
                enemManager.Spawn(ZombieLocation.Right, zombieMov);
            PositionManipulate.isValidQuadrant = false;
        }
        if (EnemyManager.ZombiesLeft == 0)
            anim.SetTrigger("Stage4Complete");


    }

    // Reloads the game after restartDelay has epalsed
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
        if (PositionManipulate.isPaused || Input.GetKeyDown("y"))
        {
            PositionManipulate.moveBowValid = false;
            if (Cur_State == (int)State.Stage3 || Cur_State == (int)State.Stage4)
            {
                //Transform t = EnemyManager.currentZombie.GetComponent<Transform>();
                //EnemyManager.currentZombie.GetComponent<ZombieMovement>().nav.SetDestination(t.position);
                EnemyManager.currentZombie.GetComponent<NavMeshAgent>().updatePosition = false;
            }

            pauseText.enabled = true;
        }
    }

    void CheckIfContinued()
    {
        if (PositionManipulate.isStart || Input.GetKeyDown("return"))
        {
            PositionManipulate.moveBowValid = true;

            if (Cur_State == (int)State.Stage4 || Cur_State == (int)State.Stage4)
            {
                //EnemyManager.currentZombie.GetComponent<ZombieMovement>().nav.SetDestination(player.position);
                EnemyManager.currentZombie.GetComponent<NavMeshAgent>().updatePosition = true;
            }
            pauseText.enabled = false;
        }
    }

}
