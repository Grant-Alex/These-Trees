using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestSpawn : MonoBehaviour
{
    public GameObject treeForest;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i<40;i++){
             float rangeX = Random.Range(-29.0f, 29.0f);
             float rangeZ = Random.Range(3.0f, 15.0f);
             Vector3 treePlacement = new Vector3(rangeX,0.0f,rangeZ);
             Instantiate(treeForest,treePlacement,Quaternion.identity);
        }  
    }

  
}
