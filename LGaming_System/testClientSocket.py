import socket
import struct

def sendNext(mys):
    mynextbyte = bytearray()
    # LAnalog
    mynextbyte.append(255)
    mynextbyte.append(255)
    # RAnalog
    mynextbyte.append(255)
    mynextbyte.append(255)
    # Buttons
    mynextbyte.append(0)
    mynextbyte.append(255)
    mynextbyte.append(0)
    mynextbyte.append(0)
    mynextbyte.append(0)
    mynextbyte.append(0)
    mynextbyte.append(0)
    mynextbyte.append(0)
    mys.close()


HOST = 'localhost'                # The remote host
PORT = 5555              # The same port as used by the server
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((HOST, PORT))
# Assuming 225 max for analog, i.e. 8 bit
mybyte = bytearray()
# LAnalog
mybyte.append(255)
mybyte.append(255)
# RAnalog
mybyte.append(255)
mybyte.append(255)
# Buttons
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)
mybyte.append(0)

s.send(mybyte)
data = s.recv(1024)
print 'Received', repr(data)

Timer(1.0,sendNext,s)
#s.close()
