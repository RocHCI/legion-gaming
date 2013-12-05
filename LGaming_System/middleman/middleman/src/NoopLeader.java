import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;


public class NoopLeader extends InputMediator {
	
	int leader = -1;
	
	NoopLeader(Socket server) {
		super(server);
	}
	
	public void init(int ident) {
		//
	}
	
	public byte[] input(byte[] b) {
		String curInput = parseByteToString(b);
		String[] tempDeci = curInput.split(" ");
		System.out.println("raw curInput return :: " + curInput + " (" + curInput.trim().length() + ")");
		System.out.println("raw uID string (len-1):: " + tempDeci[tempDeci.length-1]);
		int uID = Integer.parseInt(tempDeci[tempDeci.length-1]);
		curInput = curInput.substring(0,curInput.lastIndexOf(' '));
		System.out.println("=====================================\n");
		System.out.println("FORWARDING ALL CONTROLLER #1 INPUT!");
		System.out.println("\n=====================================");
		
		if( leader < 0 ) {
			leader = uID;
		}
		
		// If the input is from the first connected controller, forward input
		if( uID == leader ) {
			sendData(parseStringToBytes(curInput));
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
					
			    	// Connect to client on port 7777
					String gameclient_addr = "localhost";
					int gameclient_port = 7777;
			  		Socket gameclient = new Socket(gameclient_addr, gameclient_port);
			  		System.out.println("Socket open...");
			  		PrintWriter leaderReport = new PrintWriter(gameclient.getOutputStream());
			  		System.out.println("Writing... ");
			  		leaderReport.write((leader >= 0 ? leader : "NOLEADER") + ";1;0!");
			  		
			  		System.out.println("Done...");
			  		leaderReport.flush();
			  		leaderReport.close();
			      } catch( IOException ioe ) {
			  		System.out.println("Failed to connect XX: " + ioe.toString());
			  		System.exit(0);
			      }
	}
}
