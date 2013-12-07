using UnityEngine;
using System.Collections;

public class ManualPersonMove : MonoBehaviour {

	public float movementScale = 2.0F;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 movement = Vector3.zero;
		if (Input.GetKey (KeyCode.UpArrow))
			movement.z += movementScale * Time.deltaTime;
		if (Input.GetKey (KeyCode.DownArrow))
			movement.z -= movementScale * Time.deltaTime;
		if (Input.GetKey (KeyCode.LeftArrow))
			movement.x -= movementScale * Time.deltaTime;
		if (Input.GetKey (KeyCode.RightArrow))
			movement.x += movementScale * Time.deltaTime;

		this.transform.Translate (movement);
	}
}
