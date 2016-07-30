using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftToVoxalia
{
    class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public int X;

        public int Y;

        public int Z;

        public BlockInternal[] BlocksInternal = new BlockInternal[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];

        public int BlockIndex(int x, int y, int z)
        {
            return z * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + x;
        }
        
        public void SetBlockAt(int x, int y, int z, BlockInternal mat)
        {
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }
        
        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }

        public void ReadBlockBytes(byte[] data)
        {
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BlocksInternal[i]._BlockMaterialInternal = BitConverter.ToUInt16(data, i * 2);
                BlocksInternal[i].BlockData = data[BlocksInternal.Length * 2 + i];
                BlocksInternal[i].BlockLocalData = data[BlocksInternal.Length * 3 + i];
                BlocksInternal[i]._BlockPaintInternal = data[BlocksInternal.Length * 4 + i];
            }
        }

        public byte[] BlockBytes()
        {
            byte[] bytes = new byte[BlocksInternal.Length * 5];
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BitConverter.GetBytes(BlocksInternal[i]._BlockMaterialInternal).CopyTo(bytes, i * 2);
                bytes[BlocksInternal.Length * 2 + i] = BlocksInternal[i].BlockData;
                bytes[BlocksInternal.Length * 3 + i] = BlocksInternal[i].BlockLocalData;
                bytes[BlocksInternal.Length * 4 + i] = BlocksInternal[i]._BlockPaintInternal;
            }
            return bytes;
        }
    }
}
