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
        public static Dictionary<BlockType, Block> Prefabs = new Dictionary<BlockType, Block>();
        public static Dictionary<BlockType, Texture2D> Textures = new Dictionary<BlockType, Texture2D>();
        public static Dictionary<BlockType, BlockSounds> Sounds = new Dictionary<BlockType, BlockSounds>();

        public BlockType Type;
        public float LightLevel; // 0 to 1
        public int Hardness; // 0 to 10 (10 being unbreakable)
        public float Thickness; // 0 to 1 (Used for slowling player down)
        public int MaxStack;
        public Vector2 Position;
        public ChunkInfo ChunkInfo;
        public TerrainBiome Biome;

        public Block()
        {
            Type = BlockType.Air;
            LightLevel = 0;
            Hardness = 2;
            Thickness = 1f;
            MaxStack = 64;
            Position = Vector2.Zero;
            ChunkInfo = new ChunkInfo();
            Biome = TerrainBiome.Flatland;
        }

        private Block(BlockType blockType, Vector2 position)
        {
            Type = blockType;
            Position = position;
            ChunkInfo = new ChunkInfo();
            LightLevel = 0;
        }

        private Block(BlockType blockType, int hardness, float thickness, int maxStack)
        {
            Type = blockType;
            Hardness = hardness;
            Thickness = thickness;
            MaxStack = maxStack;
            ChunkInfo = new ChunkInfo();
            LightLevel = 0;
        }

        public static void InitializeBlockPrefabs()
        {
            // MUST BE IN BLOCKTYPE ORDER!!!
            // BlockInfo(hardness, thickness)
            Prefabs.Add(BlockType.Grass, new Block(BlockType.Grass, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Stone, new Block(BlockType.Stone, 4, 0.0f, 64));
            Prefabs.Add(BlockType.Dirt, new Block(BlockType.Dirt, 2, 0.0f, 64));
            Prefabs.Add(BlockType.Sand, new Block(BlockType.Sand, 1, 0.0f, 64));
            Prefabs.Add(BlockType.Water, new Block(BlockType.Water, 10, 0.5f, 64));
            Prefabs.Add(BlockType.Snow, new Block(BlockType.Snow, 1, 0.0f, 64));

            // Load all block textures/sounds (minus air) for later access
            var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length - 1;

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

        public static Block LoadPrefabAtPosition(BlockType blockType, Vector2 position)
        {
            var block = new Block();
            var blockPrefab = Prefabs[blockType];

            if (blockPrefab != null)
            {
                block.Type = blockType;
                block.LightLevel = blockPrefab.LightLevel;
                block.Hardness = blockPrefab.Hardness;
                block.Thickness = blockPrefab.Thickness;
                block.MaxStack = blockPrefab.MaxStack;
                block.Position = position;
                block.ChunkInfo = new ChunkInfo();
                block.Biome = TerrainBiome.Flatland;
            }
            else
                block = new Block();

            return block;
        }

        private static Texture2D loadBlockTexture(BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Grass:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/grass_top.png");
                case BlockType.Stone:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/stone.png");
                case BlockType.Dirt:
                    return Raylib.LoadTexture("Assets/Textures/Blocks/dirt_test.png");
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
                    return Raylib.LoadSound($"Assets/Sounds/Unused/error.ogg");
            }
        }
    }
}
