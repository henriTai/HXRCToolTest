using UnityEngine;
using UnityEngine.SceneManagement;

public class Breakable : MonoBehaviour
{
    private Collider m_collider = null;
    private Camera m_camera = null;
    private Vector3 m_lastPos = Vector3.zero;

    public GameObject m_breakable;
    public GameObject m_broken;
    private Rigidbody[] m_rbs;
    public ParticleSystem m_ps;

    private void Awake()
    {
        m_collider = this.gameObject.GetComponent<Collider>();
        m_camera = Camera.main;
        m_rbs = new Rigidbody[m_broken.transform.childCount];
        for (int i = 0; i < m_rbs.Length; i++)
        {
            m_rbs[i] = m_broken.transform.GetChild(i).GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            if (m_collider.Raycast(ray, out hit, float.MaxValue))
            {
                BreakNow(hit.point, 200f);
                m_lastPos = hit.point;
            }
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(m_lastPos, 0.5f);
    }

    private void BreakNow(Vector3 point, float v)
    {
        m_breakable.SetActive(false);
        m_broken.SetActive(true);
        m_ps.transform.position = point;
        m_ps.Play();
        for (int i = 0; i < m_broken.transform.childCount; i++)
        {
            m_rbs[i].AddExplosionForce(v * Random.Range(1f, 2f), point, 10f);
        }
    }
}
