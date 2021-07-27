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

namespace CombineFiles
{
    public partial class Form1 : Form
    {
        private string GeneratedText { get; set; }
        
        private struct FileItem
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }

            public override string ToString()
            {
                return FileName;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = @"Sql files (*.sql)|*.sql|All files (*.*)|*.*",
                Multiselect = true,
                ClientGuid = Guid.Parse("7a08b33c-b478-4d97-913a-3f55ee006778")
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                AddFiles(dialog.FileNames);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.MoveSelectedItemUp();
            GenerateText();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.MoveSelectedItemDown();
            GenerateText();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            GenerateText();
        }

        private void GenerateText()
        {
            if (GeneratedText != null && GeneratedText != textEditorControl1.Text && MessageBox.Show(@"You have unsaved changes in the editor, are you sure you'd like to overwrite them?", @"Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var fullText = "";
                var filesMissing = false;
            
                foreach (var file in listBox1.Items.Cast<FileItem>().ToList())
                {
                    if (!File.Exists(file.FilePath))
                    {
                        if (!filesMissing)
                        {
                            MessageBox.Show(@"Some files were missing, please re-add them and try again", "Missing Files",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);  
                        }

                        filesMissing = true;

                        if (!file.FileName.EndsWith(" (missing)"))
                        {
                            UpdateFileName(file,  file.FileName + " (missing)");
                        }

                        continue;
                    }

                    if (file.FileName.EndsWith(" (missing)"))
                    {
                        UpdateFileName(file, file.FileName.Replace(" (missing)", ""));
                    }
                
                    var query = File.ReadAllText(file.FilePath).Trim();
        
                    fullText += query;
                
                    var lastIndexOfGo = query.LastIndexOf("go", StringComparison.OrdinalIgnoreCase);
                    if (lastIndexOfGo == -1 || !string.IsNullOrWhiteSpace(query[(lastIndexOfGo + 2)..]))
                    {
                        fullText += "GO";
                    }
                
                    fullText +=
                        $@"

INSERT INTO SchemaVersions VALUES('{file.FileName}', dbo.GetCurrentTimePacific());
GO

";
                }

                GeneratedText = fullText.TrimEnd();
                textEditorControl1.Text = GeneratedText;
            }
            catch
            {
                MessageBox.Show(@"Unable to load files, please try again", @"Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateFileName(FileItem fileItem, string newName)
        {
            var index = listBox1.Items.IndexOf(fileItem);

            var updatedFile = fileItem;
            updatedFile.FileName = newName;
                        
            listBox1.Items.RemoveAt(index);
            listBox1.Items.Insert(index, updatedFile);
        }
        
        private void AddFiles(IEnumerable<string> fileNames)
        {
            var items = fileNames
                .Select(x => new FileItem { FilePath = x, FileName = Path.GetFileName(x) } as object)
                .Where(x => !listBox1.Items.Contains(x))
                .ToArray();
                
            if (items.Any())
            {
                listBox1.Items.AddRange(items);
                GenerateText();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textEditorControl1.Text);

            MessageBox.Show(@"Query copied to clipboard", @"Copied to Clipboard", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            listBox1.SelectedItems
                .Cast<FileItem>()
                .ToList()
                .ForEach(x => listBox1.Items.Remove(x));
            
            GenerateText();
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            AddFiles(e.Data.GetData(DataFormats.FileDrop) as IEnumerable<string>);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = @"Sql files (*.sql)|*.sql|All files (*.*)|*.*",
                DefaultExt = "sql",
                AddExtension = true,
                FileName = "combined.sql",
                ClientGuid = Guid.Parse("12676032-9741-4eac-b71e-18293978b593")
            };
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, textEditorControl1.Text);

                    MessageBox.Show(@"Query saved to file", @"Saved to File", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch
                {
                    MessageBox.Show(@"Unable to save query to file, please try again", @"Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }        
        }
    }
}