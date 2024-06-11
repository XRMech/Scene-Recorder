using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBall : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    // Start is called before the first frame update

    public void SpawnBallObject()
    {
        Instantiate(ballPrefab);
    }
}
