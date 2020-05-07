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
port_send_image = 55556
port_get = 55555 
port_listen = 55554
port_uv = 59472


motor_signal = ""
uv_signal = False
close_preview_motor = "b'34'"
isTerminatedRequest = False


#To get signal from client
def Get_Signal():
    global isTerminatedRequest
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_get))

    print("Getting signal from clients...")

    while True:   
        data, addr = sock.recvfrom(1024)
        print("Data from server: " + str(data))
        if str(data)==close_preview_motor:
            isTerminatedRequest = True
            break
        time.sleep(0.1)

    sock.close()

def Enable_Uv():
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_uv))

    print("Started UV thread...")

    while True:   
        data, addr = sock.recvfrom(1024)
        print("Data from server: " + str(data))
        if str(data) != "b'00'":
            uv_signal = False
            print("Signal state: " + str(uv_signal))
            break
        uv_signal = True
        print("Signal state: " + str(uv_signal))
        print("Data uv: " + str(data))     
        time.sleep(0.01)
    listening = th.Thread(target=Listen)
    listening.start()
    sock.close()

#To send video translation to clients
def Send_Image():
    global isTerminatedRequest
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) 
    video_capturing = cv2.VideoCapture(0)
    #video_capturing.set(cv2.CAP_PROP_FPS, 50)
    
    isread, pic = video_capturing.read()
    cv2.imwrite('opencv.png', pic)

    img = Image.open('opencv.png')
    img = img.resize((500, 300), PIL.Image.ANTIALIAS)
    img.save('opencv.jpg')

    with open('opencv.jpg','rb') as image:
        f = image.read()
        byte_image = bytearray(f)

    print("Sending video frames data...")

    while isTerminatedRequest == False:        
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
    isTerminatedRequest = False
    sock.close()

send_image = th.Thread(target=Send_Image)
get_signal = th.Thread(target=Get_Signal)


def Listen():
    global phone_ip
    global send_image
    global get_signal
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_listen))

    print("Start listening...")

    while True: 
        data, addr = sock.recvfrom(1024) # buffer size is 1024 bytes
        print("Listening: " + str(data))

        if data.count != 0:
            data_list = str(data)
            phone_ip = data_list.split('\'')[1]
            print(phone_ip)
            print("End listening..")
            
            if get_signal.is_alive()==False:
                get_signal = th.Thread(target=Get_Signal)
                get_signal.start()
            
            if send_image.is_alive() == False:
                send_image = th.Thread(target=Send_Image)
                send_image.start()

            get_uv = th.Thread(target=Enable_Uv)
            get_uv.start()
            break
        time.sleep(0.1)


listening = th.Thread(target=Listen)
listening.start()