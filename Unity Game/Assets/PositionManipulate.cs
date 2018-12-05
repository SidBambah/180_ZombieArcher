using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

public class PositionManipulate : MonoBehaviour {
    //Variable Definitions
    private string HOST = "131.179.26.253"; //Must change this each time
    //private string HOST = "131.179.38.86"; //Must change this each time
    //private string HOST = "192.168.0.2"; //Must change this each time
    int PORT = 10002;
	UdpClient unity_socket;
	IPEndPoint ep;
	private float previous_force = 0;
	public float current_force = 0;
	public bool isFired = false;
	
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
        float tiltAroundX = package["angle1"].Value<float>();
        float tiltAroundY = package["angle2"].Value<float>();
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
		moveBow(package);
        Debug.Log(responseAsString);
		float force = package["force"].Value<float>();
		previous_force = current_force;
		current_force = force;
		if(current_force - previous_force < -0.5){
			isFired = true;
		}
        //Debug.Log("FORCE: " + force);
        Debug.Log("isFired: " + isFired);
	}
}
