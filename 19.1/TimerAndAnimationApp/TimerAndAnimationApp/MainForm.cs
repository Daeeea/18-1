using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimerAndAnimationApp
{
    public partial class MainForm : Form
    {
        private Button btnStudyTimer;
        private Button btnBouncingBall;
        private Button btnAnalogClock;
        private Button btnExit;
        private Label lblTitle;
        private Panel contentPanel;

        private StudyTimerForm studyTimerForm;
        private BouncingBallForm bouncingBallForm;
        private AnalogClockForm analogClockForm;

        public MainForm()
        {
            this.Text = "Таймеры и Анимация";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(44, 62, 80);
            this.IsMdiContainer = false;

            SetupUI();
        }

        private void SetupUI()
        {
            lblTitle = new Label
            {
                Text = "Таймеры и Анимация",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            Panel menuPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(52, 73, 94)
            };

            btnStudyTimer = CreateMenuButton("📚 Таймер для занятий", 20, Color.FromArgb(46, 204, 113));
            btnBouncingBall = CreateMenuButton("⚽ Летающий мячик", 80, Color.FromArgb(231, 76, 60));
            btnAnalogClock = CreateMenuButton("🕐 Аналоговые часы", 140, Color.FromArgb(52, 152, 219));
            btnExit = CreateMenuButton("❌ Выход", 560, Color.FromArgb(149, 165, 166));

            btnStudyTimer.Click += BtnStudyTimer_Click;
            btnBouncingBall.Click += BtnBouncingBall_Click;
            btnAnalogClock.Click += BtnAnalogClock_Click;
            btnExit.Click += (s, e) => Application.Exit();

            menuPanel.Controls.AddRange(new Control[] {
                btnStudyTimer, btnBouncingBall, btnAnalogClock, btnExit
            });

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241)
            };

            this.Controls.Add(contentPanel);
            this.Controls.Add(menuPanel);
            this.Controls.Add(lblTitle);

            ShowWelcomeMessage();
        }

        private Button CreateMenuButton(string text, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(200, 50),
                Location = new Point(10, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };
        }

        private void BtnStudyTimer_Click(object sender, EventArgs e)
        {
            if (studyTimerForm != null && !studyTimerForm.IsDisposed)
            {
                studyTimerForm.Close();
                studyTimerForm.Dispose();
            }

            studyTimerForm = new StudyTimerForm();
            ShowFormInPanel(studyTimerForm);
        }

        private void BtnBouncingBall_Click(object sender, EventArgs e)
        {
            if (bouncingBallForm != null && !bouncingBallForm.IsDisposed)
            {
                bouncingBallForm.Close();
                bouncingBallForm.Dispose();
            }

            bouncingBallForm = new BouncingBallForm();
            ShowFormInPanel(bouncingBallForm);
        }

        private void BtnAnalogClock_Click(object sender, EventArgs e)
        {
            if (analogClockForm != null && !analogClockForm.IsDisposed)
            {
                analogClockForm.Close();
                analogClockForm.Dispose();
            }

            analogClockForm = new AnalogClockForm();
            ShowFormInPanel(analogClockForm);
        }

        private void ShowFormInPanel(Form form)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(form);
            form.Show();
        }

        private void ShowWelcomeMessage()
        {
            Label welcomeLabel = new Label
            {
                Text = "Добро пожаловать!\n\nВыберите приложение из меню слева",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            contentPanel.Controls.Add(welcomeLabel);
        }
    }
}