using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMXStructure.PMXClasses.Parts
{
    public partial class PMXVertex
    {
        public int EasySlashIndex { get; set; }

        private ulong? _hash;

        private uint FloatToInt(float input)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(input), 0);
        }

        public void CalculateHash()
        {
            uint lowHash =
                FloatToInt(this.Position.X) ^ FloatToInt(this.Position.Y) ^ FloatToInt(this.Position.Z) ^ FloatToInt(this.UV.U);

            uint highHash =
                FloatToInt(this.Normals.X) ^ FloatToInt(this.Normals.Y) ^ FloatToInt(this.Normals.Z) ^ FloatToInt(this.UV.V);

            this._hash = (((ulong)highHash) << 32) | lowHash;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is PMXVertex))
            {
                return false;
            }

            PMXVertex vtx = (PMXVertex)obj;
            if(!vtx._hash.HasValue)
            {
                vtx.CalculateHash();
            }
            if(!this._hash.HasValue) 
            {
                this.CalculateHash();
            }

            if(this._hash.Value != vtx._hash.Value)
            {
                return false;
            }

            return
                (
                this.Position.X == vtx.Position.X &&
                this.Position.Y == vtx.Position.Y &&
                this.Position.Z == vtx.Position.Z &&
                this.Normals.X == vtx.Normals.X &&
                this.Normals.Y == vtx.Normals.Y &&
                this.Normals.Z == vtx.Normals.Z &&
                this.UV.U == vtx.UV.U &&
                this.UV.V == vtx.UV.V
                );
        }
    }
}
