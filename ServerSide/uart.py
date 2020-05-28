import time
import serial

ser = serial.Serial('/dev/ttyACM0')
ser.baudrate = 9600

time.sleep(0.4)
print(ser.readline())

print(ser.name)

on = b'ON\n'
off = b'OFF\n'

ser.write(off)

#while True:
    #print(ser.read())
    #time.sleep(1)
