
public class ControllerInput {

	// Joystick position values
	AnalogPair joyL = new AnalogPair();
	AnalogPair joyR = new AnalogPair();
	
	// Set the max allowable noise in the joysticks (how much difference is allowed before counting the input ass different)
	int maxNoise = 40;

	
	// Button settings
	String triggers;
	String dpad;
	String buttons;
	String special;
	
	public ControllerInput(int lx, int ly, int rx, int ry, String rawButtons) {
		//
		System.out.println("-------- ci input: " + rawButtons);
		joyL.set(lx, ly);
		joyR.set(rx, ry);
		System.out.println("----- joy: "+ joyL.toString() + " " + joyR.toString());
		
		triggers = rawButtons.substring(0,4);
		dpad = rawButtons.substring(4,8);
		buttons = rawButtons.substring(8,12);
		special = rawButtons.substring(12,16);		// Only needs 3, but last one is assumed 0 to get even string lengths
	}
	
	public int setValue(AnalogPair joyPair, String axis) {
		switch(axis) {
		case "joyLx":
		case "joyLy":
			joyL = joyPair;
			return 0;
		case "joyRx":
		case "joyRy":
			joyR = joyPair;
			return 2;
		default:
			return -1;
		}
	}
	
	public int setValue(int value, String axis) {
		// Set the selected joystick position
		switch(axis) {
		case "joyLx":
			joyL.setX(value);
			return 0;
		case "joyLy":
			joyL.setY(value);
			return 1;
		case "joyRx":
			joyR.setX(value);
			return 2;
		case "joyRy":
			joyR.setY(value);
			return 3;
		default:
			return -1;
		}
		// Return the number of the segment updated (for attribution)
	}
	
	public int setValue(String value, String group) {
		// Set the selected button group
		switch(group) {
		case "triggers":
			triggers = value;
			return 4;
		case "dpad":
			dpad = value;
			return 5;
		case "buttons":
			buttons = value;
			return 6;
		case "special":
			special = value;
			return 7;
		default:
			return -1;
		}
		// TODO: Return the number of the segment updated (for attribution)
	}
	
	
	public int getAxisValue(String selector) {
		// Return the requested axis value
		switch(selector){
		case "joyLx":
			return joyL.getX();
		case "joyLy":
			return joyL.getY();
		case "joyRx":
			return joyR.getX();
		case "joyRy":
			return joyR.getY();
		default:
			return -1;
		}
	}
	
	public String getValue(String selector) {
		// Return the requested button value
		switch(selector) {
		case "triggers":
			return triggers;
		case "dpad":
			return dpad;
		case "buttons":
			return buttons;
		case "special":
			return special;
		default:
			return "";
		}
	}
	
	
	public boolean equals(ControllerInput other) {
		// Check that the joysticks are equal given a small amount of expected noise
		if( Math.abs(this.joyL.getX() - other.joyL.getX()) > maxNoise ) {
			return false;
		}
		if( Math.abs(this.joyL.getY() - other.joyL.getY()) > maxNoise ) {
			return false;
		}
		if( Math.abs(this.joyR.getX() - other.joyR.getX()) > maxNoise ) {
			return false;
		}
		if( Math.abs(this.joyR.getY() - other.joyR.getY()) > maxNoise ) {
			return false;
		}
		
		// Check button presses
		if( this.triggers != other.triggers) {
			return false;
		}
		if( this.dpad != other.dpad) {
			return false;
		}
		if( this.buttons != other.buttons) {
			return false;
		}
		if( this.special != other.special) {
			return false;
		}
		
		
		return true;
	}
	
	// Compare one specific segment
	public boolean equals(ControllerInput other, String segName) {
		// Check that the joysticks are equal given a small amount of expected noise
		switch(segName) {
		case "joyLx":
			if( Math.abs(this.joyL.getX() - other.joyL.getX()) > maxNoise ) {
				//System.out.println("----- Lx neq: "+this.joyLx+" "+other.joyLx);
				return false;
			}
			break;
		case "joyLy":
			if( Math.abs(this.joyL.getY() - other.joyL.getY()) > maxNoise ) {
				//System.out.println("----- Ly neq: "+this.joyLy+" "+other.joyLy);
				return false;
			}
			break;
		case "joyRx":
			if( Math.abs(this.joyR.getX() - other.joyR.getX()) > maxNoise ) {
				//System.out.println("----- Rx neq: "+this.joyRx+" "+other.joyRx);
				return false;
			}
			break;
		case "joyRy":
			if( Math.abs(this.joyR.getY() - other.joyR.getY()) > maxNoise ) {
				//System.out.println("----- Ry neq: "+this.joyRy+" "+other.joyRy);
				return false;
			}
			break;
		
		// Check button presses
		case "triggers":
			if( !this.triggers.equals(other.triggers)) {
				//System.out.println("----- trig: "+this.triggers+" "+other.triggers);
				return false;
			}
			break;
		case "dpad":
			if( !this.dpad.equals(other.dpad)) {
				//System.out.println("----- dpad neq: "+this.dpad+" "+other.dpad);
				return false;
			}
			break;
		case "buttons":
			if( !this.buttons.equals(other.buttons)) {
				//System.out.println("----- but neq: "+this.buttons+" "+other.buttons);
				return false;
			}
			break;
		case "special":
			if( !this.special.equals(other.special)) {
				//System.out.println("----- spc neq: "+this.special+" "+other.special);
				return false;
			}
			break;
		}
		//System.out.println("----- nullinput");
		
		return true;
	}
	
	public String toString() {
		String retStr = "";
		
		// TODO: Convert the CI to a bit string
		retStr += joyL.toString() + " ";
		retStr += joyR.toString() + " ";
		
		System.out.println("toString: " + triggers + " | " + dpad + " | " + buttons + " | " + special);
		
		retStr += triggers;
		retStr += dpad;
		retStr += buttons;
		retStr += special;
		retStr += " 0";
		
		return retStr;
	}
	
	public String toByteArrayString() {
		// --- UGLY 2: Change the grouping from the desired control groups to output format ---
		//System.out.println("++++++++++++++ Before outputBuffer (sharedInput): " + sharedInput);
		StringBuffer outputBuffer = new StringBuffer(this.toString());
		
		char R3 = outputBuffer.charAt(outputBuffer.length()-3);
		char L3 = outputBuffer.charAt(outputBuffer.length()-4);
		outputBuffer.deleteCharAt(outputBuffer.length()-3);
		outputBuffer.deleteCharAt(outputBuffer.length()-3);
		
		outputBuffer.insert(outputBuffer.length()-2-14+2, L3);
		outputBuffer.insert(outputBuffer.length()-2-15+5, R3);
		
		String outputString = outputBuffer.toString();
		//System.out.println("++++++++++++++ After outputBuffer (outputBuffer): " + outputBuffer);
		// ---- END UGLY 2 ----
		System.out.println("-=-=--=-==--=-==--=-=-=-=-==--= Sending " + outputString);
		return outputString;
	}
}
