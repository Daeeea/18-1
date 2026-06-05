using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TimerAndAnimationApp
{
    public partial class AnalogClockForm : Form  
    {
        private System.Windows.Forms.Timer clockTimer;
        private Panel clockPanel;
        private Label digitalLabel;
        private Label dateLabel;
        private CheckBox chkSmoothSecond;
        private Button btnStartStop;

        private bool isRunning = true;
        private float smoothSecond = 0;

        public AnalogClockForm()
        {
            this.BackColor = Color.FromArgb(44, 62, 80);
            SetupUI();
            SetupTimer();
        }

        private void SetupUI()
        {
            clockPanel = new Panel
            {
                Size = new Size(400, 400),
                Location = new Point(50, 30),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            clockPanel.Paint += ClockPanel_Paint;
            clockPanel.Resize += (s, e) => clockPanel.Invalidate();

            digitalLabel = new Label
            {
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219),
                BackColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 440),
                Size = new Size(400, 50)
            };

            dateLabel = new Label
            {
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(189, 195, 199),
                BackColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 490),
                Size = new Size(400, 30)
            };

            FlowLayoutPanel controlPanel = new FlowLayoutPanel
            {
                Location = new Point(150, 530),
                Size = new Size(200, 30),
                FlowDirection = FlowDirection.LeftToRight
            };

            chkSmoothSecond = new CheckBox
            {
                Text = "Плавная секундная стрелка",
                AutoSize = true,
                ForeColor = Color.White,
                Checked = true
            };

            btnStartStop = new Button
            {
                Text = "⏸ Пауза",
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat
            };

            chkSmoothSecond.CheckedChanged += (s, e) => clockPanel.Invalidate();
            btnStartStop.Click += (s, e) =>
            {
                isRunning = !isRunning;
                btnStartStop.Text = isRunning ? "⏸ Пауза" : "▶ Старт";
                btnStartStop.BackColor = isRunning ? Color.FromArgb(46, 204, 113) : Color.FromArgb(241, 196, 15);
            };

            controlPanel.Controls.AddRange(new Control[] { chkSmoothSecond, btnStartStop });

            this.Controls.Add(clockPanel);
            this.Controls.Add(digitalLabel);
            this.Controls.Add(dateLabel);
            this.Controls.Add(controlPanel);
        }

        private void SetupTimer()
        {
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 100;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;

            if (chkSmoothSecond.Checked)
            {
                DateTime now = DateTime.Now;
                smoothSecond = now.Second + (now.Millisecond / 1000f);
            }

            clockPanel.Invalidate();

            digitalLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            dateLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("ru-RU"));
        }

        private void ClockPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int centerX = clockPanel.Width / 2;
            int centerY = clockPanel.Height / 2;
            Point center = new Point(centerX, centerY);
            int radius = Math.Min(clockPanel.Width, clockPanel.Height) / 2 - 10;

            using (Brush bgBrush = new SolidBrush(Color.White))
            {
                g.FillEllipse(bgBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            using (Pen borderPen = new Pen(Color.FromArgb(52, 73, 94), 3))
            {
                g.DrawEllipse(borderPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }

            for (int i = 1; i <= 60; i++)
            {
                double angle = i * 6 * Math.PI / 180;
                int xStart = centerX + (int)((radius - 10) * Math.Sin(angle));
                int yStart = centerY - (int)((radius - 10) * Math.Cos(angle));
                int xEnd = centerX + (int)(radius * Math.Sin(angle));
                int yEnd = centerY - (int)(radius * Math.Cos(angle));

                if (i % 5 == 0)
                {
                    using (Pen pen = new Pen(Color.Black, 3))
                    {
                        xEnd = centerX + (int)((radius - 15) * Math.Sin(angle));
                        yEnd = centerY - (int)((radius - 15) * Math.Cos(angle));
                        g.DrawLine(pen, xStart, yStart, xEnd, yEnd);
                    }
                }
                else
                {
                    using (Pen pen = new Pen(Color.Gray, 1))
                    {
                        g.DrawLine(pen, xStart, yStart, xEnd, yEnd);
                    }
                }

                if (i % 5 == 0)
                {
                    int number = (i / 5 == 0) ? 12 : i / 5;
                    string text = number.ToString();
                    Font font = new Font("Segoe UI", 14, FontStyle.Bold);
                    SizeF textSize = g.MeasureString(text, font);
                    int x = centerX + (int)((radius - 30) * Math.Sin(angle)) - (int)(textSize.Width / 2);
                    int y = centerY - (int)((radius - 30) * Math.Cos(angle)) - (int)(textSize.Height / 2);

                    using (Brush brush = new SolidBrush(Color.FromArgb(44, 62, 80)))
                    {
                        g.DrawString(text, font, brush, x, y);
                    }
                    font.Dispose();
                }
            }

            DateTime now = DateTime.Now;
            int hour = now.Hour % 12;
            int minute = now.Minute;
            int second = now.Second;
            float smoothSec = chkSmoothSecond.Checked ? smoothSecond : second;

            float hourAngle = (hour * 30 + minute * 0.5f) * (float)Math.PI / 180;
            float minuteAngle = (minute * 6 + second * 0.1f) * (float)Math.PI / 180;
            float secondAngle = (smoothSec * 6) * (float)Math.PI / 180;

            int hourLength = (int)(radius * 0.5);
            Point hourEnd = new Point(
                centerX + (int)(hourLength * Math.Sin(hourAngle)),
                centerY - (int)(hourLength * Math.Cos(hourAngle))
            );

            using (Pen hourPen = new Pen(Color.FromArgb(44, 62, 80), 6))
            {
                hourPen.StartCap = LineCap.Round;
                hourPen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(hourPen, center, hourEnd);
            }

            int minuteLength = (int)(radius * 0.7);
            Point minuteEnd = new Point(
                centerX + (int)(minuteLength * Math.Sin(minuteAngle)),
                centerY - (int)(minuteLength * Math.Cos(minuteAngle))
            );

            using (Pen minutePen = new Pen(Color.FromArgb(52, 73, 94), 4))
            {
                minutePen.StartCap = LineCap.Round;
                minutePen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(minutePen, center, minuteEnd);
            }

            int secondLength = (int)(radius * 0.85);
            Point secondEnd = new Point(
                centerX + (int)(secondLength * Math.Sin(secondAngle)),
                centerY - (int)(secondLength * Math.Cos(secondAngle))
            );

            using (Pen secondPen = new Pen(Color.Red, 2))
            {
                secondPen.StartCap = LineCap.Round;
                secondPen.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(secondPen, center, secondEnd);
            }

            using (Brush centerBrush = new SolidBrush(Color.FromArgb(52, 73, 94)))
            {
                g.FillEllipse(centerBrush, centerX - 6, centerY - 6, 12, 12);
            }

            using (Brush decorationBrush = new SolidBrush(Color.Gold))
            {
                g.FillEllipse(decorationBrush, centerX - 3, centerY - 3, 6, 6);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            clockTimer?.Stop();
            clockTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}