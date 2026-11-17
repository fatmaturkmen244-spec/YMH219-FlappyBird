using System.Drawing.Drawing2D;
using System.Media;
using FlappyBirdApp.Models;
using FlappyBirdApp.Services;

namespace FlappyBirdApp;

public sealed class MainForm : Form
{
    private const int CanvasWidth = 600;
    private const int CanvasHeight = 620;
    private const int GroundHeight = 76;
    private const int PlayableHeight = CanvasHeight - GroundHeight;
    private const float PipeSpacing = 235f;

    private readonly HighScoreService _highScoreService;
    private readonly GameState _gameState;
    private readonly Bird _bird;
    private readonly List<PipePair> _pipes = [];
    private readonly Random _random = new();
    private readonly System.Windows.Forms.Timer _gameTimer;

    private readonly DoubleBufferedPanel _canvasPanel;
    private readonly Label _lblScoreValue;
    private readonly Label _lblHighScoreValue;
    private readonly Label _lblLevelValue;
    private readonly Label _lblSpeedValue;
    private readonly Label _lblStatusBadge;
    private readonly ComboBox _cmbDifficulty;
    private readonly CheckBox _chkSound;
    private readonly Button _btnStartRestart;
    private readonly Button _btnPause;

    private float _groundOffset;
    private float _cloudOffset;

    public MainForm()
    {
        _highScoreService = new HighScoreService();
        int savedHigh = _highScoreService.LoadHighScore();
        _gameState = new GameState(savedHigh, DifficultyLevel.Normal);
        _bird = new Bird(115f, PlayableHeight / 2f - 20f);

        Text = "YMH219 — Flappy Bird Arcade (Nesne Tabanlı Programlama)";
        ClientSize = new Size(920, CanvasHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(24, 24, 37);
        KeyPreview = true;

        // Oyun Çizim Alanı (Sol Panel)
        _canvasPanel = new DoubleBufferedPanel
        {
            Location = new Point(0, 0),
            Size = new Size(CanvasWidth, CanvasHeight)
        };
        _canvasPanel.Paint += CanvasPanel_Paint;
        _canvasPanel.MouseDown += (_, _) => HandleFlapAction();
        Controls.Add(_canvasPanel);

        // Sağ Kontrol ve Gösterge Paneli (Arcade HUD)
        Panel sidebar = new()
        {
            Location = new Point(CanvasWidth, 0),
            Size = new Size(320, CanvasHeight),
            BackColor = Color.FromArgb(30, 27, 46)
        };
        Controls.Add(sidebar);

        Label lblHeader = new()
        {
            Text = "FLAPPY ARCADE",
            Font = new Font("Segoe UI", 17f, FontStyle.Bold),
            ForeColor = Color.FromArgb(251, 191, 36),
            Location = new Point(22, 18),
            AutoSize = true
        };
        sidebar.Controls.Add(lblHeader);

        Label lblSubHeader = new()
        {
            Text = "YMH 219 · Nesne Tabanlı Oyun Motoru",
            Font = new Font("Segoe UI", 9.2f, FontStyle.Regular),
            ForeColor = Color.FromArgb(167, 139, 250),
            Location = new Point(24, 50),
            AutoSize = true
        };
        sidebar.Controls.Add(lblSubHeader);

        // Durum Rozeti
        _lblStatusBadge = new Label
        {
            Text = _gameState.StatusMessage,
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(254, 243, 199),
            BackColor = Color.FromArgb(67, 56, 202),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(22, 80),
            Size = new Size(276, 34)
        };
        sidebar.Controls.Add(_lblStatusBadge);

        // Skor ve İstatistik Kartları (2x2 Grid)
        _lblScoreValue = CreateStatCard(sidebar, "ANLIK SKOR", "0", 22, 126, Color.FromArgb(52, 211, 153));
        _lblHighScoreValue = CreateStatCard(sidebar, "EN YÜKSEK SKOR", savedHigh.ToString(), 164, 126, Color.FromArgb(251, 191, 36));
        _lblLevelValue = CreateStatCard(sidebar, "SEVİYE", "1", 22, 214, Color.FromArgb(96, 165, 250));
        _lblSpeedValue = CreateStatCard(sidebar, "ENGEL HIZI", $"{_gameState.CurrentSpeed:0.0}x", 164, 214, Color.FromArgb(244, 114, 182));

        // Zorluk Seviyesi Seçimi
        Label lblDiff = new()
        {
            Text = "Zorluk Seviyesi:",
            Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(226, 232, 240),
            Location = new Point(22, 308),
            AutoSize = true
        };
        sidebar.Controls.Add(lblDiff);

        _cmbDifficulty = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10f),
            BackColor = Color.FromArgb(49, 46, 75),
            ForeColor = Color.White,
            Location = new Point(22, 332),
            Size = new Size(276, 30)
        };
        _cmbDifficulty.Items.AddRange(["Kolay (Geniş Aralık, Düşük Hız)", "Normal (Dengeli Standart Mod)", "Zor (Dar Aralık, Yüksek Hız)"]);
        _cmbDifficulty.SelectedIndex = 1;
        _cmbDifficulty.SelectedIndexChanged += (_, _) =>
        {
            DifficultyLevel selected = (DifficultyLevel)_cmbDifficulty.SelectedIndex;
            _gameState.SetDifficulty(selected);
            UpdateHudLabels();
            _canvasPanel.Focus();
        };
        sidebar.Controls.Add(_cmbDifficulty);

        _chkSound = new CheckBox
        {
            Text = "Ses Efektlerini Etkinleştir",
            Checked = true,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(24, 372),
            AutoSize = true
        };
        sidebar.Controls.Add(_chkSound);

        // Aksiyon Butonları
        _btnStartRestart = new Button
        {
            Text = "▶  YENİ OYUN / BAŞLAT",
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(24, 24, 37),
            BackColor = Color.FromArgb(251, 191, 36),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(22, 408),
            Size = new Size(276, 42),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _btnStartRestart.FlatAppearance.BorderSize = 0;
        _btnStartRestart.Click += (_, _) =>
        {
            StartNewGame();
            _canvasPanel.Focus();
        };
        sidebar.Controls.Add(_btnStartRestart);

        _btnPause = new Button
        {
            Text = "⏸  DURAKLAT / DEVAM ET (P)",
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(71, 85, 105),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(22, 458),
            Size = new Size(276, 36),
            Cursor = Cursors.Hand,
            TabStop = false
        };
        _btnPause.FlatAppearance.BorderSize = 0;
        _btnPause.Click += (_, _) =>
        {
            TogglePauseGame();
            _canvasPanel.Focus();
        };
        sidebar.Controls.Add(_btnPause);

        // Kontroller Bilgi Kutusu
        Panel helpCard = new()
        {
            Location = new Point(22, 506),
            Size = new Size(276, 96),
            BackColor = Color.FromArgb(39, 35, 58)
        };
        sidebar.Controls.Add(helpCard);

        Label lblHelpTitle = new()
        {
            Text = "KLAVYE VE FARE KONTROLLERİ",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(251, 191, 36),
            Location = new Point(12, 8),
            AutoSize = true
        };
        helpCard.Controls.Add(lblHelpTitle);

        Label lblHelpBody = new()
        {
            Text = "• SPACE / YUKARI OK / Sol Tık : Zıpla\n• R Tuşu : Hızlı Yeniden Başlat\n• P Tuşu : Oyunu Duraklat / Devam Ettir",
            Font = new Font("Segoe UI", 8.8f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(12, 28),
            AutoSize = true
        };
        helpCard.Controls.Add(lblHelpBody);

        InitializePipes();

        _gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 16 // ~60 FPS
        };
        _gameTimer.Tick += GameTimer_Tick;

        KeyDown += MainForm_KeyDown;
    }

    private static Label CreateStatCard(Control parent, string title, string initialValue, int x, int y, Color accentColor)
    {
        Panel card = new()
        {
            Location = new Point(x, y),
            Size = new Size(134, 78),
            BackColor = Color.FromArgb(41, 37, 62)
        };

        Panel topBar = new()
        {
            Dock = DockStyle.Top,
            Height = 4,
            BackColor = accentColor
        };
        card.Controls.Add(topBar);

        Label lblTitle = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(148, 163, 184),
            Location = new Point(10, 10),
            AutoSize = true
        };
        card.Controls.Add(lblTitle);

        Label lblVal = new()
        {
            Text = initialValue,
            Font = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = accentColor,
            Location = new Point(8, 30),
            Size = new Size(118, 40),
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(lblVal);

        parent.Controls.Add(card);
        return lblVal;
    }

    private void InitializePipes()
    {
        _pipes.Clear();
        int gapHeight = _gameState.Difficulty.PipeGapHeight;
        for (int i = 0; i < 3; i++)
        {
            float startX = CanvasWidth + 60 + i * PipeSpacing;
            int gapY = _random.Next(70, PlayableHeight - gapHeight - 70);
            _pipes.Add(new PipePair(startX, gapY, gapHeight));
        }
    }

    public void StartNewGame()
    {
        _gameState.SetDifficulty((DifficultyLevel)_cmbDifficulty.SelectedIndex);
        _gameState.StartNewGame();
        _bird.Reset(115f, PlayableHeight / 2f - 20f);
        InitializePipes();
        _cmbDifficulty.Enabled = false;
        _gameTimer.Start();
        UpdateHudLabels();
        _canvasPanel.Invalidate();
    }

    private void TogglePauseGame()
    {
        if (!_gameState.IsRunning || _gameState.IsGameOver)
        {
            return;
        }

        _gameState.TogglePause();
        if (_gameState.IsPaused)
        {
            _gameTimer.Stop();
        }
        else
        {
            _gameTimer.Start();
        }

        UpdateHudLabels();
        _canvasPanel.Invalidate();
    }

    private void HandleFlapAction()
    {
        if (!_gameState.IsRunning && !_gameState.IsGameOver)
        {
            StartNewGame();
        }

        if (_gameState.IsRunning && !_gameState.IsPaused)
        {
            _bird.Flap(_gameState.Difficulty.JumpImpulse);
        }
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Space or Keys.Up or Keys.W)
        {
            e.SuppressKeyPress = true;
            HandleFlapAction();
        }
        else if (e.KeyCode == Keys.R)
        {
            e.SuppressKeyPress = true;
            StartNewGame();
        }
        else if (e.KeyCode == Keys.P)
        {
            e.SuppressKeyPress = true;
            TogglePauseGame();
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData is Keys.Space or Keys.Up)
        {
            HandleFlapAction();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (!_gameState.IsRunning || _gameState.IsPaused)
        {
            return;
        }

        // Kuş fiziğini güncelle
        _bird.Update(_gameState.Difficulty.Gravity);

        // Arka plan ve zemin kaydırma
        _groundOffset = (_groundOffset + _gameState.CurrentSpeed) % 36f;
        _cloudOffset = (_cloudOffset + 0.6f) % CanvasWidth;

        RectangleF birdHitBox = _bird.GetHitBox();

        // Tavan ve zemin çarpışma kontrolü
        if (_bird.Y < 0)
        {
            EndGame("Üst sınıra çarptınız!");
            return;
        }

        if (birdHitBox.Bottom >= PlayableHeight)
        {
            EndGame("Zemine çarptınız!");
            return;
        }

        // Engelleri hareket ettir ve çarpışma / skor kontrolü yap
        float maxPipeX = _pipes.Max(p => p.X);
        foreach (PipePair pipe in _pipes)
        {
            pipe.Update(_gameState.CurrentSpeed);

            if (pipe.CheckCollision(birdHitBox, PlayableHeight))
            {
                EndGame("Engele çarptınız!");
                return;
            }

            if (!pipe.IsPassed && pipe.X + pipe.Width < _bird.X)
            {
                pipe.IsPassed = true;
                bool isNewRecord = _gameState.IncrementScore();
                if (isNewRecord)
                {
                    _highScoreService.SaveHighScore(_gameState.HighScore);
                }

                if (_chkSound.Checked)
                {
                    SystemSounds.Asterisk.Play();
                }

                UpdateHudLabels();
            }

            // Ekran dışına çıkan engeli sağ tarafa yeniden konumlandır
            if (pipe.X + pipe.Width < -10)
            {
                int gapHeight = _gameState.Difficulty.PipeGapHeight;
                int newGapY = _random.Next(65, PlayableHeight - gapHeight - 65);
                float nextX = Math.Max(CanvasWidth + 30, maxPipeX + PipeSpacing);
                pipe.Reposition(nextX, newGapY, gapHeight);
                maxPipeX = nextX;
            }
        }

        _canvasPanel.Invalidate();
    }

    private void EndGame(string reason)
    {
        _gameTimer.Stop();
        _gameState.TriggerGameOver(reason);
        _highScoreService.SaveHighScore(_gameState.HighScore);
        _cmbDifficulty.Enabled = true;

        if (_chkSound.Checked)
        {
            SystemSounds.Hand.Play();
        }

        UpdateHudLabels();
        _canvasPanel.Invalidate();
    }

    private void UpdateHudLabels()
    {
        _lblScoreValue.Text = _gameState.Score.ToString();
        _lblHighScoreValue.Text = _gameState.HighScore.ToString();
        _lblLevelValue.Text = _gameState.Level.ToString();
        _lblSpeedValue.Text = $"{_gameState.CurrentSpeed:0.0}x";
        _lblStatusBadge.Text = _gameState.StatusMessage;

        if (_gameState.IsGameOver)
        {
            _lblStatusBadge.BackColor = Color.FromArgb(185, 28, 28);
        }
        else if (_gameState.IsPaused)
        {
            _lblStatusBadge.BackColor = Color.FromArgb(180, 83, 9);
        }
        else if (_gameState.IsRunning)
        {
            _lblStatusBadge.BackColor = Color.FromArgb(5, 150, 105);
        }
        else
        {
            _lblStatusBadge.BackColor = Color.FromArgb(67, 56, 202);
        }
    }

    private void CanvasPanel_Paint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 1. Gökyüzü Gradyanı (Sunset Arcade Teması)
        Rectangle skyRect = new(0, 0, CanvasWidth, PlayableHeight);
        using (LinearGradientBrush skyBrush = new(
            skyRect,
            Color.FromArgb(30, 64, 175),
            Color.FromArgb(251, 146, 60),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(skyBrush, skyRect);
        }

        // 2. Güneş / Atmosfer Efekti ve Bulutlar
        using (SolidBrush sunBrush = new(Color.FromArgb(75, 254, 240, 138)))
        {
            g.FillEllipse(sunBrush, CanvasWidth - 180, PlayableHeight - 210, 130, 130);
        }

        DrawClouds(g);
        DrawCitySilhouette(g);

        // 3. Borular (Engeller)
        foreach (PipePair pipe in _pipes)
        {
            pipe.Draw(g, PlayableHeight);
        }

        // 4. Kuş
        _bird.Draw(g);

        // 5. Hareketli Zemin
        DrawGround(g);

        // 6. Üst Oyun İçi Skor Göstergesi
        DrawInGameScoreBadge(g);

        // 7. Başlangıç veya Game Over Katmanı (Overlay)
        if (_gameState.IsGameOver)
        {
            DrawOverlayCard(
                g,
                "GAME OVER",
                _gameState.StatusMessage,
                $"Skorunuz: {_gameState.Score}   |   En Yüksek Skor: {_gameState.HighScore}",
                "Tekrar oynamak için 'R' tuşuna veya 'Yeni Oyun' butonuna basın.",
                Color.FromArgb(239, 68, 68));
        }
        else if (!_gameState.IsRunning)
        {
            DrawOverlayCard(
                g,
                "FLAPPY BIRD ARCADE",
                "Nesne Tabanlı Masaüstü Oyunu",
                "Kuşu uçurmak için SPACE, YUKARI OK veya Fare Sol Tık kullanın.",
                "Başlamak için SPACE tuşuna veya 'Yeni Oyun' butonuna basın.",
                Color.FromArgb(251, 191, 36));
        }
        else if (_gameState.IsPaused)
        {
            DrawOverlayCard(
                g,
                "OYUN DURAKLATILDI",
                $"Anlık Skor: {_gameState.Score}  ·  Seviye: {_gameState.Level}",
                "Oyuna kaldığınız yerden devam edebilirsiniz.",
                "Devam etmek için 'P' tuşuna veya 'Duraklat / Devam Et' butonuna basın.",
                Color.FromArgb(56, 189, 248));
        }
    }

    private void DrawClouds(Graphics g)
    {
        using SolidBrush cloudBrush = new(Color.FromArgb(110, 255, 255, 255));
        float[] basePositions = [60f, 250f, 440f];
        float[] heights = [65f, 115f, 80f];

        for (int i = 0; i < basePositions.Length; i++)
        {
            float cx = (basePositions[i] - _cloudOffset + CanvasWidth) % CanvasWidth;
            float cy = heights[i];
            g.FillEllipse(cloudBrush, cx, cy, 70, 28);
            g.FillEllipse(cloudBrush, cx + 18, cy - 12, 45, 30);
            g.FillEllipse(cloudBrush, cx + 42, cy - 4, 50, 26);
        }
    }

    private static void DrawCitySilhouette(Graphics g)
    {
        using SolidBrush cityBrush = new(Color.FromArgb(55, 15, 23, 42));
        int[] widths = [45, 60, 38, 70, 52, 64, 48, 58, 66, 50, 62];
        int[] heights = [75, 110, 60, 130, 85, 105, 70, 120, 90, 65, 100];
        int x = 0;
        for (int i = 0; i < widths.Length; i++)
        {
            g.FillRectangle(cityBrush, x, PlayableHeight - heights[i], widths[i] - 2, heights[i]);
            x += widths[i];
        }
    }

    private void DrawGround(Graphics g)
    {
        // Çim şeridi
        Rectangle grassRect = new(0, PlayableHeight, CanvasWidth, 18);
        using (LinearGradientBrush grassBrush = new(
            grassRect,
            Color.FromArgb(132, 204, 22),
            Color.FromArgb(77, 124, 15),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(grassBrush, grassRect);
        }

        // Diyagonal hareket şeritleri
        using (SolidBrush stripeBrush = new(Color.FromArgb(90, 255, 255, 255)))
        {
            for (float x = -36f + (36f - _groundOffset); x < CanvasWidth + 36f; x += 36f)
            {
                PointF[] pts =
                [
                    new PointF(x, PlayableHeight),
                    new PointF(x + 16, PlayableHeight),
                    new PointF(x + 6, PlayableHeight + 18),
                    new PointF(x - 10, PlayableHeight + 18)
                ];
                g.FillPolygon(stripeBrush, pts);
            }
        }

        // Toprak tabanı
        Rectangle dirtRect = new(0, PlayableHeight + 18, CanvasWidth, GroundHeight - 18);
        using (LinearGradientBrush dirtBrush = new(
            dirtRect,
            Color.FromArgb(217, 119, 6),
            Color.FromArgb(146, 64, 14),
            LinearGradientMode.Vertical))
        {
            g.FillRectangle(dirtBrush, dirtRect);
        }

        using Pen topLinePen = new(Color.FromArgb(54, 83, 20), 2f);
        g.DrawLine(topLinePen, 0, PlayableHeight, CanvasWidth, PlayableHeight);
    }

    private void DrawInGameScoreBadge(Graphics g)
    {
        string text = $"SKOR: {_gameState.Score}   ·   SEVİYE {_gameState.Level}";
        Rectangle badgeRect = new(18, 16, 215, 38);
        using SolidBrush bgBrush = new(Color.FromArgb(175, 15, 23, 42));
        using Pen borderPen = new(Color.FromArgb(251, 191, 36), 1.8f);
        g.FillRectangle(bgBrush, badgeRect);
        g.DrawRectangle(borderPen, badgeRect);

        using Font font = new("Segoe UI", 11f, FontStyle.Bold);
        TextRenderer.DrawText(g, text, font, badgeRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void DrawOverlayCard(
        Graphics g,
        string title,
        string subtitle,
        string statsLine,
        string hintLine,
        Color accentColor)
    {
        using SolidBrush dimBrush = new(Color.FromArgb(125, 10, 12, 24));
        g.FillRectangle(dimBrush, 0, 0, CanvasWidth, PlayableHeight);

        Rectangle cardRect = new(42, 155, CanvasWidth - 84, 225);
        using SolidBrush cardBrush = new(Color.FromArgb(238, 24, 24, 37));
        using Pen cardBorder = new(accentColor, 2.5f);
        g.FillRectangle(cardBrush, cardRect);
        g.DrawRectangle(cardBorder, cardRect);

        using Font titleFont = new("Segoe UI", 21f, FontStyle.Bold);
        using Font subFont = new("Segoe UI Semibold", 11.5f, FontStyle.Bold);
        using Font statsFont = new("Segoe UI", 10.5f, FontStyle.Regular);
        using Font hintFont = new("Segoe UI", 9.2f, FontStyle.Italic);

        TextRenderer.DrawText(
            g,
            title,
            titleFont,
            new Rectangle(cardRect.X, cardRect.Y + 18, cardRect.Width, 44),
            accentColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            subtitle,
            subFont,
            new Rectangle(cardRect.X + 12, cardRect.Y + 68, cardRect.Width - 24, 32),
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            statsLine,
            statsFont,
            new Rectangle(cardRect.X + 12, cardRect.Y + 112, cardRect.Width - 24, 34),
            Color.FromArgb(253, 230, 138),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(
            g,
            hintLine,
            hintFont,
            new Rectangle(cardRect.X + 12, cardRect.Y + 162, cardRect.Width - 24, 42),
            Color.FromArgb(203, 213, 225),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
    }

    public void CaptureScreenshots(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        Show();
        Application.DoEvents();

        // Ekran Görüntüsü 1: Aktif Oyun İçi (Kuş engellerin arasından geçerken, Skor: 12, Seviye: 3)
        _pipes.Clear();
        _pipes.Add(new PipePair(185f, 150, 142));
        _pipes.Add(new PipePair(410f, 220, 142));
        _pipes.Add(new PipePair(635f, 120, 142));
        _bird.Reset(118f, 212f);
        _bird.Flap(-6.2f);
        _gameState.SetScenarioForPreview(score: 12, highScore: 18, level: 3, isRunning: true, isGameOver: false, message: "Oyun Devam Ediyor");
        UpdateHudLabels();
        _canvasPanel.Invalidate();
        Refresh();
        Application.DoEvents();

        using (Bitmap bmp1 = new(Width, Height))
        {
            DrawToBitmap(bmp1, new Rectangle(0, 0, Width, Height));
            bmp1.Save(Path.Combine(outputDir, "gameplay.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        // Ekran Görüntüsü 2: Çarpışma ve Game Over Ekranı (Yeni Rekor: 21)
        _pipes.Clear();
        _pipes.Add(new PipePair(130f, 165, 142));
        _pipes.Add(new PipePair(365f, 110, 142));
        _bird.Reset(118f, 325f);
        _bird.Update(6.5f);
        _gameState.SetScenarioForPreview(score: 21, highScore: 21, level: 5, isRunning: false, isGameOver: true, message: "Oyun Bitti: Engele çarptınız!");
        UpdateHudLabels();
        _canvasPanel.Invalidate();
        Refresh();
        Application.DoEvents();

        using (Bitmap bmp2 = new(Width, Height))
        {
            DrawToBitmap(bmp2, new Rectangle(0, 0, Width, Height));
            bmp2.Save(Path.Combine(outputDir, "gameover.png"), System.Drawing.Imaging.ImageFormat.Png);
        }

        Close();
    }
}

public sealed class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
    }
}
