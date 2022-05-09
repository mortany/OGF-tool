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
using System.Runtime.InteropServices;

namespace OGF_tool
{
	public partial class OGF_Editor : Form
	{
		// About file
		byte m_version;
		byte m_model_type;

		// File sytem
		public OGF_Children OGF_V = null;
		public byte[] Current_OGF = null;
		public List<byte> file_bytes = new List<byte>();
		public string FILE_NAME = "";

		// Input
		public bool bKeyIsDown = false;

		// Elements Size
		int FormHeight = 0;
		int BoxesHeight = 0;
		int TabControlHeight = 0;

		[DllImport("converter.dll")]
		private static extern int CSharpStartAgent(string path, string out_path, int mode, int convert_to_mode, string motion_list);

		private int RunConverter(string path, string out_path, int mode, int convert_to_mode)
		{
			string dll_path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\converter.dll";
			if (File.Exists(dll_path))
				return CSharpStartAgent(path, out_path, mode, convert_to_mode, "");
			else
			{
				MessageBox.Show("Can't find converter.dll", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return -1;
			}
		}

		public OGF_Editor()
		{
			InitializeComponent();
			StartPosition = FormStartPosition.CenterScreen;

			FormHeight = Height;
			BoxesHeight = MotionRefsBox.Height;
			TabControlHeight = TabControl.Height;

			openFileDialog1.Filter = "OGF file|*.ogf";

			saveFileDialog1.Filter = "OGF file|*.ogf|Object file|*.object|Bones file|*.bones|Skl file|*.skl|Skls file|*.skls";

			if (Environment.GetCommandLineArgs().Length > 1)
			{
				Clear();
				OpenFile(Environment.GetCommandLineArgs()[1]);
				FILE_NAME = Environment.GetCommandLineArgs()[1];
				AfterLoad();
			}
			else
            {
				TabControl.Controls.Clear();
			}
		}

		private void CreateTextureGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 126 * idx);
			GroupBox.Size = new System.Drawing.Size(366, 126);
			GroupBox.Text = "Set: [" + idx + "]";
			GroupBox.Name = "TextureGrpBox_" + idx;
			CreateTextureBoxes(idx, GroupBox);
			CreateTextureLabels(idx, GroupBox);
			TexturesPage.Controls.Add(GroupBox);
		}

		private void CreateTextureBoxes(int idx, GroupBox box)
		{
			var newTextBox = new TextBox();
			newTextBox.Name = "textureBox_" + idx;
			newTextBox.Size = new System.Drawing.Size(355, 23);
			newTextBox.Location = new System.Drawing.Point(6, 39);
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);

			var newTextBox2 = new TextBox();
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.Size = new System.Drawing.Size(355, 23);
			newTextBox2.Location = new System.Drawing.Point(6, 88);
			newTextBox2.TextChanged += new System.EventHandler(this.TextBoxFilter);
			box.Controls.Add(newTextBox);
			box.Controls.Add(newTextBox2);
		}

		private void CreateTextureLabels(int idx, GroupBox box)
		{
			var newLbl = new Label();
			newLbl.Name = "textureLbl_" + idx;
			newLbl.Text = "Texture Path:";
			newLbl.Location = new System.Drawing.Point(6, 21);

			var newLbl2 = new Label();
			newLbl2.Name = "shaderLbl_" + idx;
			newLbl2.Text = "Shader Name:";
			newLbl2.Location = new System.Drawing.Point(6, 70);

			box.Controls.Add(newLbl);
			box.Controls.Add(newLbl2);
		}

		private void CreateBoneGroupBox(int idx, string bone_name, string parent_bone_name, string material, float mass)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 85 * idx);
			GroupBox.Size = new System.Drawing.Size(366, 83);
			GroupBox.Text = "Bone id: [" + idx + "]";
			GroupBox.Name = "BoneGrpBox_" + idx;

			CreateBoneTextBox(idx, GroupBox, bone_name, parent_bone_name, material, mass);
			BoneParamsPage.Controls.Add(GroupBox);
		}

		private void CreateBoneTextBox(int idx, GroupBox box, string bone_name, string parent_bone_name, string material, float mass)
		{
			var newTextBox = new RichTextBox();
			newTextBox.Name = "boneBox_" + idx;
			newTextBox.Size = new System.Drawing.Size(355, 58);
			newTextBox.Location = new System.Drawing.Point(6, 18);
			newTextBox.ReadOnly = true;

			newTextBox.MouseWheel += (s, e) =>
			{
				int sc = 0;
				if (e.Delta < 0) sc = BoneParamsPage.ClientSize.Height + 120;
				else sc = -120;
				using (Control c = new Control() { Parent = BoneParamsPage, Height = 1, Top = sc }){BoneParamsPage.ScrollControlIntoView(c);}
			};

			newTextBox.Text = $"Bone name: {bone_name}\n";
			if (parent_bone_name != "")
				newTextBox.Text += $"Parent bone name: {parent_bone_name}\n";
			newTextBox.Text += $"Material: {material}\nMass: {mass}";

			box.Controls.Add(newTextBox);
		}

		private void RecalcFormSize()
        {
			int set_size = 125;
			Height = FormHeight;
			MotionRefsBox.Height = BoxesHeight;
			CustomDataBox.Height = BoxesHeight;
			BoneNamesBox.Height = BoxesHeight;
			MotionBox.Height = BoxesHeight;
			TabControl.Height = TabControlHeight;

			if (OGF_V.childs.Count <= 1)
			{
				Height -= set_size;
				MotionRefsBox.Height -= set_size;
				CustomDataBox.Height -= set_size;
				BoneNamesBox.Height -= set_size;
				MotionBox.Height -= set_size;
				TabControl.Height -= set_size;
			}
			else if (OGF_V.childs.Count >= 3)
			{
				Height += set_size;
				MotionRefsBox.Height += set_size;
				CustomDataBox.Height += set_size;
				BoneNamesBox.Height += set_size;
				MotionBox.Height += set_size;
				TabControl.Height += set_size;
			}
		}

		private void Clear()
		{
			FILE_NAME = "";
			OGF_V = null;
			file_bytes.Clear();
			TexturesPage.Controls.Clear();
			BoneParamsPage.Controls.Clear();
			TabControl.Controls.Clear();
			MotionRefsBox.Clear();
			BoneNamesBox.Clear();
		}

		private void AfterLoad()
		{
			RecalcFormSize();

			for (int i = 0; i < OGF_V.childs.Count; i++)
			{
				CreateTextureGroupBox(i);

				var box = TexturesPage.Controls["TextureGrpBox_" + i];

				if (box != null)
				{
					var Cntrl = box.Controls["textureBox_" + i];
					Cntrl.Text = OGF_V.childs[i].m_texture;
					var Cntrl2 = box.Controls["shaderBox_" + i];
					Cntrl2.Text = OGF_V.childs[i].m_shader;
				}
			}

			for (int i = 0; i < OGF_V.bones.bones.Count; i++)
			{
				CreateBoneGroupBox(i, OGF_V.bones.bones[i], OGF_V.bones.parent_bones[i], OGF_V.bones.materials[i], OGF_V.bones.mass[i]);
			}

			MotionRefsBox.Clear();
			CustomDataBox.Clear();

			if (OGF_V.refs.refs0 != null)
				MotionRefsBox.Lines = OGF_V.refs.refs0.ToArray();

			if (OGF_V.usertdata != null)
				CustomDataBox.Text = OGF_V.usertdata.data;
		}

		private void CopyParams()
		{
			if (MotionRefsBox.Lines.Count() > 0)
			{
				OGF_V.refs.refs0 = new List<string>();
				OGF_V.refs.refs0.AddRange(MotionRefsBox.Lines);
			}
			else
			{
				OGF_V.refs.refs0 = null;
			}

			if (CustomDataBox.Text != "" && OGF_V.usertdata != null)
			{
				OGF_V.usertdata.data = "";

				for (int i = 0; i < CustomDataBox.Lines.Count(); i++)
				{
					string ext = i == CustomDataBox.Lines.Count() - 1 ? "" : "\r\n";
					OGF_V.usertdata.data += CustomDataBox.Lines[i] + ext;
				}
			}
		}

		private void SaveFile(string filename)
		{
			file_bytes.Clear();

			if (Current_OGF == null) return;

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

                //if (OGF_V.bones.bones != null)
                //{
                //    if (OGF_V.bones.pos > 0)
                //        temp = fileStream.ReadBytes((int)(OGF_V.bones.pos - fileStream.BaseStream.Position));
                //    else
                //        temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

                //    file_bytes.AddRange(temp);

                //    file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_BONE_NAMES));
                //    file_bytes.AddRange(BitConverter.GetBytes(OGF_V.bones.chunk_size()));

                //    file_bytes.AddRange(OGF_V.bones.count());
                //    file_bytes.AddRange(OGF_V.bones.data());
                //}

                if (OGF_V.usertdata != null)
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

			if (BkpCheckBox.Checked)
			{
				string backup_path = filename + ".bak";

				if (File.Exists(backup_path))
				{
					FileInfo backup_file = new FileInfo(backup_path);
					backup_file.Delete();
				}

				FileInfo file = new FileInfo(filename);
				file.CopyTo(backup_path);
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
				StatusFile.Text = filename.Substring(filename.LastIndexOf('\\') + 1);

				xr_loader.SetStream(r.BaseStream);

				uint size = xr_loader.find_chunkSize((int)OGF.OGF_HEADER);

				m_version = xr_loader.ReadByte();
				m_model_type = xr_loader.ReadByte();

				xr_loader.ReadBytes((int)size - 2);

				if (!xr_loader.find_chunk((int)OGF.OGF4_S_DESC)) return;

				OGF_V.descr = new Description();

				OGF_V.descr.pos = xr_loader.chunk_pos;

				OGF_V.descr.m_source = xr_loader.read_stringZ();
				OGF_V.descr.m_export_tool = xr_loader.read_stringZ();
				OGF_V.descr.m_export_time = xr_loader.ReadUInt32();
				OGF_V.descr.m_owner_name = xr_loader.read_stringZ();
				OGF_V.descr.m_creation_time = xr_loader.ReadUInt32();
				OGF_V.descr.m_export_modif_name_tool = xr_loader.read_stringZ();
				OGF_V.descr.m_modified_time = xr_loader.ReadUInt32();

				xr_loader.SetStream(r.BaseStream);

				if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_CHILDREN, false, true))) return;

				TabControl.Controls.Add(TexturesPage);

				OGF_V.pos = xr_loader.chunk_pos;

				int id = 0;

				// Texture && shader
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

				// Userdata
				if (xr_loader.find_chunk((int)OGF.OGF4_S_USERDATA, false, true))
				{
					TabControl.Controls.Add(CustomDataPage);

					OGF_V.usertdata = new UserData();
					OGF_V.usertdata.pos = xr_loader.chunk_pos;
					OGF_V.usertdata.data = xr_loader.read_stringZ();
					OGF_V.usertdata.old_size = (uint)OGF_V.usertdata.data.Length+1;
				}

				xr_loader.SetStream(r.BaseStream);

				// Motion Refs
				bool v3 = xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS_0, false, true);

				if (v3 || xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS_1, false, true))
				{
					TabControl.Controls.Add(MotionRefsPage);

					OGF_V.refs.pos = xr_loader.chunk_pos;
					OGF_V.refs.refs0 = new List<string>();

					if (v3)
						OGF_V.refs.refs0.Add(xr_loader.read_stringZ());
					else
					{
						uint count = xr_loader.ReadUInt32();

						for (int i = 0; i < count; i++)
							OGF_V.refs.refs0.Add(xr_loader.read_stringZ());
					}
				}

				xr_loader.SetStream(r.BaseStream);

				//Motions
				if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
				{
					MotionBox.Clear();

					TabControl.Controls.Add(MotionPage);

					id = 0;

					while (true)
					{
						if (!xr_loader.find_chunk(id)) break;

						Stream temp = xr_loader.reader.BaseStream;

						if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

						if (id == 0)
							MotionBox.Text += $"Motions count : {xr_loader.ReadUInt32()}\n";
						else
							MotionBox.Text += $"\n{id}. {xr_loader.read_stringZ()}";

						id++;
						xr_loader.SetStream(temp);
					}
				}

				xr_loader.SetStream(r.BaseStream);

				// Bones
				if (xr_loader.find_chunk((int)OGF.OGF4_S_BONE_NAMES, false, true))
				{
					OGF_V.bones.pos = xr_loader.chunk_pos;
					OGF_V.bones.bones = new List<string>();
					OGF_V.bones.parent_bones = new List<string>();
					OGF_V.bones.fobb = new List<byte[]>();

					BoneNamesBox.Clear();
					TabControl.Controls.Add(BoneNamesPage);
					TabControl.Controls.Add(BoneParamsPage);

					uint count = xr_loader.ReadUInt32();
					uint count_saved = count;

					BoneNamesBox.Text += $"Bones count : {count}\n\n";

					for (; count != 0; count--)
					{
						string bone_name = xr_loader.read_stringZ();
						string parent_name = xr_loader.read_stringZ();
						byte[] obb = xr_loader.ReadBytes(60);    // Fobb
						BoneNamesBox.Text += $"{count_saved - count + 1}. {bone_name}";

						if (count != 1)
							BoneNamesBox.Text += "\n";

						OGF_V.bones.bones.Add(bone_name);
						OGF_V.bones.parent_bones.Add(parent_name);
						OGF_V.bones.fobb.Add(obb);
					}
				}

				xr_loader.SetStream(r.BaseStream);

                // Ik Data
                if (xr_loader.find_chunk((int)OGF.OGF4_S_IKDATA, false, true))
                {
					OGF_V.bones.materials = new List<string>();
					OGF_V.bones.mass = new List<float>();

					for (int i = 0; i < OGF_V.bones.bones.Count; i++)
                    {
                        uint version = xr_loader.ReadUInt32();
                        string gmtl_name = xr_loader.read_stringZ();

						xr_loader.ReadBytes(112);   // struct SBoneShape

                        // Import
                        {
                            xr_loader.ReadBytes(4);
                            xr_loader.ReadBytes(16 * 3);
                            xr_loader.ReadBytes(4);
                            xr_loader.ReadBytes(4);
                            xr_loader.ReadBytes(4);
                            xr_loader.ReadBytes(4);
                            xr_loader.ReadBytes(4);

                            if (version > 0)
                                xr_loader.ReadBytes(4);
                        }

                        xr_loader.ReadBytes(12);    // vXYZ
                        xr_loader.ReadBytes(12);    // vT
                        float mass = xr_loader.ReadFloat();
                        xr_loader.ReadBytes(12);    // Center of mass

						OGF_V.bones.materials.Add(gmtl_name);
						OGF_V.bones.mass.Add(mass);
					}
                }
			}
		}

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (FILE_NAME == "") return;

			CopyParams();
			SaveFile(FILE_NAME);
			AutoClosingMessageBox.Show("Saved!", "", 500, MessageBoxIcon.Information);
		}

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.Control && e.KeyCode == Keys.S)
				saveToolStripMenuItem_Click(null, null);

			switch (e.KeyData)
			{
				case Keys.F4: loadToolStripMenuItem_Click(null, null); break;
				case Keys.F5: saveToolStripMenuItem_Click(null, null); break;
				case Keys.F6: saveAsToolStripMenuItem_Click(null, null); break;
			}
		}

        private void oGFInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (FILE_NAME == "")
			{
				AutoClosingMessageBox.Show("Please, open the file!", "", 900, MessageBoxIcon.Information);
				return;
			}

			System.DateTime dt_e = new System.DateTime(1970, 1, 1).AddSeconds(OGF_V.descr.m_export_time);
			System.DateTime dt_c = new System.DateTime(1970, 1, 1).AddSeconds(OGF_V.descr.m_creation_time);
			System.DateTime dt_m = new System.DateTime(1970, 1, 1).AddSeconds(OGF_V.descr.m_modified_time);
			MessageBox.Show(
				$"OGF Version: {m_version}\n" +
				$"Model type: {(m_model_type == 3 ? "Animated" : "Rigid")}\n\n" +
				$"Source File: {OGF_V.descr.m_source}\n" +
				$"Export Owner: {OGF_V.descr.m_export_tool}\n" +
				$"Model Owner: {OGF_V.descr.m_owner_name}\n" +
				$"Modifed Model Owner: {OGF_V.descr.m_export_modif_name_tool}\n\n" +
				$"Export Time: {dt_e.ToShortDateString()}\n" +
				$"Creation Time: {dt_c.ToShortDateString()}\n" +
				$"Modified Time: {dt_m.ToShortDateString()}", "OGF Info:", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (FILE_NAME == "" || OGF_V.descr == null)
			{
				AutoClosingMessageBox.Show("Please, open the file!", "", 900, MessageBoxIcon.Information);
				return;
			}

			saveFileDialog1.FileName = "";
			saveFileDialog1.ShowDialog();
		}

		private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
			string Filename = (sender as SaveFileDialog).FileName;

			string format = Path.GetExtension(Filename);

			if (File.Exists(Filename))
			{
				FileInfo backup_file = new FileInfo(Filename);
				backup_file.Delete();
			}

			if (format == ".ogf")
			{
				FileInfo file = new FileInfo(FILE_NAME);
				file.CopyTo(Filename);

				CopyParams();
				SaveFile((sender as SaveFileDialog).FileName);
			}
			else if (format == ".object")
			{
				if (File.Exists(Filename + ".ogf"))
				{
					FileInfo backup_file = new FileInfo(Filename + ".ogf");
					backup_file.Delete();
				}

				FileInfo file = new FileInfo(FILE_NAME);
				file.CopyTo(Filename + ".ogf");

				CopyParams();
				SaveFile((sender as SaveFileDialog).FileName + ".ogf");

				RunConverter(Filename + ".ogf", Filename, 0, 0);

				if (File.Exists(Filename + ".ogf"))
				{
					FileInfo backup_file = new FileInfo(Filename + ".ogf");
					backup_file.Delete();
				}
			}
			else if (format == ".bones")
				RunConverter(FILE_NAME, Filename, 0, 1);
			else if (format == ".skl")
				RunConverter(FILE_NAME, Filename, 0, 2);
			else if (format == ".skls")
				RunConverter(FILE_NAME, Filename, 0, 3);
			AutoClosingMessageBox.Show("Saved!", "", 500, MessageBoxIcon.Information);
		}

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (MessageBox.Show("Are you sure you want to exit?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				Close();
		}
    }
}
