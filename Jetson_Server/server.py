import io
import os
import PIL
import cv2
import time
import socket
import serial
import random as rd
import threading as th
from termcolor import colored
from PIL import Image

gw = os.popen("ip -4 route show default").read().split()       #This method works only on linux based systems
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.connect((gw[2], 0))
own_ip = s.getsockname()[0]

print("My ip is: " + own_ip)        
phone_ip = ""                               # this ports must be same as in the client side applications:
port_send_image = 55556                         # 55556 - to send image's bytes to client.
port_get = 55555                                # 55555 - to get motor controlling signals from client.
port_listen = 55554                             # 55554 - to listen incoming connection requests from client.
confirm_port = 45732
send_status_port = 53784

video_capturing = cv2.VideoCapture(0)       #   the camera object 
camera_number = '0'                         #   to save the command of switching into cameras
camera_previous_number = camera_number      #   to save the previous state of camera number
on_off_motors_signal = '0'                  #   to save the command of turning on/off motor drivers
motor_signal = ""                           #   any motor controlling command has its specific bytes command (incoming type: byte[], used type: string)
uv_signal = ""                              #   UV light must be turned ON or OFF, (incoming type: byte[], used type: string)
uv_signal_2 = ""
line_track_signal = ""
close_motor_request = "34"                  #   command which demands to close motor control and preview from here(robot side), (incoming type: byte[], used type: string )
close_preview_request = False               #   command to open or close camera preview (incoming type: byte[], used type: boolean)
let_motor_control = False
close_send_state = False

previous_state = '00'                       #   keeps the previous signal from client
turn_on = b'1\n'                            #   signal, which value must be sent to Arduino to enable motors control
turn_off = b'2\n'                           #   signal, which value must be sent to Arduino to disable motors control
send_me = b''                               #   signal, which must be sent to Arduino (turn ON or turn OFF)


forward = b'M1,20,1,20\n'                   #   to command motor drivers to move robot forward
backward = b'M0,15,0,15\n'                  #   to command motor drivers to move robot backward
left = b'M1,15,0,15\n'                      #   to command motor drivers to move robot left
right = b'M0,15,1,15\n'                     #   to command motor drivers to move robot right
stop = b'M0,1,0,1\n'                        #   to command motor drivers to stop wheels
send_motor_signal = b''                     #   signal, which must be sent to Arduino
previous_motor_state = '00'                 #   keeps the previous signal from client

uv1_on = b'I1\n'                            #   to enable UV Invertor
uv1_off = b'I0\n'                           #   to disable UV Invertor
uv2_on = b'U1\n'                            #   to enable UV Light
uv2_off = b'U0\n'                           #   to disable UV Light
send_uv1 = b''                              #   signal, which must be sent to Arduino
send_uv2 = b''                              #   signal, which must be sent to Arduino
previous_uv1_state = '00'                   #   keeps the previous signal for UV 1 from client
previous_uv2_state = '00'                   #   keeps the previous signal for UV 2 from client

line_track_on = b'L1\n'                     #   to enable line tracking mode
line_track_off = b'L0\n'                    #   to disable line tracking mode and enable hand control
send_line_track = b''                       #   signal, which must be sent to Arduino
previous_mode_state = '0'                   #   keeps the previous signal from client

motor_current_state = "0"                   #   the status of the motor drivers
uv1_current_state = "0"                     #   the status of the UV lamp's power 1st level
uv2_current_state = "0"                     #   the status of the UV lamp's power 2nd level
line_tracking_current_state = "0"           #   the status of the line tracking mode

lock = False

line_state = ''
ser = serial.Serial('/dev/ttyACM0')
ser.baudrate = 9600
ser.timeout = 1

#This function helps to prepair CSI camera (for ex. Jetson Nano Sony Camera)
def gstreamer_pipeline(
    capture_width=600,
    capture_height=400,
    display_width=600,
    display_height=400,
    framerate=20,
    flip_method=0,
):
    return (
        "nvarguscamerasrc ! "
        "video/x-raw(memory:NVMM), "
        "width=(int)%d, height=(int)%d, "
        "format=(string)NV12, framerate=(fraction)%d/1 ! "
        "nvvidconv flip-method=%d ! "
        "video/x-raw, width=(int)%d, height=(int)%d, format=(string)BGRx ! "
        "videoconvert ! "
        "video/x-raw, format=(string)BGR ! appsink"
        % (
            capture_width,
            capture_height,
            framerate,
            flip_method,
            display_width,
            display_height,
        )
    )


#This function helps to get signal from client applications using UDP
def Get_Signal():
    global close_preview_request
    global motor_signal
    global uv_signal
    global uv_signal_2
    global listening
    global on_off_motors_signal
    global line_track_signal
    global close_send_state
    global camera_number

    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_get))
    sock.settimeout(3)
    print("Getting signal from clients...")

    while True:
        try:
            data, addr = sock.recvfrom(1024)
        except:
            print("No connection... exiting...")
            uv_signal = "0"
            uv_signal_2 = "0"
            motor_signal = "0"
            on_off_motors_signal = "0"
            close_preview_request = True
            close_send_state = True
            time.sleep(0.5)
            break
        signal_array = data.decode('utf-8').split('|')
        try:
            motor_signal = signal_array[0]
            on_off_motors_signal = signal_array[1]
            uv_signal = signal_array[2]
            uv_signal_2 = signal_array[3]
            line_track_signal = signal_array[4]
            camera_number = signal_array[5]
        except:
            print("Something goes wrong...Couldn't get signal")
        if str(motor_signal) == close_motor_request or data == None:
            close_preview_request = True
            close_send_state = True
            break
        #time.sleep(0.4)
    if listening.is_alive() == False:
        listening = th.Thread(target=Listen)
        listening.start()
    sock.close()


#This function helps to send image frames (as video streaming method) to client applications using UDP
def Send_Image():
    global close_preview_request
    global camera_number
    
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    if camera_number == '0':
        video_capturing = None
        video_capturing = cv2.VideoCapture(gstreamer_pipeline(flip_method=0), cv2.CAP_GSTREAMER)
    elif camera_number == '1':
        video_capturing = None
        video_capturing = cv2.VideoCapture(1)

    isread, img = video_capturing.read()
    scale_percent = 70
    width = int(img.shape[1] * scale_percent / 100)
    height = int(img.shape[0] * scale_percent / 100)
    dim = (width, height)
    encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), 70]

    resized = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
    retval, buffer = cv2.imencode('.jpg', resized, encode_param)

    print("Sending video frames data... Buffer length: " + str(len(buffer)))

    while close_preview_request == False:
        sock.sendto(buffer, (phone_ip,port_send_image))
        time.sleep(0.01)
        isread, img = video_capturing.read()
        resized = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
        retval, buffer = cv2.imencode('.jpg', resized, encode_param)
        #print("Sending video frames data... Buffer length: " + str(len(buffer)))
    close_preview_request = False
    sock.close()

#this funcion helps to send control commands to Arduino using serial port
def Send_Arduino():
    global previous_state
    global previous_motor_state, previous_mode_state
    global previous_uv1_state, previous_uv2_state
    global on_off_motors_signal
    global ser
    global lock
    global send_me
    global send_motor_signal, send_line_track
    global send_uv1, send_uv2
    global motor_current_state, uv1_current_state, uv2_current_state, line_tracking_current_state
    while True:
        if on_off_motors_signal!= previous_state and lock == False:
            lock = True
            if on_off_motors_signal == 'ON':
                send_me = turn_on
                print('Send Me is: ' + str(send_me))
            elif on_off_motors_signal == 'OFF':
                send_me = turn_off
                print('Send Me is: ' + str(send_me))
            previous_state = on_off_motors_signal
            #time.sleep(0.4)
            ser.write(send_me)
            try:
                motor_current_state = ser.readline().decode()[0]
            except:
                motor_current_state = "0"
            lock = False
        if motor_signal != previous_motor_state and lock == False:
            lock = True
            if motor_signal == "1":
                send_motor_signal = forward
            elif motor_signal == "2":
                send_motor_signal = backward
            elif motor_signal == "3":
                send_motor_signal = left
            elif motor_signal == "4":
                send_motor_signal = right
            elif motor_signal == "0":
                send_motor_signal = stop
            #time.sleep(0.4)
            previous_motor_state = motor_signal
            ser.write(send_motor_signal)
            ser.readline()
            lock = False
        if uv_signal != previous_uv1_state and lock == False:
            lock = True
            if uv_signal == "1":
                send_uv1 = uv1_on
            elif uv_signal == "0":
                send_uv1 = uv1_off
            #time.sleep(0.4)
            ser.write(send_uv1)
            uv1_current_state = ser.readline().decode()[:2]
            previous_uv1_state = uv_signal
            lock = False
        if uv_signal_2 != previous_uv2_state and lock == False:
            lock = True
            if uv_signal_2 == "1":
                send_uv2 = uv2_on
            elif uv_signal_2 == "0":
                send_uv2 = uv2_off
            #time.sleep(0.4)
            ser.write(send_uv2)
            previous_uv2_state = uv_signal_2
            uv2_current_state = ser.readline().decode()[:2]
            lock = False
        if line_track_signal!= previous_mode_state and lock == False:
            if line_track_signal == "1":
                send_line_track = line_track_on
            elif line_track_signal == "0":
                send_line_track = line_track_off
            time.sleep(0.4)
            previous_mode_state = line_track_signal
            ser.write(send_line_track)
            line_tracking_current_state = ser.readline().decode()[:2]
            lock = False

#This function helps to confirm requested connection
def Confirm():
    buffer = b'iletyouconnectme'
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    for i in range(1,10):
        sock.sendto(buffer, (phone_ip,confirm_port))
        time.sleep(0.5)

#This function helps to send robot's parts state information to client application
def Send_Status():
    global close_send_state
    global motor_current_state, uv1_current_state, uv2_current_state, line_tracking_current_state
    global ser
    global lock
    command = b'S\n'
    online_state = '0'
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    while close_send_state == False:
        if lock == False:
            lock = True
            ser.write(command)
            try:
                online_state = ser.readline().decode()[0]
            except:
                online_state = '0'
            message = online_state + '|' + str(motor_current_state) + '|' + uv1_current_state + '|' + uv2_current_state
            sock.sendto(message.encode(), (phone_ip,send_status_port))
            print("STATE MESSAGE: " + message)
            lock = False
        time.sleep(3)
    close_send_state = False

confirm_you = th.Thread(target=Confirm)                         #   Connection confirmation thread instance
send_me_arduino = th.Thread(target=Send_Arduino)                #   Jetson -> Arduino communication thread instance
send_image = th.Thread(target=Send_Image)                       #   Jetson -> Client video communication thread instance
get_signal = th.Thread(target=Get_Signal)                       #   Jetson <- Client communication thread instance
send_status = th.Thread(target=Send_Status)                     #   Jetson -> Client status communication thread instance

#This function starts with the application and listens to requested connections
def Listen():
    global phone_ip
    global send_image
    global get_signal
    global send_me_arduino, confirm_you
    global on_off_signal
    global send_me, send_status
    global previous_motor_state
    previous_motor_state = '00'
    on_off_signal = '00'
    time.sleep(0.5)
    send_me = b''
    sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    sock.bind((own_ip,port_listen))
    sock.settimeout(1000)

    print("Start listening...")

    while True:
        data, addr = sock.recvfrom(1024)
        if data.count != 0:
            check_message = data.decode('utf-8').split('.')
            if len(check_message) != 4:
                continue
            phone_ip = data.decode('utf-8')
            print(phone_ip)
            print("End listening..")
            if confirm_you.is_alive()==False:
                confirm_you = th.Thread(target=Confirm)
                confirm_you.start()

            if get_signal.is_alive()==False:
                get_signal = th.Thread(target=Get_Signal)
                get_signal.start()

            if send_image.is_alive() == False:
                send_image = th.Thread(target=Send_Image)
                send_image.start()

            if send_me_arduino.is_alive() == False:
                send_me_arduino = th.Thread(target=Send_Arduino)
                send_me_arduino.start()

            if send_status.is_alive() == False:
                send_status = th.Thread(target=Send_Status)
                send_status.start()
            break

        if get_signal.is_alive() == False and send_image.is_alive() == False:
            phone_ip = ""
        time.sleep(0.1)


listening = th.Thread(target=Listen)                            #   Connection requests listening thread's instance
listening.start()


while True:
    if(camera_previous_number!=camera_number):
        close_preview_request = True
        time.sleep(0.4)
        camera_previous_number = camera_number
        send_image = th.Thread(target=Send_Image)
        send_image.start()
    #print(motor_current_state + ' ' + uv1_current_state + ' ' + uv2_current_state + ' ' + line_tracking_current_state)
    print("UV signal is: " + uv_signal + " , " + uv_signal_2 + colored(' |','green') + " ON/OFF signal is: " + on_off_motors_signal + colored(' |','green') + " Send me: " + str(send_me) + colored(' |','green') + " Previous motor state: " + previous_state + colored(' |','green') + " Motor signal is: " + str(send_motor_signal) + colored(' |','green') + " Line track signal is: " + line_track_signal)
    time.sleep(1)

