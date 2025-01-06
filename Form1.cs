using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicView
{
    public partial class Form1 : Form
    {
        private string currentImagePath;
        private string[] imageFiles;
        private int currentIndex = -1;
        private float zoomFactor = 1.0f;

        public Form1(string[] args)
        {
            InitializeComponent();
            InitializeContextMenu();

            // KeyPreview aktivieren
            this.KeyPreview = true;
            this.PreviewKeyDown += Form1_PreviewKeyDown; // PreviewKeyDown-Ereignis abonnieren
            this.KeyDown += Form1_KeyDown; // KeyDown bleibt für die Verarbeitung

            // Prüfen, ob ein Argument (Dateipfad) übergeben wurde
            if (args.Length > 0 && File.Exists(args[0]))
            {
                pictureBox.BackgroundImage = null;
                LoadImage(args[0]); // Lade das Bild
                LoadImagesInFolder(Path.GetDirectoryName(args[0])); // Lade alle Bilder im Ordner
                LoadImage(args[0]);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Überprüfen, ob die linke oder rechte Pfeiltaste gedrückt wurde
            if (keyData == Keys.Left || keyData == Keys.Right)
            {
                if (keyData == Keys.Left)
                {
                    button5_Click(null, EventArgs.Empty); // "Vorherige Bild"-Logik
                }
                else if (keyData == Keys.Right)
                {
                    button4_Click(null, EventArgs.Empty); // "Nächste Bild"-Logik
                }

                // Rückgabe von true verhindert die Standardverarbeitung der Taste
                return true;
            }

            // Für alle anderen Tasten die Standardverarbeitung durchführen
            return base.ProcessDialogKey(keyData);
        }


        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // Pfeiltasten als Eingabetasten markieren
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                e.IsInputKey = true; // Markiert die Tasten als "Eingabe"
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right) // Rechts-Taste
            {
                button4_Click(null, EventArgs.Empty); // "Nächste Bild"-Logik
            }
            else if (e.KeyCode == Keys.Left) // Links-Taste
            {
                button5_Click(null, EventArgs.Empty); // "Vorherige Bild"-Logik
            }
            else if (e.KeyCode == Keys.Delete) // ENTF-Taste
            {
                ConfirmAndDeleteImage(); // Löschabfrage
            }
        }

        private void ConfirmAndDeleteImage()
        {
            if (currentImagePath != null && File.Exists(currentImagePath))
            {
                var result = MessageBox.Show("Möchten Sie dieses Bild wirklich löschen?", "Bild löschen",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        File.Delete(currentImagePath);
                        MessageBox.Show("Bild wurde gelöscht.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        button4_Click(null, EventArgs.Empty); // Nächstes Bild anzeigen
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Löschen: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Kein Bild zum Löschen verfügbar!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                // ClickOnce-Version abrufen
                var version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

                labelBuildRevision.Text = $"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
            else
            {
                labelBuildRevision.Text = "Lokal gestartet - keine ClickOnce-Version verfügbar";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Bilddateien|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadImagesInFolder(Path.GetDirectoryName(ofd.FileName)); // Ordner laden und sortieren
                    LoadImage(ofd.FileName); // Bild laden und Position in Liste setzen
                    pictureBox.BackgroundImage = null;
                }
            }
        }


        private void InitializeContextMenu()
        {
            contextMenuStrip1 = new ContextMenuStrip();
            ToolStripMenuItem copyItem = new ToolStripMenuItem("Kopieren");
            copyItem.Click += CopyImageToClipboard; // Ereignis-Handler wird zugewiesen
            contextMenuStrip1.Items.Add(copyItem);
        }


        private void LoadImage(string path)
        {
            // Aktuelles Bild freigeben, falls vorhanden
            if (pictureBox.Image != null)
            {
                pictureBox.BackgroundImage = null;
                pictureBox.Image.Dispose(); // Bild freigeben
                pictureBox.Image = null;    // Referenz löschen
            }

            currentImagePath = path;

            // Alphabetischen Index aktualisieren
            if (imageFiles != null)
            {
                imageFiles = imageFiles.OrderBy(f => f).ToArray(); // Alphabetisch sortieren
                currentIndex = Array.IndexOf(imageFiles, path);
            }

            zoomFactor = 1.0f; // Zoom zurücksetzen

            // Neues Bild laden
            pictureBox.Image = Image.FromFile(path);

            labelFilename.Text = Path.GetFileName(path);
        }





        private void LoadImagesInFolder(string folderPath)
        {
            imageFiles = Directory.GetFiles(folderPath, "*.*")
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f) // Alphabetisch sortieren
                .ToArray();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                pictureBox.Refresh();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                pictureBox.Refresh();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (imageFiles != null && currentIndex < imageFiles.Length - 1)
            {
                LoadImage(imageFiles[++currentIndex]); // Lade das nächste Bild basierend auf dem aktuellen Index
            }
            else
            {
                MessageBox.Show("Keine weiteren Bilder vorhanden.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private void button5_Click(object sender, EventArgs e)
        {
            if (imageFiles != null && currentIndex > 0)
            {
                LoadImage(imageFiles[--currentIndex]); // Lade das vorherige Bild basierend auf dem aktuellen Index
            }
            else
            {
                MessageBox.Show("Keine vorherigen Bilder vorhanden.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private void CopyImageToClipboard(object sender, EventArgs e)
        {
        }

        private void kopierenToolStripMenuItem_Click(object sender, EventArgs e)
        {
                        if (pictureBox.Image != null)
            {
                Clipboard.SetImage(pictureBox.Image); // Bild in die Zwischenablage kopieren
            }
            else
            {
                MessageBox.Show("Kein Bild zum Kopieren verfügbar!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void labelFilename_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
