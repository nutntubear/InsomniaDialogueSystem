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
		for (int i = 0; i < destinations.Count; ++i) {
			if (nodes[destinations[i].currentDest.dest].bounds.Contains(pos)) {
				index = i;
				destinations.RemoveAt(index);
				if (nodes[selected].node.destinations.Count > 0) {
					nodes[selected].node.destinations.RemoveAt(index);
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
		for (int i = 0; i < destinations.Count; ++i) {
			destinations[i].UpdateMemory();
			nodes[node].node.destinations.Add(destinations[i].currentDest);
		}
		// Events
		if (node == -1) return;
		nodes[node].node.events = new List<DialogueEvent>();
		for (int i = 0; i < events.Count; ++i) {
			events[i].UpdateEvent();
			nodes[node].node.events.Add(events[i].currentEvent);
		}
	}

}
