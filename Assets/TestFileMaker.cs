using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;
using System.IO;

public class TestFileMaker : MonoBehaviour
{

	public string fileName;
	public List<Node> nodes = new List<Node>();

	void Start () {
		string writeToFile = "";
		for (int i = 0; i < nodes.Count; ++i) {
			writeToFile += nodes[i].SaveNode() + "\n";
		}
		string path = Application.dataPath + "/TestFiles/";
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		File.WriteAllText(path + fileName + ".json", writeToFile);
	}

}
