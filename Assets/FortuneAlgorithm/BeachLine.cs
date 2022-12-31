using System;
using System.Collections.Generic;
using UnityEngine;

public class BeachLine
{
    private Arc _root;

    private Arc _nil = new Arc(null); // Sentinel node (CLRS 4th ed, ch 10.4)
    public class Arc
    {
        public enum Color { RED, BLACK };

        public Arc(DCEL.Site site)
        {
            this.site = site;
        }

        public Arc parent;
        public Arc left;
        public Arc right;

        public DCEL.Site site;
        public DCEL.HalfEdge leftHalfEdge;
        public DCEL.HalfEdge rightHalfEdge;

        public Fortune.CircleEvent circleEvent;

        public Arc prev;
        public Arc next;

        public Color color;
    }

    public Arc GetLeftmostArc()
    {
        Arc cursor = _root;
        while (cursor.left != _nil)
        {
            cursor = cursor.left;
        }
        return cursor;
    }

    public Arc LocateArcAbove(Vector2Int point, double sweep)
    {
        Arc cursor = _root;
        bool found = false;
        while (!found)
        {
            double breakpointLeft = Double.NegativeInfinity;
            double breakpointRight = Double.PositiveInfinity;
            if (cursor.prev != _nil)
            {
                breakpointLeft = ComputeBreakpoint(cursor.prev.site.point, cursor.site.point, sweep);
            }
            if (cursor.next != _nil)
            {
                breakpointRight = ComputeBreakpoint(cursor.site.point, cursor.next.site.point, sweep);
            }
            if (point.x < breakpointLeft)
            {
                cursor = cursor.left;
            }
            else if (point.x > breakpointRight)
            {
                cursor = cursor.right;
            }
            else
            {
                found = true;
            }
        }
        return cursor;
    }

    public void SetRoot(Arc arc)
    {
        arc.parent = _nil;
        arc.left = _nil;
        arc.right = _nil;
        arc.prev = _nil;
        arc.next = _nil;
        _root = arc;
    }

    public bool IsEmpty()
    {
        return _root == null;
    }

    private double ComputeBreakpoint(Vector2 focus1, Vector2 focus2, double l)
    {
        double x1 = focus1.x, y1 = focus1.y, x2 = focus2.x, y2 = focus2.y;
        double d1 = 1.0 / (2.0 * (y1 - l));
        double d2 = 1.0 / (2.0 * (y2 - l));
        double a = d1 - d2;
        double b = 2.0 * (x2 * d2 - x1 * d1);
        double c = (y1 * y1 + x1 * x1 - l * l) * d1 - (y2 * y2 + x2 * x2 - l * l) * d2;
        double delta = b * b - 4.0 * a * c;
        return (-b + Math.Sqrt(delta)) / (2.0 * a);
    }

    public void InsertBefore(Arc node, Arc arc)
    {
        if (node.left == _nil)
        {
            node.left = arc;
            arc.parent = node;
        }
        else
        {
            node.prev.right = arc;
            arc.parent = node.prev;
        }

        arc.left = _nil;
        arc.right = _nil;

        arc.prev = node.prev;
        arc.prev.next = arc;
        arc.next = node;
        node.prev = arc;

        arc.color = Arc.Color.RED;
        InsertBalance(arc);
    }

    public void InsertAfter(Arc node, Arc arc)
    {
        if (node.right == _nil)
        {
            node.right = arc;
            arc.parent = node;
        }
        else
        {
            node.next.left = arc;
            arc.parent = node.next;
        }

        arc.left = _nil;
        arc.right = _nil;

        arc.next = node.next;
        arc.next.prev = arc;
        arc.prev = node;
        node.next = arc;

        arc.color = Arc.Color.RED;
        InsertBalance(arc);
    }

    private void InsertBalance(Arc node)
    {
        while (node.parent.color == Arc.Color.RED)
        {
            if (node.parent == node.parent.parent.left)
            {
                Arc uncle = node.parent.parent.right;

                if (uncle.color == Arc.Color.RED)
                {
                    node.parent.color = Arc.Color.BLACK;
                    uncle.color = Arc.Color.BLACK;
                    node.parent.parent.color = Arc.Color.RED;
                    node = node.parent.parent;
                }
                else
                {
                    if (node == node.parent.right)
                    {
                        node = node.parent;
                        LeftRotate(node);
                    }

                    node.parent.color = Arc.Color.BLACK;
                    node.parent.parent.color = Arc.Color.RED;
                    RightRotate(node.parent.parent);
                }
            }
            else
            {
                Arc uncle = node.parent.parent.left;

                if (uncle.color == Arc.Color.RED)
                {
                    node.parent.color = Arc.Color.BLACK;
                    uncle.color = Arc.Color.BLACK;
                    node.parent.parent.color = Arc.Color.RED;
                    node = node.parent.parent;
                }
                else
                {
                    if (node == node.parent.right)
                    {
                        node = node.parent;
                        RightRotate(node);
                    }

                    node.parent.color = Arc.Color.BLACK;
                    node.parent.parent.color = Arc.Color.RED;
                    LeftRotate(node.parent.parent);
                }
            }
        }

        _root.color = Arc.Color.BLACK;
    }

    public void Replace(Arc arc, Arc with)
    {
        Transplant(arc, with);
        
        with.left = arc.left;
        with.left.parent = with;
        with.right = arc.right;
        with.right.parent = with;
        
        with.prev = arc.prev;
        with.prev.next = with;
        with.next = arc.next;
        with.next.prev = with;

        with.color = arc.color;
    }

    void Remove(Arc node)
    {
        Arc y = node;
        Arc.Color yOriginalColor = node.color;

        Arc x;
        if (node.left == null)
        {
            x = node.right;
            Transplant(node, node.right);
        }
        else if (node.right == null)
        {
            x = node.left;
            Transplant(node, node.left);
        }
        else
        {
            y = node.next;
            yOriginalColor = y.color;

            x = y.right;
            if (y != node.right)
            {
                Transplant(y, y.right);
                y.right = node.right;
                y.right.parent = y;
            }
            else
            {
                x.parent = y;
            }

            Transplant(node, y);
            y.left = node.left;
            y.left.parent = y;
            y.color = node.color;
        }

        node.prev.next = node.next;
        node.next.prev = node.prev;

        if (yOriginalColor == Arc.Color.BLACK)
        {
            DeleteBalance(x);
        }
    }

    private void DeleteBalance(Arc node)
    {
        while (node != _root && node.color == Arc.Color.BLACK)
        {
            if (node == node.parent.left)
            {
                Arc sibling = node.parent.right;

                if (sibling.color == Arc.Color.RED)
                {
                    sibling.color = Arc.Color.BLACK;
                    node.parent.color = Arc.Color.RED;
                    LeftRotate(node.parent);
                    sibling = node.parent;
                }

                if (sibling.left.color == Arc.Color.BLACK && sibling.right.color == Arc.Color.BLACK)
                {
                    sibling.color = Arc.Color.RED;
                    node = node.parent;
                }
                else
                {
                    if (sibling.right.color == Arc.Color.BLACK)
                    {
                        sibling.left.color = Arc.Color.BLACK;
                        sibling.color = Arc.Color.RED;
                        RightRotate(sibling);
                        sibling = node.parent.right;
                    }

                    sibling.color = node.parent.color;
                    node.parent.color = Arc.Color.BLACK;
                    sibling.right.color = Arc.Color.BLACK;
                    LeftRotate(node.parent);
                    node = _root;
                }
            }
            else
            {
                Arc sibling = node.parent.left;

                if (sibling.color == Arc.Color.RED)
                {
                    sibling.color = Arc.Color.BLACK;
                    node.parent.color = Arc.Color.RED;
                    RightRotate(node.parent);
                    sibling = node.parent;
                }

                if (sibling.left.color == Arc.Color.BLACK && sibling.right.color == Arc.Color.BLACK)
                {
                    sibling.color = Arc.Color.RED;
                    node = node.parent;
                }
                else
                {
                    if (sibling.left.color == Arc.Color.BLACK)
                    {
                        sibling.right.color = Arc.Color.BLACK;
                        sibling.color = Arc.Color.RED;
                        LeftRotate(sibling);
                        sibling = node.parent.left;
                    }

                    sibling.color = node.parent.color;
                    node.parent.color = Arc.Color.BLACK;
                    sibling.left.color = Arc.Color.BLACK;
                    RightRotate(node.parent);
                    node = _root;
                }
            }
        }

        node.color = Arc.Color.BLACK;
    }

    void LeftRotate(Arc leftNode)
    {
        Arc rightNode = leftNode.right;

        leftNode.right = rightNode.left;
        if (leftNode.right != null)
        {
            leftNode.right.parent = leftNode;
        }

        rightNode.parent = leftNode.parent;
        if (rightNode.parent == null)
        {
            _root = rightNode;
        }
        else if (rightNode.parent.left == leftNode)
        {
            rightNode.parent.left = rightNode;
        }
        else
        {
            rightNode.parent.right = rightNode;
        }

        rightNode.left = leftNode;
        leftNode.parent = rightNode;
    }

    void RightRotate(Arc rightNode)
    {
        Arc leftNode = rightNode.left;

        rightNode.left = leftNode.right;
        if (rightNode.left != null)
        {
            rightNode.left.parent = rightNode;
        }

        leftNode.parent = rightNode.parent;
        if (leftNode.parent == null)
        {
            _root = leftNode;
        }
        else if (leftNode.parent.left == rightNode)
        {
            leftNode.parent.left = leftNode;
        }
        else
        {
            leftNode.parent.right = leftNode;
        }

        leftNode.right = rightNode;
        rightNode.parent = leftNode;
    }

    void Transplant(Arc into, Arc node)
    {
        if (into.parent == null)
        {
            _root = node;
        }
        else if (into.parent.left == into)
        {
            into.parent.left = node;
        }
        else
        {
            into.parent.right = node;
        }
        node.parent = into.parent;
    }
}
