using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OGF_tool
{
    public enum OGF
    {
        OGF_HEADER = 0x1,

        //build 729
        OGF2_TEXTURE = 0x2,
        OGF2_TEXTURE_L = 0x3,
        OGF2_BBOX = 0x6,
        OGF2_VERTICES = 0x7,
        OGF2_INDICES = 0x8,
        OGF2_VCONTAINER = 0xb,
        OGF2_BSPHERE = 0xc,

        OGF3_TEXTURE = 0x2,
        OGF3_TEXTURE_L = 0x3,
        OGF3_CHILD_REFS = 0x5,
        OGF3_BBOX = 0x6,
        OGF3_VERTICES = 0x7,
        OGF3_INDICES = 0x8,
        OGF3_LODDATA = 0x9, // not sure
        OGF3_VCONTAINER = 0xa,
        OGF3_BSPHERE = 0xb,
        OGF3_CHILDREN_L = 0xc,
        OGF3_S_BONE_NAMES = 0xd,
        OGF3_S_MOTIONS = 0xe,// build 1469 - 1580
        OGF3_DPATCH = 0xf,  // guessed name
        OGF3_LODS = 0x10,   // guessed name
        OGF3_CHILDREN = 0x11,
        OGF3_S_SMPARAMS = 0x12,// build 1469
        OGF3_ICONTAINER = 0x13,// build 1865
        OGF3_S_SMPARAMS_NEW = 0x14,// build 1472 - 1865
        OGF3_LODDEF2 = 0x15,// build 1865
        OGF3_TREEDEF2 = 0x16,// build 1865
        OGF3_S_IKDATA_0 = 0x17,// build 1475 - 1580
        OGF3_S_USERDATA = 0x18,// build 1537 - 1865
        OGF3_S_IKDATA = 0x19,// build 1616 - 1829, 1844
        OGF3_S_MOTIONS_NEW = 0x1a,// build 1616 - 1865
        OGF3_S_DESC = 0x1b,// build 1844
        OGF3_S_IKDATA_2 = 0x1C,// build 1842 - 1865
        OGF3_S_MOTION_REFS = 0x1D,// build 1842

        OGF4_TEXTURE = 0x2,
        OGF4_VERTICES = 0x3,
        OGF4_INDICES = 0x4,
        OGF4_P_MAP = 0x5,
        OGF4_SWIDATA = 0x6,
        OGF4_VCONTAINER = 0x7,
        OGF4_ICONTAINER = 0x8,
        OGF4_CHILDREN = 0x9,
        OGF4_CHILDREN_L = 0xa,
        OGF4_LODDEF2 = 0xb,
        OGF4_TREEDEF2 = 0xc,
        OGF4_S_BONE_NAMES = 0xd,
        OGF4_S_MOTIONS = 0xe,
        OGF4_S_SMPARAMS = 0xf,
        OGF4_S_IKDATA = 0x10,
        OGF4_S_USERDATA = 0x11,
        OGF4_S_DESC = 0x12,
        OGF4_S_MOTION_REFS_0 = 0x13,    // pre-CS format
        OGF4_SWICONTAINER = 0x14,
        OGF4_GCONTAINER = 0x15,
        OGF4_FASTPATH = 0x16,
        OGF4_S_LODS = 0x17,
        OGF4_S_MOTION_REFS_1 = 0x18,    // introduced in clear sky
    };
    public class XRayLoader
    {
        public long chunk_pos = 0;

        uint CHUNK_COMPRESSED = 0x80000000;

        public MemoryStream mem_stream;
        public BinaryReader reader;


        public void Destroy()
        {
            mem_stream.Dispose();
            reader.Dispose();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public int ReadInt32()
        {
            return reader.ReadInt32();
        }

        public float ReadFloat()
        {
            return reader.ReadSingle();
        }

        public uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public bool SetData(byte[] input)
        {
            if (input == null) return false;
            mem_stream = new MemoryStream(input);
            reader = new BinaryReader(mem_stream);
            return true;
        }

        public void SetStream(Stream stream)
        {
            reader = new BinaryReader(stream);
        }

        public void SetReader(BinaryReader rd)
        {
            reader = rd;
        }

        public bool find_chunk(int chunkId, bool skip = false, bool reset = false)
        {
            return find_chunkSize(chunkId, skip, reset) != 0;
        }

        public byte[] find_and_return_chunk_in_chunk(int chunkId, bool skip = false, bool reset = false)
        {
            int size = (int)find_chunkSize(chunkId, skip, reset);

            if (size > 0)
            {
                return ReadBytes(size);
            }
            else
                return null;
        }

        public uint find_chunkSize(int chunkId, bool skip = false, bool reset = false)
        {
            chunk_pos = 0;

            if (reset) reader.BaseStream.Position = 0;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                if (reader.BaseStream.Position + 8 > reader.BaseStream.Length)
                    return 0;

                uint dwType = reader.ReadUInt32();
                uint dwSize = reader.ReadUInt32();

                if (dwType == chunkId || (dwType ^ CHUNK_COMPRESSED) == chunkId)
                {
                    chunk_pos = reader.BaseStream.Position - 8;
                    return dwSize;
                }
                else
                {
                    if (reader.BaseStream.Position + dwSize < reader.BaseStream.Length)
                        reader.BaseStream.Position += dwSize;
                    else if (reader.BaseStream.Position + 8 < reader.BaseStream.Length)
                        reader.BaseStream.Position += 4;
                    else
                        return 0;
                }
            }

            return 0;
        }

        public void open_chunk(BinaryWriter w, int chunkId)
        {
            w.Write(chunkId);
            chunk_pos = w.BaseStream.Position;
            w.Write(0);     // the place for 'size'
        }

        public void close_chunk(BinaryWriter w)
        {
            if (chunk_pos == 0)
            {
                throw new InvalidOperationException("no chunk!");
            }

            long pos = w.BaseStream.Position;
            w.BaseStream.Position = chunk_pos;
            w.Write((int)(pos - chunk_pos - 4));
            w.BaseStream.Position = pos;
            chunk_pos = 0;
        }

        public string read_stringZ()
        {
            List<char> str = new List<char>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte one = reader.ReadByte();
                if (one != 0)
                {
                    str.Add((char)one);
                }
                else
                {
                    break;
                }
            }

            return new string(str.ToArray());
        }

        public void write_stringZ(BinaryWriter w, string str)
        {
            foreach (char c in str)
            {
                byte b = (byte)c;
                w.Write(b);
            }

            w.Write((byte)0);
        }
    }

    public class OGF_Children
    {
        public List<OGF_Child> childs = new List<OGF_Child>();

        public uint chunk_size;

        public long pos;

        public MotionRefs refs = new MotionRefs();

        public UserData userdata = new UserData();

        public OGF_Children()
        {
            refs.pos = -1;
            userdata.pos = -1;
            userdata.userdata = null;
        }

    }

    public struct MotionRefs
    {
        public long pos;
        public List<string> refs0;
        public uint chunk_size()
        {
            uint temp = 4;
            foreach (var text in refs0)
                temp += (uint)text.Length + 1;
            return temp;
        }
        public byte[] count()
        {
            return BitConverter.GetBytes(refs0.Count);
        }
        public byte[] data()
        {
            List<byte> temp = new List<byte>();

            foreach (var str in refs0)
            {
                temp.AddRange(Encoding.ASCII.GetBytes(str));
                temp.Add(0);
            }

            return temp.ToArray();
        }
    }

    public struct UserData
    {
        public long pos;

        public string userdata;

        public uint chunk_size()
        {
            uint temp = 4;
            temp += (uint)userdata.Length + 1;
            return (uint)userdata.Length + 1;
        }

        public byte[] data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(Encoding.ASCII.GetBytes(userdata));
            temp.Add(0);

            return temp.ToArray();
        }
    }

    public class OGF_Child
    {
        public string m_texture;

        public string m_shader;

        public OGF_Child(long _pos, int _parent_id, long _parent_pos, int _old_size, string texture, string shader)
        {
            pos = _pos;
            parent_id = _parent_id;
            parent_pos = _parent_pos;
            m_texture = texture;
            m_shader = shader;
            old_size = _old_size;
        }

        public long pos;

        public long parent_pos;
        public int parent_id;

        public int old_size;

        public uint NewSize()
        {
            return (uint)(m_texture.Length + m_shader.Length + 2 - old_size);
        }

        public byte[] data()
        {
            List<byte> temp = new List<byte>();

            temp.AddRange(BitConverter.GetBytes(2));
            temp.AddRange(BitConverter.GetBytes(m_texture.Length + m_shader.Length + 2));

            foreach (char c in m_texture)
            {
                byte b = (byte)c;
                temp.Add(b);
            }

            temp.Add(0);

            foreach (char c in m_shader)
            {
                byte b = (byte)c;
                temp.Add(b);
            }

            temp.Add(0);

            return temp.ToArray();

        }
    }
}
