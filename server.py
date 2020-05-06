import io
import PIL
import cv2
import time
import socket
import random as rd
import threading as th
from PIL import Image


udp_ip = "127.0.0.1"
port_send_gps = int(input("Please, enter the port you want to activate the server: "))
port_send_image = port_send_gps + 1
port_get = port_send_image + 1

print("Starting Python server...")

#To get signal from client
def Get_Signal():
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((udp_ip,port_get))

    print("Getting signal from clients...")

    while True:
        data, addr = sock.recvfrom(1024) # buffer size is 1024 bytes
        print("Data from server: " + str(data))
        time.sleep(0.01)

#To send gps coordinates to clients
def Send_Gps():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
    print("Sending gps data...")
    while True:
        var1 = rd.random()
        var2 = rd.random()
        str_var1 = bytes(str(var1) + ',' + str(var2),'utf-8')
        sock.sendto(str_var1, (udp_ip,port_send_gps))
        time.sleep(0.01)

#To send video translation to clients
def Send_Image():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM) # UDP
    video_capturing = cv2.VideoCapture(0)
    isread, pic = video_capturing.read()
    cv2.imwrite('opencv.png', pic)

    #AFTER SOLVE THIS PROBLEM TO MAKE SURE IT WON'T SAVE IMAGES ALONGE WITH PYTHON SCRIPT

    img = Image.open('opencv.png')
    img = img.resize((550, 400), PIL.Image.ANTIALIAS)
    img.save('check.jpg')

    with open('check.jpg','rb') as image:
        f = image.read()
        byte_image = bytearray(f)

    print("Sending video frames data...")

    while True:
        sock.sendto(byte_image, (udp_ip,port_send_image))

        isread, pic = video_capturing.read()
        cv2.imwrite('opencv.png', pic)
    
        img = Image.open('opencv.png')
        img = img.resize((550, 400), PIL.Image.ANTIALIAS)
        img.save('check.jpg')

        with open('check.jpg','rb') as image:
            f = image.read()
            byte_image = bytearray(f)
            #time.sleep(1)

#send_gps = th.Thread(target=Send_Gps)
#send_gps.start()

#send_video = th.Thread(target=Send_Image)
#send_video.start()

Get_Signal()