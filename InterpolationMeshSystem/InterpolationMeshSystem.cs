using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using WizardsRepublic.Delaunay;
using WizardsRepublic.Primitives;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace WizardsRepublic.InterpolationMeshSystem
{
  [ExecuteInEditMode]
  public class InterpolationMeshSystem : MonoBehaviour
  {
    [ShowInInspector] private Transform ProjectionTransform { get; set; }


    [ShowInInspector, ReadOnly] private Triangle[] triangles;

    [PublicAPI]
    public List<Vertex> Vertices { get; } = new List<Vertex>();

    public void Setup(Transform projectionTransform)
    {
      this.ProjectionTransform = projectionTransform;
    }

    public void Refresh()
    {
      ComputeMesh();
    }

    public TType GetInterpolatedData<TType, TData>(Transform projectionTransform, Vector3 point, int dataType) where TData : AInterpolationData
    {
      Triangle triangle = GetClosestTriangleFromProjectedPoint(projectionTransform, point);
      return (TType)triangle.GetInterpolatedData<TData>(projectionTransform, point, dataType);
    }

    private Triangle GetClosestTriangleFromProjectedPoint(Transform projectionTransform, Vector3 point)
    {
      Vector3 projectedFocus = point.GetProjectedPosition(projectionTransform);
      return triangles.OrderBy(p => Vector3.SqrMagnitude(p.Incenter - projectedFocus)).First();
    }

#if UNITY_EDITOR

    private void Update()
    {
      ComputeMesh();
    }

    [Button]
    private void ComputeMesh()
    {
      var locators = GetComponentsInChildren<InterpolationLocator>(false);
      
      Vertices.Clear();

      foreach (var loc in locators)
      {
        // 2D triangulation is done in the local space of the projection transform
        Vector3 projectionSpacePosition = ProjectionTransform.InverseTransformPoint(loc.transform.position);
        projectionSpacePosition.z = 0;
        Vertices.Add(new Vertex<InterpolationLocator>(projectionSpacePosition, loc));
      }

      // perform delaunay triangulation from arbitrary vertices
      Delaunay2D delaunay = Delaunay2D.Triangulate(Vertices);

      triangles = delaunay.Triangles.ConvertAll(t => new Triangle(t.A, t.B, t.C)).ToArray();
    }
    
#endif

    [CustomEditor(typeof(InterpolationMeshSystem))]
    public class InterpolationMeshSystemEditor : OdinEditor
    {
      private void OnSceneGUI()
      {
        var s = (InterpolationMeshSystem)target;

        if (s.triangles == null) return;
        if (s.ProjectionTransform == null) return;

        Handles.matrix = s.ProjectionTransform.localToWorldMatrix;
        
        foreach (Triangle triangle in s.triangles)
        {
          Vector3 a = triangle.A.Position;
          Vector3 b = triangle.B.Position;
          Vector3 c = triangle.C.Position;

          Handles.color = Color.white;
          Handles.DrawLine(a, b);
          Handles.DrawLine(a, c);
          Handles.DrawLine(b, c);
          
          Handles.color = new Color(1f, 1f, 1f, 0.1f);
          Handles.DrawWireDisc(triangle.Incenter, new Plane(a, b, c).normal, triangle.Radius);
        }

        foreach (Vertex vertex in s.Vertices)
        {
          Handles.color = Color.red;
          Vector3 position = ((Vertex<InterpolationLocator>)vertex).Item.transform.position;
          Vector3 projPos = s.ProjectionTransform.InverseTransformPoint(position);
          Handles.DrawLine(vertex.Position, projPos);
        }
      }
    }
  }
}