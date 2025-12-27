using UnityEngine;

public class LandscapeObject : MonoBehaviour
{
    private float speed;

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.back, Space.World);
    }

    public void Initialize(float moveSpeed)
    {
        speed = moveSpeed;
    }
}