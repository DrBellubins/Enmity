using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Enmity.GameEngine;
using Enmity.Utils;
using FastNoise;
using Raylib_cs;

using static Enmity.Utils.GameMath;

// TODO: Switch raylib rects to SquareColliders
namespace Enmity.Terrain
{
    internal class TerrainGeneration
    {
        public int Seed;
        public Block SelectedBlock = new Block();
        public Block[,] ColliderCheckArray = new Block[4, 4];

        private Chunk[] renderedChunks;

        private List<Chunk> chunkBuffer = new List<Chunk>();

        private FastNoiseLite noise = new FastNoiseLite();
        private Vector2 prevNearestChunkPos;

        private Vector2 worldCursorPos;
        private Rectangle terrainCursor = new Rectangle(0.0f, 0.0f, 1.0f, 1.0f);

        public Vector2 SpawnPosition = new Vector2();

        public void Initialize()
        {
            Seed = new Random().Next(int.MaxValue);
            noise.SetSeed(Seed);

            renderedChunks = new Chunk[8];

            regenerateChunks(GetNearestChunkCoord(new Vector2(0f, 32f)));

            // Set spawn point
            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    Chunk currentChunk = renderedChunks[cx];

                    for (int y = 0; y < 256; y++)
                    {
                        var currentBlock = currentChunk.Blocks[0, y];

                        if (currentBlock.Type != BlockType.Air)
                        {
                            SpawnPosition = currentBlock.Position - new Vector2(0f, 1f);
                            break;
                        }
                            
                        else
                            continue;
                    }
                }
            }
        }

        private bool cursorBlocked = false;
        private Vector2 prevSelectedBlockPos;

        public void Update(Vector2 playerPos, Camera2D camera)
        {
            worldCursorPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);
            var nearestChunkPos = GetNearestChunkCoord(playerPos);

            var terrainCursorPos = GetNearestBlockCoord(worldCursorPos);
            terrainCursor.x = terrainCursorPos.X;
            terrainCursor.y = terrainCursorPos.Y;

            prevSelectedBlockPos = SelectedBlock.Position;
            SelectedBlock = getBlockAtPos(terrainCursorPos);

            cursorBlocked = Raylib.CheckCollisionCircleRec(playerPos, 0.45f, terrainCursor);

            // TODO: Can't place blocks in the sky ?????
            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT)
                && SelectedBlock.Type == BlockType.Air && SelectedBlock.Position == prevSelectedBlockPos && !cursorBlocked)
            {
                Console.WriteLine($"place block {SelectedBlock.Position.X}, {SelectedBlock.Position.Y}");
                //var testBlock = new Block(true, SelectedBlock.Position, Block.Prefabs[hotbarSelection.Type]);
                var testBlock = Block.InstantiatePrefabPos(BlockType.Stone, SelectedBlock.Position);

                //testBlock.LightLevel = 1f;

                PlaceBlock(playerPos, SelectedBlock.Position, testBlock);

                var sound = Block.Sounds[testBlock.Type].RND;
                
                Raylib.SetSoundPitch(sound, GetXorFloat(0.8f, 1.0f));
                Raylib.PlaySound(sound);
            }

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT)
                && SelectedBlock.IsWall && SelectedBlock.Position == prevSelectedBlockPos)
            {
                BreakBlock(playerPos, SelectedBlock.Position);

                var sound = Block.Sounds[SelectedBlock.Type].RND;

                Raylib.SetSoundPitch(sound, GetXorFloat(0.8f, 1.0f));
                Raylib.PlaySound(sound);
            }

            Debug.DrawText($"selected type: {SelectedBlock.Type}, {SelectedBlock.Position.X}, {SelectedBlock.Position.Y}");

            // TODO: Player clips into world (but hovers when falling)
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    var currentBlock = getBlockAtPos(GetNearestBlockCoord(playerPos
                        + new Vector2(x - 1.5f, y - 1.5f)));

                    if (currentBlock.Type != BlockType.Air)
                        ColliderCheckArray[x, y] = currentBlock;
                    else
                        ColliderCheckArray[x, y] = new Block();
                }
            }

            if (nearestChunkPos.X != prevNearestChunkPos.X || nearestChunkPos.Y != prevNearestChunkPos.Y)
                regenerateChunks(nearestChunkPos);

            /*for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    Chunk currentChunk = renderedChunks[cx];

                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 256; y++)
                        {
                            var currentBlock = currentChunk.Blocks[x, y];

                            if (currentBlock.IsWall)
                            {
                                var collider = new SquareCollider(1f, 1f);
                                collider.Position = new Vector2(currentBlock.Position.X, currentBlock.Position.Y);
                            }
                        }
                    }
                }
            }

            var hit = new RaycastHit();
            var testRay = Physics.Raycast(Vector2.Zero, Vector2.UnitY, ref hit, 1f);

            Console.WriteLine($"Hitpos: {hit.Position.X}, {hit.Position.Y} - {testRay}");

            SpawnPos = hit.Position;*/

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

                            if (blockDistance < 64f)
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

                                var blockColor = new Color((byte)(lightLevel * 255f),
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

                    var cursorColor = Color.BLANK;

                    if (cursorBlocked)
                        cursorColor = new Color(255, 0, 0, 8);
                    else
                        cursorColor = new Color(255, 255, 255, 8);

                    Raylib.DrawRectangleLinesEx(terrainCursor, 0.05f, cursorColor);

                    //UI.DrawText($"Chunk index: {cx}", 2f, new Vector2(currentChunk.Info.Position.X, 0f));
                    //var chunkDebug = new Rectangle(currentChunk.Info.Position.X, currentChunk.Info.Position.Y, 32f, 256f);
                    //Raylib.DrawRectangleLinesEx(chunkDebug, 0.25f, Color.WHITE);
                }
            }

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    var colDebug = new Rectangle(ColliderCheckArray[x, y].Position.X, ColliderCheckArray[x, y].Position.Y, 1f, 1f);
                    Raylib.DrawRectangleLinesEx(colDebug, 0.02f, Color.RED);
                }
            }
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

                    if (!retChunk.Generated)
                    {
                        chunkBuffer.Add(retChunk); // Needs to only be done once when first generated
                        retChunk.Generated = true;
                    }
                }

                renderedChunks[cx] = retChunk;
            }
        }

        private Chunk generateChunk(Vector2 chunkPosition)
        {
            Chunk chunk = new Chunk();
            chunk.Position = chunkPosition;

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

                    var blockType = new BlockType();

                    // TODO: Doesn't do anything...
                    //if (y >= (hillGen + 63f) && y < (hillGen + 65f)) // Top grass
                    //    blockType = BlockType.Grass;

                    if (y >= (hillGen + 64f)) // Ground
                    {
                        noise.SetFrequency(0.015f);

                        var stoneGen = (int)(chunkPosition.Y + noise.GetNoise(chunkPosition.X + x, chunkPosition.Y + y) * 5f);

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
                            currentBlock = Block.InstantiatePrefab(blockType);
                            currentBlock.LightLevel = 0f;
                        }
                    }

                    currentBlock.Position = new Vector2(chunkPosition.X + x, chunkPosition.Y + y);

                    //if (chunk.Blocks[x,y] == null)
                    //    chunk.Blocks[x, y] = currentBlock;

                    currentBlock.ParentChunk = chunk;
                    chunk.Blocks[x, y] = currentBlock;
                }
            }

            clearChunkLighting(chunk);
            generateChunkLighting(chunk);

            return chunk;
        }

        // TODO: Lighting only works in y direction (kinda)
        private void generateChunkLighting(Chunk chunk)
        {
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    var currentBlock = chunk.Blocks[x, y];

                    var leftBlock = chunk.Blocks[Clamp(x - 1, 0, 31), y];
                    var rightBlock = chunk.Blocks[Clamp(x + 1, 0, 31), y];
                    var topBlock = chunk.Blocks[x, Clamp(y - 1, 0, 255)];
                    var bottomBlock = chunk.Blocks[x, Clamp(y + 1, 0, 255)];

                    if (currentBlock.Type == BlockType.Air)
                        currentBlock.LightLevel = 1f;
                    else
                    {
                        // TODO: Light is diagonal
                        if (leftBlock != null)
                            currentBlock.LightLevel += (leftBlock.LightLevel * 0.45f);
                        
                        if (rightBlock != null)
                            currentBlock.LightLevel += (rightBlock.LightLevel * 0.45f);

                        if (topBlock != null)
                            currentBlock.LightLevel += (topBlock.LightLevel * 0.45f);

                        if (bottomBlock != null)
                            currentBlock.LightLevel += (bottomBlock.LightLevel * 0.45f);

                        currentBlock.LightLevel = Clamp(currentBlock.LightLevel, 0f, 1f);
                    }
                }
            }
        }

        private void clearChunkLighting(Chunk chunk)
        {
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    var currentBlock = chunk.Blocks[x, y];

                    if (currentBlock.Type == BlockType.Air)
                        currentBlock.LightLevel = 1f;
                    else
                        currentBlock.LightLevel = 0f;
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

                if (chunk.Position == position)
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

        private void PlaceBlock(Vector2 playerPos, Vector2 blockPos, Block block)
        {
            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    var currentChunk = renderedChunks[cx];

                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 256; y++)
                        {
                            var currentBlock = currentChunk.Blocks[x, y];

                            if (Vector2Equals(currentBlock.Position, blockPos))
                            {
                                currentChunk.Blocks[x, y] = block;

                                block.Biome = currentBlock.Biome;

                                // Avoid adding same chunk twice
                                if (!currentChunk.Modified)
                                {
                                    currentChunk.Modified = true;
                                    //WorldSave.Data.ModifiedChunks.Add(currentChunk);
                                }

                                renderedChunks[cx] = currentChunk;
                            }
                        }
                    }

                    renderedChunks[cx] = currentChunk;

                    clearChunkLighting(currentChunk);
                    generateChunkLighting(currentChunk);
                }
            }
        }

        private void BreakBlock(Vector2 playerPos, Vector2 blockPos)
        {
            for (int cx = 0; cx < renderedChunks.Length; cx++)
            {
                if (renderedChunks[cx] != null)
                {
                    var currentChunk = renderedChunks[cx];

                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 256; y++)
                        {
                            var currentBlock = currentChunk.Blocks[x, y];

                            if (Vector2Equals(currentBlock.Position, blockPos))
                            {
                                //var underBlock = Block.InstantiatePrefabPos(currentBlock.Type, blockPos);
                                var underBlock = new Block();
                                underBlock.Position = blockPos;
                                underBlock.IsWall = false;
                                underBlock.Biome = currentBlock.Biome;

                                currentChunk.Blocks[x, y] = underBlock;

                                // Avoid adding same chunk twice
                                if (!currentChunk.Modified)
                                {
                                    currentChunk.Modified = true;
                                    //WorldSave.Data.ModifiedChunks.Add(currentChunk);
                                }

                                renderedChunks[cx] = currentChunk;
                            }
                        }
                    }

                    clearChunkLighting(currentChunk);
                    generateChunkLighting(renderedChunks[cx]);
                }
            }
        }
    }
}
