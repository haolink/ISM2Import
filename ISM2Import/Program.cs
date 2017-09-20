using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;

using System.Numerics;


namespace ISM2Import
{
    class Program
    {
        static void Main(string[] args)
        {
            PMXModel md = ISMModel.ImportISM(@"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\001\002.ism2");
            md.SaveToFile(@"F:\Steam\SteamApps\common\Megadimension Neptunia VII\CONTENTS\GAME\model\chara\001\002.pmx");
            Console.ReadLine();
        }
    }
}
