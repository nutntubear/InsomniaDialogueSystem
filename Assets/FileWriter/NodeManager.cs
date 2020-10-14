using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class NodeManager : MonoBehaviour
{

	public List<TextNode> nodes = new List<TextNode>();
	public GameObject nodePrefab;

	// Temp and hidden
	int selected = -1;
	Bounds temp;

	int dummy = 0;

	bool CheckInsideNode (Vector2 pos, ref int id, int ignore=-1) {
		id = -1;
		for (int i = 0; i < nodes.Count; ++i) {
			if (i == ignore) continue;
			temp = new Bounds(nodes[i].bounds.center, nodes[i].bounds.size * 2);
			if (temp.Contains(pos)) {
				id = i;
				return true;
			}
		}
		return false;
	}

	public bool CreateNode (Vector2 pos) {
		if (CheckInsideNode(pos, ref dummy)) return false;
		TextNode addedObj = Instantiate(nodePrefab, (Vector3)pos, 
							Quaternion.identity, transform).transform.GetComponent<TextNode>();
		nodes.Add(addedObj);
		addedObj.Setup((Vector3)pos);
		return true;
	}

	public bool DeleteNode (Vector2 pos) {
		return true;
	}

	public bool AddDestination (Vector2 pos) {
		return true;
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
		if (!CheckInsideNode(pos, ref dummy, selected)) {
			nodes[selected].Place();
			selected = -1;
			return true;
		}
		return false;
	}

	public void UpdateNode () {
		if (selected != -1) {
			nodes[selected].SetText();
		}
	}

}
