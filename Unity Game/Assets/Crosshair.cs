using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Description: 
// Creates crosshair at the center of the screen
// Can attach to any game object, since it references the GUI

public class Crosshair : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Public Variables
    //////////////////////////////////////////////////////////////////////////////////
    public Texture2D crosshairImage;
    public GameController gc;
    public MachineLearning ML;
    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start () {
        ML = GameObject.FindWithTag("MachineLearning").GetComponent<MachineLearning>();
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update () {
		/*if (gc.GetState() > 0)
        {
            
        }*/
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Always Display on GUI
    //////////////////////////////////////////////////////////////////////////////////
    void OnGUI()
    {
        // Gets the lower left corner of where to draw crosshair
        float xMin = Screen.width / 2  - crosshairImage.width / 2;
        float yMin = Screen.height / 2 - crosshairImage.height / 2;

        // Adds crosshair to follow mouse
        //float xMin = Screen.width - (Screen.width - Input.mousePosition.x) - (crosshairImage.width / 2);
        //float yMin = (Screen.height - Input.mousePosition.y) - (crosshairImage.height / 2);

        // Draw crosshair image
        if (gc.GetState() > 0 && gc.GetState() <= 9 && !(ML.playerSkill == 3))
        {
            GUI.DrawTexture(new Rect(xMin + 75, yMin + 25, crosshairImage.width, crosshairImage.height), crosshairImage);
        }
    }
}
