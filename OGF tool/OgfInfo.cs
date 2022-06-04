﻿using System;
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
            ModelTypeLabel.Text = (type == 3 ? "Animated" : "Rigid");

            SourceTextBox.Text = init_descr.m_source;
            ConverterTextBox.Text = init_descr.m_export_tool;
            CreatorTextBox.Text = init_descr.m_owner_name;
            EditorTextBox.Text = init_descr.m_export_modif_name_tool;

            System.DateTime dt_e = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_export_time);
            System.DateTime dt_c = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_creation_time);
            System.DateTime dt_m = new System.DateTime(1970, 1, 1).AddSeconds(init_descr.m_modified_time);

            ExportTimeLabel.Text = dt_e.ToShortDateString() + " " + dt_e.ToShortTimeString();
            CreationTimeLabel.Text = dt_c.ToShortDateString() + " " + dt_c.ToShortTimeString();
            ModifedTimeLabel.Text = dt_m.ToShortDateString() + " " + dt_m.ToShortTimeString();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            descr.m_source = SourceTextBox.Text;

            descr.m_source = SourceTextBox.Text;
            descr.m_export_tool = ConverterTextBox.Text;
            descr.m_owner_name = CreatorTextBox.Text;
            descr.m_export_modif_name_tool = EditorTextBox.Text;

            descr.m_export_time = 14;
            descr.m_creation_time = 15;
            descr.m_modified_time = 16;
            res = true;
        }
    }
}