import java.util.ArrayList;


public abstract class NoiseMask<T> {
	ArrayList<T> masterTemplate = null;
	
	public NoiseMask() {
		masterTemplate = init();
	}
	
	public abstract ArrayList<T> init();
	
	public abstract T filter(T first, T second, int idx);
	
	public abstract int compare(T first, T second, int idx);
}
