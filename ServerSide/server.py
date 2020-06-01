import io
import os
import PIL
import cv2
import time
import socket
import serial
import random as rd
import threading as th
from PIL import Image

#gw = os.popen("ip -4 route show default").read().split()       #This method works only on linux based systems
#s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
#s.connect((gw[2], 0))
#own_ip = s.getsockname()[0]

own_ip = "192.168.11.128"          
phone_ip = ""                               # this ports must be same as in the android application: Android side uses this ports:
port_send_image = 55556                         # 55556 - to send image's bytes to client.
port_get = 55555                                # 55555 - to get motor controlling signals from client.
port_listen = 55554                             # 55554 - to listen incoming connection requests from client.

on_off_motors_signal = '0'
motor_signal = ""                           #   any motor controlling command has its specific bytes command (incoming type: byte[], used type: string)
uv_signal = ""                              #   UV light must be turned ON or OFF, (incoming type: byte[], used type: boolean)
close_motor_request = "34"                  #   command which demands to close motor control and preview from here(robot side), (incoming type: byte[], used type: string )
close_preview_request = False               #   command to open or close camera preview (incoming type: _, used type: boolean)
let_motor_control = False                   #   to know if we can send motor controlling signals to Arduino or not

previous_state = ''                         #   keeps the previous signal from client
turn_on = b'ON\n'                           #   signal, which value must be sent to Arduino to enable motors control
turn_off = b'OFF\n'                         #   signal, which value must be sent to Arduino to disable motors control
send_me = b'0'                              #   signal, which must be sent to Arduino (turn ON or turn OFF)

#ser = serial.Serial('/dev/ttyACM0')
#ser.baudrate = 9600

"""To get signal from client"""
def Get_Signal():
    global close_preview_request
    global motor_signal
    global uv_signal
    global listening
    global on_off_motors_signal
    
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_get))

    print("Getting signal from clients...")

    while True:   
        data, addr = sock.recvfrom(1024)
        motor_signal = data.decode('utf-8').split('|')[0]
        uv_signal = data.decode('utf-8').split('|')[1]
        on_off_motors_signal = data.decode('utf-8').split('|')[2]
        #print("On off signal is: " + on_off_motors_signal)
        if str(motor_signal) == close_motor_request or data == None:
            close_preview_request = True
            break
        time.sleep(0.1)
    if listening.is_alive() == False:
        listening = th.Thread(target=Listen)
        listening.start()
    sock.close()


"""To send video translation to clients"""
def Send_Image():
    global close_preview_request
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) 
    video_capturing = cv2.VideoCapture(0)

    isread, img = video_capturing.read()

    scale_percent = 60 # percent of original size
    width = int(img.shape[1] * scale_percent / 100)
    height = int(img.shape[0] * scale_percent / 100)
    dim = (width, height)
    encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), 90]

    resized = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
    retval, buffer = cv2.imencode('.jpg', resized, encode_param)

    print("Sending video frames data... Buffer length: " + str(len(buffer)))

    while close_preview_request == False:        
        sock.sendto(buffer, (phone_ip,port_send_image))

        isread, img = video_capturing.read()

        resized = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
        retval, buffer = cv2.imencode('.jpg', resized, encode_param)
        
    close_preview_request = False
    sock.close()


"""To enable or disable motors control"""
def Enable():
    global previous_state
    global on_off_motors_signal
    #global ser
    global send_me
    while True:
        if on_off_motors_signal!= previous_state:
            if on_off_motors_signal == 'ON':
                let_motor_control = True
                send_me = turn_on
                print('Send Me is: ' + str(send_me))
            elif on_off_motors_signal == 'OFF':
                send_me = turn_off
                let_motor_control = False
                print('Send Me is: ' + str(send_me))
            time.sleep(0.4)
            #ser.write(send_me)
            #print(ser.readline())
            previous_state = on_off_motors_signal


"""To control motor drivers
def Motor_Control():
    global motor_signal
    #global ser

    while let_motor_control:
        #ser.write(motor_signal.encode())
        #print(ser.readline())
 """


enable_me = th.Thread(target=Enable)
send_image = th.Thread(target=Send_Image)
get_signal = th.Thread(target=Get_Signal)

"""Independent thread funcion to listen connection requests"""
def Listen():
    global phone_ip
    global send_image
    global get_signal
    global enable_me
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_listen))

    print("Start listening...")

    while True: 
        data, addr = sock.recvfrom(1024)               
        if data.count != 0:
            phone_ip = data.decode('utf-8')
            print(phone_ip)
            print("End listening..")
            
            if get_signal.is_alive()==False:
                get_signal = th.Thread(target=Get_Signal)
                get_signal.start()
            
            if send_image.is_alive() == False:
                send_image = th.Thread(target=Send_Image)
                send_image.start()

            if enable_me.is_alive() == False:
                enable_me = th.Thread(target=Enable)
                enable_me.start()

            break

        if get_signal.is_alive() == False and send_image.is_alive() == False:
            phone_ip = ""
        time.sleep(0.1)


listening = th.Thread(target=Listen)
listening.start()



while True:
    print("Motor signal is: " + motor_signal + "  UV signal is: " + uv_signal + " ON/OFF signal is: " + on_off_motors_signal)
    time.sleep(1)