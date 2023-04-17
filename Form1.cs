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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace _4ЕИТ_A6
{
    public partial class Form1 : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\A6.mdf;Integrated Security=True");
        DataTable dt; //tabela za listbox
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PopuniComboProizvodjaca();
            osveziListuModela();
            richTextBox1.LoadFile(@"..\..\DOC\A6.rtf");
        }
        private void osveziListuModela()
        {
            string sqlSel = "SELECT m.ModelID,m.Naziv,p.Naziv FROM Model AS m, Proizvodjac AS p WHERE p.ProizvodjacID=m.ProizvodjacID";
            SqlCommand cmd = new SqlCommand(sqlSel, conn);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            dt = new DataTable();
            try
            {
                dataAdapter.Fill(dt);
                listBoxModeli.Items.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    listBoxModeli.Items.Add(
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

        private void listBoxModeli_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxModeli.SelectedItems.Count > 0)
            {
                textBoxSifra.Text = dt.Rows[listBoxModeli.SelectedIndex][0].ToString();
                textBoxNaziv.Text = dt.Rows[listBoxModeli.SelectedIndex][1].ToString();
                comboBoxProizvodjac.Text = dt.Rows[listBoxModeli.SelectedIndex][2].ToString();
            }
        }

        private void buttonIzmeni_Click(object sender, EventArgs e)
        {
            // da li treba proveriti da li sifra modela postoji u bazi, sta ako je uneo nepostojeci ModelID?
            if (textBoxSifra.Text != "" && int.Parse(textBoxSifra.Text) != 0)
            {
                string sqlUpd = "UPDATE Model " +
                    "SET Naziv=@mnaz, ProizvodjacID=@pid " +  // korisnik ne menja ID modela
                    "WHERE ModelID=@mid";
                SqlCommand cmd = new SqlCommand(sqlUpd, conn);
                cmd.Parameters.AddWithValue("@mid", int.Parse(textBoxSifra.Text));
                cmd.Parameters.AddWithValue("@mnaz", textBoxNaziv.Text);
                cmd.Parameters.AddWithValue("@pid", (int)comboBoxProizvodjac.SelectedValue);
                try
                {
                    conn.Open();
                    int selInd = listBoxModeli.SelectedIndex; // pamtimo selekciju
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Uspesna izmena.");
                    osveziListuModela();
                    listBoxModeli.SelectedIndex = selInd; // vracamo selekciju
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Greska pri izmeni: " + ex.Message);
                }
                finally
                {
                    conn.Close();
                    cmd.Dispose();
                }
            }
        }

        private void buttonIzadji_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonTrazi_Click(object sender, EventArgs e)
        {
            int id, i;
            // najbitnija validacija unete šifre se može izvršiti TryParse metodom ili Parse metodom unutar try-catch bloka
            if (int.TryParse(textBoxSifra.Text, out id))
            {
                for (i = 0; i < dt.Rows.Count && (int)dt.Rows[i][0] != id; i++) ;
                if (i < dt.Rows.Count)
                {
                    listBoxModeli.SelectedIndex = i;
                    // popuniti kontrole
                    textBoxSifra.Text = dt.Rows[i][0].ToString();
                    textBoxNaziv.Text = dt.Rows[i][1].ToString();
                    comboBoxProizvodjac.Text = dt.Rows[i][2].ToString();
                }
                else
                {
                    //isprazniti kontrole
                    listBoxModeli.SelectedIndex = -1; // uklanjamo selekciju
                    textBoxNaziv.Text = ""; // praznimo kontrole
                    comboBoxProizvodjac.Text = "";
                    textBoxSifra.Focus();
                    textBoxSifra.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("Greska u sifri!");
                textBoxSifra.Focus();
                textBoxSifra.SelectAll();
            }
        }

        private void buttonPrikazi_Click(object sender, EventArgs e)
        {
            if (textBoxKM.Text != "" && int.Parse(textBoxKM.Text) != 0)
            {
                // sql3 je srtring SQL upita koji vraća dve kolone: Proizvodjac i Broj
                // razmaci na kraju stringova su neophodni kada spajamo stringove
                string sql3 = "SELECT p.Naziv AS Proizvodjac, COUNT(v.VoziloID) AS Broj " +
                            "FROM Vozilo AS v, Model AS m, Proizvodjac AS p " + // izmedju vozila i proizvodjaca je tabela model, moramo i nju uključiti
                            "WHERE p.ProizvodjacID=m.ProizvodjacID " + // veza izmedju proizvodjjaca i modela
                            "AND m.ModelID=v.ModelID " + // veza izmedju modela i vozila
                            "AND (v.GodinaProizvodnje >= @od " +
                            "AND v.GodinaProizvodnje <= @do) " +
                            "AND v.PredjenoKM < @km " +
                            "GROUP BY P.Naziv";
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = sql3;
                cmd.Connection = conn; // konekcija je u ovom slučaju već kreirana kao polje forme pri kreiranju forme
                cmd.Parameters.AddWithValue("@od", (int)numericUpDownOD.Value); // Value je Decimal tipa
                cmd.Parameters.AddWithValue("@do", (int)numericUpDownDO.Value);
                cmd.Parameters.AddWithValue("@km", int.Parse(textBoxKM.Text)); // Text je String tipa
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dtp = new DataTable(); // upit vraća jednu tabelu pa je dovoljan DataTable umesto DataSet
                try
                {
                    da.Fill(dtp); // ova naredba najčešće baca izuzetak: loš konekcioni string, sql upit.... 
                    listView1.Items.Clear();
                    foreach (DataRow row in dtp.Rows)
                    {
                        ListViewItem li = new ListViewItem(row[0].ToString());
                        li.SubItems.Add(row[1].ToString());
                        listView1.Items.Add(li);
                    }
                    // prikaz Chart-a
                    chart1.DataSource = dtp;
                    chart1.Series[0].XValueMember = "Proizvodjac"; // X-osa: naziv kolone iz SELECT upita
                    chart1.Series[0].YValueMembers = "Broj"; // Y-osa: naziv kolone iz SELECT upita
                    chart1.Series[0].IsValueShownAsLabel = false; // bez ovog svojstva neće prikazati Chart, prikaz vrednsoti

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Greska: " + ex.Message); // zbog utvrdjivanja uzroka greške prikazaćemo i detalje
                }
                finally
                {
                    // zatvorimo konekciju ka bazi što je bitno ukoliko je baza na serveru
                    if (conn != null)
                        conn.Close();
                    // eksplicitno očistiti iz memorije dinamičke objekte kreirane new operatorom 
                    cmd.Dispose();
                    da.Dispose();
                    dtp.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Unesite pređenu kilometražu !");
                textBoxKM.Focus();
                return;
            }
        }
    }
}
