using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Andmebaas_Vsevolod_Tsarev_TARpv23
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\opilane\source\repos\DatabaseSild\Andmed.mdf;Integrated Security=True");
        SqlCommand cmd;
        SqlDataAdapter adapter;
        string extension;
        DataTable laotable, dt;
        string openFilePath;
        int ID;

        public Form1()
        {
            InitializeComponent();
            CreateDatabaseAndTable();
            LaduNaitamine();
            NaitaAndmed();
        }

        public void CreateDatabaseAndTable()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\opilane\source\repos\DatabaseSild\Andmed.mdf;Integrated Security=True"))
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
                MessageBox.Show("Viga andmebaasi või tabeli loomisel: " + ex.Message);
            }
        }

        private void Eemaldamine()
        {
            Nimetus_txt.Clear();
            Kogus_txt.Clear();
            Hind_txt.Clear();
            pictureBox1.Image = null;
        }

        private void NaitaAndmed()
        {
            try
            {
                conn.Open();
                adapter = new SqlDataAdapter("SELECT * FROM Toode", conn);
                dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;

                dataGridView1.Columns["Id"].Visible = false;
                dataGridView1.Columns["Pilt"].HeaderText = "Pilt";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga andmete laadimisel: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        private void ClearPictureBoxImage()
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
        }
        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                ID = (int)dataGridView1.Rows[e.RowIndex].Cells["Id"].Value;
                Nimetus_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Nimetus"].Value.ToString();
                Kogus_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Kogus"].Value.ToString();
                Hind_txt.Text = dataGridView1.Rows[e.RowIndex].Cells["Hind"].Value.ToString();

                byte[] imageBytes = (byte[])dataGridView1.Rows[e.RowIndex].Cells["ProductPicture"].Value;

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        pictureBox1.Image = Image.FromStream(ms);
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }
                else
                {
                    pictureBox1.Image = Image.FromFile(Path.Combine(Path.GetFullPath(@"..\..\Pildid"), "pilt.png"));
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga pildi kuvamisel: " + ex.Message);
            }
        }

        private void Lisa_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Nimetus_txt.Text) &&
                !string.IsNullOrWhiteSpace(Kogus_txt.Text) &&
                !string.IsNullOrWhiteSpace(Hind_txt.Text))
            {
                try
                {
                    if (pictureBox1.Image == null)
                    {
                        MessageBox.Show("Palun valige pilt!");
                        return;
                    }

                    conn.Open();

                    byte[] imageBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox1.Image.Save(ms, pictureBox1.Image.RawFormat);
                        imageBytes = ms.ToArray();
                    }

                    cmd = new SqlCommand("SELECT Id FROM Ladu WHERE LaoNimetus=@ladu", conn);
                    cmd.Parameters.AddWithValue("@ladu", Ladu_cb.Text);
                    int laduId = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd = new SqlCommand("INSERT INTO Toode (Nimetus, Kogus, Hind, ProductPicture, LaoId) VALUES (@toode, @kogus, @hind, @pilt, @laoid)", conn);
                    cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                    cmd.Parameters.AddWithValue("@kogus", int.Parse(Kogus_txt.Text));
                    cmd.Parameters.AddWithValue("@hind", decimal.Parse(Hind_txt.Text));
                    cmd.Parameters.AddWithValue("@pilt", imageBytes);
                    cmd.Parameters.AddWithValue("@laoid", laduId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Toode lisati edukalt!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Viga toote lisamisel: " + ex.Message);
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
                MessageBox.Show("Palun täitke kõik väljad!");
            }
        }


        private void Uuenda_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Nimetus_txt.Text) && !string.IsNullOrWhiteSpace(Kogus_txt.Text) && !string.IsNullOrWhiteSpace(Hind_txt.Text))
            {
                try
                {
                    conn.Open();
                    string newImageName = Nimetus_txt.Text + extension; 
                    cmd = new SqlCommand("UPDATE Toode SET Nimetus = @toode, Kogus = @kogus, Hind = @hind, Pilt = @pilt WHERE Id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", ID);
                    cmd.Parameters.AddWithValue("@toode", Nimetus_txt.Text);
                    cmd.Parameters.AddWithValue("@kogus", int.Parse(Kogus_txt.Text));
                    cmd.Parameters.AddWithValue("@hind", decimal.Parse(Hind_txt.Text));
                    cmd.Parameters.AddWithValue("@pilt", newImageName);

                    cmd.ExecuteNonQuery();
                    conn.Close();

                    string destinationPath = Path.Combine(Path.GetFullPath(@"..\..\Pildid"), newImageName);
                    if (!File.Exists(destinationPath))
                    {
                        File.Copy(openFilePath, destinationPath, true);
                    }

                    pictureBox1.Image = Image.FromFile(destinationPath);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                    MessageBox.Show("Andmed on edukalt uuendatud!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Viga andmete uuendamisel: " + ex.Message);
                }
                finally
                {
                    NaitaAndmed();
                    Eemaldamine();
                }
            }
            else
            {
                MessageBox.Show("Palun täitke kõik väljad!");
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

                    KustFail(filename);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga kustutamisel: " + ex.Message);
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
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MessageBox.Show("Fail on kustutatud");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga faili kustutamisel: " + ex.Message);
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
                openFilePath = open.FileName;
                extension = Path.GetExtension(openFilePath);
                pictureBox1.Image = Image.FromFile(openFilePath);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }



        public void LaduNaitamine()
        {
            try
            {
                conn.Open();
                laotable = new DataTable();
                cmd = new SqlCommand("SELECT * FROM Ladu", conn);
                adapter = new SqlDataAdapter(cmd);
                adapter.Fill(laotable);
                foreach (DataRow row in laotable.Rows)
                {
                    Ladu_cb.Items.Add(row["LaoNimetus"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Viga laode andmete laadimisel: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
