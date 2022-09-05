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
using System.Threading;
using System.Diagnostics;


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

		delegate void TriangleParser(XRayLoader loader, OGF_Child child);

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

			OgfInfo.Enabled = false;
			View.Enabled = false;
			SaveMenuParam.Enabled = false;
			saveAsToolStripMenuItem.Enabled = false;
			motionToolsToolStripMenuItem.Enabled = false;
			openSkeletonInObjectEditorToolStripMenuItem.Enabled = false;
			ToolsMenuItem.Enabled = false;
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

		public string AppPath()
		{
			return Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\'));
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

				OgfInfo.Enabled = true;
				SaveMenuParam.Enabled = true;
				saveAsToolStripMenuItem.Enabled = true;
				ToolsMenuItem.Enabled = !OGF_V.IsDM;
				OgfInfo.Enabled = !OGF_V.IsDM;
				openSkeletonInObjectEditorToolStripMenuItem.Enabled = OGF_V.IsSkeleton();
				View.Enabled = OGF_V.IsSkeleton();
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

			OgfInfo.Enabled = OGF_V.description != null;

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
				if (OGF_V.bones != null)
				{
					BoneNamesBox.Clear();
					TabControl.Controls.Add(BoneNamesPage);

					BoneNamesBox.Text += $"Bones count : {OGF_V.bones.bone_names.Count}\n\n";
					for (int i = 0; i < OGF_V.bones.bone_names.Count; i++)
					{
						BoneNamesBox.Text += $"{i + 1}. {OGF_V.bones.bone_names[i]}";

						if (i != OGF_V.bones.bone_names.Count - 1)
							BoneNamesBox.Text += "\n";
					}

					// Ik Data
					TabControl.Controls.Add(BoneParamsPage);
				}

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

			if (OGF_V.bones != null)
			{
				for (int i = 0; i < OGF_V.bones.bone_names.Count; i++)
				{
					CreateBoneGroupBox(i, OGF_V.bones.bone_names[i], OGF_V.bones.parent_bone_names[i], OGF_V.ikdata.materials[i], OGF_V.ikdata.mass[i], OGF_V.ikdata.center_mass[i], OGF_V.ikdata.position[i], OGF_V.ikdata.rotation[i]);
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
				LabelBroken.Text = "Broken type: " + OGF_V.BrokenType.ToString();
				LabelBroken.Visible = OGF_V.BrokenType > 0;
			}

			UpdateModelFormat();
		}

		private void CopyParams()
		{
			if (OGF_V.motion_refs != null)
			{
				OGF_V.motion_refs.refs.Clear();

				if (IsTextCorrect(MotionRefsBox.Text))
				{
					for (int i = 0; i < MotionRefsBox.Lines.Count(); i++)
					{
						if (IsTextCorrect(MotionRefsBox.Lines[i]))
							OGF_V.motion_refs.refs.Add(GetCorrectString(MotionRefsBox.Lines[i]));
					}
				}
			}

			if (OGF_V.userdata != null)
			{
				OGF_V.userdata.userdata = "";

				if (IsTextCorrect(UserDataBox.Text))
				{
					for (int i = 0; i < UserDataBox.Lines.Count(); i++)
					{
						string ext = i == UserDataBox.Lines.Count() - 1 ? "" : "\r\n";
						OGF_V.userdata.userdata += UserDataBox.Lines[i] + ext;
					}
				}
			}

			if (OGF_V.lod != null)
			{
				OGF_V.lod.lod_path = "";

				if (IsTextCorrect(LodPathBox.Text))
					OGF_V.lod.lod_path = GetCorrectString(LodPathBox.Text);
			}

			UpdateModelType();
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
					file_bytes.AddRange(temp);

				if (OGF_V.description != null)
				{
					bool old_byte = OGF_V.description.four_byte;
					if (OGF_V.BrokenType > 0) // Если модель сломана, то восстанавливаем чанк с 8 байтными таймерами
						OGF_V.description.four_byte = false;

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_DESC));
					file_bytes.AddRange(BitConverter.GetBytes(OGF_V.description.chunk_size()));
					file_bytes.AddRange(OGF_V.description.data());

					OGF_V.description.four_byte = old_byte; // Восстанавливаем отображение колличества байтов у таймера

					fileStream.ReadBytes((int)(OGF_V.pos - fileStream.BaseStream.Position));

					fileStream.ReadBytes(4);
					uint new_size = fileStream.ReadUInt32();

					foreach (var ch in OGF_V.childs)
					{
						if (ch.to_delete)
							new_size -= ch.chunk_size + 8;
						else
							new_size += ch.NewSize();
					}

					file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_CHILDREN));
					file_bytes.AddRange(BitConverter.GetBytes(new_size));

					int child_id = 0;
					foreach (var ch in OGF_V.childs)
					{
						fileStream.ReadUInt32();
						uint size = fileStream.ReadUInt32();
						long old_pos = fileStream.BaseStream.Position; // Записываем начальную позицию чанка

						if (!ch.to_delete)
						{
							new_size = size + ch.NewSize();
							file_bytes.AddRange(BitConverter.GetBytes(child_id));
							file_bytes.AddRange(BitConverter.GetBytes(new_size));

							child_id++;
						}

						temp = fileStream.ReadBytes((int)(ch.pos - fileStream.BaseStream.Position));

						if (!ch.to_delete)
						{
							file_bytes.AddRange(temp);
							file_bytes.AddRange(ch.data());
						}

						fileStream.BaseStream.Position += ch.old_size + 8;
                        temp = fileStream.ReadBytes(8);
                        fileStream.ReadUInt32();

						if (!ch.to_delete)
						{
							file_bytes.AddRange(temp);
							file_bytes.AddRange(BitConverter.GetBytes(ch.link_type));
						}

						temp = fileStream.ReadBytes((int)ch.chunk_size - (int)(fileStream.BaseStream.Position - old_pos)); // Читаем все до конца чанка

						if (!ch.to_delete) // Дописываем чанк если он не претендент на удаление
							file_bytes.AddRange(temp);
					}
				}
				else
				{
					file_bytes.AddRange(OGF_V.childs[0].data());
					fileStream.ReadBytes(OGF_V.childs[0].old_size + 8);
				}

				if (OGF_V.IsSkeleton())
                {
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

							if (!OGF_V.motion_refs.soc)
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS2));
							else
								file_bytes.AddRange(BitConverter.GetBytes((uint)OGF.OGF4_S_MOTION_REFS));

							file_bytes.AddRange(BitConverter.GetBytes(OGF_V.motion_refs.chunk_size(OGF_V.motion_refs.soc)));

							if (!OGF_V.motion_refs.soc)
								file_bytes.AddRange(OGF_V.motion_refs.count());

							file_bytes.AddRange(OGF_V.motion_refs.data(OGF_V.motion_refs.soc));
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
					xr_loader.ReadUInt32();
					xr_loader.ReadFloat();
					xr_loader.ReadFloat();
					OGF_Child chld = new OGF_Child( 0, 0, 0, shader.Length + texture.Length + 2, texture, shader);
					chld.verts = xr_loader.ReadUInt32();
					chld.faces = xr_loader.ReadUInt32() / 3;

					for (int i = 0; i < chld.verts; i++)
					{
						SSkelVert Vert = new SSkelVert();
						Vert.offs = xr_loader.ReadVector();
						Vert.uv = xr_loader.ReadVector2();
						chld.Vertices.Add(Vert);
					}

					for (int i = 0; i < chld.faces; i++)
					{
						SSkelFace Face = new SSkelFace();
						Face.v[0] = (ushort)xr_loader.ReadUInt16();
						Face.v[1] = (ushort)xr_loader.ReadUInt16();
						Face.v[2] = (ushort)xr_loader.ReadUInt16();
						chld.Faces.Add(Face);
					}

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

				uint DescriptionSize = xr_loader.find_chunkSize((int)OGF.OGF4_S_DESC, false, true);
				if (DescriptionSize > 0)
				{
					OGF_C.description = new Description();
					OGF_C.description.pos = xr_loader.chunk_pos;

					// Читаем таймеры в 8 байт
					long reader_start_pos = xr_loader.reader.BaseStream.Position;
					OGF_C.description.m_source = xr_loader.read_stringZ();
					OGF_C.description.m_export_tool = xr_loader.read_stringZ();
					OGF_C.description.m_export_time = xr_loader.ReadInt64();
					OGF_C.description.m_owner_name = xr_loader.read_stringZ();
					OGF_C.description.m_creation_time = xr_loader.ReadInt64();
					OGF_C.description.m_export_modif_name_tool = xr_loader.read_stringZ();
					OGF_C.description.m_modified_time = xr_loader.ReadInt64();
					long description_end_pos = xr_loader.reader.BaseStream.Position;

					OGF_C.description.old_size = OGF_C.description.m_source.Length + 1 + OGF_C.description.m_export_tool.Length + 1 + 8 + OGF_C.description.m_owner_name.Length + 1 + 8 + OGF_C.description.m_export_modif_name_tool.Length + 1 + 8;

					if ((description_end_pos - reader_start_pos) != DescriptionSize) // Размер не состыковывается, пробуем читать 4 байта
                    {
						xr_loader.reader.BaseStream.Position = reader_start_pos;
						OGF_C.description.m_source = xr_loader.read_stringZ();
						OGF_C.description.m_export_tool = xr_loader.read_stringZ();
						OGF_C.description.m_export_time = xr_loader.ReadUInt32();
						OGF_C.description.m_owner_name = xr_loader.read_stringZ();
						OGF_C.description.m_creation_time = xr_loader.ReadUInt32();
						OGF_C.description.m_export_modif_name_tool = xr_loader.read_stringZ();
						OGF_C.description.m_modified_time = xr_loader.ReadUInt32();
						description_end_pos = xr_loader.reader.BaseStream.Position;

						OGF_C.description.old_size = OGF_C.description.m_source.Length + 1 + OGF_C.description.m_export_tool.Length + 1 + 4 + OGF_C.description.m_owner_name.Length + 1 + 4 + OGF_C.description.m_export_modif_name_tool.Length + 1 + 4;
						OGF_C.description.four_byte = true; // Ставим флаг на то что мы прочитали чанк с 4х байтными таймерами, если модель будет сломана то чинить чанк будем в 8 байт

						if ((description_end_pos - reader_start_pos) != DescriptionSize) // Все равно разный размер? Походу модель сломана
						{
							OGF_C.BrokenType = 1;

							// Чистим таймеры, так как прочитаны битые байты
							OGF_C.description.m_export_time = 0;
							OGF_C.description.m_creation_time = 0;
							OGF_C.description.m_modified_time = 0;
						}
					}
				}

				TriangleParser LoadTriangles = (loader, child) =>
				{
					if (loader.find_chunk((int)OGF.OGF4_VERTICES, false, true))
					{
						child.link_type = loader.ReadUInt32();
						child.verts = loader.ReadUInt32();

						uint temp_link = child.link_type;

						if (temp_link >= 0x12071980)
							temp_link /= 0x12071980;

						for (int i = 0; i < child.verts; i++)
						{
							SSkelVert Vert = new SSkelVert();
							switch (temp_link)
							{
								case 1:
									Vert.offs = loader.ReadVector();
									Vert.norm = loader.ReadVector();
									Vert.tang = loader.ReadVector();
									Vert.binorm = loader.ReadVector();
									Vert.uv = loader.ReadVector2();

									loader.ReadUInt32();
									break;
								case 2:
									loader.ReadUInt16();
									loader.ReadUInt16();

									Vert.offs = loader.ReadVector();
									Vert.norm = loader.ReadVector();
									Vert.tang = loader.ReadVector();
									Vert.binorm = loader.ReadVector();
									loader.ReadFloat();
									Vert.uv = loader.ReadVector2();
									break;
								case 3:
								case 4:
                                    for (int j = 0; j < temp_link; j++)
                                    {
                                        loader.ReadUInt16();
                                    }

                                    Vert.offs = loader.ReadVector();
									Vert.norm = loader.ReadVector();
									Vert.tang = loader.ReadVector();
									Vert.binorm = loader.ReadVector();

                                    for (int j = 0; j < temp_link - 1; j++)
                                    {
                                        loader.ReadFloat();
                                    }

                                    Vert.uv = loader.ReadVector2();
									break;
								default:
									Vert.offs = loader.ReadVector();
									Vert.norm = loader.ReadVector();
									Vert.uv = loader.ReadVector2();
									break;
							}
							child.Vertices.Add(Vert);
						}
					}

					if (loader.find_chunk((int)OGF.OGF4_INDICES, false, true))
					{
						child.faces = loader.ReadUInt32() / 3;

						for (uint i = 0; i < child.faces; i++)
						{
							SSkelFace Face = new SSkelFace();
							Face.v[0] = (ushort)loader.ReadUInt16();
							Face.v[1] = (ushort)loader.ReadUInt16();
							Face.v[2] = (ushort)loader.ReadUInt16();
							child.Faces.Add(Face);
						}
					}
				};

				bool bFindChunk = xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_CHILDREN, false, true));

				OGF_C.pos = xr_loader.chunk_pos;

				int id = 0;
				uint size;

				// Texture && shader
				if (bFindChunk)
				{
					while (true)
					{
						uint chunk_size = xr_loader.find_chunkSize(id);
						if (chunk_size == 0) break;

						Stream temp = xr_loader.reader.BaseStream;

						if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

						long pos = xr_loader.chunk_pos + OGF_C.pos + 16;
						size = xr_loader.find_chunkSize((int)OGF.OGF4_TEXTURE);

						if (size == 0) break;

						OGF_Child chld = new OGF_Child(xr_loader.chunk_pos + pos, pos - 8, chunk_size,(int)size, xr_loader.read_stringZ(), xr_loader.read_stringZ());

						LoadTriangles(xr_loader, chld);

						OGF_C.childs.Add(chld);

						id++;
						xr_loader.SetStream(temp);
					}
				}
				else
                {
					size = xr_loader.find_chunkSize((int)OGF.OGF4_TEXTURE, false, true);

					if (size != 0)
					{
						OGF_Child chld = new OGF_Child(0, 0, 0, (int)size, xr_loader.read_stringZ(), xr_loader.read_stringZ());

						LoadTriangles(xr_loader, chld);

						OGF_C.childs.Add(chld);
					}
				}

				if (OGF_C.childs.Count == 0)
				{
					MessageBox.Show("Unsupported OGF format! Can't find children chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				xr_loader.SetStream(r.BaseStream);

				if (OGF_C.IsSkeleton())
				{
					// Bones
					if (!xr_loader.find_chunk((int)OGF.OGF4_S_BONE_NAMES, false, true))
					{
						MessageBox.Show("Unsupported OGF format! Can't find bones chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						OGF_C.bones = new BoneData();
						OGF_C.bones.pos = xr_loader.chunk_pos;

						if (xr_loader.chunk_pos < OGF_C.pos)
							OGF_C.BrokenType = 2;

						uint count = xr_loader.ReadUInt32();
						OGF_C.bones.old_size += 4;

						for (; count != 0; count--)
						{
							string bone_name = xr_loader.read_stringZ();
							string parent_name = xr_loader.read_stringZ();
							byte[] obb = xr_loader.ReadBytes(60);    // Fobb
							OGF_C.bones.bone_names.Add(bone_name);
							OGF_C.bones.parent_bone_names.Add(parent_name);
							OGF_C.bones.fobb.Add(obb);

							OGF_C.bones.old_size += bone_name.Length + 1 + parent_name.Length + 1 + 60;
						}

						for (int i = 0; i < OGF_C.bones.bone_names.Count; i++)
						{
							List<int> childs = new List<int>();
							for (int j = 0; j < OGF_C.bones.parent_bone_names.Count; j++)
							{
								if (OGF_C.bones.parent_bone_names[j] == OGF_C.bones.bone_names[i])
								{
									childs.Add(j);
								}
							}
							OGF_C.bones.bone_childs.Add(childs);
						}
					}

					// Ik Data
					if (!xr_loader.find_chunk((int)OGF.OGF4_S_IKDATA, false, true))
					{
						MessageBox.Show("Unsupported OGF format! Can't find ik data chunk!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}
					else
					{
						OGF_C.ikdata = new IK_Data();

						for (int i = 0; i < OGF_C.bones.bone_names.Count; i++)
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

					// Userdata
					if (xr_loader.find_chunk((int)OGF.OGF4_S_USERDATA, false, true))
					{
						OGF_C.userdata = new UserData();
						OGF_C.userdata.pos = xr_loader.chunk_pos;
						OGF_C.userdata.userdata = xr_loader.read_stringZ();
						OGF_C.userdata.old_size = OGF_C.userdata.userdata.Length + 1;
					}

					// Lod ref
					if (xr_loader.find_chunk((int)OGF.OGF4_S_LODS, false, true))
					{
						OGF_C.lod = new Lod();
						OGF_C.lod.pos = xr_loader.chunk_pos;
						OGF_C.lod.lod_path = xr_loader.read_stringData(ref OGF_C.lod.data_str);
						OGF_C.lod.old_size = OGF_C.lod.lod_path.Length + (OGF_C.lod.data_str ? 2 : 1);
					}

					// Motion Refs
					bool v3 = xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS, false, true);

					if (v3 || xr_loader.find_chunk((int)OGF.OGF4_S_MOTION_REFS2, false, true))
					{
						OGF_C.motion_refs = new MotionRefs();
						OGF_C.motion_refs.pos = xr_loader.chunk_pos;
						OGF_C.motion_refs.refs = new List<string>();

						if (v3)
						{
							OGF_C.motion_refs.soc = true;

							string motions = xr_loader.read_stringZ();
							string motion = "";
							List<string> refs = new List<string>();

							for (int i = 0; i < motions.Length; i++)
							{
								if (motions[i] != ',')
									motion += motions[i];
								else
								{
									refs.Add(motion);
									motion = "";
								}

							}

							if (motion != "")
								refs.Add(motion);

							OGF_C.motion_refs.refs.AddRange(refs);
							OGF_C.motion_refs.old_size = motions.Length + 1;
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

					//Motions
					if (xr_loader.find_chunk((int)OGF.OGF4_S_MOTIONS, false, true))
					{
						xr_loader.reader.BaseStream.Position -= 8;
						Cur_OMF = xr_loader.ReadBytes((int)xr_loader.reader.BaseStream.Length - (int)xr_loader.reader.BaseStream.Position);
					}

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
				}
			}
			return true;
		}


		private void SaveAsObj(string filename)
		{
			using (StreamWriter writer = File.CreateText(filename))
			{
				uint v_offs = 0;
				uint childs = 0;

				string mtl_name = Path.ChangeExtension(filename, ".mtl");
				SaveMtl(mtl_name);

				writer.WriteLine("# This file uses meters as units for non-parametric coordinates.");
				writer.WriteLine("mtllib " + Path.GetFileName(mtl_name));
				foreach (var ch in OGF_V.childs)
				{
					if (ch.to_delete) continue;

					writer.WriteLine("g " + childs.ToString());
					writer.WriteLine("usemtl " + Path.GetFileName(ch.m_texture));
					childs++;

					for (int i = 0; i < ch.verts; i++)
                    {
						writer.WriteLine("v " + vPUSH(transform_tiny_Z(ch.Vertices[i].offs)));
					}

					for (int i = 0; i < ch.verts; i++)
					{
						float x = ch.Vertices[i].uv[0];
						float y = Math.Abs(1.0f - ch.Vertices[i].uv[1]);
						writer.WriteLine("vt " + x.ToString("0.000000") + " " + y.ToString("0.000000"));
					}

					for (int i = 0; i < ch.verts; i++)
					{
						writer.WriteLine("vn " + vPUSH(transform_dir_Z(ch.Vertices[i].norm)));
					}

					for (int i = 0; i < ch.verts; i++)
					{
						writer.WriteLine("vg "+ vPUSH(transform_dir_Z(ch.Vertices[i].tang)));
					}

					for (int i = 0; i < ch.verts; i++)
					{
						writer.WriteLine("vb "+ vPUSH(transform_dir_Z(ch.Vertices[i].binorm)));
					}

					foreach (var f_it in ch.Faces)
					{
						string tmp = "f " + (v_offs+f_it.v[2]+1).ToString() + "/" + (v_offs+f_it.v[2]+1).ToString() + "/" + (v_offs+f_it.v[2]+1).ToString() + " " + (v_offs+f_it.v[1]+1).ToString() + "/" + (v_offs+f_it.v[1]+1).ToString() + "/" + (v_offs+f_it.v[1]+1).ToString() + " " + (v_offs+f_it.v[0]+1).ToString() + "/" + (v_offs+f_it.v[0]+1).ToString() + "/" + (v_offs+f_it.v[0]+1).ToString();
						writer.WriteLine(tmp);
					}
					v_offs += (uint)ch.verts;
				}
				writer.Close();
			}
		}

		private void SaveMtl(string filename)
        {
			using (StreamWriter writer = File.CreateText(filename))
			{
				foreach (var ch in OGF_V.childs)
				{
					if (ch.to_delete) continue;

					writer.WriteLine("newmtl " + Path.GetFileName(ch.m_texture));
					writer.WriteLine("Ka  0 0 0");
					writer.WriteLine("Kd  1 1 1");
					writer.WriteLine("Ks  0 0 0");
					writer.WriteLine("map_Kd " + Path.GetFileName(ch.m_texture) + ".png\n");
				}
				writer.Close();
			}
		}

		private float[] transform_tiny_Z(float[] vec)
        {
			float[] dest = new float[3];
			dest[0] = vec[0];
			dest[1] = vec[1];
			dest[2] = vec[2] * -1.0f;
			return dest;
		}

		private float[] transform_dir_Z(float[] vec)
		{
			float[] dest = new float[3];
			dest[0] = vec[0];
			dest[1] = vec[1];
			dest[2] = vec[2] * -1.0f;
			return dest;
		}

		private string vPUSH(float[] vec)
        {
			return vec[0].ToString("0.000000") + " " + vec[1].ToString("0.000000") + " " + vec[2].ToString("0.000000");
        }

		private byte[] w_string(string str)
		{
			List<byte> temp = new List<byte>();

			temp.AddRange(Encoding.Default.GetBytes(str));
			temp.Add(0);

			return temp.ToArray();
		}

		private void TextBoxKeyDown(object sender, KeyEventArgs e)
		{
			bKeyIsDown = true;
		}

		private void ButtonFilter(object sender, EventArgs e)
		{
			Button curBox = sender as Button;

			string currentField = curBox.Name.ToString().Split('_')[0];
			int idx = Convert.ToInt32(curBox.Name.ToString().Split('_')[1]);

			switch (currentField)
			{
				case "DeleteButton":
					int chld_cnt = OGF_V.childs.Count;

					foreach (var ch in OGF_V.childs)
                    {
						if (ch.to_delete) chld_cnt--;
					}

					if (chld_cnt > 1 || OGF_V.childs[idx].to_delete)
					{
						OGF_V.childs[idx].to_delete = !OGF_V.childs[idx].to_delete;
						curBox.Text = (OGF_V.childs[idx].to_delete ? "Return Mesh" : "Delete Mesh");
					}
					else
                    {
						MessageBox.Show("Can't delete last mesh!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					break;
			}
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
						OGF_V.bones.bone_names[idx] = curBox.Text;

						for (int i = 0; i < OGF_V.bones.parent_bone_names.Count; i++)
                        {
							if (BoneParamsPage.Controls.Count < i) continue;
							for (int j = 0; j < OGF_V.bones.bone_childs[idx].Count; j++)
                            {
								if (OGF_V.bones.bone_childs[idx][j] == i)
								{
									var MainGroup = BoneParamsPage.Controls[i];
									OGF_V.bones.parent_bone_names[i] = curBox.Text;
									MainGroup.Controls[2].Text = OGF_V.bones.parent_bone_names[i];
								}
							}
                        }

						BoneNamesBox.Clear();
						BoneNamesBox.Text += $"Bones count : {OGF_V.bones.bone_names.Count}\n\n";

						for (int i = 0; i < OGF_V.bones.bone_names.Count; i++)
						{
							BoneNamesBox.Text += $"{i + 1}. {OGF_V.bones.bone_names[i]}";
							if (i != OGF_V.bones.bone_names.Count - 1)
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

			List<byte> flags = new List<byte>();
			if (Current_OMF != null)
			{
				var xr_loader = new XRayLoader();
				using (var fileStream = new BinaryReader(new MemoryStream(Current_OMF)))
				{
					xr_loader.SetStream(fileStream.BaseStream);

					if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
					{
						int id = 0;

						while (true)
						{
							if (!xr_loader.find_chunk(id)) break;

							Stream temp = xr_loader.reader.BaseStream;

							if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

							if (id == 0)
								xr_loader.ReadUInt32();
							else
							{
								xr_loader.read_stringZ();
								xr_loader.ReadUInt32();
								flags.Add(xr_loader.ReadByte());
							}

							id++;
							xr_loader.SetStream(temp);
						}
					}
				}
			}

			OgfInfo Info = new OgfInfo(OGF_V, IsTextCorrect(MotionRefsBox.Text), flags);
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
				OGF_V.description.four_byte = Info.descr.four_byte;
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
			else if (format == 6)
				SaveAsObj(filename);
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

		private void objToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SaveObjDialog.ShowDialog() == DialogResult.OK)
			{
				SaveTools(SaveObjDialog.FileName, 6);
				SaveObjDialog.InitialDirectory = "";
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
			UserDataBox.Clear();
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
				UpdateModelFormat();

				// Обновляем визуал интерфейса моушн рефов
				CreateMotionRefsButton.Visible = false; 
				MotionRefsBox.Visible = true;
				MotionRefsBox.Clear();

				if (OGF_V.motion_refs == null)
					OGF_V.motion_refs = new MotionRefs();
			}
		}

		private void CreateLodButton_Click(object sender, EventArgs e)
		{
			CreateLodButton.Visible = false;
			LodPathBox.Clear();
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

			switch (TabControl.Controls[TabControl.SelectedIndex].Name)
			{
				case "UserDataPage":
					{
						if (!IsTextCorrect(UserDataBox.Text))
						{
							CreateUserdataButton.Visible = true;
							UserDataBox.Visible = false;
						}
						break;
					}
				case "MotionRefsPage":
					{
						if (!IsTextCorrect(MotionRefsBox.Text))
						{
							CreateMotionRefsButton.Visible = true;
							MotionRefsBox.Visible = false;
						}
						break;
					}
				case "LodPage":
					{
						if (!IsTextCorrect(LodPathBox.Text))
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
						UpdateModelFormat();
						break;
                    }
            }
		}

		private void AppendOMFButton_Click(object sender, EventArgs e)
        {
			if (!IsTextCorrect(MotionRefsBox.Text) && (OGF_V.motion_refs == null || OGF_V.motion_refs.refs.Count() == 0) || (IsTextCorrect(MotionRefsBox.Text) || OGF_V.motion_refs != null && OGF_V.motion_refs.refs.Count() > 0) && MessageBox.Show("Build-in motions will remove motion refs, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
			UpdateModelFormat();
		}

        private void deleteChunkToolStripMenuItem_Click(object sender, EventArgs e)
        {
			MotionBox.Visible = false;
			AppendOMFButton.Visible = true;
			Current_OMF = null;
			MotionBox.Clear();
			OGF_V.motions = "";
			UpdateModelType();
			UpdateModelFormat();
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
						if (!IsTextCorrect(MotionRefsBox.Text) && (OGF_V.motion_refs == null || OGF_V.motion_refs.refs.Count() == 0) || (IsTextCorrect(MotionRefsBox.Text) || OGF_V.motion_refs != null && OGF_V.motion_refs.refs.Count() > 0) && MessageBox.Show("Build-in motions will remove motion refs, continue?", "OGF Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
						{
							Current_OMF = SecondOmfByte;
							UpdateUi = true;
						}
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

		private void ToolsClick(object sender, EventArgs e)
		{
			motionToolsToolStripMenuItem.Enabled = Current_OMF != null;
		}

		private void ChangeRefsFormat(object sender, EventArgs e)
		{
			if (OGF_V != null)
			{
				OGF_V.IsCopModel = !OGF_V.IsCopModel;

				if (OGF_V.IsCopModel)
                {
					if (OGF_V.motion_refs != null)
						OGF_V.motion_refs.soc = false;

					foreach (var ch in OGF_V.childs)
					{
						if (ch.link_type >= 0x12071980)
							ch.link_type /= 0x12071980;
					}
				}
				else
                {
					uint links = 0;

					foreach (var ch in OGF_V.childs)
					{
						if (ch.link_type >= 0x12071980)
							links = Math.Max(links, ch.link_type / 0x12071980);
						else
							links = Math.Max(links, ch.link_type);
					}

					if (links > 2)
                    {
						MessageBox.Show("Can't convert to SoC. Model has more than 2 links!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						OGF_V.IsCopModel = !OGF_V.IsCopModel;
						return;
					}

					if (Current_OMF != null)
					{
						var xr_loader = new XRayLoader();
						using (var fileStream = new BinaryReader(new MemoryStream(Current_OMF)))
						{
							xr_loader.SetStream(fileStream.BaseStream);

							if (xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk((int)OGF.OGF4_S_MOTIONS, false, true)))
							{
								int id = 0;

								while (true)
								{
									if (!xr_loader.find_chunk(id)) break;

									Stream temp = xr_loader.reader.BaseStream;

									if (!xr_loader.SetData(xr_loader.find_and_return_chunk_in_chunk(id, false, true))) break;

									if (id == 0)
										xr_loader.ReadUInt32();
									else
									{
										xr_loader.read_stringZ();
										xr_loader.ReadUInt32();
										byte flags = xr_loader.ReadByte();

										bool key16bit = (flags & (int)MotionKeyFlags.flTKey16IsBit) == (int)MotionKeyFlags.flTKey16IsBit;
										bool keynocompressbit = (flags & (int)MotionKeyFlags.flTKeyFFT_Bit) == (int)MotionKeyFlags.flTKeyFFT_Bit;

										if (key16bit || keynocompressbit)
                                        {
											MessageBox.Show("Build-in motions are in " + (keynocompressbit ? "no compression" : "16 bit compression") + " format, not supported in SoC.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
											break;
										}
									}

									id++;
									xr_loader.SetStream(temp);
								}
							}
						}
					}

					if (OGF_V.motion_refs != null)
						OGF_V.motion_refs.soc = true;

					foreach (var ch in OGF_V.childs)
					{
						if (ch.link_type < 0x12071980)
							ch.link_type *= 0x12071980;
					}
				}

				UpdateModelFormat();
			}
		}

		private void UpdateModelType()
        {
			if (OGF_V == null || OGF_V.description == null) return;

			if (OGF_V.bones == null)
				OGF_V.m_model_type = 1;
			else if (Current_OMF == null && !IsTextCorrect(MotionRefsBox.Text))
				OGF_V.m_model_type = 10;
			else
				OGF_V.m_model_type = 3;

			// Апдейтим экспорт аним тут, т.к. при любом изменении омф вызывается эта функция
			omfToolStripMenuItem.Enabled = Current_OMF != null;
			sklToolStripMenuItem.Enabled = Current_OMF != null;
			sklsToolStripMenuItem.Enabled = Current_OMF != null;
		}

		private void UpdateModelFormat()
		{
			CurrentFormat.Enabled = (OGF_V != null && !OGF_V.IsDM && OGF_V.IsSkeleton());

			if (!CurrentFormat.Enabled)
			{
				CurrentFormat.Text = "Model Format: All";
				return;
			}

			uint links = 0;

			foreach (var ch in OGF_V.childs)
				links = Math.Max(links, ch.link_type);

			OGF_V.IsCopModel = (IsTextCorrect(MotionRefsBox.Text) && OGF_V.motion_refs != null && !OGF_V.motion_refs.soc || !IsTextCorrect(MotionRefsBox.Text)) && links < 0x12071980;

			CurrentFormat.Text = "Model Format: " + (OGF_V.IsCopModel ? "CoP" : "SoC");
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

		string CheckNaN(float val)
        {
			if (val.ToString() == "NaN")
				return "0";
			return ((decimal)val).ToString();
		}

		private void RichTextBoxImgDefender(object sender, KeyEventArgs e)
		{
			RichTextBox TextBox = sender as RichTextBox;
			if (e.Control && e.KeyCode == Keys.V)
			{
				if (Clipboard.ContainsText())
					TextBox.Paste(DataFormats.GetFormat(DataFormats.Text));
				e.Handled = true;
			}
		}

		private bool IsTextCorrect(string text)
        {
			foreach (char ch in text)
            {
				if (ch > 0x1F && ch != 0x20)
					return true;
			}
			return false;
        }

		private string GetCorrectString(string text)
		{
			string ret_text = "", symbols = "";
			bool started = false;
			foreach (char ch in text)
			{
				if (started)
                {
					if (ch <= 0x1F || ch == 0x20)
						symbols += ch;
                    else
					{
						ret_text += symbols + ch;
						symbols = "";
					}
				}
				else if (ch > 0x1F && ch != 0x20)
				{
					started = true;
					ret_text += ch;
				}
			}
			return ret_text;
		}

		// Interface
		private void CreateTextureGroupBox(int idx)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(TexturesGropuBox.Location.X, TexturesGropuBox.Location.Y + (TexturesGropuBox.Size.Height + 2) * idx);
			GroupBox.Size = TexturesGropuBox.Size;
			GroupBox.Text = "Mesh: [" + idx + "]";
			GroupBox.Name = "TextureGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GroupBox.Dock = TexturesGropuBox.Dock;
			CreateTextureBoxes(idx, GroupBox);
			CreateTextureLabels(idx, GroupBox);
			TexturesPage.Controls.Add(GroupBox);
		}

		private void CreateTextureBoxes(int idx, GroupBox box)
		{
			var newTextBox = new TextBox();
			newTextBox.Name = "textureBox_" + idx;
			newTextBox.Size = TexturesTextBoxEx.Size;
			newTextBox.Location = TexturesTextBoxEx.Location;
			newTextBox.TextChanged += new System.EventHandler(this.TextBoxFilter);
			newTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var newTextBox2 = new TextBox();
			newTextBox2.Name = "shaderBox_" + idx;
			newTextBox2.Size = ShaderTextBoxEx.Size;
			newTextBox2.Location = ShaderTextBoxEx.Location;
			newTextBox2.TextChanged += new System.EventHandler(this.TextBoxFilter);
			newTextBox2.Anchor = AnchorStyles.Left | AnchorStyles.Right;;

			var newButton = new Button();
			newButton.Name = "DeleteButton_" + idx;
			newButton.Size = DeleteMesh.Size;
			newButton.Location = DeleteMesh.Location;
			newButton.Click += new System.EventHandler(this.ButtonFilter);
			newButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			newButton.Text = DeleteMesh.Text;

			box.Controls.Add(newTextBox);
			box.Controls.Add(newTextBox2);
			box.Controls.Add(newButton);
		}

		private void CreateTextureLabels(int idx, GroupBox box)
		{
			var newLbl = new Label();
			newLbl.Name = "textureLbl_" + idx;
			newLbl.Text = "Texture Path:";
			newLbl.Location = TexturesPathLabelEx.Location;

			var newLbl2 = new Label();
			newLbl2.Name = "shaderLbl_" + idx;
			newLbl2.Text = "Shader Name:";
			newLbl2.Location = ShaderNameLabelEx.Location;

			var newLbl3 = new Label();
			newLbl3.Name = "FacesLbl_" + idx;
			newLbl3.Text = FaceLabel.Text + OGF_V.childs[idx].faces.ToString();
			newLbl3.Size = new Size(FaceLabel.Size.Width + (OGF_V.childs[idx].faces.ToString().Length * 6), FaceLabel.Size.Height);
			newLbl3.Location = new Point(FaceLabel.Location.X - (OGF_V.childs[idx].faces.ToString().Length * 6), FaceLabel.Location.Y);
			newLbl3.Anchor = FaceLabel.Anchor;
			newLbl3.TextAlign = FaceLabel.TextAlign;

			var newLbl4 = new Label();
			newLbl4.Name = "VertsLbl_" + idx;
			newLbl4.Text = VertsLabel.Text + OGF_V.childs[idx].verts.ToString();
			newLbl4.Size = new Size(VertsLabel.Size.Width + (OGF_V.childs[idx].verts.ToString().Length * 6), VertsLabel.Size.Height);
			newLbl4.Location = new Point(VertsLabel.Location.X - (OGF_V.childs[idx].verts.ToString().Length * 6) - (OGF_V.childs[idx].faces.ToString().Length * 6), VertsLabel.Location.Y);
			newLbl4.Anchor = VertsLabel.Anchor;
			newLbl4.TextAlign = VertsLabel.TextAlign;

			box.Controls.Add(newLbl);
			box.Controls.Add(newLbl2);
			box.Controls.Add(newLbl3);
			box.Controls.Add(newLbl4);
		}

		private void CreateBoneGroupBox(int idx, string bone_name, string parent_bone_name, string material, float mass, Fvector center, Fvector pos, Fvector rot)
		{
			var GroupBox = new GroupBox();
			GroupBox.Location = new System.Drawing.Point(BoneParamsGroupBox.Location.X, BoneParamsGroupBox.Location.Y + (BoneParamsGroupBox.Size.Height + 2) * idx);
			GroupBox.Size = BoneParamsGroupBox.Size;
			GroupBox.Text = "Bone id: [" + idx + "]";
			GroupBox.Name = "BoneGrpBox_" + idx;
			GroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GroupBox.Dock = BoneParamsGroupBox.Dock;

			CreateBoneTextBox(idx, GroupBox, bone_name, parent_bone_name, material, mass, center, pos, rot);
			BoneParamsPage.Controls.Add(GroupBox);
		}

		private void CreateBoneTextBox(int idx, GroupBox box, string bone_name, string parent_bone_name, string material, float mass, Fvector center, Fvector pos, Fvector rot)
		{
			var BoneNameTextBox = new TextBox();
			BoneNameTextBox.Name = "boneBox_" + idx;
			BoneNameTextBox.Size = BoneNameTextBoxEx.Size;
			BoneNameTextBox.Location = BoneNameTextBoxEx.Location;
			BoneNameTextBox.Text = bone_name;
			BoneNameTextBox.Tag = "string";
			BoneNameTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			BoneNameTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			BoneNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var BoneNameLabel = new Label();
			BoneNameLabel.Name = "boneLabel_" + idx;
			BoneNameLabel.Size = BoneNameLabelEx.Size;
			BoneNameLabel.Location = BoneNameLabelEx.Location;
			BoneNameLabel.Text = "Bone Name:";

			var ParentBoneNameTextBox = new TextBox();
			ParentBoneNameTextBox.Name = "ParentboneBox_" + idx;
			ParentBoneNameTextBox.Size = ParentBoneTextBoxEx.Size;
			ParentBoneNameTextBox.Location = ParentBoneTextBoxEx.Location;
			ParentBoneNameTextBox.Text = parent_bone_name;
			ParentBoneNameTextBox.Tag = "string";
			ParentBoneNameTextBox.ReadOnly = true;
			ParentBoneNameTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var ParentBoneNameLabel = new Label();
			ParentBoneNameLabel.Name = "ParentboneLabel_" + idx;
			ParentBoneNameLabel.Size = ParentBoneLabelEx.Size;
			ParentBoneNameLabel.Location = ParentBoneLabelEx.Location;
			ParentBoneNameLabel.Text = "Parent Bone:";

			var MaterialTextBox = new TextBox();
			MaterialTextBox.Name = "MaterialBox_" + idx;
			MaterialTextBox.Size = MaterialTextBoxEx.Size;
			MaterialTextBox.Location = MaterialTextBoxEx.Location;
			MaterialTextBox.Text = material;
			MaterialTextBox.Tag = "string";
			MaterialTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			MaterialTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			MaterialTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

			var MaterialLabel = new Label();
			MaterialLabel.Name = "MaterialLabel_" + idx;
			MaterialLabel.Size = MaterialLabelEx.Size;
			MaterialLabel.Location = MaterialLabelEx.Location;
			MaterialLabel.Text = "Material:";

			var MassTextBox = new TextBox();
			MassTextBox.Name = "MassBox_" + idx;
			MassTextBox.Size = MassTextBoxEx.Size;
			MassTextBox.Location = MassTextBoxEx.Location;
			MassTextBox.Text = CheckNaN(mass);
			MassTextBox.Tag = "float";
			MassTextBox.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			MassTextBox.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			MassTextBox.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var MassLabel = new Label();
			MassLabel.Name = "MassLabel_" + idx;
			MassLabel.Size = MassLabelEx.Size;
			MassLabel.Location = MassLabelEx.Location;
			MassLabel.Text = "Mass:";

			var LayoutPanel = new TableLayoutPanel();
			LayoutPanel.Name = "LayoutPanel_" + idx;
			LayoutPanel.Size = BonesParamsPanel.Size;
			LayoutPanel.Location = BonesParamsPanel.Location;
			LayoutPanel.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			LayoutPanel.RowCount = 4;
			for (int x = 0; x < LayoutPanel.RowCount; x++)
				LayoutPanel.RowStyles.Add(new RowStyle() { Height = 25.0f, SizeType = SizeType.Percent });

			LayoutPanel.ColumnCount = 3;
			for (int x = 0; x < LayoutPanel.ColumnCount; x++)
				LayoutPanel.ColumnStyles.Add(new ColumnStyle() { Width = 33.33f, SizeType = SizeType.Percent });

			var CenterMassTextBoxX = new TextBox();
			CenterMassTextBoxX.Name = "CenterBoxX_" + idx;
			CenterMassTextBoxX.Size = CenterOfMassXTextBox.Size;
			CenterMassTextBoxX.Location = CenterOfMassXTextBox.Location;
			CenterMassTextBoxX.Text = CheckNaN(center.x);
			CenterMassTextBoxX.Tag = "float";
			CenterMassTextBoxX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			CenterMassTextBoxX.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var CenterMassTextBoxY = new TextBox();
			CenterMassTextBoxY.Name = "CenterBoxY_" + idx;
			CenterMassTextBoxY.Size = CenterOfMassYTextBox.Size;
			CenterMassTextBoxY.Location = CenterOfMassYTextBox.Location;
			CenterMassTextBoxY.Text = CheckNaN(center.y);
			CenterMassTextBoxY.Tag = "float";
			CenterMassTextBoxY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			CenterMassTextBoxY.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var CenterMassTextBoxZ = new TextBox();
			CenterMassTextBoxZ.Name = "CenterBoxZ_" + idx;
			CenterMassTextBoxZ.Size = CenterOfMassZTextBox.Size;
			CenterMassTextBoxZ.Location = CenterOfMassZTextBox.Location;
			CenterMassTextBoxZ.Text = CheckNaN(center.z);
			CenterMassTextBoxZ.Tag = "float";
			CenterMassTextBoxZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			CenterMassTextBoxZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			CenterMassTextBoxZ.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var CenterMassLabel = new Label();
			CenterMassLabel.Name = "CenterMassLabel_" + idx;
			CenterMassLabel.Size = CenterOfMassLabelEx.Size;
			CenterMassLabel.Location = CenterOfMassLabelEx.Location;
			CenterMassLabel.Text = "Center of Mass:";

			var PositionX = new TextBox();
			PositionX.Name = "PositionX_" + idx;
			PositionX.Size = PositionXTextBox.Size;
			PositionX.Location = PositionXTextBox.Location;
			PositionX.Text = CheckNaN(pos.x);
			PositionX.Tag = "float";
			PositionX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			PositionX.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var PositionY = new TextBox();
			PositionY.Name = "PositionY_" + idx;
			PositionY.Size = PositionYTextBox.Size;
			PositionY.Location = PositionYTextBox.Location;
			PositionY.Text = CheckNaN(pos.y);
			PositionY.Tag = "float";
			PositionY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			PositionY.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var PositionZ = new TextBox();
			PositionZ.Name = "PositionZ_" + idx;
			PositionZ.Size = PositionZTextBox.Size;
			PositionZ.Location = PositionZTextBox.Location;
			PositionZ.Text = CheckNaN(pos.z);
			PositionZ.Tag = "float";
			PositionZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			PositionZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			PositionZ.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var PositionLabel = new Label();
			PositionLabel.Name = "PositionLabel_" + idx;
			PositionLabel.Size = PositionLabelEx.Size;
			PositionLabel.Location = PositionLabelEx.Location;
			PositionLabel.Text = "Position:";

			var RotationX = new TextBox();
			RotationX.Name = "RotationX_" + idx;
			RotationX.Size = RotationXTextBox.Size;
			RotationX.Location = RotationXTextBox.Location;
			RotationX.Text = CheckNaN(rot.x);
			RotationX.Tag = "float";
			RotationX.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationX.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			RotationX.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var RotationY = new TextBox();
			RotationY.Name = "RotationY_" + idx;
			RotationY.Size = RotationYTextBox.Size;
			RotationY.Location = RotationYTextBox.Location;
			RotationY.Text = CheckNaN(rot.y);
			RotationY.Tag = "float";
			RotationY.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationY.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			RotationY.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var RotationZ = new TextBox();
			RotationZ.Name = "RotationZ_" + idx;
			RotationZ.Size = RotationZTextBox.Size;
			RotationZ.Location = RotationZTextBox.Location;
			RotationZ.Text = CheckNaN(rot.z);
			RotationZ.Tag = "float";
			RotationZ.TextChanged += new System.EventHandler(this.TextBoxBonesFilter);
			RotationZ.KeyDown += new KeyEventHandler(this.TextBoxKeyDown);
			RotationZ.Anchor = AnchorStyles.Top |AnchorStyles.Left | AnchorStyles.Right;

			var RotationLabel = new Label();
			RotationLabel.Name = "RotationLabel_" + idx;
			RotationLabel.Size = RotationLabelEx.Size;
			RotationLabel.Location = RotationLabelEx.Location;
			RotationLabel.Text = "Rotation:";

			LayoutPanel.Controls.Add(MassTextBox, 0, 0);
			LayoutPanel.Controls.Add(CenterMassTextBoxX, 0, 1);
			LayoutPanel.Controls.Add(CenterMassTextBoxY, 1, 1);
			LayoutPanel.Controls.Add(CenterMassTextBoxZ, 2, 1);
			LayoutPanel.Controls.Add(PositionX, 0, 2);
			LayoutPanel.Controls.Add(PositionY, 1, 2);
			LayoutPanel.Controls.Add(PositionZ, 2, 2);
			LayoutPanel.Controls.Add(RotationX, 0, 3);
			LayoutPanel.Controls.Add(RotationY, 1, 3);
			LayoutPanel.Controls.Add(RotationZ, 2, 3);

			box.Controls.Add(LayoutPanel);

			box.Controls.Add(BoneNameTextBox);
			box.Controls.Add(ParentBoneNameTextBox);
			box.Controls.Add(MaterialTextBox);

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
