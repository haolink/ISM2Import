using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;

using PMXStructure.PMXClasses.Parts;
using PMXStructure.PMXClasses.Helpers;
using System.Diagnostics;

using System.Numerics;
using System.IO;


namespace ISM2Import
{
    class Program
    {
        static void Main(string[] args)
        {
            string folder = @"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\031";

            Stopwatch sw = Stopwatch.StartNew();

            string[] ism2Files = Directory.GetFiles(folder, "*.ism2");

            foreach(string ism2 in ism2Files)
            {
                if(ism2.IndexOf("col_") != -1)
                {
                    continue; //Don't export collission data
                }
                string pmx = Path.ChangeExtension(ism2, ".pmx");
                Console.WriteLine("Importing " + Path.GetFileNameWithoutExtension(ism2));
                PMXModel md = ISMModel.ImportISM(ism2);
                CleanUpModel(md);
                md.SaveToFile(pmx);
                Console.WriteLine("");                    
            }

            sw.Stop();
            Console.WriteLine("Import complete - " + (int)Math.Round(sw.Elapsed.TotalSeconds) + " seconds");

            Console.ReadLine();
        }

        static void CleanUpModel(PMXModel md)
        {
            Console.WriteLine("Duplicate search started.");
            Stopwatch sw = Stopwatch.StartNew(); 
                
            PMXVertex[] vtxArray = md.Vertices.ToArray(); //Arrays are faster to index than lists

            int[] duplicateOf = new int[vtxArray.Length];
            for(int i = 0; i < duplicateOf.Length; i++)
            {
                vtxArray[i].EasySlashIndex = i;
                duplicateOf[i] = -1;
            }

            Console.WriteLine(duplicateOf.Length + " vertices to consider!");
            for (int i = 0; i < duplicateOf.Length - 1; i++)
            {
                if(i % 5000 == 0 && i != 0)
                {
                    Console.WriteLine(i + " vertices verified");
                }
                if(duplicateOf[i] >= 0) //Already marked as duplicate
                {
                    continue;
                }

                for(int j = i + 1; j < duplicateOf.Length; j++)
                {
                    if (duplicateOf[j] >= 0) //Already marked as duplicate of some other vertex
                    {
                        continue;
                    }

                    if(vtxArray[i].Equals(vtxArray[j]))
                    {
                        duplicateOf[j] = i;
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
            int dupIndex = duplicateList[input.EasySlashIndex];
            if (dupIndex >= 0)
            {
                return md.Vertices[dupIndex];
            }
            return input;
        }
    }
}
