﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tracer : MonoBehaviour {
	public Camera gameCamera = null;
	private LineRenderer lineRenderer = null;
	public SimpleMover mover;
	private int vertexCount = 0;
	private int stableVertexCount = 0;
	public float minDragToDraw = 1.0f;
	public GameObject lineMakerPrefab = null;
	private Vector3 almostLastVertex = Vector3.zero;
	private Vector3 lastVertex = Vector3.zero;
	private Vector3 lastDirection = Vector3.zero;
	private Vector3 lastStableVertex = Vector3.zero;
	private Vector3 lastStableDirection = Vector3.zero;

	void Start() {
		/*GameObject globalsObject = GameObject.FindGameObjectWithTag("Globals");
		if (globalsObject == null) {
			Application.LoadLevel("Main screen");	
		}
		else {
			Globals globals = globalsObject.GetComponent<Globals>();
			critMat = (Material)Instantiate(Resources.Load(globals.critmat));
			critLineMakerPrefab = (GameObject)Instantiate(Resources.Load(globals.critprefab));
			nonCritMat = (Material)Instantiate(Resources.Load(globals.noncritmat));
			nonCritLineMakerPrefab = (GameObject)Instantiate(Resources.Load(globals.noncritprefab));
		}*/

		mover = GetComponent<SimpleMover>();
		mover.moving = false;
		vertexCount = 0;
		stableVertexCount = 0;
	}

	void Update() {
		HandleTouches();
	}
	
	private void HandleTouches() {		
		if (Input.GetMouseButtonDown (0)) {
			StartLine();
		} else if (Input.GetMouseButton (0)) {
			DragAndDraw();
		}
	}

	public void StartLine(bool startAtVertex = false, Vector3 startVertex = new Vector3())
	{		
		//Vector3 mousePosition = MousePointInWorld();
		//transform.position = MousePosition;
		
		// If this line is not on the required path, create a separate polyline.
		CreateLineMaker(true);
		
		AddVertex(transform.position);
	}

	private void DragAndDraw(bool criticalLine = true) {
		Vector3 mousePosition = MousePointInWorld();
		Vector3 toMouse = mousePosition - transform.position;
		if (toMouse.sqrMagnitude > minDragToDraw * minDragToDraw)
		{
			//transform.Translate (translationToTouch);
			float toMouseMag = toMouse.magnitude;
			if (toMouseMag > 0)
			{
				toMouse /= toMouseMag;
				float speedRamp = 1;
				/*if (speedRampRadius > 0)
				{
					speedRamp = toMouseMag / speedRampRadius;
				}*/
				float moveDist = mover.maxSpeed * speedRamp;
				if (moveDist > toMouseMag)
				{
					moveDist = toMouseMag;
				}
				mover.Move(toMouse, moveDist, true);
			}
			mover.moving = true;
			AddVertex(transform.position);
		}
	}

	private Vector3 MousePointInWorld() {
		Vector3 touchPosition = gameCamera.ScreenToWorldPoint(Input.mousePosition);
		touchPosition.z = transform.position.z;
		return touchPosition;
	}

	private void AddVertex(Vector3 position) {	
		if (vertexCount > 1) {
			// Preserve look of the most recent line segement if the new vertex
			// drastically changes the direction of motion. Without this, the line segement
			// gets distorted as only the center point is ever stored.
			if (Vector3.Dot((position - lastVertex).normalized, lastDirection) < 0.6f) {
				lineRenderer.SetVertexCount(++vertexCount);
				Vector3 midPosition = lastVertex + (lastDirection * minDragToDraw / 4);
				lineRenderer.SetPosition(vertexCount - 1, midPosition); 
				lastVertex = midPosition;
			}
		}
		
		lineRenderer.SetVertexCount(++vertexCount);
		lineRenderer.SetPosition(vertexCount - 1, position); 
		lastDirection = (position - lastVertex).normalized;
		almostLastVertex = lastVertex;
		lastVertex = position;
	}	
	
	public void CreateLineMaker(bool criticalLine) {		
		GameObject newLineMaker = (GameObject)Instantiate(lineMakerPrefab, Vector3.zero, Quaternion.identity);
		newLineMaker.transform.parent = transform;
		lineRenderer = newLineMaker.GetComponent<LineRenderer>();
		lineRenderer.SetVertexCount(0);
		vertexCount = 0;
	}
}
