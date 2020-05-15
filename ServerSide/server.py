import io
import PIL
import cv2
import time
import socket
import random as rd
import threading as th
from PIL import Image

#hostname = socket.gethostname()
#own_ip = socket.gethostbyname(hostname)
#check this from system

own_ip = "192.168.1.6"          
phone_ip = ""
port_send_image = 55556         # this ports must be same as in the android application: Android side uses this ports:
port_get = 55555                    # 55555 to get motor controlling signals
port_listen = 55554                 # 55554 to listen incoming connection requests


motor_signal = ""               #   any motor controlling command has its specific bytes command (incoming type: byte[], used type: string)
uv_signal = ""                  #   UV light must be turned ON or OFF, (incoming type: byte[], used type: boolean)
close_motor_request = "34"      #   command which demands to close motor control and preview from here(robot side), (incoming type: byte[], used type: string )
close_preview_request = False   #   command to open or close camera preview (incoming type: _, used type: boolean)


"""To get signal from client"""
def Get_Signal():
    global close_preview_request
    global motor_signal
    global uv_signal
    global listening
    
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_get))

    print("Getting signal from clients...")

    while True:   
        data, addr = sock.recvfrom(1024)
        motor_signal = data.decode('utf-8').split('|')[0]
        uv_signal = data.decode('utf-8').split('|')[1]

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
            print("UV is ON")
            time.sleep(2)
            #TO DO..        
        elif uv_signal == "00":
            print("UV is OFF")
            time.sleep(2)


"""To send video translation to clients"""
def Send_Image():
    global close_preview_request
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) 
    video_capturing = cv2.VideoCapture(0)

    isread, pic = video_capturing.read()
    cv2.imwrite('opencv.png', pic)

    img = Image.open('opencv.png')
    img = img.resize((500, 300), PIL.Image.ANTIALIAS)
    img.save('opencv.jpg')

    with open('opencv.jpg','rb') as image:
        f = image.read()
        byte_image = bytearray(f)

    print("Sending video frames data...")

    while close_preview_request == False:        
        sock.sendto(byte_image, (phone_ip,port_send_image))

        isread, pic = video_capturing.read()
        cv2.imwrite('opencv.png', pic)
    
        img = Image.open('opencv.png')
        img = img.resize((500, 400), PIL.Image.ANTIALIAS)
        img.save('opencv.jpg')

        with open('opencv.jpg','rb') as image:
            f = image.read()
            byte_image = bytearray(f)
            time.sleep(0.01)
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

while True:
    print("Motor signal is: " + motor_signal + "  UV signal is: " + uv_signal)
    time.sleep(2)