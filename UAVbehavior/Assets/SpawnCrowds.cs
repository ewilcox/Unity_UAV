using UnityEngine;
using System.Collections;

public class SpawnCrowds : MonoBehaviour {

    public int crowdsPerSpawn = 5;
    public double spawnIntervalSeconds = 5;
    private System.DateTime lastSpawn;
    public int pass = 0;
    public int fail = 0;


	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if ((System.DateTime.Now - lastSpawn).TotalSeconds > spawnIntervalSeconds)
        {
            DoSpawn();
        }
#if DEBUG
#else
        TextMesh statusTM = GameObject.Find("StatusLabel").GetComponent(typeof(TextMesh)) as TextMesh;
        statusTM.text = pass + " / " + (pass + fail) + "(" + ((double)pass*100 / (double)(pass + fail)).ToString("F2") + "%)";
#endif
	}

    void DoSpawn()
    {
        for (int c = 0; c < crowdsPerSpawn; c++)
        {
            GameObject newCrowd = (GameObject)Object.Instantiate( UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/SpawnedCrowd.prefab", typeof(GameObject)));
            newCrowd.renderer.material.color = new Color(Random.Range(0.0F, 1.0F), Random.Range(0.0F, 1.0F), Random.Range(0.0F, 1.0F), 1);

        }
        lastSpawn = System.DateTime.Now;
    }
}
