using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;
using System.Diagnostics;

using System.Numerics;
using System.IO;


namespace ISM2Import
{
    class Program
    {
        static void Main(string[] args)
        {
            /*string folder = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\map\0500\001";

            string[] textureFolders = new string[2]
            {
                @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\map\texture",
                @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\map\texture2"
            };*/

            string folder = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\011";

            string[] textureFolders = new string[2]
            {
                @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\011\texture\001",
                @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\011\face\001"
            };

            Stopwatch sw = Stopwatch.StartNew();

            string[] ism2Files = Directory.GetFiles(folder, "*.ism2");

            foreach(string ism2 in ism2Files)
            {
                if(ism2.IndexOf("col_") != -1)
                {
                    continue; //Don't export collission data
                }

                string outputDir = Path.GetDirectoryName(ism2) + Path.DirectorySeparatorChar.ToString() + "pmx";
                if(!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                string pmx = outputDir + Path.DirectorySeparatorChar.ToString() + Path.GetFileName(Path.ChangeExtension(ism2, ".pmx"));
                Console.WriteLine("Importing " + Path.GetFileNameWithoutExtension(ism2));
                PMXModel md = ISMModel.ImportISM(ism2);
                CleanUpModel(md);
                TriangleClearance.SeperateTriangles(md, false, true);
                MirrorX(md);
                FindTextures(md, textureFolders, outputDir);
                md.SaveToFile(pmx);
                Console.WriteLine("");                    
            }

            sw.Stop();
            Console.WriteLine("Import complete - " + (int)Math.Round(sw.Elapsed.TotalSeconds) + " seconds");

            Console.ReadLine();
        }

        static void FindTextures(PMXModel md, string[] textureFolders, string outputFolder)
        {
            string textureOutputFolder = outputFolder + Path.DirectorySeparatorChar.ToString() + "tex";            

            foreach(PMXMaterial mat in md.Materials)
            {
                if(mat.DiffuseTexture == null || mat.DiffuseTexture.Length == 0)
                {
                    continue;
                }
                string texFileName = mat.DiffuseTexture;

                if (File.Exists(textureOutputFolder + Path.DirectorySeparatorChar.ToString() + texFileName))
                {
                    mat.DiffuseTexture = "tex\\" + mat.DiffuseTexture;
                    continue;
                }

                foreach(string textureFolder in textureFolders)
                {
                    if(File.Exists(textureFolder + Path.DirectorySeparatorChar.ToString() + texFileName))
                    {
                        mat.DiffuseTexture = "tex\\" + mat.DiffuseTexture;

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
            foreach(PMXVertex vtx in md.Vertices)
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
                foreach(PMXTriangle tri in mat.Triangles)
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
            for (int i = 0; i < vtxArrayBase.Length; i++)
            {
                vtxArray[i] = (PMXExtendedVertex)vtxArrayBase[i];
            }

            int[] duplicateOf = new int[vtxArray.Length];
            for(int i = 0; i < duplicateOf.Length; i++)
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

            for (int i = 0; i < duplicateOf.Length - 1; i++)
            {
                int originalIndexI = sortedArray[i].EasySlashIndex;

                if(i % 5000 == 0 && i != 0)
                {
                    Console.WriteLine(i + " vertices verified");
                }
                if(duplicateOf[originalIndexI] >= 0) //Already marked as duplicate
                {
                    continue;
                }

                for(int j = i + 1; j < duplicateOf.Length; j++)
                {
                    int originalIndexJ = sortedArray[j].EasySlashIndex;

                    if (duplicateOf[originalIndexJ] >= 0) //Already marked as duplicate of some other vertex
                    {
                        continue;
                    }

                    if(sortedArray[i].Equals(sortedArray[j]))
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
            for (int i = duplicateOf.Length - 1; i >= 0; i--)
            {
                if (duplicateOf[i] >= 0) //Already marked as duplicate
                {
                    slashables.Add(i);
                }
            }            

            foreach(PMXMaterial mat in md.Materials)
            {
                foreach(PMXTriangle tri in mat.Triangles)
                {
                    tri.Vertex1 = ReplaceVertexIfRequired(tri.Vertex1, md, duplicateOf);
                    tri.Vertex2 = ReplaceVertexIfRequired(tri.Vertex2, md, duplicateOf);
                    tri.Vertex3 = ReplaceVertexIfRequired(tri.Vertex3, md, duplicateOf);
                }
            }

            foreach(int slash in slashables)
            {
                md.Vertices.RemoveAt(slash);
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
