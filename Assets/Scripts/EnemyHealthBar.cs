using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image FillImage;
    public float HeightOffset = 0.8f;

    private Transform m_Target;
    private Camera m_Camera;

    public void Setup(Transform target, Camera camera)
    {
        m_Target = target;
        m_Camera = camera;
    }

    public void SetHealth(int current, int max)
    {
        if (FillImage == null)
            return;

        float ratio = max > 0 ? (float)current / max : 0f;
        FillImage.fillAmount = Mathf.Clamp01(ratio);

        if (ratio > 0.5f)
            FillImage.color = Color.green;
        else if (ratio > 0.25f)
            FillImage.color = Color.yellow;
        else
            FillImage.color = Color.red;
    }

    private void LateUpdate()
    {
        if (m_Target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = m_Target.position + new Vector3(0, HeightOffset, 0);
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        if (m_Camera != null)
            transform.rotation = m_Camera.transform.rotation;
    }
}