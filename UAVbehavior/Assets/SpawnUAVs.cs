using UnityEngine;
using System.Collections;

public class SpawnUAVs : MonoBehaviour {

    public int uavCount = 3;
    bool spawned = false;

	// Use this for initialization
    void Start()
    {
        Debug.Log("Vectors shown in scene - Avoid:red; Height:blue; Followish:green; rand2d:yellow; overall:white");
	
	}
	
	// Update is called once per frame
	void Update () {
	
        //some odd things seem to happen if you try to instantiate gameobjects during Start, so....
        if (!spawned)
        {
            for (int u = 0; u < uavCount; u++)
            {

                //where might they be placed upon startup?
                //ignoring overlap for now... (can I? what about the collider of each...)

                GameObject newUAV = (GameObject)Object.Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/SpawnedUAV.prefab", typeof(GameObject)));
                newUAV.renderer.material.color = new Color(Random.Range(0.0F, 1.0F), Random.Range(0.0F, 1.0F), Random.Range(0.0F, 1.0F), 1);
                float maxX = GameObject.Find("/Wall_Right").transform.position.x - GameObject.Find("/Wall_Right").collider.bounds.size.x / 2 - newUAV.collider.bounds.size.x / 2;
                float minX = GameObject.Find("/Wall_Left").transform.position.x + GameObject.Find("/Wall_Left").collider.bounds.size.x / 2 + newUAV.collider.bounds.size.x / 2;
                Vector3 startPos = new Vector3(Random.Range(minX, maxX), GameObject.Find("/BlockLine").transform.position.y + 1, GameObject.Find("/BlockLine").transform.position.z);

                newUAV.transform.Translate(startPos - newUAV.transform.position);
            }
            spawned = true;
        }
	}
}
