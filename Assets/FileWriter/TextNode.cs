﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class TextNode : MonoBehaviour
{

	static int id = 0;

	public Node node;
	public TextMesh text;
	public GameObject shadow;
	[HideInInspector]
	public Bounds bounds;
	
	public void Setup (Vector3 pos) {
		node = new Node();
		node.id = id;
		id++;
		node.position = pos;
		bounds = GetComponent<SpriteRenderer>().bounds;
	}

	public void Lift () {
		shadow.SetActive(true);
	}

	public void Place () {
		shadow.SetActive(false);
		transform.position = new Vector3(transform.position.x, transform.position.y, 0);
		bounds = GetComponent<SpriteRenderer>().bounds;
	}

	public void SetText () {
		text.text = node.ToTextBox();
	}

}
