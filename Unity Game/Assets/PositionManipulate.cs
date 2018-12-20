using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;


public class PositionManipulate : MonoBehaviour {


    public static bool moveBowValid = true;
    public static bool isFired = false;
    public static bool isPaused = true; 
    public static float current_force;
    public static int spawnQuadrant = 1;
    public static bool isValidQuadrant = true;

    int prevSpawnQuadrant = 1;

    //Variable Definitions
    //private string HOST = "192.168.0.3"; //Must change this each time
    private string HOST = "131.179.27.249"; //Must change this each time
    //private string HOST = "131.179.38.85"; //Must change this each time

    //Variable Definitions

    int PORT = 10002;
	UdpClient unity_socket;
	IPEndPoint ep;
	private float previous_force = 0;
	
	
	private void sendSignal(string signal) {
		Byte[] message = Encoding.ASCII.GetBytes(signal);
		//Send signal three times to be safe
		for (int i = 0; i < 3; i++) {
			unity_socket.Send(message, message.Length);
			Debug.Log("Sent Signal");
		}
	}
	
	private void createSocket() {
		unity_socket = new UdpClient();
		unity_socket.Connect(IPAddress.Parse(HOST), PORT);
		ep = new IPEndPoint(IPAddress.Parse(HOST), PORT);
	}
	
	private string getResponse() {
		Byte[] response = unity_socket.Receive(ref ep);
		string responseAsString = System.Text.Encoding.ASCII.GetString(response);
		return responseAsString;
	}
	
	private void moveBow(JObject package) {
		float smooth = 5.0f;
        //float tiltAngle = 60.0f;
        // Smoothly tilts a transform towards a target rotation.
		//float tiltAngle = 60.0f;
		// Smoothly tilts a transform towards a target rotation.
        float tiltAroundX = package["angle1"].Value<float>();
        float tiltAroundY = package["angle2"].Value<float>();
        //Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, 0);
        Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, 0);


        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target,  Time.deltaTime * smooth);
	}
	

	// Use this for initialization
	void Start () {
		//Create socket
		createSocket();
		//Always be on "ON" mode for testing purposes
		sendSignal("collect");
	}
	
	// Update is called once per frame
	void Update () {
		string responseAsString = getResponse();
		JObject package = JObject.Parse(responseAsString);
        if(moveBowValid)
		    moveBow(package);
        Debug.Log(responseAsString);
		float force = package["force"].Value<float>();
		previous_force = current_force;
		current_force = force;
        if (current_force < 0.7 && previous_force > 0.7)
        {
            isFired = true;
            Debug.Log("isFired: " + isFired);
        }
        else 
        {
            isFired = false;
        }

    
       if (package["speech"].Value<string>() == "pause"){
            isPaused = true;
        }
        if (package["speech"].Value<string>() == "play")
        {
            isPaused = false;
        }

        // Read in quadrant values
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




    }
}


