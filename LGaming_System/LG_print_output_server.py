# Echo server program
import socket
import array

HOST = ''                 # Symbolic name meaning the local host
PORT = 7698               # Arbitrary non-privileged port


def tobits(s):
	result = []
	for c in s:
		bits = bin(ord(c))[2:]
		bits = '00000000'[len(bits):] + bits
		result.extend([int(b) for b in bits])
	return result


s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((HOST, PORT))
s.listen(1)
conn, addr = s.accept()
print 'Connected by', addr
while 1:
	data = conn.recv(1024)
	#if not data: break
	if data: 
		print 'Received', repr(data)
		data_B = array.array('B',data)
		data_b = tobits(data)
		print_str = "Player "+str(data_B[12])+" -    Analog: "+str(data_B[1])+" "+str(data_B[3])+" "+str(data_B[5])+" "+str(data_B[7])
		for x in data_b[64:80]:
			print_str = print_str+" "+str(x)
		# Comment back in when this is officially added to codebase
		#print_str = print_str + "    Code: " + str(data_B[13])
		print print_str
# Never get here
conn.close()