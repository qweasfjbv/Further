using UnityEngine;

namespace Further
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(LineRenderer))]
    public class PolygonController : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth;
        [SerializeField, Range(3, 100)] private int polygonPoints;
        [SerializeField] private float radius;


        [SerializeField] private Vector2 basePosition;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer> ();       
        }

        // Update is called once per frame
        void Update()
        {
            Play();
        }


        private void Play()
        {
            lineRenderer.material = lineMaterial;
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = polygonPoints + 2;

            float anglePerStep = 2 * Mathf.PI * (1f / polygonPoints);

            for(int i=0; i<=polygonPoints+1; i++)
            {
                float angle = anglePerStep * i;
                lineRenderer.SetPosition(i, basePosition + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)));
            }

        }
    }

}