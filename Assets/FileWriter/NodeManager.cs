using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class NodeManager : MonoBehaviour
{

	public List<TextNode> nodes = new List<TextNode>();
	[HideInInspector]
	public List<DestinationObject> destinations = new List<DestinationObject>();
	[HideInInspector]
	public List<EventObject> events = new List<EventObject>();
	[HideInInspector]
	public List<MemoryObject> memories = new List<MemoryObject>();
	public GameObject nodePrefab;

	// Temp and hidden
	int selected = -1;
	Bounds temp;

	int dummy = 0;

	bool CheckInsideNode (Vector2 pos, ref int id, int ignore=-1, int scale=1) {
		id = -1;
		for (int i = 0; i < nodes.Count; ++i) {
			if (i == ignore) continue;
			temp = new Bounds(nodes[i].bounds.center, nodes[i].bounds.size * scale);
			if (temp.Contains(pos)) {
				id = i;
				return true;
			}
		}
		return false;
	}

	public bool CreateNode (Vector2 pos) {
		if (CheckInsideNode(pos, ref dummy, -1, 2)) return false;
		TextNode addedObj = Instantiate(nodePrefab, (Vector3)pos, 
							Quaternion.identity, transform).transform.GetComponent<TextNode>();
		nodes.Add(addedObj);
		addedObj.Setup((Vector3)pos);
		return true;
	}

	public bool DeleteNode (Vector2 pos) {
		int id = -1;
		if (!CheckInsideNode(pos, ref id)) return false;
		// Remove all of the destinations that lead to this node.
		for (int i = 0; i < nodes.Count; ++i) {
			for (int j = 0; j < nodes[i].node.destinations.Count; ++j) {
				if (nodes[i].node.destinations[j].dest == id) {
					nodes[i].node.destinations.RemoveAt(j);
					j--;
				}
			}
		}
		// Decrement any node ids that are higher than the node removed.
		for (int i = 0; i < nodes.Count; ++i) {
			if (nodes[i].node.id > id) {
				nodes[i].node.id--;
			}
		}
		Destroy(nodes[id].gameObject);
		nodes.RemoveAt(id);
		TextNode.id--;
		return true;
	}

	public int AddDestination (Vector2 pos) {
		int id = -1;
		CheckInsideNode(pos, ref id);
		return id;
	}

	public int DeleteDestination (Vector2 pos) {
		int index = -1;
		string checkType;
		for (int i = 0; i < destinations.Count; ++i) {
			if (index != -1) {
				destinations[i].currentDest.id--;
				continue;
			}
			if (nodes[destinations[i].currentDest.dest].bounds.Contains(pos)) {
				index = i;
				int destID = destinations[i].currentDest.id;
				destinations.RemoveAt(index);
				checkType = destinations[i].currentDest.GetTemplatedType();
				if (checkType == "NONE") {
					for (int j = 0; j < nodes[selected].node.destinations.Count; ++j) {
						if (destID == nodes[selected].node.destinations[j].id) {
							nodes[selected].node.destinations.RemoveAt(j);
						}
					}
				} else if (checkType == "Int32") {
					for (int j = 0; j < nodes[selected].node.intDestinations.Count; ++j) {
						if (destID == nodes[selected].node.intDestinations[j].id) {
							nodes[selected].node.intDestinations.RemoveAt(j);
						}
					}
				} else if (checkType == "String") {
					for (int j = 0; j < nodes[selected].node.stringDestinations.Count; ++j) {
						if (destID == nodes[selected].node.stringDestinations[j].id) {
							nodes[selected].node.stringDestinations.RemoveAt(j);
						}
					}
				} else if (checkType == "Boolean") {
					for (int j = 0; j < nodes[selected].node.boolDestinations.Count; ++j) {
						if (destID == nodes[selected].node.boolDestinations[j].id) {
							nodes[selected].node.boolDestinations.RemoveAt(j);
						}
					}
				}
				break;
			}
		}
		return index;
	}

	public int SelectNode (Vector2 pos) {
		int id = -1;
		CheckInsideNode(pos, ref id);
		selected = id;
		return id;
	}

	public int LiftNode (Vector2 pos) {
		int id = -1;
		if (CheckInsideNode(pos, ref id)) {
			selected = id;
			nodes[id].Lift();
		}
		return id;
	}

	public bool PlaceNode (Vector2 pos) {
		if (!CheckInsideNode(pos, ref dummy, selected, 2)) {
			nodes[selected].Place();
			selected = -1;
			return true;
		}
		return false;
	}

	public void UpdateAll (int node) {
		// Destinations
		if (node == -1) return;
		nodes[node].node.destinations = new List<Destination>();
		nodes[node].node.intDestinations = new List< MemoryDestination<int> >();
		nodes[node].node.stringDestinations = new List< MemoryDestination<string> >();
		nodes[node].node.boolDestinations = new List< MemoryDestination<bool> >();
		string typeCheck;
		for (int i = 0; i < destinations.Count; ++i) {
			destinations[i].UpdateMemory();
			typeCheck = destinations[i].currentDest.GetTemplatedType();
			if (typeCheck == "NONE") {
				nodes[node].node.destinations.Add(destinations[i].currentDest);
			} else if (typeCheck == "Int32") {
				nodes[node].node.intDestinations.Add((MemoryDestination<int>)destinations[i].currentDest);
			} else if (typeCheck == "String") {
				nodes[node].node.stringDestinations.Add((MemoryDestination<string>)destinations[i].currentDest); 
			} else if (typeCheck == "Boolean") {
				nodes[node].node.boolDestinations.Add((MemoryDestination<bool>)destinations[i].currentDest);
			}
		}
		nodes[node].node.destTotal = destinations.Count;
		// Events
		nodes[node].node.events = new List<DialogueEvent>();
		nodes[node].node.intEvents = new List< DialogueEventTemplated<int> >();
		nodes[node].node.stringEvents = new List< DialogueEventTemplated<string> >();
		nodes[node].node.boolEvents = new List< DialogueEventTemplated<bool> >();
		for (int i = 0; i < events.Count; ++i) {
			events[i].UpdateEvent();
			typeCheck = events[i].currentEvent.GetTemplatedType();
			if (typeCheck == "NONE") {
				nodes[node].node.events.Add(events[i].currentEvent);
			} else if (typeCheck == "Int32") {
				nodes[node].node.intEvents.Add((DialogueEventTemplated<int>)events[i].currentEvent);
			} else if (typeCheck == "String") {
				nodes[node].node.stringEvents.Add((DialogueEventTemplated<string>)events[i].currentEvent); 
			} else if (typeCheck == "Boolean") {
				nodes[node].node.boolEvents.Add((DialogueEventTemplated<bool>)events[i].currentEvent);
			}
		}
		nodes[node].node.evTotal = events.Count;
		// Memories
		nodes[node].node.intMemories = new List< Memory<int> >();
		nodes[node].node.stringMemories = new List< Memory<string> >();
		nodes[node].node.boolMemories = new List< Memory<bool> >();
		for (int i = 0; i < memories.Count; ++i) {
			memories[i].UpdateMemory();
			typeCheck = memories[i].currentMemory.GetTemplatedType();
			if (typeCheck == "Int32") {
				nodes[node].node.intMemories.Add((Memory<int>)memories[i].currentMemory);
			} else if (typeCheck == "String") {
				nodes[node].node.stringMemories.Add((Memory<string>)memories[i].currentMemory); 
			} else if (typeCheck == "Boolean") {
				nodes[node].node.boolMemories.Add((Memory<bool>)memories[i].currentMemory);
			}
		}
		nodes[node].node.memTotal = memories.Count;
	}

}
