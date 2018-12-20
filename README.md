## ZombieArcher

ZombieArcher is a video game designed for UCLA's Capstone Couse EE 180D.

The designers are Sidharth Bambah, Sparsh Gauba, Andrew Juarez, and Mohamed Shatela.


### Main Components:
1) Unity Game
2) Raspberry Pi Sensor Software
3) Python Server (including OpenCV software and Speech Recognition)

#### How to Run Python Server:
1. Define wireless interface name in HOST definition if not wifi0
2. Run with:

	```python
	python server.py
	```
	
#### How to Run Raspberry Pi Sensor Software:
1. Define server IP address in data_collect() function
2. Define wireless interface name in HOST definition if not wlan0
3. Copy file pi_socket.py to Raspberry Pi and connect necessary hardware (BerryIMU)
4. Run program on Pi with:
	
	```python
	python bow_sensors.py
	```
	
Note: It is **critical** to start server before Pi and Unity software to properly initialize
the UDP sockets. Also, UDP ports 10000 and 10002 must be opened in the firewall.

Known Issues
1. Unity client freezes if data collection is stopped externally