
import cv2
import numpy as np

global imageValue
imageValue = "Q0"
global imageCommandNumber
imageCommandNumber = 0
def recognize():

	cap = cv2.VideoCapture(0)
	
	while(1):
		global imageValue
		global imageCommandNumber
		# Take each frame
		_, frame = cap.read()

		# Convert BGR to HSV
		hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

		# define range of green color in HSV
		# ranges: H = [0, 179], S = [0, 255], V = [0, 255]
		lower_color = np.array([70,50,40])
		upper_color = np.array([100,255,255])

		# Threshold the HSV image to get only predefined colors
		mask = cv2.inRange(hsv, lower_color, upper_color)
		
		# Bitwise-AND mask and original image
		res = cv2.bitwise_and(frame,frame, mask= mask)
		
		# Original processed image
		cv2.imshow('original res', res)
		
		res = cv2.blur(res,(50,50)) # (n,n) defines kernel dimension
		# cv2.imshow('(50,50) processed res', blur3);
		
		# Contouring
		im2, contours, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
			
		# Setting up the variables
		x = y = w = h = 0 
		
		# Getting all the contours
		for conts in contours:
		
			# Only want big contours
			if cv2.contourArea(conts) < 10000:
				continue
				
			# (x,y) is the top left coordinate; w and h is width and height    
			x, y, w, h = cv2.boundingRect(conts)
			
			# Drawing the contours
			cv2.drawContours(res, [conts], -1, (0, 255, 0), 2)
			
			# Check which quadrant
			if y > cap.get(4)/2:
				if x > cap.get(3)/2:
					imageValue = "Q4"
				else:
					imageValue = "Q3"
			elif y < cap.get(4)/2:
				if x > cap.get(3)/2:
					imageValue = "Q2"
				else:
					imageValue = "Q1"
			else
				imageValue = "Q0"
		
		#print(len(contours))
		
		# cv2.imshow('frame', frame)
		# cv2.imshow('mask', mask)
		cv2.imshow('res with rectangles', res) # we only need this one
		
		# Press ESC to exit
		k = cv2.waitKey(5) & 0xFF
		if k == 27:
			break

	cv2.destroyAllWindows()
