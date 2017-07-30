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
 *  Copyright (c) 2011 - 2017 Simon Carter.  All Rights Reserved
 *
 *  Purpose:  
 *
 */
namespace Replication.Service.Forms
{
    partial class ConfigureReplicatedTable
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
            this.lblTableName = new System.Windows.Forms.Label();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.gbLogChanges = new System.Windows.Forms.GroupBox();
            this.cbDelete = new System.Windows.Forms.CheckBox();
            this.cbUpdate = new System.Windows.Forms.CheckBox();
            this.cbInsert = new System.Windows.Forms.CheckBox();
            this.lblTriggerName = new System.Windows.Forms.Label();
            this.txtTriggerName = new SharedControls.TextBoxEx();
            this.lblGenerator = new System.Windows.Forms.Label();
            this.cmbLocalGenerator = new System.Windows.Forms.ComboBox();
            this.cmbRemoteGenerator = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.clbColumns = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.udSortOrder = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbMultipleColumns = new System.Windows.Forms.RadioButton();
            this.rbForeignKey = new System.Windows.Forms.RadioButton();
            this.rbSingleColumn = new System.Windows.Forms.RadioButton();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblLocalIDColumn = new System.Windows.Forms.Label();
            this.cmbLocalIDColumn = new System.Windows.Forms.ComboBox();
            this.cbUpdateMaster = new System.Windows.Forms.CheckBox();
            this.gbLogChanges.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udSortOrder)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTableName
            // 
            this.lblTableName.AutoSize = true;
            this.lblTableName.Location = new System.Drawing.Point(9, 13);
            this.lblTableName.Name = "lblTableName";
            this.lblTableName.Size = new System.Drawing.Size(65, 13);
            this.lblTableName.TabIndex = 0;
            this.lblTableName.Text = "Table Name";
            // 
            // txtTableName
            // 
            this.txtTableName.Location = new System.Drawing.Point(9, 30);
            this.txtTableName.MaxLength = 50;
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.ReadOnly = true;
            this.txtTableName.Size = new System.Drawing.Size(213, 20);
            this.txtTableName.TabIndex = 1;
            // 
            // gbLogChanges
            // 
            this.gbLogChanges.Controls.Add(this.cbDelete);
            this.gbLogChanges.Controls.Add(this.cbUpdate);
            this.gbLogChanges.Controls.Add(this.cbInsert);
            this.gbLogChanges.Location = new System.Drawing.Point(9, 118);
            this.gbLogChanges.Name = "gbLogChanges";
            this.gbLogChanges.Size = new System.Drawing.Size(213, 104);
            this.gbLogChanges.TabIndex = 4;
            this.gbLogChanges.TabStop = false;
            this.gbLogChanges.Text = "Log Changes";
            // 
            // cbDelete
            // 
            this.cbDelete.AutoSize = true;
            this.cbDelete.Location = new System.Drawing.Point(16, 72);
            this.cbDelete.Name = "cbDelete";
            this.cbDelete.Size = new System.Drawing.Size(57, 17);
            this.cbDelete.TabIndex = 2;
            this.cbDelete.Text = "Delete";
            this.cbDelete.UseVisualStyleBackColor = true;
            // 
            // cbUpdate
            // 
            this.cbUpdate.AutoSize = true;
            this.cbUpdate.Location = new System.Drawing.Point(16, 49);
            this.cbUpdate.Name = "cbUpdate";
            this.cbUpdate.Size = new System.Drawing.Size(61, 17);
            this.cbUpdate.TabIndex = 1;
            this.cbUpdate.Text = "Update";
            this.cbUpdate.UseVisualStyleBackColor = true;
            // 
            // cbInsert
            // 
            this.cbInsert.AutoSize = true;
            this.cbInsert.Location = new System.Drawing.Point(16, 26);
            this.cbInsert.Name = "cbInsert";
            this.cbInsert.Size = new System.Drawing.Size(52, 17);
            this.cbInsert.TabIndex = 0;
            this.cbInsert.Text = "Insert";
            this.cbInsert.UseVisualStyleBackColor = true;
            // 
            // lblTriggerName
            // 
            this.lblTriggerName.AutoSize = true;
            this.lblTriggerName.Location = new System.Drawing.Point(9, 63);
            this.lblTriggerName.Name = "lblTriggerName";
            this.lblTriggerName.Size = new System.Drawing.Size(71, 13);
            this.lblTriggerName.TabIndex = 2;
            this.lblTriggerName.Text = "Trigger Name";
            // 
            // txtTriggerName
            // 
            this.txtTriggerName.AllowBackSpace = true;
            this.txtTriggerName.AllowCopy = true;
            this.txtTriggerName.AllowCut = true;
            this.txtTriggerName.AllowedCharacters = "abcdefghijklmnopqrstuvwxyz_ABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";
            this.txtTriggerName.AllowPaste = true;
            this.txtTriggerName.Location = new System.Drawing.Point(9, 80);
            this.txtTriggerName.MaxLength = 18;
            this.txtTriggerName.Name = "txtTriggerName";
            this.txtTriggerName.Size = new System.Drawing.Size(213, 20);
            this.txtTriggerName.TabIndex = 3;
            // 
            // lblGenerator
            // 
            this.lblGenerator.AutoSize = true;
            this.lblGenerator.Location = new System.Drawing.Point(9, 238);
            this.lblGenerator.Name = "lblGenerator";
            this.lblGenerator.Size = new System.Drawing.Size(114, 13);
            this.lblGenerator.TabIndex = 5;
            this.lblGenerator.Text = "Local Generator Name";
            // 
            // cmbLocalGenerator
            // 
            this.cmbLocalGenerator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLocalGenerator.FormattingEnabled = true;
            this.cmbLocalGenerator.Location = new System.Drawing.Point(9, 255);
            this.cmbLocalGenerator.Name = "cmbLocalGenerator";
            this.cmbLocalGenerator.Size = new System.Drawing.Size(213, 21);
            this.cmbLocalGenerator.TabIndex = 6;
            // 
            // cmbRemoteGenerator
            // 
            this.cmbRemoteGenerator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRemoteGenerator.FormattingEnabled = true;
            this.cmbRemoteGenerator.Location = new System.Drawing.Point(9, 305);
            this.cmbRemoteGenerator.Name = "cmbRemoteGenerator";
            this.cmbRemoteGenerator.Size = new System.Drawing.Size(213, 21);
            this.cmbRemoteGenerator.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 288);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Remote Generator Name";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.clbColumns);
            this.groupBox1.Location = new System.Drawing.Point(243, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(316, 209);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Columns";
            // 
            // clbColumns
            // 
            this.clbColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clbColumns.CheckOnClick = true;
            this.clbColumns.FormattingEnabled = true;
            this.clbColumns.Location = new System.Drawing.Point(6, 19);
            this.clbColumns.Name = "clbColumns";
            this.clbColumns.Size = new System.Drawing.Size(304, 184);
            this.clbColumns.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 340);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Sort Order";
            // 
            // udSortOrder
            // 
            this.udSortOrder.Location = new System.Drawing.Point(9, 357);
            this.udSortOrder.Maximum = new decimal(new int[] {
            32500,
            0,
            0,
            0});
            this.udSortOrder.Name = "udSortOrder";
            this.udSortOrder.Size = new System.Drawing.Size(111, 20);
            this.udSortOrder.TabIndex = 10;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbMultipleColumns);
            this.groupBox2.Controls.Add(this.rbForeignKey);
            this.groupBox2.Controls.Add(this.rbSingleColumn);
            this.groupBox2.Location = new System.Drawing.Point(243, 288);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(316, 53);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Primary Key Type";
            // 
            // rbMultipleColumns
            // 
            this.rbMultipleColumns.AutoSize = true;
            this.rbMultipleColumns.Location = new System.Drawing.Point(203, 20);
            this.rbMultipleColumns.Name = "rbMultipleColumns";
            this.rbMultipleColumns.Size = new System.Drawing.Size(104, 17);
            this.rbMultipleColumns.TabIndex = 2;
            this.rbMultipleColumns.TabStop = true;
            this.rbMultipleColumns.Text = "Multiple Columns";
            this.rbMultipleColumns.UseVisualStyleBackColor = true;
            // 
            // rbForeignKey
            // 
            this.rbForeignKey.AutoSize = true;
            this.rbForeignKey.Location = new System.Drawing.Point(116, 20);
            this.rbForeignKey.Name = "rbForeignKey";
            this.rbForeignKey.Size = new System.Drawing.Size(81, 17);
            this.rbForeignKey.TabIndex = 1;
            this.rbForeignKey.TabStop = true;
            this.rbForeignKey.Text = "Foreign Key";
            this.rbForeignKey.UseVisualStyleBackColor = true;
            // 
            // rbSingleColumn
            // 
            this.rbSingleColumn.AutoSize = true;
            this.rbSingleColumn.Location = new System.Drawing.Point(18, 20);
            this.rbSingleColumn.Name = "rbSingleColumn";
            this.rbSingleColumn.Size = new System.Drawing.Size(92, 17);
            this.rbSingleColumn.TabIndex = 0;
            this.rbSingleColumn.TabStop = true;
            this.rbSingleColumn.Text = "Single Column";
            this.rbSingleColumn.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(484, 395);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(403, 395);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblLocalIDColumn
            // 
            this.lblLocalIDColumn.AutoSize = true;
            this.lblLocalIDColumn.Location = new System.Drawing.Point(240, 238);
            this.lblLocalIDColumn.Name = "lblLocalIDColumn";
            this.lblLocalIDColumn.Size = new System.Drawing.Size(122, 13);
            this.lblLocalIDColumn.TabIndex = 12;
            this.lblLocalIDColumn.Text = "Local Unique ID Column";
            // 
            // cmbLocalIDColumn
            // 
            this.cmbLocalIDColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLocalIDColumn.FormattingEnabled = true;
            this.cmbLocalIDColumn.Location = new System.Drawing.Point(243, 255);
            this.cmbLocalIDColumn.Name = "cmbLocalIDColumn";
            this.cmbLocalIDColumn.Size = new System.Drawing.Size(315, 21);
            this.cmbLocalIDColumn.TabIndex = 13;
            // 
            // cbUpdateMaster
            // 
            this.cbUpdateMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbUpdateMaster.AutoSize = true;
            this.cbUpdateMaster.Location = new System.Drawing.Point(193, 399);
            this.cbUpdateMaster.Name = "cbUpdateMaster";
            this.cbUpdateMaster.Size = new System.Drawing.Size(172, 17);
            this.cbUpdateMaster.TabIndex = 15;
            this.cbUpdateMaster.Text = "Save changes to Master Table";
            this.cbUpdateMaster.UseVisualStyleBackColor = true;
            // 
            // ConfigureReplicatedTable
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(570, 428);
            this.Controls.Add(this.cmbLocalIDColumn);
            this.Controls.Add(this.lblLocalIDColumn);
            this.Controls.Add(this.cbUpdateMaster);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.udSortOrder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmbRemoteGenerator);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbLocalGenerator);
            this.Controls.Add(this.lblGenerator);
            this.Controls.Add(this.txtTriggerName);
            this.Controls.Add(this.lblTriggerName);
            this.Controls.Add(this.gbLogChanges);
            this.Controls.Add(this.txtTableName);
            this.Controls.Add(this.lblTableName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureReplicatedTable";
            this.SaveState = true;
            this.Text = "Configure Replicated Table";
            this.Shown += new System.EventHandler(this.ConfigureReplicatedTable_Shown);
            this.gbLogChanges.ResumeLayout(false);
            this.gbLogChanges.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.udSortOrder)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTableName;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.GroupBox gbLogChanges;
        private System.Windows.Forms.CheckBox cbDelete;
        private System.Windows.Forms.CheckBox cbUpdate;
        private System.Windows.Forms.CheckBox cbInsert;
        private System.Windows.Forms.Label lblTriggerName;
        private SharedControls.TextBoxEx txtTriggerName;
        private System.Windows.Forms.Label lblGenerator;
        private System.Windows.Forms.ComboBox cmbLocalGenerator;
        private System.Windows.Forms.ComboBox cmbRemoteGenerator;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox clbColumns;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udSortOrder;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbMultipleColumns;
        private System.Windows.Forms.RadioButton rbForeignKey;
        private System.Windows.Forms.RadioButton rbSingleColumn;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblLocalIDColumn;
        private System.Windows.Forms.ComboBox cmbLocalIDColumn;
        private System.Windows.Forms.CheckBox cbUpdateMaster;
    }
}