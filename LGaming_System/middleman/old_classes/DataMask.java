import java.util.ArrayList;


public abstract class DataMask<T> {
	// Developers need to implement and load this masterTemplate in partition()
	ArrayList<Integer> masterTemplate = null;
	
	public abstract ArrayList<T> partition(T input, int n);
	
	public ArrayList<Boolean> getTemplate(int index) {
		// Return a template containing marked cells for only the player indicated 
		ArrayList<Boolean> retTemplate = new ArrayList<Boolean>();
		
		for( int i = 0; i < masterTemplate.size(); i++ ) {
			if( masterTemplate.get(i) == index ) {
				retTemplate.add(true);
			}
			else {
				retTemplate.add(false);
			}
		}
		
		return retTemplate;
	}
}
