using UnityEngine;

public class LandscapeObject : MonoBehaviour
{
    private float speed;
    private bool movable;

    private void Update()
    {
        if (movable)
        {
            transform.Translate(speed * Time.deltaTime * Vector3.back, Space.World);
        }
    }

    public void Initialize(float speed, bool movable)
    {
        this.speed = speed;
        this.movable = movable;
    }
}