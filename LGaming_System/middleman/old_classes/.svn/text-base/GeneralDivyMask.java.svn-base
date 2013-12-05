import java.util.ArrayList;


// Divide the segment evenly over the number of workers n

public class GeneralDivyMask extends DataMask<byte[]>{
	// ArrayList<Integer> masterTemplate is an inherited var that must be initialized
	
	public ArrayList<byte[]> partition(byte[] input, int n) {
		masterTemplate = new ArrayList<Integer>();
		
		// Find the size of the bin that will be needed (round up)
		int binSize = (int)Math.ceil((double)input.length / (double)n);
		
		// Create a returnable structure for the different segments
		ArrayList<byte[]> retList = new ArrayList<byte[]>();
		// Initialize the list
		for( int i = 0; i < n; i++ ) {
			byte[] toAdd = new byte[]{};
			retList.add(toAdd);
		}
		
		System.out.println("N = " + n);
		System.out.println("List size = " + retList.size());
		
		// Divy up the bits into corresponding arrays
		for( int i = 0; i < input.length; i++ ) {
			int idx = i/binSize;
			System.out.println("idx = " + idx + " | " + "input length = " + input.length + " | " + "binSize = " + binSize);
			retList.get(idx)[retList.size()] = input[i];
			
			// Add the owner index to the list (in order, so implicitly using i)
			masterTemplate.add(idx);
		}
		
		return retList;
	}
}
