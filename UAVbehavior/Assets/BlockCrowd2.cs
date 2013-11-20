using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Vector3RefWrap
{
	public Vector3 val;
	public Vector3RefWrap(Vector3 _initVal)
	{
		this.val = _initVal;
	}
}

//simulated generic crowd detection percept
public class CrowdPercept
{
	private List<GameObject> simCrowds = new List<GameObject>();
	List<Vector3RefWrap> blockableCrowds = new List<Vector3RefWrap>();
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

	public CrowdPercept (BlockCrowd2 _unityScriptAnchor)
	{
		this.unityScriptAnchor = _unityScriptAnchor;
		blockableCrowds.Clear ();
		for(int i=0; i<10; i++)
		{
			blockableCrowds.Add(new Vector3RefWrap(Vector3.zero));
			sensedCrowds.Add(new Vector3RefWrap(Vector3.zero));
		}
	}
	public void Init()
	{
		simCrowds.Add (GameObject.Find ("/Red"));
		simCrowds.Add (GameObject.Find ("/Yellow"));
		simCrowds.Add (GameObject.Find ("/Blue"));
		simCrowds.Add (GameObject.Find ("/Black"));
	}

	public void Update()
	{
		SortedList<float,Vector3> sortCrowds = new SortedList<float, Vector3> ();
		List<Vector3> detectCrowds = new List<Vector3> ();
		//use simDetectCrowdRange to simulate data being available or not; return those within range.
		foreach (GameObject crowd in simCrowds) {
			if (Math.Abs (crowd.transform.position.z - GameObject.Find ("/BlockLine").transform.position.z) <= this.unityScriptAnchor.simDetectCrowdRange)
			{
				//must be in front of block line
				if (crowd.transform.position.z < GameObject.Find ("/BlockLine").transform.position.z) 
				{
					sortCrowds.Add(Math.Abs (crowd.transform.position.z - GameObject.Find ("/BlockLine").transform.position.z), crowd.transform.position);
				}
				detectCrowds.Add(crowd.transform.position);
			}
		}
		if (blockableCrowdCount != sortCrowds.Count)
			Debug.Log ("blockable: " + sortCrowds.Count);
		blockableCrowdCount = sortCrowds.Count;
		for(int i=0; i<this.blockableCrowds.Count; i++)
		{
			if ( i < sortCrowds.Count )
				blockableCrowds[i].val = sortCrowds[sortCrowds.Keys[i]];
			else
			{
				blockableCrowds[i].val.x = float.MaxValue;
				blockableCrowds[i].val.y = float.MaxValue;
				blockableCrowds[i].val.z = float.MaxValue;
			}
		}
		if (sensedCrowdCount != detectCrowds.Count)
						Debug.Log ("detected: " + detectCrowds.Count);
		sensedCrowdCount = detectCrowds.Count;
		for(int i=0; i<this.sensedCrowds.Count; i++)
		{
			if ( i < detectCrowds.Count )
				sensedCrowds[i].val = detectCrowds[i];
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

//simulated Hokuyo sensor percept implementation
public class LocalizationPercept_Hokuyo
{
	private BlockCrowd2 unityScriptAnchor;
	private Vector3RefWrap uavLoc = new Vector3RefWrap(Vector3.zero);
	public Vector3RefWrap UAVLocation
	{
		get { return uavLoc; }
	}
	public LocalizationPercept_Hokuyo(BlockCrowd2 _unityScriptAnchor)
	{
		this.unityScriptAnchor = _unityScriptAnchor;
	}

	public void Update()
	{
		//TODO: expand on this, from ground-truth to a simple simulation of the hokuyo planar laser ranger
		uavLoc.val = unityScriptAnchor.transform.position;
	}
}

public abstract class UAVMotorSchema
{
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

	public abstract void Update();
}

//perpendicular field, perhaps from wall, exponential, strongest at one coordinate, weakest (1%) at the "min" coordinate, absent elsewhere
//currently only supports axis-aligned fields for orientation
public class PerpendicularExponentialIncrease : UAVMotorSchema
{
	protected Vector3 fieldOrient;
	protected float maxFieldCoord, minFieldCoord;
	protected float peakFieldStrength;
	protected Vector3RefWrap uavLocation;

	//give it a unit vector away from the 'wall'; max is the surface of the wall in x/y coordinate, it will have full strength at max, none at min.
	//simple calc presumes the fieldOrient is axis-aligned, only one dimension for the field
	public PerpendicularExponentialIncrease (Vector3 _fieldOrient, float _max, float _min, float _peakFieldStrength, Vector3RefWrap _uavLocation, BlockCrowd2 _unityScriptAnchor) : base (_unityScriptAnchor)
	{
		fieldOrient = _fieldOrient;
		maxFieldCoord = _max;
		minFieldCoord = _min;
		peakFieldStrength = _peakFieldStrength;
		uavLocation = _uavLocation;
	}

	public override void Update()
	{
		this.responseRotate = Vector3.zero;
		this.responseTranslate = Vector3.zero;
		float uavFieldCoord = (fieldOrient.x != 0) ? uavLocation.val.x : uavLocation.val.y;
		bool fieldNeg = minFieldCoord > maxFieldCoord; //i.e. field orients negative on axis

		//if not in range, response is zero
		if ( fieldNeg?( uavFieldCoord < minFieldCoord && uavFieldCoord >= maxFieldCoord )
		    : (uavFieldCoord > minFieldCoord && uavFieldCoord <= maxFieldCoord) )
		{
			//blockCrowd2.wallAvoidStrength * (100^-x) * fieldOrient, where x is [0,1] between min and max
			//exponentially increasing as wall is approached, starts at 1% strength at bound
			float posInScale = Math.Abs (uavFieldCoord - maxFieldCoord);
			float fieldLen = Math.Abs(maxFieldCoord - minFieldCoord);
			float x = posInScale / fieldLen;
			float responseScale = 
				(float)(peakFieldStrength * Math.Pow (100,0-x));
			this.responseTranslate.x = this.fieldOrient.x * responseScale;
			this.responseTranslate.y = this.fieldOrient.y * responseScale;
			//if ( fieldOrient.y == -1 )
			//Debug.Log(fieldOrient.x + "," + fieldOrient.y + "," + fieldOrient.z + ":"
			        //+"["+minFieldCoord+","+maxFieldCoord+"](@"+posInScale+"); x="+x+",f(x)="+responseScale);
		}
	}
}

public class AttractiveExponentialDecrease : UAVMotorSchema
{
	private Vector3RefWrap position;
	private float capDistance;
	private float peakFieldStrength;
	private bool enableX, enableY, enableZ;
	protected Vector3RefWrap uavLocation;
	
	public AttractiveExponentialDecrease(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _peakFieldStrength,
	                                     float _capDistance, bool _enableX, bool _enableY, bool _enableZ,
	                                    Vector3RefWrap _uavLocation) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
		this.peakFieldStrength = _peakFieldStrength;
		this.enableX = _enableX;
		this.enableY = _enableY;
		this.enableZ = _enableZ;
		this.uavLocation = _uavLocation;
	}
	public override void Update()
	{
		this.responseRotate = Vector3.zero;

		this.responseTranslate = Vector3.zero;
		Vector3 fieldVecOrient = position.val - uavLocation.val;
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

public class RepulsiveExponentialIncrease : UAVMotorSchema
{
	private Vector3RefWrap position;
	private float capDistance;
	private float peakFieldStrength;
	private bool enableX, enableY, enableZ;
	//for crowd avoid, we need full force at the surface of the cylinder/collider; exponential decreasing thereafter.
	//for that, we need to define a distance which gets subtracted out when determining the strength of the field
	private float deadZoneAroundPosition;
	protected Vector3RefWrap uavLocation;
	
	public RepulsiveExponentialIncrease(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _peakFieldStrength,
	                                    float _capDistance, bool _enableX, bool _enableY, bool _enableZ, 
	                                    float _deadZoneAroundPosition, Vector3RefWrap _uavLocation) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
		this.peakFieldStrength = _peakFieldStrength;
		this.enableX = _enableX;
		this.enableY = _enableY;
		this.enableZ = _enableZ;
		this.deadZoneAroundPosition = _deadZoneAroundPosition;
		this.uavLocation = _uavLocation;
	}
	public override void Update()
	{
		this.responseRotate = Vector3.zero;

		this.responseTranslate = Vector3.zero;
		Vector3 fieldVecOrient = uavLocation.val - position.val;
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

public class Rand2D : UAVMotorSchema
{
	private float changeTime = 0.0F;
	private Vector3 currRandMove = Vector3.zero;
	private Vector3RefWrap position;
	private float capDistance;
	protected Vector3RefWrap uavLocation;
	
	public Rand2D(BlockCrowd2 _unityScriptAnchor, Vector3RefWrap _position, float _capDistance, Vector3RefWrap _uavLocation ) : base(_unityScriptAnchor)
	{
		this.position = _position;
		this.capDistance = _capDistance;
		this.uavLocation = _uavLocation;
	}
	public override void Update()
	{
		Vector3 distFromFacingCrowd = uavLocation.val - position.val;
		//note infinite field range, exponential force proprotional to distance from desired position
		//decay reaches 1% at capDistance, but strictly never reaches zero (always applies something)
		float x = distFromFacingCrowd.magnitude / capDistance;
		float responseScale = (float)Math.Pow (100, 0.0F-x);


		if (Time.time > this.changeTime) {
			UnityEngine.Random.seed = (int)System.DateTime.Now.Ticks;
			this.currRandMove.x = (float)this.unityScriptAnchor.randThreaten2DStrength * (2*UnityEngine.Random.value-1.0F);
			this.currRandMove.y = (float)this.unityScriptAnchor.randThreaten2DStrength * (2*UnityEngine.Random.value-1.0F);
			this.changeTime = Time.time + 0.2F;//UnityEngine.Random.value;
		}
		this.responseTranslate = this.currRandMove * responseScale;
	}
}

public abstract class UAVBehavior
{
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
			if (childBehaviors[behKey].ResponseTranslate.magnitude > 1.0)
				Debug.Log ("excess force: " + behKey + ": " + childBehaviors[behKey].ResponseTranslate.x + "," + childBehaviors[behKey].ResponseTranslate.y + "," + childBehaviors[behKey].ResponseTranslate.z);
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

public class KeepHeight : UAVBehavior
{
	private float height;
	public float Height
	{
		get{ return height; }
		set { height = value;}
	}

	public void KeepHeightField(Wall _wall)
	{
		Vector3 fieldOrient = Vector3.zero;
		float fieldMax = 0.0F, fieldMin = 0.0F;
		string msKey = "?";
		switch (_wall) {
		case Wall.Ceiling:
			fieldOrient = new Vector3 (0, -1, 0);
			fieldMax = GameObject.Find ("/Floor").transform.position.y + (float)unityScriptAnchor.ceilingHeight;
			fieldMin = height;
			msKey = "ceiling";
			break;
		case Wall.Floor:
			fieldOrient = new Vector3 (0, 1, 0);
			fieldMax = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Floor").collider.bounds.size.y / 2;
			fieldMin = height;
			msKey = "floor";
			break;
		}
		this.motorSchema.Add (msKey, new PerpendicularExponentialIncrease (fieldOrient, fieldMax, fieldMin, (float)unityScriptAnchor.keepHeightStrength, unityScriptAnchor.LocPerceptHokuyo.UAVLocation, unityScriptAnchor));
	}
	
	public KeepHeight(BlockCrowd2 _unityScriptAnchor, float _height) : base(_unityScriptAnchor)
	{
		this.height = _height;
		KeepHeightField (Wall.Ceiling);
		KeepHeightField (Wall.Floor);
	}
}

//hold a horizontal position, used for hallway centering while "watching"
public class HoldCenter : UAVBehavior
{
	private Vector3RefWrap midPoint;
	public HoldCenter(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		float width = (GameObject.Find ("/Wall_Left").transform.position - GameObject.Find ("/Wall_Right").transform.position).magnitude;
		float aboveEyeLevel = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y + 1.0F;
		midPoint = new Vector3RefWrap( new Vector3 (
			GameObject.Find ("/Wall_Left").transform.position.x + (width/2),
			aboveEyeLevel,
			GameObject.Find ("/BlockLine").transform.position.z
			));
		this.motorSchema.Add ("ms", new AttractiveExponentialDecrease (_unityScriptAnchor, midPoint, 
		                                                     (float)_unityScriptAnchor.holdPositionStrength, width, true, false, false,
		                                                     _unityScriptAnchor.LocPerceptHokuyo.UAVLocation)
				);
	}
}

//follow a crowd--only in x-dimension, still.  Conflicts with eye-level field, otherwise, since our percept is *center* of crowd (i.e. belly button...)
public class Follow : UAVBehavior
{
	private Vector3RefWrap crowdProjectOnPlane;
	
	public Follow(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		this.crowdProjectOnPlane = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x,
			_unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y,
			GameObject.Find ("/BlockLine").transform.position.z));
		
		float width = (GameObject.Find ("/Wall_Left").transform.position - GameObject.Find ("/Wall_Right").transform.position).magnitude;
		this.motorSchema.Add ("ms", new AttractiveExponentialDecrease (_unityScriptAnchor, crowdProjectOnPlane, 
		                                                     (float)_unityScriptAnchor.followStrength,
		                                                     (float)Math.Min (width, _unityScriptAnchor.ceilingHeight) * 0.70F, 
		                                                     true, false, false,
		                                                     _unityScriptAnchor.LocPerceptHokuyo.UAVLocation)
				);
	}
	
	public override void Update ()
	{
		this.crowdProjectOnPlane.val.x = unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x;
		this.crowdProjectOnPlane.val.y = unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y;
		
		base.Update ();
	}
}

public class AvoidCrowd : UAVBehavior
{	
	private Vector3RefWrap crowdUAVLevel;
	private int nthCrowd;//avoid this crowd, in order of z-distance, so we can avoid running into any of them... perhaps closest 1 would usually do, but if they're side by side...
	
	public AvoidCrowd(BlockCrowd2 _unityScriptAnchor, int _nthCrowd ) : base(_unityScriptAnchor)
	{
		this.nthCrowd = _nthCrowd;
		this.crowdUAVLevel = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.SensedCrowds[_nthCrowd].val.x,
			_unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y,
			_unityScriptAnchor.CrowdPercept.SensedCrowds[_nthCrowd].val.z));
		this.motorSchema.Add ("ms", new RepulsiveExponentialIncrease (_unityScriptAnchor, crowdUAVLevel,
		                                                     (float)_unityScriptAnchor.crowdAvoidStrength, 
		                                                     (float)_unityScriptAnchor.crowdAvoidDepth, 
		                                                     true, false, false,
		                                                     ((CapsuleCollider)GameObject.Find ("/Black").collider).radius,
		                                                     _unityScriptAnchor.LocPerceptHokuyo.UAVLocation)
				);
	}
	
	public override void Update ()
	{
		this.crowdUAVLevel.val.x = unityScriptAnchor.CrowdPercept.SensedCrowds[this.nthCrowd].val.x;
		this.crowdUAVLevel.val.y = unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y;
		this.crowdUAVLevel.val.z = unityScriptAnchor.CrowdPercept.SensedCrowds[this.nthCrowd].val.z;
		
		base.Update ();
	}
}

public class ThreateningRand2D : UAVBehavior
{
	private Vector3RefWrap crowdProjectOnPlane;
	public ThreateningRand2D(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		this.crowdProjectOnPlane = new Vector3RefWrap (new Vector3 (
			_unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x,
			//_unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y,
			GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y - 0.5F,
			GameObject.Find ("/BlockLine").transform.position.z));
		this.motorSchema.Add ("ms", new Rand2D (_unityScriptAnchor, this.crowdProjectOnPlane, (float)_unityScriptAnchor.randThreaten2DDepth,
		                               _unityScriptAnchor.LocPerceptHokuyo.UAVLocation)
				);
	}
	public override void Update ()
	{
		this.crowdProjectOnPlane.val.x = unityScriptAnchor.CrowdPercept.NthCrowd (0).val.x;
		//this.crowdProjectOnPlane.val.y = unityScriptAnchor.LocPerceptHokuyo.UAVLocation.val.y;
		base.Update ();
	}
}

public class Watching : UAVBehavior
{
	public static Vector3 keepheightDbgLineOffset = new Vector3(1, 1, 0);
	public static Vector3 followishDbgLineOffset = new Vector3(1, 1, 0);
	public Watching(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		float aboveEyeLevel = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y + 1.0F;
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, aboveEyeLevel));
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

public class Approaching : UAVBehavior
{
	public Approaching(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		float eyeLevel = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y - 0.15F;
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, eyeLevel));
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

public class Threatening : UAVBehavior
{
	public Threatening(BlockCrowd2 _unityScriptAnchor) : base(_unityScriptAnchor)
	{
		float eyeLevel = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y - 0.5F;
		childBehaviors.Add ("keepheight", new KeepHeight(_unityScriptAnchor, eyeLevel));
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

public class Avoid : UAVBehavior
{
	protected int currNumCrowdTracks = 0;

	public void AvoidWall(Wall _wall)
	{
		Vector3 fieldOrient = Vector3.zero;
		float fieldMax = 0.0F, fieldMin = 0.0F;
		string msKey = "?";
		switch (_wall) {
		case Wall.Left:
			fieldOrient = new Vector3 (-1, 0, 0);
			fieldMax = GameObject.Find ("/Wall_Right").transform.position.x - GameObject.Find ("/Wall_Left").collider.bounds.size.x / 2;
			fieldMin = fieldMax - (float)unityScriptAnchor.wallAvoidDepth;
			msKey = "left";
			break;
		case Wall.Right:
			fieldOrient = new Vector3 (1, 0, 0);
			fieldMax = GameObject.Find ("/Wall_Left").transform.position.x + GameObject.Find ("/Wall_Right").collider.bounds.size.x / 2;
			fieldMin = fieldMax + (float)unityScriptAnchor.wallAvoidDepth;
			msKey = "right";
			break;
		case Wall.Ceiling:
			fieldOrient = new Vector3 (0, -1, 0);
			fieldMax = GameObject.Find ("/Floor").transform.position.y + (float)unityScriptAnchor.ceilingHeight;
			fieldMin = fieldMax - (float)unityScriptAnchor.wallAvoidDepth;
			msKey = "ceiling";
			break;
		case Wall.Floor:
			fieldOrient = new Vector3 (0, 1, 0);
			fieldMax = GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Floor").collider.bounds.size.y / 2;
			fieldMin = fieldMax + (float)unityScriptAnchor.wallAvoidDepth;
			msKey = "floor";
			break;
		}
		this.motorSchema.Add (msKey, new PerpendicularExponentialIncrease (fieldOrient, fieldMax, fieldMin, 
		                                                                   (float)unityScriptAnchor.wallAvoidStrength, unityScriptAnchor.LocPerceptHokuyo.UAVLocation, unityScriptAnchor));
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
				Debug.Log ("add crowd avoid: avoidcrowd"+newNumCrowdTracks);
				childBehaviors.Add("avoidcrowd"+newNumCrowdTracks,new AvoidCrowd(this.unityScriptAnchor,newNumCrowdTracks-1));
			}
		}
		while ( newNumCrowdTracks < this.currNumCrowdTracks )
		{
			Debug.Log ("remove crowd avoid: avoidcrowd"+this.currNumCrowdTracks);
			childBehaviors.Remove("avoidcrowd"+this.currNumCrowdTracks);
			this.currNumCrowdTracks--;
		}
		this.currNumCrowdTracks = newNumCrowdTracks;
		base.Update ();
		Vector3 avoidDbgLineOffset = new Vector3(1,1,0);
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position + avoidDbgLineOffset
		                + this.motorSchema["floor"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                new Color(255,127,0));
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["ceiling"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                new Color(255,127,0));
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["left"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                new Color(255,127,0));
		Debug.DrawLine (this.unityScriptAnchor.transform.position + avoidDbgLineOffset, 
		                this.unityScriptAnchor.transform.position  + avoidDbgLineOffset
		                + this.motorSchema["right"].ResponseTranslate
		                *(float)this.unityScriptAnchor.simModelForce, 
		                new Color(255,127,0));
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

public class BlockCrowd2 : MonoBehaviour {
	//for largely cosmetic reasons, there's no ceiling in the scene, so we stub it in here
	public double ceilingHeight = 7.0;

	public double simDetectCrowdRange = 5.0;
	public double approachRange = 5.0;
	public double threatenRange = 3.0;

	public double wallAvoidStrength = 0.5;
	public double wallAvoidDepth = 1.0;
	public double crowdAvoidStrength = 1.0;
	public double crowdAvoidDepth = 0.2;
	public double keepHeightStrength = 0.4;
	public double holdPositionStrength = 0.25;
	public double followStrength = 0.4;
	public double randThreaten2DStrength = 0.04;
	public double randThreaten2DDepth = 1.0;

	//desired movement in next dt/time interval/frame
	private Vector3 overallResponse_Translation;
	private Vector3 overallResponse_Rotation;

	private AnimationState spin;

	//we will follow suit and just move... despite appearances, movement won't be a function of tilt + engine thrust + gravity...
	public double maxFauxTilt = 45.0;
	public double simModelForce = 100.0;

	private CrowdPercept crowdPercept;
	public CrowdPercept CrowdPercept {
		get { return crowdPercept; }
	}
	private LocalizationPercept_Hokuyo locPerceptHokuyo;
	public LocalizationPercept_Hokuyo LocPerceptHokuyo {
				get { return locPerceptHokuyo;}
		}
	private GameObject blockLine;

	private Dictionary<string,UAVBehavior> behaviors = new Dictionary<string, UAVBehavior>();

	public BlockCrowd2()
	{
		crowdPercept = new CrowdPercept (this);
		locPerceptHokuyo = new LocalizationPercept_Hokuyo (this);
	}

	// Use this for initialization
	void Start () {
		crowdPercept.Init();
		this.blockLine = GameObject.Find ("/BlockLine");
		this.behaviors.Add ("avoid", new Avoid (this));

		spin = animation["Spin"];
		spin.layer = 1;
		spin.blendMode = AnimationBlendMode.Additive;
		spin.wrapMode = WrapMode.Loop;
		spin.speed = 2.0F;

		transform.position = new Vector3(this.blockLine.transform.position.x, 
		                     //(this.blockLine.transform.position.y-transform.position.y)+(float)wallAvoidDepth, 
		                     GameObject.Find ("/Floor").transform.position.y + GameObject.Find ("/Red").collider.bounds.size.y + 1.0F,
		                     this.blockLine.transform.position.z);
		Debug.Log ("Avoid:red; Height:blue; Followish:green; rand2d:yellow; overall:white");
	}
	
	// Update is called once per frame
	void Update () {
		//we would have an imu, most likely, or some other means of maintaining the planar assumption of this system; without it, we'll just use
		//a simple fix to keep the UAV in that plane.
		//this.transform.Translate (0, 0, this.blockLine.transform.position.z - this.transform.position.z);
		animation.CrossFade("Spin");
		//this is essentially the deliberative layer update.  Maybe have some fun by slowing it down, relative to reactive behaviors? Not much reason to justify in this app, though.

		//tactical behaviors;
		//CrowdLocations(), assign Avoid to each
		//make a static one for each wall, run Update() here

		this.locPerceptHokuyo.Update ();
		this.crowdPercept.Update ();

		
		//releasers; behaviors happen to be mutually exclusive (motor schema employed by them do not)
		bool approachZone = false;
		bool threatenZone = false;
		for(int i=0; i<crowdPercept.BlockableCrowdCount; i++)
		{
			Vector3RefWrap crowd = crowdPercept.BlockableCrowds[i];
			if ( Math.Abs(crowd.val.z-blockLine.transform.position.z) < threatenRange )
			{
				threatenZone = true;
			}
			if ( Math.Abs(crowd.val.z-blockLine.transform.position.z) < approachRange )
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
			behaviors[behaviorKey].Update();
			if (behaviors[behaviorKey].ResponseTranslate.magnitude > 1.0)
				Debug.Log ("excess force: " + behaviorKey + ": " + behaviors[behaviorKey].ResponseTranslate.x + "," + behaviors[behaviorKey].ResponseTranslate.y + "," + behaviors[behaviorKey].ResponseTranslate.z);
			//It's a much better sim if we actually use the physics... but then our pfields really need some damping, a PID controller or something...
			this.overallResponse_Rotation = this.overallResponse_Rotation + behaviors[behaviorKey].ResponseRotate;
			this.overallResponse_Translation = this.overallResponse_Translation + behaviors[behaviorKey].ResponseTranslate;
		}
		if (this.overallResponse_Translation.magnitude > 1.0)
			Debug.Log ("excess force: (overall), check your forces (should be [0.0, 1.0]): " + overallResponse_Translation.x + "," + overallResponse_Translation.y + "," + overallResponse_Translation.z);
		
		Vector3 overallDbgLineOffset = new Vector3 (1.01F,1.01F, 0);
		Debug.DrawLine (this.transform.position + overallDbgLineOffset ,
		                this.transform.position  + overallDbgLineOffset
		                + overallResponse_Translation
		                *(float)this.simModelForce, 
		                Color.white);

		transform.Translate(this.overallResponse_Translation * (float)this.simModelForce * Time.deltaTime);
		//TODO: make it so...
		//this.rigidbody.AddForce (this.overallResponse_Translation * (float)this.simModelForce);


		//actually changes the transform.Translate results, so Translate is relative to object coordinate frame, not world (happens to be the same if euler angles are zero)
		//force on the rigidbody, however, doesn't have this issue...
		//float tiltMagnitude = (float)maxFauxTilt * (float)Math.Abs(this.rigidbody.velocity.x)/3.0F;
		//tiltMagnitude = (float)Math.Min (maxFauxTilt, tiltMagnitude);
		//transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, this.rigidbody.velocity.x<0?tiltMagnitude:0.0F-tiltMagnitude);

	}
}
