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
		// File sytem
		public OGF_Children OGF_V = null;
		public byte[] Current_OGF = null;
		public byte[] Current_OMF = null;
		public List<byte> file_bytes = new List<byte>();
		public string FILE_NAME = "";
		IniFile Settings = null;
		FolderSelectDialog SaveSklDialog = null;

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
			motionToolsToolStripMenuItem.Enabled = false;
			openSkeletonInObjectEditorToolStripMenuItem.Enabled = false;
			toolStripMenuItem1.Enabled = false;
			exportToolStripMenuItem.Enabled = false;
			LabelBroken.Visible = false;

			SaveSklDialog = new FolderSelectDialog();

			string file_path = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\Settings.ini";
			Settings = new IniFile(file_path);

			if (Environment.GetCommandLineArgs().Length > 1)
			{
				Clear(false);
				if (OpenFile(Environment.GetCommandLineArgs()[1], ref OGF_V, ref Current_OGF, ref Current_OMF))
				{
					FILE_NAME = Environment.GetCommandLineArgs()[1];
					AfterLoad(true);
				}
			}
			else
            {
				TabControl.Controls.Clear();
			}

			if (Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.Delete(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp", true);
		}

		private void Clear(bool ui_only)
		{
			if (!ui_only)
			{
				FILE_NAME = "";
				OGF_V = null;
				file_bytes.Clear();
			}
			TexturesPage.Controls.Clear();
			BoneParamsPage.Controls.Clear();
			TabControl.Controls.Clear();
			MotionRefsBox.Clear();
			BoneNamesBox.Clear();
			UserDataBox.Clear();
			MotionBox.Clear();
			LodPathBox.Clear();
		}

		private void AfterLoad(bool main_file)
		{
			if (main_file)
			{
				StatusFile.Text = FILE_NAME.Substring(FILE_NAME.LastIndexOf('\\') + 1);

				oGFInfoToolStripMenuItem.Enabled = true;
				SaveMenuParam.Enabled = true;
				saveAsToolStripMenuItem.Enabled = true;
				toolStripMenuItem1.Enabled = !OGF_V.IsDM;
				oGFInfoToolStripMenuItem.Enabled = !OGF_V.IsDM;
				openSkeletonInObjectEditorToolStripMenuItem.Enabled = OGF_V.IsSkeleton();
				viewToolStripMenuItem.Enabled = OGF_V.IsSkeleton();
				exportToolStripMenuItem.Enabled = true;
				bonesToolStripMenuItem.Enabled = OGF_V.IsSkeleton();
				omfToolStripMenuItem.Enabled = Current_OMF != null;
				sklToolStripMenuItem.Enabled = Current_OMF != null;
				sklsToolStripMenuItem.Enabled = Current_OMF != null;

				OpenOGFDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				OpenOGF_DmDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveAsDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveAsDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.'));
				OpenOMFDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				OpenProgramDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveSklDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveSklsDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveSklsDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".skls";
				SaveOmfDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveOmfDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".omf";
				SaveBonesDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveBonesDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".bones";
				SaveObjectDialog.InitialDirectory = FILE_NAME.Substring(0, FILE_NAME.LastIndexOf('\\'));
				SaveObjectDialog.FileName = StatusFile.Text.Substring(0, StatusFile.Text.LastIndexOf('.')) + ".object";
			}

			// Textures
			TabControl.Controls.Add(TexturesPage);

			if (OGF_V.IsSkeleton())
			{
				//Userdata
				TabControl.Controls.Add(UserDataPage);
				UserDataPage.Controls.Clear();
				UserDataPage.Controls.Add(UserDataBox);
				UserDataPage.Controls.Add(CreateUserdataButton);
				CreateUserdataButton.Visible = false;
				UserDataBox.Visible = false;

				if (OGF_V.userdata != null)
					UserDataBox.Visible = true;
				else
					CreateUserdataButton.Visible = true;

				// Motion Refs
				TabControl.Controls.Add(MotionRefsPage);
				MotionRefsPage.Controls.Clear();
				MotionRefsPage.Controls.Add(MotionRefsBox);
				MotionRefsPage.Controls.Add(CreateMotionRefsButton);
				CreateMotionRefsButton.Visible = false;
				MotionRefsBox.Visible = false;

				if (OGF_V.motion_refs != null)
					MotionRefsBox.Visible = true;
				else
					CreateMotionRefsButton.Visible = true;

				// Motions
				TabControl.Controls.Add(MotionPage);
				MotionBox.Text = OGF_V.motions;

				if (OGF_V.motions != "")
				{
					AppendOMFButton.Visible = false;
					MotionBox.Visible = true;
				}
				else
				{
					MotionBox.Visible = false;
					AppendOMFButton.Visible = true;
				}

				// Bones
				if (OGF_V.bones.bones != null)
				{
					BoneNamesBox.Clear();
					TabControl.Controls.Add(BoneNamesPage);

					BoneNamesBox.Text += $"Bones count : {OGF_V.bones.bones.Count}\n\n";
					for (int i = 0; i < OGF_V.bones.bones.Count; i++)
					{
						BoneNamesBox.Text += $"{i + 1}. {OGF_V.bones.bones[i]}";

						if (i != OGF_V.bones.bones.Count - 1)
							BoneNamesBox.Text += "\n";
					}
				}

				// Ik Data
				TabControl.Controls.Add(BoneParamsPage);

				// Lod
				TabControl.Controls.Add(LodPage);

				if (OGF_V.lod != null)
				{
					CreateLodButton.Visible = false;
					LodPathBox.Text = OGF_V.lod.lod_path;
				}
				else
					CreateLodButton.Visible = true;
			}

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

			if (OGF_V.bones.bones != null)
			{
				for (int i = 0; i < OGF_V.bones.bones.Count; i++)
				{
					CreateBoneGroupBox(i, OGF_V.bones.bones[i], OGF_V.bones.parent_bones[i], OGF_V.ikdata.materials[i], OGF_V.ikdata.mass[i], OGF_V.ikdata.center_mass[i], OGF_V.ikdata.position[i], OGF_V.ikdata.rotation[i]);
				}
			}

			MotionRefsBox.Clear();
			UserDataBox.Clear();

			if (OGF_V.motion_refs != null)
				MotionRefsBox.Lines = OGF_V.motion_refs.refs.ToArray();

			if (OGF_V.userdata != null)
				UserDataBox.Text = OGF_V.userdata.userdata;

			if (main_file && !OGF_V.IsDM)
			{
				OGF_V.BrokenType = CatchBrokenModel();

				if (OGF_V.BrokenType == 1)
				{
					OGF_V.Descr4Byte = CatchDescr4ByteModel();
					if (OGF_V.Descr4Byte)
						OGF_V.BrokenType = 0;
				}

				if (OGF_V.BrokenType > 0)
				{
					OGF_V.description.m_export_time = 0;
					OGF_V.description.m_creation_time = 0;
					OGF_V.description.m_modified_time = 0;
				}

				LabelBroken.Text = OGF_V.BrokenType > 0 ? "Broken " + OGF_V.BrokenType.ToString() : "4 byte";
				LabelBroken.Visible = OGF_V.BrokenType > 0 || OGF_V.Descr4Byte;
			}
		}

		private void CopyParams()
		{
			if (OGF_V.motion_refs != null)
			{
				OGF_V.motion_refs.refs.Clear();

				if (MotionRefsBox.Lines.Count() > 0)
				{
					OGF_V.motion_refs.refs.AddRange(MotionRefsBox.Lines);

					if (MotionRefsBox.Lines.Count() > 1)
						OGF_V.motion_refs.v3 = false;
				}
			}

			if (OGF_V.userdata != null)
			{
				OGF_V.userdata.userdata = "";

				if (UserDataBox.Text != "")
				{
					for (int i = 0; i < UserDataBox.Lines.Count(); i++)
					{
						string ext = i == UserDataBox.Lines.Count() - 1 ? "" : "\r\n";
						OGF_V.userdata.userdata += UserDataBox.Lines[i] + ext;
					}
				}
			}

			if (OGF_V.lod != null)
				OGF_V.lod.lod_path = LodPathBox.Text;

			UpdateModelType();
		}

		private uint CatchBrokenModel()
		{
			if (Current_OGF == null) return 0;

			if (OGF_V.IsSkeleton() && OGF_V.bones.pos < OGF_V.pos)
				return 2;

			try
			{
				using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
				{
					fileStream.ReadBytes(8);
					fileStream.ReadBytes(2);
					fileStream.ReadBytes((int)(OGF_V.description.pos - fileStream.BaseStream.Position));
					fileStream.ReadBytes(OGF_V.description.old_size + 8);
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
				return 1;
			}

			return 0;
		}

		private bool CatchDescr4ByteModel()
		{
			if (Current_OGF == null) return false;

			string source = "";
			string export_tool = "";
			uint export_time = 0;
			string owner_name = "";
			uint creation_time = 0;
			string export_modif_name_tool = "";
			uint modified_time = 0;

			int descr_old_size = 0;

			try
			{
				var xr_loader = new XRayLoader();

				long descr_pos = 0;

				using (var r = new BinaryReader(new MemoryStream(Current_OGF)))
				{
					xr_loader.SetStream(r.BaseStream);

					xr_loader.find_chunk((int)OGF.OGF4_HEADER);

					xr_loader.ReadByte();
					xr_loader.ReadByte();

					if (!xr_loader.find_chunk((int)OGF.OGF4_S_DESC, false, true))
						return false;

					descr_pos = xr_loader.chunk_pos;

					source = xr_loader.read_stringZ();
					export_tool = xr_loader.read_stringZ();
					export_time = xr_loader.ReadUInt32();
					owner_name = xr_loader.read_stringZ();
					creation_time = xr_loader.ReadUInt32();
					export_modif_name_tool = xr_loader.read_stringZ();
					modified_time = xr_loader.ReadUInt32();

					descr_old_size = source.Length + 1 + export_tool.Length + 1 + 4 + owner_name.Length + 1 + 4 + export_modif_name_tool.Length + 1 + 4;

					xr_loader.SetStream(r.BaseStream);
				}


				using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
				{
					fileStream.ReadBytes(8);
					fileStream.ReadBytes(2);
					fileStream.ReadBytes((int)(descr_pos - fileStream.BaseStream.Position));
					fileStream.ReadBytes((int)descr_old_size + 8);
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
				return false;
			}

			OGF_V.description.m_source = source;
			OGF_V.description.m_export_tool = export_tool;
			OGF_V.description.m_export_time = export_time;
			OGF_V.description.m_owner_name = owner_name;
			OGF_V.description.m_creation_time = creation_time;
			OGF_V.description.m_export_modif_name_tool = export_modif_name_tool;
			OGF_V.description.m_modified_time = modified_time;

			OGF_V.description.old_size = descr_old_size;

			return true;
		}

		private void WriteFile(string filename)
        {
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

		private void SaveFile(string filename)
		{
			file_bytes.Clear();

			if (Current_OGF == null) return;

			using (var fileStream = new BinaryReader(new MemoryStream(Current_OGF)))
			{
				if (OGF_V.IsDM)
				{
                    fileStream.ReadBytes(OGF_V.childs[0].old_size);
					file_bytes.AddRange(Encoding.Default.GetBytes(OGF_V.childs[0].m_shader));
					file_bytes.Add(0);
					file_bytes.AddRange(Encoding.Default.GetBytes(OGF_V.childs[0].m_texture));
					file_bytes.Add(0);
                    byte[] dm_data = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));
					file_bytes.AddRange(dm_data);
					WriteFile(filename);
					return;
				}

				fileStream.ReadBytes(8);
				file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_HEADER));
				file_bytes.AddRange(BitConverter.GetBytes((uint)44));

				fileStream.ReadBytes(2);
				file_bytes.Add(OGF_V.m_version);
				file_bytes.Add(OGF_V.m_model_type);

				byte[] temp = fileStream.ReadBytes(2);
				file_bytes.AddRange(temp);

				temp = fileStream.ReadBytes(40);
				if (OGF_V.BrokenType == 2)
				{
					for (int i = 0; i < 40; i++)
						file_bytes.Add(0);
				}
				else
                {
					file_bytes.AddRange(temp);
				}

				file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_DESC));
				file_bytes.AddRange(BitConverter.GetBytes(OGF_V.description.chunk_size(OGF_V.Descr4Byte)));
				file_bytes.AddRange(OGF_V.description.data(OGF_V.Descr4Byte));

				fileStream.ReadBytes((int)(OGF_V.pos - fileStream.BaseStream.Position));

				fileStream.ReadBytes(4);
				uint new_size = fileStream.ReadUInt32();

				foreach (var ch in OGF_V.childs)
					new_size += ch.NewSize();

				file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_CHILDREN));
				file_bytes.AddRange(BitConverter.GetBytes(new_size));

				uint last_child_size = 0;
				foreach (var ch in OGF_V.childs)
				{
					temp = fileStream.ReadBytes((int)(ch.parent_pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);
					fileStream.ReadUInt32();
					new_size = fileStream.ReadUInt32();
					new_size += ch.NewSize();
					last_child_size = new_size;
					file_bytes.AddRange(BitConverter.GetBytes(ch.parent_id));
					file_bytes.AddRange(BitConverter.GetBytes(new_size));

					last_child_size -= (uint)(ch.pos - fileStream.BaseStream.Position);
					temp = fileStream.ReadBytes((int)(ch.pos - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);
					file_bytes.AddRange(ch.data());
					last_child_size -= (uint)(ch.old_size + 8);
					fileStream.BaseStream.Position += ch.old_size + 8;
				}

				if (OGF_V.IsSkeleton())
                {
					temp = fileStream.ReadBytes((int)(last_child_size));
					file_bytes.AddRange(temp);

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_BONE_NAMES));
					file_bytes.AddRange(BitConverter.GetBytes(OGF_V.bones.chunk_size()));
					file_bytes.AddRange(OGF_V.bones.data(false));

					fileStream.ReadBytes(OGF_V.bones.old_size + 8);

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_IKDATA));
					file_bytes.AddRange(BitConverter.GetBytes(OGF_V.ikdata.chunk_size()));
					file_bytes.AddRange(OGF_V.ikdata.data());

					fileStream.ReadBytes(OGF_V.ikdata.old_size + 8);

					if (OGF_V.userdata != null)
					{
						if (OGF_V.userdata.pos > 0 && (OGF_V.userdata.pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
						{
							temp = fileStream.ReadBytes((int)(OGF_V.userdata.pos - fileStream.BaseStream.Position));
							file_bytes.AddRange(temp);
						}

						if (OGF_V.userdata.userdata != "") // Пишем если есть что писать
						{
							file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_USERDATA));
							file_bytes.AddRange(BitConverter.GetBytes(OGF_V.userdata.chunk_size()));
							file_bytes.AddRange(OGF_V.userdata.data());
						}

						if (OGF_V.userdata.old_size > 0) // Сдвигаем позицию риадера если в модели был чанк
							fileStream.ReadBytes(OGF_V.userdata.old_size + 8);
					}

					if (OGF_V.lod != null)
					{
						if (OGF_V.lod.pos > 0 && (OGF_V.lod.pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
						{
							temp = fileStream.ReadBytes((int)(OGF_V.lod.pos - fileStream.BaseStream.Position));
							file_bytes.AddRange(temp);
						}

						if (OGF_V.lod.lod_path != "") // Пишем если есть что писать
						{
							file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_LODS));
							file_bytes.AddRange(BitConverter.GetBytes(OGF_V.lod.chunk_size()));
							file_bytes.AddRange(OGF_V.lod.data());
						}

						if (OGF_V.lod.old_size > 0) // Сдвигаем позицию риадера если в модели был чанк
							fileStream.ReadBytes(OGF_V.lod.old_size + 8);
					}

					bool refs_created = false;
					if (OGF_V.motion_refs != null)
					{
						if (OGF_V.motion_refs.pos > 0 && (OGF_V.motion_refs.pos - fileStream.BaseStream.Position) > 0) // Двигаемся до текущего чанка
						{
							temp = fileStream.ReadBytes((int)(OGF_V.motion_refs.pos - fileStream.BaseStream.Position));
							file_bytes.AddRange(temp);
						}

						if (OGF_V.motion_refs.refs.Count > 0) // Пишем если есть что писать
						{
							refs_created = true;

							if (!OGF_V.motion_refs.v3)
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS2));
							else
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS));

							file_bytes.AddRange(BitConverter.GetBytes(OGF_V.motion_refs.chunk_size(OGF_V.motion_refs.v3)));

							if (!OGF_V.motion_refs.v3)
								file_bytes.AddRange(OGF_V.motion_refs.count());

							file_bytes.AddRange(OGF_V.motion_refs.data(OGF_V.motion_refs.v3));
						}

						if (OGF_V.motion_refs.old_size > 0) // Сдвигаем позицию риадера если в модели был чанк
							fileStream.ReadBytes(OGF_V.motion_refs.old_size + 8);
					}

					if (Current_OMF != null && !refs_created)
						file_bytes.AddRange(Current_OMF);
				}
				else
				{
					temp = fileStream.ReadBytes((int)(fileStream.BaseStream.Length - fileStream.BaseStream.Position));
					file_bytes.AddRange(temp);
				}
			}

			WriteFile(filename);
		}

		private bool OpenFile(string filename, ref OGF_Children OGF_C, ref byte[] Cur_OGF, ref byte[] Cur_OMF)
		{
			var xr_loader = new XRayLoader();

			string format = Path.GetExtension(filename);

			OGF_C = new OGF_Children();

			if (format == ".dm")
				OGF_C.IsDM = true;

			Cur_OGF = File.ReadAllBytes(filename);

			using (var r = new BinaryReader(new MemoryStream(Cur_OGF)))
			{
				xr_loader.SetStream(r.BaseStream);

				if (OGF_C.IsDM)
                {
					string shader = xr_loader.read_stringZ();
					string texture = xr_loader.read_stringZ();
					OGF_Child chld = new OGF_Child(0, 0, 0, shader.Length + texture.Length + 2, texture, shader);
					OGF_C.childs.Add(chld);
					return true;
				}

				if (!xr_loader.find_chunk((int)OGF.OGF4_HEADER, false, true))
				{
					MessageBox.Show("Unsupported OGF format! Can't find header chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				else
				{
					OGF_C.m_version = xr_loader.ReadByte();
					OGF_C.m_model_type = xr_loader.ReadByte();
					xr_loader.ReadBytes(42);
				}

				if (!xr_loader.find_chunk((int)OGF.OGF4_S_DESC, false, true))
				{
					MessageBox.Show("Unsupported OGF format! Can't find description chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				else
                {
					OGF_C.description = new Description();
					OGF_C.description.pos = xr_loader.chunk_pos;

					OGF_C.description.m_source = xr_loader.read_stringZ();
					OGF_C.description.m_export_tool = xr_loader.read_stringZ();
					OGF_C.description.m_export_time = xr_loader.ReadInt64();
					OGF_C.description.m_owner_name = xr_loader.read_stringZ();
					OGF_C.description.m_creation_time = xr_loader.ReadInt64();
					OGF_C.description.m_export_modif_name_tool = xr_loader.read_stringZ();
					OGF_C.description.m_modified_time = xr_loader.ReadInt64();

					OGF_C.description.old_size = OGF_C.description.m_source.Length + 1 + OGF_C.description.m_export_tool.Length + 1 + 8 + OGF_C.description.m_owner_name.Length + 1 + 8 + OGF_C.description.m_export_modif_name_tool.Length + 1 + 8;
				}

				xr_loader.SetStream(r.BaseStream);

				if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_CHILDREN, false, true)))
				{
					MessageBox.Show("Unsupported OGF format! Can't find children chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				OGF_C.pos = xr_loader.chunk_pos;

				int id = 0;
				uint size;

				// Texture && shader
				while (true)
				{
					if (!xr_loader.find_chunk(id)) break;

					Stream temp = xr_loader.reader.BaseStream;

					if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

					long pos = xr_loader.chunk_pos + OGF_C.pos + 16;
					size = xr_loader.find_chunkSize((int)OGF.OGF4_TEXTURE);

					if (size == 0) break;

					OGF_Child chld = new OGF_Child(xr_loader.chunk_pos + pos, id, pos - 8, (int)size, xr_loader.read_stringZ(), xr_loader.read_stringZ());
					OGF_C.childs.Add(chld);

					id++;
					xr_loader.SetStream(temp);
				}

				xr_loader.SetStream(r.BaseStream);

				if (OGF_C.IsSkeleton())
				{
					// Userdata
					if (xr_loader.find_chunk((int)OGF.OGF4_S_USERDATA, false, true))
					{
						OGF_C.userdata = new UserData();
						OGF_C.userdata.pos = xr_loader.chunk_pos;
						OGF_C.userdata.userdata = xr_loader.read_stringZ();
						OGF_C.userdata.old_size = OGF_C.userdata.userdata.Length + 1;
					}

					xr_loader.SetStream(r.BaseStream);

					// Motion Refs
					bool v3 = xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS, false, true);

					if (v3 || xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS2, false, true))
					{
						OGF_C.motion_refs = new MotionRefs();
						OGF_C.motion_refs.pos = xr_loader.chunk_pos;
						OGF_C.motion_refs.refs = new List<string>();

						if (v3)
						{
							OGF_C.motion_refs.v3 = true;
							string refs = xr_loader.read_stringZ();
							OGF_C.motion_refs.refs.Add(refs);
							OGF_C.motion_refs.old_size = refs.Length + 1;
						}
						else
						{
							uint count = xr_loader.ReadUInt32();

							OGF_C.motion_refs.old_size = 4;

							for (int i = 0; i < count; i++)
							{
								string refs = xr_loader.read_stringZ();
								OGF_C.motion_refs.refs.Add(refs);
								OGF_C.motion_refs.old_size += refs.Length + 1;
							}
						}
					}

					xr_loader.SetStream(r.BaseStream);

					//Motions
					if (xr_loader.find_chunk((int)OGF.OGF4_S_MOTIONS, false, true))
					{
						xr_loader.reader.BaseStream.Position -= 8;
						Cur_OMF = xr_loader.ReadBytes((int)xr_loader.reader.BaseStream.Length - (int)xr_loader.reader.BaseStream.Position);
					}

					xr_loader.SetStream(r.BaseStream);

					if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
					{
						id = 0;

						while (true)
						{
							if (!xr_loader.find_chunk(id)) break;

							Stream temp = xr_loader.reader.BaseStream;

							if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

							if (id == 0)
								OGF_C.motions += $"Motions count : {xr_loader.ReadUInt32()}\n";
							else
								OGF_C.motions += $"\n{id}. {xr_loader.read_stringZ()}";

							id++;
							xr_loader.SetStream(temp);
						}
					}

					xr_loader.SetStream(r.BaseStream);

					// Bones
					if (!xr_loader.find_chunk((int)OGF.OGF4_S_BONE_NAMES, false, true))
					{
						MessageBox.Show("Unsupported OGF format! Can't find bones chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						OGF_C.bones.pos = xr_loader.chunk_pos;

						OGF_C.bones.bones = new List<string>();
						OGF_C.bones.parent_bones = new List<string>();
						OGF_C.bones.fobb = new List<byte[]>();
						OGF_C.bones.bone_childs = new List<List<int>>();
						OGF_C.bones.old_size = 0;

						uint count = xr_loader.ReadUInt32();
						OGF_C.bones.old_size += 4;

						for (; count != 0; count--)
						{
							string bone_name = xr_loader.read_stringZ();
							string parent_name = xr_loader.read_stringZ();
							byte[] obb = xr_loader.ReadBytes(60);    // Fobb
							OGF_C.bones.bones.Add(bone_name);
							OGF_C.bones.parent_bones.Add(parent_name);
							OGF_C.bones.fobb.Add(obb);

							OGF_C.bones.old_size += bone_name.Length + 1 + parent_name.Length + 1 + 60;
						}

						for (int i = 0; i < OGF_C.bones.bones.Count; i++)
						{
							List<int> childs = new List<int>();
							for (int j = 0; j < OGF_C.bones.parent_bones.Count; j++)
							{
								if (OGF_C.bones.parent_bones[j] == OGF_C.bones.bones[i])
								{
									childs.Add(j);
								}
							}
							OGF_C.bones.bone_childs.Add(childs);
						}
					}

					xr_loader.SetStream(r.BaseStream);

					// Ik Data
					if (!xr_loader.find_chunk((int)OGF.OGF4_S_IKDATA, false, true))
					{
						MessageBox.Show("Unsupported OGF format! Can't find ik data chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						OGF_C.ikdata.materials = new List<string>();
						OGF_C.ikdata.mass = new List<float>();
						OGF_C.ikdata.version = new List<uint>();
						OGF_C.ikdata.center_mass = new List<Fvector>();
						OGF_C.ikdata.position = new List<Fvector>();
						OGF_C.ikdata.rotation = new List<Fvector>();
						OGF_C.ikdata.bytes_1 = new List<List<byte[]>>();
						OGF_C.ikdata.old_size = 0;

						for (int i = 0; i < OGF_C.bones.bones.Count; i++)
						{
							List<byte[]> bytes_1 = new List<byte[]>();
							OGF_C.ikdata.old_size += 4;

							byte[] temp_byte;
							uint version = xr_loader.ReadUInt32();
							string gmtl_name = xr_loader.read_stringZ();

							temp_byte = xr_loader.ReadBytes(112);   // struct SBoneShape
							bytes_1.Add(temp_byte);
							OGF_C.ikdata.old_size += gmtl_name.Length + 1 + 112;

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

								OGF_C.ikdata.old_size += 4 + 16 * 3 + 4 + 4 + 4 + 4 + 4;

								if (version > 0)
								{
									temp_byte = xr_loader.ReadBytes(4);
									bytes_1.Add(temp_byte);
									OGF_C.ikdata.old_size += 4;
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

							OGF_C.ikdata.old_size += 12 + 12 + 4 + 12;

							OGF_C.ikdata.materials.Add(gmtl_name);
							OGF_C.ikdata.mass.Add(mass);
							OGF_C.ikdata.version.Add(version);
							OGF_C.ikdata.center_mass.Add(center);
							OGF_C.ikdata.bytes_1.Add(bytes_1);
							OGF_C.ikdata.position.Add(position);
							OGF_C.ikdata.rotation.Add(rotation);
						}
					}

					xr_loader.SetStream(r.BaseStream);

					// Lod ref
					if (xr_loader.find_chunk((int)OGF.OGF4_S_LODS, false, true))
					{
						OGF_C.lod = new Lod();
						OGF_C.lod.pos = xr_loader.chunk_pos;
						OGF_C.lod.lod_path = xr_loader.read_stringData(ref OGF_C.lod.data_str);
						OGF_C.lod.old_size = OGF_C.lod.lod_path.Length + (OGF_C.lod.data_str ? 2 : 1);
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
							if (BoneParamsPage.Controls.Count < i) continue;
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
			AutoClosingMessageBox.Show(OGF_V.BrokenType > 0 ? "Repaired and Saved!" : "Saved!", "", OGF_V.BrokenType > 0 ? 700 : 500, MessageBoxIcon.Information);
		}

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			OpenOGF_DmDialog.FileName = "";
			DialogResult res = OpenOGF_DmDialog.ShowDialog();

			if (res == DialogResult.OK)
			{
				Clear(false);
				if (OpenFile(OpenOGF_DmDialog.FileName, ref OGF_V, ref Current_OGF, ref Current_OMF))
				{
					OpenOGF_DmDialog.InitialDirectory = "";
					FILE_NAME = OpenOGF_DmDialog.FileName;
					AfterLoad(true);
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

			OgfInfo Info = new OgfInfo(OGF_V.description, OGF_V.m_version, OGF_V.m_model_type);
            Info.ShowDialog();

			if (Info.res)
			{
				OGF_V.description.m_source = Info.descr.m_source;
				OGF_V.description.m_export_tool = Info.descr.m_export_tool;
				OGF_V.description.m_owner_name = Info.descr.m_owner_name;
				OGF_V.description.m_export_modif_name_tool = Info.descr.m_export_modif_name_tool;
				OGF_V.description.m_creation_time = Info.descr.m_creation_time;
				OGF_V.description.m_export_time = Info.descr.m_export_time;
				OGF_V.description.m_modified_time = Info.descr.m_modified_time;
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (OGF_V.IsDM)
				SaveAsDialog.Filter = "DM file|*.dm";
			else
				SaveAsDialog.Filter = "OGF file|*.ogf";

			if (SaveAsDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveAsDialog.FileName, 0);
				SaveAsDialog.InitialDirectory = "";
			}
		}

		private void SaveTools(string filename, int format)
		{
			bool has_msg = false;

			if (File.Exists(filename) && filename != FILE_NAME)
			{
				FileInfo backup_file = new FileInfo(filename);
				backup_file.Delete();
			}

			if (format == 0)
			{
				if (filename != FILE_NAME)
				{
					FileInfo file = new FileInfo(FILE_NAME);
					file.CopyTo(filename);
				}

				CopyParams();
				SaveFile(filename);
				AutoClosingMessageBox.Show(OGF_V.BrokenType > 0 ? "Repaired and Saved!" : "Saved!", "", OGF_V.BrokenType > 0 ? 700 : 500, MessageBoxIcon.Information);
				has_msg = true;
			}
			else if (format == 1)
			{
				string ext = OGF_V.IsDM ? ".dm" : ".ogf";

				if (filename != FILE_NAME)
				{
					if (File.Exists(filename + ext))
					{
						FileInfo backup_file = new FileInfo(filename + ext);
						backup_file.Delete();
					}

					FileInfo file = new FileInfo(FILE_NAME);
					file.CopyTo(filename + ext);
				}

				CopyParams();
				SaveFile(filename + ext);

				RunConverter(filename + ext, filename, OGF_V.IsDM ? 2 : 0, 0);

				if (File.Exists(filename + ext))
				{
					FileInfo backup_file = new FileInfo(filename + ext);
					backup_file.Delete();
				}
			}
			else if (format == 2)
				RunConverter(FILE_NAME, filename, 0, 1);
			else if (format == 3)
				RunConverter(FILE_NAME, filename, 0, 2);
			else if (format == 4)
				RunConverter(FILE_NAME, filename, 0, 3);
			else if (format == 5)
			{
				using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
				{
					fileStream.Write(Current_OMF, 0, Current_OMF.Length);
				}
			}
			if (!has_msg)
				AutoClosingMessageBox.Show(OGF_V.BrokenType > 0 ? "Repaired and Exported!" : "Exported!", "", OGF_V.BrokenType > 0 ? 700 : 500, MessageBoxIcon.Information);
		}

		private void objectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveObjectDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveObjectDialog.FileName, 1);
				SaveObjectDialog.InitialDirectory = "";
			}
		}

		private void bonesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveBonesDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveBonesDialog.FileName, 2);
				SaveBonesDialog.InitialDirectory = "";
			}
		}

		private void omfToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveOmfDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveOmfDialog.FileName, 5);
				SaveOmfDialog.InitialDirectory = "";
			}
		}

		private void sklToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveSklDialog.ShowDialog(this.Handle))
			{
				SaveTools(SaveSklDialog.FileName, 3);
				SaveSklDialog.InitialDirectory = "";
			}
		}

		private void sklsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveSklsDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveSklsDialog.FileName, 4);
				SaveSklsDialog.InitialDirectory = "";
			}
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
			if (OGF_V.userdata == null)
				OGF_V.userdata = new UserData();
		}

        private void CreateMotionRefsButton_Click(object sender, EventArgs e)
        {
			if (Current_OMF == null || Current_OMF != null && MessageBox.Show("New motion refs chunk will remove built-in motions, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				// Чистим все связанное со встроенными анимами
				MotionBox.Clear();
				MotionBox.Visible = false;
				Current_OMF = null;
				AppendOMFButton.Visible = true;
				OGF_V.motions = "";

				// Обновляем тип модели
				UpdateModelType();

				// Обновляем визуал интерфейса моушн рефов
				CreateMotionRefsButton.Visible = false; 
				MotionRefsBox.Visible = true;

				if (OGF_V.motion_refs == null)
					OGF_V.motion_refs = new MotionRefs();
			}
		}

		private void CreateLodButton_Click(object sender, EventArgs e)
		{
			CreateLodButton.Visible = false;
			if (OGF_V.lod == null)
				OGF_V.lod = new Lod();
		}

		private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
			string cur_fname = FILE_NAME;
			Clear(false);
			if (OpenFile(cur_fname, ref OGF_V, ref Current_OGF, ref Current_OMF))
			{
				FILE_NAME = cur_fname;
				AfterLoad(true);
			}
		}

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (TabControl.SelectedIndex < 0) return;
			motionToolsToolStripMenuItem.Enabled = false;

			switch (TabControl.Controls[TabControl.SelectedIndex].Name)
			{

				case "UserDataPage":
					{
						if (UserDataBox.Text == "")
						{
							CreateUserdataButton.Visible = true;
							UserDataBox.Visible = false;
						}
						break;
					}
				case "MotionRefsPage":
					{
						if (MotionRefsBox.Text == "")
						{
							CreateMotionRefsButton.Visible = true;
							MotionRefsBox.Visible = false;
						}
						break;
					}
				case "MotionPage":
					{
						if (Current_OMF != null)
							motionToolsToolStripMenuItem.Enabled = true;
						break;
					}
				case "LodPage":
					{
						if (LodPathBox.Text == "")
						{
							CreateLodButton.Visible = true;
						}
						break;
					}
			}
		}

		private void RichTextBoxTextChanged(object sender, EventArgs e)
		{
			RichTextBox curBox = sender as RichTextBox;
			switch (curBox.Name)
			{
                case "MotionRefsBox":
                    {
						UpdateModelType();
						break;
                    }
            }
		}

		private void AppendOMFButton_Click(object sender, EventArgs e)
        {
			if (MotionRefsBox.Text == "" && (OGF_V.motion_refs == null || OGF_V.motion_refs.refs.Count() == 0) || (MotionRefsBox.Text != "" || OGF_V.motion_refs != null && OGF_V.motion_refs.refs.Count() > 0) && MessageBox.Show("Build-in motions will remove motion refs, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				OpenOMFDialog.ShowDialog();
        }

		private void AppendMotion(object sender, CancelEventArgs e)
		{
			if (sender != null)
				OpenOMFDialog.InitialDirectory = "";

            byte[] OpenedOmf = File.ReadAllBytes(OpenOMFDialog.FileName);

			var xr_loader = new XRayLoader();

			using (var r = new BinaryReader(new MemoryStream(OpenedOmf)))
			{
				xr_loader.SetStream(r.BaseStream);

				if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
				{
					// Апдейтим визуал встроенных анимаций
					AppendOMFButton.Visible = false;
					MotionBox.Visible = true;
					motionToolsToolStripMenuItem.Enabled = true;

					// Чистим встроенные рефы, интерфейс почистится сам при активации вкладки
					MotionRefsBox.Clear();
					if (OGF_V.motion_refs != null)
						OGF_V.motion_refs.refs.Clear();

					Current_OMF = OpenedOmf;
					OGF_V.motions = "";

					int id = 0;

					while (true)
					{
						if (!xr_loader.find_chunk(id)) break;

						Stream temp = xr_loader.reader.BaseStream;

						if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

						if (id == 0)
							OGF_V.motions += $"Motions count : {xr_loader.ReadUInt32()}\n";
						else
							OGF_V.motions += $"\n{id}. {xr_loader.read_stringZ()}";

						id++;
						xr_loader.SetStream(temp);
					}

					MotionBox.Text = OGF_V.motions;
				}
			}

			UpdateModelType();
		}

        private void deleteChunkToolStripMenuItem_Click(object sender, EventArgs e)
        {
			MotionBox.Visible = false;
			AppendOMFButton.Visible = true;
			motionToolsToolStripMenuItem.Enabled = false;
			Current_OMF = null;
			MotionBox.Clear();
			OGF_V.motions = "";
			UpdateModelType();
		}

		private void importDataFromModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenOGFDialog.FileName = "";
			if (OpenOGFDialog.ShowDialog() == DialogResult.OK)
			{
				bool UpdateUi = false;

				OGF_Children SecondOgf = null;
				byte[] SecondOgfByte = null;
				byte[] SecondOmfByte = null;
				OpenFile(OpenOGFDialog.FileName, ref SecondOgf, ref SecondOgfByte, ref SecondOmfByte);

				if (OGF_V.childs.Count == SecondOgf.childs.Count && MessageBox.Show("Import textures and shaders path?\nThey may have different positions", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					for (int i = 0; i < OGF_V.childs.Count; i++)
					{
						OGF_V.childs[i].m_texture = SecondOgf.childs[i].m_texture;
						OGF_V.childs[i].m_shader = SecondOgf.childs[i].m_shader;
					}
					UpdateUi = true;
				}

				if (OGF_V.IsSkeleton())
				{
					if (SecondOgf.userdata != null)
					{
						if (OGF_V.userdata == null)
							OGF_V.userdata = new UserData();
						OGF_V.userdata.userdata = SecondOgf.userdata.userdata;

						UpdateUi = true;
					}
					else
						OGF_V.userdata = null;

					if (SecondOgf.lod != null)
					{
						if (OGF_V.lod == null)
							OGF_V.lod = new Lod();
						OGF_V.lod.lod_path = SecondOgf.lod.lod_path;

						UpdateUi = true;
					}
					else
						OGF_V.lod = null;

					if (SecondOgf.motion_refs != null)
					{
						if (OGF_V.motion_refs == null)
							OGF_V.motion_refs = new MotionRefs();

						OGF_V.motion_refs.refs = SecondOgf.motion_refs.refs;

						UpdateUi = true;
					}
					else
						OGF_V.motion_refs = null;

					if (SecondOmfByte != null)
					{
						Current_OMF = SecondOmfByte;
						UpdateUi = true;
					}

					if (OGF_V.ikdata.materials.Count == SecondOgf.ikdata.materials.Count)
					{
						for (int i = 0; i < OGF_V.ikdata.materials.Count; i++)
						{
							OGF_V.ikdata.materials[i] = SecondOgf.ikdata.materials[i];
							OGF_V.ikdata.mass[i] = SecondOgf.ikdata.mass[i];
						}
						UpdateUi = true;
					}
				}

				if (UpdateUi)
				{
					Clear(true);
					AfterLoad(false);
					AutoClosingMessageBox.Show("OGF Params changed!", "", 1000, MessageBoxIcon.Information);
				}
				else
                {
					AutoClosingMessageBox.Show("OGF Params don't changed!", "", 1000, MessageBoxIcon.Error);
				}
			}
		}

		private void UpdateModelType()
        {
			if (OGF_V == null) return;

			if (OGF_V.bones.bones == null)
				OGF_V.m_model_type = 1;
			else if (Current_OMF == null && MotionRefsBox.Text == "")
				OGF_V.m_model_type = 10;
			else
				OGF_V.m_model_type = 3;

			// Апдейтим экспорт аним тут, т.к. при любом изменении омф вызывается эта функция
			omfToolStripMenuItem.Enabled = Current_OMF != null;
			sklToolStripMenuItem.Enabled = Current_OMF != null;
			sklsToolStripMenuItem.Enabled = Current_OMF != null;
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

			OpenOMFDialog.FileName = Filename;
			AppendMotion(null, null);
			OpenOMFDialog.FileName = "";

			File.Delete(Filename);

			if (Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.Delete(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp", true);
		}

		private void openSkeletonInObjectEditorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
				Directory.CreateDirectory(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp");

			string Filename = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + $"\\temp\\{StatusFile.Text}_temp.ogf";
			string ObjectName = Filename.Substring(0, Filename.LastIndexOf('.'));
			ObjectName = ObjectName.Substring(0, ObjectName.LastIndexOf('.')) + ".object";

			string ObjectEditor = GetObjectEditorPath();

			if (ObjectEditor == null || ObjectEditor == "")
			{
				MessageBox.Show("Please, set Object Editor path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			File.Copy(FILE_NAME, Filename);
			CopyParams();
			SaveFile(Filename);
			RunConverter(Filename, ObjectName, 0, 0);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = ObjectEditor;
            proc.StartInfo.Arguments += $"\"{ObjectName}\" skeleton_only \"{FILE_NAME}\"";
            proc.Start();
			proc.WaitForExit();

			if (Directory.Exists(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp"))
                Directory.Delete(Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\temp", true);
        }

		private string GetOmfEditorPath()
        {
			string omf_editor_path = Settings.Read("omf_editor", "settings");
			if (!File.Exists(omf_editor_path))
			{
				MessageBox.Show("Please, open OMF Editor path", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
				if (OpenProgramDialog.ShowDialog() == DialogResult.OK)
				{
					OpenProgramDialog.InitialDirectory = "";
					omf_editor_path = OpenProgramDialog.FileName;
					Settings.Write("omf_editor", omf_editor_path, "settings");
				}
			}

			return omf_editor_path;
		}

		private string GetObjectEditorPath()
		{
			string object_editor_path = Settings.Read("object_editor", "settings");
			if (!File.Exists(object_editor_path))
			{
				MessageBox.Show("Please, open Object Editor path", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
				if (OpenProgramDialog.ShowDialog() == DialogResult.OK)
				{
					OpenProgramDialog.InitialDirectory = "";
					object_editor_path = OpenProgramDialog.FileName;
					Settings.Write("object_editor", object_editor_path, "settings");
				}
			}

			return object_editor_path;
		}

		string CheckNaN(string str)
        {
			if (str == "NaN")
				str = "0";
			return str;
		}

		// Interface
		private void CreateTextureGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(3, 3 + 126 * idx);
			GroupBox.Size = new System.Drawing.Size(421, 126);
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
			newTextBox.Size = new System.Drawing.Size(409, 23);
			newTextBox.Location = new System.Drawing.Point(6, 39);
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);
			newTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var newTextBox2 = new TextBox();
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.Size = new System.Drawing.Size(409, 23);
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
			GroupBox.Size = new System.Drawing.Size(421, 203);
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
			BoneNameTextBox.Size = new System.Drawing.Size(326, 58);
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
			ParentBoneNameTextBox.Size = new System.Drawing.Size(326, 58);
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
			MaterialTextBox.Size = new System.Drawing.Size(326, 58);
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
			MassTextBox.Text = CheckNaN(mass.ToString());
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
			CenterMassTextBoxX.Text = CheckNaN(center.x.ToString());
			CenterMassTextBoxX.Tag = "float";
			CenterMassTextBoxX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxY = new TextBox();
			CenterMassTextBoxY.Name = "CenterBoxY_" + idx;
			CenterMassTextBoxY.Size = new System.Drawing.Size(84, 58);
			CenterMassTextBoxY.Location = new System.Drawing.Point(182, 125);
			CenterMassTextBoxY.Text = CheckNaN(center.y.ToString());
			CenterMassTextBoxY.Tag = "float";
			CenterMassTextBoxY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var CenterMassTextBoxZ = new TextBox();
			CenterMassTextBoxZ.Name = "CenterBoxZ_" + idx;
			CenterMassTextBoxZ.Size = new System.Drawing.Size(84, 58);
			CenterMassTextBoxZ.Location = new System.Drawing.Point(277, 125);
			CenterMassTextBoxZ.Text = CheckNaN(center.z.ToString());
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
			PositionX.Text = CheckNaN(pos.x.ToString());
			PositionX.Tag = "float";
			PositionX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionY = new TextBox();
			PositionY.Name = "PositionY_" + idx;
			PositionY.Size = new System.Drawing.Size(84, 58);
			PositionY.Location = new System.Drawing.Point(182, 151);
			PositionY.Text = CheckNaN(pos.y.ToString());
			PositionY.Tag = "float";
			PositionY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var PositionZ = new TextBox();
			PositionZ.Name = "PositionZ_" + idx;
			PositionZ.Size = new System.Drawing.Size(84, 58);
			PositionZ.Location = new System.Drawing.Point(277, 151);
			PositionZ.Text = CheckNaN(pos.z.ToString());
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
			RotationX.Text = CheckNaN(rot.x.ToString());
			RotationX.Tag = "float";
			RotationX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationY = new TextBox();
			RotationY.Name = "RotationY_" + idx;
			RotationY.Size = new System.Drawing.Size(84, 58);
			RotationY.Location = new System.Drawing.Point(182, 177);
			RotationY.Text = CheckNaN(rot.y.ToString());
			RotationY.Tag = "float";
			RotationY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);

			var RotationZ = new TextBox();
			RotationZ.Name = "RotationZ_" + idx;
			RotationZ.Size = new System.Drawing.Size(84, 58);
			RotationZ.Location = new System.Drawing.Point(277, 177);
			RotationZ.Text = CheckNaN(rot.z.ToString());
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
    }
}
