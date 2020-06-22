import serial

ser = serial.Serial('/dev/', timeout=1)
ser.baudrate = 115200

msg = 'ON\n'
ser.write(msg)

ser.readline()