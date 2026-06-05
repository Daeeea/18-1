using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace TimerAndAnimationApp
{
    public partial class StudyTimerForm : Form  // 👈 Добавлено слово "partial"
    {
        private System.Windows.Forms.Timer mainTimer;
        private System.Windows.Forms.Timer blinkTimer;
        private Label timeLabel;
        private Label phaseLabel;
        private ProgressBar progressBar;
        private Button btnStart;
        private Button btnPause;
        private Button btnReset;
        private ListBox lbLog;
        private Label statsLabel;

        private int currentSeconds;
        private int totalSeconds;
        private TimerPhase currentPhase;
        private int cyclesCompleted = 0;
        private int totalStudyMinutes = 0;

        private enum TimerPhase
        {
            Study,      // 25 минут - учеба
            ShortBreak, // 5 минут - короткий перерыв
            LongBreak   // 15 минут - длинный перерыв
        }

        public StudyTimerForm()
        {
            this.BackColor = Color.White;
            SetupUI();
            SetupTimer();
        }

        private void SetupUI()
        {
            // Основной дисплей
            timeLabel = new Label
            {
                Text = "25:00",
                Font = new Font("Segoe UI", 72, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 150,
                BackColor = Color.White
            };

            // Фаза таймера
            phaseLabel = new Label
            {
                Text = "📚 Время учиться!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };

            // Прогресс-бар
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 30,
                Minimum = 0,
                Maximum = 100
            };

            // Статистика
            statsLabel = new Label
            {
                Text = "📊 Сегодня: 0 минут учебы | Циклов: 0",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(127, 140, 141),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.White
            };

            // Панель кнопок
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            btnStart = new Button
            {
                Text = "▶ Старт",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            btnPause = new Button
            {
                Text = "⏸ Пауза",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Enabled = false
            };

            btnReset = new Button
            {
                Text = "🔄 Сброс",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };

            btnStart.Click += BtnStart_Click;
            btnPause.Click += BtnPause_Click;
            btnReset.Click += BtnReset_Click;

            buttonPanel.Controls.AddRange(new Control[] { btnStart, btnPause, btnReset });

            // Лог сессий
            Label logLabel = new Label
            {
                Text = "📋 История сессий:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10, 5, 0, 0),
                BackColor = Color.White
            };

            lbLog = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            this.Controls.Add(lbLog);
            this.Controls.Add(logLabel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(statsLabel);
            this.Controls.Add(progressBar);
            this.Controls.Add(phaseLabel);
            this.Controls.Add(timeLabel);
        }

        private void SetupTimer()
        {
            // Основной таймер (1000 мс)
            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Interval = 1000;
            mainTimer.Tick += MainTimer_Tick;

            // Таймер для мигания (50 мс)
            blinkTimer = new System.Windows.Forms.Timer();
            blinkTimer.Interval = 50;

            currentPhase = TimerPhase.Study;
            SetPhaseDuration();
        }

        private void SetPhaseDuration()
        {
            switch (currentPhase)
            {
                case TimerPhase.Study:
                    totalSeconds = 25 * 60;
                    phaseLabel.Text = "📚 Время учиться!";
                    phaseLabel.ForeColor = Color.FromArgb(52, 152, 219);
                    timeLabel.ForeColor = Color.FromArgb(52, 152, 219);
                    break;
                case TimerPhase.ShortBreak:
                    totalSeconds = 5 * 60;
                    phaseLabel.Text = "☕ Короткий перерыв";
                    phaseLabel.ForeColor = Color.FromArgb(46, 204, 113);
                    timeLabel.ForeColor = Color.FromArgb(46, 204, 113);
                    break;
                case TimerPhase.LongBreak:
                    totalSeconds = 15 * 60;
                    phaseLabel.Text = "🎉 Длинный перерыв!";
                    phaseLabel.ForeColor = Color.FromArgb(155, 89, 182);
                    timeLabel.ForeColor = Color.FromArgb(155, 89, 182);
                    break;
            }

            currentSeconds = totalSeconds;
            UpdateDisplay();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (currentSeconds > 0)
            {
                currentSeconds--;
                UpdateDisplay();

                if (currentSeconds == 10)
                {
                    SystemSounds.Beep.Play();
                    StartBlinking();
                }
            }
            else
            {
                mainTimer.Stop();
                blinkTimer.Stop();
                SystemSounds.Exclamation.Play();

                string phaseName = currentPhase == TimerPhase.Study ? "Учеба" :
                                  (currentPhase == TimerPhase.ShortBreak ? "Короткий перерыв" : "Длинный перерыв");

                if (currentPhase == TimerPhase.Study)
                {
                    totalStudyMinutes += 25;
                    UpdateStats();
                }

                lbLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} - Завершена сессия: {phaseName}");

                if (currentPhase == TimerPhase.Study)
                {
                    cyclesCompleted++;
                    if (cyclesCompleted % 4 == 0)
                        currentPhase = TimerPhase.LongBreak;
                    else
                        currentPhase = TimerPhase.ShortBreak;
                }
                else
                {
                    currentPhase = TimerPhase.Study;
                }

                SetPhaseDuration();
                UpdateDisplay();

                MessageBox.Show($"{phaseLabel.Text} завершен!", "Таймер",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                btnStart.Enabled = true;
                btnPause.Enabled = false;
                btnStart.Text = "▶ Старт";
                phaseLabel.BackColor = Color.White;
            }
        }

        private void StartBlinking()
        {
            bool isRed = false;
            blinkTimer.Tick += (s, e) =>
            {
                isRed = !isRed;
                phaseLabel.BackColor = isRed ? Color.Red : Color.White;
            };
            blinkTimer.Start();
        }

        private void UpdateDisplay()
        {
            int minutes = currentSeconds / 60;
            int seconds = currentSeconds % 60;
            timeLabel.Text = $"{minutes:D2}:{seconds:D2}";

            int percent = (int)((double)(totalSeconds - currentSeconds) / totalSeconds * 100);
            progressBar.Value = percent;
        }

        private void UpdateStats()
        {
            statsLabel.Text = $"📊 Сегодня: {totalStudyMinutes} минут учебы | Циклов: {cyclesCompleted}";
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            mainTimer.Start();
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnStart.Text = "▶ В работе";
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            mainTimer.Stop();
            blinkTimer.Stop();
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnStart.Text = "▶ Продолжить";
            phaseLabel.BackColor = Color.White;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            mainTimer.Stop();
            blinkTimer.Stop();
            cyclesCompleted = 0;
            totalStudyMinutes = 0;
            currentPhase = TimerPhase.Study;
            SetPhaseDuration();
            UpdateDisplay();
            UpdateStats();
            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnStart.Text = "▶ Старт";
            lbLog.Items.Clear();
            phaseLabel.BackColor = Color.White;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            mainTimer?.Stop();
            mainTimer?.Dispose();
            blinkTimer?.Stop();
            blinkTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}