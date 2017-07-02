using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using LiteDB;

namespace MinecraftToVoxalia
{
    public class ChunkDataManager
    {
        const int DBCount = 10;
        
        LiteDatabase[] ChunksDatabase;

        LiteCollection<BsonDocument>[] DBChunks;

        LiteDatabase[] LODsDatabase;

        //LiteCollection<BsonDocument> DBLODs;

        LiteCollection<BsonDocument>[] DBSuperLOD; // TODO: Optimize SuperLOD to contain many chunks at once?

        LiteCollection<BsonDocument>[] DBLODSix; // TODO: Optimize LODSix to contain many chunks at once?

        LiteDatabase[] EntsDatabase;

        LiteCollection<BsonDocument>[] DBEnts;

        LiteDatabase[] TopsDatabase;

        LiteCollection<BsonDocument>[] DBTops;

        LiteCollection<BsonDocument>[] DBTopsHigher;

        LiteCollection<BsonDocument>[] DBMins;

        LiteDatabase[] HeightMapDatabase;

        LiteCollection<BsonDocument>[] DBHeights;

        LiteCollection<BsonDocument>[] DBHHelpers;

        public void Init()
        {
            string bdir = "/voxalia_saves_file/";
            Directory.CreateDirectory(bdir);
            string dir = Environment.CurrentDirectory + bdir;
            ChunksDatabase = new LiteDatabase[DBCount];
            DBChunks = new LiteCollection<BsonDocument>[DBCount];
            HeightMapDatabase = new LiteDatabase[DBCount];
            DBHeights = new LiteCollection<BsonDocument>[DBCount];
            DBHHelpers = new LiteCollection<BsonDocument>[DBCount];
            LODsDatabase = new LiteDatabase[DBCount];
            DBSuperLOD = new LiteCollection<BsonDocument>[DBCount];
            DBLODSix = new LiteCollection<BsonDocument>[DBCount];
            EntsDatabase = new LiteDatabase[DBCount];
            DBEnts = new LiteCollection<BsonDocument>[DBCount];
            TopsDatabase = new LiteDatabase[DBCount];
            DBTops = new LiteCollection<BsonDocument>[DBCount];
            DBTopsHigher = new LiteCollection<BsonDocument>[DBCount];
            DBMins = new LiteCollection<BsonDocument>[DBCount];
            for (int i = 0; i < DBCount; i++)
            {
                Directory.CreateDirectory(dir + "id_" + i + "/");
                ChunksDatabase[i] = new LiteDatabase("filename=" + dir + "id_" + i + "/chunks.ldb");
                DBChunks[i] = ChunksDatabase[i].GetCollection<BsonDocument>("chunks");
                HeightMapDatabase[i] = new LiteDatabase("filename=" + dir + "id_" + i + "/heights.ldb");
                DBHeights[i] = HeightMapDatabase[i].GetCollection<BsonDocument>("heights");
                DBHHelpers[i] = HeightMapDatabase[i].GetCollection<BsonDocument>("hhelp");
                LODsDatabase[i] = new LiteDatabase("filename=" + dir + "id_" + i + "/lod_chunks.ldb");
                //DBLODs = LODsDatabase.GetCollection<BsonDocument>("lodchunks");
                DBSuperLOD[i] = LODsDatabase[i].GetCollection<BsonDocument>("superlod");
                DBLODSix[i] = LODsDatabase[i].GetCollection<BsonDocument>("lodsix");
                EntsDatabase[i] = new LiteDatabase("filename=" + dir + "id_" + i + "/ents.ldb");
                DBEnts[i] = EntsDatabase[i].GetCollection<BsonDocument>("ents");
                TopsDatabase[i] = new LiteDatabase("filename=" + dir + "id_" + i + "/tops.ldb");
                DBTops[i] = TopsDatabase[i].GetCollection<BsonDocument>("tops");
                DBTopsHigher[i] = TopsDatabase[i].GetCollection<BsonDocument>("topshigh");
                DBMins[i] = TopsDatabase[i].GetCollection<BsonDocument>("mins");
            }
        }

        public void Shutdown()
        {
            for (int i = 0; i < DBCount; i++)
            {
                ChunksDatabase[i].Dispose();
                HeightMapDatabase[i].Dispose();
                LODsDatabase[i].Dispose();
                EntsDatabase[i].Dispose();
                TopsDatabase[i].Dispose();
            }
        }

        public BsonValue GetIDFor(int x, int y, int z)
        {
            byte[] array = new byte[12];
            BitConverter.GetBytes(x).CopyTo(array, 0);
            BitConverter.GetBytes(y).CopyTo(array, 4);
            BitConverter.GetBytes(z).CopyTo(array, 8);
            return new BsonValue(array);
        }

        public struct Heights
        {
            public int A, B, C, D;
            public ushort MA, MB, MC, MD;
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector2i, byte[]> HeightHelps = new ConcurrentDictionary<Vector2i, byte[]>();

        public static int DBIDFor(int x, int y)
        {
            return Math.Abs((x * 17 + y) % DBCount);
        }

        public byte[] GetHeightHelper(int x, int y)
        {
            if (HeightHelps.TryGetValue(new Vector2i(x, y), out byte[] hhe))
            {
                return hhe;
            }
            BsonDocument doc = DBHHelpers[DBIDFor(x, y)].FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return null;
            }
            byte[] b = doc["hh"].AsBinary;
            HeightHelps[new Vector2i(x, y)] = b;
            return b;
        }

        public void WriteHeightHelper(int x, int y, byte[] hhelper)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["hh"] = hhelper;
            DBHHelpers[DBIDFor(x, y)].Upsert(newdoc);
            HeightHelps[new Vector2i(x, y)] = hhelper;
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector2i, Heights> HeightEst = new ConcurrentDictionary<Vector2i, Heights>();

        public Heights GetHeightEstimates(int x, int y)
        {
            if (HeightEst.TryGetValue(new Vector2i(x, y), out Heights hei))
            {
                return hei;
            }
            BsonDocument doc = DBHeights[DBIDFor(x, y)].FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return new Heights() { A = int.MaxValue, B = int.MaxValue, C = int.MaxValue, D = int.MaxValue };
            }
            Heights h = new Heights()
            {
                A = doc["a"].AsInt32,
                B = doc["b"].AsInt32,
                C = doc["c"].AsInt32,
                D = doc["d"].AsInt32,
                MA = (ushort)doc["ma"].AsInt32,
                MB = (ushort)doc["mb"].AsInt32,
                MC = (ushort)doc["mc"].AsInt32,
                MD = (ushort)doc["md"].AsInt32
            };
            HeightEst[new Vector2i(x, y)] = h;
            return h;
        }

        public void WriteHeightEstimates(int x, int y, Heights h)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            // TODO: Inefficient use of document structure!
            tbs["_id"] = id;
            tbs["a"] = h.A;
            tbs["b"] = h.B;
            tbs["c"] = h.C;
            tbs["d"] = h.D;
            tbs["ma"] = (int)h.MA;
            tbs["mb"] = (int)h.MB;
            tbs["mc"] = (int)h.MC;
            tbs["md"] = (int)h.MD;
            DBHeights[DBIDFor(x, y)].Upsert(newdoc);
            HeightEst[new Vector2i(x, y)] = h;
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector3i, byte[]> LODSixes = new ConcurrentDictionary<Vector3i, byte[]>();

        public byte[] GetLODSixChunkDetails(int x, int y, int z)
        {
            Vector3i vec = new Vector3i(x, y, z);
            if (LODSixes.TryGetValue(vec, out byte[] res))
            {
                return res;
            }
            BsonDocument doc = DBLODSix[DBIDFor(x, y)].FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            byte[] blocks = doc["blocks"].AsBinary;
            SLODders[vec] = blocks;
            return blocks;
        }

        public void WriteLODSixChunkDetails(int x, int y, int z, byte[] SLOD)
        {
            Vector3i vec = new Vector3i(x, y, z);
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["blocks"] = new BsonValue(SLOD);
            LODSixes[vec] = SLOD;
            DBLODSix[DBIDFor(x, y)].Upsert(newdoc);
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector3i, byte[]> SLODders = new ConcurrentDictionary<Vector3i, byte[]>();

        public byte[] GetSuperLODChunkDetails(int x, int y, int z)
        {
            Vector3i vec = new Vector3i(x, y, z);
            if (SLODders.TryGetValue(vec, out byte[] res))
            {
                return res;
            }
            BsonDocument doc;
            doc = DBSuperLOD[DBIDFor(x, y)].FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            byte[] blocks = doc["blocks"].AsBinary;
            SLODders[vec] = blocks;
            return blocks;
        }

        public void WriteSuperLODChunkDetails(int x, int y, int z, byte[] SLOD)
        {
            Vector3i vec = new Vector3i(x, y, z);
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["blocks"] = new BsonValue(SLOD);
            SLODders[vec] = SLOD;
            DBSuperLOD[DBIDFor(x, y)].Upsert(newdoc);
        }

        public byte[] GetChunkBytes(Vector3i pos)
        {
            BsonDocument bsd = DBChunks[DBIDFor(pos.X, pos.Y)].FindById(GetIDFor(pos.X, pos.Y, pos.Z));
            if (bsd == null)
            {
                return null;
            }
            return Program.UnGZip(bsd["blocks"].AsBinary);
        }
        
        public void WriteChunkDetails(int version, int flags, byte[] blocks, Vector3i details, byte[] reachables)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["version"] = new BsonValue(version);
            tbs["flags"] = new BsonValue(flags);
            tbs["blocks"] = new BsonValue(blocks.Length == 0 ? blocks : Program.GZip(blocks));
            tbs["reach"] = new BsonValue(reachables);
            DBChunks[DBIDFor(details.X, details.Y)].Upsert(newdoc);
        }

        public void ClearChunkDetails(Vector3i details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            DBChunks[DBIDFor(details.X, details.Y)].Delete(id);
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector2i, int> Mins = new ConcurrentDictionary<Vector2i, int>();

        public int GetMins(int x, int y)
        {
            Vector2i input = new Vector2i(x, y);
            if (Mins.TryGetValue(input, out int output))
            {
                return output;
            }
            BsonDocument doc;
            doc = DBMins[DBIDFor(x, y)].FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return 0;
            }
            return doc["min"].AsInt32;
        }

        public void SetMins(int x, int y, int min)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["min"] = new BsonValue(min);
            Mins[new Vector2i(x, y)] = min;
            DBMins[DBIDFor(x, y)].Upsert(newdoc);
        }

    }
}
