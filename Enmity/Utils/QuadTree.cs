using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Raylib_cs;

using Enmity.Terrain;

namespace Enmity.Utils
{
    internal class QuadTree
    {
        public const int MaxObjects = 10;
        public const int MaxLevels = 5;

        public int Level;

        private List<Block> blocks;
        private Rectangle bounds;
        private QuadTree[] nodes;

        public QuadTree(int level, Rectangle _bounds)
        {
            Level = level;
            blocks = new List<Block>();
            bounds = _bounds;
            nodes = new QuadTree[4];
        }

        // Clears the quadtree
        public void Clear()
        {
            blocks.Clear();

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
        public int GetIndex(Block block)
        {
            int index = -1;
            var verticalMidpoint = bounds.x + (bounds.width / 2);
            var horizontalMidpoint = bounds.y + (bounds.height / 2);

            var rect = new Rectangle(block.Position.X, block.Position.Y, 1f, 1f);
            
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
        public void Insert(Block block)
        {
            if (nodes[0] != null)
            {
                int index = GetIndex(block);

                if (index != -1)
                {
                    nodes[index].Insert(block);
                    return;
                }
            }

            blocks.Add(block);

            if (blocks.Count > MaxObjects && Level < MaxLevels)
            {
                if (nodes[0] == null)
                    Split();

                int i = 0;
                while (i < blocks.Count)
                {
                    int index = GetIndex(blocks[i]);

                    if (index != -1)
                    {
                        blocks.Remove(blocks[i]);
                        nodes[index].Insert(blocks[i]);
                    }
                    else
                        i++;
                }
            }
        }

        // Return all objects that could collide with the given object
        public List<Block> Retrieve(List<Block> returnList, Block block)
        {
            int index = GetIndex(block);
            if (index != -1 && nodes[0] != null)
            {
                nodes[index].Retrieve(returnList, block);
            }

            returnList.AddRange(blocks);

            return returnList;
        }
    }
}
