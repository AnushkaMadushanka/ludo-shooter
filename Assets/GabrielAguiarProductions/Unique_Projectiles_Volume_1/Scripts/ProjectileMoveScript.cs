﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class ProjectileMoveScript : MonoBehaviour {

	public float speed;
	public float fireRate = 5f;
	public GameObject muzzlePrefab;
	public GameObject hitPrefab;
	public AudioClip shotSFX;
	public AudioClip hitSFX;
	public List<GameObject> trails;

	private bool collided;

	void Start () {	

		if (muzzlePrefab != null) {
			var muzzleVFX = Instantiate (muzzlePrefab, transform.position, Quaternion.identity);
			muzzleVFX.transform.forward = gameObject.transform.forward;
			var ps = muzzleVFX.GetComponent<ParticleSystem>();
			if (ps != null)
				Destroy (muzzleVFX, ps.main.duration);
			else {
				var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy (muzzleVFX, psChild.main.duration);
			}
		}

		if (shotSFX != null && GetComponent<AudioSource>()) {
			GetComponent<AudioSource> ().PlayOneShot (shotSFX);
		}
	}

	void Update () {	
		if (speed == 0)
			transform.position += (transform.forward)  * (speed * Time.deltaTime);
	}

	void OnCollisionEnter (Collision co) {
		if (co.gameObject.tag != "Bullet" && !collided) {
			collided = true;

			GetComponent<BulletController>()?.DoDamage(co);

			if (shotSFX != null && GetComponent<AudioSource>()) {
				GetComponent<AudioSource> ().PlayOneShot (hitSFX);
			}

			if (trails.Count > 0) {
				for (int i = 0; i < trails.Count; i++) {
					trails [i].transform.parent = null;
					var ps = trails [i].GetComponent<ParticleSystem> ();
					if (ps != null) {
						ps.Stop ();
						Destroy (ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
					}
				}
			}
		
			speed = 0;
			GetComponent<Rigidbody> ().isKinematic = true;

			ContactPoint contact = co.contacts [0];
			Quaternion rot = Quaternion.FromToRotation (Vector3.up, contact.normal);
			Vector3 pos = contact.point;

			if (hitPrefab != null) {
				var hitVFX = Instantiate (hitPrefab, pos, rot);
				var ps = hitVFX.GetComponent<ParticleSystem> ();
				if (ps == null) {
					var psChild = hitVFX.transform.GetChild (0).GetComponent<ParticleSystem> ();
					Destroy (hitVFX, psChild.main.duration);
				} else
					Destroy (hitVFX, ps.main.duration);
			}

			StartCoroutine (DestroyParticle (0f));
		}
	}

	public IEnumerator DestroyParticle (float waitTime) {

		if (transform.childCount > 0 && waitTime != 0) {
			List<Transform> tList = new List<Transform> ();

			foreach (Transform t in transform.GetChild(0).transform) {
				tList.Add (t);
			}		

			while (transform.GetChild(0).localScale.x > 0) {
				yield return new WaitForSeconds (0.01f);
				transform.GetChild(0).localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				for (int i = 0; i < tList.Count; i++) {
					tList[i].localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				}
			}
		}
		
		yield return new WaitForSeconds (waitTime);
		Destroy (gameObject);
	}


}
