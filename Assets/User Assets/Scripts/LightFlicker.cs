using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlameFlicker : MonoBehaviour
{
    public float baseIntensity = 1.2f;
    public float intensityVariation = 0.4f;
    public float flickerSpeed = 8f;

    public float baseRange = 3f;
    public float rangeVariation = 0.5f;

    Light flameLight;
    float noiseOffset;

    void Awake()
    {
        flameLight = GetComponent<Light>();
        noiseOffset = Random.Range(0f, 100f); // desync multiple torches
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);

        flameLight.intensity = baseIntensity + noise * intensityVariation;
        flameLight.range = baseRange + noise * rangeVariation;
    }
}
