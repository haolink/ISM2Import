using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;
using System.Diagnostics;

using System.Numerics;
using System.IO;

using System.Threading;
using System.Globalization;

namespace ISM2Import
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            /*string folder = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\attach";
            string converted = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\converted_attachments";            

            Stopwatch sw = Stopwatch.StartNew();

            string[] foundAttachmentFolders = Directory.GetDirectories(folder, "a*");

            List<string> attachmentFoldersList = new List<string>();
            foreach (string fat in foundAttachmentFolders)
            {
                if (File.Exists(fat + Path.DirectorySeparatorChar + "model.ism2"))
                {
                    attachmentFoldersList.Add(fat);
                }
            }
            string[] attachmentFolders = attachmentFoldersList.ToArray();

            Dictionary<string, string> failed = new Dictionary<string, string>();

            foreach (string af in attachmentFolders)
            {
                string ism2 = af + Path.DirectorySeparatorChar + "model.ism2";
                //string pmx = Path.ChangeExtension(ism2, ".pmx");

                string attName = Path.GetFileName(af);
                string output = converted + Path.DirectorySeparatorChar + attName;
                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output);
                }
                string pmx = output + Path.DirectorySeparatorChar + "model.pmx";
                if (File.Exists(pmx))
                {
                    File.Delete(pmx);
                }
                string[] textureFolders = new string[]
                {
                    af + Path.DirectorySeparatorChar + @"texture\001",
                    af + Path.DirectorySeparatorChar + @"face\001",
                };

                Console.WriteLine("Importing " + attName);
                try
                { 
                    PMXModel md = ISMModel.ImportISM(ism2);
                    CleanUpModel(md);
                    //TriangleClearance.SeperateTriangles(md, false, true);
                    MirrorX(md);
                    FindTextures(md, textureFolders, output);
                    md.SaveToFile(pmx);
                    Console.WriteLine("");
                } catch(Exception ex)
                {
                    Console.WriteLine("Importing failed: " + ex.Message);
                    failed.Add(attName, ex.Message);
                }
            }

            if(failed.Keys.Count > 0)
            {
                StreamWriter stw = new StreamWriter(converted + Path.DirectorySeparatorChar + "fails.txt");
                foreach(KeyValuePair<string, string> p in failed)
                {
                    stw.WriteLine(p.Key + " --> " + p.Value);
                }
                stw.Close();
            }

            sw.Stop();
            Console.WriteLine("Import complete - " + (int)Math.Round(sw.Elapsed.TotalSeconds) + " seconds");*/

            /*PMXModel md = ISMModel.ImportISM(@"F:\Steam\SteamApps\common\Megadimension Neptunia VII\ALLDLC\GAME\MODEL\CHARA\007\002.ism2");
            CleanUpModel(md);
            MirrorX(md);
            md.SaveToFile(@"F:\Steam\SteamApps\common\Megadimension Neptunia VII\ALLDLC\GAME\MODEL\CHARA\007\002.pmx");*/

            string folder = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\map\0200\002";

            Stopwatch sw = Stopwatch.StartNew();

            string[] ismFiles = Directory.GetFiles(folder, "*.ism2");            

            foreach (string f in ismFiles)
            {                
                string fn = Path.GetFileName(f);

                if(fn.ToLowerInvariant().Substring(0, 3) == "col")
                {
                    continue;
                }

                Console.WriteLine("Importing " + fn);
                try
                {
                    string pmx = Path.ChangeExtension(f, ".pmx");
                    PMXModel md = ISMModel.ImportISM(f);
                    CleanUpModel(md);
                    //TriangleClearance.SeperateTriangles(md, false, true);
                    MirrorX(md);
                    Console.WriteLine("Mirroring model");
                    //FindTextures(md, textureFolders, output);
                    Console.WriteLine("Saving");
                    md.SaveToFile(pmx);
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Importing failed: " + ex.Message);
                }
            }            

            sw.Stop();
            Console.WriteLine("Import complete - " + (int)Math.Round(sw.Elapsed.TotalSeconds) + " seconds");

            Console.ReadLine();
        }

        static void FindTextures(PMXModel md, string[] textureFolders, string outputFolder)
        {
            string textureOutputFolder = outputFolder;

            foreach (PMXMaterial mat in md.Materials)
            {
                if (mat.DiffuseTexture == null || mat.DiffuseTexture.Length == 0)
                {
                    continue;
                }
                string texFileName = mat.DiffuseTexture;

                if (File.Exists(textureOutputFolder + Path.DirectorySeparatorChar.ToString() + texFileName))
                {
                    mat.DiffuseTexture = mat.DiffuseTexture;
                    continue;
                }

                foreach (string textureFolder in textureFolders)
                {
                    if (File.Exists(textureFolder + Path.DirectorySeparatorChar.ToString() + texFileName))
                    {
                        mat.DiffuseTexture = mat.DiffuseTexture;

                        if (!Directory.Exists(textureOutputFolder))
                        {
                            Directory.CreateDirectory(textureOutputFolder);
                        }

                        File.Copy(textureFolder + Path.DirectorySeparatorChar.ToString() + texFileName, textureOutputFolder + Path.DirectorySeparatorChar.ToString() + texFileName);
                        break;
                    }
                }
            }
        }

        static void MirrorX(PMXModel md)
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

        static void CleanUpModel(PMXModel md)
        {
            Console.WriteLine("Duplicate search started.");
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

            Console.WriteLine(duplicateOf.Length + " vertices to consider!");

            Console.WriteLine("Sorting");
            PMXExtendedVertex[] sortedArray = new PMXExtendedVertex[vtxArray.Length];
            vtxArray.CopyTo(sortedArray, 0);
            Array.Sort(sortedArray);
            Console.WriteLine("Vertices sorted");

            for (i = 0; i < duplicateOf.Length - 1; i++)
            {
                int originalIndexI = sortedArray[i].EasySlashIndex;

                if (i % 5000 == 0 && i != 0)
                {
                    Console.WriteLine(i + " vertices verified");
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
            Console.WriteLine("Collecting slashing data");
            for (i = duplicateOf.Length - 1; i >= 0; i--)
            {
                if (duplicateOf[i] >= 0) //Already marked as duplicate
                {
                    slashables.Add(i);
                }
            }

            Console.WriteLine("Reordering triangles");
            i = 0;
            foreach (PMXMaterial mat in md.Materials)
            {
                foreach (PMXTriangle tri in mat.Triangles)
                {
                    if (i != 0 && i % 5000 == 0)
                    {
                        Console.WriteLine(i.ToString() + " triangles reordered!");
                    }
                    tri.Vertex1 = ReplaceVertexIfRequired(tri.Vertex1, md, duplicateOf);
                    tri.Vertex2 = ReplaceVertexIfRequired(tri.Vertex2, md, duplicateOf);
                    tri.Vertex3 = ReplaceVertexIfRequired(tri.Vertex3, md, duplicateOf);
                    i++;
                }
            }

            Console.WriteLine("Removing duplicate vertices");
            i = 0;
            foreach (int slash in slashables)
            {
                if (i != 0 && i % 5000 == 0)
                {
                    Console.WriteLine(i.ToString() + " vertices removed!");
                }
                md.Vertices.RemoveAt(slash);
                i++;
            }

            sw.Stop();
            Console.WriteLine(slashables.Count + " duplicate vertices identified - " + sw.ElapsedMilliseconds + " milliseconds");
        }

        private static PMXVertex ReplaceVertexIfRequired(PMXVertex input, PMXModel md, int[] duplicateList)
        {
            int dupIndex = duplicateList[((PMXExtendedVertex)input).EasySlashIndex];
            if (dupIndex >= 0)
            {
                return md.Vertices[dupIndex];
            }
            return input;
        }
    }
}
