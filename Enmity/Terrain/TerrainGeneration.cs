using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using FastNoise;
using Raylib_cs;

using static Enmity.Utils.GameMath;

// TODO: Generate blocks beneath surface
// TODO: Grass block at 0,0
namespace Enmity.Terrain
{
    internal class TerrainGeneration
    {
        public int Seed;

        private Chunk[] renderedChunks;

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

            if (nearestChunkPos.X != prevNearestChunkPos.X || nearestChunkPos.Y != prevNearestChunkPos.Y)
                regenerateChunks(nearestChunkPos);

            prevNearestChunkPos = nearestChunkPos;
        }

        public void Draw(Vector2 playerPos)
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

                            if (blockDistance < 64)
                            {
                                var origTextureRect = new Rectangle(0f, 0f, 8f, 8f);
                                var newTextureRect = new Rectangle(currentBlock.Position.X,
                                        currentBlock.Position.Y, 1f, 1f);

                                //var lightLevel = currentBlock.LightLevel * 2.0f;
                                var lightLevel = 1f;

                                var distanceFade = MathF.Pow(Clamp((1f / blockDistance) * 58, 0f, 1f), 16f);

                                var blockColor = new Color(0, 0, 0, 0);

                                blockColor = new Color((byte)(lightLevel * 255f),
                                        (byte)(lightLevel * 255f), (byte)(lightLevel * 255f), (byte)(distanceFade * 255f));

                                Raylib.DrawTexturePro(Block.Textures[currentBlock.Info.Type],
                                        origTextureRect, newTextureRect, Vector2.Zero, 0f, blockColor);
                            }
                        }
                    }

                    var chunkDebug = new Rectangle(currentChunk.Info.Position.X, currentChunk.Info.Position.Y, 32f, 256f);
                    Raylib.DrawRectangleLinesEx(chunkDebug, 0.25f, Color.WHITE);
                }
            }
        }

        private void regenerateChunks(Vector2 playerChunkPos)
        {
            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                var chunkPos = new Vector2(playerChunkPos.X + (cx * 32) - (renderedChunks.Length * 16), 0f);

                chunkPos -= new Vector2(0.5f, 0.5f); // Offset for appealing block coords

                renderedChunks[cx] = generateChunk(chunkPos);
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

                    noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                    noise.SetFrequency(0.0015f);
                    noise.SetFractalType(FastNoiseLite.FractalType.FBm);
                    noise.SetDomainWarpAmp(200.0f);
                    noise.SetFractalLacunarity(3.0f);
                    noise.SetFractalOctaves(4);

                    float hillGen = noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y);

                    int scaledHillGen = (int)(hillGen * 25f);
                    float horizonPosY = chunkPosition.Y + (scaledHillGen) + 128;

                    if (y == 0) // Horizon
                    {
                        chunk.Blocks[x, y] = new Block(true, new Vector2(chunkPosition.X + x,
                               horizonPosY), Block.Prefabs[BlockType.Grass]);
                    }

                    /*if (y < horizonPosY - 1)
                    {
                        chunk.Blocks[x, y] = new Block(true, new Vector2(chunkPosition.X + x,
                                chunkPosition.Y + y), Block.Prefabs[BlockType.Dirt]);
                    }*/

                    if (chunk.Blocks[x,y] == null)
                        chunk.Blocks[x, y] = currentBlock;

                    currentBlock.ChunkInfo = chunk.Info;
                    //chunk.Blocks[x, y] = currentBlock;
                }
            }

            return chunk;
        }
    }
}
