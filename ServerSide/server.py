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

own_ip = "192.168.11.129"          
phone_ip = ""                               # this ports must be same as in the android application: Android side uses this ports:
port_send_image = 55556                         # 55556 - to send image's bytes to client.
port_get = 55555                                # 55555 - to get motor controlling signals from client.
port_listen = 55554                             # 55554 - to listen incoming connection requests from client.

#on_off_signal = ""
motor_signal = ""                           #   any motor controlling command has its specific bytes command (incoming type: byte[], used type: string)
uv_signal = ""                              #   UV light must be turned ON or OFF, (incoming type: byte[], used type: boolean)
close_motor_request = "34"                  #   command which demands to close motor control and preview from here(robot side), (incoming type: byte[], used type: string )
close_preview_request = False               #   command to open or close camera preview (incoming type: _, used type: boolean)

#ser = serial.Serial('/dev/ttyACM0')
#ser.baudrate = 9600

"""To get signal from client"""
def Get_Signal():
    global close_preview_request
    global motor_signal
    global uv_signal
    global listening
    #global on_off_signal
    
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_get))

    print("Getting signal from clients...")

    while True:   
        data, addr = sock.recvfrom(1024)
        motor_signal = data.decode('utf-8').split('|')[0]
        uv_signal = data.decode('utf-8').split('|')[1]
        #on_off_signal = data.decode('utf-8').split('|')[2] + '\n'
        if str(motor_signal) == close_motor_request or data == None:
            close_preview_request = True
            break
        time.sleep(0.1)
    if listening.is_alive() == False:
        listening = th.Thread(target=Listen)
        listening.start()
    sock.close()

def Enable_Uv():
    global uv_signal
    while True:
        if uv_signal == "01":
            #print("UV is ON")
            time.sleep(2)
            #TO DO..        
        elif uv_signal == "00":
            #print("UV is OFF")
            time.sleep(2)


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

send_image = th.Thread(target=Send_Image)
get_signal = th.Thread(target=Get_Signal)

"""Independent thread funcion to listen connection requests"""
def Listen():
    global phone_ip
    global send_image
    global get_signal
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

            break

        if get_signal.is_alive() == False and send_image.is_alive() == False:
            phone_ip = ""
        time.sleep(0.1)


listening = th.Thread(target=Listen)
listening.start()

get_uv = th.Thread(target=Enable_Uv)
get_uv.start()

previous_state = b''

while True:
    #if on_off_signal!= previous_state:
    #    time.sleep(0.4)
    #    #print(ser.readline())

    #    #print(ser.name)
    #    #ser.write(on_off_signal)
    #    previous_state = on_off_signal
    #    print(previous_state)

    print("Motor signal is: " + motor_signal + "  UV signal is: " + uv_signal + " ON/OFF signal is: ") #on_off_signal)
    time.sleep(2)