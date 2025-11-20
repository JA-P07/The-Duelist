using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    //public KeyCode keyCode;
    public float speed;
    public bool hasBeenHit = false;
    public bool isActive = true;
    public float targetTime;
    public float travelTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0, -1, 0) * speed * Time.deltaTime);

        if (!isActive)
        {
            //Destroy(this);
        }
    }
}
