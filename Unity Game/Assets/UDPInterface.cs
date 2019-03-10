using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;


public class UDPInterface : MonoBehaviour {

    ////////////////////////////////////////////////////////////////////////////////// 
    // Static Variables
    //////////////////////////////////////////////////////////////////////////////////
    public static bool moveBowValid = true;

    // Force
    public static float force = 0;
    public static bool validForce = false;

    // Gestures
    public static bool melee = false;
    public static bool reload = false;

    // Speech
    public static string speech = "pause";
    public static bool validSpeech = false;

    // Image
    public static string spawnQuadrant = "Q1";
    public static bool validQuadrant = false;

    // For testing
    public static bool testing = false;


    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private float previous_force = 0;
    private string HOST = "131.179.27.201"; //Must change this each time
    private int PORT = 10002;
	private UdpClient unity_socket;
	private IPEndPoint ep;

    // Force
    private bool prevValidForce = false;
    private bool curValidForce;

    // Gestures
    private bool prevMelee = false;
    private bool curMelee;
    private bool prevReload = false;
    private bool curReload;

    // Speech
    private int prevSpeechNumber = 0;
    private int curSpeechNumber;

    // Image
    private int prevImageNumber = 0;
    private int curImageNumber;
    


    ////////////////////////////////////////////////////////////////////////////////// 
    // Use this for initialization
    //////////////////////////////////////////////////////////////////////////////////
    void Start () {
		//Create socket
		CreateSocket();
		//Always be on "ON" mode for testing purposes
		SendSignal("collect");
	}

    ////////////////////////////////////////////////////////////////////////////////// 
    // Update is called once per frame
    //////////////////////////////////////////////////////////////////////////////////
    void Update () {
        if (!testing)
        {
            string responseAsString = GetResponse();
            JObject package = JObject.Parse(responseAsString);
            ProcessPackage(package);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // UDP Setup Function
    //////////////////////////////////////////////////////////////////////////////////
    private void CreateSocket()
    {
        unity_socket = new UdpClient();
        unity_socket.Connect(IPAddress.Parse(HOST), PORT);
        ep = new IPEndPoint(IPAddress.Parse(HOST), PORT);
    }

    private void SendSignal(string signal)
    {
        Byte[] message = Encoding.ASCII.GetBytes(signal);
        //Send signal three times to be safe
        for (int i = 0; i < 3; i++)
        {
            unity_socket.Send(message, message.Length);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Read Response 
    //////////////////////////////////////////////////////////////////////////////////
    private string GetResponse()
    {
        Byte[] response = unity_socket.Receive(ref ep);
        string responseAsString = System.Text.Encoding.ASCII.GetString(response);
        return responseAsString;
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Adjust the Bow's position
    //////////////////////////////////////////////////////////////////////////////////
    private void MoveBow(JObject package)
    {
        float smooth = 5.0f;
        float tiltAroundX = package["angle1"].Value<float>();
        float tiltAroundY = package["angle3"].Value<float>();

        Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, 0);

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Parse through Package
    //////////////////////////////////////////////////////////////////////////////////

    void ProcessPackage(JObject package)
    {
        // Change the rotation of the bow
        if (moveBowValid)
            MoveBow(package);

        // Store current value of force and valid value
        ParseForce(package);

        // Store value of gestures
        ParseGestures(package);

        // Update speech command and valid value
        ParseSpeech(package);

        // Update current quadrant and valid value
        ParseImage(package);

    }

    void ParseForce(JObject package)
    {
        // Always store force value
        force = package["force"].Value<float>();

        // Store current value of valid signal
        curValidForce = package["launch"].Value<bool>();

        // Look for positive transition in valid signal
        validForce = (curValidForce == true && prevValidForce == false);

        // Store previous valid value
        prevValidForce = curValidForce;


    }

    void ParseGestures(JObject package)
    {
        int tmp1 = package["melee"].Value<int>();
        int tmp2 = package["reload"].Value<int>();

        // Get current values for gestures
        curMelee = (tmp1 == 1) ? true : false;
        curReload = (tmp2 == 1) ? true : false;

        // Look for positive transition gestures
        melee = (curMelee == true && prevMelee == false);
        reload = (curReload == true && prevReload == false);

        // Store previous values
        prevMelee = curMelee;
        prevReload = curReload;

    }

    void ParseSpeech(JObject package)
    {
        // Always update current speech value
        speech = package["speech"].Value<string>();

        // Get current speechNumber
        curSpeechNumber = package["speechNumber"].Value<int>();

        // If different, then new keyword was sent
        validSpeech = (curSpeechNumber != prevSpeechNumber);

        // Store current speechNumber for next iteration
        prevSpeechNumber = curSpeechNumber;

    }

    void ParseImage(JObject package)
    {
        // Always update current quadrant
        spawnQuadrant = package["quadrant"].Value<String>();

        // Get current imageNumber
        curImageNumber = package["imageNumber"].Value<int>();

        // If different, then valid quadrant is sent
        validQuadrant = (curImageNumber != prevImageNumber);

        // Store current imageNumber for next iteration
        prevImageNumber = curImageNumber;

    }

    /*private void ProcessPackage(JObject package)
    {
        // Change the rotation of the bow
        if (moveBowValid)
            MoveBow(package);

        float force = package["force"].Value<float>();
        previous_force = current_force;

        // Get value of current_force
        current_force = force;

        // Determine whether current_force is valid
        if (current_force < 0.7 && previous_force > 0.7)
        {
            isFired = true;
        }
        else
        {
            isFired = false;
        }

        // Get value of isPaused
        if (package["speech"].Value<string>() == "pause")
        {
            isPaused = true;
        }
        if (package["speech"].Value<string>() == "play")
        {
            isPaused = false;
        }

        // Get value of spawnQuadrant
        // Determine whether spawnQuadrant is valid based on 
        // changes in its value
        if (package["quadrant"].Value<string>() == "Q1")
        {
            if (spawnQuadrant == 1)
                isValidQuadrant = false;
            else
            {
                isValidQuadrant = true;
                spawnQuadrant = 1;
            }
        }
        else if (package["quadrant"].Value<string>() == "Q2")
        {
            if (spawnQuadrant == 2)
                isValidQuadrant = false;
            else
            {
                isValidQuadrant = true;
                spawnQuadrant = 2;
            }
        }
        else if (package["quadrant"].Value<string>() == "Q3")
        {
            if (spawnQuadrant == 3)
                isValidQuadrant = false;
            else
            {
                isValidQuadrant = true;
                spawnQuadrant = 3;
            }
        }
        else if (package["quadrant"].Value<string>() == "Q4")
        {
            if (spawnQuadrant == 4)
                isValidQuadrant = false;
            else
            {
                isValidQuadrant = true;
                spawnQuadrant = 4;
            }
        }
    }*/
}


