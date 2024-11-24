﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Andmebaas_Vsevolod_Tsarev_TARpv23
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Andmed.mdf;Integrated Security=True");
        SqlCommand cmd;
        SqlDataAdapter adapter;
        string extension;


        DataTable laoTable;
        public Form1()
        {
            InitializeComponent();
            CreateDatabaseAndTable();
            NaitaAndmed();
        }

        public void CreateDatabaseAndTable()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Andmed.mdf;Integrated Security=True"))
                {
                    conn.Open(); 
                    string createTablesQuery = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Ladu') 
                    BEGIN
                        CREATE TABLE Ladu (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            LaoNimetus VARCHAR(50) NOT NULL,
                            Suurus VARCHAR(50) NOT NULL,
                            Kirjeldus NCHAR(10) NOT NULL
                        );

                        INSERT INTO Ladu (LaoNimetus, Suurus, Kirjeldus)
                        VALUES
                            ('Suur', '5', 'POLE'),
                            ('Keskmine', '3', 'POLE'),
                            ('Väike', '1', 'POLE');


                    END;

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Toode') 
                    BEGIN
                        CREATE TABLE Toode (
                            Id INT PRIMARY KEY IDENTITY(1,1),
                            Nimetus NVARCHAR(100) NOT NULL,
                            Kogus INT NOT NULL,
                            Hind DECIMAL(18, 2) NOT NULL,
                            Pilt NVARCHAR(MAX),
                            ProductPicture VARBINARY(MAX),
                            LaoId INT NULL,
                            CONSTRAINT FK_Toode_Ladu FOREIGN KEY (LaoId) REFERENCES Ladu (Id)
                            
                        );
                    END;
                    ";

                    using (SqlCommand cmd = new SqlCommand(createTablesQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании базы данных или таблицы: " + ex.Message);
            }
        }

        public void NaitaAndmed()
        {
            try
            {
                conn.Open();
                DataTable dt = new DataTable();
                cmd = new SqlCommand("SELECT * FROM Toode", conn);
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при отображении данных: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        int ID = 0;

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ID = (int)dataGridView1.Rows[e.RowIndex].Cells["Id"].Value;
            Nimetus_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Nimetus"].Value.ToString();
            Kogus_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Kogus"].Value.ToString();
            Hind_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Hind"].Value.ToString();
            try
            {
                pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetFullPath(@"..\..\Pildid"), dataGridView1.Rows[e.RowIndex].Cells["Pilt"].Value.ToString()));
            }
            catch
            {
                pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetFullPath(@"..\..\Pildid"), "pilt.png"));
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void Lisa_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Nimetus_txt.Text) && !string.IsNullOrWhiteSpace(Kogus_txt.Text) && !string.IsNullOrWhiteSpace(Hind_txt.Text))
            {
                try
                {
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = ".png";
                    }

                    cmd = new SqlCommand("SELECT ID FROM Ladu WHERE LaoNimetus=@Ladu", conn);
                    cmd.Parameters.AddWithValue("@ladu",Label_4.Text);
                    ID=Convert.ToInt32(cmd.ExecuteScalar());

                    conn.Open();
                    cmd = new SqlCommand("INSERT INTO Toode (Nimetus, Kogus, Hind, Pilt) VALUES (@toode, @kogus, @hind, @pilt)", conn);
                    cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                    cmd.Parameters.AddWithValue("@kogus", int.Parse(Kogus_txt.Text));
                    cmd.Parameters.AddWithValue("@hind", decimal.Parse(Hind_txt.Text));
                    cmd.Parameters.AddWithValue("@pilt", Nimetus_txt.Text + extension);
                    cmd.ExecuteNonQuery();


                    cmd.Parameters.AddWithValue("@Ladu", ID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении данных: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    Eemaldamine();
                    NaitaAndmed();
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля!");
            }
        }

        private void Uuenda_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Nimetus_txt.Text) && !string.IsNullOrWhiteSpace(Kogus_txt.Text) && !string.IsNullOrWhiteSpace(Hind_txt.Text))
            {
                try
                {
                    conn.Open();
                    cmd = new SqlCommand("UPDATE Toode SET Nimetus = @toode, Kogus = @kogus, Hind = @hind WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", ID);
                    cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                    cmd.Parameters.AddWithValue("@kogus", int.Parse(Kogus_txt.Text));
                    cmd.Parameters.AddWithValue("@hind", decimal.Parse(Hind_txt.Text));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    Eemaldamine();
                    NaitaAndmed();
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля!");
            }
        }

        private void kustuta_btn_Click(object sender, EventArgs e)
        {
            try
            {
                ID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["Id"].Value);
                string filename = dataGridView1.SelectedRows[0].Cells["Pilt"].Value.ToString();

                if (ID != 0)
                {
                    conn.Open();
                    cmd = new SqlCommand("DELETE FROM Toode WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", ID);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                    string file = dataGridView1.SelectedRows[3].Cells["Pilt"].Value.ToString();

                    System.Threading.Thread.Sleep(500);
                    Eemaldamine();
                    KustFail(file);
                    NaitaAndmed();
                }
                KustFail(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
            finally
            {
                conn.Close();
                Eemaldamine();
                NaitaAndmed();
            }
        }

        private void KustFail(string file)
        {
            try
            {
                string filePath = Path.Combine(Path.GetFullPath(@"..\..\Pildid"), file + extension);
                MessageBox.Show($"Püüan kustutada faili {filePath}");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MessageBox.Show("Fail on kustutatud");
                }
                else
                {
                    MessageBox.Show($"Fail {file} ei leitud");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении файла: " + ex.Message);
            }
        }

        private void otsipilt_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                InitialDirectory = @"C:\Users\opilane\Pictures\",
                Multiselect = false,
                Filter = "Image Files(*.jpeg; *.png; *.bmp; *.jpg)|*.jpeg; *.png; *.bmp; *.jpg"
            };

            if (open.ShowDialog() == DialogResult.OK)
            {
                string extension = Path.GetExtension(open.FileName);
                SaveFileDialog save = new SaveFileDialog
                {
                    InitialDirectory = Path.GetFullPath(@"..\..\Pildid"),
                    FileName = Nimetus_txt.Text + extension
                };

                if (save.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(open.FileName, save.FileName, true);
                    pictureBox1.Image = Image.FromFile(save.FileName);
                }
            }
        }
        private void Eemaldamine()
        {
            Nimetus_txt.Text = "";
            Kogus_txt.Text = "";
            Hind_txt.Text = "";
            pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetFullPath(@"..\..\Pildid"), "pilt.png"));
        }
        private void Ladu_cb_Click(object sender, EventArgs e)
        {
            conn.Open();
            cmd = new SqlCommand("SELECT Id, LaoNimetus FROM Ladu", conn);
            adapter = new SqlDataAdapter();
            adapter.Fill(laoTable);


            foreach (DataRow item in laoTable.Rows)
            {
                Ladu_cb.Items.Add(item["LaoNimetus"]);
            }


            conn.Close();
        }
    }
}
