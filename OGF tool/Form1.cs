using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OGF_tool
{

	public partial class Form1 : Form
	{

		// File sytem
		public OGF_Children OGF_V = null;
        public byte[] Current_OGF = null;
		public List<byte> file_bytes = new List<byte>();
		public string FILE_NAME = "";

		// Input
		public bool bKeyIsDown = false;

        
		public Form1()
		{
			InitializeComponent();

			//foreach (var box in textBoxes)
			//tabPage1.Controls.Add(box);

			if (Environment.GetCommandLineArgs().Length > 1)
			{
				OpenFile(Environment.GetCommandLineArgs()[1]);
				FILE_NAME = Environment.GetCommandLineArgs()[1];
				AfterLoad();
			}

		}

		private void CreateGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 126 * idx);
			GroupBox.Size = new System.Drawing.Size(366, 126);
			GroupBox.Text = "Set: [" + idx + "]";
			GroupBox.Name = "GrpBox_" + idx;
			CreateBoxes(idx, GroupBox);
			CreateLabels(idx, GroupBox);
			tabPage1.Controls.Add(GroupBox);
		}

		private void CreateBoxes(int idx, GroupBox box)
		{
			var newTextBox = new TextBox();
			newTextBox.Name = "textureBox_" + idx;
			newTextBox.Size = new System.Drawing.Size(355, 23);
			//newTextBox.Location = new System.Drawing.Point(56,30+88*idx + 15* idx);
			newTextBox.Location = new System.Drawing.Point(6, 39);
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);

			var newTextBox2 = new TextBox();
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.Size = new System.Drawing.Size(355, 23);
			//newTextBox2.Location = new System.Drawing.Point(56, 74 + 88* idx + 15 * idx);
			newTextBox2.Location = new System.Drawing.Point(6, 88);
			newTextBox2.TextChanged += new System.EventHandler(this.TextBoxFilter);
			box.Controls.Add(newTextBox);
			box.Controls.Add(newTextBox2);
		}

		private void CreateLabels(int idx, GroupBox box)
		{
			var newLbl = new Label();
			newLbl.Name = "textureLbl_" + idx;
			newLbl.Text = "Texture Path:";
			//newLbl.Size = new System.Drawing.Size(75, 15);
			//newLbl.Location = new System.Drawing.Point(56, 12 + 88*idx + 15 * idx);
			newLbl.Location = new System.Drawing.Point(6, 21);

			var newLbl2 = new Label();
			newLbl2.Name = "shaderLbl_" + idx;
			newLbl2.Text = "Shader Name:";
			//newLbl2.Size = new System.Drawing.Size(81, 15);
			//newLbl2.Location = new System.Drawing.Point(56, 56 + 88 *idx + 15 * idx);
			newLbl2.Location = new System.Drawing.Point(6, 70);

			//var newLbl3 = new Label();
			//newLbl3.Name = "textureSetLbl_" + idx;
			//newLbl3.Text = "Set: [" + idx +"]";
			//newLbl3.Location = new System.Drawing.Point(0, 44 + 88 * idx + 15 * idx);

			box.Controls.Add(newLbl);
			box.Controls.Add(newLbl2);
			//box.Controls.Add(newLbl3);
		}

		private void Clear()
		{
			FILE_NAME = "";
			OGF_V = null;
			file_bytes.Clear();
			tabPage1.Controls.Clear();
			richTextBox1.Clear();
		}

		private void AfterLoad()
		{
			for (int i = 0; i < OGF_V.childs.Count; i++)
			{
				CreateGroupBox(i);

				var box = tabPage1.Controls["GrpBox_" + i];

				if (box != null)
				{
					var Cntrl = box.Controls["textureBox_" + i];
					Cntrl.Text = OGF_V.childs[i].m_texture;
					var Cntrl2 = box.Controls["shaderBox_" + i];
					Cntrl2.Text = OGF_V.childs[i].m_shader;
				}
			}

            richTextBox1.Clear();
            richTextBox2.Clear();

            if (OGF_V.refs.refs0 != null)
			    richTextBox1.Lines = OGF_V.refs.refs0.ToArray();

            if (OGF_V.usertdata!=null)
                richTextBox2.Text = OGF_V.usertdata.data;
        }

		private void button1_Click(object sender, EventArgs e)
		{
			openFileDialog1.FileName = "";
			DialogResult res = openFileDialog1.ShowDialog();

			if (res == DialogResult.OK)
			{
				Clear();
				FILE_NAME = openFileDialog1.FileName;
				OpenFile(FILE_NAME);
				AfterLoad();
			}
		}

		private void CopyParams()
		{
			if (richTextBox1.Lines.Count() > 0)
			{
				OGF_V.refs.refs0 = new List<string>();
				OGF_V.refs.refs0.AddRange(richTextBox1.Lines);
			}
            else
            {
                OGF_V.refs.refs0 = null;
            }
            

            if(richTextBox2.Text != "" && OGF_V.usertdata!=null)
            {
                OGF_V.usertdata.data = "";

                for (int i = 0; i < richTextBox2.Lines.Count(); i++)
                {
                    string ext = i == richTextBox2.Lines.Count() - 1 ? "" : "\r\n";
                    OGF_V.usertdata.data += richTextBox2.Lines[i] + ext;
                }
            }
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (FILE_NAME == "") return;

			CopyParams();
			SaveFile(FILE_NAME);
		}

		private void SaveFile(string filename)
		{
			file_bytes.Clear();

            if(Current_OGF == null) return;


			using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
			{
				byte[] temp = fileStream.ReadBytes((int)(OGF_V.pos));
				file_bytes.AddRange(temp);

				uint chld_section = fileStream.ReadUInt32();
				uint new_size = fileStream.ReadUInt32();

				foreach (var ch in OGF_V.childs)
				{
					new_size += ch.NewSize();
				}

                file_bytes.AddRange(BitConverter.GetBytes(chld_section));
				file_bytes.AddRange(BitConverter.GetBytes(new_size));

				foreach (var ch in OGF_V.childs)
				{
					temp = fileStream.ReadBytes((int)(ch.parent_pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);
					fileStream.ReadUInt32();
					new_size = fileStream.ReadUInt32();
					new_size += ch.NewSize();
					file_bytes.AddRange(BitConverter.GetBytes(ch.parent_id));
					file_bytes.AddRange(BitConverter.GetBytes(new_size));

					temp = fileStream.ReadBytes((int)(ch.pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);
					file_bytes.AddRange(ch.data());
					fileStream.BaseStream.Position += ch.old_size + 8;
				}

                if(OGF_V.usertdata != null)
                {
                    if (OGF_V.usertdata.pos > 0)
                        temp = fileStream.ReadBytes((int)(OGF_V.usertdata.pos - fileStream.BaseStream.Position));
                    else
                        temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

                    file_bytes.AddRange(temp);

                    file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_USERDATA));
                    file_bytes.AddRange(BitConverter.GetBytes(OGF_V.usertdata.chunk_size()));

                    file_bytes.AddRange(OGF_V.usertdata.data_all());

                }
                else
                {
                    if (OGF_V.refs.pos > 0)
                        temp = fileStream.ReadBytes((int)(OGF_V.refs.pos - fileStream.BaseStream.Position));
                    else
                        temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

                    file_bytes.AddRange(temp);
                }               

                if (OGF_V.refs.refs0 != null)
				{
					if (OGF_V.refs.refs0.Count() > 1)
					{
						file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS_1));
						file_bytes.AddRange(BitConverter.GetBytes(OGF_V.refs.chunk_size()));
						file_bytes.AddRange(OGF_V.refs.count());
					}
					else
					{
						file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS_0));
						file_bytes.AddRange(BitConverter.GetBytes(OGF_V.refs.chunk_size()));
					}

					file_bytes.AddRange(OGF_V.refs.data());
				}
			}

			using (var fileStream = new FileStream(filename, FileMode.Truncate))
			{
				byte[] data = file_bytes.ToArray();
				fileStream.Write(data, 0, data.Length);
			}
		}

		private void TextBoxFilter(object sender, EventArgs e)
		{

			TextBox curBox = sender as TextBox;

			if (bKeyIsDown)
			{
				string mask = @"^[A-Za-z0-9_$]*$";
				Match match = Regex.Match(curBox.Text, mask);
				if (!match.Success)
				{
					int temp = curBox.SelectionStart;
					curBox.Text = curBox.Text.Remove(curBox.SelectionStart - 1, 1);
					curBox.SelectionStart = temp - 1;
				}
			}

			bKeyIsDown = false;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (currentField)
			{
				case "textureBox": OGF_V.childs[idx].m_texture = curBox.Text; break;
				case "shaderBox": OGF_V.childs[idx].m_shader = curBox.Text; break;

			}
		}
		private void OpenFile(string filename)
		{
			var xr_loader = new XRayLoader();

			OGF_V = new OGF_Children();

            Current_OGF = File.ReadAllBytes(filename);


            using (var r = new BinaryReader(new MemoryStream(Current_OGF)))
			{

                
				xr_loader.SetStream(r.BaseStream);

				uint size = xr_loader.find_chunkSize((int)OGF.OGF_HEADER);

				xr_loader.ReadByte(); // Skip m_version
				byte model_type = xr_loader.ReadByte();

				xr_loader.ReadBytes((int)size - 2);

				//MessageBox.Show("Model type is: "+ model_type);

				if (!xr_loader.find_chunk((int)OGF.OGF4_S_DESC)) return;

				string m_source = xr_loader.read_stringZ();
				string m_export_tool = xr_loader.read_stringZ();
				uint m_export_time = xr_loader.ReadUInt32();
				string m_owner_name = xr_loader.read_stringZ();
				uint m_creation_time = xr_loader.ReadUInt32();
				string m_exportm_modif_name_tool = xr_loader.read_stringZ();
				uint m_modified_time = xr_loader.ReadUInt32();
				System.DateTime dt_e = new System.DateTime(1970, 1, 1).AddSeconds(m_export_time);
				System.DateTime dt_c = new System.DateTime(1970, 1, 1).AddSeconds(m_creation_time);
				System.DateTime dt_m = new System.DateTime(1970, 1, 1).AddSeconds(m_modified_time);

                SourceTBox.Text = m_source;
                UserTBox.Text = m_export_tool;
                OwnerTBox.Text = m_owner_name;
                LastExpTBox.Text = m_exportm_modif_name_tool;

                maskedTextBox1.Text = dt_e.ToShortDateString();
                maskedTextBox2.Text = dt_c.ToShortDateString();
                maskedTextBox3.Text = dt_m.ToShortDateString();

                //MessageBox.Show("Source: " + m_source + "\n" +
                //	"Export Tool: " + m_export_tool + "\n" +
                //	"Owner Name: " + m_owner_name + "\n" +
                //	"Export modifed Tool:" + m_exportm_modif_name_tool);
                //MessageBox.Show("Export Time: " + dt_e +"\n" + 
                //	"Creation Time: " + dt_c + "\n"+
                //	"Modified Time: " + dt_m);



                if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_CHILDREN, false, true))) return;

				OGF_V.pos = xr_loader.chunk_pos;

				int id = 0;

				while (true)
				{

					if (!xr_loader.find_chunk(id)) break;

					Stream temp = xr_loader.reader.BaseStream;

					if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

					long pos = xr_loader.chunk_pos + OGF_V.pos + 16;
					size = xr_loader.find_chunkSize((int)OGF.OGF4_TEXTURE);

					if (size == 0) break;

					OGF_Child chld = new OGF_Child(xr_loader.chunk_pos + pos, id, pos - 8, (int)size, xr_loader.read_stringZ(), xr_loader.read_stringZ());
					OGF_V.childs.Add(chld);

					id++;
					xr_loader.SetStream(temp);

				}

				xr_loader.SetStream(r.BaseStream);

                if (xr_loader.find_chunk((int)OGF.OGF4_S_USERDATA, false, true))
                {
                    OGF_V.usertdata = new UserData();
                    OGF_V.usertdata.pos = xr_loader.chunk_pos;
                    OGF_V.usertdata.data = xr_loader.read_stringZ();
                    OGF_V.usertdata.old_size = (uint)OGF_V.usertdata.data.Length + 1;
                }

                bool v3 = false;

                xr_loader.SetStream(r.BaseStream);

                if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(14, reset: true)))
                {
                    richTextBox3.Clear();
                    int chunkId = 0;
                    while (xr_loader.find_chunk(chunkId))
                    {
                        Stream baseStream = xr_loader.reader.BaseStream;
                        if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(chunkId, reset: true)))
                        {
                            if (chunkId == 0)
                                richTextBox3.Text += string.Format("Motions count : {0}\n", (object)xr_loader.ReadUInt32());
                            else
                                richTextBox3.Text += string.Format("\n{0}. {1}", (object)chunkId, (object)xr_loader.read_stringZ());
                            ++chunkId;
                            xr_loader.SetStream(baseStream);
                        }
                        else
                            break;
                    }
                }

                if (v3 = !xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS_1, false, true))
					if (!xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS_0, false, true))
						return;

				OGF_V.refs.pos = xr_loader.chunk_pos;
				OGF_V.refs.refs0 = new List<string>();

				if (v3) { OGF_V.refs.refs0.Add(xr_loader.read_stringZ()); }
                else
                {
                    uint count = xr_loader.ReadUInt32();

                    for (int i = 0; i < count; i++)
                        OGF_V.refs.refs0.Add(xr_loader.read_stringZ());
                }
            }
		}
    }
}
