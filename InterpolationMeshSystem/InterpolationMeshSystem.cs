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
    [ShowInInspector, SuffixLabel("Not Serialized, use Setup()")] 
    public Transform ProjectionTransform { get; private set; }

    [ShowInInspector, ReadOnly] private Triangle[] triangles;

    [PublicAPI]
    public List<Vertex> Vertices { get; } = new List<Vertex>();

    public void Setup(Transform projectionTransform)
    {
      this.ProjectionTransform = projectionTransform;
    }
    
    [PublicAPI]
    public void ComputeMesh(IEnumerable<InterpolationLocator> locators)
    {
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
    
    [PublicAPI]
    public TType GetInterpolatedData<TType, TData>(Vector3 point, int dataType) where TData : AInterpolationData
    {
      Triangle triangle = GetClosestTriangleFromProjectedPoint(point);
      return (TType)triangle.GetInterpolatedData<TData>(ProjectionTransform, point, dataType);
    }

    [PublicAPI]
    public Vector3 GetClosestPointToMesh(Vector3 point)
    {
      Triangle triangle = GetClosestTriangleFromProjectedPoint(point);
      return point.GetClosestPointToTriangle(triangle.A.Position, triangle.B.Position, triangle.C.Position, out _);
    }

    private Triangle GetClosestTriangleFromProjectedPoint(Vector3 point)
    {
      Vector3 projectedFocus = point.GetProjectedPosition(ProjectionTransform);
      return triangles.OrderBy(p => Vector3.SqrMagnitude(p.Incenter - projectedFocus)).First();
    }

    [CustomEditor(typeof(InterpolationMeshSystem))]
    public class InterpolationMeshSystemEditor : OdinEditor
    {
      private void OnSceneGUI()
      {
        var meshSystem = (InterpolationMeshSystem)target;

        DrawMeshGizmos(meshSystem);
      }

      [PublicAPI]
      public static void DrawMeshGizmos(InterpolationMeshSystem system)
      {
        if (system.triangles == null) return;
        if (system.ProjectionTransform == null) return;

        Handles.matrix = system.ProjectionTransform.localToWorldMatrix;

        foreach (Triangle triangle in system.triangles)
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

        foreach (Vertex vertex in system.Vertices)
        {
          Handles.color = Color.red;
          Vector3 position = ((Vertex<InterpolationLocator>)vertex).Item.transform.position;
          Vector3 projPos = system.ProjectionTransform.InverseTransformPoint(position);
          Handles.DrawLine(vertex.Position, projPos);
        }
      }
    }
  }
}