using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SphereController : MonoBehaviour
{
    HomeManager homeManager;

    private float vecX;
    private float vecY;
    private float vecZ;
    private Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        homeManager = GameObject.FindWithTag("HomeManager").GetComponent<HomeManager>();
        StartCoroutine("RandomizeTarget");
    }

    // Update is called once per frame
    void Update()
    {
        float _speed = homeManager.speed;
        transform.position = Vector3.MoveTowards(transform.position, target, _speed);
    }

    private IEnumerator RandomizeTarget()
    {
        while (true)
        {
            float _dist = homeManager.dist;
            vecX = Random.Range(-1.0f * _dist, _dist);
            vecY = Random.Range(-0.5f, 0.5f);
            vecZ = Random.Range(-1.0f * _dist, _dist);
            target = new Vector3(vecX, vecY, vecZ);
            Debug.Log(target);

            yield return new WaitForSeconds(2.0f);
        }
    }
}
