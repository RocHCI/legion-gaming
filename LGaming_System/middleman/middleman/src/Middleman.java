import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;


public class Middleman {
	/**  SETTINGS  **/
	static final String hostname = "localhost";
	static final int incoming_port = 7698;
	static final int outgoing_port = 27015;

	static short uniqID = 0;
	
	
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		System.out.println("Initializing...!");
	
		try {
			Socket outConnect = new Socket(hostname, outgoing_port);
			
			// Create the input mediator of the desired type
			InputMediator im = null;
			if( args.length > 0 ) {
				if( args[0].equals("local") ) {
					im = new LocalLeader(outConnect);
				}
				else if( args[0].equals("legion") ) {
					im = new LegionLeader(outConnect);
				}
				else if( args[0].equals("mob") ) {
					im = new MobLeader(outConnect);
				}
				else if( args[0].equals("pass") ) {
					im = new PassLeader(outConnect);
				}
				else if( args[0].equals("random") ) {
					im = new RandomLeader(outConnect);
				}
				else if( args[0].equals("randomlong") ) {
					im = new RandomLongLeader(outConnect);
				}
				else if( args[0].equals("seq") ) {
					im = new SeqLeader(outConnect);
				}
				else if( args[0].equals("noop") ) {
					im = new NoopLeader(outConnect);
				}
				else if( args[0].equals("multi") || args[0].equals("multiball") || args[0].equals("MULTIBALL!!!") ) {
					im = new MultiMediator(outConnect, new DivyMask());
				}
				else {
					System.out.println("Usage: \"java -jar middleman.jar {local,legion,mob,pass,seq,random,seq,noop}\"");
					return;
				}
			}
			else {
				System.out.println("Usage: \"java -jar middleman.jar {local,legion,mob,pass,random,seq,noop}\"");
				return;
			}
		
			System.out.println("Creating listener...");
			// Receive connections / start server
			ServerSocket listener = new ServerSocket(incoming_port);
			Socket server;
			
			System.out.println("Ready...");
			while( true ) {
				server = listener.accept();
				System.out.println("Middle recieving connection - from: " + server.getLocalAddress().toString());
				
				SocketServer conn_c = new SocketServer(server, im, uniqID++);
		        Thread t = new Thread(conn_c);
		        t.start();
			}
		} catch( IOException ioe ) {
			System.err.println("ERROR in incoming socket connection! --> " + ioe.getLocalizedMessage());
		}
	}

}
