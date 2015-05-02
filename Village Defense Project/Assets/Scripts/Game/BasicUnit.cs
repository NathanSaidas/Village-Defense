using UnityEngine;
using System.Collections;

namespace Gem
{
    public class BasicUnit : MonoBehaviour
    {
        [SerializeField]
        private NavMeshAgent m_Agent = null;

        private bool m_Click;
        private Vector3 m_ClickPoint = Vector3.zero;

        private NavMeshPath m_Path = null;
        private NavMeshPath m_TestPath = null;
        // Use this for initialization
        void Start()
        {
            m_Path = new NavMeshPath();
            m_TestPath = new NavMeshPath();
        }

        // Update is called once per frame
        void Update()
        {
            if (m_TestPath.status == NavMeshPathStatus.PathPartial)
            {
                Debug.Log("Calculating path");
            }



            if(Input.GetMouseButtonDown(0))
            {
                m_Click = true;
                m_ClickPoint = Input.mousePosition;

                if (m_TestPath.status == NavMeshPathStatus.PathComplete)
                {
                    Debug.Log("Completed path calculation.");
                    m_Path.GetCornersNonAlloc(m_TestPath.corners);
                    m_Agent.SetPath(m_Path);
                }
                else
                {
                    Debug.Log("Path not yet complete or unreachable");
                    m_Agent.Stop();
                }
                

            }
            if(Input.GetMouseButtonDown(1))
            {
                int layerMask = 1 << LayerMask.NameToLayer("Ground");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray,out hit, 50.0f, layerMask))
                {
                    m_Agent.CalculatePath(hit.point, m_TestPath);
                }
            }

            
        }

        void FixedUpdate()
        {
            if(m_Click)
            {
                //m_Click = false;
                //int layerMask = 1 << LayerMask.NameToLayer("Ground");
                //Ray ray = Camera.main.ScreenPointToRay(m_ClickPoint);
                //RaycastHit hit;
                //if(Physics.Raycast(ray,out hit, 50.0f,layerMask))
                //{
                //    //if(m_Agent.CalculatePath(hit.point,m_Path))
                //    //{
                //    //    m_Agent.path = m_Path;
                //    //}
                //    //else
                //    //{
                //    //    m_Agent.Stop();
                //    //}
                //
                //    m_Agent.CalculatePath(hit.point, m_Path);
                //    if(m_Path.status != NavMeshPathStatus.PathInvalid)
                //    {
                //        m_Agent.SetPath(m_Path);
                //    }
                //    else
                //    {
                //        m_Agent.Stop();
                //        m_Agent.SetPath(null);
                //    }
                //}
            }
        }
    }

}


