import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;


public class MobLeader extends InputMediator {
	LinkedList<Integer> players;
	
	int leader = -1;
	long leaderStartTime = 0;
	long leaderMaxTimeSpan = 50;
	
	MobLeader(Socket server) {
		super(server);
		players = new LinkedList<Integer>();
		//
	}
	
	public void init(int ident) {
		if( players.size() == 0 ) {
			leader = ident;
		}
		players.add(ident);
	}
	
	public byte[] input(byte[] b) {
		String curInput = parseByteToString(b);
		String[] tempDeci = curInput.split(" ");
		System.out.println("raw curInput return :: " + curInput + " (" + curInput.trim().length() + ")");
		System.out.println("raw uID string (len-1):: " + tempDeci[tempDeci.length-1]);
		int uID = Integer.parseInt(tempDeci[tempDeci.length-1]);
		curInput = curInput.substring(0,curInput.lastIndexOf(' '));
		System.out.println("=====================================");
		System.out.println("=====================================\n");
		System.out.println("CURRENT LEADER: Player " + leader + " @ " + players.indexOf(leader));
		System.out.println("Total (local) players: " + players.size());
		System.out.println("\n=====================================");
		System.out.println("=====================================");
		System.out.println("[Local Leader] Current input for worker #" + uID + " = " + curInput + ".");
		
		
		if( !players.contains(uID) ) {
			init(uID);
			
			// check for the select button (
			String[] deci = curInput.split(" ");
			String curStr = deci[deci.length-2];
			System.out.println("Checking: " + curStr);
			//System.out.println(curStr.substring(curStr.length()-1, curStr.length()));
		}
		
		
		
		// If the leader is the one inputting, forward it
		if( uID == leader ) {
			System.out.println("Following user #" + uID);
			sendData(parseStringToBytes(curInput));
		}
		
		
		// Check for new leader once time has expired for the current one
		long elapsedTime = System.currentTimeMillis() - leaderStartTime;
		System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderMaxTimeSpan);
		if( elapsedTime > leaderMaxTimeSpan ) {
			System.out.println("Moving to the next leader!");
			
			leader = (leader + 1) % players.size();
			leaderStartTime = System.currentTimeMillis();
		}
		
		
		// --------------------------
		// Send update to interface
		sendHeartbeat();
		
		return null;
	}
	
	private void sendHeartbeat() {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					System.out.println("Preparing to write....");
					long curTime = System.currentTimeMillis();
					int roundingFactor = 10000;
					long tempTimeLeft = (leaderMaxTimeSpan - (curTime - leaderStartTime))*roundingFactor;
					tempTimeLeft /= leaderMaxTimeSpan;
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
			  		System.out.println("Writing... " + leaderMaxTimeSpan + " - (" + curTime + " - "  + leaderStartTime + ") = " + (leaderMaxTimeSpan - (curTime - leaderStartTime)) );
			  		// Pass an impossible leader ID (one higher than the number of players) to signal that all players are in control 
			  		leaderReport.write((players.size()+1) + ";" + timeLeft +";0!");
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

