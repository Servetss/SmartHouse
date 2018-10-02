using System;
using System.IO;
using System.Windows;

namespace Butler
{
    /// <summary>
    /// Interaction logic for Kalendarz.xaml
    /// </summary>
    /// 


    public partial class Kalendarz : Window
    {
        public Kalendarz()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dataOdjaz, dataPrzyjazd;
                string miesOdjazd, miesPrzyjazd;
                string hourOdjazd, hourPrzyjazd;
                string minOdjazd, minPrzyjazd;

                if (calenddo.SelectedDate.ToString() != "" || calendod.SelectedDate.ToString() != "")
                {
                    string data = calendod.SelectedDate.ToString();
                    data = data.Substring(0, 5);

                    dataOdjaz = data.Substring(0, 2);
                    miesOdjazd = data.Substring(3, 2);

                    data = calenddo.SelectedDate.ToString();
                    data = data.Substring(0, 5);

                    dataPrzyjazd = data.Substring(0, 2);
                    miesPrzyjazd = data.Substring(3, 2);

                    hourOdjazd = OdH.Text;
                    minOdjazd = OdM.Text;

                    hourPrzyjazd = DoH.Text;
                    minPrzyjazd = DoM.Text;
                    
                    if (OdH.Text != "" && OdM.Text != "" && DoH.Text != "" && DoM.Text != "")
                    {
                        if ((Int32.Parse(dataPrzyjazd) > Int32.Parse(dataOdjaz) && miesOdjazd == miesPrzyjazd) || (Int32.Parse(miesOdjazd) < Int32.Parse(miesPrzyjazd)))
                        {
                            string pathWyjazd = "";
                            string PathStart = "";
                            PathStart = Environment.CurrentDirectory.ToString();
                            string p = @"Dyplom\Butler";
                            int IndexOfString = PathStart.IndexOf(p);
                            PathStart = PathStart.Substring(0, IndexOfString);
                            pathWyjazd = PathStart + @"Dyplom\Dictionary\Wyjazd.txt";

                            string[] fileLines = File.ReadAllLines(pathWyjazd);
                            fileLines[0] = dataOdjaz + "-" + dataPrzyjazd + "-" + miesOdjazd + "-" + miesPrzyjazd + "-" + hourOdjazd + "-" + hourPrzyjazd + "-" + minOdjazd + "-" + minPrzyjazd; // Замена
                            File.WriteAllLines(pathWyjazd, fileLines);


                            MessageBox.Show("Dane zapisane");
                            //MW.FormaFunktion(dataOdjaz, dataPrzyjazd, miesOdjazd, miesPrzyjazd, hourOdjazd, hourPrzyjazd, minOdjazd, minPrzyjazd);
                            Close();
                        }
                        else
                            MessageBox.Show("Data przyjazdu mniej od daty odjazdu");
                    }
                    else
                        MessageBox.Show("Wpisz czas odjazdu i przyjazdu");
                }
                else
                    MessageBox.Show("Nie podana data");
            }
            catch { MessageBox.Show("ERROR!"); }
                
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string pathWyjazd = "";
            string PathStart = "";
            PathStart = Environment.CurrentDirectory.ToString();
            string p = @"Dyplom\Butler";
            int IndexOfString = PathStart.IndexOf(p);
            PathStart = PathStart.Substring(0, IndexOfString);
            pathWyjazd = PathStart + @"Dyplom\Dictionary\Wyjazd.txt";

            string[] fileLines = File.ReadAllLines(pathWyjazd);
            fileLines[0] = ""; // Замена
            File.WriteAllLines(pathWyjazd, fileLines);

            Close();
        }
    }
}
