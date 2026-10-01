using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

namespace Lab03_02;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}

public class Form1 : Form
{
    // ----- Vùng soạn thảo -----
    private readonly RichTextBox richText = new RichTextBox();

    // ----- MenuStrip -----
    private readonly MenuStrip menuStrip = new MenuStrip();
    private readonly ToolStripMenuItem mnuHeThong = new ToolStripMenuItem("Hệ thống");
    private readonly ToolStripMenuItem mnuTaoMoi = new ToolStripMenuItem("Tạo văn bản mới");
    private readonly ToolStripMenuItem mnuMoTapTin = new ToolStripMenuItem("Mở tập tin");
    private readonly ToolStripMenuItem mnuLuu = new ToolStripMenuItem("Lưu nội dung văn bản");
    private readonly ToolStripMenuItem mnuThoat = new ToolStripMenuItem("Thoát");
    private readonly ToolStripMenuItem mnuDinhDang = new ToolStripMenuItem("Định dạng");

    // ----- ToolStrip -----
    private readonly ToolStrip toolStrip = new ToolStrip();
    private readonly ToolStripButton tsbMoi = new ToolStripButton("Mới");
    private readonly ToolStripButton tsbLuu = new ToolStripButton("Lưu");
    private readonly ToolStripComboBox cmbFonts = new ToolStripComboBox();
    private readonly ToolStripComboBox cmbSize = new ToolStripComboBox();
    private readonly ToolStripButton tsbBold = new ToolStripButton("B");
    private readonly ToolStripButton tsbItalic = new ToolStripButton("I");
    private readonly ToolStripButton tsbUnderline = new ToolStripButton("U");

    // ----- StatusStrip -----
    private readonly StatusStrip statusStrip = new StatusStrip();
    private readonly ToolStripStatusLabel lblSoTu = new ToolStripStatusLabel();

    // ----- Trạng thái file đang mở -----
    private string? duongDanFile = null;

    public Form1()
    {
        Text = "Soạn thảo văn bản";
        ClientSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;

        XayDungMenu();
        XayDungToolStrip();
        XayDungStatusStrip();
        XayDungRichText();

        // Thứ tự add quyết định vị trí: menu trên cùng, rồi toolstrip, rồi richtext, rồi statusbar đáy
        Controls.Add(richText);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        Controls.Add(statusStrip);
        MainMenuStrip = menuStrip;

        TaoVanBanMoi();
    }

    // ================= MENU =================
    private void XayDungMenu()
    {
        mnuTaoMoi.ShortcutKeys = Keys.Control | Keys.N;
        mnuTaoMoi.Click += (s, e) => TaoVanBanMoi();

        mnuMoTapTin.Click += (s, e) => MoTapTin();

        mnuLuu.ShortcutKeys = Keys.Control | Keys.S;
        mnuLuu.Click += (s, e) => LuuNoiDung();

        mnuThoat.Click += (s, e) => Close();
        mnuHeThong.DropDownItems.Add(mnuTaoMoi);
        mnuHeThong.DropDownItems.Add(mnuMoTapTin);
        mnuHeThong.DropDownItems.Add(new ToolStripSeparator());
        mnuHeThong.DropDownItems.Add(mnuLuu);
        mnuHeThong.DropDownItems.Add(mnuThoat);

        mnuDinhDang.Click += (s, e) => MoFontDialog();

        menuStrip.Items.Add(mnuHeThong);
        menuStrip.Items.Add(mnuDinhDang);
    }

    // ================= TOOLSTRIP =================
    private void XayDungToolStrip()
    {
        tsbMoi.Click += (s, e) => TaoVanBanMoi();
        tsbLuu.Click += (s, e) => LuuNoiDung();

        // ComboBox Font: load toàn bộ font hệ thống
        foreach (FontFamily font in new InstalledFontCollection().Families)
        {
            cmbFonts.Items.Add(font.Name);
        }
        cmbFonts.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFonts.SelectedIndexChanged += CmbFonts_SelectedIndexChanged;

        // ComboBox Size: danh sách cỡ chữ cho sẵn
        int[] cacCo = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        foreach (int co in cacCo) cmbSize.Items.Add(co.ToString());
        cmbSize.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbSize.SelectedIndexChanged += CmbSize_SelectedIndexChanged;

        tsbBold.CheckOnClick = true;
        tsbItalic.CheckOnClick = true;
        tsbUnderline.CheckOnClick = true;
        tsbBold.Font = new Font(tsbBold.Font, FontStyle.Bold);
        tsbItalic.Font = new Font(tsbItalic.Font, FontStyle.Italic);
        tsbUnderline.Font = new Font(tsbUnderline.Font, FontStyle.Underline);
        tsbBold.Click += (s, e) => DoiKieuChu(FontStyle.Bold);
        tsbItalic.Click += (s, e) => DoiKieuChu(FontStyle.Italic);
        tsbUnderline.Click += (s, e) => DoiKieuChu(FontStyle.Underline);

        toolStrip.Items.Add(tsbMoi);
        toolStrip.Items.Add(tsbLuu);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(cmbFonts);
        toolStrip.Items.Add(cmbSize);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(tsbBold);
        toolStrip.Items.Add(tsbItalic);
        toolStrip.Items.Add(tsbUnderline);
    }

    // ================= STATUS STRIP =================
    private void XayDungStatusStrip()
    {
        lblSoTu.Text = "Tổng số từ: 0";
        statusStrip.Items.Add(lblSoTu);
    }

    // ================= RICHTEXTBOX =================
    private void XayDungRichText()
    {
        richText.Dock = DockStyle.Fill;
        richText.Font = new Font("Tahoma", 14);
        richText.TextChanged += (s, e) => CapNhatSoTu();
    }

    // ================= CÁC CHỨC NĂNG =================
    private void TaoVanBanMoi()
    {
        richText.Clear();
        richText.Font = new Font("Tahoma", 14);
        richText.ForeColor = Color.Black;
        duongDanFile = null;

        cmbFonts.SelectedIndexChanged -= CmbFonts_SelectedIndexChanged;
        cmbSize.SelectedIndexChanged -= CmbSize_SelectedIndexChanged;
        cmbFonts.SelectedItem = "Tahoma";
        cmbSize.SelectedItem = "14";
        cmbFonts.SelectedIndexChanged += CmbFonts_SelectedIndexChanged;
        cmbSize.SelectedIndexChanged += CmbSize_SelectedIndexChanged;

        CapNhatSoTu();
    }

    private void MoTapTin()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Văn bản (*.txt;*.rtf)|*.txt;*.rtf|Tất cả (*.*)|*.*"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (Path.GetExtension(ofd.FileName).ToLower() == ".rtf")
                richText.LoadFile(ofd.FileName, RichTextBoxStreamType.RichText);
            else
                richText.LoadFile(ofd.FileName, RichTextBoxStreamType.PlainText);

            duongDanFile = ofd.FileName;
            CapNhatSoTu();
        }
    }

    private void LuuNoiDung()
    {
        if (duongDanFile == null)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Rich Text File (*.rtf)|*.rtf",
                FileName = "VanBanMoi.rtf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                richText.SaveFile(sfd.FileName, RichTextBoxStreamType.RichText);
                duongDanFile = sfd.FileName;
                MessageBox.Show("Lưu văn bản thành công.", "Thông báo");
            }
        }
        else
        {
            richText.SaveFile(duongDanFile, RichTextBoxStreamType.RichText);
            MessageBox.Show("Lưu văn bản thành công.", "Thông báo");
        }
    }

    private void MoFontDialog()
    {
        using var fontDlg = new FontDialog();
        fontDlg.ShowColor = true;
        fontDlg.ShowApply = true;
        fontDlg.ShowEffects = true;
        fontDlg.ShowHelp = true;

        if (fontDlg.ShowDialog() != DialogResult.Cancel)
        {
            richText.ForeColor = fontDlg.Color;
            richText.Font = fontDlg.Font;
        }
    }

    private void CmbFonts_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbFonts.SelectedItem == null) return;
        string tenFont = cmbFonts.SelectedItem.ToString()!;
        richText.SelectionFont = new Font(tenFont, richText.SelectionFont?.Size ?? 14,
            richText.SelectionFont?.Style ?? FontStyle.Regular);
    }

    private void CmbSize_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cmbSize.SelectedItem == null) return;
        float co = float.Parse(cmbSize.SelectedItem.ToString()!);
        richText.SelectionFont = new Font(richText.SelectionFont?.FontFamily ?? new FontFamily("Tahoma"),
            co, richText.SelectionFont?.Style ?? FontStyle.Regular);
    }

    // Bật/tắt 1 kiểu chữ (Bold/Italic/Underline) cho vùng đang chọn
    private void DoiKieuChu(FontStyle kieu)
    {
        if (richText.SelectionFont == null) return;
        FontStyle kieuMoi = richText.SelectionFont.Style ^ kieu; // đảo bit của kiểu tương ứng
        richText.SelectionFont = new Font(richText.SelectionFont, kieuMoi);
        richText.Focus();
    }

    private void CapNhatSoTu()
    {
        string noiDung = richText.Text.Trim();
        int soTu = noiDung.Length == 0
            ? 0
            : noiDung.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        lblSoTu.Text = $"Tổng số từ: {soTu}";
    }
}