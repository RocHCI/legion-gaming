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
import java.util.Random;


public class MultiMediator extends ParallelMediator{
	
	ArrayList<ArrayList<Integer>> inputtingPlayers = new ArrayList<ArrayList<Integer>>();
	
	// To make sure we catch when someone let's go of a button
	int[] prevLeaders;
	String[] prevInputs;
	
	public MultiMediator(Socket server) {
		super(server);
		prevLeaders = new int[attribution.length];
		prevInputs = new String[attribution.length];
		
		for(int i = 0; i < attribution.length ; i++) {
			activeInputs.add(new ArrayList<String>());
			inputtingPlayers.add(new ArrayList<Integer>());
		}
	}
	
	public void everyTurn(int curSet, int uID, String curInput, ControllerInput ci) {
		if(activePlayers.contains(uID)) {
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
						inputtingPlayers.get(curSet).add(uID);
					}
				}
			} else {
				if(!ci.getValue(segmentNames[curSet]).equals("0000")) {
					System.out.println("Adding : " + ci.getValue(segmentNames[curSet]));
					activeInputs.get(curSet).add(ci.getValue(segmentNames[curSet]));
					inputtingPlayers.get(curSet).add(uID);
				}
			}
		}
		handlePM(uID, curInput);
	}
	
	/*** Make sure we catch button up, or the system thinks someone is just always holding down the button ***/
	public String buttonUpFilter(int curSet, int ansIndex) {
		int prevLeader = prevLeaders[curSet];
		String prevLeaderPrevInput = prevInputs[curSet];
		int curLeader = inputtingPlayers.get(curSet).get(ansIndex);
		
		// ArrayList.contains() for some reason always returns false, so we have to do things a little weird
		String prevLeaderCurInput = "0000";
		System.out.println("ACTIVE INPUTS: ");
		for(int i = 0; i < activeInputs.get(curSet).size(); i++) {
			System.out.println(inputtingPlayers.get(curSet).get(i) + "-" + activeInputs.get(curSet).get(i));
			if(inputtingPlayers.get(curSet).get(i) == prevLeader) {
				prevLeaderCurInput = activeInputs.get(curSet).get(i);
				break;
			}
		}
		
		String curLeaderCurInput = activeInputs.get(curSet).get(ansIndex);
		
		System.out.println("PL: " + prevLeader + "    PLPI: " + prevLeaderPrevInput + "    PLCI: " + prevLeaderCurInput + "    CL: " + curLeader + "    CLCI: " + curLeaderCurInput);

		if(prevLeader != curLeader) {
			if(prevLeaderCurInput.equals(curLeaderCurInput)) {
				System.out.println("SAME INPUT(" + prevLeader + "," + curLeader + "): " + curLeaderCurInput);
				prevInputs[curSet] = prevLeaderCurInput;
				prevLeaders[curSet] = prevLeader;
			} else if(prevLeaderCurInput.equals("0000") && curLeaderCurInput.equals(prevLeaderPrevInput)) {
				System.out.println("WAS NULL: " + prevLeader + "-" + prevLeaderCurInput + " vs " + curLeader + "-" + curLeaderCurInput + "(PrevLeaderPrevInput: " + prevLeaderPrevInput + ")");
				prevInputs[curSet] = curLeaderCurInput;
				prevLeaders[curSet] = curLeader;
				return "0000";
			} else {
				System.out.println("DIFF INPUT: " + prevLeader + "-" + prevLeaderCurInput + " vs " + curLeader + "-" + curLeaderCurInput + "(PrevLeaderPrevInput: " + prevLeaderPrevInput + ")");
				prevInputs[curSet] = curLeaderCurInput;
				prevLeaders[curSet] = curLeader;
			}
		} else {
			System.out.println("SAME LEADER: " + prevLeader + "  " + curLeader);
			prevInputs[curSet] = curLeaderCurInput;
			prevLeaders[curSet] = curLeader;
		}
		
		System.out.println("AFTER: PL: " + prevLeader + "    PLPI: " + prevLeaderPrevInput + "    PLCI: " + prevLeaderCurInput + "    CL: " + curLeader + "    CLCI: " + curLeaderCurInput);
		
		// Since we just updated this value...
		return prevInputs[curSet];
	}
	
	public AnalogPair findNextAnalogConfig(int curSet) {
		// If no one gave input, just pass along null
		if(activeInputs.get(curSet).size() == 0) {
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return new AnalogPair(128,128);
		// If only one person contributed, pass along their input
		} else if(activeInputs.get(curSet).size() == 1) {
			int ans = Integer.parseInt(activeInputs.get(curSet).get(0));
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return new AnalogPair (Integer.parseInt(activeInputs.get(curSet).get(0)), Integer.parseInt(activeInputs.get(curSet+1).get(0)));
		} else {
			int ansIndex = randGen.nextInt(activeInputs.get(curSet).size());
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return new AnalogPair(Integer.parseInt(activeInputs.get(curSet).get(ansIndex)), Integer.parseInt(activeInputs.get(curSet+1).get(ansIndex)));
		}
	}
	
	public String findNextConfig(int curSet) {
		// If no one gave input, just pass along null
		if(activeInputs.get(curSet).size() == 0) {
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return "0000";
		// If only one person contributed, pass along their input
		} else if(activeInputs.get(curSet).size() == 1) {
			String ans = buttonUpFilter(curSet, 0);
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return ans;
		// If more than one person contributed, randomly pick one to pass along
		} else {
			String ans = buttonUpFilter(curSet, randGen.nextInt(activeInputs.get(curSet).size()));
			inputtingPlayers.set(curSet, new ArrayList<Integer>());
			return ans;
		}
	}
}