using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Enmity.Terrain
{
    public enum BlockType
    {
        Air,
        Grass,
        Stone,
        Dirt,
        Sand,
        Water,
        Snow
    }

    public enum TerrainBiome
    {
        Flatland,
        Plains,
        Ocean,
        Beach,
        Mountains,
        Forest,
        Snowlands
    }

    public class ChunkInfo
    {
        public bool Generated;
        public bool Modified;
        public Vector2 Position;
    }

    public class Chunk
    {
        public ChunkInfo Info;
        public Block[,] Blocks = new Block[32, 256];

        public Chunk()
        {
            Info = new ChunkInfo();
        }
    }

    public class BlockInfo
    {
        public BlockType Type;
        public TerrainBiome Biome;
        public int Hardness; // 0 to 10 (10 being unbreakable)
        public float Thickness; // 0 to 1 (Used for slowling player down)
        public int MaxStack;

        public BlockInfo() { }

        public BlockInfo(BlockType type, int hardness, float thickness, int maxStack)
        {
            Type = type;
            Hardness = hardness;
            Thickness = thickness;
            MaxStack = maxStack;
        }
    }

    public class BlockSounds
    {
        public Sound[] Sounds = new Sound[4];

        /// <summary>
        /// Get random sound from array
        /// </summary>
        public Sound RND
        {
            get { return Sounds[new Random().Next(0, 3)]; }
        }
    }

    public class Block
    {
        public static Dictionary<BlockType, BlockInfo> Prefabs = new Dictionary<BlockType, BlockInfo>();
        public static Dictionary<BlockType, Texture2D> Textures = new Dictionary<BlockType, Texture2D>();
        public static Dictionary<BlockType, BlockSounds> Sounds = new Dictionary<BlockType, BlockSounds>();

        public bool IsWall;
        public bool IsHorizon;
        public Vector2 Position;
        public BlockInfo Info;
        public ChunkInfo ChunkInfo;
        public float LightLevel; // 0 to 1

        public Block()
        {
            IsWall = false;
            Position = Vector2.Zero;
            Info = new BlockInfo(BlockType.Air, 2, 0.0f, 64);
            ChunkInfo = new ChunkInfo();
            LightLevel = 1.0f;
        }

        public Block(bool isWall, Vector2 position, BlockInfo info)
        {
            IsWall = isWall;
            Position = position;
            Info = info;
            ChunkInfo = new ChunkInfo();
            LightLevel = 1.0f;
        }

        public static void InitializeBlockPrefabs()
        {
            // MUST BE IN BLOCKTYPE ORDER!!!
            // BlockInfo(hardness, thickness)
            Prefabs.Add(BlockType.Grass, new BlockInfo(BlockType.Grass, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Stone, new BlockInfo(BlockType.Stone, 4, 0.0f, 64));
            Prefabs.Add(BlockType.Dirt, new BlockInfo(BlockType.Dirt, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Sand, new BlockInfo(BlockType.Sand, 1, 0.0f, 64));
            Prefabs.Add(BlockType.Water, new BlockInfo(BlockType.Water, 10, 0.5f, 64));
            Prefabs.Add(BlockType.Snow, new BlockInfo(BlockType.Snow, 1, 0.0f, 64));

            // Load all block textures for later access
            var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

            for (int i = 0; i < blockTypeCount; i++)
            {
                Textures.Add((BlockType)i, loadBlockTexture((BlockType)i));

                var blockSounds = new BlockSounds();

                for (int ii = 0; ii < 4; ii++)
                {
                    Console.WriteLine(ii);
                    blockSounds.Sounds[ii] = loadBlockSounds((BlockType)i, ii);
                }


                Sounds.Add((BlockType)i, blockSounds);
            }
        }

        private static Texture2D loadBlockTexture(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Grass:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/grass.png");
                case BlockType.Stone:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/stone.png");
                case BlockType.Dirt:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/dirt.png");
                case BlockType.Sand:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/sand.png");
                case BlockType.Water:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/water.png");
                case BlockType.Snow:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/snow.png");
                default:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/error.png");
            }
        }

        private static Sound loadBlockSounds(BlockType blockType, int index)
        {
            var i = index + 1;

            switch (blockType)
            {
                case BlockType.Grass:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Dirt/dirt{i}.ogg");
                case BlockType.Stone:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Stone/stone{i}.ogg");
                case BlockType.Dirt:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Dirt/dirt{i}.ogg");
                case BlockType.Sand:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Sand/sand{i}.ogg");
                case BlockType.Snow:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Snow/snow{i}.ogg");
                default:
                    return Raylib.LoadSound($"Assets/Sounds/Blocks/Stone/stone{i}.ogg");
            }
        }
    }
}
