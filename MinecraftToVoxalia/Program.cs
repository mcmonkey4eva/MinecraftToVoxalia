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
            if (mat == 0)
            {
                return BlockInternal.AIR;
            }
            if (mat == 2 || mat == 3)
            {
                return Quick(mat);
            }
            else if (mat == 5)
            {
                return Quick(26);
            }
            else if (mat == 8 || mat == 9)
            {
                return Quick(4);
            }
            else if (mat == 12)
            {
                return Quick(13);
            }
            else if (mat == 17 || mat == 162)
            {
                return Quick(11);
            }
            else if (mat == 18 || mat == 161)
            {
                return Quick(6);
            }
            // TODO: Other material translations!
            return Quick(1);
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
            NbtWorld world = NbtWorld.Open("./");
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
            GZipStream GZStream = new GZipStream(memstream, CompressionMode.Compress);
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
                GZipStream GZStream = new GZipStream(memstream, CompressionMode.Decompress);
                GZStream.CopyTo(output);
                GZStream.Close();
                memstream.Close();
                return output.ToArray();
            }
        }
    }
}
