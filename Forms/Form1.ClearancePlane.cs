using _014;
using devDept;
using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace _014
{
    public partial class CNC_Measurement : Form
    {
        private void txt_form1_Clerance_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Enter tuşu - Validasyon yap ve işle
            if (e.KeyChar == (char)Keys.Enter)
            {
                try
                {
                    double finalValue = minimumClearancePlane; // Varsayılan değer

                    // Boş ise minimum değeri yaz
                    if (string.IsNullOrWhiteSpace(txt_form1_Clerance.Text))
                    {
                        txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                        Debug.WriteLine($"⚠️ Clearance Plane boş bırakıldı, minimum değer yazıldı: {minimumClearancePlane:F2}");
                        finalValue = minimumClearancePlane;
                    }
                    else if (double.TryParse(txt_form1_Clerance.Text, out double value))
                    {
                        // Minimum değerden küçükse, minimum değere çevir
                        if (value < minimumClearancePlane)
                        {
                            txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                            Debug.WriteLine($"⚠️ Girilen değer ({value:F2}) minimum değerin altında! Otomatik {minimumClearancePlane:F2} yapıldı");
                            finalValue = minimumClearancePlane;
                        }
                        else
                        {
                            Debug.WriteLine($"✅ Clearance Plane güncellendi: {value:F2} mm");
                            finalValue = value;
                        }
                    }
                    else
                    {
                        // Parse edilemezse minimum değeri yaz
                        txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                        Debug.WriteLine($"⚠️ Geçersiz değer girildi, minimum değer yazıldı: {minimumClearancePlane:F2}");
                        finalValue = minimumClearancePlane;
                    }

                    // ✅ TreeView'deki Z Safety'yi güncelle
                    if (treeViewManager != null)
                    {
                        treeViewManager.UpdateZSafetyFromClearancePlane(finalValue);
                    }

                    // Enter tuşu eventini tüket (bip sesi çıkmasın)
                    e.Handled = true;

                    // Focus'u başka bir kontrole taşı (opsiyonel)
                    this.ActiveControl = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Clearance Plane Enter tuşu hatası: {ex.Message}");
                }

                return;
            }

            // Sadece rakam (0-9) ve kontrol tuşlarına (Backspace, Delete) izin ver
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true; // Karakteri engelle
            }
        }

        private void txt_form1_Clerance_Leave(object sender, EventArgs e)
        {
            try
            {
                double finalValue = minimumClearancePlane; // Varsayılan değer

                // Boş ise minimum değeri yaz
                if (string.IsNullOrWhiteSpace(txt_form1_Clerance.Text))
                {
                    txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                    Debug.WriteLine($"⚠️ Clearance Plane boş bırakıldı, minimum değer yazıldı: {minimumClearancePlane:F2}");
                    finalValue = minimumClearancePlane;
                }
                else if (double.TryParse(txt_form1_Clerance.Text, out double value))
                {
                    // Minimum değerden küçükse, minimum değere çevir
                    if (value < minimumClearancePlane)
                    {
                        txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                        Debug.WriteLine($"⚠️ Girilen değer ({value:F2}) minimum değerin altında! Otomatik {minimumClearancePlane:F2} yapıldı");
                        finalValue = minimumClearancePlane;
                    }
                    else
                    {
                        Debug.WriteLine($"✅ Clearance Plane güncellendi: {value:F2} mm");
                        finalValue = value;
                    }
                }
                else
                {
                    // Parse edilemezse minimum değeri yaz
                    txt_form1_Clerance.Text = Math.Round(minimumClearancePlane, 2).ToString();
                    Debug.WriteLine($"⚠️ Geçersiz değer girildi, minimum değer yazıldı: {minimumClearancePlane:F2}");
                    finalValue = minimumClearancePlane;
                }

                // ✅ YENİ: TreeView'deki Z Safety'yi güncelle
                if (treeViewManager != null)
                {
                    treeViewManager.UpdateZSafetyFromClearancePlane(finalValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Clearance Plane değer kontrolü hatası: {ex.Message}");
            }
        }

        public void SetMinimumClearancePlane(double value)
        {
            minimumClearancePlane = value;

            // ✅ YENİ: TreeView'deki Z Safety'yi de güncelle
            if (treeViewManager != null)
            {
                treeViewManager.UpdateZSafetyFromClearancePlane(value);
            }

            Debug.WriteLine($"✅ Minimum Clearance Plane ayarlandı: {value:F2} mm");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (clearancePlaneManager != null)
                {
                    clearancePlaneManager.SetVisibility(checkBox1.Checked);
                    Debug.WriteLine($"✅ Clearance Plane checkbox: {(checkBox1.Checked ? "İşaretli (Görünür)" : "İşaretsiz (Gizli)")}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Clearance Plane checkbox hatası: {ex.Message}");
            }
        }
    }
}
