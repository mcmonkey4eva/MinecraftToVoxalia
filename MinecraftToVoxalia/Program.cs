using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Substrate;
using Substrate.Core;
using LiteDB;
using LZ4;

namespace MinecraftToVoxalia
{
    class Program
    {
        static BlockInternal Quick(ushort id)
        {
            return new BlockInternal(id, 0, 0, 0);
        }

        static BlockInternal Translate(byte mat, byte dat)
        {
            switch (mat)
            {
                case 0:
                    return BlockInternal.AIR;
                case 1:
                    return Quick(1); // Stone
                case 2:
                    return Quick(2); // Grass_Forest
                case 3:
                    return Quick(3); // Dirt
                case 4:
                    return Quick(33); // Cobblestone
                case 5:
                    return Quick(26); // Planks_Oak
                // 6 : baby tree
                // 7: bedrock
                case 8:
                case 9:
                    // TODO: flowing stuff, shapes
                    return Quick(4); // Water
                case 10:
                case 11:
                    // TODO: flowing stuff, shapes
                    return Quick(35); // Lava
                case 12:
                    return Quick(13); // Sand
                // 13: gravel
                // 14: gold ore
                // 15: iron ore
                case 16:
                    return Quick(22); // Coal_ore
                case 17:
                    // TODO: Log types
                    return Quick(11); // Log_Oak
                case 18:
                    // TODO: Leaf types
                    return Quick(6); // Leaves_Oak_Solid
                // 19: sponge
                case 20:
                    return Quick(27); // Glass_Window
                // 21: lapis ore
                // 22: lapis block
                // 23: dispenser
                case 24:
                    return Quick(17); // Sandstone
                // 25: Note block
                // 26: Bed
                // 27: powered rail
                // 28: detector rail
                // 29: sticky piston
                // 30: cobweb
                // 31: dead shrub, tallgrass, fern
                // 32: dead bush
                // 33: piston
                // 34: piston head
                // 35: wool (colors)
                // 37: dandelion
                // 38: poppy
                // 39: brown mushroom
                // 40: red mushroom
                // 41: gold block
                // 42: iron block
                case 43:
                    // TODO: "Double slab types"
                    return Quick(1); // Stone
                // 44: half height slab
                case 45:
                    return Quick(37); // Bricks
                // 46: tnt
                // 47: bookshelves
                // 48: mossy stone
                // 49: obsidian
                // 50: torch
                case 51:
                    return Quick(38); // Fire
                // 52: monster cage
                // 53: oak stairs (directions)
                // 54: chest
                // 55: redstone wire
                // 56: diamond ore
                // 57: diamond block
                // 58: crafting table
                // 59: wheat crops
                // 60: farmland
                // 61: furnace (directions)
                // 62: burning furnace (directions)
                // 63: standing sign
                // 64: oak door
                // 65: ladder
                // 66: rail
                // 67: cobblestone stairs
                // 68: wall mounted sign
                // 69: lever
                // 70: stone pressure plate
                // 71: iron door
                // 72: wooden pressure plate
                // 73: redstone ore
                // 74: glowing redstone ore
                // 75: redstone torch off
                // 76: redstone torch lit
                // 77: stone button
                case 78:
                    // TODO: Heights
                    return Quick(9); // Snow_Solid
                case 79:
                    return Quick(29); // Ice
                case 80:
                    return Quick(9); // Snow_Solid
                // 81: cactus
                // 82: clay
                // 83: sugar cane
                // 84: jukebox
                // 85: oak fence
                // 86: pumpkin
                // 87: nether rack
                // 88: soul sand
                // 89: glowstone
                // 90: nether portal
                // 91: jack o'lantern
                // 92: cake
                // 93: repeater off
                // 94: repeater on
                case 95:
                    // TODO: colors!
                    return Quick(27); // Glass_Window
                // 96: wood trapdoor
                case 97:
                    return Quick(1);
                case 98:
                    // TODO: stone bricks!
                    // TODO: Sub-types!
                    return Quick(1);
                // 99: Brown mushroom block
                // 100: red mushroom block
                // 101: iron bars
                case 102:
                    // TODO: Glass pane directions!
                    return Quick(27); // Glass_Window
                // 103: melon
                // 104: baby pumpkin plant
                // 105: baby melon plant
                // 106: vines
                // 107: oak fence gate
                // 108: brick stairs
                // 109: stone brick stairs
                // 110: mycelium
                // 111: lily pad
                // 112: nether brick
                // ... 
                // TODO: Continue from: http://minecraft-ids.grahamedgecombe.com/
                case 161:
                    // TODO: Leaf types
                    return Quick(6); // Leaves_Oak_Solid
                case 162:
                    // TODO: Log types
                    return Quick(11); // Log_Oak
                default:
                    // TODO: Other material translations!
                    return Quick(24); // Color
            }
        }

        static LiteCollection<BsonDocument> DBChunks;

        static void Main(string[] args)
        {
            Console.WriteLine("Start processing...");
            LiteDatabase Database = new LiteDatabase("filename=" + Environment.CurrentDirectory + "/chunks.ldb");
            DBChunks = Database.GetCollection<BsonDocument>("chunks");
            if (!File.Exists("./level.dat"))
            {
                Console.WriteLine("No level.dat found!");
                return;
            }
            AnvilWorld world = AnvilWorld.Open("./");
            if (world == null)
            {
                Console.WriteLine("World loading failed!");
                return;
            }
            IChunkManager chunks = world.GetChunkManager();
            foreach (ChunkRef chunk in chunks)
            {
                if (chunk == null || chunk.Blocks == null)
                {
                    Console.WriteLine("Skip chunk, invalid data!");
                    continue;
                }
                Console.WriteLine("Try " + chunk.X + "," + chunk.Z);
                AlphaBlockCollection blocks = chunk.Blocks;
                int xw = blocks.XDim;
                int yw = blocks.YDim;
                int zw = blocks.ZDim;
                for (int x = 0; x < xw; x++)
                {
                    for (int z = 0; z < zw; z++)
                    {
                        for (int y = 0; y < yw; y++)
                        {
                            int xb = chunk.X * xw + x;
                            int yb = y;
                            int zb = chunk.Z * zw + z;
                            Vector3i cpos = new Vector3i((int)Math.Floor(xb / (double)Chunk.CHUNK_SIZE), (int)Math.Floor(zb / (double)Chunk.CHUNK_SIZE), (int)Math.Floor(yb / (double)Chunk.CHUNK_SIZE));
                            Chunk ch = GetChunkAt(cpos);
                            AlphaBlock ab = blocks.GetBlock(x, y, z);
                            ch.SetBlockAt(xb - cpos.X * Chunk.CHUNK_SIZE, zb - cpos.Y * Chunk.CHUNK_SIZE, yb - cpos.Z * Chunk.CHUNK_SIZE, Translate((byte)ab.ID, (byte)ab.Data));
                        }
                    }
                }
            }
            SaveAndClean();
            Database.Dispose();
            Console.WriteLine("Processing complete!");
        }

        public static void SaveAndClean()
        {
            Console.WriteLine("Save progress and clean...");
            foreach (Chunk tch in LoadedChunks.Values)
            {
                WriteChunkDetails(tch);
            }
            LoadedChunks.Clear();
            Console.WriteLine("Save and clean complete!");
        }

        public static Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

        public static Chunk GetChunkAt(Vector3i cpos)
        {
            Chunk ch;
            if (LoadedChunks.TryGetValue(cpos, out ch))
            {
                return ch;
            }
            ch = GetChunkDetails(cpos.X, cpos.Y, cpos.Z);
            if (ch == null)
            {
                ch = new Chunk();
                ch.X = cpos.X;
                ch.Y = cpos.Y;
                ch.Z = cpos.Z;
            }
            if (LoadedChunks.Count > 1000)
            {
                SaveAndClean();
            }
            LoadedChunks[cpos] = ch;
            return ch;
        }

        public static BsonValue GetIDFor(int x, int y, int z)
        {
            byte[] array = new byte[12];
            BitConverter.GetBytes(x).CopyTo(array, 0);
            BitConverter.GetBytes(y).CopyTo(array, 4);
            BitConverter.GetBytes(z).CopyTo(array, 8);
            return new BsonValue(array);
        }

        public static Chunk GetChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBChunks.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            Chunk chunk = new Chunk();
            chunk.X = x;
            chunk.Y = y;
            chunk.Z = z;
            chunk.ReadBlockBytes(UnGZip(doc["blocks"].AsBinary));
            return chunk;
        }

        public static void WriteChunkDetails(Chunk chunk)
        {
            BsonValue id = GetIDFor(chunk.X, chunk.Y, chunk.Z);
            BsonDocument newdoc = new BsonDocument();
            newdoc["_id"] = id;
            newdoc["version"] = new BsonValue(2);
            newdoc["flags"] = new BsonValue(0);
            newdoc["blocks"] = new BsonValue(GZip(chunk.BlockBytes()));
            newdoc["reach"] = new BsonValue(new byte[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            DBChunks.Delete(id);
            DBChunks.Insert(newdoc);
        }

        public static byte[] GZip(byte[] input)
        {
            MemoryStream memstream = new MemoryStream();
            LZ4Stream GZStream = new LZ4Stream(memstream, CompressionMode.Compress);
            GZStream.Write(input, 0, input.Length);
            GZStream.Close();
            byte[] finaldata = memstream.ToArray();
            memstream.Close();
            return finaldata;
        }
        
        public static byte[] UnGZip(byte[] input)
        {
            using (MemoryStream output = new MemoryStream())
            {
                MemoryStream memstream = new MemoryStream(input);
                LZ4Stream GZStream = new LZ4Stream(memstream, CompressionMode.Decompress);
                GZStream.CopyTo(output);
                GZStream.Close();
                memstream.Close();
                return output.ToArray();
            }
        }
    }
}
