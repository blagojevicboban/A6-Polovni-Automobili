using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLOKA6
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\A6.mdf;Integrated Security=True");
        DataTable dt;
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            osveziListuModela();
            PopuniComboProizvodjaca();
        }
        private void PopuniComboProizvodjaca()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM Proizvodjac ORDER BY ProizvodjacID";
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dtc = new DataTable();

            try
            {
                da.Fill(dtc);
                comboBoxProizvodjac.DataSource = dtc;
                comboBoxProizvodjac.DisplayMember = "Naziv";
                comboBoxProizvodjac.ValueMember = "ProizvodjacID";
                comboBoxProizvodjac.Text = "";
            }
            catch
            {
                MessageBox.Show("Doslo je do greške!");
            }
            finally
            {
                da.Dispose();
                cmd.Dispose();
            }
        }
        private void osveziListuModela()
        {
            string sqlSel = "SELECT m.ModelID,m.Naziv,p.Naziv FROM Model AS m, Proizvodjac AS p WHERE p.ProizvodjacID=m.ProizvodjacID order by m.Naziv";
            SqlCommand cmd = new SqlCommand(sqlSel, conn);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            dt = new DataTable();
            try
            {
                dataAdapter.Fill(dt);
                listBox1.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    listBox1.Items.Add(
                        String.Format("{0,-6}", row[0]) +
                        row[1] +
                        ", " + row[2]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska: " + ex.Message);
            }
            finally
            {
                dataAdapter.Dispose();
                cmd.Dispose();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                textBoxSifra.Text = dt.Rows[listBox1.SelectedIndex][0].ToString();
                textBoxNaziv.Text = dt.Rows[listBox1.SelectedIndex][1].ToString();
                comboBoxProizvodjac.Text = dt.Rows[listBox1.SelectedIndex][2].ToString();
            }
        }

        private void buttonIzadji_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonIzmeni_Click(object sender, EventArgs e)
        {
            if (textBoxSifra.Text != "" && int.Parse(textBoxSifra.Text) != -0)
            {
                SqlCommand cmd = new SqlCommand("update Model set Naziv=@naziv, ProizvodjacID=@pid where ModelID=@mid", conn);
                cmd.Parameters.AddWithValue("@mid", int.Parse(textBoxSifra.Text));
                cmd.Parameters.AddWithValue("@naziv", textBoxNaziv.Text);
                cmd.Parameters.AddWithValue("@pid", (int)comboBoxProizvodjac.SelectedValue);
                try
                {
                    conn.Open();
                    int selInd = listBox1.SelectedIndex;
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("uspesna izmena");
                    osveziListuModela();
                    //Oznaci();
                    listBox1.SelectedIndex = selInd;
                }
                catch (Exception)
                {
                    MessageBox.Show("Doslo je d0 greske");
                }
                finally
                {
                    conn.Close();
                    cmd.Dispose();
                }
            }
        }

        private void buttonPretrazi_Click(object sender, EventArgs e)
        {
            int id, i;
            if (int.TryParse(textBoxSifra.Text, out id))
            {
                for (i = 0; i < dt.Rows.Count && (int)dt.Rows[i][0] != id; i++) ;
                if (i < dt.Rows.Count)
                {
                    listBox1.SelectedIndex = i;
                    textBoxSifra.Text = dt.Rows[i][0].ToString();
                    textBoxNaziv.Text = dt.Rows[i][1].ToString();
                    comboBoxProizvodjac.Text = dt.Rows[i][2].ToString();
                }
                else
                {
                    listBox1.SelectedIndex = -1;
                    textBoxNaziv.Text = "";
                    comboBoxProizvodjac.Text = "";
                    textBoxSifra.Focus();
                    textBoxSifra.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("Greska u sifri");
                textBoxSifra.Focus();
                textBoxSifra.SelectAll();
            }
        }
        //private void Oznaci()
        //{
        //    int id = int.Parse(textBoxSifra.Text);
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        if ((int)dt.Rows[i][0]==id)
        //        {
        //            listBox1.SelectedIndex = i;
        //        }
        //    }
        //}

        private void buttonIzadji1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonPrikazi_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && int.Parse(textBox1.Text) != 0)
            {
                SqlCommand cmd = new SqlCommand("select p.Naziv as Proizvodjac, COUNT(v.VoziloID) as Broj from Vozilo as v, Model as m, Proizvodjac as p where p.ProizvodjacID=m.ProizvodjacID and m.ModelID=v.ModelID and (v.GodinaProizvodnje >= @od and v.GodinaProizvodnje <= @do) and v.PredjenoKM < @km group by p.Naziv", conn);
                cmd.Parameters.AddWithValue("@od", (int)numericUpDownOd.Value);
                cmd.Parameters.AddWithValue("@do", (int)numericUpDownDo.Value);
                cmd.Parameters.AddWithValue("@km", int.Parse(textBox1.Text));
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dtp = new DataTable();
                try
                {
                    da.Fill(dtp);
                    chart1.DataSource = dtp;
                    listView1.Items.Clear();
                    foreach (DataRow row in dtp.Rows)
                    {
                        ListViewItem li = new ListViewItem(row[0].ToString());
                        li.SubItems.Add(row[1].ToString());
                        listView1.Items.Add(li);
                    }
                    chart1.Series[0].XValueMember = "Proizvodjac";
                    chart1.Series[0].YValueMembers = "Broj";
                    chart1.Series[0].IsValueShownAsLabel = false;
                }
                catch (Exception)
                {

                    MessageBox.Show("Doslo je do greske");
                }
                finally
                {
                    if (conn != null)
                        conn.Close();
                    cmd.Dispose();
                    da.Dispose();
                    dtp.Dispose();
                }

            }
            else
            {
                MessageBox.Show("Unesite predjenu kilometrazu");
                textBox1.Focus();
                return;
            }
        }
    }
}
