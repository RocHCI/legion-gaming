import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;
import java.util.Random;
import java.lang.InterruptedException;


public abstract class LeaderMediator extends InputMediator {
	long leaderTimeSpan = 10000;

	LeaderMediator(Socket server, String maxLeaderTime) {
		super(server);

		if(maxLeaderTime.startsWith("-")) {
			if(maxLeaderTime.endsWith("s")) {
				leaderTimeSpan = 10000;
			} else if(maxLeaderTime.endsWith("l")) {
				leaderTimeSpan = 30000;
			} else {
				leaderTimeSpan = 10000;
			}
		} else {
			try{
				leaderTimeSpan = Long.parseLong(maxLeaderTime);
			} catch(NumberFormatException ex) {
				leaderTimeSpan = 10000;
			}
		}
	}
	
	public void init(int ident) {
		players.add(ident);
		activePlayers.add(ident);
	}
	
	public void everyTurn(int uID, String curInput, ControllerInput ci) {
		handlePM(uID, curInput);
	}
	
}

