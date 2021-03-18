using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class NodeManager : MonoBehaviour
{

	public UIManager ui;

	public List<TextNode> nodes = new List<TextNode>();
	public Dictionary<string, LineRenderer> connections = new Dictionary<string, LineRenderer>();
	// Lists of LineRenderers being changed while a node is moved.
	public List<LineRenderer> activeFromConnections = new List<LineRenderer>();
	public List<LineRenderer> activeToConnections = new List<LineRenderer>();
	[HideInInspector]
	public List<DestinationObject> destinations = new List<DestinationObject>();
	[HideInInspector]
	public List<EventObject> events = new List<EventObject>();
	[HideInInspector]
	public List<MemoryObject> memories = new List<MemoryObject>();
	public GameObject nodePrefab;
	public GameObject connectionPrefab;

	// Temp and hidden
	public int selected = -1;
	Bounds temp;

	int dummy = 0;

	// Sort of singleton. On Awake, instance is set, regardless of whether it's set already.
	public static NodeManager instance;
	void Awake () {
		instance = this;
	}

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

	public int DeleteNode (Vector2 pos) {
		int id = -1;
		if (!CheckInsideNode(pos, ref id)) return -1;
		// Remove all of the destinations that lead to this node.
		for (int i = 0; i < nodes.Count; ++i) {
			for (int j = 0; j < nodes[i].node.destinations.Count; ++j) {
				if (nodes[i].node.destinations[j].dest == id) {
					nodes[i].node.destinations.RemoveAt(j);
					RemoveConnection(nodes[i].node.id, id);
					j--;
				}
			}
			for (int j = 0; j < nodes[i].node.intDestinations.Count; ++j) {
				if (nodes[i].node.intDestinations[j].dest == id) {
					nodes[i].node.intDestinations.RemoveAt(j);
					RemoveConnection(nodes[i].node.id, id);
					j--;
				}
			}
			for (int j = 0; j < nodes[i].node.stringDestinations.Count; ++j) {
				if (nodes[i].node.stringDestinations[j].dest == id) {
					nodes[i].node.stringDestinations.RemoveAt(j);
					RemoveConnection(nodes[i].node.id, id);
					j--;
				}
			}
			for (int j = 0; j < nodes[i].node.boolDestinations.Count; ++j) {
				if (nodes[i].node.boolDestinations[j].dest == id) {
					nodes[i].node.boolDestinations.RemoveAt(j);
					RemoveConnection(nodes[i].node.id, id);
					j--;
				}
			}
			bool found = false;
			// Also check through temporary destinations:
			for (int j = 0; j < destinations.Count; ++j) {
				if (found) {
					destinations[j].currentDest.id--;
				} else if (destinations[j].currentDest.dest == id) {
					found = true;
					ui.RemoveDestinationObject(destinations[j].currentDest.id);
					RemoveConnection(destinations[j].currentDest.dest, id);
				}
			}
		}
		// Decrement any node ids that are higher than the node removed.
		for (int i = 0; i < nodes.Count; ++i) {
			if (nodes[i].node.id > id) {
				nodes[i].node.id--;
			}
		}
		// Finally, remove any connections it has with other nodes.
		for (int i = 0; i < nodes[id].node.destinations.Count; ++i) {
			RemoveConnection(id, nodes[id].node.destinations[i].dest);
		}
		for (int i = 0; i < nodes[id].node.intDestinations.Count; ++i) {
			RemoveConnection(id, nodes[id].node.intDestinations[i].dest);
		}
		for (int i = 0; i < nodes[id].node.stringDestinations.Count; ++i) {
			RemoveConnection(id, nodes[id].node.stringDestinations[i].dest);
		}
		for (int i = 0; i < nodes[id].node.boolDestinations.Count; ++i) {
			RemoveConnection(id, nodes[id].node.boolDestinations[i].dest);
		}
		Destroy(nodes[id].gameObject);
		nodes.RemoveAt(id);
		TextNode.id--;
		if (id == selected) selected = -1;
		return id;
	}

	public int AddDestination (Vector2 pos) {
		int id = -1;
		CheckInsideNode(pos, ref id);
		return id;
	}

	public void AddDestinationToManager (DestinationObject dest) {
		// Add destination.
		dest.currentDest.id = destinations.Count;
		destinations.Add(dest);
		// Add connection.
		string key = selected.ToString() + 'x' + dest.currentDest.dest.ToString();
		if (connections.ContainsKey(key)) {
			return;
		}
		LineRenderer newConnection = Instantiate(connectionPrefab, transform).transform.GetComponent<LineRenderer>();
		newConnection.SetPosition(0, nodes[selected].node.position);
		newConnection.SetPosition(1, nodes[dest.currentDest.dest].node.position);
		connections[key] = newConnection;
	}

	public void AddConnection (int from, int to) {
		string key = from.ToString() + 'x' + to.ToString();
		LineRenderer newConnection = Instantiate(connectionPrefab, transform).transform.GetComponent<LineRenderer>();
		newConnection.SetPosition(0, nodes[from].node.position);
		newConnection.SetPosition(1, nodes[to].node.position);
		connections[key] = newConnection;
	}

	public void SetupAllConnections () {
		for (int i = 0; i < nodes.Count; ++i) {
			for (int j = 0; j < nodes[i].node.destinations.Count; ++j) {
				AddConnection(i, nodes[i].node.destinations[j].dest);
			}
			for (int j = 0; j < nodes[i].node.intDestinations.Count; ++j) {
				AddConnection(i, nodes[i].node.intDestinations[j].dest);
			}
			for (int j = 0; j < nodes[i].node.stringDestinations.Count; ++j) {
				AddConnection(i, nodes[i].node.stringDestinations[j].dest);
			}
			for (int j = 0; j < nodes[i].node.boolDestinations.Count; ++j) {
				AddConnection(i, nodes[i].node.boolDestinations[j].dest);
			}
		}
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
				int destIndex = destinations[i].currentDest.dest;
				checkType = destinations[i].currentDest.GetTemplatedType();
				destinations.RemoveAt(index);
				if (checkType == "NONE") {
					if (nodes[selected].node.destinations.Count != 0) {
						for (int j = 0; j < nodes[selected].node.destinations.Count; ++j) {
							if (destID == nodes[selected].node.destinations[j].id) {
								nodes[selected].node.destinations.RemoveAt(j);
							}
						}
					}
				} else if (checkType == "Int32") {
					if (nodes[selected].node.intDestinations.Count != 0) {
						for (int j = 0; j < nodes[selected].node.intDestinations.Count; ++j) {
							if (destID == nodes[selected].node.intDestinations[j].id) {
								nodes[selected].node.intDestinations.RemoveAt(j);
							}
						}
					}
				} else if (checkType == "String") {
					if (nodes[selected].node.stringDestinations.Count != 0) {
						for (int j = 0; j < nodes[selected].node.stringDestinations.Count; ++j) {
							if (destID == nodes[selected].node.stringDestinations[j].id) {
								nodes[selected].node.stringDestinations.RemoveAt(j);
							}
						}
					}
				} else if (checkType == "Boolean") {
					if (nodes[selected].node.boolDestinations.Count != 0) {
						for (int j = 0; j < nodes[selected].node.boolDestinations.Count; ++j) {
							if (destID == nodes[selected].node.boolDestinations[j].id) {
								nodes[selected].node.boolDestinations.RemoveAt(j);
							}
						}
					}
				}
				// Remove the connection.
				RemoveConnection(selected, destIndex);
			}
		}
		return index;
	}

	void RemoveConnection (int from, int to) {
		string conn = from.ToString() + 'x' + to.ToString();
		if (!connections.ContainsKey(conn)) return;
		Destroy(connections[conn].gameObject);
		connections.Remove(conn);
	}

	public void SetMemoriesDelete (bool set) {
		for (int i = 0; i < memories.Count; ++i) {
			memories[i].deleteButton.SetActive(set);
		}
	}

	public void DeleteMemory (int id) {
		Destroy(memories[id].gameObject);
		memories.RemoveAt(id);
		for (int i = id; i < memories.Count; ++i) {
			memories[i].currentMemory.id--;
			ui.AdjustScrollRectMember(memories[i].gameObject.GetComponent<RectTransform>(), i);
		}
	}

	public void SetEventsDelete (bool set) {
		for (int i = 0; i < events.Count; ++i) {
			events[i].deleteButton.SetActive(set);
		}
	}

	public void DeleteEvent (int id) {
		Destroy(events[id].gameObject);
		events.RemoveAt(id);
		for (int i = id; i < events.Count; ++i) {
			events[i].currentEvent.id--;
			ui.AdjustScrollRectMember(events[i].gameObject.GetComponent<RectTransform>(), i);
		}
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
			GetActiveConnections(id);
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

	void GetActiveConnections (int nodeID) {
		activeFromConnections = new List<LineRenderer>();
		activeToConnections = new List<LineRenderer>();
		string[] splitConnection;
		foreach (KeyValuePair<string, LineRenderer> connection in connections) {
			splitConnection = connection.Key.Split('x');
			if (splitConnection[0] == nodeID.ToString()) {
				activeFromConnections.Add(connection.Value);
			} else if (splitConnection[1] == nodeID.ToString()) {
				activeToConnections.Add(connection.Value);
			}
		}
	}

	public void MoveConnection (Vector2 position) {
		for (int i = 0; i < activeFromConnections.Count; ++i) {
			// For from connections, change the first position.
			activeFromConnections[i].SetPosition(0, position);
		}
		for (int i = 0; i < activeToConnections.Count; ++i) {
			// For to connections, change the second position.
			activeToConnections[i].SetPosition(1, position);
		}
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
		// Extra Data
		nodes[node].node.spareData = ui.spareData.text;
	}

	void Start () {
		if (SaveLoad.instance.loadingFile != null) {
			TextNode.id = 0;
			Node temp;
			int length = 0;
			for (int i = 1; i < SaveLoad.instance.loadingFile.Length - 1; ++i) {
				length = SaveLoad.instance.loadingFile[i].Length - 1;
				if (i < SaveLoad.instance.loadingFile.Length - 2) {
					length -= 1;
				}
				temp = JsonUtility.FromJson<Node>(SaveLoad.instance.loadingFile[i].Substring(1, length));
				CreateNode(temp.position);
				nodes[nodes.Count - 1].node = temp;
				nodes[nodes.Count - 1].SetText();
			}
			SaveLoad.instance.loadingFile = null;
			SetupAllConnections();
		}
	}

}
