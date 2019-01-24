using UnityEngine;
using System.Collections;

public class ArrowStuck : MonoBehaviour {

	RaycastHit hit;
	public AudioClip hitSound; //sound to play when arrow hits a surface


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Only check for obstacles to stick to if the arrow is moving
		if(GetComponent<Rigidbody>().velocity.magnitude > 0.5) {
			CheckForObstacles();
		}
		else {
			enabled = false;
		}
	}

	void CheckForObstacles() {
		//the length of the raycast needs to be greater the faster the arrow is traveling, in order for it to register
		float myVelocity = GetComponent<Rigidbody>().velocity.magnitude;
		//get length of raycast based on velocity of arrow | based on this, arrows with too little of a velocity will not stick
		float raycastLength = myVelocity * 0.03f;
		
		if(Physics.Raycast(transform.position, transform.forward, out hit, raycastLength)) {
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().clip = hitSound;
			GetComponent<AudioSource>().Play();
			
			//since we need to disable the boxCollider and freeze the rigidbody for the arrow to stick,
			//we must add our own force to any moveable objects (objects with rigidbodies) that the arrow hits
			if(hit.transform.GetComponent<Rigidbody>()) {
				//add force to object hit
				hit.transform.GetComponent<Rigidbody>().AddForce(transform.forward * myVelocity * 10);
			}
			
			//disable arrow's collider | if you leave the arrow's collider on, while inside another object, it may have strange effects
			GetComponent<BoxCollider>().enabled = false;

			//if you require the boxCollider to remain active, say for picking up the arrow, you can instead set the collider to trigger:
			//GetComponent(BoxCollider).isTrigger = true;
			
			//freeze rigidbody
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
			//position arrow
			transform.position = hit.point;
			//parent arrow to object | if the object the arrow hits moves, this will make the arrow move with it
			transform.parent = hit.transform;
			
			//once the arrow is stuck, disable this script
			enabled = false;
		}
		else {
			//make arrows top-heavy | on arrows with little velocity (when the bow is not drawn back very far) the arrows will rotated toward the ground
			Quaternion newRot = transform.rotation;
			newRot.SetLookRotation(GetComponent<Rigidbody>().velocity);
			transform.rotation = newRot;
		}
	}

	void OnCollisionEnter(Collision collision) {
		if(GetComponent<Rigidbody>().velocity.magnitude > 5) {
			if(collision.transform.tag != "Player") {
				GetComponent<BoxCollider>().enabled = false;

				//freeze rigidbody
				GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				//position arrow
				foreach(ContactPoint contact in collision.contacts) {
					Vector3 pos = contact.point;
					transform.position = pos;
				}
				//parent arrow to object | if the object the arrow hits moves, this will make the arrow move with it
				transform.parent = collision.transform;
				
				enabled = false;
			}
		}
	}
}
