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
    public static bool isFired = false;
    public static bool isPaused = false; // CHANGED: false for testing
    public static float current_force;
    public static int spawnQuadrant = 1;
    public static bool isValidQuadrant = true;

    ////////////////////////////////////////////////////////////////////////////////// 
    // Private Variables
    //////////////////////////////////////////////////////////////////////////////////
    private float previous_force = 0;
    private string HOST = "131.179.19.48"; //Must change this each time
    private int PORT = 10002;
	private UdpClient unity_socket;
	private IPEndPoint ep;


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
		//string responseAsString = GetResponse();
		//JObject package = JObject.Parse(responseAsString);
        //ProcessPackage(package);
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

        //Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, 0);
        Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY, 0);

        // Dampen towards the target rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
    }

    ////////////////////////////////////////////////////////////////////////////////// 
    // Parse through Package
    //////////////////////////////////////////////////////////////////////////////////
    private void ProcessPackage(JObject package)
    {
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
    }
}


