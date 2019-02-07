using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class MachineLearning : MonoBehaviour
{
	//Define the player and database location
	public string playerName = "Sid";
	public string dbPath;
	
    // Start is called before the first frame update
    void Start()
    {
		dbPath = "URI=file:" + Application.dataPath + "/ML_Database.db";
		CreateSchema(dbPath);
		checkUser(playerName, dbPath);
		double[] arr = getPercents(playerName, dbPath);
    }
	
	//Database Setup Functions

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
				Debug.Log("create schema: " + result);
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
				Debug.Log("Checked player");
			}
		}
	}
	
	//Database Manipulation Functions
	
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
				Debug.Log("Headshot!");
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
				Debug.Log("Bodyshot!");
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
				Debug.Log("Missed shot!");
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
				Debug.Log("Calculated statistics");
			}
		}
	}
		
	public double[] getPercents (string player, string dbPath){
		
		//Define and initialize placeholders for the statistics
		double hitpercent = 0;
		double misspercent = 0;
		double headshotpercent = 0;
		
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
				}
			}
		}
		//Fill and return array with all of the neccessary statistics.
		double[] arr = {hitpercent, misspercent, headshotpercent};
		return arr;
	}
	
	public void statsReact (string player, string dbPath){
		//Main machine learning done here
		
		//Extract player statistics
		double[] stats = getPercents(player, dbPath);
		double hitpercent = stats[0];
		double misspercent = stats[1];
		double headshotpercent = stats[2];
		
		//React to player statistics
		
		//REACT HERE
		
		Debug.Log("Machine learning done");
		
	}
    // Update is called once per frame
    void Update()
    {
        //Run the Machine Learning each frame
		//Uncomment function below to run MachineLearning
		//statsReact(playerName, dbPath);
    }
}
