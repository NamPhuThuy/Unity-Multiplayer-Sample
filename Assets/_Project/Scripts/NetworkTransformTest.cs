using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkTransformTest : NetworkBehaviour
{

    private float _yValue;
    private void Start()
    {
        _yValue = Random.Range(-4f, 4f);
    }

    void Update()
    {
        if (IsServer)
        {
            float theta = Time.frameCount / 10.0f;
            transform.position = new Vector3((float) Math.Cos(theta), _yValue, (float) Math.Sin(theta));
        }
    }
}