using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBillboard : MonoBehaviour
{
    public Camera main;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(main.transform);

        //transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
