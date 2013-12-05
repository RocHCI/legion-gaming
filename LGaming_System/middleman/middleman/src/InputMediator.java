/*
   Legion: Crowd Control of Existing Interfaces. This software enables multiple
   			users to control a single interface by combining their inputs in real-time.
    Copyright (C) 2011 Walter S. Lasecki, Kyle I. Murray, Samuel White, Jeffrey P. Bigham

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */



import java.io.DataOutputStream;
import java.io.IOException;
import java.net.Socket;
import java.util.BitSet;


abstract public class InputMediator {
	//BufferedWriter out;
	DataOutputStream dataOut;
	
	
	public InputMediator(Socket server) {
		try {
			dataOut = new DataOutputStream(server.getOutputStream());
		} catch( IOException ioe ) {
			System.err.println("Failed to create outgoing connection! --> " + ioe.getLocalizedMessage());
		}
	}
	
	public abstract void init(int ident);
	
	public abstract byte[] input(byte[] b);

	/*
	 * Convert the 12(data)+1(ID) input bytes to usable sets of strings
	 * Note: bytes are little-endian (rhs is lagest)
	 */
	public String parseByteToString(byte[] b) {
		String retStr = "";
		
		// Variables set
		int[] jPos = new int[4];	// jlX, jlY, jrX, jrY
		String triggers, dpad, face, special;
		int idNum;
		
		String controlStr = "";
		
		// Get the joystick position data
		for( int pos = 0; pos < 8; pos += 2 ) {
			BitSet high = fromByte(b[pos]);
			BitSet low = fromByte(b[pos+1]);
			controlStr = "";
			
			// Read Left_Joystick X axis
			String highStr = "";
			for( int i = high.length()-1; i >= 0; i-- ) {
				if( high.get(i) ) {
					highStr += "1";
				}
				else {
					highStr += "0";
				}
			}
			while( highStr.length() < 8 ) {
				highStr = "0" + highStr;
			}
			controlStr += highStr;
			
			String lowStr = "";
			for( int i = low.length()-1; i >= 0; i-- ) {
				if( low.get(i) ) {
					lowStr += "1";
				}
				else {
					lowStr += "0";
				}
			}
			while( lowStr.length() < 8 ) {
				lowStr = "0" + lowStr;
			}
			controlStr += lowStr;
			
			
			jPos[pos/2] = Integer.parseInt(controlStr, 2);
			//System.out.println("Joy pos #" + pos/2 + ": " + controlStr + " ==> " + jPos[pos/2]);
			
			retStr += jPos[pos/2] + " ";
		}
		
		
		// Read the rest of the input in, then divide after
		controlStr = "";
		for( int i = 8; i < 12; i++ ) {
			//System.out.println("Raw Byte: " + b[i]);
			BitSet cur = fromByte(b[i]);
			String tempStr = "";
			
			// Read Left_Joystick X axis
			//for( int j = 0; j < cur.length(); j++ ) {
			for( int j = cur.length()-1; j >= 0; j-- ) {
				if( cur.get(j) ) {
					tempStr += "1";
				}
				else {
					tempStr += "0";
				}
			}
			// Add left padding
			while( tempStr.length() < 8 ) {
				tempStr = "0" + tempStr;
			}
			controlStr += tempStr;
			//System.out.println("Size: " + controlStr.length() + " (+" + cur.length() + ")");
		}
		
		// Get the ID (13th position)
		BitSet cur = fromByte(b[12]);
		String tempStr = "";
		
		for( int j = cur.length()-1; j >= 0; j-- ) {
			if( cur.get(j) ) {
				tempStr += "1";
			}
			else {
				tempStr += "0";
			}
		}
		
		// Add left padding
		while( tempStr.length() < 8 ) {
			tempStr = "0" + tempStr;
		}
		idNum = Integer.parseInt(tempStr, 2);
		
		// Test print
		//System.out.println(controlStr);
		
		
		
		// Get the trigger input: L1, L2, L3, R1, R2, R3
		triggers = controlStr.substring(0,6);
		//System.out.println("Triggers: " + triggers);
		retStr += triggers;
		
		// Get the D-pad input: UP, DOWN, LEFT, RIGHT
		dpad = controlStr.substring(6,10);
		//System.out.println("D-Pad: " + dpad);
		retStr += dpad;
		
		// Get the button input: S,T,C,X
		face = controlStr.substring(10,14);
		//System.out.println("Buttons: " + face);
		retStr += face;
		
		// Get the special button input: St, Sel, PS
		special = controlStr.substring(14,16);
		special += " " + controlStr.substring(23,24);
		//System.out.println("Special: " + special);
		retStr += special;
		
		retStr += " ";
		retStr += idNum;
		
		
		return retStr;
	}
	
	public byte[] parseStringToBytes(String inStr) {
		String[] elems = inStr.split(" ");
		System.out.println("inStr = " + inStr + " (size: " + elems.length + ")");
		byte[] retAry = new byte[12];
		String tempStr = "";
		
		// First convert the joystick coords to string 
		for( int i = 0; i < 4; i++ ) {
			String toAdd = Integer.toString(Integer.parseInt(elems[i]), 2);
			// Could reverse this?
			//toAdd = reverseString(toAdd);
			while( toAdd.length() < 8 ) {
				toAdd = "0" + toAdd;
			}
			//System.out.println("to Add is of size: " + toAdd.length());
			tempStr += "00000000";
			tempStr += toAdd;
		}
		//System.out.println("tail elem size: " + elems[4].length());
		tempStr += elems[4];
		
		//System.out.println("Parsing string:: " + tempStr + " (size: " + tempStr.length() + ")");
		// Then convert the string to bytes
		for( int i = 0; i < 10; i++ ) {
			retAry[i] = binToByte(tempStr.substring(i*8, (i+1)*8));
		}
		
		retAry[10] = Byte.parseByte(elems[5], 2);
		//System.out.println("Byte 10 -> " + retAry[10]);
		retAry[11] = 0;
		
		//System.out.print("Sending string ==> ");
		for(int i = 0; i < retAry.length; i++ ) {
			//System.out.println("Byte [" + i + "] = " + retAry[i]);
		}
		
		return retAry;
	}
	
	public String reverseString(String inStr) {
		String retStr = "";
		
		for( int i = inStr.length()-1; i > 0; i-- ) {
			retStr += inStr.substring(i-1, i);
		}
		
		return retStr;
	}
	
	// Convert an n-ary string to a byte
	public byte binToByte(String str) {
		int sum = 0;
		for( int i = str.length(); i > 0; i-- ) {
			if( str.substring(i-1, i).equals("1") ) {
				sum += Math.pow(2,7-(i-1));
				//System.out.println("2^" + (7-(i-1)) + " = " + Math.pow(2,7-(i-1)));
			}
		}
		
		//System.out.println("Sum of [" + str + "] = " + sum);
		
		return (byte)sum;
	}
	
	public static BitSet fromByte(byte b)  
	{  
	    BitSet bits = new BitSet(8);  
	    for (int i = 0; i < 8; i++)  
	    {  
	        bits.set(i, (b & 1) == 1);  
	        b >>= 1;  
	    }
	    
	    return bits;  
	}  
	
	/*public void writeData(String str) {
		try {
			out.write(str + "\n");
			out.flush();
		} catch( IOException e ) {
			System.err.println("Logging error: " + e.toString());
		}
	}*/
	
	public void sendData(byte[] send) {
		try {
			dataOut.write(send);
			dataOut.flush();
		} catch( IOException e ) {
			System.err.println("Logging error: " + e.toString());
		}
	}

}
