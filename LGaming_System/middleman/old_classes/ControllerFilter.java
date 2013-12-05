import java.util.ArrayList;


public class ControllerFilter extends NoiseMask<Byte> {

	@Override
	public ArrayList<Byte> init() {
		// 
		ArrayList<Byte> newTemplate = new ArrayList<Byte>();
		
		// Controller scheme -- note: all 0's means 'require perfect match'
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		newTemplate.add(0, (byte)0);
		
		
		return newTemplate;
	}

	@Override
	public Byte filter(Byte first, Byte second, int idx) {
		// 
		if( Math.abs(first - second) < masterTemplate.get(idx) ) {
			return first;
		}
		
		return second;
	}

	@Override
	public int compare(Byte first, Byte second, int idx) {
		// Is the first element smaller, larger, or about the same?
		if( first < second - masterTemplate.get(idx) ) {
			return -1;
		}
		else if( second < first - masterTemplate.get(idx) ) {
			return 1;
		}
		
		// Otherwise, they're equal
		return 0;
	}
 
}
