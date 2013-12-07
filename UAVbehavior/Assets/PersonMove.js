//speed variables
var speed = 1.0;

//movement variables
var startPos = Vector3.zero;
var trans = 0.0;
var xMove = 0.0;
var zMove = 0.0;

//target variables
var bodies = new Array();
var goBack = false;
var turnPoint = 0.0;

//position variables
var intimate = 0.46;
var personal = 1.22;
var social = 3.66;
var distNorm = 2.0;
var distMin = 7.5;
var motionNorm = 1.0;
var sqrLen = 0.0;

function Start(){
	//random returns a float f such that 0 <= f <= 1
	var rand = Random.value;
	//movement in x direction
	//transform into a value between -.5 and .5 (+/- 45 degrees)
	trans = rand - 0.5;
	
	//determine random speed (at least 0.3 so it's not completely tedious)
	rand = Random.value;
	speed = rand + speed;
	
	//determine random starting position within bounding box
	//box in front of fore wall between side walls
	var maxZ = GameObject.Find("/Wall_Front").transform.position.z;
	maxZ = maxZ + GameObject.Find("/Wall_Front").collider.bounds.size.z/2;
	maxZ = maxZ + collider.bounds.size.z/2;
	var minZ = maxZ + 3;
	var maxX = GameObject.Find("/Wall_Right").transform.position.x;
	maxX = maxX - GameObject.Find("/Wall_Right").collider.bounds.size.x/2;
	maxX = maxX - collider.bounds.size.x/2;
	var minX = GameObject.Find("/Wall_Left").transform.position.x;
	minX = minX + GameObject.Find("/Wall_Left").collider.bounds.size.x/2;
	minX = minX + collider.bounds.size.x/2;
	var Y = GameObject.Find("/Floor").transform.position.y;
	Y = Y + GameObject.Find("/Floor").collider.bounds.size.y/2;
	Y = Y + collider.bounds.size.y/2;
	startPos = new Vector3(Random.Range(minX,maxX), Y, Random.Range(minZ,maxZ));//startPos = new Vector3(startX, 0.33, startZ);
	transform.position = startPos;
}
function Update () {
	//determines tilt-ness	xMove = trans * Time.deltaTime * speed;
	//move forward in z direction	zMove = 1.0 * Time.deltaTime * speed;
	if ( goBack)
	{
		xMove = 0.0;
		zMove = -0.5 * Time.deltaTime * speed;
	}
	
	//determine distance between me and robot
	var myEye = new Vector3(0,(collider.bounds.size.y/2)-0.15,0);
    uavs = GameObject.FindGameObjectsWithTag("UAV");
	for(var c=0; c<uavs.length; c=c+1)
	{
		var bc2 = uavs[c].GetComponent("BlockCrowd2");
		if ( bc2 == null )//I think unity is sometimes actually making a fourth gameobject... a kind of fake one, maybe for the disabled uav in the scene?  ignore...
			continue;
		var cVel = bc2.velocity;
		var motion = cVel.magnitude;
		var angle = Mathf.Acos(Vector3.Dot(cVel.normalized,((transform.position+myEye)-uavs[c].transform.position).normalized));
		sqrLen = (uavs[c].transform.position - (transform.position+myEye)).sqrMagnitude;
		sqrLen = sqrLen - (collider.bounds.size.x/2);
		/*
		if( sqrLen < social && !goBack){
			zMove += -.125 * Time.deltaTime * speed;
		}
		if( sqrLen < personal && !goBack){
			zMove += -.25 * Time.deltaTime * speed;
		}
		if( sqrLen < intimate || goBack){
			//too scared = leave
			xMove = 0.0;
			zMove = -0.5 * Time.deltaTime * speed;
			goBack = true;
			turnPoint = transform.position.z;
		}*/
		if ( angle < 90 )	//it moved toward me!
		{
			//how fast? how close?
			var motionScary = motion / motionNorm;
			var scariness = 0.0;
			if ( sqrLen > distNorm )
			{
				var distScary = 0.0;
				if ( sqrLen < distMin )
					distScary = (sqrLen-distNorm)/(distNorm-distMin);
				scariness = distScary * motionScary;
			}
			else
				scariness = motionScary;
			//Debug.Log(distNorm.ToString("F4")+ ","+ sqrLen.ToString("F4")+ ","+
				//(distNorm-sqrLen).ToString("F4")+ ","+ distScary.ToString("F4"));
			//Debug.Log(motionNorm.ToString("F4")+ ","+ motion.ToString("F4")+ ","+
				//motionScary.ToString("F4"));
			//Debug.Log(motionScary.ToString("F4")+ ","+ distScary.ToString("F4")+ ","+
				//scariness.ToString("F4"));
			//Debug.Log("Scare:"+scariness.ToString("F4") + "..." + cVel.x.ToString("F4")+ ","+ cVel.y.ToString("F4")+ ","+
				//cVel.z.ToString("F4")+","+cVel.magnitude.ToString("F4"));
			if ( scariness > 1.0 )
			{
				goBack = true;	//run for it, Marty
				turnPoint = transform.position.z;
			}	
		}
	}
	
	transform.Translate(xMove, 0, zMove);
	
	//succeeded, go away
	if(transform.position.z < turnPoint-2.0 && goBack == true){
		Destroy(gameObject);
		var sc1 = GameObject.Find("/Wall_Front").GetComponent("SpawnCrowds");
		sc1.pass = sc1.pass + 1;
	}
	
	//failed, go away
	if(transform.position.z > -5.5){
		Destroy(gameObject);
		var sc2 = GameObject.Find("/Wall_Front").GetComponent("SpawnCrowds");
		sc2.fail = sc2.fail + 1;
	}}
