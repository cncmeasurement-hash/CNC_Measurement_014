using devDept.Eyeshot.Control;
using System.Drawing;
using System.Windows.Forms;
using DrawingPoint = System.Drawing.Point;
using DrawingSize = System.Drawing.Size;

namespace _014
{
    /// <summary>
    /// UIManager - COORDINATE LABEL
    /// ✅ PARTIAL CLASS 2/4: Coordinate label oluşturma ve yönetme
    /// </summary>
    public partial class UIManager
    {
        private void CreateCoordinateLabel()
        {
            coordinateLabel = new Label();
            coordinateLabel.AutoSize = false;
            coordinateLabel.Size = new DrawingSize(320, 90);
            coordinateLabel.Location = new DrawingPoint(10, 10);
            coordinateLabel.BackColor = Color.FromArgb(200, 50, 50, 50);
            coordinateLabel.ForeColor = Color.Lime;
            coordinateLabel.Font = new Font("Consolas", 12, FontStyle.Bold);
            coordinateLabel.TextAlign = ContentAlignment.MiddleLeft;
            coordinateLabel.Padding = new Padding(10);
            coordinateLabel.Visible = false;
            coordinateLabel.BringToFront();

            design.Controls.Add(coordinateLabel);
        }
    }
}
