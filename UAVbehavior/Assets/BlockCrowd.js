//guard line definition
var ZUpperBound = -5.4;
var ZLowerBound = -5.6;
var YUpperBound = 1.0;
var YLowerBound = 0.9;

//animation variables
private var spin : AnimationState;

//target variables
var currentTarget : Transform;
//var builtin : Transform[];
var bodies : Array;
var targets : Array;
var numBodies = 3.0;

//proximity variables
var ignore = ZLowerBound - 4;
var social = ZLowerBound - 3.66;
var personal = ZLowerBound - 1.22;
var intimate = ZLowerBound - 0.46;
var guardLine = ZUpperBound;
var stopThreshold = ZUpperBound + 3;

//state
var state = "ignore";
var dist = 0.0;

//rotation variables
var rotation = Vector3.zero;
var xRotation = 0.0;
var zRotation = 0.0;
var maxTilt = 15.0;

//movement variables
var movement = Vector3.zero;
var moveSpeed = 2.0;
var followDistance = 0.6;
var keepAliveSpeed = .01;
var goingup = false;

//define animation behaviors
function Start() {
	var rand = Random.value;
	//numBodies = rand*3;
	numBodies = 3;
	
	spin = animation["Spin"];
	spin.layer = 1;
	spin.blendMode = AnimationBlendMode.Additive;
	spin.wrapMode = WrapMode.Loop;
	spin.speed = 2.0;
	
	if(numBodies <= 1){
		bodies = new Array();
		bodies.Add(GameObject.Find("/Red"));
		Destroy(GameObject.Find("/Blue"));
		Destroy(GameObject.Find("/Yellow"));
		targets = new Array(bodies);
	}
	if(numBodies <= 2 && numBodies > 1){
		bodies = new Array();
		bodies.Add(GameObject.Find("/Red"));
		bodies.Add(GameObject.Find("/Yellow"));
		Destroy(GameObject.Find("/Blue"));
		targets = new Array(bodies);
	}
	if(numBodies <= 3 && numBodies > 2){
		bodies = new Array();
		bodies.Add(GameObject.Find("/Red"));
		bodies.Add(GameObject.Find("/Yellow"));
		bodies.Add(GameObject.Find("/Blue"));
		targets = new Array(bodies);
	}
	
	targetsAboveLine = new Array ();
	currentTarget = targets[0].transform;
	
	dist = guardLine - currentTarget.transform.position.z;
}

function Update () {
	animation.CrossFade("Spin");
	
	updateState();
	
	dist = guardLine - currentTarget.transform.position.z;
	
	if(state == "calm"){
		findLine();
		maxTilt = Mathf.Abs((10-5)/(personal-social) * dist);
		moveSpeed = (1-.5)/(personal-social) * dist;
		followDistance = Mathf.Abs((1-2)/(personal-social) * dist);
		YUpperBound = 1.0;
		YLowerBound = 0.9;
		stalk();
		keepAliveSpeed = .001; //const
		keepAlive();
	}
	if(state == "caution"){
		findLine();
		maxTilt = 10.0; //const
		moveSpeed = (2-1)/(intimate-personal) * dist;
		followDistance = Mathf.Abs((.5-1)/(intimate-personal) * dist);
		YUpperBound = 0.9;
		YLowerBound = 0.8;
		stalk();
		keepAliveSpeed = .001; //const
		keepAlive();
	}
	if(state == "aggressive"){
		findLine();
		maxTilt = 10.0; //const
		moveSpeed = 2.0; //const
		followDistance = 0.5; //const
		YUpperBound = 0.6;
		YLowerBound = 0.5;
		stalk();
		keepAliveSpeed = .001; //const
		keepAlive();
	}
	if(state == "ignore"){
		if(currentTarget.transform == null){
			maxTilt = 0.0;
		}
		else{
			maxTilt = Mathf.Abs((5-0)/(social-ignore) * dist);
		}
		moveSpeed = 0.5; //const
		followDistance = 2.0; //const
		YUpperBound = 1.0;
		YLowerBound = 0.9;
		findLine();
		stabilize();
		keepAliveSpeed = .001; //const
		keepAlive();
	}
}

function updateState(){
	findTargets();
	
	if(targets.length > 0){
		var closest = targets[0].transform;
		var pos = targets[0].transform.position.z;
	
		//determine closest z-value
		for (var body in targets)
		{
			if(body.transform.position.z > pos)
			{
				pos = body.transform.position.z;
				closest = body.transform;
			}	
		}
	}
	
	currentTarget = closest;
	
	targetpos = currentTarget.transform.position.z;
	
	state = "ignore";
	
	if(targetpos > social){
		state = "calm";
	}
	if(targetpos > personal){
		state = "caution";
	}
	if(targetpos > intimate){
		state = "aggressive";
	}
	if(targetpos > stopThreshold){
		state = "ignore";
	}
	if(currentTarget == null){
		state = "ignore";	
	}
}

//removes the current target from the list of targets
function removeCurrent(){
	var it = 0.0;
	var currentTargetpos = 0.0;
	
	//determine position of current target in target array
	for (var body in targets)
	{
		if(body.transform == currentTarget)
		{
			currentTargetPos = it;
		}
		it += 1;	
	}	
	
	targets.RemoveAt(currentTargetpos);
}

//determines how many bodies are current targets
function findTargets(){
	var newTargets = new Array ();
	var newTargetsAboveLine = new Array ();
	
	for (var body in bodies)
	{
		if(body.transform.position.z < guardLine && body.transform.position.z > social)
		{
			newTargets.Push(body);
		}
	}
	
	targets = newTargets;
}

//this tells the robot to move to the z-position specified by the z-bounds
function findLine(){
	var controller : CharacterController = GetComponent(CharacterController);
	
	//find the guard line
	if(transform.position.z < ZLowerBound){
		controller.Move(new Vector3(0,0,.01));
	}
	if(transform.position.z > ZUpperBound){
		controller.Move(new Vector3(0,0,-.01));
	}
}

//follow target in x and z directions
function follow() {
	var tx = currentTarget.position.x;
	var tz = currentTarget.position.z;
	
	var myx = transform.position.x;
	var myz = transform.position.z;
	
	var diffx = tx - myx;
	var diffz = tz - myz;
	
	var tiltAroundZ = Input.GetAxis("Horizontal");
	var tiltAroundX = Input.GetAxis("Vertical") ;
	
	//keep from tipping over
	if(zRotation > maxTilt)
	{
		zRotation = maxTilt;
	}
	if(zRotation < -maxTilt)
	{
		zRotation = -maxTilt;
	}
	if(xRotation > maxTilt)
	{
		xRotation = maxTilt;
	}
	if(xRotation < -maxTilt)
	{
		xRotation = -maxTilt;	
	}
	
	//keep within follow distance (x)
	if(diffx < -followDistance)
	{
		movement.x = Time.deltaTime * -moveSpeed;
		zRotation -= 1;
	}
	else if(diffx > followDistance)
	{
		movement.x = Time.deltaTime * moveSpeed;
		zRotation += 1;
	}
	else 
	{
		tiltAroundZ = Input.GetAxis("Horizontal");
		movement.x = 0.0;
	}
	
	//keep within follow distance (z)
	if(diffz < -followDistance)
	{
		movement.z = Time.deltaTime * -moveSpeed;
		xRotation -= 1;
	}
	else if(diffz > followDistance)
	{
		movement.z = Time.deltaTime * moveSpeed;
		xRotation += 1;
	}
	else
	{
		tiltAroundX = Input.GetAxis("Vertical") ;
		movement.z = 0.0;
	}
	
	transform.Translate(movement.x, 0, movement.z, currentTarget.transform);
	transform.eulerAngles = Vector3(xRotation, 0, zRotation);
}


//follow only in x direction
function stalk(){
	var tx = currentTarget.position.x;
	var tz = currentTarget.position.z;
	
	var myx = transform.position.x;
	var myz = transform.position.z;
	
	var diffx = tx - myx;
	var diffz = tz - myz;
	
	var tiltAroundZ = Input.GetAxis("Horizontal");
	var tiltAroundX = Input.GetAxis("Vertical") ;
	
	//keep from tipping over
	if(zRotation > maxTilt)
	{
		zRotation = maxTilt;
	}
	if(zRotation < -maxTilt)
	{
		zRotation = -maxTilt;
	}
	if(xRotation > maxTilt)
	{
		xRotation = maxTilt;
	}
	if(xRotation < -maxTilt)
	{
		xRotation = -maxTilt;	
	}
	
	//keep within follow distance (x)
	if(diffx < -followDistance)
	{
		movement.x = Time.deltaTime * -moveSpeed;
		zRotation += 1;
	}
	else if(diffx > followDistance)
	{
		movement.x = Time.deltaTime * moveSpeed;
		zRotation -= 1;
	}
	else 
	{
		tiltAroundZ = Input.GetAxis("Horizontal");
		movement.x = 0.0;
	}
	
	//keep x value, but rotate forward
	if(xRotation > -maxTilt){
		xRotation -= 1;	
	}
	
	transform.Translate(movement.x, 0, 0, currentTarget.transform);
	transform.eulerAngles = Vector3(xRotation, 0, zRotation);
}


//this resets the robot's tilt and height to normal
function stabilize(){
	transform.eulerAngles = Vector3(0,0,0);
}

//this defines keep alive behavior - bobbing up and down
function keepAlive(){
	//Social distance: idle movement for when the person is far away
	if(transform.position.y > YUpperBound){
		transform.Translate(0,-keepAliveSpeed,0);
		goingup = false;
	}
	if(transform.position.y < YLowerBound){
		transform.Translate(0,keepAliveSpeed,0);
		goingup = true;
	}
	if(transform.position.y < YUpperBound && transform.position.y > YLowerBound){
		if(goingup){
			transform.Translate(0,keepAliveSpeed,0);
			if(transform.position.y >= YUpperBound)
				goingup = false;
		}
		if(!goingup){
			transform.Translate(0, -keepAliveSpeed,0);
			if(transform.position.y <= YLowerBound)
				goingup = true;
		}
	}
}

