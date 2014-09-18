﻿using UnityEngine;
using System.Collections;

public class ConversationScore : MonoBehaviour {
	public SimpleMover mover;
	public PartnerLink partnerLink;
	public Tracer tracer;
	public Tracer partnerTracer;
	public int oldNearestIndex = 0;
	public float score = 0;
	public float scoreToLead;
	public float proximityToLead;
	public float scorePortionExponent = 1;
	public float scoreDeboostOffset = 0.1f;
	public float rewardSpeedBoost;
	public int boostLevels;
	private int currentBoostLevel = 0;
	private bool changingBoostLevel = false;
	public float changeTime;
	private float changeTimeElapsed;
	public float startSpeed;
	public Camera gameCamera = null;

	void Start()
	{
		if (mover == null)
		{
			mover = GetComponent<SimpleMover>();
		}
		if (partnerLink == null)
		{
			partnerLink = GetComponent<PartnerLink>();
		}
		if (tracer == null)
		{
			tracer = GetComponent<Tracer>();
		}
		startSpeed = mover.maxSpeed;
	}
	
	void Update()
	{
		if (!mover.Moving || partnerLink.Partner == null)
		{
			SendMessage("SpeedNormal", SendMessageOptions.DontRequireReceiver);
			SendMessage("ExitWake", SendMessageOptions.DontRequireReceiver);
		}
		else if (partnerLink.Leading)
		{
			// TODO Should not have to do this every frame.
			mover.maxSpeed = startSpeed;
			SendMessage("ExitWake", SendMessageOptions.DontRequireReceiver);
		}
		else if (partnerTracer.GetVertexCount() > 1 && tracer.GetVertexCount() > 1)
		{
			// Find nearest vertex on leader line and the vertex after it.
			int nearestIndex = partnerTracer.FindNearestIndex(transform.position, oldNearestIndex);
			Vector3 nearestVertex = partnerTracer.GetVertex(nearestIndex);
			Vector3 nextVertex;
			if (nearestIndex < partnerTracer.GetVertexCount() - 1)
			{
				nextVertex = partnerTracer.GetVertex(nearestIndex + 1);
			}
			else
			{
				nextVertex = nearestVertex;
				nearestVertex = partnerTracer.GetVertex(nearestIndex - 1);
			}

			// Compare follower to leader line.
			Vector3 nearestToNext = (nextVertex - nearestVertex).normalized;
			Vector3 nearestToFollower = (transform.position - nearestVertex);
			Vector3 pointOnPath = Helper.ProjectVector(nearestToNext, nearestToFollower) + nearestVertex;
			float followerToPathDist = (transform.position - pointOnPath).magnitude;

			// Determine how the required score to get a reward speed boost.
			Vector3 toPartner = (partnerTracer.transform.position - transform.position);
			float scorePortion = 1 - toPartner.magnitude / (partnerLink.Conversation.initiateDistance);
			float scoreReq = scoreToLead * Mathf.Max(Mathf.Pow(scorePortion, scorePortionExponent), 0);

			// Update score based on accuracy.
			float indexPortion = (float)nearestIndex / partnerTracer.GetVertexCount();
			float followThreshold = (((indexPortion * partnerTracer.trailNearWidth) + ((1 - indexPortion) * partnerTracer.trailFarWidth))) / 2;
			float accuracyFactor = Mathf.Max(1 - (followerToPathDist / followThreshold), -1);
			score = Mathf.Max(accuracyFactor * Time.deltaTime, 0);
			
			// Show if player is in partner wake or not.
			if (accuracyFactor > 0)
			{
				SendMessage("EnterWake", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				SendMessage("ExitWake", SendMessageOptions.DontRequireReceiver);
			}

			// Handle special behavior while changing boost level.
			if (changingBoostLevel)
			{
				changeTimeElapsed += Time.deltaTime;
				if (changeTimeElapsed >= changeTime)
				{
					SendMessage("SpeedNormal", SendMessageOptions.DontRequireReceiver);
					changingBoostLevel = false;
					changeTimeElapsed = 0;
				}
			}			

			// Start leading if score is high enough.
			if (score >= scoreToLead && (toPartner.sqrMagnitude <= Mathf.Pow(partnerLink.Partner.yieldProximity, 2) || Vector3.Dot(toPartner, partnerLink.Partner.mover.velocity) < 0))
			{
				partnerLink.SetLeading(true);
				mover.MoveTo(partnerLink.Partner.transform.position + mover.velocity.normalized * partnerLink.yieldProximity * 2);
				mover.maxSpeed = startSpeed;
				SendMessage("SpeedNormal", SendMessageOptions.DontRequireReceiver);
			}

			// Boost speed if score exceeds requirement.
			if (score >= scoreReq && accuracyFactor > 0)
			{
				mover.maxSpeed = startSpeed + rewardSpeedBoost * (Mathf.Min(Mathf.Max(1 - scorePortion, 0), 1));
			}
			else
			{
				mover.maxSpeed = startSpeed;
			}

			// Update boost level. 
			if (boostLevels > 0)
			{
				float scorePortionPerBoost = 1.0f / Mathf.Pow(boostLevels + 1, scorePortionExponent);
				if (scorePortion > scorePortionPerBoost * Mathf.Pow((currentBoostLevel + 1), scorePortionExponent))
				{
					if (currentBoostLevel < boostLevels && accuracyFactor > 0)
					{
						currentBoostLevel++;
						changingBoostLevel = true;
						SendMessage("SpeedBoost", SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (scorePortion < scorePortionPerBoost * Mathf.Pow((currentBoostLevel - scoreDeboostOffset), scorePortionExponent))
				{
					if (currentBoostLevel > 0)
					{
						currentBoostLevel--;
						changingBoostLevel = true;
						SendMessage("SpeedDrain", SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
	}

	private void LinkPartner()
	{
		if (partnerLink != null && partnerLink.Partner != null)
		{
			partnerTracer = partnerLink.Partner.tracer;
		}
	}

	private void UnlinkPartner()
	{
		partnerTracer = null;
	}
}
