#!/usr/bin/python
#
#    This program  reads the angles from the acceleromter, gyrscope
#    and mangnetometeron a BerryIMU connected to a Raspberry Pi.
#
#    This program includes two filters (low pass and mdeian) to improve the 
#    values returned from BerryIMU by reducing noise.
#
#
#    http://ozzmaker.com/
#    Both the BerryIMUv1 and BerryIMUv2 are supported
#
#    BerryIMUv1 uses LSM9DS0 IMU
#    BerryIMUv2 uses LSM9DS1 IMU
#




import sys
import time
import math
import IMU
import datetime
import os
import json
# Import SPI library (for hardware SPI) and MCP3008 library.                                                                                                  
import Adafruit_GPIO.SPI as SPI
import Adafruit_MCP3008



def collect(sensor_socket, ADDRESS, signal):
    # If the IMU is upside down (Skull logo facing up), change this value to 1
    IMU_UPSIDE_DOWN = 0	

    ##Force Sensor Configuration
    # Software SPI configuration:                                                                                                                                 
    CLK  = 18
    MISO = 23
    MOSI = 24
    CS   = 25
    mcp = Adafruit_MCP3008.MCP3008(clk=CLK, cs=CS, miso=MISO, mosi=MOSI)
    threshold = 250
    gamma = 1.5
    max_limit = 999.0
    force_val = 0.0


    RAD_TO_DEG = 57.29578
    M_PI = 3.14159265358979323846
    G_GAIN = 0.070  	# [deg/s/LSB]  If you change the dps for gyro, you need to update this value accordingly
    AA =  0.40      	# Complementary filter constant
    MAG_LPF_FACTOR = 0.4 	# Low pass filter constant magnetometer
    ACC_LPF_FACTOR = 0.4 	# Low pass filter constant for accelerometer
    ACC_MEDIANTABLESIZE = 9    	# Median filter table size for accelerometer. Higher = smoother but a longer delay
    MAG_MEDIANTABLESIZE = 9    	# Median filter table size for magnetometer. Higher = smoother but a longer delay



    ################# Compass Calibration values ############
    # Use calibrateBerryIMU.py to get calibration values 
    # Calibrating the compass isnt mandatory, however a calibrated 
    # compass will result in a more accurate heading value.

    magXmin =  0
    magYmin =  0
    magZmin =  0
    magXmax =  0
    magYmax =  0
    magZmax =  0


    '''
    Here is an example:
    magXmin =  -1748
    magYmin =  -1025
    magZmin =  -1876
    magXmax =  959
    magYmax =  1651
    magZmax =  708
    Dont use the above values, these are just an example.
    '''
    
    gyroXangle = 0.0
    gyroYangle = 0.0
    gyroZangle = 0.0
    CFangleX = 0.0
    CFangleY = 0.0
    CFangleXFiltered = 0.0
    CFangleYFiltered = 0.0
    
    oldXMagRawValue = 0
    oldYMagRawValue = 0
    oldZMagRawValue = 0
    oldXAccRawValue = 0
    oldYAccRawValue = 0
    oldZAccRawValue = 0

    a = datetime.datetime.now()



    #Setup the tables for the mdeian filter. Fill them all with '1' soe we dont get devide by zero error 
    acc_medianTable1X = [1] * ACC_MEDIANTABLESIZE
    acc_medianTable1Y = [1] * ACC_MEDIANTABLESIZE
    acc_medianTable1Z = [1] * ACC_MEDIANTABLESIZE
    acc_medianTable2X = [1] * ACC_MEDIANTABLESIZE
    acc_medianTable2Y = [1] * ACC_MEDIANTABLESIZE
    acc_medianTable2Z = [1] * ACC_MEDIANTABLESIZE
    mag_medianTable1X = [1] * MAG_MEDIANTABLESIZE
    mag_medianTable1Y = [1] * MAG_MEDIANTABLESIZE
    mag_medianTable1Z = [1] * MAG_MEDIANTABLESIZE
    mag_medianTable2X = [1] * MAG_MEDIANTABLESIZE
    mag_medianTable2Y = [1] * MAG_MEDIANTABLESIZE
    mag_medianTable2Z = [1] * MAG_MEDIANTABLESIZE

    IMU.detectIMU()     #Detect if BerryIMUv1 or BerryIMUv2 is connected.
    IMU.initIMU()       #Initialise the accelerometer, gyroscope and compass


    while True:

        #Read the accelerometer,gyroscope and magnetometer values
        ACCx = IMU.readACCx()
        ACCy = IMU.readACCy()
        ACCz = IMU.readACCz()
        GYRx = IMU.readGYRx()
        GYRy = IMU.readGYRy()
        GYRz = IMU.readGYRz()
        MAGx = IMU.readMAGx()
        MAGy = IMU.readMAGy()
        MAGz = IMU.readMAGz()


        #Apply compass calibration    
        MAGx -= (magXmin + magXmax) /2 
        MAGy -= (magYmin + magYmax) /2 
        MAGz -= (magZmin + magZmax) /2 
     

        ##Calculate loop Period(LP). How long between Gyro Reads
        b = datetime.datetime.now() - a
        a = datetime.datetime.now()
        LP = b.microseconds/(1000000*1.0)


        ############################################### 
        #### Apply low pass filter ####
        ###############################################
        MAGx =  MAGx  * MAG_LPF_FACTOR + oldXMagRawValue*(1 - MAG_LPF_FACTOR);
        MAGy =  MAGy  * MAG_LPF_FACTOR + oldYMagRawValue*(1 - MAG_LPF_FACTOR);
        MAGz =  MAGz  * MAG_LPF_FACTOR + oldZMagRawValue*(1 - MAG_LPF_FACTOR);
        ACCx =  ACCx  * ACC_LPF_FACTOR + oldXAccRawValue*(1 - ACC_LPF_FACTOR);
        ACCy =  ACCy  * ACC_LPF_FACTOR + oldYAccRawValue*(1 - ACC_LPF_FACTOR);
        ACCz =  ACCz  * ACC_LPF_FACTOR + oldZAccRawValue*(1 - ACC_LPF_FACTOR);

        oldXMagRawValue = MAGx
        oldYMagRawValue = MAGy
        oldZMagRawValue = MAGz
        oldXAccRawValue = ACCx
        oldYAccRawValue = ACCy
        oldZAccRawValue = ACCz

        ######################################### 
        #### Median filter for accelerometer ####
        #########################################
        # cycle the table
        for x in range (ACC_MEDIANTABLESIZE-1,0,-1 ):
            acc_medianTable1X[x] = acc_medianTable1X[x-1]
            acc_medianTable1Y[x] = acc_medianTable1Y[x-1]
            acc_medianTable1Z[x] = acc_medianTable1Z[x-1]

        # Insert the lates values
        acc_medianTable1X[0] = ACCx
        acc_medianTable1Y[0] = ACCy
        acc_medianTable1Z[0] = ACCz    

        # Copy the tables
        acc_medianTable2X = acc_medianTable1X[:]
        acc_medianTable2Y = acc_medianTable1Y[:]
        acc_medianTable2Z = acc_medianTable1Z[:]

        # Sort table 2
        acc_medianTable2X.sort()
        acc_medianTable2Y.sort()
        acc_medianTable2Z.sort()

        # The middle value is the value we are interested in
        ACCx = acc_medianTable2X[ACC_MEDIANTABLESIZE/2];
        ACCy = acc_medianTable2Y[ACC_MEDIANTABLESIZE/2];
        ACCz = acc_medianTable2Z[ACC_MEDIANTABLESIZE/2];



        ######################################### 
        #### Median filter for magnetometer ####
        #########################################
        # cycle the table
        for x in range (MAG_MEDIANTABLESIZE-1,0,-1 ):
            mag_medianTable1X[x] = mag_medianTable1X[x-1]
            mag_medianTable1Y[x] = mag_medianTable1Y[x-1]
            mag_medianTable1Z[x] = mag_medianTable1Z[x-1]

        # Insert the latest values    
        mag_medianTable1X[0] = MAGx
        mag_medianTable1Y[0] = MAGy
        mag_medianTable1Z[0] = MAGz    

        # Copy the tables
        mag_medianTable2X = mag_medianTable1X[:]
        mag_medianTable2Y = mag_medianTable1Y[:]
        mag_medianTable2Z = mag_medianTable1Z[:]

        # Sort table 2
        mag_medianTable2X.sort()
        mag_medianTable2Y.sort()
        mag_medianTable2Z.sort()

        # The middle value is the value we are interested in
        MAGx = mag_medianTable2X[MAG_MEDIANTABLESIZE/2];
        MAGy = mag_medianTable2Y[MAG_MEDIANTABLESIZE/2];
        MAGz = mag_medianTable2Z[MAG_MEDIANTABLESIZE/2];



        #Convert Gyro raw to degrees per second
        rate_gyr_x =  GYRx * G_GAIN
        rate_gyr_y =  GYRy * G_GAIN
        rate_gyr_z =  GYRz * G_GAIN


        #Calculate the angles from the gyro. 
        gyroXangle+=rate_gyr_x*LP
        gyroYangle+=rate_gyr_y*LP
        gyroZangle+=rate_gyr_z*LP

        #Convert Accelerometer values to degrees

        if not IMU_UPSIDE_DOWN:
            # If the IMU is up the correct way (Skull logo facing down), use these calculations
            AccXangle =  (math.atan2(ACCy,ACCz)*RAD_TO_DEG)
            AccYangle =  (math.atan2(ACCz,ACCx)+M_PI)*RAD_TO_DEG
        else:
            #Us these four lines when the IMU is upside down. Skull logo is facing up
            AccXangle =  (math.atan2(-ACCy,-ACCz)*RAD_TO_DEG)
            AccYangle =  (math.atan2(-ACCz,-ACCx)+M_PI)*RAD_TO_DEG



        #Change the rotation value of the accelerometer to -/+ 180 and
        #move the Y axis '0' point to up.  This makes it easier to read.
        if AccYangle > 90:
            AccYangle -= 270.0
        else:
            AccYangle += 90.0



        #Complementary filter used to combine the accelerometer and gyro values.
        CFangleX=AA*(CFangleX+rate_gyr_x*LP) +(1 - AA) * AccXangle
        CFangleY=AA*(CFangleY+rate_gyr_y*LP) +(1 - AA) * AccYangle

        if IMU_UPSIDE_DOWN:
            MAGy = -MAGy      #If IMU is upside down, this is needed to get correct heading.
        #Calculate heading
        heading = 180 * math.atan2(MAGy,MAGx)/M_PI

        #Only have our heading between 0 and 360
        if heading < 0:
            heading += 360



        ####################################################################
        ###################Tilt compensated heading#########################
        ####################################################################
        #Normalize accelerometer raw values.
        if not IMU_UPSIDE_DOWN:        
            #Use these two lines when the IMU is up the right way. Skull logo is facing down
            accXnorm = ACCx/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
            accYnorm = ACCy/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
        else:
            #Us these four lines when the IMU is upside down. Skull logo is facing up
            accXnorm = -ACCx/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)
            accYnorm = ACCy/math.sqrt(ACCx * ACCx + ACCy * ACCy + ACCz * ACCz)

        #Calculate pitch and roll

        pitch = math.asin(accXnorm)
        roll = -math.asin(accYnorm/math.cos(pitch))


        #Calculate the new tilt compensated values
        magXcomp = MAGx*math.cos(pitch)+MAGz*math.sin(pitch)
     
        #The compass and accelerometer are orientated differently on the LSM9DS0 and LSM9DS1 and the Z axis on the compass
        #is also reversed. This needs to be taken into consideration when performing the calculations
        if(IMU.LSM9DS0):
            magYcomp = MAGx*math.sin(roll)*math.sin(pitch)+MAGy*math.cos(roll)-MAGz*math.sin(roll)*math.cos(pitch)   #LSM9DS0
        else:
            magYcomp = MAGx*math.sin(roll)*math.sin(pitch)+MAGy*math.cos(roll)+MAGz*math.sin(roll)*math.cos(pitch)   #LSM9DS1




        #Calculate tilt compensated heading
        tiltCompensatedHeading = 180 * math.atan2(magYcomp,magXcomp)/M_PI

        if tiltCompensatedHeading < 0:
                    tiltCompensatedHeading += 360
                    
        #Collect force sensor data
        value = mcp.read_adc(0)
        if value <= threshold:
            force_val = 0.0
        else:
            force_val = ((value - threshold)/(max_limit - threshold))**gamma            

        ############################ END ##################################
        
        #Package angles and force sensor data into JSON and output as string
        package = {"angle1": CFangleX, "angle2": CFangleY, "angle3": round(tiltCompensatedHeading, 4), "force": force_val}
        package_string = json.dumps(package)
        #Send package over socket
        sensor_socket.sendto(package_string, ADDRESS)

        #slow program down a bit, makes the output more readable
        time.sleep(0.03)
        
        
        ###available params
        ##heading, tiltCompensatedHeading

        #slow program down a bit, makes the output more readable
        time.sleep(0.03)

