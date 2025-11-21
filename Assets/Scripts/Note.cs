using UnityEngine;

public class Note : MonoBehaviour
{
    public int directionIndex;
    public float scheduledHitTime;
    public NoteResult result = NoteResult.Unjudged;

    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime;
    private float spawnTime;

    public void Initialize(int direction, Vector3 hitPos, float t)
    {
        directionIndex = direction;
        travelTime = t;
        spawnTime = Time.time;

        startPos = transform.position;
        endPos = hitPos;

        // Optional: offset by lane
        float offset = (directionIndex - 1.5f) * 0.7f;
        startPos += new Vector3(offset, 0, 0);
        endPos += new Vector3(offset, 0, 0);

        transform.position = startPos;

        scheduledHitTime = spawnTime + travelTime;
    }

    private void Update()
    {
        float lerp = (Time.time - spawnTime) / travelTime;
        transform.position = Vector3.Lerp(startPos, endPos, lerp);
    }
}
