using System.Drawing.Drawing2D;

namespace FlappyBirdApp.Models;

/// <summary>
/// Oyuncu tarafından kontrol edilen kuş nesnesini, hız/yerçekimi fiziğini ve çizimini yönetir.
/// </summary>
public sealed class Bird
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public int Width { get; } = 42;
    public int Height { get; } = 32;
    public float VelocityY { get; private set; }
    public float RotationAngle { get; private set; }
    private float _wingPhase;

    public Bird(float startX, float startY)
    {
        Reset(startX, startY);
    }

    public void Reset(float startX, float startY)
    {
        X = startX;
        Y = startY;
        VelocityY = 0f;
        RotationAngle = 0f;
        _wingPhase = 0f;
    }

    public void Flap(float jumpImpulse)
    {
        VelocityY = jumpImpulse;
        RotationAngle = -24f;
        _wingPhase += 1.4f;
    }

    public void Update(float gravity)
    {
        VelocityY += gravity;
        if (VelocityY > 11.5f)
        {
            VelocityY = 11.5f;
        }

        Y += VelocityY;
        _wingPhase += 0.35f;

        if (VelocityY < 0)
        {
            RotationAngle = Math.Max(-26f, VelocityY * 3.0f);
        }
        else
        {
            RotationAngle = Math.Min(65f, VelocityY * 5.2f);
        }
    }

    public RectangleF GetHitBox()
    {
        // Görsel kenarlardan birkaç piksel içeride adil çarpışma kutusu
        return new RectangleF(X + 5, Y + 4, Width - 10, Height - 8);
    }

    public void Draw(Graphics g)
    {
        GraphicsState state = g.Save();
        float centerX = X + Width / 2f;
        float centerY = Y + Height / 2f;

        g.TranslateTransform(centerX, centerY);
        g.RotateTransform(RotationAngle);
        g.TranslateTransform(-Width / 2f, -Height / 2f);

        // Gövde gölgesi ve altın-turuncu gradyan gövde
        RectangleF bodyRect = new(0, 2, Width - 6, Height - 4);
        using (LinearGradientBrush bodyBrush = new(
            bodyRect,
            Color.FromArgb(255, 224, 66),
            Color.FromArgb(245, 130, 32),
            LinearGradientMode.Vertical))
        {
            g.FillEllipse(bodyBrush, bodyRect);
        }

        using (Pen outlinePen = new(Color.FromArgb(85, 40, 10), 2f))
        {
            g.DrawEllipse(outlinePen, bodyRect);
        }

        // Kanat animasyonu
        float wingOffsetY = (float)Math.Sin(_wingPhase) * 4.5f;
        RectangleF wingRect = new(7, 10 + wingOffsetY, 17, 11);
        using (SolidBrush wingBrush = new(Color.FromArgb(255, 245, 196)))
        using (Pen wingPen = new(Color.FromArgb(140, 75, 20), 1.6f))
        {
            g.FillEllipse(wingBrush, wingRect);
            g.DrawEllipse(wingPen, wingRect);
        }

        // Göz
        RectangleF eyeWhite = new(Width - 18, 5, 11, 11);
        RectangleF pupil = new(Width - 13, 8, 5, 5);
        g.FillEllipse(Brushes.White, eyeWhite);
        g.DrawEllipse(Pens.Black, eyeWhite);
        g.FillEllipse(Brushes.Black, pupil);
        g.FillEllipse(Brushes.White, Width - 12, 8.5f, 2f, 2f);

        // Gaga
        PointF[] beakPoints =
        [
            new PointF(Width - 8, 13),
            new PointF(Width + 5, 17),
            new PointF(Width - 8, 22)
        ];
        using (SolidBrush beakBrush = new(Color.FromArgb(239, 68, 68)))
        using (Pen beakPen = new(Color.FromArgb(127, 29, 29), 1.5f))
        {
            g.FillPolygon(beakBrush, beakPoints);
            g.DrawPolygon(beakPen, beakPoints);
        }

        g.Restore(state);
    }
}
