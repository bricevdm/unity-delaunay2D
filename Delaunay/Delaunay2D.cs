/* Adapted from https://github.com/Bl4ckb0ne/delaunay-triangulation

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:


The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WizardsRepublic.Primitives;

namespace WizardsRepublic.Delaunay
{
  public class Delaunay2D
  {
    public class GraphEdge : Edge
    {
      public bool IsBad { get; set; }

      public GraphEdge(Vertex u, Vertex v) : base(u, v)
      {
        this.U = u;
        this.V = v;
      }

      public static bool AlmostEqual(GraphEdge left, GraphEdge right)
      {
        return AlmostEqual(left.U, right.U) && AlmostEqual(left.V, right.V)
               || AlmostEqual(left.U, right.V) && AlmostEqual(left.V, right.U);
      }

      private static bool AlmostEqual(Vertex left, Vertex right)
      {
        return Mathf.Approximately(left.Position.x, right.Position.x)
               && Mathf.Approximately(left.Position.y, right.Position.y);
      }
    }

    [PublicAPI] public List<GraphEdge> Edges { get; }
    [PublicAPI] public List<Vertex> Vertices { get; set; }
    [PublicAPI] public List<Triangle> Triangles { get; }

    private Delaunay2D()
    {
      Edges = new List<GraphEdge>();
      Triangles = new List<Triangle>();
    }

    public static Delaunay2D Triangulate(IEnumerable<Vertex> vertices)
    {
      Delaunay2D delaunay = new Delaunay2D
      {
        Vertices = new List<Vertex>(vertices)
      };

      delaunay.Triangulate();

      return delaunay;
    }

    private void Triangulate()
    {
      if (Vertices.Count <= 0)
      {
        return;
      }

      float minX = Vertices[0].Position.x;
      float minY = Vertices[0].Position.y;
      float maxX = minX;
      float maxY = minY;

      foreach (var vertex in Vertices)
      {
        if (vertex.Position.x < minX) minX = vertex.Position.x;
        if (vertex.Position.x > maxX) maxX = vertex.Position.x;
        if (vertex.Position.y < minY) minY = vertex.Position.y;
        if (vertex.Position.y > maxY) maxY = vertex.Position.y;
      }

      float dx = maxX - minX;
      float dy = maxY - minY;
      float deltaMax = Mathf.Max(dx, dy) * 2;

      Vertex p1 = new Vertex(new Vector2(minX - 1, minY - 1));
      Vertex p2 = new Vertex(new Vector2(minX - 1, maxY + deltaMax));
      Vertex p3 = new Vertex(new Vector2(maxX + deltaMax, minY - 1));

      Triangles.Add(new Triangle(p1, p2, p3));

      foreach (var vertex in Vertices)
      {
        List<GraphEdge> polygon = new List<GraphEdge>();

        foreach (var t in Triangles)
        {
          if (t.CircumCircleContains(vertex.Position))
          {
            t.IsBad = true;
            polygon.Add(new GraphEdge(t.A, t.B));
            polygon.Add(new GraphEdge(t.B, t.C));
            polygon.Add(new GraphEdge(t.C, t.A));
          }
        }

        Triangles.RemoveAll(t => t.IsBad);

        for (int i = 0; i < polygon.Count; i++)
        {
          for (int j = i + 1; j < polygon.Count; j++)
          {
            if (GraphEdge.AlmostEqual(polygon[i], polygon[j]))
            {
              polygon[i].IsBad = true;
              polygon[j].IsBad = true;
            }
          }
        }

        polygon.RemoveAll(e => e.IsBad);

        foreach (var edge in polygon)
        {
          Triangles.Add(new Triangle(edge.U, edge.V, vertex));
        }
      }

      Triangles.RemoveAll(t =>
        t.ContainsVertex(p1.Position) || t.ContainsVertex(p2.Position) || t.ContainsVertex(p3.Position));

      HashSet<GraphEdge> edgeSet = new HashSet<GraphEdge>();

      foreach (var t in Triangles)
      {
        var ab = new GraphEdge(t.A, t.B);
        var bc = new GraphEdge(t.B, t.C);
        var ca = new GraphEdge(t.C, t.A);

        if (edgeSet.Add(ab))
        {
          Edges.Add(ab);
        }

        if (edgeSet.Add(bc))
        {
          Edges.Add(bc);
        }

        if (edgeSet.Add(ca))
        {
          Edges.Add(ca);
        }
      }
    }
  }
}