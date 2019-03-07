using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class MachineLearning : MonoBehaviour
{

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public GameController tutCont;      // Reference to the tutorial controller
    public EnemyManager enemManager;    // Reference to enemy manager script
    public enum SkillState { Novice, Amateur, Advanced, Sharpshooter };  // Different states for gameplay
    public int playerSkill;             // Player's skill level
    public int iteration = 0;           // Iteration of decision for player's skill level
    public string playerName;           // Define the player's name
    public string dbPath;               // Path to database

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private float speedIncrease;        // How much to increase zombie speed
    private float spawnTimeDecrease;    // How much to decrease zombie speed
    private bool activeZombieIncrease;  // Whether to increment number of zombies on scene
    private bool repeatTutorial;        // Whether to repeat the tutorial stage
    private List<string[]> rowData = new List<string[]>(); // Used tor writing csv file
    private string filePath;

    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start()
    {

        //filePath = Application.dataPath + "/PlayerData/" + playerName + ".csv";
        // Creating First row of titles manually..
        string[] rowDataTemp = new string[4];
        rowDataTemp[0] = "Iteration";
        rowDataTemp[1] = "Hit Percentage";
        rowDataTemp[2] = "Head Shot Percentage";
        rowDataTemp[3] = "Decision";

        rowData.Add(rowDataTemp);


        tutCont = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        enemManager = GameObject.FindWithTag("EnemyManager").GetComponent<EnemyManager>();

        dbPath = "URI=file:" + Application.dataPath + "/ML_Database.db";
		CreateSchema(dbPath);
		checkUser(playerName, dbPath);
		double[] arr = getPercents(playerName, dbPath);
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update()
    {
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Database Setup Functions
    //////////////////////////////////////////////////////////////////////////////////
    // Instantiate a new database if one does not already exist
    public void CreateSchema(string dbPath) {
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "CREATE TABLE IF NOT EXISTS 'stats' ( " +
								  "  'playerName' TEXT PRIMARY KEY, " +
								  "  'totalshots' FLOAT DEFAULT '0', " +
								  "  'missshots' FLOAT DEFAULT '0', " +
								  "  'headshots' FLOAT DEFAULT '0', " +
								  "  'bodyshots' FLOAT DEFAULT '0', " +
								  "  'misspercent' FLOAT DEFAULT '0', " +
								  "  'hitpercent' FLOAT DEFAULT '0', " +
								  "  'headshotpercent' FLOAT DEFAULT '0'" +
								  ");";

				var result = cmd.ExecuteNonQuery();
			}
		}
	}
	
	// Make sure a record for the user exists. If not, generate one
	public void checkUser(string player, string dbPath) {
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "INSERT INTO stats(playerName) Select(@playerName)" + 
									"WHERE NOT EXISTS (SELECT * FROM stats WHERE playerName = @playerName);";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var result = cmd.ExecuteNonQuery();
				//Debug.Log("Checked player");
			}
		}
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Database Manipulation Functions
    //////////////////////////////////////////////////////////////////////////////////
    //Update headshot stats
    public void headShot(string player, string dbPath) {
		//Increment headshot counter
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE stats SET headshots = headshots + 1, totalshots = totalshots + 1 " +
									"WHERE playerName = @playerName;";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var result = cmd.ExecuteNonQuery();
			}
		}
		//Re-calculate percentages
		calcStats(player, dbPath);
	}
	
	//Update bodyshot stats
	public void bodyShot (string player, string dbPath) {
		//Increment bodyshot counter
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE stats SET bodyshots = bodyshots + 1, totalshots = totalshots + 1 " +
									"WHERE playerName = @playerName;";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var result = cmd.ExecuteNonQuery();
			}
		}
		//Re-calculate percentages
		calcStats(player, dbPath);
	}
	
	//Update missed shot stats
	public void missShot (string player, string dbPath) {
		//Increment missed shot counter
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE stats SET missshots = missshots + 1, totalshots = totalshots + 1 " +
									"WHERE playerName = @playerName;";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var result = cmd.ExecuteNonQuery();
				//Debug.Log("Missed shot!");
			}
		}
		//Re-calculate percentages
		calcStats(player, dbPath);
	}
	
	//Calculates all of the statistic percentages
	public void calcStats (string player, string dbPath) {
		using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "UPDATE stats SET headshotpercent = headshots/totalshots, hitpercent = (headshots + bodyshots)/totalshots, misspercent = missshots/totalshots " +
									"WHERE playerName = @playerName;";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var result = cmd.ExecuteNonQuery();
				//Debug.Log("Calculated statistics");
			}
		}
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Read Data from Database
    //////////////////////////////////////////////////////////////////////////////////
    public double[] getPercents (string player, string dbPath){
		
		//Define and initialize placeholders for the statistics
		double hitpercent = 0;
		double misspercent = 0;
		double headshotpercent = 0;
        double missshots = 0;
        double headshots = 0;
        double bodyshots = 0;


        //Get all of the statistics from database
        using (var conn = new SqliteConnection(dbPath)) {
			conn.Open();
			using (var cmd = conn.CreateCommand()) {
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT * FROM stats WHERE playerName = @playerName";
				cmd.Parameters.Add(new SqliteParameter {
					ParameterName = "playerName",
					Value = player
				});
				var reader = cmd.ExecuteReader();
				if(reader.Read()){
					hitpercent = (double) reader["hitpercent"];
					misspercent = (double) reader["misspercent"];
					headshotpercent = (double) reader["headshotpercent"];
                    missshots = (double) reader["missshots"];
                    headshots = (double) reader["headshots"];
                    bodyshots = (double) reader["bodyshots"];
                }
			}
		}

        //Fill and return array with all of the neccessary statistics.
        double[] arr = {hitpercent, misspercent, headshotpercent, missshots, headshots, bodyshots};
		return arr;
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // React to Player Statistics
    //////////////////////////////////////////////////////////////////////////////////
    public bool statsReact (string player, string dbPath){
		
		//Extract player statistics
		double[] stats = getPercents(playerName, dbPath);
		double hitpercent = stats[0];
		double misspercent = stats[1];
		double headshotpercent = stats[2];



        // Determine player's skill level using two features: misspercent and headshotpercent
        if (hitpercent < 0.50f)
        {
            playerSkill = (int)SkillState.Novice;
        }
        else if (hitpercent < 0.6f)
        {
            playerSkill = (int)SkillState.Amateur;
        }
        else if (hitpercent < 0.75f)
        {
            playerSkill = (int)SkillState.Advanced;
        }
        else if (hitpercent >= 0.75f && headshotpercent < 0.5f) 
        {
            playerSkill = (int)SkillState.Advanced;
        }
        else if (hitpercent > 0.75f && headshotpercent >= 0.5f) 
        {
            playerSkill = (int)SkillState.Sharpshooter;
        }

        // Based on player's skill level, change parameters of survival mode
        switch (playerSkill)
        {
            case (int)SkillState.Novice:
                speedIncrease = 0f;
                spawnTimeDecrease = 0f;
                activeZombieIncrease = false;
                repeatTutorial = true;
                break;
            case (int)SkillState.Amateur:
                speedIncrease = 0.05f;
                spawnTimeDecrease = 0.05f;
                activeZombieIncrease = false;
                repeatTutorial = false;
                break;
            case (int)SkillState.Advanced:
                speedIncrease = 0.1f;
                spawnTimeDecrease = 0.1f;
                activeZombieIncrease = false;
                repeatTutorial = false;
                break;
            case (int)SkillState.Sharpshooter:
                speedIncrease = 0.15f;
                spawnTimeDecrease = 0.15f;
                activeZombieIncrease = true;
                repeatTutorial = false;
                break;

        }

        // Change parameters of survival mode
        tutCont.AdjustZombieSpeed(speedIncrease);
        tutCont.AdjustSpawnTime(-spawnTimeDecrease);
        if (activeZombieIncrease == true)
        {
            enemManager.IncMaxActiveZombies();
        }


        // Record inputs and outputs of decision tree to csv file
        string[] rowDataTemp = new string[4];
        rowDataTemp[0] = "" + iteration; 
        rowDataTemp[1] = "" + hitpercent;
        rowDataTemp[2] = "" + headshotpercent;
        rowDataTemp[3] = "" + playerSkill;
        rowData.Add(rowDataTemp);

        // Increment what decision this was
        iteration += 1;
        //Save();

        // Return whether the player should repeat the tutorial stage
        return repeatTutorial;
		
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Write to CSV File
    //////////////////////////////////////////////////////////////////////////////////
    public void Save()
    {
    
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));


        string filePath = Application.dataPath + "/PlayerData/" + playerName + ".csv";
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
    }

    public void AppendToFile()
    {


    }
}
