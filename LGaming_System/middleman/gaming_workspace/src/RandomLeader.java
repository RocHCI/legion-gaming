import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;
import java.util.Random;


public class RandomLeader extends SingleLeaderMediator {

	RandomLeader(Socket server, String maxLeaderTime) {
		super(server, maxLeaderTime);
	}
	
	public int findNextLeader() {
		System.out.println("Lotto!");
		// Select randomly from the best set
		System.out.println("Old leader: " + leader + " --> new candidates: " + activePlayers.size());
		//int oldLeader = leader;
		int ans = leader;
		if(activePlayers.size() > 1) {
			while( ans == leader ) {
				//leader = players.get(players.get(rand.nextInt(players.size())));
				ans = activePlayers.get(randGen.nextInt(activePlayers.size()));
			}
			return ans;
		} else {
			return activePlayers.getFirst();
		}
	}
}