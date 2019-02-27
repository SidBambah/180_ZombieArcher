import socket
import threading
import os
import signal
import time
import Queue
import fcntl
import struct
import speech_processing
import camera_processing
import json

def get_ip_address(ifname):
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    return socket.inet_ntoa(fcntl.ioctl(
        s.fileno(),
        0x8915,  # SIOCGIFADDR
        struct.pack('256s', ifname[:15])
    )[20:24])
	
def socket_create():
	##Creating UDP socket to receive sensor data
	global sensor_data_socket 
	sensor_data_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
	
	##Creating UDP socket to communicate with Unity
	global unity_socket
	unity_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

	##Binding sensor data socket to IP address
	HOST = get_ip_address('enp0s3')
	SENSOR_DATA_PORT = 10000
	SENSOR_DATA_ADDRESS = (HOST, SENSOR_DATA_PORT)
	sensor_data_socket.bind(SENSOR_DATA_ADDRESS)
	
	##Binding Unity socket to IP address
	UNITY_PORT = 10002
	UNITY_ADDRESS = (HOST, UNITY_PORT)
	unity_socket.bind(UNITY_ADDRESS)
	
def client_communicate():
	while True:
	##Receiving sensor data from Pi
		data,addr = sensor_data_socket.recvfrom(4096)
		##Send received data to Unity over socket after adding in Speech Recognition and Image Processing data
		if unityClientAddress is not None and data[0] is '{':
			##Convert JSON string to Python dictionary
			data = json.loads(data)
			##Add speech processing and image processing information
			data['speech'] = speech_processing.speechValue
			data['speechNumber'] = speech_processing.speechCommandNumber
			data['quadrant'] = camera_processing.imageValue
			##Convert back to python and send
			data = json.dumps(data)
			print (data) ##Print the data for debugging purposes
			unity_socket.sendto(data, unityClientAddress)
		if not signal.is_set():
			##Send stop signal if no more data is needed now
			##Sending 3 to be safe
			for _ in range(3):
				sensor_data_socket.sendto("stop".encode(), addr)
		signal.wait()
		##Send start signal to collect more data
		for _ in range(3):
			sensor_data_socket.sendto("collect".encode(), addr)
			
def unity_communicate():
	global unityClientAddress
	unityClientAddress = None
	while True:
		##Get signal from Unity
		data,addr = unity_socket.recvfrom(4096)
		unityClientAddress = addr
		if data.decode() == "collect":
			signal.set()
		elif data.decode() == "stop":
			signal.clear()
	
	
def image_processing():
	##Take camera data here
	camera_processing.recognize()
	
def main():
	##Create sockets
	socket_create()
	##Create signal
	global signal
	signal = threading.Event()
	signal.clear()
	##Define threads
	clientThread = threading.Thread(target = client_communicate)
	unityThread = threading.Thread(target = unity_communicate)
	speechRecognitionThread = threading.Thread(target = speech_processing.recognize)
	imageProcessingThread = threading.Thread(target = image_processing)
	##Close threads when main thread ends
	clientThread.daemon = True
	unityThread.daemon = True
	speechRecognitionThread.daemon = True
	imageProcessingThread.daemon = True
	##Start threads
	clientThread.start()
	unityThread.start()
	speechRecognitionThread.start()
	imageProcessingThread.start()
	##Keep main thread alive
	while True:
		time.sleep(0.01)
		
if __name__ == "__main__":
	main()
