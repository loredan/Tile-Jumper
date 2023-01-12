using System.Collections.Generic;
using UnityEngine;

public class DCEL
{
    private List<Site> _sites;
    private List<Face> _faces;
    private List<Vertex> _verticies;
    private List<HalfEdge> _halfEdges;

    public class Site
    {
        public Vector2Int point;

        public Site(Vector2Int point)
        {
            this.point = point;
        }
    }

    public class Vertex
    {
        public Vector2 point;
    }

    public class HalfEdge
    {
        public Vertex origin;
        public Vertex destination;
        public HalfEdge twin;
        public Face incidentFace;
        public HalfEdge next;
        public HalfEdge prev;
    }

    public class Face
    {
        public Site site;
        public HalfEdge outerComponent;
    }

    public HalfEdge createHalfEdge(Face face) {
        HalfEdge halfEdge = new HalfEdge();
        halfEdge.incidentFace = face;
        if (face.outerComponent == null) {
            face.outerComponent = halfEdge;
        }
        _halfEdges.Add(halfEdge);
        return halfEdge;
    }
}
