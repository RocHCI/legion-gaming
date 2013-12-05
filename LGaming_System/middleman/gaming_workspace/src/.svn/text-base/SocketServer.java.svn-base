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



import java.io.*;
import java.net.Socket;


public class SocketServer implements Runnable {
    private Socket server;
    private InputMediator input_mediator;
    
    private DataInputStream inStream;
    private BufferedReader in;
    
    public static int userCount = 0;
    

    SocketServer(Socket server, InputMediator im, int guid) {
      this.server = server;
      this.input_mediator = im;
            
      try { 
	      in = new BufferedReader(new InputStreamReader(server.getInputStream()));
	      inStream = new DataInputStream(server.getInputStream());
      } catch (Exception err){
    	  System.out.println("SocketServer constructor error.");
      }
    }
    
    public void disconnect(){
		System.out.println("disconnecting client: " + server.getInetAddress().getHostName());
		
		in.notify();
		
		
		try {
			server.close();
		} catch(IOException err){
			System.out.println("Couldn't close socket connection: " + err.getLocalizedMessage());
		}
		
		
	}

    public void run () {
    	try {
    		userCount++;
    		System.out.println("Got something! (Current users: " + userCount + " @ " + server.getInetAddress().getHostName() + ")");
    		// Get input from the client
    		// GAMING: Legion string input from controllers
    		final int BUFFER_SIZE = 13;
    		byte[] inBuffer = new byte[BUFFER_SIZE];
    		int inputSize = inStream.read(inBuffer);
    		//System.out.println("Bytes recieved: " + inputSize + " / out of: " + BUFFER_SIZE);

    		while( inputSize > 0 ) {
    			// Submit the input to the mediator
       			input_mediator.input(inBuffer);
	        	
    			inBuffer = new byte[BUFFER_SIZE];
    			inputSize = inStream.read(inBuffer);
    			//System.out.println("Bytes recieved: " + inputSize + " / out of: " + BUFFER_SIZE);
    		}
	
    		System.out.println("Inbound socket connection (controller-in) closing...");
	        server.close();
    	} catch (IOException ioe) {
    		System.out.println("IOException on socket listen [run]: " + ioe);
    		ioe.printStackTrace();
      }
      
    }
    
    String parseControllerInput(byte[] rec) {
    	String retStr = "";
    	
    	// GAMING: Should be 12 bytes
    	// NOTE: Has this changed to 13?
    	for( int i = 0; i < rec.length; i++ )
    	{
    		retStr += rec.toString() + " | ";
    		
    	}
    	
    	return retStr;
    }
}
