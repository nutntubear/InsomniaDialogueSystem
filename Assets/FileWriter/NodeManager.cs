using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class NodeManager : MonoBehaviour
{

	public List<Node> nodes = new List<Node>();
	public List<Transform> nodeObjects = new List<Transform>();
	public GameObject nodePrefab;

	public void CreateNode (Vector3 pos) {
		Node added = new Node();
		added.position = pos;
		nodes.Add(added);
		Transform addedObj = Instantiate(nodePrefab, pos, Quaternion.identity, transform).transform;
		nodeObjects.Add(addedObj);
	}

}
