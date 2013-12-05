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



import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.ArrayList;


public class MultiMediator extends InputMediator{	
	ArrayList<InputMediator> mediatorSet;
	DataMask mask;					// NOTE: Temp fix for typing
	
	byte[] answer;
	int segSize;
	
	ArrayList<Integer> players;
	
	public MultiMediator(Socket server, DataMask inMask) {
		super(server);
		mask = inMask;
		players =  new ArrayList<Integer>();
		
		// Inheriting classes must define the set of mediators here (most often, multiple instances of one type)
		mediatorSet.add(new SlaveActiveMobLeader(server,this));
		
		// NOTE: This relies on a 13 byte string being passed
		answer = null;
	}
	
	public void init(int ident) {
		// Init the player in the IMs
		for( int i = 0; i < mediatorSet.size(); i++ ) {
			// TODO: fill in the blank spaces to make the arrays all match the original side
			mediatorSet.get(i).init(ident);
		}
		players.add(ident);
	}
	
	public byte[] input(byte[] b) {
		// Note: For later...
		//String curInput = parseByteToString(b);
		//String[] tempDeci = curInput.split(" ");
		
		// If it's the first pass, use all inputs to initialize the control
		if( answer == null ) {
			answer = b;
			segSize = (int)Math.ceil((double)answer.length/mediatorSet.size());
		}
		
		ArrayList<byte[]> partitionedInput = mask.partition(b,mediatorSet.size());
		
		for( int i = 0; i < mediatorSet.size(); i++ ) {
			recieve(mediatorSet.get(i).input(partitionedInput.get(i)), mask.getTemplate(i));
		}
		
		return null;
	}
	
	public void recieve(byte[] b, ArrayList<Boolean> indices) {
		// Send an update to the interface/console
		int idx = 0;
		for( int i = 0; i < indices.size() && idx < b.length; i++ ) {
			if( indices.get(i) ) {
				// Fill in the answer using the given segment 
				answer[i] = b[idx];
				idx++;
			}
		}
		
		sendHeartbeat();
	}
	
	private void sendHeartbeat() {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					System.out.println("Preparing to write....");

			    	// Connect to client on port 7777
					String gameclient_addr = "localhost";
					int gameclient_port = 7777;
			  		Socket gameclient = new Socket(gameclient_addr, gameclient_port);
			  		System.out.println("Socket open...");
			  		PrintWriter leaderReport = new PrintWriter(gameclient.getOutputStream());
			  		// Pass an impossible leader ID (one higher than the number of players) to signal that all players are in control 
			  		leaderReport.write((players.size()+1) + ";" + 1 +";0!");
			  		System.out.println("Done...");
			  		leaderReport.flush();
			  		leaderReport.close();
			  		
		            // Create log file
					//KeyEventDescription.rec = new BufferedWriter(new FileWriter(dateFormat.format(date) + "_Recording.log"));
			      } catch( IOException ioe ) {
			  		System.out.println("Failed to connect XX: " + ioe.toString());
			  		System.exit(0);
			      }
	}

}
