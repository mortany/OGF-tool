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
		byte m_model_type; // 3 - Animated, 10 - Rigid

		// File sytem
		public OGF_Children OGF_V = null;
		public byte[] Current_OGF = null;
		public byte[] Current_OMF = null;
		public bool IsModelBroken = false;
		public List<byte> file_bytes = new List<byte>();
		public string FILE_NAME = "";

		// Input
		public bool bKeyIsDown = false;
		string number_mask = "";

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

			number_mask = @"^-[0-9.]*$";
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			oGFInfoToolStripMenuItem.Enabled = false;
			viewToolStripMenuItem.Enabled = false;
			SaveMenuParam.Enabled = false;
			saveAsToolStripMenuItem.Enabled = false;
			MotionToolsMenuItem.Enabled = false;

			if (Environment.GetCommandLineArgs().Length > 1)
			{
				Clear();
				if (OpenFile(Environment.GetCommandLineArgs()[1]))
				{
					FILE_NAME = Environment.GetCommandLineArgs()[1];
					AfterLoad();
				}
			}
			else
            {
				TabControl.Controls.Clear();
			}

			if (Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.Delete(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp", true);
		}

		private void CreateTextureGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 126 * idx);
			GroupBox.Size = new System.Drawing.Size(385, 126);
			GroupBox.Text = "Set: [" + idx + "]";
			GroupBox.Name = "TextureGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
			CreateTextureBoxes(idx, GroupBox);
			CreateTextureLabels(idx, GroupBox);
			TexturesPage.Controls.Add(GroupBox);
		}

		private void CreateTextureBoxes(int idx, GroupBox box)
		{
			var newTextBox = new TextBox();
			newTextBox.Name = "textureBox_" + idx;
			newTextBox.Size = new System.Drawing.Size(373, 23);
			newTextBox.Location = new System.Drawing.Point(6, 39);
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);
			newTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var newTextBox2 = new TextBox();
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.Size = new System.Drawing.Size(373, 23);
			newTextBox2.Location = new System.Drawing.Point(6, 88);
			newTextBox2.TextChanged += new System.EventHandler(this.TextBoxFilter);
			newTextBox2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
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

		private void CreateBoneGroupBox(int idx, string bone_name, string parent_bone_name, string material, float mass, Fvector center, Fvector pos, Fvector rot)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 205 * idx);
			GroupBox.Size = new System.Drawing.Size(385, 203);
			GroupBox.Text = "Bone id: [" + idx + "]";
			GroupBox.Name = "BoneGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

			CreateBoneTextBox(idx, GroupBox, bone_name, parent_bone_name, material, mass, center, pos, rot);
			BoneParamsPage.Controls.Add(GroupBox);
		}

		private void CreateBoneTextBox(int idx, GroupBox box, string bone_name, string parent_bone_name, string material, float mass, Fvector center, Fvector pos, Fvector rot)
		{
			var BoneNameTextBox = new TextBox();
			BoneNameTextBox.Name = "boneBox_" + idx;
			BoneNameTextBox.Size = new System.Drawing.Size(290, 58);
			BoneNameTextBox.Location = new System.Drawing.Point(86, 18);
			BoneNameTextBox.Text = bone_name;
			BoneNameTextBox.Tag = "string";
			BoneNameTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			BoneNameTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			BoneNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var BoneNameLabel = new Label();
			BoneNameLabel.Name = "boneLabel_" + idx;
			BoneNameLabel.Size = new System.Drawing.Size(100, 20);
			BoneNameLabel.Location = new System.Drawing.Point(6, 20);
			BoneNameLabel.Text = "Bone Name:";

			var ParentBoneNameTextBox = new TextBox();
			ParentBoneNameTextBox.Name = "ParentboneBox_" + idx;
			ParentBoneNameTextBox.Size = new System.Drawing.Size(290, 58);
			ParentBoneNameTextBox.Location = new System.Drawing.Point(86, 45);
			ParentBoneNameTextBox.Text = parent_bone_name;
			ParentBoneNameTextBox.Tag = "string";
			ParentBoneNameTextBox.ReadOnly = true;
			ParentBoneNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var ParentBoneNameLabel = new Label();
			ParentBoneNameLabel.Name = "ParentboneLabel_" + idx;
			ParentBoneNameLabel.Size = new System.Drawing.Size(100, 20);
			ParentBoneNameLabel.Location = new System.Drawing.Point(6, 47);
			ParentBoneNameLabel.Text = "Parent Bone:";

			var MaterialTextBox = new TextBox();
			MaterialTextBox.Name = "MaterialBox_" + idx;
			MaterialTextBox.Size = new System.Drawing.Size(290, 58);
			MaterialTextBox.Location = new System.Drawing.Point(86, 72);
			MaterialTextBox.Text = material;
			MaterialTextBox.Tag = "string";
			MaterialTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			MaterialTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			MaterialTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var MaterialLabel = new Label();
			MaterialLabel.Name = "MaterialLabel_" + idx;
			MaterialLabel.Size = new System.Drawing.Size(100, 20);
			MaterialLabel.Location = new System.Drawing.Point(6, 74);
			MaterialLabel.Text = "Material:";

			var MassTextBox = new TextBox();
			MassTextBox.Name = "MassBox_" + idx;
			MassTextBox.Size = new System.Drawing.Size(84, 58);
			MassTextBox.Location = new System.Drawing.Point(86, 99);
			MassTextBox.Text = mass.ToString();
			MassTextBox.Tag = "float";
			MassTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			MassTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var MassLabel = new Label();
			MassLabel.Name = "MassLabel_" + idx;
			MassLabel.Size = new System.Drawing.Size(100, 20);
			MassLabel.Location = new System.Drawing.Point(6, 101);
			MassLabel.Text = "Mass:";

			var CenterMassTextBoxX = new TextBox();
			CenterMassTextBoxX.Name = "CenterBoxX_" + idx;
			CenterMassTextBoxX.Size = new System.Drawing.Size(84, 58);
			CenterMassTextBoxX.Location = new System.Drawing.Point(86, 125);
			CenterMassTextBoxX.Text = center.x.ToString();
			CenterMassTextBoxX.Tag = "float";
			CenterMassTextBoxX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxY = new TextBox();
			CenterMassTextBoxY.Name = "CenterBoxY_" + idx;
			CenterMassTextBoxY.Size = new System.Drawing.Size(84, 58);
			CenterMassTextBoxY.Location = new System.Drawing.Point(182, 125);
			CenterMassTextBoxY.Text = center.y.ToString();
			CenterMassTextBoxY.Tag = "float";
			CenterMassTextBoxY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxZ = new TextBox();
			CenterMassTextBoxZ.Name = "CenterBoxZ_" + idx;
			CenterMassTextBoxZ.Size = new System.Drawing.Size(84, 58);
			CenterMassTextBoxZ.Location = new System.Drawing.Point(277, 125);
			CenterMassTextBoxZ.Text = center.z.ToString();
			CenterMassTextBoxZ.Tag = "float";
			CenterMassTextBoxZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassLabel = new Label();
			CenterMassLabel.Name = "CenterMassLabel_" + idx;
			CenterMassLabel.Size = new System.Drawing.Size(100, 20);
			CenterMassLabel.Location = new System.Drawing.Point(6, 127);
			CenterMassLabel.Text = "Center of Mass:";

			var PositionX = new TextBox();
			PositionX.Name = "PositionX_" + idx;
			PositionX.Size = new System.Drawing.Size(84, 58);
			PositionX.Location = new System.Drawing.Point(86, 151);
			PositionX.Text = pos.x.ToString();
			PositionX.Tag = "float";
			PositionX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionY = new TextBox();
			PositionY.Name = "PositionY_" + idx;
			PositionY.Size = new System.Drawing.Size(84, 58);
			PositionY.Location = new System.Drawing.Point(182, 151);
			PositionY.Text = pos.y.ToString();
			PositionY.Tag = "float";
			PositionY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionZ = new TextBox();
			PositionZ.Name = "PositionZ_" + idx;
			PositionZ.Size = new System.Drawing.Size(84, 58);
			PositionZ.Location = new System.Drawing.Point(277, 151);
			PositionZ.Text = pos.z.ToString();
			PositionZ.Tag = "float";
			PositionZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionLabel = new Label();
			PositionLabel.Name = "PositionLabel_" + idx;
			PositionLabel.Size = new System.Drawing.Size(100, 20);
			PositionLabel.Location = new System.Drawing.Point(6, 153);
			PositionLabel.Text = "Position:";

			var RotationX = new TextBox();
			RotationX.Name = "RotationX_" + idx;
			RotationX.Size = new System.Drawing.Size(84, 58);
			RotationX.Location = new System.Drawing.Point(86, 177);
			RotationX.Text = rot.x.ToString();
			RotationX.Tag = "float";
			RotationX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationY = new TextBox();
			RotationY.Name = "RotationY_" + idx;
			RotationY.Size = new System.Drawing.Size(84, 58);
			RotationY.Location = new System.Drawing.Point(182, 177);
			RotationY.Text = rot.y.ToString();
			RotationY.Tag = "float";
			RotationY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationZ = new TextBox();
			RotationZ.Name = "RotationZ_" + idx;
			RotationZ.Size = new System.Drawing.Size(84, 58);
			RotationZ.Location = new System.Drawing.Point(277, 177);
			RotationZ.Text = rot.z.ToString();
			RotationZ.Tag = "float";
			RotationZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationLabel = new Label();
			RotationLabel.Name = "RotationLabel_" + idx;
			RotationLabel.Size = new System.Drawing.Size(100, 20);
			RotationLabel.Location = new System.Drawing.Point(6, 179);
			RotationLabel.Text = "Rotation:";

			box.Controls.Add(BoneNameTextBox);
			box.Controls.Add(ParentBoneNameTextBox);
			box.Controls.Add(MaterialTextBox);
			box.Controls.Add(MassTextBox);
			box.Controls.Add(CenterMassTextBoxX);
			box.Controls.Add(CenterMassTextBoxY);
			box.Controls.Add(CenterMassTextBoxZ);
			box.Controls.Add(PositionX);
			box.Controls.Add(PositionY);
			box.Controls.Add(PositionZ);
			box.Controls.Add(RotationX);
			box.Controls.Add(RotationY);
			box.Controls.Add(RotationZ);

			box.Controls.Add(BoneNameLabel);
			box.Controls.Add(ParentBoneNameLabel);
			box.Controls.Add(MaterialLabel);
			box.Controls.Add(MassLabel);
			box.Controls.Add(CenterMassLabel);
			box.Controls.Add(PositionLabel);
			box.Controls.Add(RotationLabel);
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
			UserDataBox.Clear();
			MotionBox.Clear();
		}

		private void AfterLoad()
		{
			oGFInfoToolStripMenuItem.Enabled = true;
			viewToolStripMenuItem.Enabled = true;
			SaveMenuParam.Enabled = true;
			saveAsToolStripMenuItem.Enabled = true;

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
				CreateBoneGroupBox(i, OGF_V.bones.bones[i], OGF_V.bones.parent_bones[i], OGF_V.ikdata.materials[i], OGF_V.ikdata.mass[i], OGF_V.ikdata.center_mass[i], OGF_V.ikdata.position[i], OGF_V.ikdata.rotation[i]);
			}

			MotionRefsBox.Clear();
			UserDataBox.Clear();

			if (OGF_V.refs != null)
				MotionRefsBox.Lines = OGF_V.refs.refs0.ToArray();

			if (OGF_V.usertdata != null)
				UserDataBox.Text = OGF_V.usertdata.data;

			IsModelBroken = CatchBrokenModel();

			if (IsModelBroken)
            {
				OGF_V.descr.m_export_time = 0;
				OGF_V.descr.m_creation_time = 0;
				OGF_V.descr.m_modified_time = 0;
			}
		}

		private void CopyParams()
		{
			if (OGF_V.refs != null)
			{
				if (MotionRefsBox.Lines.Count() > 0)
				{
					OGF_V.refs.refs0 = new List<string>();
					OGF_V.refs.refs0.AddRange(MotionRefsBox.Lines);
				}
				else
				{
					OGF_V.refs.refs0 = null;
					if (MotionBox.Text == "")
						m_model_type = 10;
				}
			}

			if (OGF_V.usertdata != null)
			{
				OGF_V.usertdata.data = "";

				if (UserDataBox.Text != "")
				{
					for (int i = 0; i < UserDataBox.Lines.Count(); i++)
					{
						string ext = i == UserDataBox.Lines.Count() - 1 ? "" : "\r\n";
						OGF_V.usertdata.data += UserDataBox.Lines[i] + ext;
					}
				}
			}
		}

		public byte[] GetUserdataChunk()
        {
			byte[] chunk;

			var xr_loader = new XRayLoader();
			using (var fileStream = new BinaryWriter(File.Create("userdata.chunk")))
			{
				xr_loader.open_chunk(fileStream, (int)OGF.OGF4_S_USERDATA);
				xr_loader.write_stringZ(fileStream, OGF_V.usertdata.data);
				xr_loader.close_chunk(fileStream);
			}

			chunk = File.ReadAllBytes("userdata.chunk");
			File.Delete("userdata.chunk");

			return chunk;
        }

		public byte[] GetMotionRefsChunk()
		{
			byte[] chunk;

			var xr_loader = new XRayLoader();
			using (var fileStream = new BinaryWriter(File.Create("motion_refs.chunk")))
			{
				xr_loader.open_chunk(fileStream, (int)OGF.OGF4_S_MOTION_REFS2);
				xr_loader.write_u32(fileStream, (uint)OGF_V.refs.refs0.Count);
				for (int i = 0; i < OGF_V.refs.refs0.Count; i++)
					xr_loader.write_stringZ(fileStream, OGF_V.refs.refs0[i]);
				xr_loader.close_chunk(fileStream);
			}

			chunk = File.ReadAllBytes("motion_refs.chunk");
			File.Delete("motion_refs.chunk");

			return chunk;
		}

		public byte[] GetDescriptionChunk()
		{
			List<byte> chunk = new List<byte>();

			using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
			{
				fileStream.ReadBytes(8);
				fileStream.ReadBytes(2);
				fileStream.ReadBytes(2);
				fileStream.ReadBytes(40);

				byte[] temp = fileStream.ReadBytes((int)(OGF_V.descr.pos - fileStream.BaseStream.Position));
				chunk.AddRange(temp);

				chunk.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_DESC));
				chunk.AddRange(BitConverter.GetBytes(OGF_V.descr.chunk_size()));
				chunk.AddRange(OGF_V.descr.data());
			}

			return chunk.ToArray();
		}

		private bool CatchBrokenModel()
		{
			if (Current_OGF == null) return false;

			try
			{
				using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
				{
					fileStream.ReadBytes(8);
					fileStream.ReadBytes(2);
					fileStream.ReadBytes((int)(OGF_V.descr.pos - fileStream.BaseStream.Position));
					fileStream.ReadBytes(OGF_V.descr.old_size + 8);
					fileStream.ReadUInt32();
					fileStream.ReadUInt32();

					foreach (var ch in OGF_V.childs)
					{
						fileStream.ReadBytes((int)(ch.parent_pos - fileStream.BaseStream.Position));
						fileStream.ReadUInt32();
						fileStream.ReadUInt32();
						fileStream.ReadBytes((int)(ch.pos - fileStream.BaseStream.Position));
						fileStream.BaseStream.Position += ch.old_size + 8;
					}
				}
			}
			catch (Exception)
			{
				return true;
			}

			return false;
		}

		private void SaveFile(string filename)
		{
			file_bytes.Clear();

			if (Current_OGF == null) return;

			bool remove_motions_chunk = OGF_V.delete_motions;

			if (!remove_motions_chunk && OGF_V.refs != null && OGF_V.refs.refs0 != null && OGF_V.refs.need_create && MotionBox.Text != "" && MessageBox.Show("New motion refs chunk will remove built-in motions, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				remove_motions_chunk = true;

			using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
			{
				byte[] temp = fileStream.ReadBytes(8);
				file_bytes.AddRange(temp);

				fileStream.ReadBytes(2);

				file_bytes.Add(m_version);
				file_bytes.Add(m_model_type);

				temp = fileStream.ReadBytes(42);
				file_bytes.AddRange(temp);

				if (!IsModelBroken)
				{
					temp = fileStream.ReadBytes((int)(OGF_V.descr.pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_DESC));
					file_bytes.AddRange(BitConverter.GetBytes(OGF_V.descr.chunk_size()));
					file_bytes.AddRange(OGF_V.descr.data());

					fileStream.ReadBytes(OGF_V.descr.old_size + 8);
				}
				else
                {
					fileStream.ReadBytes((int)(OGF_V.pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(GetDescriptionChunk());
				}

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

                if (OGF_V.bones.bones != null)
                {
                    if (OGF_V.bones.pos > 0)
                        temp = fileStream.ReadBytes((int)(OGF_V.bones.pos - fileStream.BaseStream.Position));
                    else
                        temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

                    file_bytes.AddRange(temp);

                    file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_BONE_NAMES));
                    file_bytes.AddRange(BitConverter.GetBytes(OGF_V.bones.chunk_size()));

                    file_bytes.AddRange(OGF_V.bones.count());
                    file_bytes.AddRange(OGF_V.bones.data());

					fileStream.ReadBytes(OGF_V.bones.old_size + 8);
				}

				if (OGF_V.ikdata.materials != null)
				{
					if (OGF_V.ikdata.pos > 0)
						temp = fileStream.ReadBytes((int)(OGF_V.ikdata.pos - fileStream.BaseStream.Position));
					else
						temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

					file_bytes.AddRange(temp);

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_IKDATA));
					file_bytes.AddRange(BitConverter.GetBytes(OGF_V.ikdata.chunk_size()));
					file_bytes.AddRange(OGF_V.ikdata.data());

					fileStream.ReadBytes(OGF_V.ikdata.old_size + 8);
				}

				if (OGF_V.usertdata != null)
				{
					if (OGF_V.usertdata.need_create)
					{
						if (OGF_V.usertdata.data != "")
						{
							byte[] userdata = GetUserdataChunk();
							file_bytes.AddRange(userdata);
						}
					}
					else
					{
						if (OGF_V.usertdata.pos > 0)
							temp = fileStream.ReadBytes((int)(OGF_V.usertdata.pos - fileStream.BaseStream.Position));
						else
							temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

						file_bytes.AddRange(temp);

						if (OGF_V.usertdata.data == "")
						{
							fileStream.ReadBytes(4);
							fileStream.ReadBytes(OGF_V.usertdata.old_size);
							fileStream.ReadBytes(OGF_V.usertdata.old_size + 8);
						}
						else
						{
							file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_USERDATA));
							file_bytes.AddRange(BitConverter.GetBytes(OGF_V.usertdata.chunk_size()));

							file_bytes.AddRange(OGF_V.usertdata.data_all());
						}
					}
				}
				else if (OGF_V.refs != null)
                {
					if (OGF_V.refs.pos > 0)
						temp = fileStream.ReadBytes((int)(OGF_V.refs.pos - fileStream.BaseStream.Position));
					else if (!remove_motions_chunk)
						temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));

					file_bytes.AddRange(temp);
				}

				if (OGF_V.refs != null)
				{
					if (OGF_V.refs.refs0 != null)
					{
						if (OGF_V.refs.need_create)
						{
							if (MotionBox.Text == "" || remove_motions_chunk)
							{
								byte[] motionrefs = GetMotionRefsChunk();
								file_bytes.AddRange(motionrefs);
							}
						}
						else
						{
							if (OGF_V.refs.refs0.Count() > 1 || !OGF_V.refs.v3)
							{
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS2));
								file_bytes.AddRange(BitConverter.GetBytes(OGF_V.refs.chunk_size()));
								file_bytes.AddRange(OGF_V.refs.count());
							}
							else
							{
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS));
								file_bytes.AddRange(BitConverter.GetBytes(OGF_V.refs.chunk_size()));
							}

							file_bytes.AddRange(OGF_V.refs.data());
						}
					}

					fileStream.ReadBytes(4);
					fileStream.ReadBytes(OGF_V.refs.old_size);
					fileStream.ReadBytes(OGF_V.refs.old_size + 8);
				}

				if (!remove_motions_chunk)
				{
					if (Current_OMF != null)
						file_bytes.AddRange(Current_OMF);
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

		private bool OpenFile(string filename)
		{
			var xr_loader = new XRayLoader();

			OGF_V = new OGF_Children();

			Current_OGF = File.ReadAllBytes(filename);

			using (var r = new BinaryReader(new MemoryStream(Current_OGF)))
			{
				StatusFile.Text = filename.Substring(filename.LastIndexOf('\\') + 1);

				xr_loader.SetStream(r.BaseStream);

				xr_loader.find_chunk((int)OGF.OGF4_HEADER);

				m_version = xr_loader.ReadByte();
				m_model_type = xr_loader.ReadByte();

				if (!xr_loader.find_chunk((int)OGF.OGF4_S_DESC, false, true))
				{
					MessageBox.Show("Unsupported OGF format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				OGF_V.descr = new Description();

				OGF_V.descr.pos = xr_loader.chunk_pos;

				OGF_V.descr.m_source = xr_loader.read_stringZ();
				OGF_V.descr.m_export_tool = xr_loader.read_stringZ();
				OGF_V.descr.m_export_time = xr_loader.ReadInt64();
				OGF_V.descr.m_owner_name = xr_loader.read_stringZ();
				OGF_V.descr.m_creation_time = xr_loader.ReadInt64();
				OGF_V.descr.m_export_modif_name_tool = xr_loader.read_stringZ();
				OGF_V.descr.m_modified_time = xr_loader.ReadInt64();

				OGF_V.descr.old_size = OGF_V.descr.m_source.Length + 1 + OGF_V.descr.m_export_tool.Length + 1 + 8 + OGF_V.descr.m_owner_name.Length + 1 + 8 + OGF_V.descr.m_export_modif_name_tool.Length + 1 + 8;

				xr_loader.SetStream(r.BaseStream);

				if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_CHILDREN, false, true)))
				{
					MessageBox.Show("Unsupported OGF format!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				TabControl.Controls.Add(TexturesPage);

				OGF_V.pos = xr_loader.chunk_pos;

				int id = 0;
				uint size;

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
				TabControl.Controls.Add(UserDataPage);
				UserDataPage.Controls.Clear();

				UserDataPage.Controls.Add(UserDataBox);
				UserDataPage.Controls.Add(CreateUserdataButton);
				CreateUserdataButton.Visible = false;
				UserDataBox.Visible = false;

				if (xr_loader.find_chunk((int)OGF.OGF4_S_USERDATA, false, true))
				{
					UserDataBox.Visible = true;
					OGF_V.usertdata = new UserData();
					OGF_V.usertdata.pos = xr_loader.chunk_pos;
					OGF_V.usertdata.data = xr_loader.read_stringZ();
					OGF_V.usertdata.old_size = OGF_V.usertdata.data.Length + 1;
				}
				else
					CreateUserdataButton.Visible = true;

				xr_loader.SetStream(r.BaseStream);

				// Motion Refs
				TabControl.Controls.Add(MotionRefsPage);
				MotionRefsPage.Controls.Clear();

				MotionRefsPage.Controls.Add(MotionRefsBox);
				MotionRefsPage.Controls.Add(CreateMotionRefsButton);
				CreateMotionRefsButton.Visible = false;
				MotionRefsBox.Visible = false;

				bool v3 = xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS, false, true);

				if (v3 || xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS2, false, true))
				{
					MotionRefsBox.Visible = true;
					OGF_V.refs = new MotionRefs();
					OGF_V.refs.pos = xr_loader.chunk_pos;
					OGF_V.refs.refs0 = new List<string>();

					if (v3)
					{
						OGF_V.refs.v3 = true;
						string refs = xr_loader.read_stringZ();
						OGF_V.refs.refs0.Add(refs);
						OGF_V.refs.old_size = refs.Length + 1;
					}
					else
					{
						uint count = xr_loader.ReadUInt32();

						OGF_V.refs.old_size = 4;

						for (int i = 0; i < count; i++)
						{
							string refs = xr_loader.read_stringZ();
							OGF_V.refs.refs0.Add(refs);
							OGF_V.refs.old_size += refs.Length + 1;
						}
					}
				}
				else
					CreateMotionRefsButton.Visible = true;

				xr_loader.SetStream(r.BaseStream);

				MotionBox.Clear();
				TabControl.Controls.Add(MotionPage);
				Current_OMF = null;

				//Motions
				if (xr_loader.find_chunk((int)OGF.OGF4_S_MOTIONS, false, true))
				{
					xr_loader.reader.BaseStream.Position -= 8;
					Current_OMF = xr_loader.ReadBytes((int)xr_loader.reader.BaseStream.Length - (int)xr_loader.reader.BaseStream.Position);
				}

				xr_loader.SetStream(r.BaseStream);

				if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
				{
					AppendOMFButton.Visible = false;
					MotionBox.Visible = true;

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
				else
				{
					MotionBox.Visible = false;
					AppendOMFButton.Visible = true;
				}

				xr_loader.SetStream(r.BaseStream);

				// Bones
				if (xr_loader.find_chunk((int)OGF.OGF4_S_BONE_NAMES, false, true))
				{
					OGF_V.bones.pos = xr_loader.chunk_pos;
					OGF_V.bones.bones = new List<string>();
					OGF_V.bones.parent_bones = new List<string>();
					OGF_V.bones.fobb = new List<byte[]>();
					OGF_V.bones.bone_childs = new List<List<int>>();
					OGF_V.bones.old_size = 0;

					BoneNamesBox.Clear();
					TabControl.Controls.Add(BoneNamesPage);

					uint count = xr_loader.ReadUInt32();
					uint count_saved = count;
					OGF_V.bones.old_size += 4;

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

						OGF_V.bones.old_size += bone_name.Length + 1 + parent_name.Length + 1 + 60;
					}

					for (int i = 0; i < OGF_V.bones.bones.Count; i++)
                    {
						List<int> childs = new List<int>();
						for (int j = 0; j < OGF_V.bones.parent_bones.Count; j++)
                        {
							if (OGF_V.bones.parent_bones[j] == OGF_V.bones.bones[i])
                            {
								childs.Add(j);
							}
                        }
						OGF_V.bones.bone_childs.Add(childs);
					}
				}

				xr_loader.SetStream(r.BaseStream);

                // Ik Data
                if (xr_loader.find_chunk((int)OGF.OGF4_S_IKDATA, false, true))
                {
					TabControl.Controls.Add(BoneParamsPage);

					OGF_V.ikdata.pos = xr_loader.chunk_pos;
					OGF_V.ikdata.materials = new List<string>();
					OGF_V.ikdata.mass = new List<float>();
					OGF_V.ikdata.version = new List<uint>();
					OGF_V.ikdata.center_mass = new List<Fvector>();
					OGF_V.ikdata.position = new List<Fvector>();
					OGF_V.ikdata.rotation = new List<Fvector>();
					OGF_V.ikdata.bytes_1 = new List<List<byte[]>>();
					OGF_V.ikdata.old_size = 0;

					for (int i = 0; i < OGF_V.bones.bones.Count; i++)
                    {
						List<byte[]> bytes_1 = new List<byte[]>();
						OGF_V.ikdata.old_size += 4;

						byte[] temp_byte;
						uint version = xr_loader.ReadUInt32();
                        string gmtl_name = xr_loader.read_stringZ();

						temp_byte = xr_loader.ReadBytes(112);   // struct SBoneShape
						bytes_1.Add(temp_byte);
						OGF_V.ikdata.old_size += gmtl_name.Length + 1 + 112;

						// Import
						{
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(16 * 3);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);
							temp_byte = xr_loader.ReadBytes(4);
							bytes_1.Add(temp_byte);

							OGF_V.ikdata.old_size += 4 + 16 * 3 + 4 + 4 + 4 + 4 + 4;

							if (version > 0)
							{
								temp_byte = xr_loader.ReadBytes(4);
								bytes_1.Add(temp_byte);
								OGF_V.ikdata.old_size += 4;
							}
                        }

						Fvector rotation = new Fvector();
						rotation.x = xr_loader.ReadFloat();
						rotation.y = xr_loader.ReadFloat();
						rotation.z = xr_loader.ReadFloat();

						Fvector position = new Fvector();
						position.x = xr_loader.ReadFloat();
						position.y = xr_loader.ReadFloat();
						position.z = xr_loader.ReadFloat();

						float mass = xr_loader.ReadFloat();

						Fvector center = new Fvector();
						center.x = xr_loader.ReadFloat();
						center.y = xr_loader.ReadFloat();
						center.z = xr_loader.ReadFloat();

						OGF_V.ikdata.old_size += 12 + 12 + 4 + 12;

						OGF_V.ikdata.materials.Add(gmtl_name);
						OGF_V.ikdata.mass.Add(mass);
						OGF_V.ikdata.version.Add(version);
						OGF_V.ikdata.center_mass.Add(center);
						OGF_V.ikdata.bytes_1.Add(bytes_1);
						OGF_V.ikdata.position.Add(position);
						OGF_V.ikdata.rotation.Add(rotation);
					}
                }
			}
			return true;
		}

		private void TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			bKeyIsDown = true;
		}

		private void TextBoxFilter(object sender, EventArgs e)
		{
			TextBox curBox = sender as TextBox;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (currentField)
			{
				case "textureBox": OGF_V.childs[idx].m_texture = curBox.Text; break;
				case "shaderBox": OGF_V.childs[idx].m_shader = curBox.Text; break;
			}
		}

		private void TextBoxBonesFilter(object sender, EventArgs e)
		{
			TextBox curBox = sender as TextBox;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (curBox.Tag.ToString())
			{
				case "float":
					{
						if (bKeyIsDown)
						{
							if (curBox.Text.Length == 0)
								return;

							int temp = curBox.SelectionStart;
							string mask = number_mask;
							Regex.Match(curBox.Text, mask);

                            try
							{
								Convert.ToSingle(curBox.Text);
							}
							catch (Exception)
							{
								switch (currentField)
								{
									case "MassBox": curBox.Text = OGF_V.ikdata.mass[idx].ToString(); break;
									case "CenterBoxX": curBox.Text = OGF_V.ikdata.center_mass[idx].x.ToString(); break;
									case "CenterBoxY": curBox.Text = OGF_V.ikdata.center_mass[idx].y.ToString(); break;
									case "CenterBoxZ": curBox.Text = OGF_V.ikdata.center_mass[idx].z.ToString(); break;
									case "PositionX": curBox.Text = OGF_V.ikdata.position[idx].x.ToString(); break;
									case "PositionY": curBox.Text = OGF_V.ikdata.position[idx].y.ToString(); break;
									case "PositionZ": curBox.Text = OGF_V.ikdata.position[idx].z.ToString(); break;
									case "RotationX": curBox.Text = OGF_V.ikdata.rotation[idx].x.ToString(); break;
									case "RotationY": curBox.Text = OGF_V.ikdata.rotation[idx].y.ToString(); break;
									case "RotationZ": curBox.Text = OGF_V.ikdata.rotation[idx].z.ToString(); break;
								}

								if (curBox.SelectionStart < 1)
									curBox.SelectionStart = curBox.Text.Length;

								curBox.SelectionStart = temp - 1;
							}
						}
					}
					break;
			}

			Fvector vec = new Fvector();
			switch (currentField)
			{
				case "boneBox":
					{
						string old_name = OGF_V.bones.bones[idx];
						OGF_V.bones.bones[idx] = curBox.Text;

						for (int i = 0; i < OGF_V.bones.parent_bones.Count; i++)
                        {
							for (int j = 0; j < OGF_V.bones.bone_childs[idx].Count; j++)
                            {
								if (OGF_V.bones.bone_childs[idx][j] == i)
								{
									var MainGroup = BoneParamsPage.Controls[i];
									OGF_V.bones.parent_bones[i] = curBox.Text;
									MainGroup.Controls[1].Text = OGF_V.bones.parent_bones[i];
								}
							}
                        }

						BoneNamesBox.Clear();
						BoneNamesBox.Text += $"Bones count : {OGF_V.bones.bones.Count}\n\n";

						for (int i = 0; i < OGF_V.bones.bones.Count; i++)
						{
							BoneNamesBox.Text += $"{i + 1}. {OGF_V.bones.bones[i]}";
							if (i != OGF_V.bones.bones.Count - 1)
								BoneNamesBox.Text += "\n";
						}
					}
					break;
				case "MaterialBox": OGF_V.ikdata.materials[idx] = curBox.Text; break;
				case "MassBox": OGF_V.ikdata.mass[idx] = Convert.ToSingle(curBox.Text); break;
				case "CenterBoxX": vec.x = Convert.ToSingle(curBox.Text); vec.y = OGF_V.ikdata.center_mass[idx].y; vec.z = OGF_V.ikdata.center_mass[idx].z; OGF_V.ikdata.center_mass[idx] = vec; break;
				case "CenterBoxY": vec.x = OGF_V.ikdata.center_mass[idx].x; vec.y = Convert.ToSingle(curBox.Text); vec.z = OGF_V.ikdata.center_mass[idx].z; OGF_V.ikdata.center_mass[idx] = vec; break;
				case "CenterBoxZ": vec.x = OGF_V.ikdata.center_mass[idx].x; vec.y = OGF_V.ikdata.center_mass[idx].y; vec.z = Convert.ToSingle(curBox.Text); OGF_V.ikdata.center_mass[idx] = vec; break;
				case "PositionX": vec.x = Convert.ToSingle(curBox.Text); vec.y = OGF_V.ikdata.position[idx].y; vec.z = OGF_V.ikdata.position[idx].z; OGF_V.ikdata.position[idx] = vec; break;
				case "PositionY": vec.x = OGF_V.ikdata.position[idx].x; vec.y = Convert.ToSingle(curBox.Text); vec.z = OGF_V.ikdata.position[idx].z; OGF_V.ikdata.position[idx] = vec; break;
				case "PositionZ": vec.x = OGF_V.ikdata.position[idx].x; vec.y = OGF_V.ikdata.position[idx].y; vec.z = Convert.ToSingle(curBox.Text); OGF_V.ikdata.position[idx] = vec; break;
				case "RotationX": vec.x = Convert.ToSingle(curBox.Text); vec.y = OGF_V.ikdata.rotation[idx].y; vec.z = OGF_V.ikdata.rotation[idx].z; OGF_V.ikdata.rotation[idx] = vec; break;
				case "RotationY": vec.x = OGF_V.ikdata.rotation[idx].x; vec.y = Convert.ToSingle(curBox.Text); vec.z = OGF_V.ikdata.rotation[idx].z; OGF_V.ikdata.rotation[idx] = vec; break;
				case "RotationZ": vec.x = OGF_V.ikdata.rotation[idx].x; vec.y = OGF_V.ikdata.rotation[idx].y; vec.z = Convert.ToSingle(curBox.Text); OGF_V.ikdata.rotation[idx] = vec; break;
			}

			bKeyIsDown = false;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (FILE_NAME == "") return;

			CopyParams();
			SaveFile(FILE_NAME);
			AutoClosingMessageBox.Show(IsModelBroken ? "Repaired and Saved!" : "Saved!", "", IsModelBroken ? 700 : 500, MessageBoxIcon.Information);
		}

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			openOGFDialog.FileName = "";
			DialogResult res = openOGFDialog.ShowDialog();

			if (res == DialogResult.OK)
			{
				Clear();
				if (OpenFile(openOGFDialog.FileName))
				{
					FILE_NAME = openOGFDialog.FileName;
					AfterLoad();
				}
			}
		}

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.Control && e.KeyCode == Keys.S)
				saveToolStripMenuItem_Click(null, null);

			switch (e.KeyData)
			{
				case Keys.F3: reloadToolStripMenuItem_Click(null, null); break;
				case Keys.F4: loadToolStripMenuItem_Click(null, null); break;
				case Keys.F5: saveToolStripMenuItem_Click(null, null); break;
				case Keys.F6: saveAsToolStripMenuItem_Click(null, null); break;
			}
		}

        private void oGFInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");

			OgfInfo Info = new OgfInfo(OGF_V.descr, m_version, m_model_type);
            Info.ShowDialog();

			if (Info.res)
			{
				OGF_V.descr.m_source = Info.descr.m_source;
				OGF_V.descr.m_export_tool = Info.descr.m_export_tool;
				OGF_V.descr.m_owner_name = Info.descr.m_owner_name;
				OGF_V.descr.m_export_modif_name_tool = Info.descr.m_export_modif_name_tool;
				OGF_V.descr.m_creation_time = Info.descr.m_creation_time;
				OGF_V.descr.m_export_time = Info.descr.m_export_time;
				OGF_V.descr.m_modified_time = Info.descr.m_modified_time;
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
			else if (format == ".omf")
			{
				using (var fileStream = new FileStream(Filename, FileMode.OpenOrCreate))
				{
					fileStream.Write(Current_OMF, 0, Current_OMF.Length);
				}
			}
			AutoClosingMessageBox.Show(IsModelBroken ? "Repaired and Saved!" : "Saved!", "", IsModelBroken ? 700 : 500, MessageBoxIcon.Information);
		}

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (MessageBox.Show("Are you sure you want to exit?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				Close();
		}

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string exe_path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\OGFViewer.exe";
			if (File.Exists(exe_path))
			{
				System.Diagnostics.Process p = new System.Diagnostics.Process();
				p.StartInfo.FileName = exe_path;
				p.StartInfo.Arguments += FILE_NAME;
				p.Start();
			}
			else
            {
				MessageBox.Show("Can't find OGFViewer.exe in program folder.\nDownload OGF Viewer 1.0.2 or later!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

        private void CreateUserdataButton_Click(object sender, EventArgs e)
        {
			CreateUserdataButton.Visible = false;
			UserDataBox.Visible = true;
			OGF_V.usertdata = new UserData();
			OGF_V.usertdata.need_create = true;
		}

        private void CreateMotionRefsButton_Click(object sender, EventArgs e)
        {
			m_model_type = 3;
			CreateMotionRefsButton.Visible = false;
			MotionRefsBox.Visible = true;
			OGF_V.refs = new MotionRefs();
			OGF_V.refs.need_create = true;
		}

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string cur_fname = FILE_NAME;
			Clear();
			if (OpenFile(cur_fname))
			{
				FILE_NAME = cur_fname;
				AfterLoad();
			}
		}

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (TabControl.SelectedIndex < 0) return;
			MotionToolsMenuItem.Enabled = false;

			switch (TabControl.Controls[TabControl.SelectedIndex].Name)
			{

				case "UserDataPage":
					{
						if (UserDataBox.Text == "")
						{
							CreateUserdataButton.Visible = true;
							UserDataBox.Visible = false;
							OGF_V.usertdata = null;
						}
						break;
					}
				case "MotionRefsPage":
					{
						if (MotionRefsBox.Text == "")
						{
							if (MotionBox.Text == "")
								m_model_type = 10;

							CreateMotionRefsButton.Visible = true;
							MotionRefsBox.Visible = false;
							OGF_V.refs = null;
						}
						break;
					}
				case "MotionPage":
					{
						MotionToolsMenuItem.Enabled = true;
						break;
					}
			}
		}

        private void AppendOMFButton_Click(object sender, EventArgs e)
        {
			openOMFDialog.ShowDialog();
        }

		private void AppendMotion(object sender, CancelEventArgs e)
		{
			AppendOMFButton.Visible = false;
			MotionBox.Visible = true;
			OGF_V.delete_motions = false;

			var xr_loader = new XRayLoader();

			Current_OMF = File.ReadAllBytes(openOMFDialog.FileName);
			MotionBox.Clear();
			m_model_type = 3;

			using (var r = new BinaryReader(new MemoryStream(Current_OMF)))
			{
				xr_loader.SetStream(r.BaseStream);

				if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
				{
					int id = 0;

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
			}
		}

        private void deleteChunkToolStripMenuItem_Click(object sender, EventArgs e)
        {
			OGF_V.delete_motions = !OGF_V.delete_motions;
			if (OGF_V.delete_motions)
            {
				deleteChunkToolStripMenuItem.Text = "Return last motions";
				MotionBox.Visible = false;
				AppendOMFButton.Visible = true;
				replaceMotionsToolStripMenuItem.Enabled = false;
				editImOMFEditorToolStripMenuItem.Enabled = false;

				MotionBox.Clear();
				m_model_type = 3;
			}
			else
            {
				deleteChunkToolStripMenuItem.Text = "Delete motions";
				MotionBox.Visible = true;
				AppendOMFButton.Visible = false;
				replaceMotionsToolStripMenuItem.Enabled = true;
				editImOMFEditorToolStripMenuItem.Enabled = true;

				var xr_loader = new XRayLoader();

				using (var r = new BinaryReader(new MemoryStream(Current_OMF)))
				{
					xr_loader.SetStream(r.BaseStream);

					if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
					{
						int id = 0;

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
				}
			}
        }

		private void editImOMFEditorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.CreateDirectory(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp");

			string Filename = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + $"\\temp\\{StatusFile.Text}_temp.omf";
			string OmfEditor = GetOmfEditorPath();

			if (OmfEditor == null)
            {
				MessageBox.Show("Please, set OMF Editor path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
            }

			using (var fileStream = new FileStream(Filename, FileMode.OpenOrCreate))
			{
				fileStream.Write(Current_OMF, 0, Current_OMF.Length);
			}

			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.StartInfo.FileName = OmfEditor;
			proc.StartInfo.Arguments += $"\"{Filename}\"";
			proc.Start();
			proc.WaitForExit();

			openOMFDialog.FileName = Filename;
			AppendMotion(null, null);
			openOMFDialog.FileName = "";

			File.Delete(Filename);

			if (Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.Delete(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp", true);
		}

		private string GetOmfEditorPath()
        {
			string file_path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\Settings.ini";
			string omf_editor_path = null;

			if (File.Exists(file_path))
			{
				IniFile file = new IniFile(file_path);
				omf_editor_path = file.Read("omf_editor", "settings");
			}
			else
            {
				MessageBox.Show("Please, open OMF Editor path", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
				if (openProgramDialog.ShowDialog() == DialogResult.OK)
				{
					omf_editor_path = openProgramDialog.FileName;
					File.WriteAllText(file_path, $"[settings]\nomf_editor = {omf_editor_path}");
				}
				else
					File.Delete(file_path);
			}

			return omf_editor_path;
		}
    }
}
