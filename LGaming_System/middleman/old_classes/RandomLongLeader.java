import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;
import java.util.Random;


public class RandomLongLeader extends InputMediator {
	LinkedList<Integer> players;
	LinkedList<Integer> skipmenot;
	LinkedList<Integer> selPressedPrev;
	int leader = -1;
	long leaderMaxTimeSpan = 30000;
	long leaderStartTime = 0;

	
	RandomLongLeader(Socket server) {
		super(server);
		players = new LinkedList<Integer>();
		skipmenot = new LinkedList<Integer>();
		selPressedPrev = new LinkedList<Integer>();
		//
	}
	
	public void init(int ident) {
		players.add(ident);
		skipmenot.add(ident);
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
		}
		
		// check for the select button (
		String[] deci = curInput.split(" ");
		String curStr = deci[deci.length-2];
		System.out.println("Checking: " + curStr);
		// If the select button is pressed...
		if( curStr.substring(curStr.length()-1, curStr.length()).equals("1") ) {
			if( skipmenot.contains(uID) && !selPressedPrev.contains((Integer)uID) ) {
				skipmenot.remove((Integer)uID);
				System.out.println("-=-=-=-=-= skipmode " + uID + ": false");
			} else if( !skipmenot.contains(uID) && !selPressedPrev.contains((Integer)uID) ){
				skipmenot.add((Integer)uID);
				System.out.println("-=-=-=-=-= skipmode " + uID + ": true");
			}
			
			if( !selPressedPrev.contains((Integer)uID) ) {
				selPressedPrev.add(uID);
			}
		}
		else {
			if( selPressedPrev.contains((Integer)uID) ) {
				selPressedPrev.remove((Integer)uID);
			}
		}
		
		System.out.println("-=-=-= new skipmenot: " + skipmenot.toString());
		
		// If the leader is the one inputting, forward it
		if( uID == leader && skipmenot.contains(uID) ) {
			System.out.println("Following user #" + uID);
			sendData(parseStringToBytes(curInput));
		}
		

		// Check for new leader once time has expired for the current one
		long elapsedTime = System.currentTimeMillis() - leaderStartTime;
		System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderMaxTimeSpan);
		if( elapsedTime > leaderMaxTimeSpan  || (uID == leader && !skipmenot.contains(uID))) {
			System.out.println("Finding a new leader!");
			// Change the leader
			System.out.println("Lotto!");
			// Select randomly from the best set
			Random rand = new Random();
			System.out.println("Old leader: " + leader + " --> new candidates: " + skipmenot.size());
			int oldLeader = leader;
			if(skipmenot.size() > 1) {
				while( leader == oldLeader ) {
					//leader = players.get(players.get(rand.nextInt(players.size())));
					leader = skipmenot.get(rand.nextInt(skipmenot.size()));
				}
			} else {
				leader = skipmenot.getFirst();
			}
			leaderStartTime = System.currentTimeMillis();
			System.out.println("New leader: " + leader);
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
			  		leaderReport.write((leader >= 0 ? leader : "NOLEADER") + ";" + timeLeft + ";0!");
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
