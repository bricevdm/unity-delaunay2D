using System;
using Sirenix.OdinInspector;
using UnityEngine;
using WizardsRepublic.Primitives;

namespace WizardsRepublic.InterpolationMeshSystem
{
  internal class Triangle : WizardsRepublic.Primitives.Triangle
  {
    [ShowInInspector] private InterpolationLocator vertexA, vertexB, vertexC;

    public Triangle(Vertex a, Vertex b, Vertex c) : base(a, b, c)
    {
      vertexA = ((Vertex<InterpolationLocator>)a).Item;
      vertexB = ((Vertex<InterpolationLocator>)b).Item;
      vertexC = ((Vertex<InterpolationLocator>)c).Item;
      
      ComputeCircumcircle();
    }

    public Vector3 Incenter { get; private set; }
    public float Radius { get; private set; }

    private void ComputeCircumcircle()
    {
      Incenter = GetTriangleCircumcenter(A.Position, B.Position, C.Position, out float radius);
      Radius = radius;
    }

    public object GetInterpolatedData<TDataType>(Transform projectionTransform, Vector3 point, int dataType) where TDataType : AInterpolationData
    {
      // every computation is in projection space, the local space of the projectionTransform
      
      Vector3 projPoint = projectionTransform.InverseTransformPoint(point);
      
      Vector3 projA = A.Position ;
      Vector3 projB = B.Position ;
      Vector3 projC = C.Position ;

      Vector3 coordinates = projPoint.GetBarycentricCoordinates(projA, projB, projC);

      AInterpolationData aData = vertexA.GetData<TDataType>(dataType);
      AInterpolationData bData = vertexB.GetData<TDataType>(dataType);
      AInterpolationData cData = vertexC.GetData<TDataType>(dataType);

      if (typeof(TDataType) == typeof(ColorData))
      {
        return ColorData.Interpolate(coordinates, (ColorData)aData, (ColorData)bData, (ColorData)cData);
      }

      if (typeof(TDataType) == typeof(FloatData))
      {
        return FloatData.Interpolate(coordinates, (FloatData)aData, (FloatData)bData, (FloatData)cData);
      }

      if (typeof(TDataType) == typeof(Vector2Data))
      {
        return Vector2Data.Interpolate(coordinates, (Vector2Data)aData, (Vector2Data)bData, (Vector2Data)cData);
      }

      if (typeof(TDataType) == typeof(Vector3Data))
      {
        return Vector3Data.Interpolate(coordinates, (Vector3Data)aData, (Vector3Data)bData, (Vector3Data)cData);
      }

      throw new Exception();
    }
  }
}