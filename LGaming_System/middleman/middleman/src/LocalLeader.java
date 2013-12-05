import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.Random;


public class LocalLeader extends InputMediator {
	HashMap<Integer,String> inputTable;
	HashMap<Integer,Integer> voteTable;
	LinkedList<Integer> players;
	
	
	
	HashMap<Integer,Boolean> selIsPressed;
	int controllerSmoothing = 0;	// 0 = just use a toggle
	int synchCount = 0;
	
	int curVote = 0;
	int leader = -1;
	long leaderStartTime = 0;
	long leaderMaxTimeSpan = 30000;
	long leaderMinTimeSpan = 000;
	
	LocalLeader(Socket server) {
		super(server);
		inputTable = new HashMap<Integer,String>();
		players = new LinkedList<Integer>();
		voteTable = new HashMap<Integer,Integer>();
		selIsPressed = new HashMap<Integer,Boolean>();
		//
	}
	
	public void init(int ident) {
		if( players.size() == 0 ) {
			leader = ident;
		}
		players.add(ident);
		voteTable.put(ident, players.indexOf(ident));
		selIsPressed.put(ident, false);
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
		
		
		inputTable.put(uID, curInput);
		if( !players.contains(uID) ) {
			init(uID);
		}
		
		// check for the select button (
		String[] deci = curInput.split(" ");
		String curStr = deci[deci.length-2];
		System.out.println("Checking: " + curStr);
		if( selIsPressed.get(uID) == null ) {
			System.out.println("Failing on null sel-table lookup!");
		}
		System.out.println(curStr.substring(curStr.length()-1, curStr.length()));
		if( !selIsPressed.get(uID) && curStr.substring(curStr.length()-1, curStr.length()).equals("1") ) {
			// Increment the player the user is voting for
			System.out.println("SELECT PRESSED! Changing vote from: " + voteTable.get(uID) + " to " + (voteTable.get(uID)+1 % players.size()) );
			voteTable.put(uID, (voteTable.get(uID)+1) % players.size());
			
			selIsPressed.put(uID, true);
		}
		else {
			if( synchCount > controllerSmoothing ) {
				selIsPressed.put(uID, false);
				synchCount = 0;
			}
			else {
				synchCount++;
			}
		}
		
		System.out.println("Vote currently cast for: " + voteTable.get(uID));
		
		// If the leader is the one inputting, forward it
		if( uID == leader ) {
			System.out.println("Following user #" + uID);
			sendData(parseStringToBytes(curInput));
		}
		
		// Check for new leader once time has expired for the current one
		long elapsedTime = System.currentTimeMillis() - leaderStartTime;
		System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderMaxTimeSpan);
		if( elapsedTime > leaderMaxTimeSpan ) {
			System.out.println("Finding a new leader!");
			// Change the leader
			System.out.println("voteTable size: " + voteTable.size());
			HashMap<Integer,Integer> tally = new HashMap<Integer, Integer>();
			for( int key : voteTable.keySet() ) {
				tally.put(voteTable.get(key), (tally.get(voteTable.get(key)) != null ? tally.get(voteTable.get(key))+1 : 1));
			}
			System.out.println("tally size: " + tally.size());
			
			System.out.println("Selecting a candidate... from " + tally.keySet().size());
			// Select all the best candidates
			int max = 0;
			ArrayList<Integer> maxSet = new ArrayList<Integer>();
			for( int key : tally.keySet() ) {
				System.out.print(".");
				// Find the best next candidate
				if( key != players.indexOf(leader) && tally.get(key) >= max ) {
					if( tally.get(key) > max ) {
						maxSet = new ArrayList<Integer>();
					}
					maxSet.add(key);
				}
				else {
					System.out.println("Odd... Player" + key + " has " + tally.get(key) + " votes.");
				}
			}
			
			if( maxSet.contains(players.indexOf(leader) )) {
				System.out.println("--> WHAT?!");
			}
			
			System.out.println("Lotto!");
			if( maxSet.size() > 0 ) {
				// Select randomly from the best set
				Random rand = new Random();
				System.out.println("Old leader: " + leader + " --> new candidates: " + maxSet.size());
				leader = players.get(maxSet.get(rand.nextInt(maxSet.size())));
				leaderStartTime = System.currentTimeMillis();
				System.out.println("New leader: " + leader);
			}
			else {
				System.out.println("NO NEW LEADER FOUND!");
			}
			
			System.out.println("New leader is " + leader);
		}
		else if( elapsedTime > leaderMinTimeSpan ) {
			System.out.println("Seeing if there is a better leader!");
			// Check if there is a better leader
			HashMap<Integer,Integer> tally = new HashMap<Integer, Integer>();
			for( int key : voteTable.keySet() ) {
				tally.put(voteTable.get(key), (tally.get(voteTable.get(key)) != null ? tally.get(voteTable.get(key))+1 : 1));
			}
			
			// Select all the best candidates
			int max = 0;
			ArrayList<Integer> maxSet = new ArrayList<Integer>();
			for( int key : tally.keySet() ) {
				System.out.print(".");
				// Find the best next candidate
				if( tally.get(key) >= max ) {
					if( tally.get(key) > max ) {
						maxSet = new ArrayList<Integer>();
					}
					maxSet.add(key);
				}
			}
			
			if( maxSet.size() > 0 ) {
				// Select randomly from the best set
				Random rand = new Random();
				System.out.println("Old leader: " + leader + " | new candidates: " + maxSet.size());
				int newLeader = players.get(maxSet.get(rand.nextInt(maxSet.size())));
				System.out.println("}");
				if( newLeader != leader ) {
					System.out.println("New leader: " + leader);
					leader = newLeader;
					leaderStartTime = System.currentTimeMillis();
				}
			}
			else {
				//System.out.println("NO NEW LEADER FOUND!");
			}
		}
		else {
			// Deal with it. No leader change for you!
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
			  		leaderReport.write((leader >= 0 ? leader : "NOLEADER") + ";" + timeLeft +
			  					";" + (leaderMinTimeSpan - (curTime - leaderStartTime) < 0 ? "1" : "0") + "!");
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
