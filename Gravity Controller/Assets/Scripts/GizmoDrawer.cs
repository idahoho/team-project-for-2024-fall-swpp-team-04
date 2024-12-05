using UnityEngine;

[ExecuteInEditMode] 
public class GizmoDrawer : MonoBehaviour
{
    public Color gizmoColor = Color.red;
    public float gizmoSize = 0.5f;       

    private void OnDrawGizmos()
    {
        
        Gizmos.color = gizmoColor;

        
        Gizmos.DrawSphere(transform.position, gizmoSize);
    }
}