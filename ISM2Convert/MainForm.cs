using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using PMXStructure.PMXClasses;
using System.Diagnostics;

using System.Numerics;
using System.IO;

using System.Globalization;

namespace ISM2Convert
{
    public partial class MainForm : Form
    {
        private string[] _selectedFiles;

        private static MainForm _staticSelf;

        public MainForm()
        {
            InitializeComponent();
            _staticSelf = this;
        }

        private void btnSelectInput_Click(object sender, EventArgs e)
        {
            if(dlgOpenISM.ShowDialog() == DialogResult.OK)
            {
                List<string> fns = new List<string>();
                fns.AddRange(dlgOpenISM.FileNames);

                if(fns.Count == 0)
                {
                    return;
                }

                this._selectedFiles = fns.ToArray();

                string fleText = " - " + this._selectedFiles.Length + " files selected - ";
                if(this._selectedFiles.Length == 1)
                {
                    fleText = " - one file selected - ";
                }

                lblInputInfo.Text = fleText;

                btnCv.Enabled = true;
            }
        }

        private bool _outputPMX;
        private bool _cleanVTX;
        private bool _splitTris;

        private void btnCv_Click(object sender, EventArgs e)
        {
            btnCv.Enabled = false;
            btnSelectInput.Enabled = false;
            chkMergeVertices.Enabled = false;
            chkSplitMeshes.Enabled = false;
            cbOutputFormat.Enabled = false;

            tbOutputlog.Text = "";

            this._outputPMX = (cbOutputFormat.SelectedIndex == 0);
            this._cleanVTX = chkMergeVertices.Checked;
            this._splitTris = chkSplitMeshes.Checked;

            Thread cvThread = new Thread(ConvertISMFiles);
            cvThread.Start();
        }

        public static void AppendToOutput(string text, bool newLine)
        {
            if(_staticSelf.tbOutputlog.InvokeRequired)
            {
                _staticSelf.tbOutputlog.Invoke((MethodInvoker)delegate
                {
                    AppendToOutput(text, newLine);
                });
            }
            else
            {
                string txt = text;
                if(newLine)
                {
                    txt = txt + "\r\n";
                }
                _staticSelf.tbOutputlog.AppendText(txt);                
            }
        }

        private void EnableFormFields()
        {
            if(this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    this.EnableFormFields();
                });
            } else
            {
                btnCv.Enabled = true;
                btnSelectInput.Enabled = true;
                chkMergeVertices.Enabled = true;
                chkSplitMeshes.Enabled = true;
                cbOutputFormat.Enabled = true;
            }
        }

        private void ConvertISMFiles()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string[] ismFiles = this._selectedFiles;

            

            foreach (string f in ismFiles)
            {
                string fn = Path.GetFileName(f);

                if (fn.ToLowerInvariant().Substring(0, 3) == "col")
                {
                    continue;
                }

                AppendToOutput("Importing " + fn, true);
                try
                {
                    PMXModel md = ISMModel.ImportISM(f);

                    if (this._cleanVTX)
                    {
                        CleanUpModel(md);
                    }

                    if (this._splitTris)
                    {
                        AppendToOutput("Splitting meshes", true);
                        TriangleClearance.SeperateTriangles(md, false, true);
                    }

                    AppendToOutput("Mirroring model", true);
                    MirrorX(md);
                    AppendToOutput("Saving", true);

                    if (this._outputPMX)
                    {
                        string pmx = Path.ChangeExtension(f, ".pmx");
                        md.SaveToFile(pmx);
                    }
                    else
                    {
                        string pmd = Path.ChangeExtension(f, ".pmd");
                        md.SaveToPMDFile(pmd);
                    }
                    AppendToOutput("", true);
                }
                catch (Exception ex)
                {
                    AppendToOutput("Importing failed: " + ex.Message, true);
                }
            }

            sw.Stop();
            AppendToOutput("Import complete - " + (int)Math.Round(sw.Elapsed.TotalSeconds) + " seconds", true);

            EnableFormFields();
        }

        void MirrorX(PMXModel md)
        {
            foreach (PMXVertex vtx in md.Vertices)
            {
                vtx.Position.X *= -1.0f;
                vtx.Normals.X *= -1.0f;
            }

            foreach (PMXBone bn in md.Bones)
            {
                bn.Position.X *= -1.0f;
            }

            foreach (PMXMaterial mat in md.Materials)
            {
                foreach (PMXTriangle tri in mat.Triangles)
                {
                    PMXVertex b1 = tri.Vertex1;
                    tri.Vertex1 = tri.Vertex3;
                    tri.Vertex3 = b1;
                }
            }
        }

        void CleanUpModel(PMXModel md)
        {
            AppendToOutput("Duplicate search started.", true);
            Stopwatch sw = Stopwatch.StartNew();

            PMXVertex[] vtxArrayBase = md.Vertices.ToArray(); //Arrays are faster to index than lists
            PMXExtendedVertex[] vtxArray = new PMXExtendedVertex[vtxArrayBase.Length];
            int i = 0;
            for (i = 0; i < vtxArrayBase.Length; i++)
            {
                vtxArray[i] = (PMXExtendedVertex)vtxArrayBase[i];
            }

            int[] duplicateOf = new int[vtxArray.Length];
            for (i = 0; i < duplicateOf.Length; i++)
            {
                ((PMXExtendedVertex)vtxArray[i]).EasySlashIndex = i;
                duplicateOf[i] = -1;
            }

            AppendToOutput(duplicateOf.Length + " vertices to consider!", true);

            AppendToOutput("Sorting", true);
            PMXExtendedVertex[] sortedArray = new PMXExtendedVertex[vtxArray.Length];
            vtxArray.CopyTo(sortedArray, 0);
            Array.Sort(sortedArray);
            AppendToOutput("Vertices sorted", true);

            for (i = 0; i < duplicateOf.Length - 1; i++)
            {
                int originalIndexI = sortedArray[i].EasySlashIndex;

                if (i % 5000 == 0 && i != 0)
                {
                    AppendToOutput(i + " vertices verified", true);
                }
                if (duplicateOf[originalIndexI] >= 0) //Already marked as duplicate
                {
                    continue;
                }

                for (int j = i + 1; j < duplicateOf.Length; j++)
                {
                    int originalIndexJ = sortedArray[j].EasySlashIndex;

                    if (duplicateOf[originalIndexJ] >= 0) //Already marked as duplicate of some other vertex
                    {
                        continue;
                    }

                    if (sortedArray[i].Equals(sortedArray[j]))
                    {
                        duplicateOf[originalIndexJ] = originalIndexI;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            List<int> slashables = new List<int>();
            AppendToOutput("Collecting slashing data", true);
            for (i = duplicateOf.Length - 1; i >= 0; i--)
            {
                if (duplicateOf[i] >= 0) //Already marked as duplicate
                {
                    slashables.Add(i);
                }
            }

            AppendToOutput("Reordering triangles", true);
            i = 0;
            foreach (PMXMaterial mat in md.Materials)
            {
                foreach (PMXTriangle tri in mat.Triangles)
                {
                    if (i != 0 && i % 5000 == 0)
                    {
                        AppendToOutput(i.ToString() + " triangles reordered!", true);
                    }
                    tri.Vertex1 = ReplaceVertexIfRequired(tri.Vertex1, md, duplicateOf);
                    tri.Vertex2 = ReplaceVertexIfRequired(tri.Vertex2, md, duplicateOf);
                    tri.Vertex3 = ReplaceVertexIfRequired(tri.Vertex3, md, duplicateOf);
                    i++;
                }
            }

            AppendToOutput("Removing duplicate vertices", true);
            i = 0;
            foreach (int slash in slashables)
            {
                if (i != 0 && i % 5000 == 0)
                {
                    AppendToOutput(i.ToString() + " vertices removed!", true);
                }
                md.Vertices.RemoveAt(slash);
                i++;
            }

            sw.Stop();
            AppendToOutput(slashables.Count + " duplicate vertices identified - " + sw.ElapsedMilliseconds + " milliseconds", true);
        }

        private PMXVertex ReplaceVertexIfRequired(PMXVertex input, PMXModel md, int[] duplicateList)
        {
            int dupIndex = duplicateList[((PMXExtendedVertex)input).EasySlashIndex];
            if (dupIndex >= 0)
            {
                return md.Vertices[dupIndex];
            }
            return input;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            cbOutputFormat.SelectedIndex = 0;
        }
    }
}
