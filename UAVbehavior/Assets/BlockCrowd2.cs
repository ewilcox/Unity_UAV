using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if DEBUG

namespace UnityEngine
{
    public class Debug
    {
        public static void Log(string s) { Console.WriteLine(s); }
        public static void LogWarning(string s) { Console.WriteLine(s); }
        public static void LogError(string s) { Console.WriteLine(s); }
        public static void DrawLine(Vector3 start, Vector3 end, Color color) { }
    }

    public class transformdummy
    {
        public Vector3 position;
        public void Translate(Vector3 dummy) { }
    }

    public class AnimationState
    {
        public int layer;
        public AnimationBlendMode blendMode;
        public WrapMode wrapMode;
        public float speed;
    }

    public class animationdummy
    {
        public void CrossFade(string s) { }
        AnimationState asd = new AnimationState();
        public AnimationState this[string s]
        {
            get { return asd; }
        }
    }
    public class boundsdummy
    {
        public Vector3 size;
    }
    public class CapsuleCollider
    {
        public float radius;
        public boundsdummy bounds = new boundsdummy();
    }

    public class MonoBehaviour
    {
        public transformdummy transform = new transformdummy();
        public animationdummy animation = new animationdummy();
    }
    public class GameObject
    {
        public transformdummy transform = new transformdummy();
        public CapsuleCollider collider = new CapsuleCollider();
        public GameObject(Vector3 pos, Vector3 colBoundSize, float capCollRadius)
        {
            this.transform.position = pos;
            this.collider.bounds.size = colBoundSize;
            this.collider.radius = capCollRadius;
        }
        public static GameObject Find(string name)
        {
            switch (name)
            {
                case "/Wall_Right":
                    return new GameObject(new Vector3(4.0F, 1.736009F, -5.854808F), new Vector3(1,7,20), 0);
                    break;
                case "/Wall_Left":
                    return new GameObject(new Vector3(-4.0F, 1.736009F, -5.857873F), new Vector3(1,7,20), 0);
                    break;
                case "/Black":
                    return new GameObject(new Vector3(-1.179642F, 1.75F, -12.29286F), new Vector3(0.875F,3.5F,0.875F), 0.5F);
                    break;
                case "/Floor":
                    return new GameObject(new Vector3(0,0,-2.852886F), new Vector3(20,0.05F,25), 0);
                    break;
                default:
                    return new GameObject(Vector3.zero, Vector3.zero, 0);
            }
        }
    }
}
#endif

//this just allows us to pass one as a parameter, by reference to the actual values (classes are passed by reference... Vector3 structs are not...)
//thus we can pass it once and update at once location and not have to copy to all other places utilizing it every time it updates

//xyz vector memory reference (essentially a pointer)
public class Vector3RefWrap
{
	public Vector3 val;
	public Vector3RefWrap(Vector3 _initVal)
	{
		this.val = _initVal;
	}
}
//float memory reference
public class FloatRefWrap
{
	public float val;
	public FloatRefWrap(float _initVal)
	{
		this.val = _initVal;
	}
}


//--Sensor--
//Computer vision stub.
//has a list of positions of sensed crowds
public abstract class CrowdDetectionSensorBase
{
    public abstract List<Vector3> CrowdsDetected();
}
public class UnitTestCrowdDetectionSensor : CrowdDetectionSensorBase
{
    public List<Vector3> crowds;
    public override List<Vector3> CrowdsDetected()
    {
        return crowds;
    }
}
public class CrowdDetectionSensor : CrowdDetectionSensorBase
{
	private BlockCrowd2 unityScriptAnchor;
    //will take UAV entity (ie BlockCrowd2) by reference
	public CrowdDetectionSensor(BlockCrowd2 _unityScriptAnchor)
	{
		this.unityScriptAnchor = _unityScriptAnchor;
	}

    //called by CrowdPercept in it's update()
    //returns list of egocentric vectors (pointing from UAV to Crowd objects)
	public override List<Vector3> CrowdsDetected()
	{
		List<GameObject> simCrowds = new List<GameObject> ();
		if (GameObject.Find ("/Red") != null)
			simCrowds.Add (GameObject.Find ("/Red"));
		if (GameObject.Find ("/Yellow") != null)
			simCrowds.Add (GameObject.Find ("/Yellow"));
		if (GameObject.Find ("/Blue") != null)
			simCrowds.Add (GameObject.Find ("/Blue"));
		if (GameObject.Find ("/Black") != null)
			simCrowds.Add (GameObject.Find ("/Black"));

        //list of egocentric vectors (pointing from UAV to Crowd objects)
		List<Vector3> sensedCrowds = new List<Vector3> ();
		foreach(GameObject crowd in simCrowds)
		{
            //distance from UAV to crowd GameObject position
			float dist = (crowd.transform.position - unityScriptAnchor.transform.position).magnitude;
            //if UAV can see the crowd
			if ( dist < unityScriptAnchor.simDetectCrowdRange

			    //otherwise how could we see them? assumes 180 degree field of view, though, and no rotation...
			    //TODO: make it better by handling rotation and field of view
			    && crowd.transform.position.z < unityScriptAnchor.transform.position.z)
			{
                //then add the egocentric vector pointing to the crowd to sensedCrowds
				sensedCrowds.Add(crowd.transform.position - unityScriptAnchor.transform.position);
			}
		}
		if (sensedCrowds.Count > 4 || sensedCrowds.Count < 0)
			Debug.Log("***** vector count exceeds crowd elements by: " + sensedCrowds.Count);
		return sensedCrowds;
	}
}
//--Percept--
//--returns blockableCrowds (ordered list of crowds) and/or sensedCrowds (unordered)
//simulated generic crowd detection percept
public class CrowdPercept
{
	CrowdDetectionSensor sensor;

	List<Vector3RefWrap> blockableCrowds = new List<Vector3RefWrap>();

	//not just a list, but actually a sorted list, but the elements themselves are changed to allow a reference to a 
	//changing point of avoidance (one crowd passing another by, perhaps) 
	//so this is actually a list ordered so that BlockableCrowds[0] is always the coordinates of the closest crowd
	public List<Vector3RefWrap> BlockableCrowds
	{
		get { return this.blockableCrowds; }
	}
	private int blockableCrowdCount = 0;
	public int BlockableCrowdCount
	{
		get { return this.blockableCrowdCount; }
	}

	List<Vector3RefWrap> sensedCrowds = new List<Vector3RefWrap>();
	//not sorted, but just a list of those within sensor range... for Avoid behaviors
	public List<Vector3RefWrap> SensedCrowds
	{
		get { return this.sensedCrowds; }
	}
	private int sensedCrowdCount = 0;
	public int SensedCrowdCount
	{
		get { return this.sensedCrowdCount; }
	}

	private BlockCrowd2 unityScriptAnchor;

	public CrowdPercept (BlockCrowd2 _unityScriptAnchor, CrowdDetectionSensor _sensor)
	{
		this.sensor = _sensor;
		this.unityScriptAnchor = _unityScriptAnchor;
	}

	public void Update()
	{
		SortedList<float,Vector3> sortCrowds = new SortedList<float, Vector3> ();
		//use simDetectCrowdRange to simulate data being available or not; return those within range.
		List<Vector3> crowds = sensor.CrowdsDetected ();
		foreach (Vector3 crowd in crowds) {
			//must be in front of block line
			if (crowd.z < 0) //i.e. in front of the block line, and the UAV
			{
				sortCrowds.Add(Math.Abs (crowd.z), crowd);
			}
		}

        //these lists are used as percepts; they get assigned once, passed to a motor schema perhaps; then they expect values to continue updating
        //note that we overwrite crowd positions according to the order they appear in the sensor data, or the distance they are from the blocking plane
        //there's no guarantee that the crowd a certain behavior responded to at one instant is the same one it will respond to in the next
        //also, we must only grow the list, and overwrite, so we still refer to the same Vector3RefWrap which was passed to a motor schema or behavior
		blockableCrowdCount = sortCrowds.Count;
        while(blockableCrowdCount > this.blockableCrowds.Count)
            blockableCrowds.Add(new Vector3RefWrap(Vector3.zero));
		for(int i=0; i<this.blockableCrowds.Count; i++)
		{
			if ( i < sortCrowds.Count )
			{
				blockableCrowds[i].val = sortCrowds[sortCrowds.Keys[i]];
			}	
			else
			{
				blockableCrowds[i].val.x = float.MaxValue;
				blockableCrowds[i].val.y = float.MaxValue;
				blockableCrowds[i].val.z = float.MaxValue;
			}
			//This debug verified when they are out of range their values are reset to max values, as they move back into range they are reset.
			//Debug.Log("x["+i+"]="+blockableCrowds[i].val.x + "y["+i+"]="+ blockableCrowds[i].val.y+"z["+i+"]="+blockableCrowds[i].val.z);
		}
        sensedCrowdCount = crowds.Count;
        while (sensedCrowdCount > this.sensedCrowds.Count)
            sensedCrowds.Add(new Vector3RefWrap(Vector3.zero));
		for(int i=0; i<this.sensedCrowds.Count; i++)
		{
			if ( i < crowds.Count )
				sensedCrowds[i].val = crowds[i];
			else
			{
				sensedCrowds[i].val.x = float.MaxValue;
				sensedCrowds[i].val.y = float.MaxValue;
				sensedCrowds[i].val.z = float.MaxValue;
			}
		}
	}
	
	public Vector3RefWrap NthCrowd(int n)
	{
		return blockableCrowds[n];
	}
}



//--Sensor--
//returns vector3 position of uav relative to Hokuyo.
public abstract class HokuyoBase
{
    public abstract Vector3 HokuyoSensorData();
}
public class UnitTestHokuyo : HokuyoBase
{
    public Vector3 uavLocFromSensor;
    public override Vector3 HokuyoSensorData()
    {
        return uavLocFromSensor;
    }
}
public class Hokuyo : HokuyoBase
{
	private BlockCrowd2 unityScriptAnchor;
	public Hokuyo(BlockCrowd2 _unityScriptAnchor)
	{
		unityScriptAnchor = _unityScriptAnchor;
	}
	
	//TODO: expand on this, from ground-truth to a simple simulation of the hokuyo planar laser ranger
    //called by LocalizationPercept_Hokuyo
	public override Vector3 HokuyoSensorData()
	{
		//note that we are returning the UAV's location *relative to the hokuyo*; this is all we get from such a sensor at best
		Vector3 hokuyoLocation = new Vector3(
			GameObject.Find ("/Wall_Left").transform.position.x + GameObject.Find ("/Wall_Left").collider.bounds.size.x / 2,
			GameObject.Find ("/Floor").transform.position.y + (float)unityScriptAnchor.HallwaySize/2.0F,
			GameObject.Find ("/BlockLine").transform.position.z);
		return unityScriptAnchor.transform.position - hokuyoLocation;
	}
}
//--Percept--
//simulated Hokuyo sensor percept implementation
//note that the hokayu is located 6 feet from the floor on the right wall, so it can tell where both walls and ceiling are relative the UAV
public class LocalizationPercept_Hokuyo
{
	private BlockCrowd2 unityScriptAnchor;
    //raw data from Hokuyo sensor
	private Vector3RefWrap uavLoc = new Vector3RefWrap(Vector3.zero);
	private Vector3RefWrap hokuyoLoc = new Vector3RefWrap(Vector3.zero);
	public Vector3RefWrap HokuyoLoc
	{
		get { return this.hokuyoLoc; }
	}
    
    //relative to UAV
	private FloatRefWrap leftWall = new FloatRefWrap(0);
	private FloatRefWrap rightWall = new FloatRefWrap(0);
	private FloatRefWrap ceiling = new FloatRefWrap(0);
	private FloatRefWrap floor = new FloatRefWrap(0);

	public FloatRefWrap LeftWall
	{
		get { return leftWall; }
	}
	public FloatRefWrap RightWall
	{
		get { return rightWall; }
	}
	public FloatRefWrap Ceiling
	{
		get { return ceiling; }
	}
	public FloatRefWrap Floor
	{
		get { return floor; }
	}
    private HokuyoBase sensor;
    public LocalizationPercept_Hokuyo(BlockCrowd2 _unityScriptAnchor, HokuyoBase _sensor)
	{
		this.unityScriptAnchor = _unityScriptAnchor;
		this.sensor = _sensor;
	}

	public void Update()
	{
		//if we expand upon the hokuyo, we'll need to analyze the... points... to get UAV relative to sensor...right now it is just ground truth
		uavLoc.val = sensor.HokuyoSensorData ();


		//from this, we know the UAV's location relative to the sensor; for egocentric coords, we need the opposite
		hokuyoLoc.val = Vector3.zero - uavLoc.val;
		//and then we need to know where the walls are:
		this.rightWall.val = hokuyoLoc.val.x;
		this.leftWall.val = rightWall.val + (float)unityScriptAnchor.HallwaySize;
		this.ceiling.val = hokuyoLoc.val.y + (float)unityScriptAnchor.HallwaySize/2.0F;
		this.floor.val = hokuyoLoc.val.y - (float)unityScriptAnchor.HallwaySize/2.0F;
	}
}




//--Motor Schemas: percept->motor schema->vector (response)
public abstract class UAVMotorSchema
{
    //response of this motor schema; this will have a magnitude [0.0, 1.0], which gets scaled later
	protected Vector3 responseTranslate;
	public Vector3	ResponseTranslate
	{
		get{ return responseTranslate;}
	}
	protected Vector3 responseRotate;
	public Vector3 ResponseRotate
	{
		get{ return responseRotate;}
	}
	protected BlockCrowd2 unityScriptAnchor;
	public UAVMotorSchema(BlockCrowd2 _unityScriptAnchor)
	{
		unityScriptAnchor = _unityScriptAnchor;
	}

    //each motor schema must implement this, and inside must update responseTranslate and responseRotate according to their design
	public abstract void Update();
}

//walls
//perpendicular field, perhaps from wall, exponential, strongest at one coordinate, weakest (1%) at the "min" coordinate, absent elsewhere
//currently only supports axis-aligned fields for orientation
public class PerpendicularExponentialIncrease : UAVMotorSchema
{
	protected Vector3 fieldOrient;
	protected FloatRefWrap maxFieldCoord, minFieldCoord;
	protected float peakFieldStrength;

	//give it a unit vector away from the 'wall'; max is the surface of the wall in x/y coordinate, it will have full strength at max, none at min.
	//simple calc presumes the fieldOrient is axis-aligned, only one dimension for the field
    //field orient--direction of field
	public PerpendicularExponentialIncrease (Vector3 _fieldOrient, FloatRefWrap _max, FloatRefWrap _min, 
	                                         float _peakFieldStrength, 
	                                         BlockCrowd2 _unityScriptAnchor) : base (_unityScriptAnchor)
	{
		fieldOrient = _fieldOrient;
		maxFieldCoord = _max;
		minFieldCoord = _min;
		peakFieldStrength = _peakFieldStrength;
	}

	public override void Update()
	{
		this.responseRotate = Vector3.zero;
		this.responseTranslate = Vector3.zero;
		bool fieldNeg = minFieldCoord.val > maxFieldCoord.val; //i.e. field orients negative on axis

		//if not in range, response is zero
		if ( fieldNeg?( 0 < minFieldCoord.val && 0 >= maxFieldCoord.val )
		    : (0 > minFieldCoord.val && 0 <= maxFieldCoord.val) )
		{
			//blockCrowd2.wallAvoidStrength * (100^-x) * fieldOrient, where x is [0,1] between min and max
			//exponentially increasing as wall is approached, starts at 1% strength at bound
			float posInScale = Math.Abs (maxFieldCoord.val);
			float fieldLen = Math.Abs(maxFieldCoord.val - minFieldCoord.val);
			float x = posInScale / fieldLen;
			float responseScale = 
				(float)(peakFieldStrength * Math.Pow (100,0-x));
			this.responseTranslate.x = this.fieldOrient.x * responseScale;
			this.responseTranslate.y = this.fieldOrient.y * responseScale;
		}
	}
}
//crowd follow--(passed a projected point on plane)
public class AttractiveExponentialDecrease : UAVMotorSchema
{
	private Vector3RefWrap position;
	private float capDistance;
	private float peakFieldStrength;
	private bool enableX, enableY, enableZ;
	
	public AttractiveExponentialDecrease(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _peakFieldStrength,
	                                     float _capDistance, bool _enableX, bool _enableY, bool _enableZ) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
		this.peakFieldStrength = _peakFieldStrength;
		this.enableX = _enableX;
		this.enableY = _enableY;
		this.enableZ = _enableZ;
	}
	public override void Update()
	{
		this.responseRotate = Vector3.zero;

		this.responseTranslate = Vector3.zero;
		Vector3 fieldVecOrient = position.val; //away from UAV, toward position
		//note infinite field range, exponential force proprotional to distance from desired position
		//caps at holdPositionStrength, which is at distance capDistance...
		float x = Math.Min (Math.Abs(fieldVecOrient.magnitude), capDistance) / capDistance;
		float responseScale = (float)(this.peakFieldStrength * (Math.Pow (100, x) / 100.0));
		Vector3 responseVector = Vector3.zero;
		if ( fieldVecOrient.magnitude > 0 )
		{
			responseVector = (fieldVecOrient / fieldVecOrient.magnitude) * responseScale;
		}

		if ( this.enableX )
			this.responseTranslate.x = responseVector.x;
		if ( this.enableY )
			this.responseTranslate.y = responseVector.y;
		if ( this.enableZ )
			this.responseTranslate.z = responseVector.z;
	}
}

//crowd avoid (not a projection)
public class RepulsiveExponentialIncrease : UAVMotorSchema
{
	private Vector3RefWrap position;
	private float capDistance;
	private float peakFieldStrength;
	private bool enableX, enableY, enableZ;
	//for crowd avoid, we need full force at the surface of the cylinder/collider; exponential decreasing thereafter.
	//for that, we need to define a distance which gets subtracted out when determining the strength of the field
	private float deadZoneAroundPosition;
	
	public RepulsiveExponentialIncrease(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _peakFieldStrength,
	                                    float _capDistance, bool _enableX, bool _enableY, bool _enableZ, 
	                                    float _deadZoneAroundPosition) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
		this.peakFieldStrength = _peakFieldStrength;
		this.enableX = _enableX;
		this.enableY = _enableY;
		this.enableZ = _enableZ;
		this.deadZoneAroundPosition = _deadZoneAroundPosition;
	}
	public override void Update()
	{
		this.responseRotate = Vector3.zero;

		this.responseTranslate = Vector3.zero;
		Vector3 fieldVecOrient = Vector3.zero - position.val;//toward UAV
		//note infinite field range, exponential force proprotional to distance from desired position
		//decay reaches 1% at capDistance, but strictly never reaches zero (always applies something)
		float x = Math.Abs(fieldVecOrient.magnitude-this.deadZoneAroundPosition) / capDistance;
		float responseScale = (float)(this.peakFieldStrength * Math.Pow (100, 0.0F-x));
		Vector3 responseVector = (fieldVecOrient / fieldVecOrient.magnitude) * responseScale;

		if ( this.enableX )
			this.responseTranslate.x = responseVector.x;
		if ( this.enableY )
			this.responseTranslate.y = responseVector.y;
		if ( this.enableZ )
			this.responseTranslate.z = responseVector.z;
	}
}

//random 2d scaled down by exponential decrease from point...point passed is truncated/projected onto plane
public class Rand2D : UAVMotorSchema
{
	private float changeTime = 0.0F;
	private Vector3 currRandMove = Vector3.zero;
    //position is a point directly down the z axis from crowd (that you're currently focused on) onto the plane in which
    //the UAV exists.  Used to scale 
	private Vector3RefWrap position;
    private float capDistance;
    private float peakFieldStrength;
    private float interval;
	
	public Rand2D(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _capDistance, float _interval, float _peakFieldStrength) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
        this.peakFieldStrength = _peakFieldStrength;
        this.interval = _interval;
	}
	public override void Update()
	{
		//note infinite field range, exponential force proprotional to distance from desired position
		//decay reaches 1% at capDistance, but strictly never reaches zero (always applies something)
		float x = position.val.magnitude / capDistance;
		float responseScale = (float)Math.Pow (100, 0.0F-x) * (float)this.peakFieldStrength;

		if (Time.time > this.changeTime) {
			UnityEngine.Random.seed = (int)System.DateTime.Now.Ticks;
            this.currRandMove.x = (2 * UnityEngine.Random.value - 1.0F);
			this.currRandMove.y = (2 * UnityEngine.Random.value - 1.0F);
			this.currRandMove = this.currRandMove / this.currRandMove.magnitude;
			this.changeTime = Time.time + this.interval;
		}
		this.responseTranslate = this.currRandMove * responseScale;
	}
}




//---Behaviors: FINDS unityScriptAnchor.percept -> motor schema -> vector (response)
//"Behaviors are different from motor schemas in that they know which percepts to get to 
//pass to motor schemas and what motor schemas or child behaviors to use to accomplish their behavior."
public abstract class UAVBehavior
{
    //the behavior's response, which will be the sum of its child behaviors' and motor schemas' responses
	protected Vector3 responseTranslate;
	public Vector3	ResponseTranslate
	{
		get{ return responseTranslate;}
	}
	protected Vector3 responseRotate;
	public Vector3 ResponseRotate
	{
		get{ return responseRotate;}
	}
	//percepts are accessible via unityScriptAnchor:
	protected BlockCrowd2 unityScriptAnchor;
	public UAVBehavior(BlockCrowd2 _unityScriptAnchor)
	{
		unityScriptAnchor = _unityScriptAnchor;
	}
	
	protected Dictionary<string,UAVBehavior> childBehaviors = new Dictionary<string,UAVBehavior>();
	protected Dictionary<string,UAVMotorSchema> motorSchema = new Dictionary<string, UAVMotorSchema>();

	public virtual void Update ()
	{
		//basically a coordination function, simple pfield sum across this behavior and any children:
		this.responseTranslate = Vector3.zero;
		this.responseRotate = Vector3.zero;
		foreach (string behKey in childBehaviors.Keys) 
		{
			childBehaviors[behKey].Update();
			this.responseTranslate = this.responseTranslate + childBehaviors[behKey].ResponseTranslate;
			this.responseRotate = this.responseRotate + childBehaviors[behKey].ResponseRotate;
		}
		foreach(string msKey in motorSchema.Keys)
		{
			this.motorSchema[msKey].Update ();
			this.responseTranslate = this.responseTranslate + motorSchema[msKey].ResponseTranslate;
			this.responseRotate = this.responseRotate + motorSchema[msKey].ResponseRotate;
		}
	}
}


//---Child Behaviors

//KeepHeight: two opposing PerpendicularExponentialIncrease fields, with their minimums at the height in question,
//and their maximums at the floor/ceiling perceptions
//Percepts:  LocPerceptHokuyo
//Motor Schemas: PerpendicularExponentialIncrease
public class KeepHeight : UAVBehavior
{
	//in terms of coordinates, this becomes relative to the UAV
	//but, initially we do our constructor with a fixed value, which we preserve to know our offset from the floor percept
	private float height;

    //the distance we want from the floor doesn't change, but coordinates are egocentric; the position of that height in question DOES change, relative to the UAV.
    //this is a transformation of the floor percept from the planar laser sensor to the y-axis position desired
	private FloatRefWrap heightRelative = new FloatRefWrap(0);

	public void KeepHeightField(Wall _wall)
	{
		Vector3 fieldOrient = Vector3.zero;
		FloatRefWrap fieldMax = new FloatRefWrap(0);
		string msKey = "?";
		switch (_wall) {
		case Wall.Ceiling:
			fieldOrient = new Vector3 (0, -1, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.Ceiling;
			msKey = "ceiling";
			break;
		case Wall.Floor:
			fieldOrient = new Vector3 (0, 1, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.Floor;
			msKey = "floor";
			break;
		}
		this.motorSchema.Add (msKey, new PerpendicularExponentialIncrease (fieldOrient, fieldMax, heightRelative, 
		                                                                   (float)unityScriptAnchor.keepHeightStrength, 
		                                                                   unityScriptAnchor));
	}
	
	public KeepHeight(BlockCrowd2 _unityScriptAnchor, float _height) : base(_unityScriptAnchor)
	{
		this.height = _height;
		KeepHeightField (Wall.Ceiling);
		KeepHeightField (Wall.Floor);
	}

	public override void Update ()
	{
		this.heightRelative.val = unityScriptAnchor.LocPerceptHokuyo.Floor.val + height;

		base.Update ();
	}
}

//hold a horizontal position, used for hallway centering while "watching"
//Percepts:  LocPerceptHokuyo
//Motor Schemas: AttractiveExponentialDecrease
public class HoldCenter : UAVBehavior
{
    //the point in the center, which changes relative to the UAV, so we transform the left wall, floor, and hokuyo sensor locations to provide it to the motor schema
	private Vector3RefWrap midPoint;

	public HoldCenter(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		midPoint = new Vector3RefWrap( new Vector3 (
			_unityScriptAnchor.LocPerceptHokuyo.LeftWall.val - (float)_unityScriptAnchor.HallwaySize/2.0F,
			_unityScriptAnchor.LocPerceptHokuyo.Floor.val + (float)_unityScriptAnchor.AboveEyeLevel,
			_unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z
			));
		this.motorSchema.Add ("ms", new AttractiveExponentialDecrease (_unityScriptAnchor, midPoint, 
		                                                     (float)_unityScriptAnchor.holdPositionStrength, 
		                                                               (float)_unityScriptAnchor.HallwaySize, true, false, false)
				);
	}

	public override void Update ()
	{
		midPoint.val.x = unityScriptAnchor.LocPerceptHokuyo.LeftWall.val - (float)unityScriptAnchor.HallwaySize/2.0F;
		midPoint.val.y = unityScriptAnchor.LocPerceptHokuyo.Floor.val + (float)unityScriptAnchor.AboveEyeLevel;
		midPoint.val.z = unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z;

		base.Update ();
	}
}

//follow a crowd--only in x-dimension, still.  Conflicts with eye-level field, otherwise, since our percept is *center* of crowd (i.e. belly button...)
//Percepts: LocPerceptHokuyo, CrowdPercept
//Motor Schemas: AttractiveExponentialDecrease
public class Follow : UAVBehavior
{
    //for the magnitude of the attractive force, we want to be drawn to the crowd, or rather the position in the blocking plane directly in front of the crowd
    //height doesn't matter to this behavior, just that we're in front, so we use 0.0, the existing coordinate of the UAV, in y
    //z should be 0.0 too, but we use the hokuyo's location percept just in case of future changes
	private Vector3RefWrap crowdProjectOnPlane;
	
	public Follow(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		this.crowdProjectOnPlane = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.NthCrowd(0).val.x,
			0.0F,
			_unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z));
        //TODO: transform the crowd projection to take eye-level height into account; then, enable Y-dimension field from this, and get rid of the KeepHeight in Follow-state.
		this.motorSchema.Add ("ms", new AttractiveExponentialDecrease (_unityScriptAnchor, crowdProjectOnPlane, 
		                                                     (float)_unityScriptAnchor.followStrength,
		                                                     (float)_unityScriptAnchor.HallwaySize * 0.70F, 
		                                                     true, false, false)
				);
	}
	
	public override void Update ()
	{
		this.crowdProjectOnPlane.val.x = unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x;
		this.crowdProjectOnPlane.val.z = unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z;
		
		base.Update ();
	}
}

//keeps the UAV from colliding with the crowds, interrupting its service and possibly presenting a danger
//Percepts: CrowdPercept
//Motor Schemas: RepulsiveExponentialIncrease
public class AvoidCrowd : UAVBehavior
{
    //doesn't attempt to move up or down, nor should the avoidance care whether we're close to their feet or head,
    //so y-coordinate is 0.0, the UAV's current location.  Technically this means we also avoid the empty space above their head.
	private Vector3RefWrap crowdUAVLevel;
	
    private int nthCrowd;//avoid this crowd, in order of z-distance, so we can avoid running into any of them... perhaps closest 1 would usually do, but if they're side by side...
	
	public AvoidCrowd(BlockCrowd2 _unityScriptAnchor, int _nthCrowd ) : base(_unityScriptAnchor)
	{
		this.nthCrowd = _nthCrowd;
		this.crowdUAVLevel = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.SensedCrowds[_nthCrowd].val.x,
			0.0F,
			_unityScriptAnchor.CrowdPercept.SensedCrowds[_nthCrowd].val.z));
		this.motorSchema.Add ("ms", new RepulsiveExponentialIncrease (_unityScriptAnchor, crowdUAVLevel,
		                                                     (float)_unityScriptAnchor.crowdAvoidStrength, 
		                                                     (float)_unityScriptAnchor.crowdAvoidDepth, 
		                                                     true, false, false,
		                                                     (float)_unityScriptAnchor.CrowdAvoidDeadZone)
				);
		
		//I was going to put detected crowds as omnidirectional... but after discussion in lecture today, 
		//I don't think so, I think it's an onboard forward only percept... so, not sure how we would 
		//actually avoid that "wall-checked a guy" behavior, since we can't see them anymore... wait awhile?
		//TODO: actually make a fixed action pattern around the "crowd" former location to continue avoiding for some predicted time
		//...and quite possibly eliminate detected vs sorted crowds, since they will essentially be the same...
	}
	
	public override void Update ()
	{
		this.crowdUAVLevel.val.x = unityScriptAnchor.CrowdPercept.SensedCrowds[this.nthCrowd].val.x;
		this.crowdUAVLevel.val.z = unityScriptAnchor.CrowdPercept.SensedCrowds[this.nthCrowd].val.z;
		
		base.Update ();
	}
}

//uses Rand2D to make unpredictable motions in front of a crowd, to scare them hopefully.
//Percepts: LocPerceptHokuyo, CrowdPercept
//Motor Schemas: Rand2D
public class ThreateningRand2D : UAVBehavior
{
    //if they're close enough to the blocking plane for threatening behaviors, that's great, but doing random 2D while across the hallway won't scare them,
    //it'll just slow us down getting there.  So, the strength of this pfield is determined by the usual exponential function, relative to being close to the
    //point on the blocking plane in front of the crowd
	private Vector3RefWrap crowdProjectOnPlane;
	public ThreateningRand2D(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		this.crowdProjectOnPlane = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x,
			_unityScriptAnchor.LocPerceptHokuyo.Floor.val + (float)_unityScriptAnchor.ThreatenHeight,
			_unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z));
		this.motorSchema.Add ("ms", new Rand2D (_unityScriptAnchor, this.crowdProjectOnPlane, 
		                                        (float)_unityScriptAnchor.randThreaten2DDepth,
		                                        (float)_unityScriptAnchor.randThreaten2DInterval,
		                                        (float)_unityScriptAnchor.randThreaten2DStrength)
				);
	}
	public override void Update ()
	{
		this.crowdProjectOnPlane.val.x = unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x;
		this.crowdProjectOnPlane.val.y = unityScriptAnchor.LocPerceptHokuyo.Floor.val + (float)unityScriptAnchor.ThreatenHeight;
		this.crowdProjectOnPlane.val.z = unityScriptAnchor.LocPerceptHokuyo.HokuyoLoc.val.z;
		base.Update ();
	}
}



//---Parent Behaviors
//released depending on distance of closest crowd.  Released == added to list of childBehaviors

//Uses: KeepHeight and HoldCenter
public class Watching : UAVBehavior
{
	public static Vector3 keepheightDbgLineOffset = new Vector3(1, 1, 0);
	public static Vector3 followishDbgLineOffset = new Vector3(1, 1, 0);
	public Watching(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, (float)_unityScriptAnchor.AboveEyeLevel));
		childBehaviors.Add ("holdctr", new HoldCenter(_unityScriptAnchor));
	}
		//TODO: perhaps GazeStatic(forward) as well
	public override void Update ()
	{
		base.Update ();
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.keepheightDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.keepheightDbgLineOffset
		                + this.childBehaviors["keepheight"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.blue);
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.followishDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.followishDbgLineOffset
		                + this.childBehaviors["holdctr"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.green);
	}
}
//Uses: KeepHeight and Follow
public class Approaching : UAVBehavior
{
	public Approaching(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, (float)_unityScriptAnchor.EyeLevel));
		childBehaviors.Add ("follow", new Follow(_unityScriptAnchor));
	}
	//TODO: perhaps GazeNearest(Ci)
	public override void Update ()
	{
		base.Update ();
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.keepheightDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.keepheightDbgLineOffset
		                + this.childBehaviors["keepheight"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.blue);
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.followishDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.followishDbgLineOffset
		                + this.childBehaviors["follow"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.green);
	}
}
//Uses: KeepHeight, Follow and ThreateningRand2D
public class Threatening : UAVBehavior
{
	public Threatening(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, (float)_unityScriptAnchor.EyeLevel));
		childBehaviors.Add ("follow", new Follow(_unityScriptAnchor));
		childBehaviors.Add ("rand2d", new ThreateningRand2D (_unityScriptAnchor));
		//TODO: perhaps GazeStatic(forward) as well
	}
	public override void Update ()
	{
		base.Update ();
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.keepheightDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.keepheightDbgLineOffset
		                + this.childBehaviors["keepheight"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.blue);
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.followishDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.followishDbgLineOffset
		                + this.childBehaviors["follow"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.green);
		
		Debug.DrawLine (this.unityScriptAnchor.transform.position + Watching.followishDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + Watching.followishDbgLineOffset
		                + this.childBehaviors["rand2d"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.yellow);
	}
}



public enum Wall
{
	Left,
	Right,
	Ceiling,
	Floor
}
//^^ used by vv //  Only tactical behavior.
//avoids both wall and crowd collisions
//Uses: AvoidCrowd and MS: PerpendicularExponentialIncrease
public class Avoid : UAVBehavior
{
	protected int currNumCrowdTracks = 0;

    //wall avoid motor schema need one moving value not provided by a percept: the position, wallAvoidDepth from each surface, 
    //at which the exponential of the pfield strength reaches 1% (the "min").  These maintain that input to the motor schema.
	FloatRefWrap avoidFieldMin_Left = new FloatRefWrap(0);
	FloatRefWrap avoidFieldMin_Right = new FloatRefWrap(0);
	FloatRefWrap avoidFieldMin_Floor = new FloatRefWrap(0);
	FloatRefWrap avoidFieldMin_Ceiling = new FloatRefWrap(0);

	public void AvoidWall(Wall _wall)
	{
		Vector3 fieldOrient = Vector3.zero;
		FloatRefWrap fieldMax = new FloatRefWrap(0);
		FloatRefWrap fieldMin = new FloatRefWrap(0);
		string msKey = "?";
		switch (_wall) {
		case Wall.Left:
			fieldOrient = new Vector3 (-1, 0, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.LeftWall;
			fieldMin = avoidFieldMin_Left;
			msKey = "left";
			break;
		case Wall.Right:
			fieldOrient = new Vector3 (1, 0, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.RightWall;
			fieldMin = avoidFieldMin_Right;
			msKey = "right";
			break;
		case Wall.Ceiling:
			fieldOrient = new Vector3 (0, -1, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.Ceiling;
			fieldMin = avoidFieldMin_Ceiling;
			msKey = "ceiling";
			break;
		case Wall.Floor:
			fieldOrient = new Vector3 (0, 1, 0);
			fieldMax = unityScriptAnchor.LocPerceptHokuyo.Floor;
			fieldMin = avoidFieldMin_Floor;
			msKey = "floor";
			break;
		}
		this.motorSchema.Add (msKey, 
		                      new PerpendicularExponentialIncrease (fieldOrient, fieldMax, fieldMin, 
		                                      (float)unityScriptAnchor.wallAvoidStrength, unityScriptAnchor));
	}

	public Avoid(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		//walls are tactical avoid behaviors (so are crowds, when they're in range, but that varies); add & leave
		AvoidWall (Wall.Floor);
		AvoidWall (Wall.Ceiling);
		AvoidWall (Wall.Left);
		AvoidWall (Wall.Right);
	}
	
	public override void Update()
	{
		//implement update, which should add AvoidCrowd instances as needed, then call base Update 
		int newNumCrowdTracks = 0;
		for(int i=0; i<unityScriptAnchor.CrowdPercept.SensedCrowdCount; i++)
		{
			newNumCrowdTracks++;
			if ( newNumCrowdTracks > this.currNumCrowdTracks )
			{
				childBehaviors.Add("avoidcrowd"+newNumCrowdTracks,new AvoidCrowd(this.unityScriptAnchor,newNumCrowdTracks-1));
			}
		}
		while ( newNumCrowdTracks < this.currNumCrowdTracks )
		{
			childBehaviors.Remove("avoidcrowd"+this.currNumCrowdTracks);
			this.currNumCrowdTracks--;
		}
		this.currNumCrowdTracks = newNumCrowdTracks;

		avoidFieldMin_Left.val = unityScriptAnchor.LocPerceptHokuyo.LeftWall.val - (float)unityScriptAnchor.wallAvoidDepth;
		avoidFieldMin_Right.val = unityScriptAnchor.LocPerceptHokuyo.RightWall.val + (float)unityScriptAnchor.wallAvoidDepth;
		avoidFieldMin_Ceiling.val = unityScriptAnchor.LocPerceptHokuyo.Ceiling.val - (float)unityScriptAnchor.wallAvoidDepth;
		avoidFieldMin_Floor.val = unityScriptAnchor.LocPerceptHokuyo.Floor.val + (float)unityScriptAnchor.wallAvoidDepth;

		base.Update ();

        //-------here & below, draw response vector sum debug in scene view----------
		Vector3 avoidDbgLineOffset = new Vector3(1,1,0);
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position + avoidDbgLineOffset
		                + this.motorSchema["floor"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.grey);
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["ceiling"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.grey);
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["left"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.grey);
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["right"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.grey);
		for(int i=0; i<this.currNumCrowdTracks; i++)
		{
			Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
			                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
			                + this.childBehaviors["avoidcrowd"+(i+1)].ResponseTranslate
			                *(float)this.unityScriptAnchor.simModelForce, 
			                Color.red);
		}
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                Color.magenta);
	}
}






//UAV entity and relevant information
public class BlockCrowd2 : MonoBehaviour {
	//for largely cosmetic reasons, there's no ceiling in the scene, so we stub it in here
    //these values are actually initialized in start()..these values are just to make the compiler happy
	private double hallwaySize = 7.5;
	public double HallwaySize
	{
		get { return hallwaySize; }
	}
	private double eyeLevel = 0.0;
	public double EyeLevel
	{
		get { return this.eyeLevel; }
	}
	private double aboveEyeLevel = 0.0;
	public double AboveEyeLevel
	{
		get { return aboveEyeLevel; }
	}
	private double threatenHeight;
	public double ThreatenHeight
	{
		get { return threatenHeight; }
	}

	public double simDetectCrowdRange = 5.0;
	public double approachRange = 5.0;
	public double threatenRange = 3.0;

	public double wallAvoidStrength = 0.5;
	public double wallAvoidDepth = 1.0;
	public double crowdAvoidStrength = 1.0;
	private double crowdAvoidDeadZone = 0.0; //start repulsive field at crowd surface, not center
	public double CrowdAvoidDeadZone
	{
		get { return this.crowdAvoidDeadZone; }
	}
	public double crowdAvoidDepth = 0.4;
	public double keepHeightStrength = 0.4;
	public double holdPositionStrength = 0.25;
	public double followStrength = 0.4;
	public double randThreaten2DStrength = 0.1;
	public double randThreaten2DDepth = 1.0;
    public double randThreaten2DInterval = 0.2;

	//desired movement in next dt/time interval/frame
	private Vector3 overallResponse_Translation;
	private Vector3 overallResponse_Rotation;

    //make blades spin
	private AnimationState spin;

	//we will follow suit and just move... despite appearances, movement won't be a function of tilt + engine thrust + gravity...
	private double maxFauxTilt = 45.0; //max tilt appearance


	public double simModelForce = 100.0; //multiplier for overallResponse_Translation...all field strengths are between 0 and 1--
                                            //--meaning that each motor schema transform.translate has a max value of 1
   
    //already have description of these
	private CrowdDetectionSensor crowdSensor;

	private CrowdPercept crowdPercept; 
	public CrowdPercept CrowdPercept {
		get { return crowdPercept; }
	}

	private Hokuyo hokuyo;
	private LocalizationPercept_Hokuyo locPerceptHokuyo;
	public LocalizationPercept_Hokuyo LocPerceptHokuyo {
				get { return locPerceptHokuyo;}
		}

	private Dictionary<string,UAVBehavior> behaviors = new Dictionary<string, UAVBehavior>(); //

	public BlockCrowd2()
	{
	}

	// Use this for initialization
	public void Start () {
        //these aren't changed after Start()--------
		this.crowdSensor = new CrowdDetectionSensor (this);
		crowdPercept = new CrowdPercept (this, this.crowdSensor);
		this.hokuyo = new Hokuyo (this);
		locPerceptHokuyo = new LocalizationPercept_Hokuyo (this, this.hokuyo);

		this.hallwaySize = Math.Abs(GameObject.Find ("/Wall_Right").transform.position.x - GameObject.Find ("/Wall_Left").transform.position.x) - GameObject.Find ("/Wall_Left").collider.bounds.size.x;
		this.eyeLevel = GameObject.Find ("/Black").collider.bounds.size.y - 0.15F;
		this.aboveEyeLevel = GameObject.Find ("/Black").collider.bounds.size.y + 1.0F;
		this.crowdAvoidDeadZone = ((CapsuleCollider)GameObject.Find ("/Black").collider).radius;
		this.threatenHeight = GameObject.Find ("/Black").collider.bounds.size.y - 0.5F;
        //--------------------------------------------

        //start with Avoid() because it's tactical
		this.behaviors.Add ("avoid", new Avoid (this));

        //this section makes the rotors spin realistically... it's cosmetic
		spin = animation["Spin"];
		spin.layer = 1;
		spin.blendMode = AnimationBlendMode.Additive;
		spin.wrapMode = WrapMode.Loop;
		spin.speed = 2.0F;

		GameObject blockLine = GameObject.Find ("/BlockLine");
        //start position at block line, above eye level.  (only time z is set/changed)
		transform.position = new Vector3(blockLine.transform.position.x, 
		                     //(this.blockLine.transform.position.y-transform.position.y)+(float)wallAvoidDepth, 
		                     GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Black").collider.bounds.size.y + 1.0F,
		                     blockLine.transform.position.z);
		Debug.Log ("Avoid:red; Height:blue; Followish:green; rand2d:yellow; overall:white");
	}
	
	// Update is called once per frame
	public void Update () {
        //this line makes the rotors spin realistically... it's cosmetic
		animation.CrossFade("Spin");

		this.locPerceptHokuyo.Update ();
		this.crowdPercept.Update ();

		
		//releasers; behaviors happen to be mutually exclusive (motor schema employed by them are not)
		bool approachZone = false;
		bool threatenZone = false;
		if ( crowdPercept.BlockableCrowdCount > 0 )
		{
			float distToFirstCrowd = Math.Abs(CrowdPercept.BlockableCrowds[0].val.z - locPerceptHokuyo.HokuyoLoc.val.z);
			if ( distToFirstCrowd < threatenRange )
			{
				threatenZone = true;
			}
			if ( distToFirstCrowd < approachRange )
			{
				approachZone = true;
			}
		}

		if (threatenZone) {
			if ( !this.behaviors.ContainsKey("threaten") )
			{
				this.behaviors.Remove("approach");
				this.behaviors.Remove("watch");
				this.behaviors.Add("threaten", new Threatening(this));
				Debug.Log ("threatening");
			}
		} else if (approachZone) {
			if ( !this.behaviors.ContainsKey("approach") )
			{
				this.behaviors.Remove("threaten");
				this.behaviors.Remove("watch");
				this.behaviors.Add("approach", new Approaching(this));
				Debug.Log ("approaching");
			}
		} else {
			if (!this.behaviors.ContainsKey("watch"))
			{
				this.behaviors.Remove("approach");
				this.behaviors.Remove("threaten");
				this.behaviors.Add("watch", new Watching(this));
				Debug.Log ("watching");
			}
		}

		this.overallResponse_Rotation = Vector3.zero;
		this.overallResponse_Translation = Vector3.zero;
		foreach (string behaviorKey in behaviors.Keys)
		{
            //calls update function for every behavior
			behaviors[behaviorKey].Update();
			//It's a much better sim if we actually use the physics... but then our pfields really need some damping, a PID controller or something...
			this.overallResponse_Rotation = this.overallResponse_Rotation + behaviors[behaviorKey].ResponseRotate;
			this.overallResponse_Translation = this.overallResponse_Translation + behaviors[behaviorKey].ResponseTranslate;
		}
        //draw white overall debug line
		Vector3 overallDbgLineOffset = new Vector3 (1.01F,1.01F, 0);
		Debug.DrawLine (this.transform.position + overallDbgLineOffset ,
		                this.transform.position  + overallDbgLineOffset
		                + overallResponse_Translation
		                *(float)this.simModelForce, 
		                Color.white);

		transform.Translate(this.overallResponse_Translation * (float)this.simModelForce * Time.deltaTime);
        //using force, along with a physical model of the UAV in question, would make the simulation much more accurate... but harder to stabilize/dampen without a control theory background
        //TODO: develop controller, use physics
		//this.rigidbody.AddForce (this.overallResponse_Translation * (float)this.simModelForce);

		//actually changes the transform.Translate results, so Translate is relative to object coordinate frame, not world (happens to be the same if euler angles are zero)
		//force on the rigidbody, however, doesn't have this issue...
        //TODO: re-enable once controller/physics are used; just cosmetic, though.
		//float tiltMagnitude = (float)maxFauxTilt * (float)Math.Abs(this.rigidbody.velocity.x)/3.0F;
		//tiltMagnitude = (float)Math.Min (maxFauxTilt, tiltMagnitude);
		//transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, this.rigidbody.velocity.x<0?tiltMagnitude:0.0F-tiltMagnitude);

	}
}
