/*****************************************************************************/
/* Project  : AvionicsInstrumentControlDemo                                  */
/* File     : AirSpeedIndicatorInstrumentControl.cs                          */
/* Version  : 1                                                              */
/* Language : C#                                                             */
/* Summary  : The air speed indicator instrument control                     */
/* Creation : 19/06/2008                                                     */
/* Autor    : Guillaume CHOUTEAU                                             */
/* History  :                                                                */
/*****************************************************************************/

using System.Drawing;
using System.Windows.Forms;

namespace AvionicsInstrumentControlDemo
{
    class EngineIndicatorControl : InstrumentControl
    {
        #region Fields

        // Parameters
        int N2;

        // Images
        Bitmap bmpCadran = new Bitmap(AvionicsInstrumentControlDemo.AvionicsInstrumentsControls.AvionicsInstrumentsControlsRessources.EngineIndicator_Background);
        Bitmap bmpNeedle = new Bitmap(AvionicsInstrumentControlDemo.AvionicsInstrumentsControls.AvionicsInstrumentsControlsRessources.EngineIndicatorNeedle);

        #endregion

        #region Contructor

        /// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        public EngineIndicatorControl()
        {
            // Double bufferisation
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
        }

        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        #region Paint

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);

            // Pre Display computings
            Point ptRotation = new Point(150, 150);
            Point ptimgNeedle = new Point(136, 39);

            bmpCadran.MakeTransparent(Color.Yellow);
            bmpNeedle.MakeTransparent(Color.Yellow);

            double alphaNeedle = InterpolPhyToAngle(N2, 0, 100, 180, 430);

            float scale = (float)this.Width / bmpCadran.Width;

            // diplay mask
            Pen maskPen = new Pen(this.BackColor, 30 * scale);
            pe.Graphics.DrawRectangle(maskPen, 0, 0, bmpCadran.Width * scale, bmpCadran.Height * scale);

            // display cadran
            pe.Graphics.DrawImage(bmpCadran, 0, 0, (float)(bmpCadran.Width * scale), (float)(bmpCadran.Height * scale));

            // display small needle
            RotateImage(pe, bmpNeedle, alphaNeedle, ptimgNeedle, ptRotation, scale);

        }

        #endregion

        #region Methods


        /// <summary>
        /// Define the physical value to be displayed on the indicator
        /// </summary>
        /// <param name="TLA">The throttle lever angle</param>
        public void SetEngineIndicatorParameters(int N2Percentage)
        {
            N2 = N2Percentage;

            this.Refresh();
        }

        #endregion

        # region IDE




        # endregion

    }
}
