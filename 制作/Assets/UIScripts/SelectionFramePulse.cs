using UnityEngine;

public class SelectionFramePulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.05f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * scale;
    }
}