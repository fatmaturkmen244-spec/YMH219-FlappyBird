using System.Drawing.Drawing2D;

namespace FlappyBirdApp.Models;

/// <summary>
/// Sağdan sola hareket eden alt-üst boru (engel) çiftini ve çarpışma kontrolünü temsil eder.
/// </summary>
public sealed class PipePair
{
    public float X { get; private set; }
    public int GapY { get; private set; }
    public int GapHeight { get; private set; }
    public int Width { get; } = 68;
    public bool IsPassed { get; set; }

    public PipePair(float initialX, int gapY, int gapHeight)
    {
        X = initialX;
        GapY = gapY;
        GapHeight = gapHeight;
        IsPassed = false;
    }

    public void Update(float speed)
    {
        X -= speed;
    }

    public void Reposition(float newX, int newGapY, int newGapHeight)
    {
        X = newX;
        GapY = newGapY;
        GapHeight = newGapHeight;
        IsPassed = false;
    }

    public RectangleF GetTopPipeBounds()
    {
        return new RectangleF(X, 0, Width, GapY);
    }

    public RectangleF GetBottomPipeBounds(int playableHeight)
    {
        int bottomStart = GapY + GapHeight;
        return new RectangleF(X, bottomStart, Width, Math.Max(0, playableHeight - bottomStart));
    }

    public bool CheckCollision(RectangleF birdHitBox, int playableHeight)
    {
        return birdHitBox.IntersectsWith(GetTopPipeBounds()) ||
               birdHitBox.IntersectsWith(GetBottomPipeBounds(playableHeight));
    }

    public void Draw(Graphics g, int playableHeight)
    {
        RectangleF topBounds = GetTopPipeBounds();
        RectangleF bottomBounds = GetBottomPipeBounds(playableHeight);

        DrawSinglePipe(g, topBounds, isTopPipe: true);
        DrawSinglePipe(g, bottomBounds, isTopPipe: false);
    }

    private void DrawSinglePipe(Graphics g, RectangleF bounds, bool isTopPipe)
    {
        if (bounds.Height <= 0 || bounds.Width <= 0)
        {
            return;
        }

        using LinearGradientBrush pipeBrush = new(
            bounds,
            Color.FromArgb(52, 211, 153),
            Color.FromArgb(5, 150, 105),
            LinearGradientMode.Horizontal);

        using Pen borderPen = new(Color.FromArgb(6, 78, 59), 2.2f);
        g.FillRectangle(pipeBrush, bounds);
        g.DrawRectangle(borderPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);

        // Parlama şeridi
        using SolidBrush highlightBrush = new(Color.FromArgb(90, 255, 255, 255));
        g.FillRectangle(highlightBrush, bounds.X + 8, bounds.Y + 2, 9, Math.Max(0, bounds.Height - 4));

        // Boru ağzı (manşon)
        const int capHeight = 24;
        float capY = isTopPipe ? bounds.Bottom - capHeight : bounds.Y;
        RectangleF capRect = new(bounds.X - 5, capY, bounds.Width + 10, capHeight);

        using LinearGradientBrush capBrush = new(
            capRect,
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(4, 120, 87),
            LinearGradientMode.Horizontal);

        g.FillRectangle(capBrush, capRect);
        g.DrawRectangle(borderPen, capRect.X, capRect.Y, capRect.Width, capRect.Height);
    }
}
