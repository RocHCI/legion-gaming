import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;
import java.lang.InterruptedException;


public class SeqLeader extends SingleLeaderMediator {

	SeqLeader(Socket server, String maxLeaderTime) {
		super(server, maxLeaderTime);
	}
	
	public int findNextLeader() {
		int ans = (leader + 1) % players.size();
		while(!activePlayers.contains(ans)) {
			ans = (ans + 1) % players.size();
		}
		return ans;
	}
}