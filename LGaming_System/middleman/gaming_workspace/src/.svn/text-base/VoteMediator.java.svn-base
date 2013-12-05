/*
   Legion: Crowd Control of Existing Interfaces. This software enables multiple
   			users to control a single interface by combining their inputs in real-time.
    Copyright (C) 2011 Walter S. Lasecki, Kyle I. Murray, Samuel White, Jeffrey P. Bigham

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */



import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Random;
import java.util.Hashtable;
import java.util.Enumeration;


public class VoteMediator extends ParallelMediator{	
	
	public VoteMediator(Socket server) {
		super(server);
		
		for(int i = 0; i < attribution.length ; i++) {
			activeInputs.add(new ArrayList<String>());
		}
	}
	
	public Hashtable<String,Integer> sumHash(int i) {
		Hashtable<String,Integer> inputHash = new Hashtable<String,Integer>();
		for(String bitSet : activeInputs.get(i)) {
			if(inputHash.containsKey(bitSet)) {
				System.out.println("Contains: " + bitSet);
				inputHash.put(bitSet, inputHash.get(bitSet) + 1);
			} else {
				inputHash.put(bitSet,1);
			}
		}
		return inputHash;
	}
	
	public ArrayList<String> getMaxInputs(int i,Hashtable<String,Integer> inputHash) {
		int maxInput = 0;
		ArrayList<String> curAns = new ArrayList<String>();
		Enumeration<String> e = inputHash.keys();
		System.out.println("---------------------------------+++++ HASH +++++");
		while(e.hasMoreElements()) {
			String key = (String) e.nextElement();
			System.out.println(key + "    " + inputHash.get(key));
			if(inputHash.get(key) > maxInput) {
				curAns = new ArrayList<String>();
				maxInput = inputHash.get(key);
				curAns.add(key);
				System.out.println("curAns(" + curAns.size() + ") +: " + key);
			} else if(inputHash.get(key) == maxInput) {
				curAns.add(key);
				System.out.println("curAns NEW: " + maxInput + "    " + key);
			}
		}
		System.out.println("curAns SIZE: " + curAns.size());
		return curAns;
	}
	
	public void everyTurn(int curSet, int uID, String curInput, ControllerInput ci) {
		if( curSet < 4 ) {
			// If the new input is non-null...
			// Lump Ys with Xs, so put both in X position in structure
			if(curSet == 0 || curSet == 2) {
				// If the new input is non-null...
				if((ci.getAxisValue(segmentNames[curSet]) <= 128 - analogNoise || ci.getAxisValue(segmentNames[curSet]) >= 128 + analogNoise)
						|| (ci.getAxisValue(segmentNames[curSet+1]) <= 128 - analogNoise || ci.getAxisValue(segmentNames[curSet+1]) >= 128 + analogNoise)) {
					System.out.println("Adding : " + ci.getAxisValue(segmentNames[curSet]));
					activeInputs.get(curSet).add(Integer.toString(ci.getAxisValue(segmentNames[curSet])));
					activeInputs.get(curSet+1).add(Integer.toString(ci.getAxisValue(segmentNames[curSet+1])));
				}
			}
		} else {
			//System.out.println("CI attribute " + segmentNames[i] + " has value " + ci.getValue(segmentNames[i]))/;
			if(!ci.getValue(segmentNames[curSet]).equals("0000")) {
				System.out.println("Adding : " + ci.getValue(segmentNames[curSet]));
				activeInputs.get(curSet).add(ci.getValue(segmentNames[curSet]));
			}
		}
	}
	
	public AnalogPair pickAnalogInput(int curSet) {
		Hashtable<String, ArrayList<AnalogPair>> quadAnalogs = new Hashtable<String, ArrayList<AnalogPair>>();
		quadAnalogs.put("l", new ArrayList<AnalogPair>());
		quadAnalogs.put("r", new ArrayList<AnalogPair>());
		quadAnalogs.put("u", new ArrayList<AnalogPair>());
		quadAnalogs.put("d", new ArrayList<AnalogPair>());
		quadAnalogs.put("lu", new ArrayList<AnalogPair>());
		quadAnalogs.put("ld", new ArrayList<AnalogPair>());
		quadAnalogs.put("ru", new ArrayList<AnalogPair>());
		quadAnalogs.put("rd", new ArrayList<AnalogPair>());
		
		boolean left;
		boolean right;
		boolean up;
		boolean down;
		System.out.println(curSet + " size: " + activeInputs.get(curSet).size() + "    " + (curSet+1) + " size: " + activeInputs.get(curSet+1).size());
		for(int i = 0; i < activeInputs.get(curSet).size(); i++) {
			left = Integer.parseInt(activeInputs.get(curSet).get(i)) <= 128 - analogNoise;
			right = Integer.parseInt(activeInputs.get(curSet).get(i)) >= 128 + analogNoise;
			up = Integer.parseInt(activeInputs.get(curSet+1).get(i)) <= 128 - analogNoise;
			down = Integer.parseInt(activeInputs.get(curSet+1).get(i)) >= 128 + analogNoise;
			
			if(left) {
				if(up) {
					quadAnalogs.get("lu").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				} else if(down) {
					quadAnalogs.get("ld").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				} else {
					quadAnalogs.get("l").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				}
			} else if(right) {
				if(up) {
					quadAnalogs.get("ru").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				} else if(down) {
					quadAnalogs.get("rd").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				} else {
					quadAnalogs.get("r").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
				}
			} else if(up) {
				quadAnalogs.get("u").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
			} else if(down) {
				quadAnalogs.get("d").add(new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(i)), Integer.parseInt(activeInputs.get(curSet+1).get(i))));
			}
		}
		
		int maxSize = -1;
		ArrayList<ArrayList<AnalogPair>> maxQuads = new ArrayList<ArrayList<AnalogPair>>();
		Enumeration<String> e = quadAnalogs.keys();
		while(e.hasMoreElements()) {
			String curQuad = e.nextElement();
			if(quadAnalogs.get(curQuad).size() > maxSize) {
				maxQuads.clear();
				maxQuads.add(quadAnalogs.get(curQuad));
				maxSize = quadAnalogs.get(curQuad).size();
				System.out.println("NEW MAX SIZE: " + maxSize);
			} else if(quadAnalogs.get(curQuad).size() == maxSize) {
				maxQuads.add(quadAnalogs.get(curQuad));
				System.out.println("SAME MAX SIZE: " + maxSize);
			}
		}
		ArrayList<AnalogPair> quadSet = maxQuads.get(randGen.nextInt(maxQuads.size()));
		return quadSet.get(randGen.nextInt(quadSet.size()));
	}
	
	public String pickInput(int curSet) {
		//System.out.println("Multi Input " + segmentNames[i] + "(" + activeInputs.get(i).size()  + "): " + activeInputs.get(i).get(0));
		// If only one person in inputting
		if(activeInputs.get(curSet).size() == 1) {
			System.out.println("One Input: " + activeInputs.get(curSet).get(0));
			return activeInputs.get(curSet).get(0);
		// If more than one person is inputting
		} else {
			if(curSet < 4) {
				String nextVal = activeInputs.get(curSet).get(randGen.nextInt(activeInputs.get(curSet).size()));
				System.out.println("Analog: " + nextVal);
				return nextVal;
			} else {
				ArrayList<String> curAns = getMaxInputs(curSet, sumHash(curSet));
				
				if(curAns.size() == 1) {
					System.out.println("One: " + curAns.get(0));
					return curAns.get(0);
				} else {
					String nextVal = curAns.get(randGen.nextInt(curAns.size()));
					System.out.println("MoreThanOne: " + nextVal);
					return nextVal;
				}
			}
		}
	}
	
	public AnalogPair findNextAnalogConfig(int curSet) {
		// If no one gave input, just pass along null
		if(activeInputs.get(curSet).size() == 0) {
			return new AnalogPair(128,128);
		// If only one person contributed, pass along their input
		} else if(activeInputs.get(curSet).size() == 1) {
			return new AnalogPair (Integer.parseInt(activeInputs.get(curSet).get(0)), Integer.parseInt(activeInputs.get(curSet+1).get(0)));
		} else {
			return pickAnalogInput(curSet);
		}
	}
	
	public String findNextConfig(int curSet) {
		// If no one gave input, just pass along null
		if(activeInputs.get(curSet).size() == 0) {
			return "0000";
		// If only one person contributed, pass along their input
		} else if(activeInputs.get(curSet).size() == 1) {
			return activeInputs.get(curSet).get(0);
		// If more than one person contributed, randomly pick one to pass along
		} else {
			String tmp = pickInput(curSet);
			System.out.println("''''''''''''''''''NEXT INPUT: " + tmp);
			return tmp;
		}
	}
}