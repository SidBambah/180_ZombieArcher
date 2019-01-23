import cv2
import numpy as np
import math

cap = cv2.VideoCapture(0)

while(1):

    # Take each frame
    _, frame = cap.read()

    # Convert BGR to HSV
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # define range of green color in HSV
	# ranges: H = [0, 179], S = [0, 255], V = [0, 255]
    lower_blue = np.array([70,50,40]) # Blue = (110,50,50), Green = (40,50,50)
    upper_blue = np.array([100,255,255]) # Blue = (130,255,255), Green = (70,255,255)

    # Threshold the HSV image to get only blue colors
    mask = cv2.inRange(hsv, lower_blue, upper_blue)
    
    # Bitwise-AND mask and original image
    res = cv2.bitwise_and(frame,frame, mask= mask)
    
    # Original processed image
    # cv2.imshow('original res', res)
    
    res = cv2.blur(res,(50,50)) # (n,n) defines kernel dimension
    # cv2.imshow('Blurred res', res);
    
    # Contouring
    im2, contours, hierarchy = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        
    # Setting up the variables
    x = y = w = h = 0 
    
    # Getting all the contours
    for conts in contours:
    
        # Only want big contours
        if cv2.contourArea(conts) < 10000:
            continue
            
        # Want to get the x-coordinate of the centroid
        M = cv2.moments(conts)
        x_centroid = int(M['m10'] / M['m00'])
        x_line1  = math.floor(cap.get(3)/x_centroid) + x_centroid
        x_line2 = math.ceil(cap.get(4)/x_centroid) + x_centroid
        cv2.line(res, (x_line1, 0), (x_line1, int(cap.get(4))), (0,255,0), 2)
        cv2.line(res, (x_line2, 0), (x_line2, int(cap.get(4))), (0,255,0), 2)
    
    cv2.imshow('res with rectangles', res) # we only need this one
    
    # Press ESC to exit
    k = cv2.waitKey(5) & 0xFF
    if k == 27:
        break

cv2.destroyAllWindows()