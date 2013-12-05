import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.Random;
import java.lang.InterruptedException;


public abstract class SixArmLeaderMediator extends LeaderMediator {
	int[] leaders;
	long[] leaderStartTimes;

	SixArmLeaderMediator(Socket server, String maxLeaderTime) {
		super(server, maxLeaderTime);
		leaders = new int[attribution.length];
		leaderStartTimes = new long[attribution.length];
	}
	
	public abstract int findNextLeader(int curSet);
	
	public byte[] input(byte[] b) {
		String curInput = parseByteToString(b);
		String[] tempDeci = curInput.split(" ");
		System.out.println("raw curInput return :: " + curInput + " (" + curInput.trim().length() + ")");
		System.out.println("raw uID string (len-1):: " + tempDeci[tempDeci.length-1]);
		int uID = Integer.parseInt(tempDeci[tempDeci.length-1]);
		curInput = curInput.substring(0,curInput.lastIndexOf(' '));
		System.out.println("=====================================");
		System.out.println("=====================================\n");
		//System.out.println("CURRENT LEADER: Player " + leader + " @ " + players.indexOf(leader));
		System.out.println("Total (local) players: " + players.size());
		System.out.println("\n=====================================");
		System.out.println("=====================================");
		System.out.println("[Local Leader] Current input for worker #" + uID + " = " + curInput + ".");
		
		
		if( !players.contains(uID) ) {
			init(uID);
		}

		ControllerInput ci = parseStringToCI(parseByteToString(b));
		everyTurn(uID, curInput, ci);
		LinkedList<Integer> needToUpdateLeader = new LinkedList<Integer>();
		
		// If the leader is the one inputting, forward it
		for(int i = 0; i < leaders.length; i++) {
			if( uID == leaders[i] && activePlayers.contains(uID) ) {
				System.out.println("Following user #" + uID);
				if(i < 4) {
					sharedInput.setValue(ci.getAxisValue(segmentNames[i]), segmentNames[i]);
				} else {
					System.out.println("One Input: " + ci.getAxisValue(segmentNames[i]));
					sharedInput.setValue(ci.getValue(segmentNames[i]), segmentNames[i]);
				}
				long elapsedTime = System.currentTimeMillis() - leaderStartTimes[i];
				System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderTimeSpan + " for " + i);
				if( elapsedTime > leaderTimeSpan || (!activePlayers.contains(uID))) {
					needToUpdateLeader.add(i);
				}
			} else {
				long elapsedTime = System.currentTimeMillis() - leaderStartTimes[i];
				System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderTimeSpan + " for " + i);
				if( elapsedTime > leaderTimeSpan) {
					needToUpdateLeader.add(i);
				}
			}
		}
		
		for(int curSet : needToUpdateLeader) {
			System.out.println("Moving to the next leader!");
			// Lump Y values of analog with X, so don't calculate new leader for Y values
			if(curSet != 1 && curSet != 3) {
				leaders[curSet] = findNextLeader(curSet);
				// If this is an X value, update the corresponding Y leader as well
				if(curSet == 0 || curSet == 2) {
					leaders[curSet+1] = leaders[curSet];
				}
			}
			leaderStartTimes[curSet] = System.currentTimeMillis();
		}
		
		// Transform the shared answer to a byte[]
		// Send output to controllers
		sendData(parseStringToBytes((sharedInput.toByteArrayString())));
		
		// --------------------------
		// Send update to interface
		sendHeartbeat();
		
		return null;
	}
	
	protected void sendHeartbeat() {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					System.out.println("Preparing to write....");
					long curTime = System.currentTimeMillis();
					int roundingFactor = 10000;
					long tempTimeLeft = (leaderTimeSpan - (curTime - leaderStartTimes[0]))*roundingFactor;
					tempTimeLeft /= leaderTimeSpan;
					double timeLeft = (double)tempTimeLeft / (double)roundingFactor;
					if( timeLeft < 0 ) {
						timeLeft = 0.0;
					}
					 
			    	// Connect to client on port 7777
					String gameclient_addr = "localhost";
					int gameclient_port = 7777;
			  		Socket gameclient = new Socket(gameclient_addr, gameclient_port);
			  		System.out.println("Socket open...");
			  		PrintWriter leaderReport = new PrintWriter(gameclient.getOutputStream());
			  		System.out.println("Writing... " + leaderTimeSpan + " - (" + curTime + " - "  + leaderStartTimes[0] + ") = " + (leaderTimeSpan - (curTime - leaderStartTimes[0])) );
			  		leaderReport.write((leaders[0] >= 0 ? leaders[0] : "NOLEADER") + ";" + timeLeft +";0!");
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

