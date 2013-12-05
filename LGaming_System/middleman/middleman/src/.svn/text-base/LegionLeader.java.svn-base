import java.io.IOException;
import java.io.PrintWriter;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.HashMap;
import java.util.LinkedList;


public class LegionLeader extends InputMediator {
	HashMap<Integer,String> inputTable;
	LinkedList<Integer> players;
	
	int leader = -1;
	long leaderStartTime = 0;
	long leaderMaxTimeSpan = 30000;
	long leaderMinTimeSpan = 2000;
	
	// Legion
	int lgn_incoming_port = 1492;
	int lgn_outgoing_port = 8000;
	//BufferedWriter lout;
	
	long lastSend = 0;
	
	LegionLeader(Socket server) {
		super(server);
		inputTable = new HashMap<Integer,String>();
		players = new LinkedList<Integer>();
		
		// Receive results
		try {
			// Get feedback from legion
			ServerSocket llistener = new ServerSocket(lgn_incoming_port);
			
			
			// [[ NOW RUN LEGION! ]]
			ExecThread ext = new ExecThread("java -jar LegionGaming.jar leader 1 1000 .01");
			Thread t = new Thread(ext);
			t.start();
			// [[ CONTINUE WITH MIDDLEMAN ]]
			
			System.out.println("Listening for Legion...");
			Socket lserver = llistener.accept();
			
			
			System.out.print("Starting Legion server thread...");
			// Start the forwarding thread
			LegionSocketServer conn_c = new LegionSocketServer(lserver, this);
			Thread f = new Thread(conn_c);
			f.start();
		}
		catch (IOException ioe) {
			System.err.println(ioe.getLocalizedMessage());
		}
		
		
		System.out.print("Finished constructing legion leader.");
		///////////////////////////////////////////////////////
	}
	
	public void init(int ident) {
		if( players.size() == 0 ) {
			leader = ident;
		}
		players.add(ident);
	}
	
	public byte[] input(byte[] b) {
		//System.out.print("Got input! :: ");
				
		String curInput = parseByteToString(b);
		String[] tempDeci = curInput.split(" ");
		//System.out.println("raw curInput return :: " + curInput + " (" + curInput.trim().length() + ")");
		//System.out.println("raw uID string (len-1):: " + tempDeci[tempDeci.length-1]);
		int uID = Integer.parseInt(tempDeci[tempDeci.length-1]);
		curInput = curInput.substring(0,curInput.lastIndexOf(' '));
		
		inputTable.put(uID, curInput);
		if( !players.contains(uID) ) {
			players.add(uID);
		}
		
		// Send the new input to Legion server
		long curTime = System.currentTimeMillis();
		//if( curTime - lastSend > 200 ) {
			sendLegion(uID+":0:"+curInput+":"+curTime);
			lastSend = curTime;
			//System.out.println("Sent to Legion: " + uID+":0:"+curInput+":"+System.currentTimeMillis());
		//}
		
		
		
		// --------------------------
		// Send update to interface
		sendHeartbeat();
		
		return null;
	}
	
	private void sendHeartbeat() {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					//System.out.println("Preparing to write....");
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
			  		//System.out.println("Socket open...");
			  		PrintWriter leaderReport = new PrintWriter(gameclient.getOutputStream());
			  		//System.out.println("Writing... " + leaderMaxTimeSpan + " - (" + curTime + " - "  + leaderStartTime + ") = " + (leaderMaxTimeSpan - (curTime - leaderStartTime)) );
			  		leaderReport.write((leader >= 0 ? leader : "NOLEADER") + ";" + timeLeft + ";0!");
			  		//System.out.println("Done...");
			  		leaderReport.flush();
			  		leaderReport.close();
			  		
		            // Create log file
					//KeyEventDescription.rec = new BufferedWriter(new FileWriter(dateFormat.format(date) + "_Recording.log"));
			      } catch( IOException ioe ) {
			  		System.err.println("Failed to connect XX: " + ioe.toString());
			  		System.exit(0);
			      }
	}
	
	public void sendLegion(String toSend) {
		// GAMING: Send back: {leader.worker, leaderTimeSpan - (curTime - leaderStartTime)}
				try {
					//WARNING: THIS OPENS A NEW CONNECTION EVERYTIME, DO WE USE THIS?
					//System.out.println("Preparing to send to Legion...");
			    	// Connect to legion on port 8000
					String lgn_addr = "localhost";
			  		Socket lgnclient = new Socket(lgn_addr, lgn_outgoing_port);
			  		//System.out.println("Socket open...");
			  		PrintWriter lgnLog = new PrintWriter(lgnclient.getOutputStream());
			  		lgnLog.write(toSend);
			  		//System.out.println("Sent to Legion.");
			  		lgnLog.flush();
			  		lgnLog.close();
			  		
		            // Create log file
			      } catch( IOException ioe ) {
			  		System.err.println("Failed to connect to LEGION: " + ioe.toString());
			  		System.exit(0);
			      }
	}
	
	public void output(String toSend) {
		// Send selected input to VMulti
		System.out.println("Sending to VMulti...");
		sendData(parseStringToBytes(toSend));
	}
}
