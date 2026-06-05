using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TimerAndAnimationApp
{
    public partial class BouncingBallForm : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer trailTimer;
        private PictureBox ball;
        private Label infoLabel;
        private TrackBar speedTrackBar;
        private TrackBar gravityTrackBar;
        private Button btnStartStop;
        private Button btnReset;
        private CheckBox chkShowTrail;
        private Panel gameArea;

        private float velocityX = 5f;
        private float velocityY = 5f;
        private float gravity = 0.5f;
        private float bounceDamping = 0.95f;
        private bool isRunning = true;
        private int collisionCount = 0;

        public BouncingBallForm()
        {
            this.BackColor = Color.FromArgb(240, 248, 255);
            SetupUI();
            SetupTimers();
        }

        private void SetupUI()
        {
            gameArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 248, 255)
            };

            ball = new PictureBox
            {
                Size = new Size(50, 50),
                BackColor = Color.Transparent,
                Location = new Point(100, 100)
            };

            ball.Paint += DrawBall;

            infoLabel = new Label
            {
                Text = "Скорость: 0 | Позиция: (0, 0) | Отскоков: 0",
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            FlowLayoutPanel controlPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(236, 240, 241)
            };

            Label speedLabel = new Label { Text = "Скорость:", AutoSize = true };
            speedTrackBar = new TrackBar
            {
                Minimum = 1,
                Maximum = 30,
                Value = 10,
                Width = 120,
                TickFrequency = 5
            };
            speedTrackBar.ValueChanged += SpeedTrackBar_ValueChanged;

            Label gravityLabel = new Label { Text = "Гравитация:", AutoSize = true };
            gravityTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 20,
                Value = 5,
                Width = 120,
                TickFrequency = 2
            };
            gravityTrackBar.ValueChanged += (s, e) => gravity = gravityTrackBar.Value / 10f;

            btnStartStop = new Button
            {
                Text = "⏸ Пауза",
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat
            };

            btnReset = new Button
            {
                Text = "🔄 Сброс",
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                FlatStyle = FlatStyle.Flat
            };

            chkShowTrail = new CheckBox
            {
                Text = "След",
                AutoSize = true,
                Checked = false
            };

            btnStartStop.Click += BtnStartStop_Click;
            btnReset.Click += BtnReset_Click;

            controlPanel.Controls.AddRange(new Control[] {
                speedLabel, speedTrackBar,
                gravityLabel, gravityTrackBar,
                btnStartStop, btnReset, chkShowTrail
            });

            gameArea.Controls.Add(ball);
            this.Controls.Add(gameArea);
            this.Controls.Add(infoLabel);
            this.Controls.Add(controlPanel);
        }

        private void DrawBall(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 49, 49),
                Color.Crimson,
                Color.DarkRed,
                LinearGradientMode.ForwardDiagonal))
            {
                e.Graphics.FillEllipse(brush, 0, 0, 49, 49);
            }

            using (var highlight = new SolidBrush(Color.FromArgb(150, 255, 255, 255)))
            {
                e.Graphics.FillEllipse(highlight, 10, 10, 15, 15);
            }

            using (var eyeWhite = new SolidBrush(Color.White))
            using (var eyeBlack = new SolidBrush(Color.Black))
            {
                e.Graphics.FillEllipse(eyeWhite, 30, 15, 10, 10);
                e.Graphics.FillEllipse(eyeBlack, 33, 18, 4, 4);
                e.Graphics.FillEllipse(eyeWhite, 10, 15, 10, 10);
                e.Graphics.FillEllipse(eyeBlack, 13, 18, 4, 4);
            }

            using (var pen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawArc(pen, 15, 25, 20, 15, 0, 180);
            }
        }

        private void SetupTimers()
        {
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            trailTimer = new System.Windows.Forms.Timer();
            trailTimer.Interval = 30;
            trailTimer.Tick += TrailTimer_Tick;
        }

        private void SpeedTrackBar_ValueChanged(object sender, EventArgs e)
        {
            float factor = speedTrackBar.Value / 10f;
            float currentSpeed = (float)Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
            if (currentSpeed > 0)
            {
                velocityX = (velocityX / currentSpeed) * factor * 5;
                velocityY = (velocityY / currentSpeed) * factor * 5;
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!isRunning) return;

            velocityY += gravity;

            float newX = ball.Left + velocityX;
            float newY = ball.Top + velocityY;

            bool collided = false;

            if (newX <= 0)
            {
                newX = 0;
                velocityX = Math.Abs(velocityX) * bounceDamping;
                collided = true;
            }
            else if (newX >= gameArea.Width - ball.Width)
            {
                newX = gameArea.Width - ball.Width;
                velocityX = -Math.Abs(velocityX) * bounceDamping;
                collided = true;
            }

            if (newY <= 0)
            {
                newY = 0;
                velocityY = Math.Abs(velocityY) * bounceDamping;
                collided = true;
            }
            else if (newY >= gameArea.Height - ball.Height - 10)
            {
                newY = gameArea.Height - ball.Height - 10;

                if (Math.Abs(velocityY) < 1)
                {
                    velocityY = 0;
                    velocityX *= 0.98f;
                }
                else
                {
                    velocityY = -Math.Abs(velocityY) * bounceDamping;
                }
                collided = true;
            }

            if (collided)
            {
                collisionCount++;
                ball.Invalidate();
            }

            ball.Location = new Point((int)newX, (int)newY);

            float speed = (float)Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
            infoLabel.Text = $"Скорость: {speed:F1} | Позиция: ({ball.Left}, {ball.Top}) | Отскоков: {collisionCount} | Гравитация: {gravity:F1}";
        }

        private void TrailTimer_Tick(object sender, EventArgs e)
        {
            if (chkShowTrail.Checked && isRunning)
            {
                Panel trail = new Panel
                {
                    Size = new Size(ball.Width - 10, ball.Height - 10),
                    Location = new Point(ball.Left + 5, ball.Top + 5),
                    BackColor = Color.FromArgb(100, 255, 100, 100)
                };

                gameArea.Controls.Add(trail);
                trail.BringToFront();

                System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
                fadeTimer.Interval = 100;
                int opacity = 100;
                fadeTimer.Tick += (s, args) =>
                {
                    opacity -= 20;
                    if (opacity <= 0)
                    {
                        fadeTimer.Stop();
                        gameArea.Controls.Remove(trail);
                        trail.Dispose();
                        fadeTimer.Dispose();
                    }
                    else
                    {
                        trail.BackColor = Color.FromArgb(opacity, 255, 100, 100);
                    }
                };
                fadeTimer.Start();

                if (gameArea.Controls.Count > 30)
                {
                    foreach (Control ctrl in gameArea.Controls)
                    {
                        if (ctrl != ball)
                        {
                            gameArea.Controls.Remove(ctrl);
                            ctrl.Dispose();
                            break;
                        }
                    }
                }
            }
        }

        private void BtnStartStop_Click(object sender, EventArgs e)
        {
            isRunning = !isRunning;
            btnStartStop.Text = isRunning ? "⏸ Пауза" : "▶ Старт";
            btnStartStop.BackColor = isRunning ? Color.FromArgb(46, 204, 113) : Color.FromArgb(241, 196, 15);

            if (isRunning && chkShowTrail.Checked)
                trailTimer.Start();
            else
                trailTimer.Stop();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            ball.Location = new Point(100, 100);
            velocityX = 5f;
            velocityY = 5f;
            collisionCount = 0;

            foreach (Control ctrl in gameArea.Controls)
            {
                if (ctrl != ball)
                    ctrl.Dispose();
            }

            while (gameArea.Controls.Count > 1)
            {
                gameArea.Controls.RemoveAt(0);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            trailTimer?.Stop();
            trailTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}