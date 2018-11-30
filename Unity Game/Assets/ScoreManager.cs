using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Description: 
// Displays the player's score
public class ScoreManager : MonoBehaviour {

    // Public variables
    public static int score; // Holds the player's scores

    // Private variables
    Text text; // Text to display in GUI

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>(); // Get reference to text component
        score = 0;  // Initialize score to 0
	}
	
	// Update is called once per frame
	void Update () {

        // Display the score
        text.text = "SCORE: " + score;
	}
}
