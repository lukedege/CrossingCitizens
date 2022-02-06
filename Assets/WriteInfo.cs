using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WriteInfo : MonoBehaviour
{
    public Transform other;
    public Collider crossing;

    // Start is called before the first frame update
    void Start()
    {
        crossing = GameObject.FindGameObjectWithTag("Crossing").transform.GetChild(0).GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 fw = Vector3.Scale((crossing.transform.position - other.position), crossing.transform.right).normalized;

        float b = Vector3.Dot(fw, other.position - transform.position);
        Debug.Log(b);
    }

    private void OnDrawGizmos()
    {

    }
}
