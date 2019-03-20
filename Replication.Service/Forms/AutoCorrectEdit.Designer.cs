/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 
 * The contents of this file are subject to the GNU General Public License
 * v3.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy * of the License at 
 * https://github.com/k3ldar/FbReplicationEngine/blob/master/LICENSE
 *
 * Software distributed under the License is distributed on an
 * "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
 * or implied. See the License for the specific language governing
 * rights and limitations under the License.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2011 - 2019 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  
 *
 */
namespace Replication.Service.Forms
{
    partial class AutoCorrectEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
#if DEBUG
            System.GC.SuppressFinalize(this);
#endif
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbTables = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbKeys = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbOptions = new System.Windows.Forms.ComboBox();
            this.cmbTargetColumn = new System.Windows.Forms.ComboBox();
            this.lblTargetColumn = new System.Windows.Forms.Label();
            this.lstAvailable = new System.Windows.Forms.ListBox();
            this.lblAvailableColumns = new System.Windows.Forms.Label();
            this.lblSelectedColumns = new System.Windows.Forms.Label();
            this.lstSelected = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblSelectSQL = new System.Windows.Forms.Label();
            this.txtSelectSQL = new System.Windows.Forms.TextBox();
            this.lblUpdateSQL = new System.Windows.Forms.Label();
            this.txtUpdateSQL = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbTables
            // 
            this.cmbTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTables.FormattingEnabled = true;
            this.cmbTables.Location = new System.Drawing.Point(9, 25);
            this.cmbTables.Name = "cmbTables";
            this.cmbTables.Size = new System.Drawing.Size(204, 21);
            this.cmbTables.TabIndex = 1;
            this.cmbTables.SelectedIndexChanged += new System.EventHandler(this.cmbTables_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Table Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(222, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Key Name / Error";
            // 
            // cmbKeys
            // 
            this.cmbKeys.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeys.FormattingEnabled = true;
            this.cmbKeys.Location = new System.Drawing.Point(225, 25);
            this.cmbKeys.Name = "cmbKeys";
            this.cmbKeys.Size = new System.Drawing.Size(198, 21);
            this.cmbKeys.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Option";
            // 
            // cmbOptions
            // 
            this.cmbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOptions.FormattingEnabled = true;
            this.cmbOptions.Location = new System.Drawing.Point(9, 74);
            this.cmbOptions.Name = "cmbOptions";
            this.cmbOptions.Size = new System.Drawing.Size(201, 21);
            this.cmbOptions.TabIndex = 5;
            this.cmbOptions.SelectedIndexChanged += new System.EventHandler(this.cmbOptions_SelectedIndexChanged);
            // 
            // cmbTargetColumn
            // 
            this.cmbTargetColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTargetColumn.FormattingEnabled = true;
            this.cmbTargetColumn.Location = new System.Drawing.Point(223, 74);
            this.cmbTargetColumn.Name = "cmbTargetColumn";
            this.cmbTargetColumn.Size = new System.Drawing.Size(200, 21);
            this.cmbTargetColumn.TabIndex = 7;
            this.cmbTargetColumn.SelectedIndexChanged += new System.EventHandler(this.cmbTargetColumn_SelectedIndexChanged);
            // 
            // lblTargetColumn
            // 
            this.lblTargetColumn.AutoSize = true;
            this.lblTargetColumn.Location = new System.Drawing.Point(220, 56);
            this.lblTargetColumn.Name = "lblTargetColumn";
            this.lblTargetColumn.Size = new System.Drawing.Size(76, 13);
            this.lblTargetColumn.TabIndex = 6;
            this.lblTargetColumn.Text = "Target Column";
            // 
            // lstAvailable
            // 
            this.lstAvailable.FormattingEnabled = true;
            this.lstAvailable.Location = new System.Drawing.Point(9, 126);
            this.lstAvailable.Name = "lstAvailable";
            this.lstAvailable.Size = new System.Drawing.Size(153, 108);
            this.lstAvailable.TabIndex = 9;
            this.lstAvailable.DoubleClick += new System.EventHandler(this.btnAdd_Click);
            // 
            // lblAvailableColumns
            // 
            this.lblAvailableColumns.AutoSize = true;
            this.lblAvailableColumns.Location = new System.Drawing.Point(9, 108);
            this.lblAvailableColumns.Name = "lblAvailableColumns";
            this.lblAvailableColumns.Size = new System.Drawing.Size(93, 13);
            this.lblAvailableColumns.TabIndex = 8;
            this.lblAvailableColumns.Text = "Available Columns";
            // 
            // lblSelectedColumns
            // 
            this.lblSelectedColumns.AutoSize = true;
            this.lblSelectedColumns.Location = new System.Drawing.Point(251, 108);
            this.lblSelectedColumns.Name = "lblSelectedColumns";
            this.lblSelectedColumns.Size = new System.Drawing.Size(92, 13);
            this.lblSelectedColumns.TabIndex = 12;
            this.lblSelectedColumns.Text = "Selected Columns";
            // 
            // lstSelected
            // 
            this.lstSelected.FormattingEnabled = true;
            this.lstSelected.Location = new System.Drawing.Point(254, 127);
            this.lstSelected.Name = "lstSelected";
            this.lstSelected.Size = new System.Drawing.Size(169, 108);
            this.lstSelected.TabIndex = 13;
            this.lstSelected.DoubleClick += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(173, 142);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 10;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(173, 171);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 11;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblSelectSQL
            // 
            this.lblSelectSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSelectSQL.AutoSize = true;
            this.lblSelectSQL.Location = new System.Drawing.Point(9, 241);
            this.lblSelectSQL.Name = "lblSelectSQL";
            this.lblSelectSQL.Size = new System.Drawing.Size(61, 13);
            this.lblSelectSQL.TabIndex = 14;
            this.lblSelectSQL.Text = "Select SQL";
            // 
            // txtSelectSQL
            // 
            this.txtSelectSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSelectSQL.Location = new System.Drawing.Point(9, 259);
            this.txtSelectSQL.Multiline = true;
            this.txtSelectSQL.Name = "txtSelectSQL";
            this.txtSelectSQL.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.txtSelectSQL.Size = new System.Drawing.Size(414, 61);
            this.txtSelectSQL.TabIndex = 15;
            // 
            // lblUpdateSQL
            // 
            this.lblUpdateSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUpdateSQL.AutoSize = true;
            this.lblUpdateSQL.Location = new System.Drawing.Point(9, 325);
            this.lblUpdateSQL.Name = "lblUpdateSQL";
            this.lblUpdateSQL.Size = new System.Drawing.Size(66, 13);
            this.lblUpdateSQL.TabIndex = 16;
            this.lblUpdateSQL.Text = "Update SQL";
            // 
            // txtUpdateSQL
            // 
            this.txtUpdateSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtUpdateSQL.Location = new System.Drawing.Point(9, 343);
            this.txtUpdateSQL.Multiline = true;
            this.txtUpdateSQL.Name = "txtUpdateSQL";
            this.txtUpdateSQL.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.txtUpdateSQL.Size = new System.Drawing.Size(414, 61);
            this.txtUpdateSQL.TabIndex = 17;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(347, 419);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(266, 419);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // AutoCorrectEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 454);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtUpdateSQL);
            this.Controls.Add(this.lblUpdateSQL);
            this.Controls.Add(this.txtSelectSQL);
            this.Controls.Add(this.lblSelectSQL);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lblSelectedColumns);
            this.Controls.Add(this.lstSelected);
            this.Controls.Add(this.lblAvailableColumns);
            this.Controls.Add(this.lstAvailable);
            this.Controls.Add(this.cmbTargetColumn);
            this.Controls.Add(this.lblTargetColumn);
            this.Controls.Add(this.cmbOptions);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbKeys);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbTables);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoCorrectEdit";
            this.ShowInTaskbar = false;
            this.Text = "Auto Correct Rule";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbTables;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbKeys;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbOptions;
        private System.Windows.Forms.ComboBox cmbTargetColumn;
        private System.Windows.Forms.Label lblTargetColumn;
        private System.Windows.Forms.ListBox lstAvailable;
        private System.Windows.Forms.Label lblAvailableColumns;
        private System.Windows.Forms.Label lblSelectedColumns;
        private System.Windows.Forms.ListBox lstSelected;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblSelectSQL;
        private System.Windows.Forms.TextBox txtSelectSQL;
        private System.Windows.Forms.Label lblUpdateSQL;
        private System.Windows.Forms.TextBox txtUpdateSQL;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}