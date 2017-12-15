using System;
using System.Collections.Generic;
using System.IO;
using EndianStreamTools;

using PMXStructure.PMXClasses;

using System.Numerics;

namespace ISM2Import
{
    public class ISMModel
    {
        public static PMXModel ImportISM(string filename)
        {
            ISMModel im = new ISMModel(filename);
            PMXModel ret = im.pmxModel;

            //Rotate model by 180°

            foreach(PMXVertex vtx in ret.Vertices)
            {
                vtx.Position.X *= -1;
                vtx.Position.Z *= -1;
                vtx.Normals.X *= -1;
                vtx.Normals.Z *= -1;
            }
            foreach (PMXBone bn in ret.Bones)
            {
                bn.Position.X *= -1;
                bn.Position.Z *= -1;                
            }

            im = null;
            return ret;
        }

        internal class SectionData
        {
            public uint SectionType { get; set; }
            public uint SectionOffset { get; set; }

            public SectionData() { }
        }

        internal class BoneDataTransform
        {
            public PMXBone Bone { get; set; }
            public Matrix4x4 Transformation { get; set; }
            public string Name { get; set; }
            
            public BoneDataTransform() { }
        }

        private string[] stringArray = null;
        private PMXModel pmxModel;
        private StreamParser sp;
        private byte versionA;

        private float _totalScale = 12.5f;

        private Dictionary<int, PMXBone> BoneArrRig = new Dictionary<int, PMXBone>();

        private List<string> SurfaceTextures = new List<string>();

        private ISMModel(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            sp = new StreamParser(fs, Endian.Little);

            pmxModel = new PMXModel();

            sp.BaseStream.Seek(0x00, SeekOrigin.Begin);

            if (sp.ReadU32() != 0x324D5349)
            {
                fs.Close();
                return;
            }

            sp.BaseStream.Seek(0x14, SeekOrigin.Begin);

            uint endianCheck = sp.ReadU32();
            if (endianCheck > 0 && endianCheck < 65535)
            {
                sp.Endian = Endian.Little;
            }
            else
            {
                sp.Endian = Endian.Big;
            }

            sp.BaseStream.Seek(0x04, SeekOrigin.Begin);
            versionA = sp.ReadU8();
            byte VersionB = sp.ReadU8();
            byte VersionC = sp.ReadU8();
            byte VersionD = sp.ReadU8();
            uint header3 = sp.ReadU32();
            uint header4 = sp.ReadU32();
            uint filesize = sp.ReadU32();
            uint sectionCount = sp.ReadU32();
            uint header7 = sp.ReadU32();
            uint header8 = sp.ReadU32();

            int i, j, k;

            SectionData[] SectionArray = new SectionData[sectionCount];
            for (i = 0; i < sectionCount; i++)
            {
                SectionArray[i] = new SectionData()
                {
                    SectionType = sp.ReadU32(),
                    SectionOffset = sp.ReadU32()
                };
            }


            /**
             * Importing strings begin
             */
            foreach (SectionData sd in SectionArray)
            {
                if (sd.SectionType == 33) //String array needs to be filled first  
                {
                    sp.BaseStream.Seek(sd.SectionOffset + 8, SeekOrigin.Begin);
                    int stringCount = sp.ReadS32();
                    stringArray = new string[stringCount];
                    uint[] strOffsets = new uint[stringCount];

                    for (i = 0; i < stringCount; i++)
                    {
                        strOffsets[i] = sp.ReadU32();
                    }

                    for (i = 0; i < stringCount; i++)
                    {
                        sp.BaseStream.Seek(strOffsets[i], SeekOrigin.Begin);
                        stringArray[i] = sp.ReadAnsiNullTerminatedString();
                    }
                }
            }
            /**
             * Importing strings end
             */


            /**
             *  Materials
             */
            foreach (SectionData sd in SectionArray)
            {
                if (sd.SectionType == 97)
                {
                    sp.BaseStream.Seek(sd.SectionOffset + 8, SeekOrigin.Begin);
                    int matTotal = sp.ReadS32();
                    uint[] matOffsetArray = new uint[matTotal];
                    for (i = 0; i < matTotal; i++)
                    {
                        matOffsetArray[i] = sp.ReadU32();
                    }

                    for (i = 0; i < matTotal; i++)
                    {
                        sp.BaseStream.Seek(matOffsetArray[i] + 8, SeekOrigin.Begin);
                        int matSubTotal = sp.ReadS32();
                        string matSubString1 = stringArray[sp.ReadS32()];
                        string matSubString2 = stringArray[sp.ReadS32()];
                        string matSubString3 = stringArray[sp.ReadS32()];
                        sp.BaseStream.Seek(4, SeekOrigin.Current);

                        PMXMaterial mat = new PMXMaterial(pmxModel);
                        pmxModel.Materials.Add(mat);
                        mat.NameEN = matSubString1;
                        mat.NameJP = matSubString1;
                        mat.Diffuse = new PMXColorRGB(0.77f, 0.77f, 0.77f);
                        mat.Specular = new PMXColorRGB(0.0f, 0.0f, 0.0f);
                        mat.Ambient = new PMXColorRGB(1.0f, 1.0f, 1.0f);
                        mat.StandardToonIndex = 3;
                        mat.EdgeEnabled = false;

                        if (matSubTotal > 0)
                        {
                            int matSubOffset = sp.ReadS32();
                            sp.BaseStream.Seek(matSubOffset + 12, SeekOrigin.Begin);
                            matSubOffset = sp.ReadS32();
                            sp.BaseStream.Seek(matSubOffset + 20, SeekOrigin.Begin);
                            matSubOffset = sp.ReadS32();
                            sp.BaseStream.Seek(matSubOffset + 24, SeekOrigin.Begin);
                            matSubOffset = sp.ReadS32();
                            sp.BaseStream.Seek(matSubOffset + 24, SeekOrigin.Begin);
                            matSubOffset = sp.ReadS32();
                            sp.BaseStream.Seek(matSubOffset, SeekOrigin.Begin);
                            mat.DiffuseTexture = stringArray[sp.ReadS32()] + ".dds";
                        }
                        else
                        {
                            mat.DiffuseTexture = "tex_c.dds";
                        }


                        //Console.WriteLine(texturename);
                    }
                }
            }

            /**
             *  Bones and material groups
             */
            foreach (SectionData sd in SectionArray)
            {
                if (sd.SectionType == 03)
                {
                    Console.WriteLine("Object data");
                    sp.BaseStream.Seek(sd.SectionOffset + 8, SeekOrigin.Begin);
                    int boneCount = sp.ReadS32();
                    uint[] boneOffsets = new uint[boneCount];

                    string boneDataString1 = stringArray[sp.ReadS32()];
                    string boneDataString2 = stringArray[sp.ReadS32()];

                    for (i = 0; i < boneCount; i++)
                    {
                        boneOffsets[i] = sp.ReadU32();
                    }

                    for (i = 0; i < boneCount; i++)
                    {
                        sp.BaseStream.Seek(boneOffsets[i] + 8, SeekOrigin.Begin);
                        /*uint sectionType = sp.ReadU32();
                        sp.ReadU32();*/
                        int boneHeaderTotal = sp.ReadS32();
                        string boneName1 = stringArray[sp.ReadS32()];
                        string boneName2 = stringArray[sp.ReadS32()];
                        sp.BaseStream.Seek(8, SeekOrigin.Current);
                        uint boneParentOffset = sp.ReadU32();
                        int boneParent = -1;
                        for (j = 0; j < boneCount; j++)
                        {
                            if (boneOffsets[j] == boneParentOffset)
                            {
                                boneParent = j;
                            }
                        }
                        sp.BaseStream.Seek(12, SeekOrigin.Current);
                        int boneIdNum = sp.ReadS32();
                        sp.BaseStream.Seek(16, SeekOrigin.Current);

                        uint[] boneHeaderOffsets = new uint[boneHeaderTotal];

                        for (j = 0; j < boneHeaderTotal; j++)
                        {
                            boneHeaderOffsets[j] = sp.ReadU32();
                        }

                        for (j = 0; j < boneHeaderTotal; j++)
                        {
                            sp.BaseStream.Seek(boneHeaderOffsets[j], SeekOrigin.Begin);
                            uint sectionType = sp.ReadU32();

                            if (sectionType == 76)
                            {
                                ImportSurface();
                            }
                            if (sectionType == 91)
                            {
                                ImportBone(boneName1, i, boneParent, boneIdNum);
                            }
                        }
                    }
                }
            }


            /**
             * Importing textures begin
             */
            /*foreach (SectionData sd in SectionArray)
            {
                if (sd.SectionType == 46)
                {
                    sp.BaseStream.Seek(sd.SectionOffset + 8, SeekOrigin.Begin);
                    int texTotal = sp.ReadS32();
                    uint[] texOffsetArray = new uint[texTotal];
                    for (i = 0; i < texTotal; i++)
                    {
                        texOffsetArray[i] = sp.ReadU32();
                    }

                    for (i = 0; i < texTotal; i++)
                    {
                        sp.BaseStream.Seek(texOffsetArray[i] + 12, SeekOrigin.Begin);
                        string texturename = stringArray[sp.ReadS32()];
                        sp.BaseStream.Seek(12, SeekOrigin.Current);
                        //Console.WriteLine(texturename);
                    }
                }
            }*/
            //TODO: Not sure what these do

            /**
             * Importing strings end
             */

            /**
             * Vertices
             */
            foreach (SectionData sd in SectionArray)
            {
                if (sd.SectionType == 11)
                {
                    Console.WriteLine("Vertex data");
                    sp.BaseStream.Seek(sd.SectionOffset + 8, SeekOrigin.Begin);
                    int vtxHeaderTotal = sp.ReadS32();

                    uint[] vtxHeadOffsets = new uint[vtxHeaderTotal];
                    for(i = 0; i < vtxHeaderTotal; i++)
                    {
                        vtxHeadOffsets[i] = sp.ReadU32();
                    }

                    for (i = 0; i < vtxHeaderTotal; i++)
                    {
                        sp.BaseStream.Seek(vtxHeadOffsets[i], SeekOrigin.Begin);
                        uint sectionType = sp.ReadU32();
                        if(sectionType == 10)
                        {
                            ImportVertexGroup();
                        }
                    }
                }
            }

            fs.Close();
        }


        private void ImportVertexGroup()
        {
            int i;

            sp.BaseStream.Seek(4, SeekOrigin.Current);
            int vtxHeadTotal = sp.ReadS32();
            sp.BaseStream.Seek(20, SeekOrigin.Current);

            uint[] vtxHeadOffsets = new uint[vtxHeadTotal];
            for (i = 0; i < vtxHeadTotal; i++)
            {
                vtxHeadOffsets[i] = sp.ReadU32();
            }

            for (i = 0; i < vtxHeadTotal; i++)
            { //Ensuring vertices import before triangles
                sp.BaseStream.Seek(vtxHeadOffsets[i], SeekOrigin.Begin);
                uint sectionType = sp.ReadU32();
                if(sectionType == 89)
                {
                    ImportVertices();
                }
            }

            for (i = 0; i < vtxHeadTotal; i++)
            {
                sp.BaseStream.Seek(vtxHeadOffsets[i], SeekOrigin.Begin);
                uint sectionType = sp.ReadU32();
                if (sectionType == 70)
                {
                    ImportTriangles();
                }
            }
        }

        private int matIndex = 0;
        private void ImportTriangles()
        {
            int i, j;

            sp.BaseStream.Seek(4, SeekOrigin.Current);
            int triDataTotal = sp.ReadS32();
            sp.BaseStream.Seek(16, SeekOrigin.Current);

            uint[] triDataOffset = new uint[triDataTotal];
            for (i = 0; i < triDataTotal; i++)
            {
                triDataOffset[i] = sp.ReadU32();
            }

            for (i = 0; i < triDataTotal; i++)
            {
                sp.BaseStream.Seek(triDataOffset[i], SeekOrigin.Begin);

                uint sectionType = sp.ReadU32();
                if(sectionType == 69)
                {
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    int triCount = (sp.ReadS32() / 3);
                    ushort triType = sp.ReadU16();
                    sp.BaseStream.Seek(6, SeekOrigin.Current);
                    int matNum = matIndex;

                    if(matIndex < this.SurfaceTextures.Count)
                    {
                        string texName = this.SurfaceTextures[matIndex];
                        for(j = 0; j < Math.Min(this.SurfaceTextures.Count, this.pmxModel.Materials.Count); j++)
                        {
                            string matName = this.pmxModel.Materials[j].NameEN;
                            if(matName == texName)
                            {
                                matNum = j;
                                break;
                            }
                        }
                    }

                    ImportTrianglesToMaterial(pmxModel.Materials[matNum], triCount, triType);                    
                }
            }
            matIndex++;
        }

        private void ImportTrianglesToMaterial(PMXMaterial mat, int triCount, int triType)
        {    
            for(int i = 0; i < triCount; i++)
            {
                PMXVertex vtx1 = null, vtx2 = null, vtx3 = null;
                if (triType == 0x05)
                {
                    vtx1 = pmxModel.Vertices[(int)sp.ReadU16()];
                    vtx2 = pmxModel.Vertices[(int)sp.ReadU16()];
                    vtx3 = pmxModel.Vertices[(int)sp.ReadU16()];
                }
                else
                {
                    vtx1 = pmxModel.Vertices[sp.ReadS32()];
                    vtx2 = pmxModel.Vertices[sp.ReadS32()];
                    vtx3 = pmxModel.Vertices[sp.ReadS32()];
                }

                PMXTriangle tri = new PMXTriangle(pmxModel, vtx1, vtx2, vtx3);
                mat.Triangles.Add(tri);
            }            
        }

        private int vtxLocIndexPMX = 0;
        private int vtxWeighIndexPMX = 0;
        private void ImportVertices()
        {
            int i;

            sp.BaseStream.Seek(4, SeekOrigin.Current);
            int vtxTotal = sp.ReadS32();
            ushort vtxType = sp.ReadU16();
            ushort vtxTypeB = sp.ReadU16();
            int vtxCount = sp.ReadS32();
            int vtxSize = sp.ReadS32();
            sp.BaseStream.Seek(4, SeekOrigin.Current);

            uint[] vtxTtlOffsets = new uint[vtxTotal];
            for (i = 0; i < vtxTotal; i++)
            {
                vtxTtlOffsets[i] = sp.ReadU32();
            }
            for (i = 0; i < vtxTotal; i++)
            {
                sp.BaseStream.Seek(vtxTtlOffsets[i], SeekOrigin.Begin);
                sp.BaseStream.Seek(20, SeekOrigin.Current);
                uint startOffset = sp.ReadU32();
                sp.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            }

            PMXVertex vtx;
            for (i = 0; i < vtxCount; i++)
            {                
                if(vtxType == 0x01)
                {
                    vtx = GetVertexOfIndex(ref vtxLocIndexPMX);
                    vtx.Position = new PMXVector3(sp.ReadSingle() * _totalScale, sp.ReadSingle() * _totalScale, sp.ReadSingle() * _totalScale);
                    vtx.Normals = new PMXVector3(sp.ReadHalfFloat(), sp.ReadHalfFloat(), sp.ReadHalfFloat());
                    vtx.UV.U = sp.ReadHalfFloat();
                    sp.BaseStream.Seek(6, SeekOrigin.Current);
                    vtx.UV.V = sp.ReadHalfFloat();
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                }

                if(vtxType == 0x03)
                {
                    vtx = GetVertexOfIndex(ref vtxWeighIndexPMX);

                    if (vtxSize == 0x10 && versionA == 1)
                    {
                        CreateDeform(vtx, 4, 1, 2);
                        sp.BaseStream.Seek(4, SeekOrigin.Current);
                    }

                    if (vtxSize == 0x20 && versionA == 1)
                    {
                        CreateDeform(vtx, 4, 1, 4);
                        sp.BaseStream.Seek(12, SeekOrigin.Current);
                    }

                    if (vtxSize == 0x20 && versionA == 2)
                    {
                        CreateDeform(vtx, 4, 2, 4);
                        sp.BaseStream.Seek(8, SeekOrigin.Current);
                    }

                    if (vtxSize == 0x30 && versionA == 2)
                    {
                        CreateDeform(vtx, 8, 2, 4);
                        //sp.BaseStream.Seek(8, SeekOrigin.Current);
                    }
                }                                
            }
        }

        internal class BonePlusWeight : IComparable<BonePlusWeight>
        {
            public PMXBone Bone { get; set; }
            public float Weight { get; set; }

            public BonePlusWeight() { }

            public int CompareTo(BonePlusWeight b)
            {
                return (-1) * this.Weight.CompareTo(b.Weight);
            }
        }
        private void CreateDeform(PMXVertex vtx, int boneCount, int boneIndexSize, int floatLength)
        {
            PMXBone[] bones = new PMXBone[boneCount];
            float[] weights = new float[boneCount];

            for(int i = 0; i < boneCount; i++)
            {
                switch(boneIndexSize)
                {
                    case 1:
                        bones[i] = GetBoneByBoneId((int)sp.ReadU8());
                        break;
                    case 2:
                        bones[i] = GetBoneByBoneId((int)sp.ReadU16());
                        break;
                    default:
                        throw new Exception();
                }                
            }

            for (int i = 0; i < boneCount; i++)
            {
                switch (floatLength)
                {
                    case 2:
                        weights[i] = sp.ReadHalfFloat();
                        break;
                    case 4:
                        weights[i] = sp.ReadSingle();
                        break;
                    default:
                        throw new Exception();
                }
            }

            List<BonePlusWeight> usableBones = new List<BonePlusWeight>();
            for (int i = 0; i < boneCount; i++)
            {
                if(bones[i] != null)
                {
                    usableBones.Add(new BonePlusWeight()
                    {
                        Bone = bones[i],
                        Weight = weights[i]
                    });
                }
            }

            usableBones.Sort();

            if(usableBones.Count == 1)
            {
                PMXVertexDeformBDEF1 df = new PMXVertexDeformBDEF1(pmxModel, vtx);
                df.Bone1 = usableBones[0].Bone;
                vtx.Deform = df;
            }
            else if(usableBones.Count == 2)
            {
                PMXVertexDeformBDEF2 df = new PMXVertexDeformBDEF2(pmxModel, vtx);
                df.Bone1 = usableBones[0].Bone;
                df.Bone2 = usableBones[1].Bone;
                df.Bone1Weight = (usableBones[0].Weight) / (usableBones[0].Weight + usableBones[1].Weight);
                vtx.Deform = df;
            }
            else if(usableBones.Count == 3)
            {
                PMXVertexDeformBDEF4 df = new PMXVertexDeformBDEF4(pmxModel, vtx);
                float totalWeight = usableBones[0].Weight + usableBones[1].Weight + usableBones[2].Weight;
                df.Bone1 = usableBones[0].Bone;
                df.Bone2 = usableBones[1].Bone;
                df.Bone3 = usableBones[2].Bone;
                df.Bone4 = null;
                df.Bone1Weight = (usableBones[0].Weight) / totalWeight;
                df.Bone2Weight = (usableBones[1].Weight) / totalWeight;
                df.Bone3Weight = (usableBones[2].Weight) / totalWeight;
                df.Bone4Weight = 0.0f;
                vtx.Deform = df;
            }
            else
            {
                PMXVertexDeformBDEF4 df = new PMXVertexDeformBDEF4(pmxModel, vtx);
                float totalWeight = usableBones[0].Weight + usableBones[1].Weight + usableBones[2].Weight + usableBones[3].Weight;
                df.Bone1 = usableBones[0].Bone;
                df.Bone2 = usableBones[1].Bone;
                df.Bone3 = usableBones[2].Bone;
                df.Bone4 = usableBones[3].Bone;
                df.Bone1Weight = (usableBones[0].Weight) / totalWeight;
                df.Bone2Weight = (usableBones[1].Weight) / totalWeight;
                df.Bone3Weight = (usableBones[2].Weight) / totalWeight;
                df.Bone4Weight = (usableBones[3].Weight) / totalWeight;
                vtx.Deform = df;
            }
        }

        private PMXBone GetBoneByBoneId(int boneId)
        {
            if(boneId <= 0)
            {
                return null;
            }
            else
            {
                if(boneId >= this.BoneArrRig.Count)
                {
                    //Console.WriteLine("Mark here");
                    return null;
                } else
                {
                    return this.BoneArrRig[boneId];
                }
                
            }
        }

        private PMXVertex GetVertexOfIndex(ref int index)
        {
            PMXVertex vtx = null;
            if (pmxModel.Vertices.Count <= index)
            {
                vtx = new PMXExtendedVertex(pmxModel);
                pmxModel.Vertices.Add(vtx);
            }
            else
            {
                vtx = pmxModel.Vertices[index];
            }
            index++;
            return vtx;
        }

        private Dictionary<int, BoneDataTransform> boneTransforms = new Dictionary<int, BoneDataTransform>();
        private void ImportBone(string boneName, int boneIndex, int boneParent, int boneId)
        {
            uint headerLength = sp.ReadU32();
            int transformTotal = sp.ReadS32();

            int i;

            uint[] transFormOffsets = new uint[transformTotal];
            for(i = 0; i < transformTotal; i++)
            {
                transFormOffsets[i] = sp.ReadU32();
            }

            float m11 = 1.0f, m12 = 0.0f, m13 = 0.0f, m14 = 0.0f;
            float m21 = 0.0f, m22 = 1.0f, m23 = 0.0f, m24 = 0.0f;
            float m31 = 0.0f, m32 = 0.0f, m33 = 1.0f, m34 = 0.0f;
            float m41 = 0.0f, m42 = 0.0f, m43 = 0.0f, m44 = 0.0f;

            float m15 = 1.0f, m16 = 0.0f, m17 = 0.0f, m18 = 0.0f;
            float m25 = 0.0f, m26 = 1.0f, m27 = 0.0f, m28 = 0.0f;
            float m35 = 0.0f, m36 = 0.0f, m37 = 1.0f, m38 = 0.0f;

            float rX = 0.0f, rY = 0.0f, rZ = 0.0f;
            float sX = 1.0f, sY = 1.0f, sZ = 1.0f;

            for (i = 0; i < transformTotal; i++)
            {
                sp.BaseStream.Seek(transFormOffsets[i], SeekOrigin.Begin);
                uint sectionType = sp.ReadU32();

                if (sectionType == 20) {
                    //Bone translation
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m41 = sp.ReadSingle() * _totalScale;
                    m42 = sp.ReadSingle() * _totalScale;
                    m43 = sp.ReadSingle() * _totalScale;
                }

                if (sectionType == 21)
                {
                    //Bone scale
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    sX = sp.ReadSingle();
                    sY = sp.ReadSingle();
                    sZ = sp.ReadSingle();
                }

                if (sectionType == 93)
                {
                    //Matrix X transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m15 = sp.ReadSingle();
                    m16 = sp.ReadSingle();
                    m17 = sp.ReadSingle();
                    m18 = sp.ReadSingle();
                }

                if (sectionType == 94)
                {
                    //Matrix Y transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m25 = sp.ReadSingle();
                    m26 = sp.ReadSingle();
                    m27 = sp.ReadSingle();
                    m28 = sp.ReadSingle();
                }

                if (sectionType == 95)
                {
                    //Matrix Z transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m35 = sp.ReadSingle();
                    m36 = sp.ReadSingle();
                    m37 = sp.ReadSingle();
                    m38 = sp.ReadSingle();
                }

                if (sectionType == 103)
                {
                    //Matrix J-X transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m11 = sp.ReadSingle();
                    m12 = sp.ReadSingle();
                    m13 = sp.ReadSingle();
                    m14 = sp.ReadSingle();
                }

                if (sectionType == 104)
                {
                    //Matrix J-Y transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m21 = sp.ReadSingle();
                    m22 = sp.ReadSingle();
                    m23 = sp.ReadSingle();
                    m24 = sp.ReadSingle();
                }

                if (sectionType == 105)
                {
                    //Matrix J-Z transform
                    sp.BaseStream.Seek(4, SeekOrigin.Current);
                    m31 = sp.ReadSingle();
                    m32 = sp.ReadSingle();
                    m33 = sp.ReadSingle();
                    m34 = sp.ReadSingle();
                }

                //Todo: 113, 114, 115, 116, 117, 118, 119
            }


            Matrix4x4 tfm = Matrix4x4.CreateScale(sX, sY, sZ);
            tfm = tfm * Matrix4x4.CreateRotationX(DegreeToRadian(m18)) * Matrix4x4.CreateRotationY(DegreeToRadian(m28)) * Matrix4x4.CreateRotationZ(DegreeToRadian(m38));
            Matrix4x4 tfm2 = Matrix4x4.CreateScale(sX, sY, sZ);
            tfm2 = tfm2 * Matrix4x4.CreateRotationX(DegreeToRadian(m14)) * Matrix4x4.CreateRotationY(DegreeToRadian(m24)) * Matrix4x4.CreateRotationZ(DegreeToRadian(m34));
            tfm = tfm * tfm2;
            tfm.M41 = m41;
            tfm.M42 = m42;
            tfm.M43 = m43;
            
            PMXBone parent = null;
            if(boneName != null && boneName.Trim().Length > 0)
            {
                if (boneParent >= 0 && boneTransforms.ContainsKey(boneParent))
                {                    
                    tfm *= boneTransforms[boneParent].Transformation;
                    parent = boneTransforms[boneParent].Bone;                    
                }
            }

            /*Console.WriteLine(String.Format("matrix3 [{0:G6}, {1:G6}, {2:G6}] [{3:G6}, {4:G6}, {5:G6}] [{6:G6}, {7:G6}, {8:G6}] [{9:G6}, {10:G6}, {11:G6}]", new object[] {
                tfm.M11, tfm.M12, tfm.M13, tfm.M21, tfm.M22, tfm.M23, tfm.M31, tfm.M32, tfm.M33, tfm.M41, tfm.M42, tfm.M43
            }));*/

            PMXBone bn = new PMXBone(pmxModel);
            boneTransforms.Add(boneIndex, new BoneDataTransform()
            {
                Bone = bn,
                Transformation = tfm,
                Name = boneName
            });

            bn.NameEN = boneName;
            bn.NameJP = boneName;
            Vector3 pos = tfm.Translation;
            bn.Position = new PMXVector3(pos.X, pos.Y, pos.Z);
            bn.Parent = parent;

            if(boneId > 0)
            {
                BoneArrRig.Add(boneId, bn);
            }

            pmxModel.Bones.Add(bn);
        }

        private float DegreeToRadian(float angle)
        {
            return (float)Math.PI * angle / 180.0f;
        }

        private void ImportSurface()
        {
            uint headerLength = sp.ReadU32();
            int surfaceTotal = sp.ReadS32();

            string surfaceName = stringArray[sp.ReadS32()];
            Console.WriteLine(surfaceName);

            sp.BaseStream.Seek(8, SeekOrigin.Current);

            uint[] surfaceOffsets = new uint[surfaceTotal];

            int i;

            for (i = 0; i < surfaceTotal; i++) {
                surfaceOffsets[i] = sp.ReadU32();
            }

            for (i = 0; i < surfaceTotal; i++)
            {
                sp.BaseStream.Seek(surfaceOffsets[i], SeekOrigin.Begin);
                Console.WriteLine("Surface start is located at 0x" + sp.BaseStream.Position.ToString("X").ToUpperInvariant());

                sp.BaseStream.Seek(12, SeekOrigin.Current);
                string surfaceMatName = stringArray[sp.ReadS32()];
                string surfaceTexName = stringArray[sp.ReadS32()];

                this.SurfaceTextures.Add(surfaceTexName);
                
                /*Console.WriteLine(surfaceMatName);
                Console.WriteLine(surfaceTexName);*/
            }  
        }
    }
}
