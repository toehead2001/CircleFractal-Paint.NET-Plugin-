#region UICode
int Amount1 = 8; // [1,10] Angles
int Amount2 = 6; // [2,11] Vertices , 11 = Circle
bool Amount3 = true; // [0,1] Fill
ColorBgra Amount4 = ColorBgra.FromBgr(200,120,0); // Fill Color
bool Amount5 = true; // [0,1] Outline
int Amount6 = 1; // [1,100] Brush Size
ColorBgra Amount7 = ColorBgra.FromBgr(120,0,0); // Outline Color
int Amount8 = 100; // [0,200] Zoom
bool Amount9 = true; // [0,1] rotation
bool Amount10 = false; // [0,1] anti-aliasing
#endregion

int lengthdirX(int length, double dir)
{
    return (int)(Math.Cos(dir / 180 * Math.PI) * length);
}
int lengthdirY(int length, double dir)
{
    return (int)(Math.Sin(dir / 180 * Math.PI) * length);
}

void DrawShape(Graphics g, Brush b, Pen p, int x, int y, int r, bool fill, bool outline, int verts, int rot)
{

    if (verts > 10)
    {
        Rectangle rect1 = new Rectangle(x - r, y - r, r * 2, r * 2);
        if (fill) g.FillPie(b, rect1, 0.0f, 360.0f);
        if (outline) g.DrawArc(p, rect1, 0.0f, 360.0f);
    }
    else
    {
        if (fill)
        {

            System.Drawing.PointF[] shapePoints = new System.Drawing.PointF[verts];
            int length = (int)(2 * r * Math.Sin(Math.PI / verts)),
                originalAngle = (180 - (verts - 2) * 180 / verts) / 2,
            angle = rot + originalAngle;
            shapePoints[0] = new System.Drawing.PointF((float)(x - lengthdirX(r, 270 - rot)), (float)(y + lengthdirY(r, 270 - rot)));
            for (int i = 1; i < verts; i++)
            {
                float xx = shapePoints[i - 1].X + lengthdirX(length, angle),
                yy = shapePoints[i - 1].Y + lengthdirY(length, angle);
                shapePoints[i] = new System.Drawing.PointF((float)xx, (float)yy);
                angle = (angle + originalAngle * 2) % 360;
            }
            System.Drawing.Drawing2D.FillMode newFillMode = System.Drawing.Drawing2D.FillMode.Winding;
            g.FillPolygon(b, shapePoints, newFillMode);
        }

        if (outline)
        {
            System.Drawing.Point[] shapePoints = new System.Drawing.Point[verts];
            int length = (int)(2 * r * Math.Sin(Math.PI / verts)),
                originalAngle = (180 - (verts - 2) * 180 / verts) / 2,
            angle = rot + originalAngle;
            shapePoints[0] = new System.Drawing.Point(x - lengthdirX(r, 270 - rot), y + lengthdirY(r, 270 - rot));
            for (int i = 1; i < verts; i++)
            {
                int xx = shapePoints[i - 1].X + lengthdirX(length, angle),
                yy = shapePoints[i - 1].Y + lengthdirY(length, angle);
                shapePoints[i] = new System.Drawing.Point(xx, yy);
                angle = (angle + originalAngle * 2) % 360;
            }
            g.DrawPolygon(p, shapePoints);
        }

    }
}

void DrawShapes(Graphics g, Brush b, Pen p, double x, double y, double r, ColorBgra c, ColorBgra cF, bool fill, bool outline, int verts, int rotVal)
{
    if (r <= 1) return;

    DrawShape(g, b, p, (int)x, (int)y, (int)r, fill, outline, verts, rotVal);
    Pen newPen = new Pen(c);
    SolidBrush newBrush = new SolidBrush(cF);
    newPen.Width = Amount6;
    for (double Ang = 360; Ang >= 0; Ang -= Math.Ceiling((double)(360.00 / (Amount1))))
    {
        DrawShapes(g, newBrush, newPen, x + (2 * r) * Math.Cos(Ang / 180 * Math.PI),
        y + (2 * r) * Math.Sin(Ang / 180 * Math.PI), r / 3, c, cF, fill, outline, verts, (Amount9 ? (int)Ang : 0));
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{

    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    dst.CopySurface(src, rect.Location, rect);
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
    ColorBgra PrimaryColor = (ColorBgra)EnvironmentParameters.PrimaryColor;
    Graphics canvas = new RenderArgs(dst).Graphics;

    canvas.SmoothingMode = Amount10 ? System.Drawing.Drawing2D.SmoothingMode.AntiAlias : System.Drawing.Drawing2D.SmoothingMode.None;
    int BrushWidth = (int)EnvironmentParameters.BrushWidth;
    Pen myPen = new Pen(Amount7);
    SolidBrush myBrush = new SolidBrush(Amount4);
    myPen.Width = Amount6;
    double r = ((double)Amount8 / 100.00) * Math.Min(selection.Right - selection.Left, selection.Bottom - selection.Top) / 6;
    DrawShapes(canvas, myBrush, myPen, CenterX, CenterY, r, Amount7, Amount4, Amount3, Amount5, Amount2, 0);

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x, y];
            dst[x, y] = CurrentPixel;
        }
    }
}