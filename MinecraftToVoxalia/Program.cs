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
    struct BlockData
    {
        public string Name;

        public int ResultantID;
    }

    class Program
    {
        static BlockInternal Quick(ushort id)
        {
            return new BlockInternal(id, 0, 0, 0);
        }

        static Dictionary<int, Dictionary<int, BlockData>> TranslationsPrefixed = new Dictionary<int, Dictionary<int, BlockData>>();

        static BlockInternal Translate(byte mat, byte dat)
        {
            // TODO: This whole switch should be a dictionary... or even an array!
            switch (mat)
            {
                case 0: // Air
                    return BlockInternal.AIR;
                case 1: // Stone
                    return Quick(40); // Stone
                case 2: // Grass
                    return Quick(36); // Grass_Forest
                case 3: // Dirt
                    return Quick(3); // Dirt
                case 4: // Cobblestone
                    return Quick(33); // Cobblestone
                case 5: // Planks
                    // TODO: Plank types
                    return Quick(26); // Planks_Oak
                // 6: baby tree
                // 7: bedrock
                case 8: // water
                case 9: // water
                    // TODO: flowing stuff, shapes
                    return Quick(4); // Water
                case 10: // lava
                case 11: // lava
                    // TODO: flowing stuff, shapes
                    return Quick(35); // Lava
                case 12: // sand
                    return Quick(13); // Sand
                // 13: gravel
                case 14: // gold ore
                    return Quick(22); // Coal_ore // TODO
                case 15: // iron ore
                    return Quick(22); // Coal_ore // TODO
                case 16: // coal_ore
                    return Quick(22); // Coal_ore
                case 17: // log
                    // TODO: Log types
                    return Quick(11); // Log_Oak
                case 18:
                    // TODO: Leaf types
                    return Quick(6); // Leaves_Oak_Solid
                // 19: sponge
                case 20: // glass
                    return Quick(27); // Glass_Window
                // 21: lapis ore
                // 22: lapis block
                // 23: dispenser
                case 24: // sandstone
                    return Quick(17); // Sandstone
                // 25: Note block
                // 26: Bed
                // 27: powered rail
                // 28: detector rail
                // 29: sticky piston
                // 30: cobweb
                case 31: // dead shrub, tallgrass, fern
                    return BlockInternal.AIR; // TODO
                // 32: dead bush
                // 33: piston
                // 34: piston head
                // 35: wool (colors)
                case 37: // dandelion
                    return BlockInternal.AIR; // TODO
                case 38: // poppy
                    return BlockInternal.AIR; // TODO
                // 39: brown mushroom
                // 40: red mushroom
                // 41: gold block
                // 42: iron block
                case 43: // double_slab
                    // TODO: "Double slab types"
                    return Quick(40); // Stone
                // 44: half height slab
                case 45: // slab
                    return Quick(37); // Bricks
                // 46: tnt
                // 47: bookshelves
                // 48: mossy stone
                // 49: obsidian
                // 50: torch
                case 51: // fire
                    return Quick(38); // Fire
                // 52: monster cage
                // 53: oak stairs (directions)
                // 54: chest
                // 55: redstone wire
                case 56: // diamond ore
                    return Quick(22); // Coal_ore // TODO
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
                case 78: // snow
                    // TODO: Heights
                    return Quick(9); // Snow_Solid
                case 79: // ice
                    return Quick(29); // Ice
                case 80: // snow block
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
                case 95: // stained glass
                    // TODO: colors!
                    return Quick(27); // Glass_Window
                // 96: wood trapdoor
                case 97: // stone monster egg
                    return Quick(40);
                case 98: // stone bricks
                    // TODO: stone bricks!
                    // TODO: Sub-types!
                    return Quick(40);
                // 99: Brown mushroom block
                // 100: red mushroom block
                // 101: iron bars
                case 102: // glass pane
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
                // 113: nether brick fence
                // 114: nether brick stairs
                // 115: nether wart
                // 116: enchantment table
                // 117: brewing stand
                // 118: cauldron
                // 119: end portal
                // 120: end portal frame
                case 121: // end stone
                    return Quick(17); // Sandstone
                // 122: dragon egg
                // 123: redstone lamp off
                // 124: redstone lamp on
                case 125: // Double Wood Plank Slab
                    // TODO: Plank types
                    return Quick(26); // Planks_Oak
                // 126: wood plank slabs
                // 127: cocoa
                // 128: sandstone stairs
                // 129: emerald ore
                // 130: ender chest
                // 131: tripwire hook
                // 132: tripwire
                // 133: emerald block
                // 134: spruce wood stairs
                // 135: birch wood stairs
                // 136: jungle wood stairs
                // 137: coomand block
                // 138: beacon
                // 139: Cobblestone wall
                // 140: flower pot
                // 141: carrots
                // 142: potatoes
                // 143: wooden button
                // 144: mob head
                // 145: anvil
                // 146: trapped chest
                // 147: weighted pressure plate light (gold)
                // 148: weight pressure plate heavy (iron)
                // 149: redstone comparator off
                // 150: redstone comparator on
                // 151: daylight sensor
                // 152: redstone block
                // 153: nether quartz ore
                // 154: hopper
                // 155: quartz block
                // 156: quartz stairs
                // 157: activator rail
                // 158: dropper
                // 159: hardened clay
                case 160: // stained glass pane
                    // TODO: Glass pane directions, colors!
                    return Quick(27); // Glass_Window
                case 161:
                    // TODO: Leaf types
                    return Quick(6); // Leaves_Oak_Solid
                case 162:
                    // TODO: Log types
                    return Quick(11); // Log_Oak
                // 163: acacia wood stairs
                // 164: dark oak wood stairs
                // 165: slime block
                // 166: barrier block
                // 167: iron trapdoor
                // 168: prismarine
                // 169: sea lantern
                // 170: hay bale
                // 171: carpet (colors)
                // 172: hardened clay
                // 173: block of coal
                // 174: packed ice
                // 175: double tall plants
                // 176: free standing banner
                // 177: wall mounted banner
                // 178: inverted daylight sensor
                // 179: red sandstone
                // 180: red sandstone stairs
                // 181: double red sandstone slab
                // 182: red sandstone slab
                // 183: spruce fence gate
                // 184: birch fence gate
                // 185: jungle fence gate
                // 186: dark oak fence gate
                // 187: acacia fence gate
                // 188: spruce fence
                // 189: birch fence
                // 190: jungle fence
                // 191: dark oak fence
                // 192: acacia fence
                // 193: spruce door block
                // 194: birch door block
                // 195: jungle door block
                // 196: acacia door block
                // 197: dark oak door block
                // 198: end rod
                // 199: chorus plant
                // 200: chorus flower
                // ... 
                // TODO: Continue from (up to 255): http://minecraft-ids.grahamedgecombe.com/
                default:
                    if (TranslationsPrefixed.TryGetValue(mat, out Dictionary<int, BlockData> vals) && vals.TryGetValue(dat, out BlockData d))
                    {
                        return Quick((ushort)d.ResultantID);
                    }
                    // TODO: Other material translations!
                    return Quick(24); // Color
            }
        }

        static ChunkDataManager ChunkManager;

        static string FindTextureFor(string bName)
        {
            if (!File.Exists("./block/" + bName + ".json"))
            {
                return "TextureBasic=blocks/minecraft/stone";
            }
            string datum = File.ReadAllText("./block/" + bName + ".json");
            BsonValue b = JsonSerializer.Deserialize(datum);
            if (b.IsDocument && b.AsDocument.ContainsKey("textures"))
            {
                BsonDocument doc = b.AsDocument["textures"].AsDocument;
                if (doc.ContainsKey("all"))
                {
                    return "TextureBasic=blocks/minecraft/" + doc["all"].AsString;
                }
                else if (doc.ContainsKey("end") && doc.ContainsKey("side"))
                {
                    return "TextureBasic=blocks/minecraft/" + doc["side"].AsString
                        + "\r\nTexture_TOP=" + doc["end"].AsString
                        + "\r\nTexture_BOTTOM=" + doc["end"].AsString;
                }
                else if (doc.ContainsKey("top") && doc.ContainsKey("bottom") && doc.ContainsKey("side"))
                {
                    return "TextureBasic=blocks/minecraft/" + doc["side"].AsString
                        + "\r\nTexture_TOP=" + doc["top"].AsString
                        + "\r\nTexture_BOTTOM=" + doc["bottom"].AsString;
                }
            }
            return "TextureBasic=blocks/minecraft/stone";
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "interpret_data")
                {
                    int zf = 0;
                    string[] xml = File.ReadAllText("./xml.txt").Split('\n');
                    for (int i = 0; i < xml.Length; i++)
                    {
                        if (xml[i].Contains("<td class=\"id\">"))
                        {
                            string trimmed = xml[i].Trim();
                            string idder = trimmed.Substring("<td class=\"id\">".Length, trimmed.Length - ("<td class=\"id\">".Length + "</td>".Length));
                            string[] bits = idder.Split(':');
                            int b = bits.Length > 1 ? int.Parse(bits[1]) : 0;
                            int d = int.Parse(bits[0]);
                            if (!TranslationsPrefixed.TryGetValue(d, out Dictionary<int, BlockData> subVal))
                            {
                                subVal = new Dictionary<int, BlockData>();
                                TranslationsPrefixed[d] = subVal;
                            }
                            string textValPrep = xml[i + 2].Trim();
                            textValPrep = textValPrep.Substring(textValPrep.IndexOf("<span class=\"text-id\">(minecraft:") + "<span class=\"text-id\">(minecraft:".Length);
                            textValPrep = textValPrep.Substring(0, textValPrep.Length - ")</span></td>".Length);
                            subVal[b] = new BlockData() { Name = textValPrep, ResultantID = 1000 + zf };
                            zf++;
                        }
                    }
                    Directory.CreateDirectory("./info_blocks/");
                    Directory.CreateDirectory("./item_blocks/");
                    foreach (KeyValuePair<int, Dictionary<int, BlockData>> vals in TranslationsPrefixed)
                    {
                        foreach (KeyValuePair<int, BlockData> bDat in vals.Value)
                        {
                            File.WriteAllText("./info_blocks/" + bDat.Value.ResultantID + ".blk",
                                "Name=" + bDat.Value.Name + "_MC_VERSION\r\nSound=STONE\r\n" + FindTextureFor(bDat.Value.Name) + "\r\n"
                                );
                            File.WriteAllText("./item_blocks/" + bDat.Value.Name + ".itm",
                                "type: block\r\nicon: render_block:self\r\ndisplay: " + bDat.Value.Name + "\r\nDescription: Imported from minecraft!\r\ncolor: 1,1,1\r\nmodel: block\r\nbound: false"
                                + "\r\ndatum: " + bDat.Value.ResultantID + "\r\nweight: 1\r\nvolume: 1\r\n"
                                );
                        }
                    }
                }
            }
            Console.WriteLine("Start processing...");
            ChunkManager = new ChunkDataManager();
            ChunkManager.Init();
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
            // TODO: LOD/SLOD post-management? or do that via Voxalia itself somehow?
            ChunkManager.Shutdown();
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
            if (LoadedChunks.TryGetValue(cpos, out Chunk ch))
            {
                return ch;
            }
            ch = GetChunkDetails(cpos.X, cpos.Y, cpos.Z);
            if (ch == null)
            {
                ch = new Chunk() { X = cpos.X, Y = cpos.Y, Z = cpos.Z };
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
            byte[] doc = ChunkManager.GetChunkBytes(new Vector3i(x, y, z));
            if (doc == null)
            {
                return null;
            }
            Chunk chunk = new Chunk() { X = x, Y = y, Z = z };
            chunk.ReadBlockBytes(doc);
            return chunk;
        }

        static byte[] reach = new byte[15] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        public static void WriteChunkDetails(Chunk chunk)
        {
            ChunkManager.WriteChunkDetails(2, 0, chunk.BlockBytes(), new Vector3i(chunk.X, chunk.Y, chunk.Z), reach);
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
