## Server Program

This folder contains the Python server required to interface between
the Unity game engine and the Raspberry Pi's IMU, which is mounted
on the controller.

The camera and microphone necessary for the game are also on the server
computer. The server imports Python code that controls the OpenCV camera
processing and Porcupine speech recognition. Furthermore, the server uses
the UDP communication protocol to act as a hub between the various hardware
components needed for proper operation of the game.

### Executing the Server

To run the server, simply execute:

```python
python server.py
```

Note: It is important to ensure that the host machine has UDP ports 10000 and
10002 open in the firewall to allow for the Unity engine and Raspberry Pi to
communicate successfully.

### Software Developed by the Team

server.py - Main server file that connects to Pi and Unity
camera_processing.py - Tracks objects on bow and gloves for quasi-multiplayer mode
columndetect.py - Tracks objects on bow to correct for gyroscopic drift from IMU
speech_processing.py - Detects hotwords for powerups and game interaction