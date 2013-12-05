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
    
    
    -------------------------
    
    LEGION SOCKET SERVER:
    Receives input from a legion instance (this acts in plce of KOIP),
    then forwards it back to the MiddleMan user
 */



import java.io.*;
import java.net.Socket;


public class LegionSocketServer implements Runnable {
    private Socket server;
    private String line;
    private LegionLeader linput_mediator;
    
    private BufferedReader in;
    
    public static int userCount = 0;
    
  

    LegionSocketServer(Socket server, LegionLeader im) {
      this.server = server;
      this.linput_mediator = im;
     
      // Open the connections with legion
      try { 
	      in = new BufferedReader(new InputStreamReader(server.getInputStream()));
      } catch (Exception err){
    	  System.out.println("SocketServer constructor error.");
      }
      
    }
    
    public void disconnect(){
		System.out.println("disconnecting client: " + server.getInetAddress().getHostName());
		
		in.notify();
		
		
		try {
			System.out.println("Legion SocketServer called DISCONNECT!");
			server.close();
		} catch(IOException err){
			System.out.println("Couldn't close socket connection: " + err.getLocalizedMessage());
		}
		
	}

    public void run () {
    	try {
    		System.out.println("Legion SocketServer recieved new connection! (Current users: " + userCount + "(" + server.getInetAddress().getHostName() + ")");
    		// Get input from Legion
    		while((line = in.readLine()) != null){// && !line.equals(".")) {
    			// For each line sent...
    			
	        	System.out.println("[LSS] LINE == " + line);
	        	
	        	//input_mediator.leader = Integer.parseInt(divLine[0]);
	        	//input_mediator.output(divLine[2]);
	        	
	        	// The 'action' that is returned is the input we want to use.
	        	// Parse it and forward the byte version back to VMulti (using .output())
	        	linput_mediator.output(line);
	        }
    		
    		System.out.println("Legion SocketServer closing connection!");
	        server.close();
      } catch (IOException ioe) {
        System.out.println("IOException on socket listen [LSS run]: " + ioe.toString());
        ioe.printStackTrace();
      }
      
    }
    
}