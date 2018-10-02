using System.Windows;
using System.Net;  
using System;
using System.IO;
using System.Linq;
using System.IO.Ports;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Butler
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpeechSynthesizer SS = new SpeechSynthesizer();
        SpeechRecognitionEngine SR = new SpeechRecognitionEngine();
        bool ticked = true;
        bool firstStart = true;
        int[] HourLightOn;
        int[] MinLightOn;

        int Den = 0;
        string hoursNow = "", minNow = "", dayNow = "", monthNow = "", yearNow = "", denNedeli = "";//Настоящее время
        private SerialPort myport; //Порт Ардуино
        bool ArduinoOff = true;
        string room = "";
        int[] RoomsOn;
        int h = 0, m = 0;

        string[] text = new string[10];//Слова для распознавания
        string PathStart = "";
        string Response; //Текст html
        string path = ""; //Файл с кодами городов
        string pathSpeek = ""; //Файл с кодами городов для разговора
        string pathNumber = "";
        string pathTravel = "";
        string pathLight = "";
        string pathUzytkownik = "";
        string pathWyjazd = "";

        //Переменные создания ссылки для парсера билетов
        bool x = true; // для заказа билетов
        bool YearDooble = false;
        string cityDepGow = "", cityArrGow = "";
        string cityDep, cityArr, kodArr = "", kodDep = "", cityLine;
        string hourArr = "", minArr = "", dayArr = "", monthArr = "", yearArr = "";


        int puls = 72;
        int Weightbed = 0;
        float TemperatureBody = 36.6f;
        bool budzik = false;
        bool morning;
        bool uzytkownikSpi;


        string MdataOdjaz = "", MdataPrzyjazd = "", MmiesOdjazd = "", MmiesPrzyjazd = "", MhourOdjazd = "", MhourPrzyjazd = "", MminOdjazd = "", MminPrzyjazd = ""; //Данные с второй вормы "Отъезд на долго"
        bool Wyjazd = false;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create and load a grammar.
                Choices choice = new Choices(new Choices(File.ReadAllLines(PathStart + @"Dyplom\Dictionary\Allwords.txt")));
                choice.Add(new string[] { "what the", "turn on light in", "turn off light in", "order ticket", "open the door in", "close the door in" });

                GrammarBuilder test = new GrammarBuilder();
                test.Append(choice);
                Grammar testGrammar = new Grammar(test);
                testGrammar.Name = "Test";
                SR.LoadGrammarAsync(testGrammar);
                try
                {
                    // Configure recognizer input.
                    SR.SetInputToDefaultAudioDevice();
                    MessageBox.Show("Recognized ON");
                    // Attach an event handler for the SpeechRecognized event.
                    SR.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
                    SR.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch { MessageBox.Show("Niepodłączony mikrofon, lub "); }
            }
            catch { MessageBox.Show("Błąd"); }

        }

        public void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text;
            komands.Text += " " + speech;

            if (speech == "open the door in")
            { text[0] = speech; }
            else if (speech == "kitchen" && text[0] == "open the door in")
            { init("5"); Clear(); }

            if (speech == "close the door in")
            { text[0] = speech; }
            else if (speech == "kitchen" && text[0] == "close the door in")
            { init("6"); Clear(); }


            if (speech == "what the")
            { text[0] = speech; }
            else if (text[0] == "what the")
            {
                wikipedia(speech);
                text[0] = "";
                komands.Text = "";
            }


            if (speech == "turn on light in")
            { text[0] = speech; }
            else if (speech == "kitchen" && text[0] == "turn on light in")
            { init("1"); Clear(); }
            else if (speech == "hall" && text[0] == "turn on light in")
            { init("3"); Clear(); }

            if (speech == "turn off light in")
            { text[0] = speech; }
            else if (speech == "kitchen" && text[0] == "turn off light in")
            { init("2"); Clear(); }
            else if (speech == "hall" && text[0] == "turn off light in")
            { init("4"); Clear(); }



            if (speech == "order ticket")
            { text[0] = speech; ComputerSpeak("call place of departure and arrival"); }
            else if (speech == "usually" && text[0] == "order ticket" && ticked == true)
            {
                Travel();
                Bilety(cityArrGow, cityDepGow, hourArr, minArr, dayArr, monthArr, yearArr);
                Clear();
            }

            if (text[0] == "order ticket" && cityArrGow == "")
            {
                using (StreamReader sr = new StreamReader(pathSpeek, System.Text.Encoding.Default))
                {
                    while ((cityLine = sr.ReadLine()) != null)//Поиск заказаного города отправления в базе
                    {
                        cityArrGow = cityLine.Substring(0, cityLine.Length - 8);
                        if (speech == cityArrGow)
                        {
                            cityArrGow = cityLine;
                            break;
                        }
                        else
                            cityArrGow = "";
                    }
                }
            }
            else if (text[0] == "order ticket" && cityArrGow != "" && cityDepGow == "")
            {
                using (StreamReader sr = new StreamReader(pathSpeek, System.Text.Encoding.Default))
                {
                    while ((cityLine = sr.ReadLine()) != null)//Поиск заказаного города отправления в базе
                    {
                        cityDepGow = cityLine.Substring(0, cityLine.Length - 8);
                        if (speech == cityDepGow)
                        {
                            cityDepGow = cityLine;
                            ComputerSpeak("how much, and when should the train be sent");
                            break;
                        }
                        else
                            cityDepGow = "";
                    }
                }
                ticked = false;
            }
            else if (speech == "now" || speech == "buy" && x)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    text[i] = "";
                    komands.Text = "";
                }
                komands.Text = "";
                Bilety(cityArrGow, cityDepGow, hourArr, minArr, dayArr, monthArr, yearArr);
            }
            else if (cityDepGow != "")
            {
                if (speech == "year" || speech == "buy")
                {
                    Bilety(cityArrGow, cityDepGow, hourArr, minArr, dayArr, monthArr, yearArr);
                    Clear();
                }
                x = false;
                if (hourArr == "" && minArr == "")
                {
                    using (StreamReader sr = new StreamReader(pathNumber, System.Text.Encoding.Default))
                    {
                        while ((cityLine = sr.ReadLine()) != null)
                        {
                            hourArr = cityLine.Substring(0, cityLine.Length - 3);
                            if (speech == hourArr)
                            {
                                hourArr = cityLine.Substring(cityLine.Length - 2, 2);
                                break;
                            }
                            else
                                hourArr = "";
                        }
                    }
                }
                else if (hourArr != "" && minArr == "")
                {
                    using (StreamReader sr = new StreamReader(pathNumber, System.Text.Encoding.Default))
                    {
                        while ((cityLine = sr.ReadLine()) != null)
                        {
                            minArr = cityLine.Substring(0, cityLine.Length - 3);
                            if (speech == minArr)
                            {
                                minArr = cityLine.Substring(cityLine.Length - 2, 2);
                                break;
                            }
                            else
                                minArr = "";
                        }
                    }
                }
                else if (dayArr == "")
                {
                    if (speech == "tomorrow")
                    {
                        int day = Int32.Parse(dayNow) + 1;
                        dayNow = day.ToString();
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(pathNumber, System.Text.Encoding.Default))
                        {
                            while ((cityLine = sr.ReadLine()) != null)
                            {
                                dayArr = cityLine.Substring(0, cityLine.Length - 3);
                                if (speech == dayArr)
                                {
                                    dayArr = cityLine.Substring(cityLine.Length - 2, 2);
                                    MessageBox.Show(dayArr);
                                    break;
                                }
                                else
                                    dayArr = "";
                            }
                        }
                    }
                }
                else if (monthArr == "")
                {
                    using (StreamReader sr = new StreamReader(pathNumber, System.Text.Encoding.Default))
                    {
                        MessageBox.Show(speech);
                        while ((cityLine = sr.ReadLine()) != null)
                        {
                            monthArr = cityLine.Substring(0, cityLine.Length - 3);
                            if (speech == monthArr)
                            {
                                monthArr = cityLine.Substring(cityLine.Length - 2, 2);
                                MessageBox.Show(monthArr);
                                break;
                            }
                            else
                                monthArr = "";
                        }
                    }
                }
                else if (monthArr != "")
                {
                    using (StreamReader sr = new StreamReader(pathNumber, System.Text.Encoding.Default))
                    {
                        while ((cityLine = sr.ReadLine()) != null)
                        {
                            yearArr = cityLine.Substring(0, cityLine.Length - 3);
                            if (speech == yearArr && !YearDooble)
                            {
                                yearArr = cityLine.Substring(cityLine.Length - 2, 2);
                                YearDooble = true;
                                break;
                            }
                            else if (speech == yearArr && YearDooble)
                            {
                                yearArr += cityLine.Substring(cityLine.Length - 2, 2);
                                MessageBox.Show(yearArr);
                                break;
                            }
                            else
                                yearArr = "";
                        }
                    }
                }

            }


            //MessageBox.Show(speech);
            if (speech == "clear")
            {
                Clear();
            }
        }
        private void Clear()
        {
            for (int i = 0; i < text.Length; i++)
            {
                text[i] = "";
                komands.Text = "";
            }

            ticked = true;
            x = true; // для заказа билетов
            YearDooble = false;
            cityDepGow = ""; cityArrGow = "";
            cityDep = ""; cityArr = ""; kodArr = ""; kodDep = ""; cityLine = "";
            hourArr = ""; minArr = ""; dayArr = ""; monthArr = ""; yearArr = "";
        }

        public void ComputerSpeak(string text)
        {
            SS.Rate = 1;
            SS.Volume = 100;
            VoiceInfo info = SS.Voice;

            SS.SelectVoiceByHints(VoiceGender.Male, VoiceAge.Adult);

            SS.SetOutputToDefaultAudioDevice();
            PromptBuilder builder = new PromptBuilder();
            builder.AppendText(text);
            SS.Speak(builder);
        }

        private void parseW_Click(object sender, RoutedEventArgs e)
        {
            //Парсинг Википедии
            if (wiki.Text == "")
            { }
            else
            {
                string linkWiki = "https://en.wikipedia.org/wiki/" + wiki.Text;
                textBoxWiki.Text = linkWiki; // Запись ссылки
                WebClient wc = new WebClient();
                string WikiParse = ""; // Целый текст
                int SubString; // Что обрезать в тексте 
                WikiParse = wc.DownloadString(linkWiki); // Запиши всб HTML разметку с сайта

                //Текст из первого абзаца
                if (WikiParse.IndexOf("<p>") != -1)
                {
                    SubString = WikiParse.IndexOf("<p>");// Найди в тексте <p>
                    WikiParse = WikiParse.Substring(SubString + 3, 10000); // Обреж до найденого символа и после на 10000 знаков
                    if (WikiParse.IndexOf("</p>") != -1)
                    {
                        SubString = WikiParse.IndexOf("</p>");// Найди в тексте </p>
                        WikiParse = WikiParse.Substring(0, SubString);// Обреж после найденого символа
                    }
                }
                //Перевод HTML разметки в обычный текст
                while (WikiParse.IndexOf(">") != -1)
                {
                    WikiParse = WikiParse.Remove(WikiParse.IndexOf("<"), WikiParse.IndexOf(">") - WikiParse.IndexOf("<") + 1);// Удали все от "<" и до ">"
                }
                while (WikiParse.IndexOf("]") != -1)
                {
                    WikiParse = WikiParse.Remove(WikiParse.IndexOf("["), WikiParse.IndexOf("]") - WikiParse.IndexOf("[") + 1);// Удали все от "[" и до "]"
                }


                string[] words = WikiParse.Split(new char[] { ' ' });// Разделяет текст по словам и записывает слова в массив
                string writePath = PathStart + @"Dyplom\Dictionary\words\";// Путь к словам Джарвиса
                string allword = PathStart + @"Dyplom\Dictionary\Allwords.txt";
                string path; // Конкретный файл в который будет записано слово
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].ToLower();// Нижний регистр слов
                    words[i] = words[i].Replace(".", "");// Замени разные символы пустотой
                    words[i] = words[i].Replace(",", "");
                    words[i] = words[i].Replace("(", "");
                    words[i] = words[i].Replace(")", "");
                    words[i] = words[i].Replace("!", "");
                    words[i] = words[i].Replace("?", "");

                    string firstLet;// Первая буква в слове
                    firstLet = words[i].Substring(0, 1);
                    path = writePath + firstLet + ".txt";// Полный путь к файлу записи
                    try
                    {
                        bool on = true;
                        bool AllOn = true;
                        using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))// Проверка слова в файле. Если его нет то слово запишится
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line == words[i])
                                { on = false; }
                            }
                        }
                        using (StreamReader sr = new StreamReader(allword, System.Text.Encoding.Default))// Проверка слова в файле. Если его нет то слово запишится
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line == words[i])
                                { AllOn = false; }
                            }
                        }
                        if (on)
                        {
                            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                            {
                                sw.WriteLine(words[i]);
                            }
                        }
                        if (AllOn)
                        {
                            using (StreamWriter sw = new StreamWriter(allword, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                            {
                                sw.WriteLine(words[i]);
                            }
                        }
                    }
                    catch { }

                }

                ComputerSpeak(WikiParse);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void visib(int x, bool z)
        {
            for (int i = 0; i < x; i++)
            {
                if (!z)
                {
                    switch (i)
                    {
                        case 0:
                            ot.Visibility = Visibility.Hidden;
                            _do.Visibility = Visibility.Hidden;
                            t1.Visibility = Visibility.Hidden;
                            t2.Visibility = Visibility.Hidden;
                            break;
                        case 1:
                            ot_Copy.Visibility = Visibility.Hidden;
                            _do_Copy.Visibility = Visibility.Hidden;
                            t1_Copy.Visibility = Visibility.Hidden;
                            t2_Copy.Visibility = Visibility.Hidden;
                            break;
                        case 2:
                            ot_Copy1.Visibility = Visibility.Hidden;
                            _do_Copy1.Visibility = Visibility.Hidden;
                            t1_Copy1.Visibility = Visibility.Hidden;
                            t2_Copy1.Visibility = Visibility.Hidden;
                            break;
                        case 3:
                            ot_Copy2.Visibility = Visibility.Hidden;
                            _do_Copy2.Visibility = Visibility.Hidden;
                            t1_Copy2.Visibility = Visibility.Hidden;
                            t2_Copy2.Visibility = Visibility.Hidden;
                            break;
                        case 4:
                            ot_Copy3.Visibility = Visibility.Hidden;
                            _do_Copy3.Visibility = Visibility.Hidden;
                            t1_Copy3.Visibility = Visibility.Hidden;
                            t2_Copy3.Visibility = Visibility.Hidden;
                            break;
                        case 5:
                            ot_Copy4.Visibility = Visibility.Hidden;
                            _do_Copy4.Visibility = Visibility.Hidden;
                            t1_Copy4.Visibility = Visibility.Hidden;
                            t2_Copy4.Visibility = Visibility.Hidden;
                            break;
                        case 6:
                            ot_Copy5.Visibility = Visibility.Hidden;
                            _do_Copy5.Visibility = Visibility.Hidden;
                            t1_Copy5.Visibility = Visibility.Hidden;
                            t2_Copy5.Visibility = Visibility.Hidden;
                            break;
                        case 7:
                            ot_Copy6.Visibility = Visibility.Hidden;
                            _do_Copy6.Visibility = Visibility.Hidden;
                            t1_Copy6.Visibility = Visibility.Hidden;
                            t2_Copy6.Visibility = Visibility.Hidden;
                            break;
                        default:
                            break;
                    }
                }
                else
                    switch (i)
                    {
                        case 0:
                            ot.Visibility = Visibility.Visible;
                            _do.Visibility = Visibility.Visible;
                            t1.Visibility = Visibility.Visible;
                            t2.Visibility = Visibility.Visible;
                            break;
                        case 1:
                            ot_Copy.Visibility = Visibility.Visible;
                            _do_Copy.Visibility = Visibility.Visible;
                            t1_Copy.Visibility = Visibility.Visible;
                            t2_Copy.Visibility = Visibility.Visible;
                            break;
                        case 2:
                            ot_Copy1.Visibility = Visibility.Visible;
                            _do_Copy1.Visibility = Visibility.Visible;
                            t1_Copy1.Visibility = Visibility.Visible;
                            t2_Copy1.Visibility = Visibility.Visible;
                            break;
                        case 3:
                            ot_Copy2.Visibility = Visibility.Visible;
                            _do_Copy2.Visibility = Visibility.Visible;
                            t1_Copy2.Visibility = Visibility.Visible;
                            t2_Copy2.Visibility = Visibility.Visible;
                            break;
                        case 4:
                            ot_Copy3.Visibility = Visibility.Visible;
                            _do_Copy3.Visibility = Visibility.Visible;
                            t1_Copy3.Visibility = Visibility.Visible;
                            t2_Copy3.Visibility = Visibility.Visible;
                            break;
                        case 5:
                            ot_Copy4.Visibility = Visibility.Visible;
                            _do_Copy4.Visibility = Visibility.Visible;
                            t1_Copy4.Visibility = Visibility.Visible;
                            t2_Copy4.Visibility = Visibility.Visible;
                            break;
                        case 6:
                            ot_Copy5.Visibility = Visibility.Visible;
                            _do_Copy5.Visibility = Visibility.Visible;
                            t1_Copy5.Visibility = Visibility.Visible;
                            t2_Copy5.Visibility = Visibility.Visible;
                            break;
                        case 7:
                            ot_Copy6.Visibility = Visibility.Visible;
                            _do_Copy6.Visibility = Visibility.Visible;
                            t1_Copy6.Visibility = Visibility.Visible;
                            t2_Copy6.Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            visib(8, false);
            Puls.Content = puls.ToString();
            Temperatura.Content = TemperatureBody.ToString();

            spoz.IsEnabled = false;
            Potw.IsEnabled = false;
            Zap.IsEnabled = false;
            Spoz.IsEnabled = false;
            Wyjezd.IsEnabled = false;
            ChanTem.IsEnabled = false;
            temp.IsEnabled = false;
            textBox.IsReadOnly = true;
            komands.IsReadOnly = true;
            textBoxRecogn.IsReadOnly = true;
            textBoxWiki.IsReadOnly = true;
            PathStart = Environment.CurrentDirectory.ToString();
            string p = @"Dyplom\Butler";
            int IndexOfString = PathStart.IndexOf(p);
            PathStart = PathStart.Substring(0, IndexOfString);

            path = PathStart + @"Dyplom\Dictionary\kodCity.txt"; //Файл с кодами городов
            pathSpeek = PathStart + @"Dyplom\Dictionary\kodCitySpeek.txt"; //Файл с кодами городов для разговора
            pathNumber = PathStart + @"Dyplom\Dictionary\Number.txt";
            pathTravel = PathStart + @"Dyplom\Dictionary\Travels.txt";
            pathLight = PathStart + @"Dyplom\Dictionary\Light.txt";
            pathUzytkownik = PathStart + @"Dyplom\Dictionary\Uzytkownik.txt";
            pathWyjazd = PathStart + @"Dyplom\Dictionary\Wyjazd.txt";

            Parse.IsEnabled = false;
            timeNow();
            TimeThrough();
            LightOn();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(WklLight);
            dispatcherTimer.Interval = new TimeSpan(h, m, 0);
            dispatcherTimer.Start();


            DispatcherTimer TimerJ = new DispatcherTimer();
            TimerJ.Tick += new EventHandler(Jaluzi);
            TimerJ.Interval = new TimeSpan(0, 0, 1);
            TimerJ.Start();
        }

        
    int minuta = 0;
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            
            try
            {
                int zxc = 12;
                double qaz = zxc;
                MessageBox.Show(qaz.ToString());


                string s1 = "1234 -2345-23";

                string qwe = PathStart + @"Dyplom\Dictionary\qwe.txt";

                string[] parts = new string[2];
                parts = s1.Split('-');

                //s1 = s1.Substring(s1.LastIndexOf(';'));
                // MessageBox.Show(parts[0] + "  " + parts[1]);

                string s = File.ReadAllLines(PathStart + @"Dyplom\Dictionary\kodCity.txt").Skip(1).First();

                IEnumerable<string> result = File.ReadLines(qwe).Skip(0).Take(1);



                int x;

                string line;
                string[] part = new string[2];

                using (StreamReader sr = new StreamReader(qwe, System.Text.Encoding.Default))
                {
                    x = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (x == 1)
                        {
                            break;
                        }
                        x++;
                        MessageBox.Show(x.ToString());
                    }
                }

                string[] fileLines = File.ReadAllLines(qwe);
                fileLines[x] = "qqq"; // deleting
                File.WriteAllLines(qwe, fileLines);
            }
            catch { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        private void timeNow()
        {

            DateTime localDate = DateTime.Now;// Sorawdzenie czasu
            hoursNow = localDate.ToString().Substring(11, 2);
            minNow = localDate.ToString().Substring(14, 2); //Zapis czasu do zmiennych
            dayNow = localDate.ToString().Substring(0, 2);
            monthNow = localDate.ToString().Substring(3, 2);
            yearNow = localDate.ToString().Substring(8, 2);
            denNedeli = new DateTime(Int32.Parse(yearNow), Int32.Parse(monthNow), Int32.Parse(dayNow)).DayOfWeek.ToString();

            switch (denNedeli)
            {
                case "Monday":
                    Den = 1;
                    break;
                case "Tuesday":
                    Den = 2;
                    break;
                case "Wednesday":
                    Den = 3;
                    break;
                case "Thursday":
                    Den = 4;
                    break;
                case "Friday":
                    Den = 5;
                    break;
                case "Saturday":
                    Den = 6;
                    break;
                case "Sunday":
                    Den = 7;
                    break;
                default:
                    break;
            }
        }

        private void Parse_Click(object sender, RoutedEventArgs e)
        {
            int[] ilosc = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };// Смотрит на какой строке
            string[] part = new string[10] { "", "", "", "", "", "", "", "", "", "" }; // Части текста из таблиц в HTML
            string text = "";// Текст который мы режим на части 
            int n = 0;
            bool zakaz = false;

            string search = "focus_guiVCtrl_connection_detailsOut_select_C0-";
            string time = "ODJAZD";
            string data = "czas przejazdu:";
            string kupZ = "kupBiletPrHandler";
            string kupZo = "W komunikacji krajowej zakup mo";

            timeNow();

            for (int i = 0; i <= 12; i++)
            {
                if (Response.LastIndexOf(search + i.ToString()) != -1)
                {
                    n++;
                    ilosc[0] = Response.LastIndexOf(search + i.ToString());// Поиск нулевого элемента
                    text = Response.Substring(ilosc[0]);// Обрезка до нулевого элемента
                }
            }


            for (int i = 0; i <= n; i++)
            {
                if (i == 0)
                {
                    if (Response.LastIndexOf(search + "0") != -1)
                    {
                        ilosc[0] = Response.LastIndexOf(search + "0");// Поиск нулевого элемента
                        text = Response.Substring(ilosc[0]);// Обрезка до нулевого элемента
                    }
                }
                else if (i == 1)
                {
                    if (text.LastIndexOf(search + i.ToString()) != -1)
                    {
                        ilosc[i] = text.LastIndexOf(search + i.ToString());// Находит первый элемент из таблицы 
                        part[i - 1] = text.Substring(0, text.Length - ilosc[i]);// Записывает первую часть в массив
                    }
                }
                else
                {
                    if (text.LastIndexOf(search + i.ToString()) != -1)
                    {
                        ilosc[i] = text.LastIndexOf(search + i.ToString());// Находит первый элемент из таблицы 
                        part[i - 1] = text.Substring(ilosc[i - 1], text.Length - ilosc[i]);// Записывает части в массив
                    }
                    else
                    {
                        part[i - 1] = text.Substring(ilosc[i - 1], 10000);// Записывает последнюю часть в массив
                    }
                }
            }

            for (int i = 0; i < n; i++)// Смотрит на время отправления каждого поезда в таблице
            {
                int a = part[i].IndexOf(time);
                int v = part[i].IndexOf(data);
                string barada2 = part[i].Substring(v + 38, 8);
                string daySubs = barada2.Substring(0, 3);


                string[] words = daySubs.Split(new char[] { ' ' });
                for (int j = 0; j < words.Length; j++)
                {
                    words[j] = words[j].Replace(".", "");// Замени разные символы пустотой
                    words[j] = words[j].Replace(">", "");
                }
                daySubs = "";
                for (int j = 0; j < words.Length; j++)
                {
                    daySubs += words[j];
                }

                string barada = part[i].Substring(a + 7, 5);
                string hours = barada.Substring(0, 2);
                string min = barada.Substring(3, 2);

                if (hour.Text != "" && minn.Text == "")
                { minn.Text = "00"; }

                if (hour.Text == "" || minn.Text == "")
                {
                    if (Int32.Parse(hours) > Int32.Parse(hoursNow) || (Int32.Parse(hours) < Int32.Parse(hoursNow) && Int32.Parse(daySubs) > Int32.Parse(dayNow)))// Если время отправки больше за настоящее время, то закажи.
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                    else if (Int32.Parse(hours) == Int32.Parse(hoursNow) && 20 < Int32.Parse(min) - Int32.Parse(minNow))
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                }
                else if ((hour.Text != "" || minn.Text != "") && day.Text == "")
                {
                    if (Int32.Parse(hours) == Int32.Parse(hour.Text) && Int32.Parse(min) >= Int32.Parse(minn.Text))// Если время отправки больше за настоящее время, то закажи.
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                    else if ((Int32.Parse(hours) > Int32.Parse(hour.Text) && Int32.Parse(daySubs) == Int32.Parse(dayNow) || Int32.Parse(hours) < Int32.Parse(hour.Text) && Int32.Parse(daySubs) > Int32.Parse(dayNow)) && day.Text == "")
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                }
                else if ((hour.Text != "" || minn.Text != "") && day.Text != "")
                {
                    if (Int32.Parse(hours) == Int32.Parse(hour.Text) && Int32.Parse(min) >= Int32.Parse(minn.Text))// Если время отправки больше за настоящее время, то закажи.
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                    else if ((Int32.Parse(hours) > Int32.Parse(hour.Text) && Int32.Parse(daySubs) == Int32.Parse(day.Text) || Int32.Parse(hours) < Int32.Parse(hour.Text) && Int32.Parse(daySubs) > Int32.Parse(day.Text)) && day.Text == "")
                    {
                        if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                        {
                            MessageBox.Show("Time depart: " + hours + " : " + min);
                            zakaz = true;
                            break;
                        }
                    }
                }
            }

            if (!zakaz)
            {
                if (Int32.Parse(hourArr) + 2 < 24)
                {
                    int x = Int32.Parse(hourArr) + 2;
                    hourArr = x.ToString();
                }
                else
                {
                    int x = Int32.Parse(hourArr) + 2 - 24;
                    hourArr = "0" + x.ToString();
                }

                string link = "http://rozklad-pkp.pl/pl/tp?queryPageDisplayed=yes&REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0ID=&REQ0JourneyStops1.0G=&REQ0JourneyStopover1=&REQ0JourneyStops2.0G=&REQ0JourneyStopover2=&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0ID=&REQ0HafasSearchForw=1&existBikeEverywhere=yes&existHafasAttrInc=yes&existHafasAttrInc=yes&REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_1_0=1&REQ0JourneyProduct_prod_section_2_0=1&REQ0JourneyProduct_prod_section_3_0=1&REQ0JourneyProduct_prod_section_0_1=1&REQ0JourneyProduct_prod_section_1_1=1&REQ0JourneyProduct_prod_section_2_1=1&REQ0JourneyProduct_prod_section_3_1=1&REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_1_2=1&REQ0JourneyProduct_prod_section_2_2=1&REQ0JourneyProduct_prod_section_3_2=1&REQ0JourneyProduct_prod_section_0_3=1&REQ0JourneyProduct_prod_section_1_3=1&REQ0JourneyProduct_prod_section_2_3=1&REQ0JourneyProduct_prod_section_3_3=1&REQ0JourneyProduct_opt_section_0_list=0%3A000000&REQ0HafasOptimize1=&existOptimizePrice=1&REQ0HafaChangeTime=0%3A1&existSkipLongChanges=0&REQ0HafasAttrExc=&existHafasAttrInc=yes&existHafasAttrExc=yes&wDayExt0=Pn%7CWt%7C%C5%9Ar%7CCz%7CPt%7CSo%7CNd&start=start&existUnsharpSearch=yes&singlebutton=&came_from_form=1&REQ0JourneyStopsS0G=" + kodArr + "&REQ0JourneyStopsZ0G=" + kodDep + "&date=" + dayArr + "." + monthArr + "." + yearArr + "&dateStart=" + dayArr + "." + monthArr + "." + yearArr + "&dateEnd=" + dayArr + "." + monthArr + "." + yearArr + "&REQ0JourneyDate=" + dayArr + "." + monthArr + "." + yearArr + "&time=" + hourArr + "%3A" + minArr + "&REQ0JourneyTime=" + hourArr + "%3A" + minArr;
                WebClient wc = new WebClient();
                Response = wc.DownloadString(link);

                    MessageBox.Show("Nie ma biletów w najbliższym czasie");
            }
            Parse.IsEnabled = false;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void day_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
        bool b = true;
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (b)
            {
                init("3");
                b = false;
            }
            else
            {
                init("4");
                b = true;
            }
        }
        bool q = true;
        private void door_Click(object sender, RoutedEventArgs e)
        {
            if (q)
            {
                init("5");
                q = false;
            }
            else
            {
                init("6");
                q = true;
            }
        }

        float tempUlica = 0;
        float uzytkownikTemp;
        float tempDom = 0;
        bool uzytkownikDom = false;
        bool uzytkownikJedzi = false;
        int ileOtworz = 0;
        bool spozniam = false;
        int TwentyMin = 0;
        int TenMin = 0;

        int DelH = 0, DelM = 0;
        Random rand = new Random();

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            spoz.IsEnabled = true;
            Spoz.IsEnabled = true;
            Wyjezd.IsEnabled = true;
            uzytkownikTemp = Convert.ToSingle(temp.Text);
            if (uzytkownikTemp >= 10 && uzytkownikTemp <= 39)
            {
                teUl.Content = tempUlica.ToString();
                TeDo.Content = tempDom.ToString();

                DispatcherTimer TempTimer = new DispatcherTimer();
                TempTimer.Tick += new EventHandler(tempChange);
                TempTimer.Interval = new TimeSpan(DelH, DelM, 1);
                TempTimer.Start();
            }
            else { MessageBox.Show("Podaj przedział od 10 do 39"); }
        }

        private void tempChange(object sender, EventArgs e)
        {
            timeNow();
            minuta++;
            

            if (minuta == 20)
            {
                using (StreamReader sr = new StreamReader(pathWyjazd, System.Text.Encoding.Default))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)//Поиск заказаного города отправления в базе
                    {
                        if (line.ToString() != "")
                        {
                            string[] part = new string[8];
                            part = line.Split('-');
                            

                            MdataOdjaz = part[0];
                            MdataPrzyjazd = part[1];
                            MmiesOdjazd = part[2];
                            MmiesPrzyjazd = part[3];
                            MhourOdjazd = part[4];
                            MhourPrzyjazd = part[5];
                            MminOdjazd = part[6];
                            MminPrzyjazd = part[7];

                            Wyjazd = true;
                        }
                    }
                }
                
                minuta = 0;
                if (!Wyjazd)
                {
                    if (!uzytkownikDom)
                    {
                        timeNow();
                        string line = "";
                        string[] part = new string[] { "", "", "", "", "", "" };
                        using (StreamReader sr = new StreamReader(pathUzytkownik, System.Text.Encoding.Default))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                part = line.Split('-');
                                if ((part[0] != "7" || part[0] != "6") && part[0] == "8" && Int32.Parse(part[3]) == Int32.Parse(hoursNow) && (10 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                                {
                                    uzytkownikJedzi = true;
                                    uzytkownikDom = true;
                                    break;
                                }
                                else if ((part[0] != "7" || part[0] != "6") && part[0] == "8" && Int32.Parse(part[3]) - 1 == Int32.Parse(hoursNow) && (10 <= (Int32.Parse(part[4]) + 59) - Int32.Parse(minNow)))
                                {
                                    uzytkownikJedzi = true;
                                    uzytkownikDom = true;
                                    break;
                                }

                                if (part[0] == Den.ToString() && Int32.Parse(part[3]) == Int32.Parse(hoursNow) && (10 >= Int32.Parse(part[4]) - Int32.Parse(minNow) && 0 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                                {
                                    uzytkownikJedzi = true;
                                    uzytkownikDom = true;
                                    break;
                                }
                                else if (part[0] == Den.ToString() && Int32.Parse(part[3]) - 1 == Int32.Parse(hoursNow) && (10 >= (Int32.Parse(part[4]) + 59) - Int32.Parse(minNow) && 0 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                                {
                                    uzytkownikJedzi = true;
                                    uzytkownikDom = true;
                                    break;
                                }

                            }
                        }
                    }
                }
                else if(Wyjazd)
                {
                    DelH = 23 - Int32.Parse(hoursNow);
                    spozniam = false;
                    uzytkownikJedzi = false;

                    //MessageBox.Show(MminPrzyjazd.ToString() + " - " + monthNow + " || " + MdataPrzyjazd + " - " + dayNow);

                    if (MmiesPrzyjazd == monthNow && MdataPrzyjazd == dayNow)
                    {
                        if (Int32.Parse(MminPrzyjazd) < Int32.Parse(minNow))
                        {
                            DelM = (Int32.Parse(MminPrzyjazd) + 59) - Int32.Parse(minNow);
                        }
                        DelH = Int32.Parse(MhourPrzyjazd) - Int32.Parse(hoursNow);
                        DelM = Int32.Parse(MminPrzyjazd) - Int32.Parse(minNow);

                        MessageBox.Show("Время до запуска функции: " + DelH.ToString() + " : " + DelM.ToString());
                        if (DelH == 0 && DelM == 0)
                        {
                            MessageBox.Show("NEN");
                            MdataOdjaz = null;
                            MdataPrzyjazd = null;
                            MmiesOdjazd = null;
                            MmiesPrzyjazd = null;
                            MhourOdjazd = null;
                            MhourPrzyjazd = null;
                            MminOdjazd = null;
                            MminPrzyjazd = null;

                            string[] fileLines = File.ReadAllLines(pathWyjazd);
                            fileLines[0] = ""; // Замена
                            File.WriteAllLines(pathWyjazd, fileLines);

                            Wyjazd = false;
                            uzytkownikJedzi = true;
                        }
                    }

                }
            }

            if (spozniam)
            {
                if (TwentyMin >= 1200)
                {
                    if (TenMin >= 600)
                    {
                        TwentyMin = 0;
                        TenMin = 0;
                    }
                    else
                    {
                        Klimat();
                        TenMin++;
                    }
                }
                else
                    TwentyMin++;
            }
            else if (spozniam && spoz.Text != "")
            {
                if (minuta == 60)
                {
                    minuta = 0;
                    timeNow();
                    string line = "";
                    string[] part = new string[] { "", "", "", "", "", "" };
                    using (StreamReader sr = new StreamReader(pathUzytkownik, System.Text.Encoding.Default))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            part = line.Split('-');
                            if ((part[0] != "7" || part[0] != "6") && part[0] == "8" && Int32.Parse(spoz.Text) + Int32.Parse(part[3]) == Int32.Parse(hoursNow) && (10 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                            {
                                uzytkownikJedzi = true;
                                uzytkownikDom = true;
                                break;
                            }
                            else if ((part[0] != "7" || part[0] != "6") && part[0] == "8" && Int32.Parse(spoz.Text) + Int32.Parse(part[3]) - 1 == Int32.Parse(hoursNow) && (10 <= (Int32.Parse(part[4]) + 59) - Int32.Parse(minNow)))
                            {
                                uzytkownikJedzi = true;
                                uzytkownikDom = true;
                                break;
                            }

                            if (part[0] == Den.ToString() && Int32.Parse(spoz.Text) + Int32.Parse(part[3]) == Int32.Parse(hoursNow) && (10 >= Int32.Parse(part[4]) - Int32.Parse(minNow) && 0 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                            {
                                uzytkownikJedzi = true;
                                uzytkownikDom = true;
                                break;
                            }
                            else if (part[0] == Den.ToString() && Int32.Parse(spoz.Text) + Int32.Parse(part[3]) - 1 == Int32.Parse(hoursNow) && (10 >= (Int32.Parse(part[4]) + 59) - Int32.Parse(minNow) && 0 <= Int32.Parse(part[4]) - Int32.Parse(minNow)))
                            {
                                uzytkownikJedzi = true;
                                uzytkownikDom = true;
                                break;
                            }

                        }
                    }
                    if (uzytkownikJedzi)
                        UzIdz.IsChecked = true;

                    if (UzIdz.IsChecked == true)
                        uzytkownikDom = true;

                    if (UzDo.IsChecked == true)
                    {
                        UzIdz.IsChecked = false;
                        uzytkownikDom = true;
                    }
                    else if (UzIdz.IsChecked == false)
                        uzytkownikDom = false;

                    Kondi.IsChecked = false;
                    Obogrev.IsChecked = false;
                    Okno.IsChecked = false;

                    Klimat();
                }
            }
            else
            {
                if (uzytkownikJedzi)
                    UzIdz.IsChecked = true;

                if (UzIdz.IsChecked == true)
                    uzytkownikDom = true;

                if (UzDo.IsChecked == true)
                {
                    UzIdz.IsChecked = false;
                    uzytkownikDom = true;
                }
                else if (UzIdz.IsChecked == false)
                    uzytkownikDom = false;

                Kondi.IsChecked = false;
                Obogrev.IsChecked = false;
                Okno.IsChecked = false;

                Klimat();
            }
        }

        private void Klimat()
        {
            if (tempDom > uzytkownikTemp && uzytkownikDom == true)
            {
                ileOtworz = 0;
                if (tempUlica + 2 < tempDom)
                {
                    tempDom--;
                    TeDo.Content = tempDom.ToString();
                    Okno.IsChecked = true;
                }
                else
                {
                    tempDom--;
                    TeDo.Content = tempDom.ToString();
                    Kondi.IsChecked = true;
                }
            }
            else if (tempDom < uzytkownikTemp && uzytkownikDom == true)
            {
                ileOtworz = 0;
                if (tempUlica - 2 > tempDom)
                {
                    tempDom++;
                    TeDo.Content = tempDom.ToString();
                    Okno.IsChecked = true;
                }
                else
                {
                    tempDom++;
                    TeDo.Content = tempDom.ToString();
                    Obogrev.IsChecked = true;
                }
            }
            else if (uzytkownikDom == false)
            {
                if (ileOtworz != 5)
                {
                    ileOtworz++;
                    Okno.IsChecked = true;
                    if (tempUlica - 2 > tempDom)
                    { tempDom++; TeDo.Content = tempDom.ToString(); }
                    else if (tempUlica + 2 < tempDom)
                    { tempDom--; TeDo.Content = tempDom.ToString(); }
                }
            }
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            //ChanTem.IsEnabled = true;
            temp.IsEnabled = true;
            


            tempUlica = rand.Next(-30, 40);
            tempDom = rand.Next(10, 30);

            teUl.Content = tempUlica.ToString();
            TeDo.Content = tempDom.ToString();
        }

        private void temp_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {
        }


        bool op = true;

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if (BudzGod.Text != "" && BudzMin.Text != "")
            {
                if (op)
                { budzik = true; op = false; }
                else
                { budzik = false; op = true; }
            }
        }
        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            if (JaluzOp.IsChecked == true)
                JaluzOp.IsChecked = false;
            else
                JaluzOp.IsChecked = true;
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            Travel();
            Bilety(cityArrGow, cityDepGow, hourArr, minArr, dayArr, monthArr, yearArr);
            Clear();
        }

        private void year_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
        }

        private void Button_Click_13(object sender, RoutedEventArgs e)
        {

            Zap.IsEnabled = true;
            visib(8, false);
            if (Int32.Parse(textUz.Text) > 0 && Int32.Parse(textUz.Text) <= 8 || (textUz.Text != null))
            {
                visib(Int32.Parse(textUz.Text), true);
            }
        }

        private void textUz_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Potw.IsEnabled = true;
        }

        private void textUz_TouchEnter(object sender, System.Windows.Input.TouchEventArgs e)
        {
            
        }

        private void textUz_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            
        }
   
        private void Button_Click_14(object sender, RoutedEventArgs e)
        {
            try
            {
                int min = 0, max = 24;

                if (Int32.Parse(textUz.Text) > 0)
                {
                    for (int i = 0; i < Int32.Parse(textUz.Text); i++)
                    {
                        switch (i)
                        {
                            case 0:
                                max = Int32.Parse(t2.Text);
                                min = Int32.Parse(t1.Text);
                                break;
                            case 1:
                                if (max > Int32.Parse(t2_Copy.Text)) max = Int32.Parse(t2_Copy.Text);
                                if (min < Int32.Parse(t1_Copy.Text)) min = Int32.Parse(t1_Copy.Text);
                                break;
                            case 2:
                                if (max > Int32.Parse(t2_Copy1.Text)) max = Int32.Parse(t2_Copy1.Text);
                                if (min < Int32.Parse(t1_Copy1.Text)) min = Int32.Parse(t1_Copy1.Text);
                                break;
                            case 3:
                                if (max > Int32.Parse(t2_Copy2.Text)) max = Int32.Parse(t2_Copy2.Text);
                                if (min < Int32.Parse(t1_Copy2.Text)) min = Int32.Parse(t1_Copy2.Text);
                                break;
                            case 4:
                                if (max > Int32.Parse(t2_Copy3.Text)) max = Int32.Parse(t2_Copy3.Text);
                                if (min < Int32.Parse(t1_Copy3.Text)) min = Int32.Parse(t1_Copy3.Text);
                                break;
                            case 5:
                                if (max > Int32.Parse(t2_Copy4.Text)) max = Int32.Parse(t2_Copy4.Text);
                                if (min < Int32.Parse(t1_Copy4.Text)) min = Int32.Parse(t1_Copy4.Text);
                                break;
                            case 6:
                                if (max > Int32.Parse(t2_Copy5.Text)) max = Int32.Parse(t2_Copy5.Text);
                                if (min < Int32.Parse(t1_Copy5.Text)) min = Int32.Parse(t1_Copy5.Text);
                                break;
                            case 7:
                                if (max > Int32.Parse(t2_Copy6.Text)) max = Int32.Parse(t2_Copy6.Text);
                                if (min < Int32.Parse(t1_Copy6.Text)) min = Int32.Parse(t1_Copy6.Text);
                                break;
                            default:
                                break;
                        }
                    }

                    if (min < max)
                    {
                        string line = "";
                        string[] part = new string[] { "", "", "", "", "", "" };
                        int x = 0;
                        int prawda = 0;
                        bool on = true;
                        using (StreamReader sr = new StreamReader(pathUzytkownik, System.Text.Encoding.Default))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                part = line.Split('-');
                                if (Int32.Parse(part[0]) == 8)
                                {
                                    on = false;
                                    prawda = x;
                                }
                            }
                            x++;


                            if (on)
                            {
                                using (StreamWriter sw = new StreamWriter(pathUzytkownik, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                                {
                                    sw.WriteLine("8" + "-" + min.ToString() + "-" + "0" + "-" + max.ToString() + "-" + "0" + "-" + "0");
                                }
                            }
                            else
                            {
                                string[] fileLines = File.ReadAllLines(pathUzytkownik);
                                fileLines[prawda] = "8" + "-" + min.ToString() + "-" + "0" + "-" + max.ToString() + "-" + "0" + "-" + "0"; // Замена
                                File.WriteAllLines(pathUzytkownik, fileLines);
                            }
                        }

                        MessageBox.Show("Dane zapisane");
                    }
                    else
                        MessageBox.Show("Dane nie zapisane");
                    visib(8, false);
                }
            }
            catch { MessageBox.Show("Podaj dane"); }
        }

        private void Button_Click_16(object sender, RoutedEventArgs e)
        {
        }

        private void UzDo_Checked(object sender, RoutedEventArgs e)
        {
        }
        bool uzD = false;
        bool prziszol = false;
        int uszolHour = -1;
        int uszolMin = -1;

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void temp_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (temp.Text != "" || temp.Text != null)
                ChanTem.IsEnabled = true;
        }

        private void TabControl_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        int pozition = 0;
        int iloscc = 0;
        private void UzDo_Click(object sender, RoutedEventArgs e)
        {
            UzytkownikDom();
        }

        private void UzytkownikDom()
        {
            pozition = 0;

            if (!uzD)//Пользователь пришел
            {
                uzD = true;


                string line = "";
                string[] part = new string[] { "", "", "", "", "", "" };
                if (prziszol)
                {
                    using (StreamReader sr = new StreamReader(pathUzytkownik, System.Text.Encoding.Default))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            part = line.Split('-');

                            if (iloscc == pozition && (part[3] == hoursNow && (5 >= Int32.Parse(part[4]) - Int32.Parse(minNow) && -5 <= Int32.Parse(part[4]) - Int32.Parse(minNow))))
                            {
                                string[] fileLines = File.ReadAllLines(pathUzytkownik);
                                fileLines[iloscc] = Den + "-" + part[1] + "-" + part[2] + "-" + part[3] + "-" + part[4] + "-" + (Int32.Parse(part[5]) + 1); // Замена
                                File.WriteAllLines(pathUzytkownik, fileLines);
                                iloscc = 0;
                            }
                            else if (iloscc == pozition && (part[3] != hoursNow || (part[3] == hoursNow && 5 <= Int32.Parse(part[4]) - Int32.Parse(minNow) && -5 >= Int32.Parse(part[4]) - Int32.Parse(minNow))))
                            {
                                using (StreamWriter sw = new StreamWriter(pathUzytkownik, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                                {
                                    sw.WriteLine(part[0] + "-" + part[1] + "-" + part[2] + "-" + hoursNow + "-" + minNow + "-" + "0");

                                    iloscc = 0;
                                }
                                pozition++;
                            }
                        }
                    }
                }
                else
                {

                    if (uszolHour != -1 || uszolMin != -1)
                    {
                        using (StreamWriter sw = new StreamWriter(pathUzytkownik, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                        {
                            sw.WriteLine(Den + "-" + uszolHour + "-" + uszolMin + "-" + hoursNow + "-" + minNow + "-" + "0");
                        }
                    }
                }

            }
            else //Пользователь ушел
            {
                uzD = false;

                string line = "";
                string[] part = new string[] { "", "", "", "", "", "" };

                using (StreamReader sr = new StreamReader(pathUzytkownik, System.Text.Encoding.Default))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        part = line.Split('-');
                        if (part[0] == Den.ToString() && part[1] == hoursNow && (Int32.Parse(part[2]) - Int32.Parse(minNow) >= -5 && Int32.Parse(part[2]) - Int32.Parse(minNow) <= 5))
                        {
                            iloscc = pozition;
                            uszolHour = Int32.Parse(part[1]);
                            uszolMin = Int32.Parse(part[2]);
                            prziszol = true;
                            break;
                        }
                        else if (part[0] != Den.ToString() && (part[1] != hoursNow || (part[1] == hoursNow && Int32.Parse(part[2]) - Int32.Parse(minNow) < -5 && Int32.Parse(part[2]) - Int32.Parse(minNow) > 5)))
                        {
                            iloscc = pozition;
                            uszolHour = Int32.Parse(hoursNow);
                            uszolMin = Int32.Parse(minNow);
                            prziszol = false;
                            break;

                        }
                        pozition++;
                    }
                }

            }
        }
        bool flip = false;
        private void Button_Click_17(object sender, RoutedEventArgs e)
        {
            if (uzytkownikDom == false)
            {
                if (!flip && (spoz.Text == null || spoz.Text == "0"))
                {
                    MessageBox.Show("Status: spużniam się");
                    flip = true;

                    Spoz.Content = "Anuluj";
                    spoz.Text = null;
                    spozniam = true;
                }
                else if (!flip && (spoz.Text != null || spoz.Text != "0"))
                {
                    MessageBox.Show("Status: spużniam się na " + spoz.Text);
                    flip = true;

                    spoz.Text = null;
                    Spoz.Content = "Anuluj";
                    spozniam = true;
                }
                else
                {
                    MessageBox.Show("Status: normalny");
                    flip = false;

                    Spoz.Content = "Spóźniam się";
                    spozniam = false;
                }
            }
            else
                MessageBox.Show("Użytkownik w domu");
        }
   
        private void Button_Click_18(object sender, RoutedEventArgs e)
        {
            Kalendarz kalend = new Kalendarz();
            kalend.Show();
        }
        

        private void Button_Click_15(object sender, RoutedEventArgs e)
        {
        }

        private void Jaluzi(object sender, EventArgs e)
        {

            timeNow();
            if (Int32.Parse(hoursNow) >= 6 && Int32.Parse(hoursNow) < 20)
            { Morn.Content = "Ranek/Dzień"; morning = true; }
            else
            { Morn.Content = "Wieczór/Noc"; morning = false; }


            if (PC.IsChecked == true && puls > 62)
              puls--; 
            else if (PC.IsChecked == false && puls < 72)
              puls++; 

            if (WC.IsChecked == true && Weightbed < 80)
              Weightbed += 40; 
            else if (WC.IsChecked == false && Weightbed > 0)
              Weightbed -= 40; 

            if (TC.IsChecked == true && TemperatureBody > 36)
              TemperatureBody -= 0.2f; 
            else if (TC.IsChecked == false && TemperatureBody < 36.5f)
              TemperatureBody += 0.2f;


            if (puls <= 63 && Weightbed > 35 && TemperatureBody <= 36.2f && !morning)
            { JaluzOp.IsChecked = false; uzytkownikSpi = true; }
            else if (puls >= 71 && TemperatureBody > 36.6 && morning)
            { JaluzOp.IsChecked = true; uzytkownikSpi = false; }

            if (BudzGod.Text != "" && BudzMin.Text != "")
            {
                if (Int32.Parse(BudzGod.Text) == Int32.Parse(hoursNow) && Int32.Parse(BudzMin.Text) == Int32.Parse(minNow) && budzik && morning)
                    JaluzOp.IsChecked = true;
            }

            Puls.Content = puls.ToString();
            weightB.Content = Weightbed.ToString();
            Temperatura.Content = TemperatureBody.ToString();
        }

   
        bool a = true;
        private void arduino_Click(object sender, RoutedEventArgs e)
        {

            if (a)
            {
                init("1");
                a = false;
            }
            else
            {
                init("2");
                a = true;
            }
        }

        private void init(string room)
        {
            if (room != "0")
            {
                if (Int32.Parse(room) < 5 && !uzytkownikSpi && uzytkownikDom)
                {
                    int x = 0;
                    bool on = true;
                    string line;
                    string[] part = new string[4];

                    timeNow();

                    using (StreamReader sr = new StreamReader(pathLight, System.Text.Encoding.Default))
                    {
                        string minOne = minNow.Substring(0, 1);
                        string minTwo = minNow.Substring(minNow.Length - 1, 1);
                        if (Int32.Parse(minTwo) > 0 && Int32.Parse(minTwo) <= 5)
                            minTwo = "5";
                        else
                        {
                            if (minOne == "5")
                            {
                                minOne = "0";
                            }
                            minTwo = "0";
                        }

                        minNow = minOne + minTwo;

                        while ((line = sr.ReadLine()) != null)
                        {
                            part = line.Split('-');
                            if (part[0] == room && part[2] == hoursNow && part[3] == minNow)
                            {
                                on = false;
                                break;
                            }
                            x++;
                        }
                    }
                    using (StreamWriter sw = new StreamWriter(pathLight, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                    {
                        if (on)
                        {
                            sw.WriteLine(room + "-" + "0" + "-" + hoursNow + "-" + minNow);
                        }
                    }
                    if (!on)
                    {
                        string[] fileLines = File.ReadAllLines(pathLight);
                        fileLines[x] = part[0] + "-" + (Int32.Parse(part[1]) + 1) + "-" + part[2] + "-" + part[3]; // Замена
                        File.WriteAllLines(pathLight, fileLines);
                    }
                }
                try // подключается к порт к которому подключен Ардуино
                {
                    if (ArduinoOff)
                    {
                        myport = new SerialPort();
                        myport.BaudRate = 9600;
                        myport.PortName = "COM3";
                        myport.Open();
                        ArduinoOff = false;
                    }

                    myport.WriteLine(room);// Передает на порт значение один. В компиляторе Ардуино это 1 должно обработатся 
                }
                catch (Exception)
                { MessageBox.Show("Błąd. Arduino nie podłączone do portu"); }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            timeNow();
            MessageBox.Show("Day: " + dayNow + " Month: " + monthNow + " Year: " + yearNow);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
                if (Arr1.Text != "" && Dep1.Text != "")
                {
                    using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                    {
                        if (Arr1.Text != "")
                        {
                            while ((cityLine = sr.ReadLine()) != null)//Поиск заказаного города отправления в базе
                            {
                                cityArr = cityLine.Substring(0, cityLine.Length - 8);
                                if (Arr1.Text == cityArr)
                                {
                                    kodArr = cityLine.Substring(cityArr.Length + 1, 7);
                                    break;
                                }
                            }
                        }
                        else MessageBox.Show("Prosze wpisać skąd wyjeżdża (Nie używać liter polskich)");
                    }

                    using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                    {
                        if (Dep1.Text != "")
                        {
                            while ((cityLine = sr.ReadLine()) != null)//Поиск заказаного города прибытия в базе
                            {
                                cityDep = cityLine.Substring(0, cityLine.Length - 8);
                                if (Dep1.Text == cityDep)
                                {
                                    kodDep = cityLine.Substring(cityDep.Length + 1, 7);
                                    break;
                                }
                            }
                        }
                        else MessageBox.Show("Prosze wpisać dokąd wyjeżdżasz (Nie używać liter polskich)");
                    }

                    timeNow();//Узнает настоящее время

                    if (hour.Text == "") hourArr = hoursNow;//Если записаны часы, то ставится оно иначе ставится настоящее время
                    else if (Int32.Parse(hour.Text) < 24 && Int32.Parse(hour.Text) >= 0) hourArr = hour.Text;

                    if (minn.Text == "") minArr = minNow;//Если записаны минуты, то ставится оно иначе ставится настоящее время
                    else if (Int32.Parse(minn.Text) < 60 && Int32.Parse(minn.Text) >= 0) minArr = minn.Text;

                    if (day.Text == "") dayArr = dayNow;//Если записаны часы, то ставится оно иначе ставится настоящее время
                    else dayArr = day.Text;

                    if (month1.Text == "") monthArr = monthNow;//Если записаны минуты, то ставится оно иначе ставится настоящее время
                    else monthArr = month1.Text;

                    if (year1.Text == "") yearArr = yearNow;//Если записаны минуты, то ставится оно иначе ставится настоящее время
                    else yearArr = year1.Text;


                    if ((hour.Text == "" && minn.Text == "" || minn.Text == "" && Int32.Parse(hour.Text) < 24 && Int32.Parse(hour.Text) >= 0 || hour.Text == "" && Int32.Parse(minn.Text) < 60 && Int32.Parse(minn.Text) >= 0) && day.Text == "" || Int32.Parse(day.Text) <= 31 && Int32.Parse(day.Text) > 0)
                    {
                        string link = "http://rozklad-pkp.pl/pl/tp?queryPageDisplayed=yes&REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0ID=&REQ0JourneyStops1.0G=&REQ0JourneyStopover1=&REQ0JourneyStops2.0G=&REQ0JourneyStopover2=&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0ID=&REQ0HafasSearchForw=1&existBikeEverywhere=yes&existHafasAttrInc=yes&existHafasAttrInc=yes&REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_1_0=1&REQ0JourneyProduct_prod_section_2_0=1&REQ0JourneyProduct_prod_section_3_0=1&REQ0JourneyProduct_prod_section_0_1=1&REQ0JourneyProduct_prod_section_1_1=1&REQ0JourneyProduct_prod_section_2_1=1&REQ0JourneyProduct_prod_section_3_1=1&REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_1_2=1&REQ0JourneyProduct_prod_section_2_2=1&REQ0JourneyProduct_prod_section_3_2=1&REQ0JourneyProduct_prod_section_0_3=1&REQ0JourneyProduct_prod_section_1_3=1&REQ0JourneyProduct_prod_section_2_3=1&REQ0JourneyProduct_prod_section_3_3=1&REQ0JourneyProduct_opt_section_0_list=0%3A000000&REQ0HafasOptimize1=&existOptimizePrice=1&REQ0HafaChangeTime=0%3A1&existSkipLongChanges=0&REQ0HafasAttrExc=&existHafasAttrInc=yes&existHafasAttrExc=yes&wDayExt0=Pn%7CWt%7C%C5%9Ar%7CCz%7CPt%7CSo%7CNd&start=start&existUnsharpSearch=yes&singlebutton=&came_from_form=1&REQ0JourneyStopsS0G=" + kodArr + "&REQ0JourneyStopsZ0G=" + kodDep + "&date=" + dayArr + "." + monthArr + "." + yearArr + "&dateStart=" + dayArr + "." + monthArr + "." + yearArr + "&dateEnd=" + dayArr + "." + monthArr + "." + yearArr + "&REQ0JourneyDate=" + dayArr + "." + monthArr + "." + yearArr + "&time=" + hourArr + "%3A" + minArr + "&REQ0JourneyTime=" + hourArr + "%3A" + minArr;
                        WebClient wc = new WebClient();
                        Response = wc.DownloadString(link);
                        textBox.Text = link;
                        Parse.IsEnabled = true;

                        try
                        {
                            int x = 0;
                            bool on = true;
                            string line;
                            string[] part = new string[2];

                            using (StreamReader sr = new StreamReader(pathTravel, System.Text.Encoding.Default))
                            {
                                while ((line = sr.ReadLine()) != null)
                                {
                                    part = line.Split('-');
                                    if (part[0] == kodArr && part[1] == kodDep)
                                    {
                                        on = false;
                                        break;
                                    }
                                    x++;
                                }
                            }

                            using (StreamWriter sw = new StreamWriter(pathTravel, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                            {
                                if (on)
                                {
                                    sw.WriteLine(kodArr + "-" + kodDep + "-" + "0");
                                }
                            }
                            if (!on)
                            {
                                string[] fileLines = File.ReadAllLines(pathTravel);
                                fileLines[x] = part[0] + "-" + part[1] + "-" + (Int32.Parse(part[2]) + 1); // Замена
                                File.WriteAllLines(pathTravel, fileLines);
                            }
                        }
                        catch { }//Проверка есть ли такой путь в базе
                    }
                }
        }

        private void Bilety(string cityLineArr, string cityLineDep, string hour, string min, string day, string month, string year)
        {
            //MessageBox.Show("From: " + cityLineArr + " To: " + cityLineDep + "  Hour: " + hour + " Min: " + min + " Day: " + day + " Month: " + month + " Year: " + year);
            if (cityLineArr == "" && cityLineDep == "" && hour == "" && min == "" && day == "" && month == "" && year == "")
            { }
            else
                {
                if (cityLineArr.Length > 8 && cityLineDep.Length > 8)
                {
                    kodArr = cityLineArr.Substring(cityLineArr.Length - 7, 7);

                    kodDep = cityLineDep.Substring(cityLineDep.Length - 7, 7);
                }
                else
                {
                    kodArr = cityLineArr;
                    kodDep = cityLineDep;
                }

                timeNow();//Узнает настоящее время

                if (hour == "") hourArr = hoursNow;//Если не записан час, то ставится настоящий час

                if (min == "") minArr = minNow;//Если не записаны минуты, то ставится настоящий минута

                if (day == "") dayArr = dayNow;//Если не записан день, то ставится настоящий день

                if (month == "") monthArr = monthNow;//Если не записан месяц, то ставится настоящий месяц

                if (year == "") yearArr = yearNow;//Если не записан год, то ставится настоящий год



                string link = "http://rozklad-pkp.pl/pl/tp?queryPageDisplayed=yes&REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0ID=&REQ0JourneyStops1.0G=&REQ0JourneyStopover1=&REQ0JourneyStops2.0G=&REQ0JourneyStopover2=&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0ID=&REQ0HafasSearchForw=1&existBikeEverywhere=yes&existHafasAttrInc=yes&existHafasAttrInc=yes&REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_1_0=1&REQ0JourneyProduct_prod_section_2_0=1&REQ0JourneyProduct_prod_section_3_0=1&REQ0JourneyProduct_prod_section_0_1=1&REQ0JourneyProduct_prod_section_1_1=1&REQ0JourneyProduct_prod_section_2_1=1&REQ0JourneyProduct_prod_section_3_1=1&REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_1_2=1&REQ0JourneyProduct_prod_section_2_2=1&REQ0JourneyProduct_prod_section_3_2=1&REQ0JourneyProduct_prod_section_0_3=1&REQ0JourneyProduct_prod_section_1_3=1&REQ0JourneyProduct_prod_section_2_3=1&REQ0JourneyProduct_prod_section_3_3=1&REQ0JourneyProduct_opt_section_0_list=0%3A000000&REQ0HafasOptimize1=&existOptimizePrice=1&REQ0HafaChangeTime=0%3A1&existSkipLongChanges=0&REQ0HafasAttrExc=&existHafasAttrInc=yes&existHafasAttrExc=yes&wDayExt0=Pn%7CWt%7C%C5%9Ar%7CCz%7CPt%7CSo%7CNd&start=start&existUnsharpSearch=yes&singlebutton=&came_from_form=1&REQ0JourneyStopsS0G=" + kodArr + "&REQ0JourneyStopsZ0G=" + kodDep + "&date=" + dayArr + "." + monthArr + "." + yearArr + "&dateStart=" + dayArr + "." + monthArr + "." + yearArr + "&dateEnd=" + dayArr + "." + monthArr + "." + yearArr + "&REQ0JourneyDate=" + dayArr + "." + monthArr + "." + yearArr + "&time=" + hourArr + "%3A" + minArr + "&REQ0JourneyTime=" + hourArr + "%3A" + minArr;
                WebClient wc = new WebClient();
                Response = wc.DownloadString(link);
                textBoxRecogn.Text = link;

                try
                {
                    int x = 0;
                    bool on = true;
                    string line;
                    string[] part = new string[2];

                    using (StreamReader sr = new StreamReader(pathTravel, System.Text.Encoding.Default))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            part = line.Split('-');
                            if (part[0] == kodArr && part[1] == kodDep)
                            {
                                on = false;
                                break;
                            }
                            x++;
                        }
                    }

                    using (StreamWriter sw = new StreamWriter(pathTravel, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                    {
                        if (on)
                        {
                            sw.WriteLine(kodArr + "-" + kodDep + "-" + "0");
                        }
                    }
                    if (!on)
                    {
                        string[] fileLines = File.ReadAllLines(pathTravel);
                        fileLines[x] = part[0] + "-" + part[1] + "-" + (Int32.Parse(part[2]) + 1); // Замена
                        File.WriteAllLines(pathTravel, fileLines);
                    }
                }
                catch { }//Проверка есть ли такой путь в базе


                Buy();
            }
        }

        private void WklLight(object sender, EventArgs e)
        {
            init(room);
            LightOn();
        }

        private void LightOn()
        {
            int Hmax = 0;
            int Mmax = 0;
            int ma = 0;
            int room1;
            for (int sort = 1; sort <= HourLightOn.Length - 1; sort++)
            {
                for (int sort2 = 1; sort2 <= HourLightOn.Length - 1; sort2++)
                {
                    if ((HourLightOn[sort2 - 1] > 0 && MinLightOn[sort2 - 1] > 0) && (HourLightOn[sort2] > 0 && MinLightOn[sort2] > 0))
                    {
                        if (Int32.Parse(HourLightOn[sort2].ToString() + MinLightOn[sort2].ToString()) < Int32.Parse(HourLightOn[sort2 - 1].ToString() + MinLightOn[sort2 - 1].ToString()))
                        {
                            room1 = RoomsOn[sort2];
                            Hmax = HourLightOn[sort2];
                            Mmax = MinLightOn[sort2];
                            RoomsOn[sort2] = RoomsOn[sort2 - 1];
                            HourLightOn[sort2] = HourLightOn[sort2 - 1];
                            MinLightOn[sort2] = MinLightOn[sort2 - 1];
                            RoomsOn[sort2 - 1] = room1;
                            HourLightOn[sort2 - 1] = Hmax;
                            MinLightOn[sort2 - 1] = Mmax;
                        }
                    }
                    if (HourLightOn[sort2] > ma)
                        ma = HourLightOn[sort2];
                }
            }
            h = HourLightOn[0];
            m = MinLightOn[0];
            for (int i = 0; i < HourLightOn.Length; i++)
            {
                if (HourLightOn[i] - Int32.Parse(hoursNow) < 0 && MinLightOn[i] - Int32.Parse(minNow) < 0)
                {
                }
                else
                {
                    if (HourLightOn[i] - Int32.Parse(hoursNow) == 0 && MinLightOn[i] - Int32.Parse(minNow) < 0)
                    {
                    }
                    else if (HourLightOn[i] - Int32.Parse(hoursNow) >= 0)
                    {
                        HourLightOn[i] -= Int32.Parse(hoursNow);
                        MinLightOn[i] -= Int32.Parse(minNow);
                        if (MinLightOn[i] < 0)
                        {
                            HourLightOn[i]--;
                            MinLightOn[i] = 60 - Math.Abs(MinLightOn[i]);
                        }
                    }
                    if (Int32.Parse(h.ToString() + m.ToString()) > Int32.Parse(HourLightOn[i].ToString() + MinLightOn[i].ToString()))
                    {
                        h = HourLightOn[i];
                        m = MinLightOn[i];
                        room = RoomsOn[i].ToString();
                    }
                }
            }
            if ((ma < Int32.Parse(hoursNow)) || (ma == 23 && hoursNow == "23"))
            {
                h = 24 - Int32.Parse(hoursNow) - 1;
                m = 60 - Int32.Parse(minNow);
                room = "0";
            }
        }

        private void TimeThrough()
        {
            int loop = -1;
            int RoomSelect = 0;
            int count = 0;
            string line = "";
            firstStart = false;
            int For = 0;
            using (StreamReader sr = new StreamReader(pathLight, System.Text.Encoding.Default))
            {
                while ((line = sr.ReadLine()) != null)
                { count++; }
            }

            int[] exception = new int[count];
            HourLightOn = new int[count];
            MinLightOn = new int[count];
            for (int o = 0; o < exception.Length; o++)
                        exception[o] = -1;
            for (int o = 0; o < HourLightOn.Length; o++)
            {
                HourLightOn[o] = -1;
                MinLightOn[o] = -1;
            }
            RoomsOn = new int[count];
            for (int rooms = 1; rooms <= 3; rooms += 2)
            {
                for (int zapis = 0; zapis < 3; zapis++)
                {
                    loop++;
                    string[] times = new string[count];// Все минуты которые попали в разлет от -15 до +15
                    int[] timesWeight = new int[count];
                    string[] CountMinMax = new string[count];
                    for (int i = 0; i < times.Length; i++)
                    {
                        times[i] = "";
                        timesWeight[i] = -1;
                    }
                    count = -1;
                    line = "";
                    int max = 0, maxCount = 0, hour = 0, min = 0;
                    int maxMin = 0, minMin = 0;// Разлет времени +15 и -15 минут
                    int sum = 0, avg = 0, notNull = 0;
                    short PerewodHour = 0; //Перевод часа вперед или назад  0- не нужен перевод, 1- перевод назад, 2 - перевод вперед
                    string PartNow = "";
                    string[] part = new string[4] { "", "", "", "" }; //Разделение строк из блокнота



                    // Находит максимальное число его время
                    using (StreamReader sr = new StreamReader(pathLight, System.Text.Encoding.Default))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            bool jest = true;
                            count++;
                            part = line.Split('-');
                            for (int i = 0; i < exception.Length; i++)
                            {
                                if (exception[i] == count)
                                    jest = false;
                            }
                            if (Int32.Parse(part[1]) > 5 && jest)
                            {
                                if (part[0] == rooms.ToString() && Int32.Parse(part[1]) > max)
                                {
                                    max = Int32.Parse(part[1]);
                                    maxCount = count;
                                    hour = Int32.Parse(part[2]);
                                    min = Int32.Parse(part[3]);
                                    PartNow = part[2] + part[3];
                                }
                            }

                        }
                    }
                    //MessageBox.Show(max.ToString());
                    if (max > 5)
                    {
                        count = 0;

                        room = part[0];

                        //Ставит разлет -15 до +15 минут от самого максимального. Ставит перевод часов (hour).
                        if (min + 15 >= 60)
                        {
                            maxMin = 15 - (60 - min);
                            PerewodHour = 2;
                        }
                        else
                            maxMin = min + 15;

                        if (min - 15 < 0)
                        {
                            minMin = 60 - (15 - min);
                            PerewodHour = 1;
                        }
                        else if (min - 15 == 0)
                            minMin = 0;
                        else
                            minMin = min - 15;
                        bool on = true;
                        int predel = -1;
                        //Находит время включения света в пределе -15 до +15 минут. Запись минут в times[] а количество раз в CountMinMax[]
                        using (StreamReader sr = new StreamReader(pathLight, System.Text.Encoding.Default))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                on = true;
                                predel++;
                                //exception[For]++;
                                //MessageBox.Show("Exception " + predel.ToString());
                                part = line.Split('-');
                                if (part[0] == rooms.ToString() && Int32.Parse(part[3]) >= minMin && Int32.Parse(part[3]) < 60 || Int32.Parse(part[3]) <= maxMin && Int32.Parse(part[3]) >= 0 && Int32.Parse(part[1]) > 5)
                                {
                                    switch (PerewodHour)
                                    {
                                        case 0:
                                            if (Int32.Parse(part[2]) == hour)
                                            {
                                                times[count] = part[3];
                                                if (exception.Length - 1 != exception[For])
                                                {
                                                    exception[For] = predel;
                                                    For++;
                                                }
                                                on = false;
                                            }
                                            break;
                                        case 1:
                                            if ((minMin <= Int32.Parse(part[3]) && hour != Int32.Parse(part[2])) || (maxMin >= Int32.Parse(part[3]) && hour == Int32.Parse(part[2])))
                                            {
                                                times[count] = part[3];
                                                CountMinMax[count] = part[1];
                                                if (exception.Length - 1 != exception[For])
                                                {
                                                    exception[For] = predel;
                                                    For++;
                                                }
                                            }
                                            break;
                                        case 2:
                                            if ((maxMin >= Int32.Parse(part[3]) && hour != Int32.Parse(part[2])) || (minMin <= Int32.Parse(part[3]) && hour == Int32.Parse(part[2])))
                                            {
                                                times[count] = part[3];
                                                CountMinMax[count] = part[1];
                                                if (exception.Length - 1 != exception[For])
                                                {
                                                    exception[For] = predel;
                                                    For++;
                                                }
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    if (exception[For] == exception.Length - 1 && on)
                                    {
                                        exception[For] = -1;
                                    }
                                }

                                count++;
                            }
                        }

                        //Раставление весов и пеервод минут
                        for (int i = 0; i < times.Length; i++)
                        {
                            if (times[i] != min.ToString())
                            {
                                // Если центральное число больше 0, то минуты (45,50,55) пеерведи в (-15,-10,-5) и наоборот
                                if (PerewodHour == 1)
                                {
                                    switch (times[i])
                                    {
                                        case "55":
                                            times[i] = "-5";
                                            break;
                                        case "50":
                                            times[i] = "-10";
                                            break;
                                        case "45":
                                            times[i] = "-15";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else if (PerewodHour == 2)
                                {
                                    switch (times[i])
                                    {
                                        case "00":
                                            times[i] = "60";
                                            break;
                                        case "05":
                                            times[i] = "65";
                                            break;
                                        case "10":
                                            times[i] = "70";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                //Ставит первые веса в переменную timesWeight[]. Эти веса по расстоянию. +15 или -15 от центрального = 1; +10 или -10 от центрального = 2 и так далее
                                if (times[i] != null && times[i] != "")
                                {
                                    if (Int32.Parse(times[i]) - 15 == min || Int32.Parse(times[i]) + 15 == min)
                                    { timesWeight[i] = 1; }
                                    else if (Int32.Parse(times[i]) - 10 == min || Int32.Parse(times[i]) + 10 == min)
                                    { timesWeight[i] = 2; }
                                    else if (Int32.Parse(times[i]) - 5 == min || Int32.Parse(times[i]) + 5 == min)
                                    { timesWeight[i] = 3; }
                                }
                            }
                            else
                                timesWeight[i] = 0;
                           }

                        if (PerewodHour != 0)
                        {
                            int proc = 0;
                            int proc2 = 0;
                            // Ставятся вторые веса по процентам. Замена переменной где хранились количество включенного света, на вторые веса. Суммирование обеих весов
                            for (int i = 0; i < CountMinMax.Length; i++)
                            {
                                if (timesWeight[i] > 0)
                                {
                                    // proc = количество включенного света * 100 / максимальное число включенного света. 
                                    proc = Int32.Parse(CountMinMax[i]) * 100 / max;
                                    if (proc >= 0 && proc <= 24)
                                    { CountMinMax[i] = "1"; }
                                    else if (proc >= 25 && proc <= 49)
                                    { CountMinMax[i] = "2"; }
                                    else if (proc >= 50 && proc <= 74)
                                    { CountMinMax[i] = "3"; }
                                    else if (proc >= 75 && proc <= 100)
                                    { CountMinMax[i] = "4"; }

                                    // К первым весам добавляются вторые веса
                                    timesWeight[i] += Int32.Parse(CountMinMax[i]); // timesWeight[] - Сумма весов
                                }
                                //MessageBox.Show("Веса: " + timesWeight[i]);
                            }

                            //Находит самые максимальные веса и запоминает его позицию
                            string PartMin1 = "", PartMin2 = "";

                            int weightMax = 0;
                            int position = 0;


                            for (int i = 0; i < CountMinMax.Length; i++)
                            {
                                if (timesWeight[i] > weightMax)
                                {
                                    weightMax = timesWeight[i];
                                    position = i;
                                }
                            }

                            //Поиск максимального значения в пределе -15 до +15
                            string s = File.ReadAllLines(pathLight).Skip(position).First();
                            part = s.Split('-');
                            PartMin1 = part[2] + part[3];
                            // Поиск второго самого максимального числа и запоминание его позиции
                            int weightMax2 = 0;
                            int position2 = -1;

                            for (int i = 0; i < CountMinMax.Length; i++)
                            {
                                if (timesWeight[i] > weightMax2 && i != position)
                                {
                                    weightMax2 = timesWeight[i];
                                    position2 = i;
                                }
                            }
                            //Поиск данных в блокноте
                            string a = File.ReadAllLines(pathLight).Skip(position2).First();
                            part = a.Split('-');
                            PartMin2 = part[2] + part[3];

                            //Перевод в проценты первого и второго максимального числа
                            proc = weightMax * 100 / 7;
                            proc2 = weightMax2 * 100 / 7;

                            if ((Int32.Parse(PartMin1) < Int32.Parse(PartNow) && Int32.Parse(PartNow) < Int32.Parse(PartMin2)) || (Int32.Parse(PartMin1) > Int32.Parse(PartNow) && Int32.Parse(PartNow) > Int32.Parse(PartMin2)))
                            {
                                if (proc > proc2)
                                    proc -= proc2;
                                else if (proc < proc2)
                                    proc = proc2 - proc;
                            }
                            else if ((Int32.Parse(PartMin1) < Int32.Parse(PartNow) && Int32.Parse(PartNow) > Int32.Parse(PartMin2)) || (Int32.Parse(PartMin1) > Int32.Parse(PartNow) && Int32.Parse(PartNow) < Int32.Parse(PartMin2)))
                                proc += proc2;



                            //MessageBox.Show("Процент = " + proc);

                            int rozn = 0; //Отрезок минут. Например максимальное число 50, минимально 40, rozn = 10

                            if (min > Int32.Parse(times[position]))
                                rozn = (min - Int32.Parse(times[position])) / 2;
                            else
                                rozn = (Int32.Parse(times[position]) - min) / 2;

                            //перевод прцентов в минуты.
                            int godz = rozn * proc / 100;

                            int TI = 0;

                           
                            if ((Int32.Parse(PartMin1) < Int32.Parse(PartNow) && Int32.Parse(PartNow) < Int32.Parse(PartMin2)) || ((Int32.Parse(PartMin1) > Int32.Parse(PartNow) && Int32.Parse(PartNow) > Int32.Parse(PartMin2))))
                            {
                                if (Int32.Parse(times[position]) < min)
                                    TI = min - godz;
                                else if (Int32.Parse(times[position]) > min)
                                    TI = min + godz;
                                else
                                    TI = godz;
                            }
                            else if ((Int32.Parse(PartMin1) < Int32.Parse(PartNow) && Int32.Parse(PartNow) > Int32.Parse(PartMin2)) || (Int32.Parse(PartMin1) > Int32.Parse(PartNow) && Int32.Parse(PartNow) < Int32.Parse(PartMin2)))
                            {
                                if (Int32.Parse(PartNow) > Int32.Parse(PartMin1))
                                    TI = min - godz;
                                else if (Int32.Parse(PartNow) < Int32.Parse(PartMin1))
                                    TI = min + godz;
                            }

                            if (PerewodHour == 1)
                            {
                                TI += 60;
                                if (TI < 60)
                                { hour--; }
                                else if (TI > 60)
                                { TI -= 60; }
                                else
                                    TI = 0;
                            }

                            HourLightOn[RoomSelect] = hour;
                            MinLightOn[RoomSelect] = TI;
                            RoomsOn[RoomSelect] = rooms;

                            RoomSelect++;
                        }
                        else
                        {
                            for (int i = 0; i < times.Length; i++)
                            {
                                if (times[i] != null && times[i] != "")
                                {
                                    sum += Int32.Parse(times[i]);
                                    notNull++;
                                }
                            }
                            avg = sum / notNull;

                            avg = (min + avg) / 2;

                            if (avg > min && PerewodHour == 1)
                                hour--;
                            else if (avg < min && PerewodHour == 2)
                                hour++;

                            if (10 - avg > 0)
                            {
                                string avgS = "0" + avg.ToString();
                            }

                            HourLightOn[RoomSelect] = hour;
                            MinLightOn[RoomSelect] = avg;
                            RoomsOn[RoomSelect] = rooms;

                            RoomSelect++;
                        }
                    }
                }
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            
        }

        private void Travel()
        {
            int max = 0;
            string line;
            string[] part = new string[2];

            using (StreamReader sr = new StreamReader(pathTravel, System.Text.Encoding.Default))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    part = line.Split('-');
                    if (Int32.Parse(part[2]) > max)
                    {
                        max = Int32.Parse(part[2]);
                        break;
                    }
                }
            }
            using (StreamReader sr = new StreamReader(pathTravel, System.Text.Encoding.Default))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    part = line.Split('-');
                    if (Int32.Parse(part[2]) == max)
                    {
                        cityArrGow = part[0];
                        cityDepGow = part[1];
                        break;
                    }
                }
            }
        }

        private void Buy()
        {
            int[] ilosc = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };// Смотрит на какой строке
            string[] part = new string[10] { "", "", "", "", "", "", "", "", "", "" }; // Части текста из таблиц в HTML
            string text = "";// Текст который мы режим на части 
            int n = 0;
            bool zakaz = false;

            string search = "focus_guiVCtrl_connection_detailsOut_select_C0-";
            string time = "ODJAZD";
            string data = "czas przejazdu:";
            string kupZ = "kupBiletPrHandler";
            string kupZo = "W komunikacji krajowej zakup mo";

            timeNow();

            for (int i = 0; i <= 12; i++)
            {
                if (Response.LastIndexOf(search + i.ToString()) != -1)
                {
                    n++;
                    ilosc[0] = Response.LastIndexOf(search + i.ToString());// Поиск нулевого элемента
                    text = Response.Substring(ilosc[0]);// Обрезка до нулевого элемента
                }
            }


            for (int i = 0; i <= n; i++)
            {
                if (i == 0)
                {
                    if (Response.LastIndexOf(search + "0") != -1)
                    {
                        ilosc[0] = Response.LastIndexOf(search + "0");// Поиск нулевого элемента
                        text = Response.Substring(ilosc[0]);// Обрезка до нулевого элемента
                    }
                }
                else if (i == 1)
                {
                    if (text.LastIndexOf(search + i.ToString()) != -1)
                    {
                        ilosc[i] = text.LastIndexOf(search + i.ToString());// Находит первый элемент из таблицы 
                        part[i - 1] = text.Substring(0, text.Length - ilosc[i]);// Записывает первую часть в массив
                    }
                }
                else
                {
                    if (text.LastIndexOf(search + i.ToString()) != -1)
                    {
                        ilosc[i] = text.LastIndexOf(search + i.ToString());// Находит первый элемент из таблицы 
                        part[i - 1] = text.Substring(ilosc[i - 1], text.Length - ilosc[i]);// Записывает части в массив
                    }
                    else
                    {
                        part[i - 1] = text.Substring(ilosc[i - 1], 10000);// Записывает последнюю часть в массив
                    }
                }
            }

            for (int i = 0; i < n; i++)// Смотрит на время отправления каждого поезда в таблице
            {
                int v = part[i].IndexOf(data);
                string barada2 = part[i].Substring(v + 38, 8);
                string day = barada2.Substring(0, 2);
                int a = part[i].IndexOf(time);
                string barada = part[i].Substring(a + 7, 5);
                string hours = barada.Substring(0, 2);
                string min = barada.Substring(3, 2);
                if (Int32.Parse(hours) > Int32.Parse(hoursNow) && Int32.Parse(day) == Int32.Parse(dayNow) || Int32.Parse(hours) < Int32.Parse(hoursNow) && Int32.Parse(day) > Int32.Parse(dayNow))// Если время отправки больше за настоящее время, то закажи.
                {
                    if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                    {
                        ComputerSpeak("Arrive Time: " + hours + " : " + min);
                        zakaz = true;
                        break;
                    }
                }
                else if (Int32.Parse(hours) == Int32.Parse(hoursNow) && 20 < Int32.Parse(min) - Int32.Parse(minNow))
                {
                    if (part[i].LastIndexOf(kupZo) != -1 || part[i].LastIndexOf(kupZ) != -1)
                    {
                        ComputerSpeak("Arrive Time: " + hours + " : " + min);
                        zakaz = true;
                        break;
                    }
                }
            }

            




            if (!zakaz)
            {
                ComputerSpeak("No tickets");
                if (Int32.Parse(hourArr) + 2 < 24)
                {
                    int x = Int32.Parse(hourArr) + 2;
                    hourArr = x.ToString();
                }
                else
                {
                    int x = Int32.Parse(hourArr) + 2 - 24;
                    hourArr = "0" + x.ToString();
                }

                string link = "http://rozklad-pkp.pl/pl/tp?queryPageDisplayed=yes&REQ0JourneyStopsS0A=1&REQ0JourneyStopsS0ID=&REQ0JourneyStops1.0G=&REQ0JourneyStopover1=&REQ0JourneyStops2.0G=&REQ0JourneyStopover2=&REQ0JourneyStopsZ0A=1&REQ0JourneyStopsZ0ID=&REQ0HafasSearchForw=1&existBikeEverywhere=yes&existHafasAttrInc=yes&existHafasAttrInc=yes&REQ0JourneyProduct_prod_section_0_0=1&REQ0JourneyProduct_prod_section_1_0=1&REQ0JourneyProduct_prod_section_2_0=1&REQ0JourneyProduct_prod_section_3_0=1&REQ0JourneyProduct_prod_section_0_1=1&REQ0JourneyProduct_prod_section_1_1=1&REQ0JourneyProduct_prod_section_2_1=1&REQ0JourneyProduct_prod_section_3_1=1&REQ0JourneyProduct_prod_section_0_2=1&REQ0JourneyProduct_prod_section_1_2=1&REQ0JourneyProduct_prod_section_2_2=1&REQ0JourneyProduct_prod_section_3_2=1&REQ0JourneyProduct_prod_section_0_3=1&REQ0JourneyProduct_prod_section_1_3=1&REQ0JourneyProduct_prod_section_2_3=1&REQ0JourneyProduct_prod_section_3_3=1&REQ0JourneyProduct_opt_section_0_list=0%3A000000&REQ0HafasOptimize1=&existOptimizePrice=1&REQ0HafaChangeTime=0%3A1&existSkipLongChanges=0&REQ0HafasAttrExc=&existHafasAttrInc=yes&existHafasAttrExc=yes&wDayExt0=Pn%7CWt%7C%C5%9Ar%7CCz%7CPt%7CSo%7CNd&start=start&existUnsharpSearch=yes&singlebutton=&came_from_form=1&REQ0JourneyStopsS0G=" + kodArr + "&REQ0JourneyStopsZ0G=" + kodDep + "&date=" + dayArr + "." + monthArr + "." + yearArr + "&dateStart=" + dayArr + "." + monthArr + "." + yearArr + "&dateEnd=" + dayArr + "." + monthArr + "." + yearArr + "&REQ0JourneyDate=" + dayArr + "." + monthArr + "." + yearArr + "&time=" + hourArr + "%3A" + minArr + "&REQ0JourneyTime=" + hourArr + "%3A" + minArr;
                WebClient wc = new WebClient();
                Response = wc.DownloadString(link);
            }

            Clear();
        }

        private void wikipedia(string speech)
        {
            //Парсинг Википедии
            string linkWiki = "https://en.wikipedia.org/wiki/" +speech;
            textBoxRecogn.Text = linkWiki; // Запись ссылки
            WebClient wc = new WebClient();
            string WikiParse = ""; // Целый текст
            int SubString; // Что обрезать в тексте 
            WikiParse = wc.DownloadString(linkWiki); // Запиши всб HTML разметку с сайта

            //Текст из первого абзаца
            if (WikiParse.IndexOf("<p>") != -1)
            {
                SubString = WikiParse.IndexOf("<p>");// Найди в тексте <p>
                WikiParse = WikiParse.Substring(SubString + 3, 10000); // Обреж до найденого символа и после на 10000 знаков
                if (WikiParse.IndexOf("</p>") != -1)
                {
                    SubString = WikiParse.IndexOf("</p>");// Найди в тексте </p>
                    WikiParse = WikiParse.Substring(0, SubString);// Обреж после найденого символа
                }
            }
            //Перевод HTML разметки в обычный текст
            while (WikiParse.IndexOf(">") != -1)
            {
                WikiParse = WikiParse.Remove(WikiParse.IndexOf("<"), WikiParse.IndexOf(">") - WikiParse.IndexOf("<") + 1);// Удали все от "<" и до ">"
            }
            while (WikiParse.IndexOf("]") != -1)
            {
                WikiParse = WikiParse.Remove(WikiParse.IndexOf("["), WikiParse.IndexOf("]") - WikiParse.IndexOf("[") + 1);// Удали все от "[" и до "]"
            }


            string[] words = WikiParse.Split(new char[] { ' ' });// Разделяет текст по словам и записывает слова в массив
            string writePath = PathStart + @"Dyplom\Dictionary\words\";// Путь к словам Джарвиса
            string allword = PathStart + @"Dyplom\Dictionary\Allwords.txt";
            string path; // Конкретный файл в который будет записано слово
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].ToLower();// Нижний регистр слов
                words[i] = words[i].Replace(".", "");// Замени разные символы пустотой
                words[i] = words[i].Replace(",", "");
                words[i] = words[i].Replace("(", "");
                words[i] = words[i].Replace(")", "");
                words[i] = words[i].Replace("!", "");
                words[i] = words[i].Replace("?", "");

                string firstLet;// Первая буква в слове
                firstLet = words[i].Substring(0, 1);
                path = writePath + firstLet + ".txt";// Полный путь к файлу записи
                try
                {
                    bool on = true;
                    bool AllOn = true;
                    using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))// Проверка слова в файле. Если его нет то слово запишится
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line == words[i])
                            { on = false; }
                        }
                    }
                    using (StreamReader sr = new StreamReader(allword, System.Text.Encoding.Default))// Проверка слова в файле. Если его нет то слово запишится
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line == words[i])
                            { AllOn = false; }
                        }
                    }
                    if (on)
                    {
                        using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                        {
                            sw.WriteLine(words[i]);
                        }
                    }
                    if (AllOn)
                    {
                        using (StreamWriter sw = new StreamWriter(allword, true, System.Text.Encoding.Default))// Если нет такого слова, то запиши его
                        {
                            sw.WriteLine(words[i]);
                        }
                    }
                }
                catch { }
            }

            //MessageBox.Show(WikiParse);
            ComputerSpeak(WikiParse);
        }
    }
}
