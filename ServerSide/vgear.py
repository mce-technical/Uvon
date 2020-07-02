# import required libraries
from vidgear.gears import VideoGear
import cv2
import socket

options = {"hflip": True, "exposure_mode": "auto", "iso": 800, "exposure_compensation": 15, "awb_mode": "horizon", "sensor_mode": 0}

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(("192.168.1.5", 55556))
stream = VideoGear(resolution=(128, 128), framerate=60, logging = True, **options).start() 

while True:

    frame = stream.read()
    if frame is None:
        break
        
    byte_image = bytearray(frame)
    print(len(byte_image))
    with open('opencv.jpg','rb') as image:
        f = image.read()
        byte_image = bytearray(f)
    sock.send(byte_image)
    # {do something with the frame here}


    cv2.imshow("Output Frame", frame)

    # check for 'q' key if pressed
    key = cv2.waitKey(1) & 0xFF
    if key == ord("q"):
        sock.close()
        break

# close output window
cv2.destroyAllWindows()

# safely close video stream
stream.stop()