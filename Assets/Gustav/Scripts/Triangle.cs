using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle
{
    public Vector2 pointA, pointB, pointC;
    public readonly Vector2[] vertices = new Vector2[3];

    public readonly Edge edgeAB, edgeBC, edgeCA;
    public readonly Edge[] edges = new Edge[3];

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        pointA = a;
        pointB = b;
        pointC = c;

        vertices[0] = pointA;
        vertices[1] = pointB;
        vertices[2] = pointC;

        edgeAB = new Edge(a, b);
        edgeBC = new Edge(b, c);
        edgeCA = new Edge(c, a);

        edges[0] = edgeAB;
        edges[1] = edgeBC;
        edges[2] = edgeCA;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return vertices.Contains(point);
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public bool PointInTriangle(Vector2 pt)
    {
        Vector2 v1 = pointA;
        Vector2 v2 = pointB;
        Vector2 v3 = pointC;

        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, v1, v2);
        d2 = Sign(pt, v2, v3);
        d3 = Sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    #region Circum circle related methods
    public Vector2 CircumCenter()
    {
        List<double> a = new()
        {
            pointA.x,
            pointA.y
        };
        List<double> b = new()
        {
            pointB.x,
            pointB.y
        };
        List<double> c = new()
        {
            pointC.x,
            pointC.y
        };

        List<double> center = FindCircumCenter(a, b, c);

        return new Vector2((float)center[0], (float)center[1]);
    }

    public static void LineFromPoints(List<double> P, List<double> Q, ref double a, ref double b, ref double c)
    {
        a = Q[1] - P[1];
        b = P[0] - Q[0];
        c = a * (P[0]) + b * (P[1]);
    }

    // Function which converts the input line to its
    // perpendicular bisector. It also inputs the points
    // whose mid-point lies on the bisector
    public static void PerpendicularBisectorFromLine(List<double> P, List<double> Q, ref double a, ref double b, ref double c)
    {
        List<double> mid_point = new List<double>();
        mid_point.Add((P[0] + Q[0]) / 2);

        mid_point.Add((P[1] + Q[1]) / 2);

        // c = -bx + ay
        c = -b * (mid_point[0]) + a * (mid_point[1]);

        double temp = a;
        a = -b;
        b = temp;
    }

    // Returns the intersection point of two lines
    public static List<double> LineLineIntersection(double a1, double b1, double c1, double a2, double b2, double c2)
    {
        List<double> ans = new List<double>();
        double determinant = a1 * b2 - a2 * b1;
        if (determinant == 0)
        {
            // The lines are parallel. This is simplified
            // by returning a pair of FLT_MAX
            ans.Add(double.MaxValue);
            ans.Add(double.MaxValue);
        }

        else
        {
            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;
            ans.Add(x);
            ans.Add(y);
        }

        return ans;
    }

    public static List<double> FindCircumCenter(List<double> P, List<double> Q, List<double> R)
    {
        // Line PQ is represented as ax + by = c
        double a = 0;
        double b = 0;
        double c = 0;
        LineFromPoints(P, Q, ref a, ref b, ref c);

        // Line QR is represented as ex + fy = g
        double e = 0;
        double f = 0;
        double g = 0;
        LineFromPoints(Q, R, ref e, ref f, ref g);

        // Converting lines PQ and QR to perpendicular
        // vbisectors. After this, L = ax + by = c
        // M = ex + fy = g
        PerpendicularBisectorFromLine(P, Q, ref a, ref b, ref c);
        PerpendicularBisectorFromLine(Q, R, ref e, ref f, ref g);

        // The point of intersection of L and M gives
        // the circumcenter
        List<double> circumcenter = LineLineIntersection(a, b, c, e, f, g);

        return circumcenter;
    }
    #endregion
}
