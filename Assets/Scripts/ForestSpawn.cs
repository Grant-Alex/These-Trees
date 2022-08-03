using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestSpawn : MonoBehaviour
{
    public GameObject treeForest;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i<30;i++){
             float rangeX = Random.Range(-29.0f, 29.0f);
             float rangeY = Random.Range(-0.2f, 0.0f);
             float rangeZ = Random.Range(5.0f, 30.0f);
             Vector3 treePlacement = new Vector3(rangeX,rangeY,rangeZ);
             Instantiate(treeForest,treePlacement,Quaternion.identity);
        }  
    }

  
}
