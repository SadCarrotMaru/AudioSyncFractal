using UnityEngine;
using UnityEngine.UI;

public class FrameRateCounter : MonoBehaviour
{
    public Text fpsText; // Assign in inspector
    public float m_update_interval = 0.5f;

    private float m_last_time;
    private float m_accum = 0.0f;
    private int m_frames = 0;
    private float m_time_left;
    private float m_result;

    void Start()
    {
        m_time_left = m_update_interval;
        m_last_time = Time.realtimeSinceStartup;
    }

    void Update()
    {
        float now = Time.realtimeSinceStartup;
        float delta = now - m_last_time;
        m_last_time = now;
        m_time_left -= delta;
        m_accum += 1.0f / delta;
        ++m_frames;

        if (m_time_left <= 0.0f)
        {
            m_result = m_accum / m_frames;
            if (fpsText != null)
                fpsText.text = m_result.ToString("f2");
            m_time_left = m_update_interval;
            m_accum = 0.0f;
            m_frames = 0;
        }
    }
}
