using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Enmity.Utils;
using FastNoise;
using Raylib_cs;

using static Enmity.Utils.GameMath;

namespace Enmity.Terrain
{
    internal class TerrainGeneration
    {
        public int Seed;

        public Block[,] ColliderCheckArray = new Block[4, 4];

        private Chunk[] renderedChunks;

        private List<Chunk> chunkBuffer = new List<Chunk>();

        private int sqrtRenderDistance;
        private FastNoiseLite noise = new FastNoiseLite();
        private Vector2 prevNearestChunkPos;

        public void Initialize(Vector2 spawnPoint)
        {
            Seed = new Random().Next(int.MaxValue);
            noise.SetSeed(Seed);

            renderedChunks = new Chunk[8];

            sqrtRenderDistance = (int)MathF.Sqrt(renderedChunks.Length);

            regenerateChunks(GetNearestChunkCoord(spawnPoint));
        }

        public void Update(Vector2 playerPos)
        {
            var nearestChunkPos = GetNearestChunkCoord(playerPos);

            // TODO: Player clips into world (but hovers when falling)
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    ColliderCheckArray[x, y] = getBlockAtPos(GetNearestBlockCoord(playerPos
                        + new Vector2(x - 1.5f, y - 1.5f)));
                }
            }

            if (nearestChunkPos.X != prevNearestChunkPos.X || nearestChunkPos.Y != prevNearestChunkPos.Y)
                regenerateChunks(nearestChunkPos);

            prevNearestChunkPos = nearestChunkPos;
        }

        // TODO: Time of day affect is only visual
        public void Draw(float skyBrightness, Vector2 playerPos)
        {
            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    Chunk currentChunk = renderedChunks[cx];

                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 256; y++)
                        {
                            var currentBlock = currentChunk.Blocks[x, y];

                            var blockDistance = Vector2.Distance(playerPos, currentBlock.Position);

                            if (blockDistance < 1024)
                            {
                                var origTextureRect = new Rectangle(0f, 0f, 16f, 16f);
                                var newTextureRect = new Rectangle(currentBlock.Position.X,
                                        currentBlock.Position.Y, 1f, 1f);

                                var lightLevel = currentBlock.LightLevel * Clamp(skyBrightness + 0.3f, 0f, 1f);
                                //lightLevel = Clamp(lightLevel, 0f, 1f);

                                //var lightLevel = currentBlock.LightLevel * 2f;
                                //lightLevel = Clamp(1f - lightLevel, 0f, 1f);

                                /*if (x == 16 && y == 128)
                                {
                                    Console.WriteLine($"Light level: {lightLevel}");
                                }*/

                                var distanceFade = MathF.Pow(Clamp((1f / blockDistance) * 58, 0f, 1f), 16f);

                                var blockColor = new Color(0, 0, 0, 0);

                                blockColor = new Color((byte)(lightLevel * 255f),
                                        (byte)(lightLevel * 255f), (byte)(lightLevel * 255f), (byte)(255f));

                                if (currentBlock.Type != BlockType.Air)
                                {
                                    if (currentBlock.Type == BlockType.Grass)
                                    {
                                        // TODO: Draw grass without using two draw calls
                                        Raylib.DrawTexturePro(Block.Textures[BlockType.Dirt],
                                            origTextureRect, newTextureRect, Vector2.Zero, 0f, blockColor);

                                        Raylib.DrawTexturePro(Block.Textures[BlockType.Grass],
                                            origTextureRect, newTextureRect, Vector2.Zero, 0f, Color.GREEN);
                                    }
                                    else
                                    {
                                        Raylib.DrawTexturePro(Block.Textures[currentBlock.Type],
                                            origTextureRect, newTextureRect, Vector2.Zero, 0f, blockColor);
                                    }
                                }
                            }
                        }
                    }

                    //UI.DrawText($"Chunk index: {cx}", 2f, new Vector2(currentChunk.Info.Position.X, 0f));
                    //var chunkDebug = new Rectangle(currentChunk.Info.Position.X, currentChunk.Info.Position.Y, 32f, 256f);
                    //Raylib.DrawRectangleLinesEx(chunkDebug, 0.25f, Color.WHITE);
                }
            }

            /*for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    var chunkDebug = new Rectangle(ColliderCheckArray[x, y].Position.X, ColliderCheckArray[x, y].Position.Y, 1f, 1f);

                    Raylib.DrawRectangleLinesEx(chunkDebug, 0.1f, Color.RED);
                }
            }*/
        }

        private void regenerateChunks(Vector2 playerChunkPos)
        {
            chunkBuffer = chunkBuffer.Distinct().ToList();

            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                var chunkPos = new Vector2(playerChunkPos.X + (cx * 32f) - (renderedChunks.Length * 16f), 0f);
                chunkPos -= new Vector2(0.5f, 0.5f); // Offset for appealing block coords

                var bufferChunk = chunkBuffer.Find(x => x == getChunkAtPos(chunkPos));

                Chunk retChunk;

                if (bufferChunk != null) // Load from previously generated buffer
                {
                    retChunk = bufferChunk;
                }
                else
                {
                    retChunk = generateChunk(chunkPos);

                    if (!retChunk.Info.Generated)
                    {
                        chunkBuffer.Add(retChunk); // Needs to only be done once when first generated
                        retChunk.Info.Generated = true;
                    }
                }

                renderedChunks[cx] = retChunk;
            }
        }

        private Chunk generateChunk(Vector2 chunkPosition)
        {
            Chunk chunk = new Chunk();
            chunk.Info.Position = chunkPosition;

            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    var currentBlock = new Block();
                    noise.SetFrequency(0.0015f);
                    noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                    noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                    noise.SetDomainWarpAmp(200.0f);
                    noise.SetFractalLacunarity(3.0f);
                    noise.SetFractalOctaves(4);

                    var hillGen = (int)(chunkPosition.Y + noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y) * 25f);

                    // TODO: Surface grass is spotty
                    /*if (y >= (hillGen + 63f) && y < (hillGen + 65f))
                    {
                        currentBlock = Block.LoadPrefabAtPosition(BlockType.Grass, new Vector2(chunkPosition.X + x,
                                chunkPosition.Y + y));
                    }*/

                    if (y >= (hillGen + 64f)) // Ground
                    {
                        noise.SetFrequency(0.015f);

                        var stoneGen = (int)(chunkPosition.Y + noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y) * 5f);
                        var blockType = new BlockType();

                        if (y > (stoneGen + 80f)) // Stone mantle
                        {
                            noise.SetFrequency(0.055f);
                            noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
                            noise.SetFractalType(FastNoiseLite.FractalType.None);
                            noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
                            noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);

                            var caveGen = noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y) * 0.55f;

                            if (caveGen < -0.5f || caveGen > 0.5f)
                                blockType = BlockType.Stone;
                            else
                            {
                                // Only gen caves if below this height
                                if ((y + chunkPosition.Y) > (stoneGen + 90f))
                                    blockType = BlockType.Air;
                                else
                                    blockType = BlockType.Stone;
                            }
                        }
                        else // Top soil
                            blockType = BlockType.Dirt;

                        if (blockType != BlockType.Air)
                        {
                            currentBlock = Block.LoadPrefabAtPosition(blockType, new Vector2(chunkPosition.X + x,
                                chunkPosition.Y + y));

                            currentBlock.LightLevel = 0f;
                        }
                    }

                    //if (chunk.Blocks[x,y] == null)
                    //    chunk.Blocks[x, y] = currentBlock;

                    currentBlock.ChunkInfo = chunk.Info;
                    chunk.Blocks[x, y] = currentBlock;
                }
            }

            generateChunkLighting(chunk);

            return chunk;
        }

        // TODO: Lighting only works for sunlight (kinda)
        private void generateChunkLighting(Chunk chunk)
        {
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    var currentBlock = chunk.Blocks[x, y];

                    var leftBlock = chunk.Blocks[Clamp(x - 1, 0, 31), 0];
                    var rightBlock = chunk.Blocks[Clamp(x + 1, 0, 31), 0];
                    var topBlock = chunk.Blocks[x, Clamp(y - 1, 0, 255)];
                    var bottomBlock = chunk.Blocks[x, Clamp(y + 1, 0, 255)];

                    if (currentBlock.Type == BlockType.Air)
                        currentBlock.LightLevel = 1f;
                    else
                    {
                        // TODO: Average all neighboring blocks together
                        if (topBlock != null)
                            currentBlock.LightLevel = Clamp(topBlock.LightLevel * 0.9f, 0f, 1f);

                        /*if (leftBlock != null)
                            currentBlock.LightLevel -= Clamp(leftBlock.LightLevel * 0.1f, 0f, 1f);

                        if (rightBlock != null)
                            currentBlock.LightLevel -= Clamp(rightBlock.LightLevel * 0.1f, 0f, 1f);

                        if (topBlock != null)
                            currentBlock.LightLevel -= Clamp(topBlock.LightLevel * 0.1f, 0f, 1f);

                        if (bottomBlock != null)
                            currentBlock.LightLevel -= Clamp(bottomBlock.LightLevel * 0.1f, 0f, 1f);*/
                    }

                    // Debug
                    /*if (x == 16 && y == 128)
                    {
                        Console.WriteLine($"Pos: ({currentBlock.Position.X}, {currentBlock.Position.Y}), ({topBlock.Position.X}, {topBlock.Position.Y})");
                        Console.WriteLine($"Light level: {currentBlock.LightLevel}");
                    }*/
                }
            }
        }

        private Chunk? getChunkAtPos(Vector2 position)
        {
            Chunk retChunk = new Chunk();
            bool foundChunk = false;

            for (int i = 0; i < chunkBuffer.Count; i++)
            {
                var chunk = chunkBuffer[i];

                if (chunk.Info.Position == position)
                {
                    foundChunk = true;
                    retChunk = chunk;
                }
            }

            if (foundChunk)
                return retChunk;
            else
                return null;
        }

        private Block getBlockAtPos(Vector2 pos)
        {
            Block retBlock = new Block(); // TODO: Will never return null

            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    Chunk currentChunk = renderedChunks[cx];

                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 256; y++)
                        {
                            Block curBlock = currentChunk.Blocks[x, y];

                            if ((curBlock.Position.X == pos.X) && (curBlock.Position.Y == pos.Y))
                            {
                                retBlock = curBlock;
                                break;
                            }
                        }
                    }
                }
            }

            return retBlock;
        }
    }
}
