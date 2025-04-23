using UnityEngine;
using System.Collections;

public class MovingLog : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 2f;
    public float waitTime = 1f;

    private Vector2 target;
    private bool isWaiting = false;

    void Start()
    {
        target = pointB;
    }

    void Update()
    {
        if (isWaiting) return;

        transform.localPosition = Vector2.MoveTowards(transform.localPosition, target, speed * Time.deltaTime);

        if (Vector2.Distance(transform.localPosition, target) < 0.01f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        target = (target == pointA) ? pointB : pointA;
        isWaiting = false;
    }
}
