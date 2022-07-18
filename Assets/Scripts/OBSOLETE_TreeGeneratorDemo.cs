using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBSOLETE_TreeGeneratorDemo : MonoBehaviour
{
    Mesh tree;
    Branch root;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<int> indices = new List<int>();

    void Start()
    {
        tree = GetComponent<MeshFilter>().mesh;
        root = new Branch(0.6f, 0.45f, 2.5f);
    }


    void Update()
    {
        root.grow(1.0f);
        
        vertices.Clear();
        normals.Clear();
        indices.Clear();

        root.contruct(vertices, normals, indices);

        tree.Clear();

        tree.vertices = vertices.ToArray();
        tree.normals = normals.ToArray();
        tree.triangles = indices.ToArray();
    }

    public class Branch {
        //Translated from https://github.com/weigert/TinyEngine/blob/master/examples/6_Tree/tree.h

        //ID tracker for leaves
        private int ID = 0;
        private bool leaf = true;
        private static float[] treescale = { 15.0f, 5.0f };
        private static int ringsize = 12;
        private float taper = 0.6f;

        //Branch "Left", "Right", and Parent
        Branch A, B, P;

        //Variables that change the look of the tree that generated
        public float ratio = 0;
        public float spread = 0;
        public float splitSize = 0;
        public int depth = 0;

        private float splitDecay = 1e-2f;
        private float passRatio = 0.3f;
        private bool conserveArea = true;
        private float directedness = 0.5f;
        private int localdepth = 2;

        //Constructor for the branch
        public Branch(float ratio, float spread, float splitSize)
        {
            this.ratio = ratio;
            this.spread = spread;
            this.splitSize = splitSize;
        }

        //Adds a branch to the tree
        Branch(Branch b, bool root)
        {
            this.ratio = b.ratio;
            this.spread = b.spread;
            this.splitSize = b.splitSize;

            if (root) return;
            depth = b.depth + 1;
            P = b;

        }

        private Vector3 dir = new Vector3(0, 1, 0);
        private float length = 0;
        private float radius = 0;
        private float area = 0.1f;

        public void grow(float feed) 
        {
            radius = Mathf.Sqrt(area / Mathf.PI);

            if (leaf)
            {
                length += Mathf.Pow(feed, 1f / 3f);
                feed -= Mathf.Pow(feed, 1f / 3f) * area;
                area += feed / length;

                if(length > splitSize * Mathf.Exp(-splitDecay * depth))
                {
                    split();
                }
                return;

            }

            float pass = passRatio;

            if (conserveArea) //Feedback control for area conservation
            {
                pass = (A.area + B.area) / (A.area + B.area + area);
            }

            area += pass * feed / length; //Grow in Girth
            feed *= (1.0f - pass); //Reduce feed

            if (feed < 1e-5f) return; //Prevents over branching 

            A.grow(feed * ratio); //Grow children
            B.grow(feed * ratio);

        }


        Vector3 leafAverage(Branch b)
        {
            if (b.leaf) return b.length * b.dir;
            else return b.length * b.dir + ratio * leafAverage(b.A) + (1.0f - ratio) * leafAverage(b.B);
        } 

        void split()
        {
            leaf = false;

            A = new Branch(this, false);
            B = new Branch(this, false);

            A.ID = 2 * ID + 0; // All ID's are unique due to binary
            B.ID = 2 * ID + 1;

            //Branches should grow perpendicular to the direction with the highest leaf density

            Vector3 D = leafDensity(localdepth);            //Direction of Highest Density
            Vector3 N = Vector3.Cross(dir, D).normalized;   //Normal Vector
            Vector3 M = -1.0f * N;                          //Reflection

            float flip = Random.Range(0, 2) > 1 ? 1.0f : -1.0f;
            A.dir = Vector3.Lerp(flip * spread * N, dir,        ratio).normalized;
            B.dir = Vector3.Lerp(flip * spread * N, dir, 1.0f - ratio).normalized;

        }

        Vector3 leafDensity(int searchdepth)
        {
            //Random Vector for noise
            Vector3 r = new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));

            if (depth == 0) return r;

            Branch C = this;
            Vector3 rel = new Vector3(0f,0f,0f);
            while(C.depth > 0 && searchdepth-- >= 0)
            {
                rel += C.length * C.dir;
                C = C.P;
            }

            return directedness * (leafAverage(C) - rel).normalized + (1.0f - directedness) * r; 

        }

        delegate void AddBranchFunction(Branch b, Vector3 p);

        public void contruct(List<Vector3> vertices, List<Vector3> normals, List<int> indices)
        {
            var vetrex_list = new List<Vector3>();
            var normal_list = new List<Vector3>();
            var indbuf = new List<int>();

            AddBranchFunction addBranch = null;
            addBranch = (Branch b, Vector3 p) =>
            {
                Vector3 start = p;
                float xyz = b.length * treescale[0];
                Vector3 end = p + new Vector3(xyz * b.dir.x, xyz * b.dir.y, xyz * b.dir.z);

                Vector3 x = (b.dir + Vector3.one).normalized;
                Vector3 normal = Vector3.Cross(b.dir, x).normalized;
                Vector4 n = new Vector4(normal.x, normal.y, normal.z, 1.0f);
                Matrix4x4 r = Matrix4x4.Rotate(Quaternion.AngleAxis(Mathf.PI / ringsize, b.dir));

                int _b = vetrex_list.Count;

                for (int i = 0; i < ringsize; i++)
                {
                    indbuf.Add(_b + i * 2 + 0);
                    indbuf.Add(_b + (i * 2 + 2) % (2 * ringsize));
                    indbuf.Add(_b + i * 2 + 1);
                    //Upper Triangle
                    indbuf.Add(_b + (i * 2 + 2) % (2 * ringsize));
                    indbuf.Add(_b + (i * 2 + 3) % (2 * ringsize));
                    indbuf.Add(_b + i * 2 + 1);
                }

                for (int i = 0; i < ringsize; i++)
                {
                    vetrex_list.Add(
                        new Vector3
                            ( start.x + b.radius * treescale[1] * n.x
                            , start.y + b.radius * treescale[1] * n.y
                            , start.z + b.radius * treescale[1] * n.z
                            )
                    );
                    normal_list.Add(new Vector3(n.x, n.y, n.z));
                    n = r * n;

                    vetrex_list.Add(
                        new Vector3
                            ( end.x + taper * b.radius * treescale[1] * n.x
                            , end.y + taper * b.radius * treescale[1] * n.y
                            , end.z + taper * b.radius * treescale[1] * n.z
                            )
                    );
                    normal_list.Add(new Vector3(n.x, n.y, n.z));
                    n = r * n;
                }

                if (b.leaf) return;

                addBranch(b.A, end);
                addBranch(b.B, end);
            };

            addBranch(this, Vector3.zero);

            vertices.AddRange(vetrex_list);
            normals.AddRange(normal_list);
            indices.AddRange(indbuf);
        }

    }
}
