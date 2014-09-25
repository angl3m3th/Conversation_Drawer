﻿using UnityEngine;
using System.Collections;

public class Detail : MonoBehaviour {

	public bool isHit = false;

	private float myAlpha;
	private float fadeConst = 0.2f;
	public bool fading = false;
	public bool bright = false;

	// Use this for initialization
	void Start () {

		myAlpha = 1;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, myAlpha);

		if(fading == true)
		{
			if(myAlpha >= 0)
				myAlpha -= Time.deltaTime * fadeConst;
		}
		
		if(bright == true)
		{
			if(myAlpha <=1)
				myAlpha += Time.deltaTime * fadeConst;
		}
	}

	void OnTriggerEnter (Collider collide)
	{
		if (collide.gameObject.tag == "Converser")
		{
			isHit = true;
			renderer.material.color = Color.blue;
			audio.Play();
		}
	}

	void IsHitOff ()
	{
		isHit = false;
		renderer.material.color = Color.blue;
	}

	public void IsFading()
	{
		fading = true;
		bright = false;
		//print ("Is fading");
	}
	
	public void IsBright()
	{
		fading = false;
		bright = true;
		//print ("Is Bright");
	}

}
