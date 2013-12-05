import java.io.BufferedReader;
import java.io.InputStreamReader;


public class ExecThread implements Runnable{
	String cmd;
	
	public ExecThread(String msg) {
		this.cmd = msg;
	}
	
	public void run() {
		try {
			System.out.print("Running \"" + cmd + "\"...");
			Runtime rt = Runtime.getRuntime() ;
			
			Process p = rt.exec(cmd) ;
			System.out.println("Process started.");
			
			BufferedReader bri = new BufferedReader(new InputStreamReader(p.getInputStream()));
			BufferedReader bre = new BufferedReader(new InputStreamReader(p.getErrorStream()));
			String line = "XXX";
			System.out.println("Reading output...");
			while ((line = bri.readLine()) != null) {
		        System.out.println(line);
		    }
			System.out.println("Finished reading output");
			System.out.println("Reading error...");
			while ((line = bre.readLine()) != null) {
		        System.out.println(line);
		    }
			System.out.println("Finished reading error");
			
			//p.destroy();
		} catch(Exception e) {
			System.out.println("Error running new thread --> " + e.getLocalizedMessage());
		}
		return;
	}
}
