//namespace GUI
//{
//    partial class NewShip
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.label1 = new System.Windows.Forms.Label();
//            this.shipNameTextBox = new System.Windows.Forms.TextBox();
//            this.label2 = new System.Windows.Forms.Label();
//            this.priceTextBox = new System.Windows.Forms.TextBox();
//            this.label3 = new System.Windows.Forms.Label();
//            this.shieldsTextBox = new System.Windows.Forms.TextBox();
//            this.label4 = new System.Windows.Forms.Label();
//            this.hullTextBox = new System.Windows.Forms.TextBox();
//            this.label5 = new System.Windows.Forms.Label();
//            this.energyTextBox = new System.Windows.Forms.TextBox();
//            this.label6 = new System.Windows.Forms.Label();
//            this.cargoTextBox = new System.Windows.Forms.TextBox();
//            this.label7 = new System.Windows.Forms.Label();
//            this.valueTextBox = new System.Windows.Forms.TextBox();
//            this.label8 = new System.Windows.Forms.Label();
//            this.descriptionTextBox = new System.Windows.Forms.TextBox();
//            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
//            this.clientSyncedBool = new System.Windows.Forms.CheckBox();
//            this.enumBox = new System.Windows.Forms.ComboBox();
//            this.label16 = new System.Windows.Forms.Label();
//            this.classTextBox = new System.Windows.Forms.TextBox();
//            this.label15 = new System.Windows.Forms.Label();
//            this.thrustTypeTextBox = new System.Windows.Forms.TextBox();
//            this.label14 = new System.Windows.Forms.Label();
//            this.graphicTextBox = new System.Windows.Forms.TextBox();
//            this.label13 = new System.Windows.Forms.Label();
//            this.regenTextBox = new System.Windows.Forms.TextBox();
//            this.label12 = new System.Windows.Forms.Label();
//            this.turnTextBox = new System.Windows.Forms.TextBox();
//            this.label11 = new System.Windows.Forms.Label();
//            this.thrustTextBox = new System.Windows.Forms.TextBox();
//            this.label10 = new System.Windows.Forms.Label();
//            this.topSpeedTextBox = new System.Windows.Forms.TextBox();
//            this.label9 = new System.Windows.Forms.Label();
//            this.addToDatabaseButton = new System.Windows.Forms.Button();
//            this.databaseRefresh = new System.Windows.Forms.Button();
//            this.removeship = new System.Windows.Forms.Button();
//            this.shipGridView = new System.Windows.Forms.DataGridView();
//            this.pushToDB = new System.Windows.Forms.Button();
//            this.graphicComboBox = new System.Windows.Forms.ComboBox();
//            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
//            this.splitContainer1.Panel1.SuspendLayout();
//            this.splitContainer1.Panel2.SuspendLayout();
//            this.splitContainer1.SuspendLayout();
//            ((System.ComponentModel.ISupportInitialize)(this.shipGridView)).BeginInit();
//            this.SuspendLayout();
//            // 
//            // label1
//            // 
//            this.label1.AutoSize = true;
//            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label1.Location = new System.Drawing.Point(3, 0);
//            this.label1.Name = "label1";
//            this.label1.Size = new System.Drawing.Size(72, 13);
//            this.label1.TabIndex = 1;
//            this.label1.Text = "Ship Name:";
//            this.label1.Click += new System.EventHandler(this.label1_Click);
//            // 
//            // shipNameTextBox
//            // 
//            this.shipNameTextBox.Location = new System.Drawing.Point(81, 0);
//            this.shipNameTextBox.Name = "shipNameTextBox";
//            this.shipNameTextBox.Size = new System.Drawing.Size(236, 20);
//            this.shipNameTextBox.TabIndex = 0;
//            this.shipNameTextBox.Text = "My Awesome Ship";
//            // 
//            // label2
//            // 
//            this.label2.AutoSize = true;
//            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label2.Location = new System.Drawing.Point(3, 26);
//            this.label2.Name = "label2";
//            this.label2.Size = new System.Drawing.Size(40, 13);
//            this.label2.TabIndex = 3;
//            this.label2.Text = "Price:";
//            // 
//            // priceTextBox
//            // 
//            this.priceTextBox.Location = new System.Drawing.Point(81, 26);
//            this.priceTextBox.Name = "priceTextBox";
//            this.priceTextBox.Size = new System.Drawing.Size(236, 20);
//            this.priceTextBox.TabIndex = 2;
//            this.priceTextBox.Text = "10000";
//            // 
//            // label3
//            // 
//            this.label3.AutoSize = true;
//            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label3.Location = new System.Drawing.Point(3, 52);
//            this.label3.Name = "label3";
//            this.label3.Size = new System.Drawing.Size(52, 13);
//            this.label3.TabIndex = 5;
//            this.label3.Text = "Shields:";
//            // 
//            // shieldsTextBox
//            // 
//            this.shieldsTextBox.Location = new System.Drawing.Point(81, 52);
//            this.shieldsTextBox.Name = "shieldsTextBox";
//            this.shieldsTextBox.Size = new System.Drawing.Size(236, 20);
//            this.shieldsTextBox.TabIndex = 4;
//            this.shieldsTextBox.Text = "8000";
//            // 
//            // label4
//            // 
//            this.label4.AutoSize = true;
//            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label4.Location = new System.Drawing.Point(3, 78);
//            this.label4.Name = "label4";
//            this.label4.Size = new System.Drawing.Size(33, 13);
//            this.label4.TabIndex = 7;
//            this.label4.Text = "Hull:";
//            // 
//            // hullTextBox
//            // 
//            this.hullTextBox.Location = new System.Drawing.Point(81, 78);
//            this.hullTextBox.Name = "hullTextBox";
//            this.hullTextBox.Size = new System.Drawing.Size(236, 20);
//            this.hullTextBox.TabIndex = 6;
//            this.hullTextBox.Text = "8000";
//            // 
//            // label5
//            // 
//            this.label5.AutoSize = true;
//            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label5.Location = new System.Drawing.Point(3, 104);
//            this.label5.Name = "label5";
//            this.label5.Size = new System.Drawing.Size(50, 13);
//            this.label5.TabIndex = 9;
//            this.label5.Text = "Energy:";
//            // 
//            // energyTextBox
//            // 
//            this.energyTextBox.Location = new System.Drawing.Point(81, 104);
//            this.energyTextBox.Name = "energyTextBox";
//            this.energyTextBox.Size = new System.Drawing.Size(236, 20);
//            this.energyTextBox.TabIndex = 8;
//            this.energyTextBox.Text = "1000";
//            // 
//            // label6
//            // 
//            this.label6.AutoSize = true;
//            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label6.Location = new System.Drawing.Point(3, 130);
//            this.label6.Name = "label6";
//            this.label6.Size = new System.Drawing.Size(44, 13);
//            this.label6.TabIndex = 11;
//            this.label6.Text = "Cargo:";
//            // 
//            // cargoTextBox
//            // 
//            this.cargoTextBox.Location = new System.Drawing.Point(81, 130);
//            this.cargoTextBox.Name = "cargoTextBox";
//            this.cargoTextBox.Size = new System.Drawing.Size(236, 20);
//            this.cargoTextBox.TabIndex = 10;
//            this.cargoTextBox.Text = "50";
//            // 
//            // label7
//            // 
//            this.label7.AutoSize = true;
//            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label7.Location = new System.Drawing.Point(3, 156);
//            this.label7.Name = "label7";
//            this.label7.Size = new System.Drawing.Size(43, 13);
//            this.label7.TabIndex = 13;
//            this.label7.Text = "Value:";
//            // 
//            // valueTextBox
//            // 
//            this.valueTextBox.Location = new System.Drawing.Point(81, 156);
//            this.valueTextBox.Name = "valueTextBox";
//            this.valueTextBox.Size = new System.Drawing.Size(236, 20);
//            this.valueTextBox.TabIndex = 12;
//            this.valueTextBox.Text = "1";
//            // 
//            // label8
//            // 
//            this.label8.AutoSize = true;
//            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label8.Location = new System.Drawing.Point(3, 394);
//            this.label8.Name = "label8";
//            this.label8.Size = new System.Drawing.Size(75, 13);
//            this.label8.TabIndex = 15;
//            this.label8.Text = "Description:";
//            // 
//            // descriptionTextBox
//            // 
//            this.descriptionTextBox.Location = new System.Drawing.Point(83, 391);
//            this.descriptionTextBox.Multiline = true;
//            this.descriptionTextBox.Name = "descriptionTextBox";
//            this.descriptionTextBox.Size = new System.Drawing.Size(236, 99);
//            this.descriptionTextBox.TabIndex = 14;
//            this.descriptionTextBox.Text = "A basic ship for flying.";
//            // 
//            // splitContainer1
//            // 
//            this.splitContainer1.Location = new System.Drawing.Point(1, 1);
//            this.splitContainer1.Name = "splitContainer1";
//            // 
//            // splitContainer1.Panel1
//            // 
//            this.splitContainer1.Panel1.Controls.Add(this.graphicComboBox);
//            this.splitContainer1.Panel1.Controls.Add(this.clientSyncedBool);
//            this.splitContainer1.Panel1.Controls.Add(this.enumBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label16);
//            this.splitContainer1.Panel1.Controls.Add(this.classTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label15);
//            this.splitContainer1.Panel1.Controls.Add(this.thrustTypeTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label14);
//            this.splitContainer1.Panel1.Controls.Add(this.label13);
//            this.splitContainer1.Panel1.Controls.Add(this.regenTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label12);
//            this.splitContainer1.Panel1.Controls.Add(this.turnTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label11);
//            this.splitContainer1.Panel1.Controls.Add(this.thrustTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label10);
//            this.splitContainer1.Panel1.Controls.Add(this.topSpeedTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label9);
//            this.splitContainer1.Panel1.Controls.Add(this.addToDatabaseButton);
//            this.splitContainer1.Panel1.Controls.Add(this.descriptionTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label8);
//            this.splitContainer1.Panel1.Controls.Add(this.valueTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label7);
//            this.splitContainer1.Panel1.Controls.Add(this.cargoTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label6);
//            this.splitContainer1.Panel1.Controls.Add(this.energyTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label5);
//            this.splitContainer1.Panel1.Controls.Add(this.hullTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label4);
//            this.splitContainer1.Panel1.Controls.Add(this.shieldsTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label3);
//            this.splitContainer1.Panel1.Controls.Add(this.priceTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label2);
//            this.splitContainer1.Panel1.Controls.Add(this.shipNameTextBox);
//            this.splitContainer1.Panel1.Controls.Add(this.label1);
//            // 
//            // splitContainer1.Panel2
//            // 
//            this.splitContainer1.Panel2.Controls.Add(this.databaseRefresh);
//            this.splitContainer1.Panel2.Controls.Add(this.removeship);
//            this.splitContainer1.Panel2.Controls.Add(this.shipGridView);
//            this.splitContainer1.Panel2.Controls.Add(this.pushToDB);
//            this.splitContainer1.Panel2.Controls.Add(this.graphicTextBox);
//            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
//            this.splitContainer1.Size = new System.Drawing.Size(975, 522);
//            this.splitContainer1.SplitterDistance = 322;
//            this.splitContainer1.TabIndex = 2;
//            // 
//            // clientSyncedBool
//            // 
//            this.clientSyncedBool.AutoSize = true;
//            this.clientSyncedBool.Location = new System.Drawing.Point(83, 500);
//            this.clientSyncedBool.Name = "clientSyncedBool";
//            this.clientSyncedBool.Size = new System.Drawing.Size(114, 17);
//            this.clientSyncedBool.TabIndex = 36;
//            this.clientSyncedBool.Text = "Synced On Client?";
//            this.clientSyncedBool.UseVisualStyleBackColor = true;
//            // 
//            // enumBox
//            // 
//            this.enumBox.FormattingEnabled = true;
//            this.enumBox.Location = new System.Drawing.Point(81, 364);
//            this.enumBox.Name = "enumBox";
//            this.enumBox.Size = new System.Drawing.Size(236, 21);
//            this.enumBox.TabIndex = 33;
//            // 
//            // label16
//            // 
//            this.label16.AutoSize = true;
//            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label16.Location = new System.Drawing.Point(3, 364);
//            this.label16.Name = "label16";
//            this.label16.Size = new System.Drawing.Size(42, 13);
//            this.label16.TabIndex = 32;
//            this.label16.Text = "Enum:";
//            // 
//            // classTextBox
//            // 
//            this.classTextBox.Location = new System.Drawing.Point(81, 338);
//            this.classTextBox.Name = "classTextBox";
//            this.classTextBox.Size = new System.Drawing.Size(236, 20);
//            this.classTextBox.TabIndex = 29;
//            this.classTextBox.Text = "Fighter";
//            // 
//            // label15
//            // 
//            this.label15.AutoSize = true;
//            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label15.Location = new System.Drawing.Point(3, 338);
//            this.label15.Name = "label15";
//            this.label15.Size = new System.Drawing.Size(41, 13);
//            this.label15.TabIndex = 30;
//            this.label15.Text = "Class:";
//            // 
//            // thrustTypeTextBox
//            // 
//            this.thrustTypeTextBox.Location = new System.Drawing.Point(81, 312);
//            this.thrustTypeTextBox.Name = "thrustTypeTextBox";
//            this.thrustTypeTextBox.Size = new System.Drawing.Size(236, 20);
//            this.thrustTypeTextBox.TabIndex = 27;
//            this.thrustTypeTextBox.Text = "Basic";
//            // 
//            // label14
//            // 
//            this.label14.AutoSize = true;
//            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label14.Location = new System.Drawing.Point(3, 312);
//            this.label14.Name = "label14";
//            this.label14.Size = new System.Drawing.Size(79, 13);
//            this.label14.TabIndex = 28;
//            this.label14.Text = "Thrust Type:";
//            // 
//            // graphicTextBox
//            // 
//            this.graphicTextBox.Location = new System.Drawing.Point(-9, 234);
//            this.graphicTextBox.Name = "graphicTextBox";
//            this.graphicTextBox.Size = new System.Drawing.Size(236, 20);
//            this.graphicTextBox.TabIndex = 25;
//            this.graphicTextBox.Text = "Penguin";
//            // 
//            // label13
//            // 
//            this.label13.AutoSize = true;
//            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label13.Location = new System.Drawing.Point(3, 286);
//            this.label13.Name = "label13";
//            this.label13.Size = new System.Drawing.Size(55, 13);
//            this.label13.TabIndex = 26;
//            this.label13.Text = "Graphic:";
//            this.label13.Click += new System.EventHandler(this.label13_Click);
//            // 
//            // regenTextBox
//            // 
//            this.regenTextBox.Location = new System.Drawing.Point(81, 260);
//            this.regenTextBox.Name = "regenTextBox";
//            this.regenTextBox.Size = new System.Drawing.Size(236, 20);
//            this.regenTextBox.TabIndex = 23;
//            this.regenTextBox.Text = "0.3";
//            // 
//            // label12
//            // 
//            this.label12.AutoSize = true;
//            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label12.Location = new System.Drawing.Point(3, 260);
//            this.label12.Name = "label12";
//            this.label12.Size = new System.Drawing.Size(79, 13);
//            this.label12.TabIndex = 24;
//            this.label12.Text = "Regen Rate:";
//            // 
//            // turnTextBox
//            // 
//            this.turnTextBox.Location = new System.Drawing.Point(81, 234);
//            this.turnTextBox.Name = "turnTextBox";
//            this.turnTextBox.Size = new System.Drawing.Size(236, 20);
//            this.turnTextBox.TabIndex = 21;
//            this.turnTextBox.Text = "3.8";
//            // 
//            // label11
//            // 
//            this.label11.AutoSize = true;
//            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label11.Location = new System.Drawing.Point(3, 234);
//            this.label11.Name = "label11";
//            this.label11.Size = new System.Drawing.Size(68, 13);
//            this.label11.TabIndex = 22;
//            this.label11.Text = "Turn Rate:";
//            // 
//            // thrustTextBox
//            // 
//            this.thrustTextBox.Location = new System.Drawing.Point(81, 208);
//            this.thrustTextBox.Name = "thrustTextBox";
//            this.thrustTextBox.Size = new System.Drawing.Size(236, 20);
//            this.thrustTextBox.TabIndex = 19;
//            this.thrustTextBox.Text = "100";
//            // 
//            // label10
//            // 
//            this.label10.AutoSize = true;
//            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label10.Location = new System.Drawing.Point(3, 208);
//            this.label10.Name = "label10";
//            this.label10.Size = new System.Drawing.Size(47, 13);
//            this.label10.TabIndex = 20;
//            this.label10.Text = "Thrust:";
//            // 
//            // topSpeedTextBox
//            // 
//            this.topSpeedTextBox.Location = new System.Drawing.Point(81, 182);
//            this.topSpeedTextBox.Name = "topSpeedTextBox";
//            this.topSpeedTextBox.Size = new System.Drawing.Size(236, 20);
//            this.topSpeedTextBox.TabIndex = 17;
//            this.topSpeedTextBox.Text = "250";
//            // 
//            // label9
//            // 
//            this.label9.AutoSize = true;
//            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
//            this.label9.Location = new System.Drawing.Point(3, 182);
//            this.label9.Name = "label9";
//            this.label9.Size = new System.Drawing.Size(73, 13);
//            this.label9.TabIndex = 18;
//            this.label9.Text = "Top Speed:";
//            // 
//            // addToDatabaseButton
//            // 
//            this.addToDatabaseButton.Location = new System.Drawing.Point(209, 496);
//            this.addToDatabaseButton.Name = "addToDatabaseButton";
//            this.addToDatabaseButton.Size = new System.Drawing.Size(108, 23);
//            this.addToDatabaseButton.TabIndex = 16;
//            this.addToDatabaseButton.Text = "Add To Database";
//            this.addToDatabaseButton.UseVisualStyleBackColor = true;
//            // 
//            // databaseRefresh
//            // 
//            this.databaseRefresh.Location = new System.Drawing.Point(411, 496);
//            this.databaseRefresh.Name = "databaseRefresh";
//            this.databaseRefresh.Size = new System.Drawing.Size(107, 23);
//            this.databaseRefresh.TabIndex = 3;
//            this.databaseRefresh.Text = "Refresh From DB";
//            this.databaseRefresh.UseVisualStyleBackColor = true;
//            // 
//            // removeship
//            // 
//            this.removeship.Location = new System.Drawing.Point(4, 496);
//            this.removeship.Name = "removeship";
//            this.removeship.Size = new System.Drawing.Size(97, 23);
//            this.removeship.TabIndex = 2;
//            this.removeship.Text = "Remove Ship";
//            this.removeship.UseVisualStyleBackColor = true;
//            // 
//            // shipGridView
//            // 
//            this.shipGridView.AllowUserToAddRows = false;
//            this.shipGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
//            this.shipGridView.Location = new System.Drawing.Point(4, 4);
//            this.shipGridView.Name = "shipGridView";
//            this.shipGridView.Size = new System.Drawing.Size(632, 486);
//            this.shipGridView.TabIndex = 1;
//            this.shipGridView.Resize += new System.EventHandler(this.shipGridView_Resize);
//            // 
//            // pushToDB
//            // 
//            this.pushToDB.Location = new System.Drawing.Point(524, 496);
//            this.pushToDB.Name = "pushToDB";
//            this.pushToDB.Size = new System.Drawing.Size(97, 23);
//            this.pushToDB.TabIndex = 0;
//            this.pushToDB.Text = "Push To DB";
//            this.pushToDB.UseVisualStyleBackColor = true;
//            // 
//            // graphicComboBox
//            // 
//            this.graphicComboBox.FormattingEnabled = true;
//            this.graphicComboBox.Location = new System.Drawing.Point(81, 286);
//            this.graphicComboBox.Name = "graphicComboBox";
//            this.graphicComboBox.Size = new System.Drawing.Size(236, 21);
//            this.graphicComboBox.TabIndex = 37;
//            // 
//            // NewShip
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.AutoSize = true;
//            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
//            this.ClientSize = new System.Drawing.Size(983, 523);
//            this.Controls.Add(this.splitContainer1);
//            this.MaximumSize = new System.Drawing.Size(1403, 561);
//            this.MinimumSize = new System.Drawing.Size(973, 561);
//            this.Name = "NewShip";
//            this.Text = "New Ship to MongoDB";
//            this.splitContainer1.Panel1.ResumeLayout(false);
//            this.splitContainer1.Panel1.PerformLayout();
//            this.splitContainer1.Panel2.ResumeLayout(false);
//            this.splitContainer1.Panel2.PerformLayout();
//            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
//            this.splitContainer1.ResumeLayout(false);
//            ((System.ComponentModel.ISupportInitialize)(this.shipGridView)).EndInit();
//            this.ResumeLayout(false);

//        }

//        #endregion

//        private System.Windows.Forms.Label label1;
//        private System.Windows.Forms.TextBox shipNameTextBox;
//        private System.Windows.Forms.Label label2;
//        private System.Windows.Forms.TextBox priceTextBox;
//        private System.Windows.Forms.Label label3;
//        private System.Windows.Forms.TextBox shieldsTextBox;
//        private System.Windows.Forms.Label label4;
//        private System.Windows.Forms.TextBox hullTextBox;
//        private System.Windows.Forms.Label label5;
//        private System.Windows.Forms.TextBox energyTextBox;
//        private System.Windows.Forms.Label label6;
//        private System.Windows.Forms.TextBox cargoTextBox;
//        private System.Windows.Forms.Label label7;
//        private System.Windows.Forms.TextBox valueTextBox;
//        private System.Windows.Forms.Label label8;
//        private System.Windows.Forms.TextBox descriptionTextBox;
//        private System.Windows.Forms.SplitContainer splitContainer1;
//        private System.Windows.Forms.TextBox graphicTextBox;
//        private System.Windows.Forms.Label label13;
//        private System.Windows.Forms.TextBox regenTextBox;
//        private System.Windows.Forms.Label label12;
//        private System.Windows.Forms.TextBox turnTextBox;
//        private System.Windows.Forms.Label label11;
//        private System.Windows.Forms.TextBox thrustTextBox;
//        private System.Windows.Forms.Label label10;
//        private System.Windows.Forms.TextBox topSpeedTextBox;
//        private System.Windows.Forms.Label label9;
//        private System.Windows.Forms.Button addToDatabaseButton;
//        private System.Windows.Forms.TextBox classTextBox;
//        private System.Windows.Forms.Label label15;
//        private System.Windows.Forms.TextBox thrustTypeTextBox;
//        private System.Windows.Forms.Label label14;
//        private System.Windows.Forms.Button pushToDB;
//        private System.Windows.Forms.DataGridView shipGridView;
//        private System.Windows.Forms.Button removeship;
//        private System.Windows.Forms.Button databaseRefresh;
//        private System.Windows.Forms.ComboBox enumBox;
//        private System.Windows.Forms.Label label16;
//        private System.Windows.Forms.CheckBox clientSyncedBool;
//        private System.Windows.Forms.ComboBox graphicComboBox;

//    }
//}