using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class Fortune
{
    private DCEL _diagram;
    private BeachLine _beachLine;
    private List<Vector2Int> _sites;
    private SortedList<float, Event> _events = new SortedList<float, Event>();

    private float sweep;

    public Fortune(List<Vector2Int> sites)
    {
        _sites = sites;
        foreach (Vector2Int site in sites)
        {
            _events.Add(site.y, new SiteEvent(site));
        }

        while (_events.Count > 0)
        {
            Event e = _events[0];
            sweep = e.y;
            e.ProcessEvent(_events, _beachLine);
        }
    }

    private void ProcessSiteEvent(SiteEvent siteEvent)
    {
        if (beachLine.IsEmpty())
        {
            beachLine.SetRoot(new BeachLine.Arc(_site));
            return;
        }

        BeachLine.Arc arcToSplit = beachLine.LocateArcAbove(_site.point, y);
        if (arcToSplit.circleEvent != null)
        {
            events.Remove(arcToSplit.circleEvent.y);
            arcToSplit.circleEvent = null;
        }

        BeachLine.Arc middleArc = SplitArc(beachLine, arcToSplit);
        BeachLine.Arc leftArc = middleArc.prev;
        BeachLine.Arc rightArc = middleArc.next;

        leftArc.rightHalfEdge = diagram.createHalfEdge(leftArc.site.face);
        middleArc.leftHalfEdge = diagram.createHalfEdge(middleArc.site.face);
        leftArc.rightHalfEdge.twin = middleArc.leftHalfEdge;
        middleArc.leftHalfEdge.twin = leftArc.rightHalfEdge;
        middleArc.rightHalfEdge = middleArc.leftHalfEdge;
        rightArc.leftHalfEdge = leftArc.rightHalfEdge;

        if (leftArc.prev != beachLine.nil)
        {

        }
    }

    public abstract class Event
    {
        public float y;

        public abstract void ProcessEvent(SortedList<float, Event> events, BeachLine beachLine, DCEL diagram);
    }

    public class SiteEvent : Event
    {
        DCEL.Site _site;
        public SiteEvent(Vector2Int site)
        {
            _site = new DCEL.Site(site);
            y = site.y;
        }

        public override void ProcessEvent(SortedList<float, Event> events, BeachLine beachLine, DCEL diagram)
        {
            if (beachLine.IsEmpty())
            {
                beachLine.SetRoot(new BeachLine.Arc(_site));
                return;
            }

            BeachLine.Arc arcToSplit = beachLine.LocateArcAbove(_site.point, y);
            if (arcToSplit.circleEvent != null)
            {
                events.Remove(arcToSplit.circleEvent.y);
                arcToSplit.circleEvent = null;
            }

            BeachLine.Arc middleArc = SplitArc(beachLine, arcToSplit);
            BeachLine.Arc leftArc = middleArc.prev;
            BeachLine.Arc rightArc = middleArc.next;

            leftArc.rightHalfEdge = diagram.createHalfEdge(leftArc.site.face);
            middleArc.leftHalfEdge = diagram.createHalfEdge(middleArc.site.face);
            leftArc.rightHalfEdge.twin = middleArc.leftHalfEdge;
            middleArc.leftHalfEdge.twin = leftArc.rightHalfEdge;
            middleArc.rightHalfEdge = middleArc.leftHalfEdge;
            rightArc.leftHalfEdge = leftArc.rightHalfEdge;

            if (leftArc.prev != beachLine.nil)
            {

            }
        }

        private BeachLine.Arc SplitArc(BeachLine beachLine, BeachLine.Arc arc)
        {
            BeachLine.Arc middleArc = new BeachLine.Arc(_site);
            BeachLine.Arc leftArc = new BeachLine.Arc(arc.site);
            leftArc.leftHalfEdge = arc.leftHalfEdge;
            BeachLine.Arc rightArc = new BeachLine.Arc(arc.site);
            rightArc.rightHalfEdge = arc.rightHalfEdge;

            beachLine.Replace(arc, middleArc);
            beachLine.InsertBefore(middleArc, leftArc);
            beachLine.InsertAfter(middleArc, rightArc);

            return middleArc;
        }
    }

    public class CircleEvent : Event
    {
        public Vector2 center;
        public BeachLine.Arc arc;
        public CircleEvent(BeachLine.Arc leftArc, BeachLine.Arc middleArc, BeachLine.Arc rightArc)
        {
            Vector2Int leftSite = leftArc.site.point;
            Vector2Int middleSite = middleArc.site.point;
            Vector2Int rightSite = rightArc.site.point;

            center = new Vector2(
                -0.5f *
                (leftSite.y * (middleSite.x * middleSite.x + middleSite.y * middleSite.y - rightSite.x * rightSite.x - rightSite.y * rightSite.y) +
                middleSite.y * (rightSite.x * rightSite.x + rightSite.y * rightSite.y - leftSite.x * leftSite.x - leftSite.y * leftSite.y) +
                rightSite.y * (leftSite.x * leftSite.x + leftSite.y * leftSite.y - middleSite.x * middleSite.x - middleSite.y * middleSite.y)) /
                (leftSite.x * (middleSite.y - rightSite.y) + middleSite.x * (rightSite.y - leftSite.y) + rightSite.x * (leftSite.y - middleSite.y)),
                0.5f *
                (leftSite.x * (middleSite.x * middleSite.x + middleSite.y * middleSite.y - rightSite.x * rightSite.x - rightSite.y * rightSite.y) +
                middleSite.x * (rightSite.x * rightSite.x + rightSite.y * rightSite.y - leftSite.x * leftSite.x - leftSite.y * leftSite.y) +
                rightSite.x * (leftSite.x * leftSite.x + leftSite.y * leftSite.y - middleSite.x * middleSite.x - middleSite.y * middleSite.y)) /
                (leftSite.x * (middleSite.y - rightSite.y) + middleSite.x * (rightSite.y - leftSite.y) + rightSite.x * (leftSite.y - middleSite.y))
            );

            y = center.y - (leftSite - center).magnitude;
            arc = middleArc;
        }

        public override void ProcessEvent(SortedList<float, Event> events, BeachLine beachLine)
        {

        }
    }
}
