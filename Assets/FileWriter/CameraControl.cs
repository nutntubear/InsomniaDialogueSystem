using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

	public Camera mainCam;
	UIManager uim;

	[Header("Movement")]
	public int minZoom = 5;
	public int maxZoom = 20;
	float currentZoom;
	public float zoomRate = 1.5f;
	public float zoomSensitivity = 15;
	public float planeSize = 1000;

	[HideInInspector]
	public bool hoveringOver = false;

	Vector2 start; Vector2 current; Vector2 end;

	void Start () {
		if (mainCam == null) {
			mainCam = GetComponent<Camera>();
		}
		uim = GetComponent<UIManager>();
		currentZoom = mainCam.orthographicSize;
	}

	void Update () {
		if (uim.paused || hoveringOver || SaveLoad.instance.paused) return;
		if (Input.mouseScrollDelta.y != 0) {
			// Camera zooming.
			currentZoom -= Input.mouseScrollDelta.y * zoomSensitivity;
			currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
			mainCam.orthographicSize = Mathf.Lerp(mainCam.orthographicSize, currentZoom, Time.deltaTime * zoomRate);
		}
		if (Input.GetMouseButtonDown(0)) {
			// Start camera movement.
			start = mainCam.ScreenToWorldPoint(Input.mousePosition);
		}
		if (Input.GetMouseButton(0)) {
			// Move the camera.
			current = mainCam.ScreenToWorldPoint(Input.mousePosition);
			Vector2 diff = (current - start) / 2;
			mainCam.transform.position -= new Vector3(diff.x, diff.y, 0);
			mainCam.transform.position = new Vector3(
				Mathf.Clamp(mainCam.transform.position.x, -planeSize, planeSize),
				Mathf.Clamp(mainCam.transform.position.y, -planeSize, planeSize),
				mainCam.transform.position.z);
		}
	}

}
