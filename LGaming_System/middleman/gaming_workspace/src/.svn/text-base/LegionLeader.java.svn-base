import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.LinkedList;
import java.lang.InterruptedException;
import java.util.HashMap;
import java.util.Enumeration;


public class LegionLeader extends SixArmLeaderMediator {

	double alpha = 0.9;
	// Players X Sets X Configs X Num each config
	LinkedList<HashMap<Integer,HashMap<String,Double>>> configs = new LinkedList<HashMap<Integer,HashMap<String,Double>>>();
	// Players X Sets X Weights
	LinkedList<Double[]> weights;
	// Players who entered mid-round will be in the running for leader, but won't have their weight updated
	LinkedList<Integer> prevInactivePlayers = new LinkedList<Integer>();
	
	LegionLeader(Socket server, String maxLeaderTime) {
		super(server, maxLeaderTime);
		weights = new LinkedList<Double[]>();
	}
	
	public void init(int ident) {
		super.init(ident);
		weights.add(new Double[attribution.length]);
		for(int i = 0; i < weights.get(ident).length; i++) {
			weights.getLast()[i] = 0.5;
		}
		configs.add(new HashMap<Integer,HashMap<String,Double>>());
	}
	
	public void everyTurn(int uID, String curInput, ControllerInput ci) {
		super.everyTurn(uID, curInput, ci);
		
		// Ignore input if the player is inactive
		if(activePlayers.contains(uID)) {
			for( int i = 0; i < attribution.length; i++ ) {
				if( i < 4 ) {
					// Lump Ys with Xs, so put both in X position in structure
					if(i == 0 || i == 2) {
						// If the new input is non-null...
						if((ci.getAxisValue(segmentNames[i]) <= 128 - analogNoise || ci.getAxisValue(segmentNames[i]) >= 128 + analogNoise)
								|| (ci.getAxisValue(segmentNames[i+1]) <= 128 - analogNoise || ci.getAxisValue(segmentNames[i+1]) >= 128 + analogNoise)) {
							if(!configs.get(uID).containsKey(i)) {
								configs.get(uID).put(i, new HashMap<String,Double>());
							}
							
							// Analog gets split into sections
							String setKey;
							int setX = ci.getAxisValue(segmentNames[i]);
							int setY = ci.getAxisValue(segmentNames[i+1]);
							
							boolean left = ci.getAxisValue(segmentNames[i]) <= 128 - analogNoise;
							boolean right = ci.getAxisValue(segmentNames[i]) >= 128 + analogNoise;
							boolean up = ci.getAxisValue(segmentNames[i+1]) <= 128 - analogNoise;
							boolean down = ci.getAxisValue(segmentNames[i+1]) >= 128 + analogNoise;
							
							if(left) {
								if(up) {
									setKey = "lu";
								} else if(down) {
									setKey = "ld";
								} else {
									setKey = "l";
								}
							} else if(right) {
								if(up) {
									setKey = "ru";
								} else if(down) {
									setKey = "rd";
								} else {
									setKey = "r";
								}
							} else if(up) {
								setKey = "u";
							} else if(down) {
								setKey = "d";
							// Should never get here!
							} else {
								setKey = "null";
							}
							
							
							System.out.println("Adding : " + ci.getAxisValue(segmentNames[i]));
							HashMap<String,Double> setTbl = configs.get(uID).get(i);
							//String setKey = Integer.toString(ci.getAxisValue(segmentNames[i]));
							if(setTbl.containsKey(setKey)) {
								double newCnt = setTbl.get(setKey)+1;
								setTbl.put(setKey, newCnt);
							} else {
								setTbl.put(setKey, 1.0);
							}
							System.out.println("Adding : " + setKey + "    Size: " + configs.get(uID).size());
						}
					}
				} else {
					if(!ci.getValue(segmentNames[i]).equals("0000")) {
						if(!configs.get(uID).containsKey(i)) {
							configs.get(uID).put(i, new HashMap<String,Double>());
						}
						HashMap<String,Double> setTbl = configs.get(uID).get(i);
						String setKey = ci.getValue(segmentNames[i]);
						if(setTbl.containsKey(setKey)) {
							double newCnt = setTbl.get(setKey)+1;
							setTbl.put(setKey, newCnt);
						} else {
							setTbl.put(setKey, 1.0);
						}
						System.out.println("Adding : " + setKey + "    Size: " + configs.get(uID).size());
					}
				}
			}
		} else {
			if(!prevInactivePlayers.contains(uID)){
				prevInactivePlayers.add(uID);
			}
		}
	}
	
	
	/*** Helper Methods ***/
	
	
	private void printConfigList() {
		int n = 0;
		for(HashMap<Integer,HashMap<String,Double>> somePlayer : configs) {
			for(int someSet : somePlayer.keySet()) {
				System.out.println("\nPlayer " + n + " - " + someSet + ":");
				HashMap<String,Double> setConfigs = somePlayer.get(someSet);
				for(String config : setConfigs.keySet()) {
					System.out.println(config + "  " + setConfigs.get(config));
				}
			}
			n++;
		}
	}
	
	public void resetConfigs(int curSet) {
		for(HashMap<Integer,HashMap<String,Double>> player : configs) {
			if(player.containsKey(curSet)) {
				player.remove(curSet);
			}
		}
	}
	
	public void cleanUp(int curSet) {
		resetConfigs(curSet);
		prevInactivePlayers.clear();
	}
	
	
	/*** LEADER SELECTION METHODS ***/
	
	/*
	// For analog, just pick randomly
	public int pickAnalogLeader(int curSet, LinkedList<Integer> inputtingPlayers) {
		cleanUp(curSet);
		if(inputtingPlayers.size() == 0) {
			return leaders[curSet];
		} else {
			return inputtingPlayers.get(randGen.nextInt(inputtingPlayers.size()));
		}
	}
	*/
	
	// Just keep the same leader if no one is inputting
	public int pickNoWeightUpdateLeader(int curSet, LinkedList<Integer> inputtingPlayers) {
		if(inputtingPlayers.size() == 0) {
			int ans = -1;
			if(activePlayers.contains(leaders[curSet])){
				ans = leaders[curSet];
			} else {
				// NOTE: We assume that at least one player is not in pause mode
				ans = activePlayers.get(0);
			}
			cleanUp(curSet);
			return ans;
		// Pick the best active player to lead
		} else {
			double maxWeight = -1;
			LinkedList<Integer> potentialLeaders = new LinkedList<Integer>();
			for(int player : inputtingPlayers) {
				if(weights.get(player)[curSet] == maxWeight) {
					potentialLeaders.add(player);
				} else if(weights.get(player)[curSet] > maxWeight) {
					potentialLeaders.clear();
					potentialLeaders.add(player);
					maxWeight = weights.get(player)[curSet];
				}
			}
			cleanUp(curSet);
			return potentialLeaders.get(randGen.nextInt(potentialLeaders.size()));
		}
	}
	
	public int pickLeader(int curSet, LinkedList<Integer> inputtingPlayers) {
		// Calculate weight for this set
		HashMap<String,Double> totals = new HashMap<String,Double>();
		int numConfigInputs = 0;
		// For each player
		for(HashMap<Integer,HashMap<String,Double>> player : configs) {
			if(!player.isEmpty()) {
				if(player.containsKey(curSet)) {
					HashMap<String,Double> setConfigs = player.get(curSet);
					if(!setConfigs.isEmpty()) {
						// For each config in this set
						int numPlayerConfigInputs = 0;
						System.out.println("Player " + player + " configCount init:" + numPlayerConfigInputs);
						for(String config : setConfigs.keySet()) {
							// Update the totals for this set
							if(!totals.containsKey(config)) {
								totals.put(config, (double) setConfigs.get(config));
							} else {
								double curTotal = totals.get(config);
								totals.put(config, curTotal + (double) setConfigs.get(config));
							}
							numPlayerConfigInputs += setConfigs.get(config);
							System.out.println("Player " + player + " configCount update:" + numPlayerConfigInputs);
							numConfigInputs += setConfigs.get(config);
						}
						
						// Normalize player config counts
						System.out.println("Player " + player + "'s normalized configs:");
						for(String config : setConfigs.keySet()) {
							double curVal = setConfigs.get(config);
							System.out.println(curVal + " -> " + (curVal/(double)numPlayerConfigInputs));
							setConfigs.put(config, curVal/(double)numPlayerConfigInputs);
						}
					}
				}
			}
		}
		
		// Normalize the totals
		System.out.println("Totals:");
		if(!totals.isEmpty()) {
			for(String config : totals.keySet()) {
				double curTotal = totals.get(config);
				System.out.println(config + "    " + curTotal + " -> " + (((double)curTotal)/((double)numConfigInputs)) + "    ");
				totals.put(config, ((double)curTotal)/((double)numConfigInputs));
			}
		}
		
		// Update weights and pick leader
		LinkedList<Integer> potentialLeaders = new LinkedList<Integer>();
		double maxWeight = -1;
		// For each player
		for(int curPlayer = 0;  curPlayer < configs.size(); curPlayer++) {
			// Only pick and update weights from active players
			// Don't update the weights for players who became active this round
			if(inputtingPlayers.contains(curPlayer) && !prevInactivePlayers.contains(curPlayer)) {
				// Dot product, a.k.a. agreement level
				double dotProd = 0;
				System.out.println("dotProd orig: " + dotProd);
				if(!configs.get(curPlayer).isEmpty() && configs.get(curPlayer).containsKey(curSet)) {
					HashMap<String,Double> setConfigs = configs.get(curPlayer).get(curSet);
					if(!setConfigs.isEmpty()) {
						// For each config in this set
						for(String config : setConfigs.keySet()) {
							if(totals.containsKey(config)) {
								dotProd += setConfigs.get(config) * totals.get(config);
								System.out.println(dotProd + " = dotProd + " + setConfigs.get(config) + " * " + totals.get(config));
							}
						}
						
						// Update the weight
						double curWeight = weights.get(curPlayer)[curSet];
						System.out.println("NEW WEIGHT = " + curWeight + " * " + alpha + " + (1 - " + alpha  + ") * " + dotProd);
						weights.get(curPlayer)[curSet] = curWeight * alpha + (1 - alpha) * dotProd;
						System.out.println("PLAYER " + curPlayer + "'S WEIGHT FOR " + curSet + " IS " + weights.get(curPlayer)[curSet]);
					}
				}
				
				// Update the list of potential leaders
				for(int somePlayer = 0;  somePlayer < configs.size(); somePlayer++) {
					System.out.print("\n" + somePlayer + ": ");
					for(int someSet = 0; someSet< weights.get(somePlayer).length; someSet++) {
						System.out.print(weights.get(somePlayer)[someSet] + "    ");
					}
				}
				if(weights.get(curPlayer)[curSet] == maxWeight) {
					potentialLeaders.add(curPlayer);
				} else if(weights.get(curPlayer)[curSet] > maxWeight) {
					potentialLeaders.clear();
					potentialLeaders.add(curPlayer);
					maxWeight = weights.get(curPlayer)[curSet];
				}
			}
			
			// Reset
			if(!configs.get(curPlayer).isEmpty() && configs.get(curPlayer).containsKey(curSet) && !configs.get(curPlayer).get(curSet).isEmpty()) {
				configs.get(curPlayer).get(curSet).clear();
			}
		}
		
		// Finish up 
		cleanUp(curSet);
		
		// Print potential leaders
		System.out.println("Potential Leaders:");
		for(int potentialLeader : potentialLeaders) {
			System.out.println(potentialLeader);
		}
		
		System.out.println("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
		
		if(potentialLeaders.size() == 1) {
			System.out.println("NEW LEADER: " + curSet + "    " + potentialLeaders.get(0));
			return players.get(potentialLeaders.get(0));
		} else if(potentialLeaders.size() > 1) {
			int newLeader = potentialLeaders.get(randGen.nextInt(potentialLeaders.size()));
			System.out.println("NEW LEADER: "  + curSet + "    " + newLeader);
			return players.get(newLeader);
		} else {
			// Should never get here!
			System.out.println("NO NEW LEADER " + curSet);
			return -1;
		}
	}
	
	public int findNextLeader(int curSet) {
		System.out.println("================CURSET: " + curSet);		
		
		// Print out config list
		printConfigList();

		// Who has provided input this round?
		LinkedList<Integer> inputtingPlayers = new LinkedList<Integer>();
		for(int i = 0; i < configs.size(); i++) {
			if(configs.get(i).containsKey(curSet) && activePlayers.contains(i)) {
				System.out.println("Player " + i + " is active");
				inputtingPlayers.add(i);
			} else {
				System.out.println("Player " + i + " is INactive");
			}
		}

		/*
		// If it's analog input
		if(curSet < 4) {
			return pickAnalogLeader(curSet, inputtingPlayers);
		}
		*/
		
		// If there aren't enough active players to recalculate weights
		if(inputtingPlayers.size() < 3) {
			return pickNoWeightUpdateLeader(curSet, inputtingPlayers);
		// The normal case where we have to update weights and pick a new leader
		} else {
			return pickLeader(curSet, inputtingPlayers);
		}
	}
}