using System;
using System.Drawing;
using System.Windows.Forms;

namespace _014.Utilities.UI
{
    /// <summary>
    /// Kullanıcıya talimatlar gösteren modern, temiz panel
    /// Form + Label kombinasyonu ile scrollbar sorunu yok!
    /// </summary>
    public class InstructionPanel : Form
    {
        private Label lblInstruction;
        private Panel contentPanel;
        private Label separator;  // ✅ YENİ: Separator çizgisi

        public InstructionPanel(object design1)
        {
            InitializeComponent(design1);
        }

        private void InitializeComponent(object design1)
        {
            // ═══════════════════════════════════════════════════════════
            // FORM AYARLARI
            // ═══════════════════════════════════════════════════════════

            // ✅ BAŞLIK: Başlangıçta "Main Menu"
            Text = "Main Menu";

            // ✅ BOYUTLAR (FaceToFaceInfoPanel ile aynı - kompakt!)
            Size = new Size(250, 150);
            MinimumSize = new Size(200, 120);
            MaximumSize = new Size(300, 200);

            // ✅ POZİSYON: Sağ alt köşe (sabit!)
            StartPosition = FormStartPosition.Manual;
            int x = Screen.PrimaryScreen.WorkingArea.Width - Width - 10;  // Sağ kenardan 10px
            int y = Screen.PrimaryScreen.WorkingArea.Height - Height - 10;  // Alt kenardan 10px
            Location = new Point(x, y);

            // ✅ FORM STİLİ: Küçük başlık çubuğu
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            // ✅ KAPATMA DÜĞMESİNİ KALDIR (X düğmesi yok!)
            ControlBox = false;  // ❌ X düğmesi kaldırıldı!

            // ✅ Her zaman üstte
            TopMost = true;

            // ✅ Taskbar'da gösterme
            ShowInTaskbar = false;

            // ✅ Arka plan rengi
            BackColor = Color.FromArgb(245, 245, 245); // Açık gri

            // ✅ Resize devre dışı
            FormBorderStyle = FormBorderStyle.FixedToolWindow;

            // ═══════════════════════════════════════════════════════════
            // İÇERİK PANEL
            // ═══════════════════════════════════════════════════════════

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(10); // İç boşluk (daha kompakt)
            contentPanel.BackColor = Color.White;
            contentPanel.BorderStyle = BorderStyle.None; // ✅ Çerçeve yok

            // ═══════════════════════════════════════════════════════════
            // SEPARATOR (AYIRICI ÇİZGİ)
            // ═══════════════════════════════════════════════════════════

            separator = new Label();
            separator.Height = 2;
            separator.Dock = DockStyle.Top;
            separator.BorderStyle = BorderStyle.Fixed3D;
            separator.BackColor = Color.FromArgb(200, 200, 200); // Gri çizgi

            // ═══════════════════════════════════════════════════════════
            // LABEL (TALİMAT METNİ)
            // ═══════════════════════════════════════════════════════════

            lblInstruction = new Label();
            lblInstruction.Dock = DockStyle.Fill;
            lblInstruction.AutoSize = false; // Sabit boyut
            lblInstruction.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular); // Daha küçük font
            lblInstruction.ForeColor = Color.FromArgb(50, 50, 50); // Koyu gri
            lblInstruction.BackColor = Color.White;
            lblInstruction.TextAlign = ContentAlignment.TopLeft;
            lblInstruction.Padding = new Padding(3);

            // ✅ SCROLLBAR ASLA ÇIKMAZ (Label kullandığımız için!)

            // ═══════════════════════════════════════════════════════════
            // KONTROL HİYERARŞİSİ
            // ═══════════════════════════════════════════════════════════

            contentPanel.Controls.Add(lblInstruction);
            contentPanel.Controls.Add(separator);  // ✅ Separator en üstte
            Controls.Add(contentPanel);

            // ═══════════════════════════════════════════════════════════
            // BAŞLANGIÇ METNİ
            // ═══════════════════════════════════════════════════════════

            UpdatePanel(InstructionTexts.TITLE_MAIN_MENU, InstructionTexts.WELCOME);

            System.Diagnostics.Debug.WriteLine("✅ InstructionPanel oluşturuldu:");
            System.Diagnostics.Debug.WriteLine($"   → Boyut: {Width}x{Height}");
            System.Diagnostics.Debug.WriteLine($"   → Pozisyon: ({Location.X}, {Location.Y}) - Sağ alt köşe");
            System.Diagnostics.Debug.WriteLine($"   → Kapatma düğmesi: {(ControlBox ? "VAR ❌" : "YOK ✅")}");
            System.Diagnostics.Debug.WriteLine($"   → Başlık: {Text}");
        }

        /// <summary>
        /// Talimat metnini güncelle (sadece içerik)
        /// </summary>
        public void UpdateInstruction(string text)
        {
            if (lblInstruction != null && !lblInstruction.IsDisposed)
            {
                lblInstruction.Text = text;
                System.Diagnostics.Debug.WriteLine($"📋 Talimat güncellendi: {text.Substring(0, Math.Min(50, text.Length))}...");
            }
        }

        /// <summary>
        /// ✅ YENİ: Başlık ve içeriği birlikte güncelle
        /// </summary>
        public void UpdatePanel(string title, string instruction)
        {
            if (!IsDisposed)
            {
                Text = title;
                lblInstruction.Text = instruction;

                // ✅ Main Menu başlığı SOLUK GRİ olacak
                if (title == "Main Menu")
                {
                    ForeColor = Color.FromArgb(160, 160, 160); // Soluk gri başlık
                }
                else
                {
                    ForeColor = Color.FromArgb(50, 50, 50); // Normal koyu gri başlık
                }

                System.Diagnostics.Debug.WriteLine($"📋 Panel güncellendi: [{title}] - {instruction.Substring(0, Math.Min(30, instruction.Length))}...");
            }
        }

        /// <summary>
        /// Panel'i göster ve en üste getir
        /// </summary>
        public new void Show()
        {
            base.Show();
            BringToFront();
            TopMost = true;
        }
    }

    /// <summary>
    /// Talimat metinleri sabit sınıfı
    /// </summary>
    public static class InstructionTexts
    {
        // ═══════════════════════════════════════════════════════════
        // BAŞLIKLAR (Form başlığı için)
        // ═══════════════════════════════════════════════════════════

        public const string TITLE_MAIN_MENU = "Main Menu";
        public const string TITLE_FACE_TO_FACE = "Face Measurement";  // ✅ "Face to Face" değil!
        public const string TITLE_DIAMETER = "Diameter Measurement";
        public const string TITLE_EDGE_TO_EDGE = "Edge Measurement";  // ✅ Edge to Edge
        public const string TITLE_LENGTH = "Length Measurement";
        public const string TITLE_SURFACE_TO_SURFACE = "Surface Measurement";
        public const string TITLE_RIDGE_WIDTH = "Ridge Width";
        public const string TITLE_ANGLE_MEASUREMENT = "Angle Measurement";


        // ═══════════════════════════════════════════════════════════
        // İÇERİKLER (Panel içeriği için)
        // ═══════════════════════════════════════════════════════════

        public const string WELCOME =
            "Lütfen yapmak istediğiniz işlemi seçiniz.";

        public const string FACE_TO_FACE =
            "* Pick Surface\n" +
            "* ESC Quit";

        public const string DIAMETER =
            "* Pick Surface\n" +
            "* ESC Quit";

        // ✅ DIAMETER_MODE ekledim (Form1.cs'de kullanılıyor)
        public const string DIAMETER_MODE = DIAMETER;

        public const string LENGTH =
            "* Pick Point\n" +      // ✅ Length için "Point"
            "* ESC Quit";


        public const string RIDGE_WIDTH =
            "Dik duvar ölçüm modu aktif edilmiştir.\n\n" +
            "Sadece sarı renkli X+, X-, Y+, Y- yüzeyleri üzerinden bir nokta seçiniz.";

        public const string EDGE_TO_EDGE =
            "* Pick Edge\n" +
            "* ESC Quit" ;

        public const string ANGLE_MEASUREMENT =
    "Select a planar face, then pick 2 points on the surface.\n\n" +
    "ESC to quit.";

    }


}


    

