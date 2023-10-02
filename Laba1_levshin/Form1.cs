using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba1_levshin
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {


            label2.Text = String.Format("Текущее значение: {0}", trackBar1.Value);
            Graphics g = pictureBox1.CreateGraphics();
            g.Clear(BackColor);
            DrawPole(g);

            Model mod = new Model(comands);

            List<Itm> m = mod.Rabot();


            trackBar1.Maximum = m[m.Count - 1].time / 30 + 1;


            for (int i = 0; i < m.Count; i++)
            {
                int j = trackBar1.Value * 30;
                switch (m[i].type)
                {
                    case 0: ZeroComand (g, m[i].time - j, m[i].nomer);         break;
                    case 1: OneComand  (g, m[i].time - j, m[i].nomer);         break;
                    case 2: TwoComand  (g, m[i].time - j, m[i].nomer, m[i].T); break;
                    case 3: ThreeComand(g, m[i].time - j, m[i].nomer, m[i].T); break;
                    case 4: FourComand (g, m[i].time - j, m[i].nomer);         break;
                }
            }
        }


        public Comand[] comands;

        #region consts
        public const int HEIGHT = 40;
        public const int WIDTH = 25;
        public const int LOCAL_ONE_LINE = 125;
        public const int LOCAL_TWO_LINE = LOCAL_ONE_LINE + HEIGHT * 2 + HEIGHT / 2;
        #endregion

        #region модель
        // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш
        public struct Comand
        {
            public int t;
            public bool kh;
            public int type;   // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш
            public int nomer;
            public int time;

            public Comand(int t, bool kh, int type)
            {
                this.t = t;
                this.kh = kh;
                this.type = type;
                nomer = 0;
                time = 0;
            }
            public Comand(int t, bool kh)
            {
                this.t = t;
                this.kh = kh;
                this.type = 0;
                nomer = 0;
                time = 0;
            }
        }

        public struct Itm
        {
            public int nomer;
            public int time;
            public int type;
            public int T;
            public Itm(int nomer, int time, int type, int T)
            {
                this.nomer = nomer;
                this.time = time;
                this.type = type;
                this.T = T;
            }
        }

        public partial class K1
        {
            public int timeStop;
            public int zadacha;



            public K1()
            {
                timeStop = 0;
                zadacha = 0;
            }


        }

        public partial class KK
        {
            public int timeStop;
            public int zadacha;

            public KK()
            {
                timeStop = 0;
                zadacha = 0;
            }



        }

        public partial class Model
        {
            KK kk = new KK();
            K1 k1 = new K1();

            List<Itm> queueKK = new List<Itm>();
            List<Itm> queueK1 = new List<Itm>();

            Comand[] cs;

            public Model(Comand[] cs) { this.cs = cs; }

            public List<Itm> Rabot()
            {
                List<Itm> DrawCom = new List<Itm>();
                int time = 0;

                int countCom = 0;

                //Один цыкл == Один такт

                while (true)
                {
                    // Если конвеир свободен и нет заявок от кэш контролера
                    // Обрабатывается новая команда
                    if (k1.timeStop <= 0 & queueK1.Count == 0)
                    {
                        // countCom - номер команды
                        // проверка того чтобы countCom не был больше количества команд
                        // Что означает что все команды обработаны
                        if (countCom == cs.Length)
                        {
                            //Доп проверка того что выполнены команды из КК
                            if (queueK1.Count == 0 & queueKK.Count == 0)
                                return DrawCom;
                        }
                        else
                        {
                            // Выполняется Декодировка

                            if (cs[countCom].kh)  // Данные есть в кэше
                            {
                                // Кэшь занят на 1 цикл
                                k1.timeStop = 1;

                                //Команда для отрисовки 

                                DrawCom.Add(new Itm(countCom, time, 1, cs[countCom].t));

                                queueK1.Insert(0, new Itm(countCom, time, cs[countCom].type, cs[countCom].t));


                            }
                            else // Кэш промах
                            {
                                //cs[countCom].nomer = countCom;

                                // Отпрака запроса в КК
                                queueKK.Add(new Itm(countCom, time, cs[countCom].type, cs[countCom].t));

                                //Отрисовка Команды 
                                DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t));

                                countCom++;
                                // continue чтобы не зашитало такт
                                continue;
                            }
                            countCom++;

                        }
                    }

                    // К1 свободен и есть заявка на работу
                    if (k1.timeStop <= 0 & queueK1.Count != 0)
                    {
                        // type == уровление устройством 
                        if (queueK1[0].type == 3)
                        {
                            // КК должен быть свободен 
                            // Иначе Должен ждать 
                            if (kk.timeStop <= 0)
                            {
                                Itm cur = queueK1[0];

                                k1.timeStop = 3 * cur.T;
                                k1.zadacha = 3;

                                cur.time = time;

                                DrawCom.Add(cur);
                                queueK1.RemoveAt(0);
                            }
                        }
                        else
                        {
                            if (queueK1[0].type == 2)
                            {

                                Itm cur = queueK1[0];
                                cur.time = time;
                                cur.type = 2;
                                DrawCom.Add(cur);
                                queueK1.RemoveAt(0);
                                k1.timeStop = 1 * cur.T;
                            }
                            else
                            {
                                if (queueK1[0].type == 1)
                                {
                                    k1.timeStop = 1;
                                    Itm cur = queueK1[0];
                                    cur.time = time;
                                    cur.type = 1;
                                    DrawCom.Add(cur);
                                    queueK1.RemoveAt(0);
                                }
                            }
                        }
                    }
                    // КК свободен и есть запрос
                    if (kk.timeStop <= 0 & queueKK.Count != 0 & k1.zadacha != 3)
                    {

                        kk.timeStop = 6;
                        Itm cur = queueKK[0];

                        cur.time = time;

                        DrawCom.Add(new Itm(queueKK[0].nomer, time, 4, queueKK[0].T));

                    }


                    if (kk.timeStop - 1 == 0)
                    {
                        Itm cur = queueKK[0];

                        cur.time = time;

                        //DrawCom.Add(new Itm(queueKK[0].nomer, time, 4, queueKK[0].T));

                        queueK1.Add(new Itm(cur.nomer, time, 1, 1));
                        queueK1.Add(cur);

                        queueKK.RemoveAt(0);
                    }

                    if (k1.timeStop - 1 == 0)
                    {
                        k1.zadacha = 0;
                    }

                    // Условный такт
                    kk.timeStop--;
                    k1.timeStop--;






                    time++;

                }



            }
        }

        public class TimeChek
        {
            public int start;
            public int end;

            public TimeChek(int start, int end)
            {
                this.start = start;
                this.end = end;
            }
        }

        #endregion

        #region "отрисовка"
        private void ZeroComand(Graphics g, int i, int n)
        {
            g.DrawLine(new Pen(Color.White, 4), new Point(i * WIDTH, LOCAL_ONE_LINE), new Point(i * WIDTH, 30));

            var rect = new Rectangle(i * WIDTH + 5, 30, 15, 13);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(new Pen(Color.White, 1), rect);
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 9), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void OneComand(Graphics g, int i, int n)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i *     WIDTH, LOCAL_ONE_LINE),
                new Point(i *     WIDTH, LOCAL_ONE_LINE - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_ONE_LINE - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_ONE_LINE)});

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE - HEIGHT + 6, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(new Pen(Color.White, 1), rect);
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        }
        private void TwoComand(Graphics g, int i, int n, int T)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i * WIDTH,       LOCAL_ONE_LINE),
                new Point(i * WIDTH,       LOCAL_ONE_LINE + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_ONE_LINE + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_ONE_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE + 12, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(new Pen(Color.White, 1), rect);
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void ThreeComand(Graphics g, int i, int n, int T)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH,     LOCAL_ONE_LINE),
                new Point(i * WIDTH,     LOCAL_ONE_LINE + HEIGHT),
                new Point((i+3*T) * WIDTH, LOCAL_ONE_LINE + HEIGHT),
                new Point((i+3*T) * WIDTH, LOCAL_ONE_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE + 12, 14, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(new Pen(Color.Black, 2), rect);
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void FourComand(Graphics g, int i, int n)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH, LOCAL_TWO_LINE),
                new Point(i * WIDTH, LOCAL_TWO_LINE - HEIGHT),
                new Point((i+6) * WIDTH, LOCAL_TWO_LINE - HEIGHT),
                new Point((i+6) * WIDTH,LOCAL_TWO_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_TWO_LINE - HEIGHT + 6, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawEllipse(new Pen(Color.Black, 2), rect);
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void DrawPole(Graphics g)
        {

            Pen whit = new Pen(Color.White, 2);


            Point line_n1 = new Point(0, LOCAL_ONE_LINE);
            Point line_e1 = new Point(2000, LOCAL_ONE_LINE);

            Point line_n2 = new Point(0, LOCAL_TWO_LINE);
            Point line_e2 = new Point(2000, LOCAL_TWO_LINE);

            g.DrawLine(new Pen(Color.White, 4), line_n1, line_e1);
            g.DrawLine(new Pen(Color.White, 4), line_n2, line_e2);

            Point[] chtrih1 = new Point[100];
            Point[] chtrih2 = new Point[100];

            for (int i = 0; i < chtrih1.Length; i++)
            {
                chtrih1[i] = new Point(i * WIDTH, LOCAL_ONE_LINE - 5);
            }


            for (int i = 0; i < chtrih2.Length; i++)
            {
                chtrih2[i] = new Point(i * WIDTH, LOCAL_ONE_LINE + 5);
            }

            for (int i = 0; i < chtrih1.Length; i++)
            {
                g.DrawLine(whit, chtrih1[i], chtrih2[i]);
                g.DrawLine(whit, new Point(chtrih1[i].X, chtrih1[i].Y + 100), new Point(chtrih2[i].X, chtrih2[i].Y + 100));
            }
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
            label1.Text = string.Format("{0},{1}", e.Location.X, e.Location.Y);
        }

        #endregion

        #region интерфейс
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            comands = new Comand[]
           {
                new Comand(2, false, 3),
                new Comand(2, false, 2),
                new Comand(2, true, 2),
                new Comand(2, true, 3),
                new Comand(1,false, 3),
                new Comand(1,false, 2),
                new Comand(1,false, 2),
                new Comand(2,true, 3),
                new Comand(1,true, 2),
                new Comand(1,false, 3),
                new Comand(1,true, 3),
                new Comand(1,false, 2),
                new Comand(1,true, 2)
           };
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Visible = true;
            label3.Visible   = true;
            button1.Visible  = true;
            
           
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Visible = true;
            label3.Visible = true;
            button1.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label8.Visible = true;
            richTextBox1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            button6.Visible = true;
            button4.Visible = true;
            textBox2.Visible = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int n = int.Parse(textBox1.Text);

            Random rnd = new Random();

            comands = new Comand[n];

            if (textBox1.Text != "")
            {

                for (int i = 0; i < n; i++)
                {
                    int rkh = rnd.Next(0, 100);
                    int rtype = rnd.Next(0, 100);

                    if (rtype > 15)
                    {
                        if (rkh > 25)
                            comands[i] = new Comand(rnd.Next(1, 4), true, 2);
                        else
                            comands[i] = new Comand(rnd.Next(1, 4), false, 2);
                    }
                    else
                    {
                        if (rkh > 10)
                            comands[i] = new Comand(rnd.Next(1, 4), true, 3);
                        else
                            comands[i] = new Comand(rnd.Next(1, 4), false, 3);
                    }

                }

                textBox1.Visible = false;
                label3.Visible   = false;
                button1.Visible  = false;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string cur = label8.Text;
            
            var r = cur.Split(';');

            if (r[1] == "Н,К")
            {
                r[1] = "КЭШ";
            }
            else
            { 
                r[1] = "Н,К"; 
            }

            label8.Text = string.Join(";", r);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string cur = label8.Text;

            var r = cur.Split(';');

            if (r[2] == "--")
            {
                r[2] = "УО";
            }
            else
            {
                r[2] = "--";
            }

            label8.Text = string.Join(";", r);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string cur = label8.Text;

            var r = cur.Split(';');

            r[0] = textBox2.Text;

            label8.Text = string.Join(";", r);

        }
        public int n = 0;

        private void button6_Click(object sender, EventArgs e)
        {
            
            
            richTextBox1.Text += label8.Text + "\n";

            string cur = label8.Text;

            var r = cur.Split(';');

            Comand f = new Comand();

            if (r[2] == "--")
            {
                f.type = 2;
            }
            else
                f.type = 3;

            if (r[1] == "Н,К")
            {
                f.kh = false;
            }
            else
            {
                f.kh = true;
            }
            f.t = int.Parse(r[0].Trim('т'));

            comands[n] = f;
            n++;

        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Visible     = false;
            label3.Visible       = false;
            button1.Visible      = false;
            label4.Visible       = false;
            label5.Visible       = false;
            label8.Visible       = false;
            richTextBox1.Visible = false;
            button2.Visible      = false;
            button3.Visible      = false;
            button6.Visible      = false;
            textBox2.Visible     = false;
            n = 0;
        }

        #endregion
    }
}

  

