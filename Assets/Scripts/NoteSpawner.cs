using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject[] notes;
    public float timer;
    public float delayTimer = 1;
    // Start is called before the first frame update
    void Start()
    {
        timer = delayTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0) { 
        int noteIndex = Random.Range(0, 3);
        Vector3 notePos = new Vector3(0, 0, 0);
            GameObject newNoteObj = Instantiate(notes[noteIndex], notePos, transform.rotation);
            timer = delayTimer;
        }
    }
}
