## Raspberry Pi Software

Berry IMU Sensor Fusion: Uses MadgwickAHRS filter along with 
BerryIMU's library to determine the orientation of the device.

## Python and C scripts

* bow_sensors.py is a wrapper that utilizes all below scripts

* compass_calib.c outputs the calibration values for the magnetometer.

* calib.c outputs the roll, pitch and yaw angles in radians to stdout
in CSV format.

* term_out.py executes calib program, parses its output, and converts the
orientation angles into degrees.

* BerryIMU.py sends data to the rest of the Zombie_Archer system.