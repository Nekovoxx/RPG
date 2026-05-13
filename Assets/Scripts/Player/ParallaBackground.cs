using UnityEngine;

public class ParallaBackground : MonoBehaviour
{
    private GameObject cam;

    [SerializeField] private float ParallaxEffect;

    private float xPosition;
    private float length;
    void Start()
    {
        cam = GameObject.Find("Main Camera");

        length = GetComponent<SpriteRenderer>().bounds.size.x;
        xPosition = transform.position.x;
    }


    void Update()
    {
        float distamceMoved = cam.transform.position.x * (1 - ParallaxEffect);
        float distanceToMove = cam.transform.position.x;

        transform.position = new Vector3(xPosition + distanceToMove, transform.position.y);

        if (distamceMoved > xPosition + length)
            xPosition = xPosition + length;
        else if (distamceMoved < xPosition - length)
            xPosition = xPosition - length;
    }
}
