using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PMXStructure.PMXClasses;

namespace ISM2Import
{
    public class TriangleClearance
    {
        private PMXModel _model; 

        internal class TriangleEdge : IEqualityComparer<TriangleEdge>
        {
            public PMXExtendedVertex Vertex1 { get; set; }
            public PMXExtendedVertex Vertex2 { get; set; }

            private bool _identicalLocation;

            private int _hashCode = 0;

            public TriangleEdge(PMXVertex vtx1b, PMXVertex vtx2b, bool identicalLocation)
            {
                this._identicalLocation = identicalLocation;

                PMXExtendedVertex vtx1 = (PMXExtendedVertex)vtx1b;
                PMXExtendedVertex vtx2 = (PMXExtendedVertex)vtx2b;

                if (vtx1.EasySlashIndex < vtx2.EasySlashIndex)
                {
                    this.Vertex1 = vtx1;
                    this.Vertex2 = vtx2;
                }
                else
                {
                    this.Vertex1 = vtx2;
                    this.Vertex2 = vtx1;
                }

                if(!identicalLocation)
                {
                    this._hashCode = ((~this.Vertex1.EasySlashIndex) ^ this.Vertex2.EasySlashIndex);
                }
                else
                {
                    this._hashCode =
                        (this.Vertex1.Position.X + this.Vertex1.Position.Y + this.Vertex1.Position.Z + this.Vertex2.Position.X + this.Vertex2.Position.Y + this.Vertex2.Position.Z).GetHashCode();
                }
            }

            public TriangleEdge(PMXVertex vtx1, PMXVertex vtx2) : this(vtx1, vtx2, false)
            {
                
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (!(obj is TriangleEdge))
                {
                    return false;
                }

                TriangleEdge t = (TriangleEdge)obj;

                bool sameVertices = (t.Vertex1 == this.Vertex1 && t.Vertex2 == this.Vertex2);

                if(sameVertices)
                {
                    return true;
                }

                if(!this._identicalLocation)
                {
                    return false;
                }

                /*if(t.Vertex1.EasySlashIndex == 8857 && t.Vertex2.EasySlashIndex == 8858)
                {
                    Console.WriteLine("Mat 6");
                }*/

                PMXVector3 va1 = this.Vertex1.Position;
                PMXVector3 va2 = this.Vertex2.Position;

                PMXVector3 vb1 = t.Vertex1.Position;
                PMXVector3 vb2 = t.Vertex2.Position;

                if((SameLocation(va1, vb1) && SameLocation(va2, vb2)) || (SameLocation(va2, vb1) && SameLocation(va1, vb2)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private bool SameLocation(PMXVector3 a, PMXVector3 b)
            {
                return
                    (
                    a.X == b.X &&
                    a.Y == b.Y &&
                    a.Z == b.Z
                    );
            }

            public override int GetHashCode()
            {
                return this._hashCode;
            }

            public bool Equals(TriangleEdge x, TriangleEdge y)
            {
                return x.Equals(y);            
            }

            public int GetHashCode(TriangleEdge obj)
            {
                return obj.GetHashCode();
            }
        }

        private bool _identicalLocation;
        private bool _ignoreSmallParts;

        private TriangleClearance(PMXModel model, bool ignoreSmallParts, bool identicalLocation)
        {
            this._model = model;
            this._ignoreSmallParts = ignoreSmallParts;
            this._identicalLocation = identicalLocation;
        }

        private List<PMXMaterial> SplitTriangles(PMXMaterial mat)
        {
            int[] matIndex = new int[mat.Triangles.Count];
            int i, j, mI;
            int addedTriangles;

            for(i = 0; i < matIndex.Length; i++)
            {
                matIndex[i] = -1;
            }

            mI = -1;
            int outInfo = 0;
            HashSet<TriangleEdge> edgesInPart;
            for(i = 0; i < matIndex.Length; i++)
            {
                int cIndex = matIndex[i];
                if(cIndex >= 0)
                {
                    continue;
                }

                if(i >= outInfo)
                {
                    Console.WriteLine(i + " / " + matIndex.Length);
                    while(i >= outInfo)
                    {
                        outInfo += 250;
                    }
                }

                mI++;
                matIndex[i] = mI;
                edgesInPart = new HashSet<TriangleEdge>();
                AddEdgesToList(GetEdges(mat.Triangles[i]), edgesInPart);

                do
                {
                    addedTriangles = 0;
                    for(j = i + 1; j < matIndex.Length; j++)
                    {
                        if(matIndex[j] >= 0)
                        {
                            continue;
                        }

                        PMXTriangle tri = mat.Triangles[j];
                        if(IsTriangleConnected(tri, edgesInPart))
                        {
                            matIndex[j] = mI;
                            addedTriangles++;
                        }
                    }
                } while (addedTriangles > 0);
            }

            List<PMXTriangle>[] triangleGroups = new List<PMXTriangle>[mI + 1];
            for(i = 0; i <= mI; i++)
            {
                triangleGroups[i] = new List<PMXTriangle>();
            }
            for(i = 0; i < matIndex.Length; i++)
            {
                PMXTriangle tri = mat.Triangles[i];
                triangleGroups[matIndex[i]].Add(tri);
            }

            PMXMaterial restMaterial = new PMXMaterial(this._model);
            CopyMaterialSettings(mat, restMaterial);

            List<PMXMaterial> newMaterials = new List<PMXMaterial>();
            int resIndex = 0;
            foreach(List<PMXTriangle> triangleGroup in triangleGroups)
            {
                if(triangleGroup.Count < 10 && this._ignoreSmallParts)
                {
                    restMaterial.Triangles.AddRange(triangleGroup);
                    continue;
                }

                resIndex++;
                PMXMaterial nm = new PMXMaterial(this._model);
                CopyMaterialSettings(mat, nm);
                nm.NameEN = nm.NameEN + " " + resIndex.ToString();
                nm.NameJP = nm.NameJP + " " + resIndex.ToString();
                nm.Triangles.AddRange(triangleGroup);

                newMaterials.Add(nm);
            }

            if(restMaterial.Triangles.Count > 0)
            {
                restMaterial.NameEN = restMaterial.NameEN + " Rest";
                restMaterial.NameJP = restMaterial.NameJP + " Rest";
                newMaterials.Add(restMaterial);
            }

            return newMaterials;
        }

        private void CopyMaterialSettings(PMXMaterial baseMat, PMXMaterial newMat)
        {
            newMat.Alpha = baseMat.Alpha;
            newMat.Ambient = baseMat.Ambient;
            newMat.Comment = baseMat.Comment;
            newMat.Diffuse = baseMat.Diffuse;
            newMat.DiffuseTexture = baseMat.DiffuseTexture;
            newMat.DoubleSided = baseMat.DoubleSided;
            newMat.EdgeColor = baseMat.EdgeColor;
            newMat.EdgeEnabled = baseMat.EdgeEnabled;
            newMat.EdgeSize = baseMat.EdgeSize;
            newMat.GroundShadow = baseMat.GroundShadow;
            newMat.GroundShadowType = baseMat.GroundShadowType;
            newMat.NameEN = baseMat.NameEN;
            newMat.NameJP = baseMat.NameJP;
            newMat.NonStandardToonTexture = baseMat.NonStandardToonTexture;
            newMat.SelfShadow = baseMat.SelfShadow;
            newMat.SelfShadowPlus = baseMat.SelfShadowPlus;
            newMat.Specular = baseMat.Specular;
            newMat.SphereMode = baseMat.SphereMode;
            newMat.SphereTexture = baseMat.SphereTexture;
            newMat.StandardToon = baseMat.StandardToon;
            newMat.StandardToonIndex = baseMat.StandardToonIndex;
            newMat.VertexColor = baseMat.VertexColor;
            
            //Triangles are not being copied!
        }

        private bool IsTriangleConnected(PMXTriangle tri, HashSet<TriangleEdge> edgeList)
        {
            TriangleEdge[] edges = GetEdges(tri);
            bool found = false;

            foreach(TriangleEdge edg in edges)
            {
                if(edgeList.Contains(edg))
                {
                    found = true;
                    break;
                }
            }

            if(found)
            {
                AddEdgesToList(edges, edgeList);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddEdgesToList(TriangleEdge[] edges, HashSet<TriangleEdge> edgeList)
        {
            foreach (TriangleEdge edg in edges)
            {
                edgeList.Add(edg);
            }
        }

        private TriangleEdge[] GetEdges(PMXTriangle tri)
        {
            return new TriangleEdge[3]
            {
                new TriangleEdge(tri.Vertex1, tri.Vertex2, this._identicalLocation),
                new TriangleEdge(tri.Vertex2, tri.Vertex3, this._identicalLocation),
                new TriangleEdge(tri.Vertex3, tri.Vertex1, this._identicalLocation)
            };
        }

        public static void SeperateTriangles(PMXModel model)
        {
            SeperateTriangles(model, true, false);
        }

        public static void SeperateTriangles(PMXModel model, bool ignoreSmallParts, bool combineVertexLocations)
        {
            TriangleClearance clearModel = new TriangleClearance(model, ignoreSmallParts, combineVertexLocations);
            int i = 0;
            for(i = 0; i < clearModel._model.Vertices.Count; i++)
            {
                ((PMXExtendedVertex)clearModel._model.Vertices[i]).EasySlashIndex = i;
            }

            List<PMXMaterial> resultMaterials = new List<PMXMaterial>();

            foreach(PMXMaterial mat in clearModel._model.Materials)
            {
                List<PMXMaterial> splittedMaterials = clearModel.SplitTriangles(mat);
                resultMaterials.AddRange(splittedMaterials);
            }

            clearModel._model.Materials.Clear();
            clearModel._model.Materials.AddRange(resultMaterials);
        }
    }
}
