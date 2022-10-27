using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

namespace Enmity.Utils
{
    internal class QuadTree
    {
        public const int MaxObjects = 10;
        public const int MaxLevels = 5;

        public int Level;

        private List<Rectangle> objects;
        private Rectangle bounds;
        private QuadTree[] nodes;

        public QuadTree(int level, Rectangle pBounds)
        {
            Level = level;
            objects = new List<Rectangle>();
            bounds = pBounds;
            nodes = new QuadTree[4];
        }

        // Clears the quadtree
        public void Clear()
        {
            objects.Clear();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null)
                {
                    nodes[i].Clear();
                    nodes[i] = null; // Not sure about this
                }
            }
        }

        // Splits the node into 4 subnodes
        public void Split()
        {
            var subWidth = (int)(bounds.width / 2);
            var subHeight = (int)(bounds.height / 2);
            var x = (int)bounds.x;
            var y = (int)bounds.y;

            nodes[0] = new QuadTree(Level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
            nodes[1] = new QuadTree(Level + 1, new Rectangle(x, y, subWidth, subHeight));
            nodes[2] = new QuadTree(Level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
            nodes[3] = new QuadTree(Level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
        }

        // Determine which node the object belongs to. -1 means
        // object cannot completely fit within a child node and is part of the parent node
        public int GetIndex(Rectangle rect)
        {
            int index = -1;
            var verticalMidpoint = bounds.x + (bounds.width / 2);
            var horizontalMidpoint = bounds.y + (bounds.height / 2);

            // Object can completely fit within the top quadrants
            bool topQuadrant = (rect.y < horizontalMidpoint && rect.y + rect.height < horizontalMidpoint);
            // Object can completely fit within the bottom quadrants
            bool bottomQuadrant = (rect.y > horizontalMidpoint);

            // Object can completely fit within the left quadrants
            if (rect.x < verticalMidpoint && rect.x + rect.width < verticalMidpoint)
            {
                if (topQuadrant)
                    index = 1;
                else if (bottomQuadrant)
                    index = 2;
            }
            else if (rect.x > verticalMidpoint) // Object can completely fit within the right quadrants
            {
                if (topQuadrant)
                    index = 0;
                else if (bottomQuadrant)
                    index = 3;
            }

            return index;
        }

        // Insert the object into the quadtree. If the node
        // exceeds the capacity, it will split and add all
        // objects to their corresponding nodes.
        public void Insert(Rectangle rect)
        {
            if (nodes[0] != null)
            {
                int index = GetIndex(rect);

                if (index != -1)
                {
                    nodes[index].Insert(rect);

                    return;
                }
            }

            objects.Add(rect);

            if (objects.Count > MaxObjects && Level < MaxLevels)
            {
                if (nodes[0] == null)
                    Split();

                int i = 0;
                while (i < objects.Count)
                {
                    int index = GetIndex(objects[i]);

                    if (index != -1)
                    {
                        objects.Remove(objects[i]);
                        nodes[index].Insert(objects[i]);
                    }
                    else
                        i++;
                }
            }
        }

        // Return all objects that could collide with the given object
        public List<Rectangle> Retrieve(List<Rectangle> returnList, Rectangle rect)
        {
            int index = GetIndex(rect);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].Retrieve(returnList, rect);
            }

            returnList.AddRange(objects);

            return returnList;
        }
    }
}
