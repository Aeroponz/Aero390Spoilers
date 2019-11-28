using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QuantumConcepts.Common.Forms.UI.Controls
{
    public class NewProgressBar : VerticalProgressBar
    {
        public NewProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // None... Helps control the flicker.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const int inset = 2; // A single inset value to control teh sizing of the inner rect.

            using (Image offscreenImage = new Bitmap(this.Width, this.Height))
            {
                using (Graphics offscreen = Graphics.FromImage(offscreenImage))
                {
                    Color color1 = this.BackColor;
                    Color color2 = this.ForeColor;
                    Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

                    if (ProgressBarRenderer.IsSupported)
                        ProgressBarRenderer.DrawVerticalBar(offscreen, rect);

                    rect.Inflate(new Size(-inset, -inset)); // Deflate inner rect.
                    rect.Height = (int)(rect.Height * ((double)this.Value / this.Maximum));

                    if (rect.Height == 0) // Can't draw rec with width of 0.
                    {
                        rect.Height = 1;
                        color1 = Color.WhiteSmoke;
                        color2 = Color.WhiteSmoke;
                    }

                    LinearGradientBrush brush = new LinearGradientBrush(rect, color1, color2, LinearGradientMode.Vertical);
                    offscreen.FillRectangle(brush, inset, inset, rect.Width, rect.Height);

                    e.Graphics.DrawImage(offscreenImage, 0, 0);
                    offscreenImage.Dispose();
                }
            }
        }
    }

    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }
}