using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;

namespace ISM2Import
{
    public class PMXExtendedVertex : PMXVertex, IComparable<PMXExtendedVertex>
    {
        public int EasySlashIndex { get; set; }

        private int? _hash;

        public PMXExtendedVertex(PMXModel md) : base(md) { }

        private uint FloatToInt(float input)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(input), 0);
        }

        public override int GetHashCode()
        {
            if(this._hash.HasValue)
            {
                return this._hash.Value;
            }

            uint lowHash =
                FloatToInt(this.Position.X) ^ FloatToInt(this.Position.Y) ^ FloatToInt(this.Position.Z) ^ FloatToInt(this.UV.U);

            uint highHash =
                FloatToInt(this.Normals.X) ^ FloatToInt(this.Normals.Y) ^ FloatToInt(this.Normals.Z) ^ FloatToInt(this.UV.V);

            this._hash = (int)(highHash ^  lowHash);

            return this._hash.Value;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PMXVertex))
            {
                return false;
            }

            PMXVertex vtx = (PMXVertex)obj;

            if (this.GetHashCode() != vtx.GetHashCode())
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

        public int CompareTo(PMXExtendedVertex other)
        {
            int res = this.Position.X.CompareTo(other.Position.X);
            if(res != 0)
            {
                return res;
            }

            res = this.Position.Y.CompareTo(other.Position.Y);
            if (res != 0)
            {
                return res;
            }

            res = this.Position.Z.CompareTo(other.Position.Z);
            if (res != 0)
            {
                return res;
            }

            res = this.Normals.X.CompareTo(other.Normals.X);
            if (res != 0)
            {
                return res;
            }

            res = this.Normals.Y.CompareTo(other.Normals.Y);
            if (res != 0)
            {
                return res;
            }

            res = this.Normals.Z.CompareTo(other.Normals.Z);
            if (res != 0)
            {
                return res;
            }

            res = this.UV.U.CompareTo(other.UV.U);
            if (res != 0)
            {
                return res;
            }

            res = this.UV.V.CompareTo(other.UV.V);
            return res;       
        }
    }
}
