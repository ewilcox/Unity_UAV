var cameraWV = null;
var cameraRed = null;
var cameraYellow = null;
var cameraBlue = null;
var cameraRV = null;
var cameraOV = null;

var cameraIndex = 0.0;

var target : Transform;

function Start () {
	cameraWV = GameObject.Find("Main Camera");
	if(cameraWV == null){
		Debug.Log("World view camera not found.");	
	}	
	
	cameraRed = GameObject.Find("Red Camera");
	if(cameraRed == null){
		Debug.Log("Red camera not found.");	
	}
	
	cameraYellow = GameObject.Find("Yellow Camera");
	if(cameraYellow == null){
		Debug.Log("Yellow camera not found.");	
	}
	
	cameraBlue = GameObject.Find("Blue Camera");
	if(cameraBlue == null){
		Debug.Log("Blue camera not found.");	
	}
	
	cameraRV = GameObject.Find("Reverse Camera");
	if(cameraRV == null){
		Debug.Log("Reverse camera not found.");	
	}
	
	cameraOV = GameObject.Find("Overhead Camera");
	if(cameraOV == null){
		Debug.Log("Overhead camera not found.");	
	}
}

function Update () {
	if(Input.GetKey(KeyCode.Keypad0)){
		cameraIndex = 0;
	}
	if(Input.GetKey(KeyCode.Keypad1)){
		cameraIndex = 1;	
	}
	if(Input.GetKey(KeyCode.Keypad2)){
		cameraIndex = 2;	
	}
	if(Input.GetKey(KeyCode.Keypad3)){
		cameraIndex = 3;	
	}
	if(Input.GetKey(KeyCode.Keypad4)){
		cameraIndex = 4;	
	}
	if(Input.GetKey(KeyCode.Keypad5)){
		cameraIndex = 5;	
	}
	
	if(cameraWV != null){
		cameraWV.camera.enabled = (cameraIndex == 0);	
	}	
	if(cameraRed != null){
		cameraRed.transform.LookAt(target);
		cameraRed.camera.enabled = (cameraIndex == 1);	
	}
	if(cameraYellow != null){
		cameraYellow.transform.LookAt(target);
		cameraYellow.camera.enabled = (cameraIndex == 2);	
	}
	if(cameraBlue != null){
		cameraBlue.transform.LookAt(target);
		cameraBlue.camera.enabled = (cameraIndex == 3);	
	}
	if(cameraRV != null){
		cameraRV.camera.enabled = (cameraIndex == 4);	
	}
	if(cameraOV != null){
		cameraOV.camera.enabled = (cameraIndex == 5);	
	}
}





