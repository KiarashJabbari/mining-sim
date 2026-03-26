using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothSpeed = 8f;
    public Vector2 offset = Vector2.zero;

    private Transform target;
    private Camera cam;
    private CaveGenerator world;

    void Start()
    {
        cam = GetComponent<Camera>();
        world = FindObjectOfType<CaveGenerator>();
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) target = p.transform;
        else Invoke("FindPlayer", 0.1f);
    }

    void LateUpdate()
    {
        if (target == null) { FindPlayer(); return; }

        Vector3 desired = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);

        if (world != null && cam != null)
        {
            float halfH = cam.orthographicSize;
            desired.y = Mathf.Clamp(desired.y, halfH, world.worldHeight - halfH);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}