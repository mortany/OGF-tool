using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OGF_tool
{
    public partial class OgfInfo : Form
    {
        public Description descr = new Description();
        public bool res = false;
        public OgfInfo(Description init_descr, byte vers, byte type)
        {
            InitializeComponent();

            OgfVersLabel.Text = vers.ToString();
            ModelTypeLabel.Text = (type == 1 ? "Object" : type == 3 ? "Animated" : "Rigid");
            ByteLabel.Text = (init_descr.four_byte ? "4 byte" : "8 byte");
            RepairTimersButton.Enabled = init_descr.four_byte;

            SourceTextBox.Text = init_descr.m_source;
            ConverterTextBox.Text = init_descr.m_export_tool;
            CreatorTextBox.Text = init_descr.m_owner_name;
            EditorTextBox.Text = init_descr.m_export_modif_name_tool;

            System.DateTime dt_e = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_export_time);
            System.DateTime dt_c = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_creation_time);
            System.DateTime dt_m = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_modified_time);

            ExportTimeDate.Value = dt_e;
            CreationTimeDate.Value = dt_c;
            ModifedTimeDate.Value = dt_m;
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            descr.m_source = SourceTextBox.Text;

            descr.m_source = SourceTextBox.Text;
            descr.m_export_tool = ConverterTextBox.Text;
            descr.m_owner_name = CreatorTextBox.Text;
            descr.m_export_modif_name_tool = EditorTextBox.Text;

            descr.m_export_time = Convert.ToUInt32(ExportTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            descr.m_creation_time = Convert.ToUInt32(CreationTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            descr.m_modified_time = Convert.ToUInt32(ModifedTimeDate.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            res = true;
        }

        private void RepairTimersButton_Click(object sender, EventArgs e)
        {
            descr.four_byte = false;
            RepairTimersButton.Enabled = false;
            ByteLabel.Text = "8 byte";
        }
    }
}
