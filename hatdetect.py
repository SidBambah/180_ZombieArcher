#https://opencv-python-tutroals.readthedocs.io/en/latest/py_tutorials/py_imgproc/py_colorspaces/py_colorspaces.html
import cv2
import numpy as np

cap = cv2.VideoCapture(0)

while(1):

    # Take each frame
    _, frame = cap.read()

    # Convert BGR to HSV
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # define range of green color in HSV
	# ranges: H = [0, 179], S = [0, 255], V = [0, 255]
    lower_blue = np.array([70,50,50]) # Pink = (110,50,50), #Green = (70,50,50)
    upper_blue = np.array([100,255,255]) # Pink = (130,255,255), #Green = (100,255,255)

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
            
        # (x,y) is the top left coordinate; w and h is width and height    
        x, y, w, h = cv2.boundingRect(conts)
        
        # Drawing the bounding rectangle
        # res = cv2.rectangle(res,(x,y),(x+w,y+h),(0,255,0),3)
        cv2.drawContours(res, [conts], -1, (0, 255, 0), 2)
        
        # Check which quadrant
        if y > cap.get(4)/2:
            if x > cap.get(3)/2:
                print('Lower right')
            else:
                print('Lower left')
        elif y < cap.get(4)/2:
            if x > cap.get(3)/2:
                print('Upper right')
            else:
                print('Upper left')
    
    #print(len(contours))
    
    # cv2.imshow('frame', frame)
    # cv2.imshow('mask', mask)
    cv2.imshow('res with rectangles', res) # we only need this one
    
    # Press ESC to exit
    k = cv2.waitKey(5) & 0xFF
    if k == 27:
        break

cv2.destroyAllWindows()