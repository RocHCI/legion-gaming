import java.net.Socket;
import java.util.ArrayList;

/*	[__PS3 CONTROLLERS ONLY__]
 * 
 *  Same as mob, but doesn't count null actions (no change from default) as a vote for anything [thus, single players can control at will]
 *  
 *  Note that THIS REQUIRES A CALIBRATION PERIOD (first message sent) and will be buggy if a button is being pressed during calibration!
 *    -- As a result this is NOT A GENERAL PURPOSE MEDIATOR, but rather is customized for ps3 controllers only
 */

// PENDING: remove all data-send's and replace with a fill command to the master's data array (which then gets sent to client/interface)


public class SlaveActiveMobLeader extends InputMediator {
	ArrayList<Integer> players;
	
	int leader = -1;
	long leaderStartTime = 0;
	long leaderMaxTimeSpan = 50;

	MultiMediator owner;
	
	SlaveActiveMobLeader(Socket server, MultiMediator ownerID) {
		super(server);
		players = new ArrayList<Integer>();
		//
		owner = ownerID;
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
		
		
		
		
		// Check for new leader once time has expired for the current one
		long elapsedTime = System.currentTimeMillis() - leaderStartTime;
		System.out.println("Length of current leader's reign: " + elapsedTime + " / " + leaderMaxTimeSpan);
		if( elapsedTime > leaderMaxTimeSpan ) {
			System.out.println("Moving to the next leader!");
			
			leader = (leader + 1) % players.size();
			leaderStartTime = System.currentTimeMillis();
		}
		
		
		// If the leader is the one inputting, forward it
		if( uID == leader ) {
			System.out.println("Following user #" + uID);
			//sendData(parseStringToBytes(curInput));
			
			// --------------------------
			// Send update to parent
			return parseStringToBytes(curInput);
		}
		
		
		return null;
	}
	
	
}

