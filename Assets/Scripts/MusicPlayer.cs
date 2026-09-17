using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer m_Instance;

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}