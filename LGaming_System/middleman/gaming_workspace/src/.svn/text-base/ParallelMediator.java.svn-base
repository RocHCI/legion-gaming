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
import java.util.Random;


abstract public class ParallelMediator extends InputMediator{	

	// Set X Player
	ArrayList<ArrayList<String>> activeInputs = new ArrayList<ArrayList<String>>();
	
	// This has to be global!
	ControllerInput sharedInput = new ControllerInput(128,128,128,128,"0000000000000000");
	
	// NOTE: This relies on a 13 byte string being passed
	byte[] answer = {(byte)0,(byte)128,(byte)0,(byte)128,(byte)0,(byte)128,(byte)0,(byte)128,(byte)0,(byte)0,(byte)0,(byte)0,(byte)0};
	boolean firstTime = true;
	
	public ParallelMediator(Socket server) {
		super(server);
	}
	
	public void init(int ident) {
		players.add(ident);
		activePlayers.add(ident);
	}
	
	public abstract void everyTurn(int curSet, int uID, String curInput, ControllerInput ci);
	
	public abstract String findNextConfig(int curSet);
	
	public abstract AnalogPair findNextAnalogConfig(int curSet);

	public byte[] input(byte[] b) {
		// If it's the first pass, use all inputs to initialize the control
		if( firstTime == true ) {
			System.out.println("Zeroing @ " + b.toString());
			if( answer == null ) {
				System.out.println("no answer!");
			}
			
			firstTime = false;
		}
		
		////////////
		// TEST -- does this input all come in?
		String curInput = parseByteToString(b);
		String[] tempDeci = curInput.split(" ");
		System.out.println("raw curInput return :: " + curInput + " (" + curInput.trim().length() + ")");
		System.out.println("raw uID string (len-1):: " + tempDeci[tempDeci.length-1]);
		int uID = Integer.parseInt(tempDeci[tempDeci.length-1]);
		curInput = curInput.substring(0,curInput.lastIndexOf(' '));
		System.out.println("\n=====================================");
		System.out.println("=====================================");
		System.out.println("[Local Leader] Current input for worker #" + uID + " = " + curInput + ".");
		
		if( !players.contains(uID) ) {
			init(uID);
		}

		ControllerInput ci = parseStringToCI(parseByteToString(b));
		
		System.out.println("activeInputs size: " + activeInputs.size());
		
		for( int curSet = 0; curSet < attribution.length; curSet++ ) {
			everyTurn(curSet, uID, curInput, ci);

			// If it's time to pass on input
			// NOTE: We don't pass on input during every player's turn because the last player's input is almost always the only one that gets through
			if(uID == players.size()-1) {
				if(curSet < 4) {
					// Y values get lumped into X
					if(curSet == 0 || curSet == 2) {
						sharedInput.setValue(findNextAnalogConfig(curSet), segmentNames[curSet]);
					}
				} else {
					sharedInput.setValue(findNextConfig(curSet), segmentNames[curSet]);
				}
				
				// Reset the ArrayList
				activeInputs.set(curSet, new ArrayList<String>());
			}
		}
		
		// Transform the shared answer to a byte[]
		// Send output to controllers
		sendData(parseStringToBytes((sharedInput.toByteArrayString())));
		
		// Send update to interface
		sendHeartbeat();

		return null;
	}
	
	
	private void sendHeartbeat() {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					System.out.println("Preparing to write....");

			    	// Connect to client on port 7777
					String gameclient_addr = "localhost";
					int gameclient_port = 7777;
			  		Socket gameclient = new Socket(gameclient_addr, gameclient_port);
			  		//System.out.println("Socket open...");
			  		PrintWriter leaderReport = new PrintWriter(gameclient.getOutputStream());
			  		// Pass an impossible leader ID (one higher than the number of players) to signal that all players are in control 
			  		leaderReport.write((players.size()+1) + ";" + 1 +";0!");
			  		//System.out.println("Done...");
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
