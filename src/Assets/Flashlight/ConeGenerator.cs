using System.Collections;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
public class ConeGenerator : MonoBehaviour
{
    public Material coneMaterial; // material for the cone part
    public Material planesMaterial; // material for the plane part
    public int coneVertices = 10; // number of vertices in the base top/bottom
    public float coneRadiusTop = 0f; // radius of the base top
    public float coneRadiusBottom = 1f; // radius of the base bottom
    public float coneDepth = 1f; // height of the entire cone (between top and bottom bases)
    //public float openingAngle = 0f; // if >0, create a cone with this angle by setting radiusTop to 0, and adjust radiusBottom according to coneDepth;
    public int coneHorizontalSplit = 0;
    public bool outside = true; // draw faces outside the cone
    public bool inside = false; // draw faces inside the cone

    public int nPlanes = 1; // number of planes along the cone

    public void CreateCone()
    {
        CleanChildren();
        // if (openingAngle > 0 && openingAngle < 180)
        // {
        //     radiusTop = 0;
        //     radiusBottom = coneDepth * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);
        // }
        GameObject Cone = new GameObject("cone");
        Cone.AddComponent<MeshFilter>();
        Cone.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        int multiplier = (outside ? 1 : 0) + (inside ? 1 : 0); // 1 or 2, corresponding to sides shown (or nothing)
        int offset = (outside && inside ? 2 * coneVertices : 0); // offset in the vertices lists when draw triangles when 2 sides
        Vector3[] vertices = new Vector3[2 * multiplier * coneVertices]; // 0..n-1: top, n..2n-1: bottom  | 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        Vector3[] normals = new Vector3[2 * multiplier * coneVertices]; // 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        Vector2[] uvs = new Vector2[2 * multiplier * coneVertices]; // 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        int[] tris; // triangles idx list
        float slope = Mathf.Atan((coneRadiusBottom - coneRadiusTop) / coneDepth); // (rad difference)/height
        float slopeSin = Mathf.Sin(slope);
        float slopeCos = Mathf.Cos(slope);
        int i;

        for (i = 0; i < coneVertices; i++)
        {
            float angle = 2 * Mathf.PI * i / coneVertices;
            float angleSin = Mathf.Sin(angle);
            float angleCos = Mathf.Cos(angle);
            float angleHalf = 2 * Mathf.PI * (i + 0.5f) / coneVertices; // for degenerated normals at cone tips
            float angleHalfSin = Mathf.Sin(angleHalf);
            float angleHalfCos = Mathf.Cos(angleHalf);

            vertices[i] = new Vector3(coneRadiusTop * angleCos, coneRadiusTop * angleSin, 0);
            vertices[i + coneVertices] = new Vector3(coneRadiusBottom * angleCos, coneRadiusBottom * angleSin, coneDepth);

            if (coneRadiusTop == 0)
                normals[i] = new Vector3(angleHalfCos * slopeCos, angleHalfSin * slopeCos, -slopeSin);
            else
                normals[i] = new Vector3(angleCos * slopeCos, angleSin * slopeCos, -slopeSin);
            if (coneRadiusBottom == 0)
                normals[i + coneVertices] = new Vector3(angleHalfCos * slopeCos, angleHalfSin * slopeCos, -slopeSin);
            else
                normals[i + coneVertices] = new Vector3(angleCos * slopeCos, angleSin * slopeCos, -slopeSin);

            uvs[i] = new Vector2(1.0f * i / coneVertices, 1);
            uvs[i + coneVertices] = new Vector2(1.0f * i / coneVertices, 0);

            if (outside && inside)
            {
                // vertices and uvs are identical on inside and outside, so just copy
                vertices[i + 2 * coneVertices] = vertices[i];
                vertices[i + 3 * coneVertices] = vertices[i + coneVertices];
                uvs[i + 2 * coneVertices] = uvs[i];
                uvs[i + 3 * coneVertices] = uvs[i + coneVertices];
            }
            if (inside)
            {
                // invert normals
                normals[i + offset] = -normals[i];
                normals[i + coneVertices + offset] = -normals[i + coneVertices];
            }
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        // create triangles
        int cnt = 0;
        if (coneRadiusTop == 0)
        {
            // top cone
            tris = new int[coneVertices * 3 * multiplier];
            if (outside)
                for (i = 0; i < coneVertices; i++)
                {
                    tris[cnt++] = i + coneVertices;
                    tris[cnt++] = i;
                    if (i == coneVertices - 1)
                        tris[cnt++] = coneVertices;
                    else
                        tris[cnt++] = i + 1 + coneVertices;
                }
            if (inside)
                for (i = offset; i < coneVertices + offset; i++)
                {
                    tris[cnt++] = i;
                    tris[cnt++] = i + coneVertices;
                    if (i == coneVertices - 1 + offset)
                        tris[cnt++] = coneVertices + offset;
                    else
                        tris[cnt++] = i + 1 + coneVertices;
                }
        }
        else if (coneRadiusBottom == 0)
        {
            // bottom cone
            tris = new int[coneVertices * 3 * multiplier];
            if (outside)
                for (i = 0; i < coneVertices; i++)
                {
                    tris[cnt++] = i;
                    if (i == coneVertices - 1)
                        tris[cnt++] = 0;
                    else
                        tris[cnt++] = i + 1;
                    tris[cnt++] = i + coneVertices;
                }
            if (inside)
                for (i = offset; i < coneVertices + offset; i++)
                {
                    if (i == coneVertices - 1 + offset)
                        tris[cnt++] = offset;
                    else
                        tris[cnt++] = i + 1;
                    tris[cnt++] = i;
                    tris[cnt++] = i + coneVertices;
                }
        }
        else
        {
            // truncated cone
            tris = new int[coneVertices * 6 * multiplier];
            if (outside)
                for (i = 0; i < coneVertices; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == coneVertices)
                        ip1 = 0;

                    tris[cnt++] = i;
                    tris[cnt++] = ip1;
                    tris[cnt++] = i + coneVertices;

                    tris[cnt++] = ip1 + coneVertices;
                    tris[cnt++] = i + coneVertices;
                    tris[cnt++] = ip1;
                }
            if (inside)
                for (i = offset; i < coneVertices + offset; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == coneVertices + offset)
                        ip1 = offset;

                    tris[cnt++] = ip1;
                    tris[cnt++] = i;
                    tris[cnt++] = i + coneVertices;

                    tris[cnt++] = i + coneVertices;
                    tris[cnt++] = ip1 + coneVertices;
                    tris[cnt++] = ip1;
                }
        }
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Cone.GetComponent<MeshFilter>().sharedMesh = mesh;
        Cone.GetComponent<MeshRenderer>().material = coneMaterial;

        Cone.transform.SetParent(transform, false);

        AddPlane();
    }

    public void CreateConeSplited()
    {
        CleanChildren();
        // if (openingAngle > 0 && openingAngle < 180)
        // {
        //     radiusTop = 0;
        //     radiusBottom = coneDepth * Mathf.Tan(openingAngle * Mathf.Deg2Rad / 2);
        // }
        GameObject Cone = new GameObject("cone");
        Cone.AddComponent<MeshFilter>();
        Cone.AddComponent<MeshRenderer>();

        int n_split = coneHorizontalSplit;
        int steps = 2 + n_split;

        Mesh mesh = new Mesh();
        int multiplier = (outside ? 1 : 0) + (inside ? 1 : 0); // 1 or 2, corresponding to sides shown (or nothing)
        int doubleside_offset = (outside && inside ? steps * coneVertices : 0); // offset in the vertices lists when draw triangles when 2 sides
        Vector3[] vertices = new Vector3[steps * multiplier * coneVertices]; // 0..n-1: top, n..2n-1: bottom  | 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        Vector3[] normals = new Vector3[steps * multiplier * coneVertices]; // 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        Vector2[] uvs = new Vector2[steps * multiplier * coneVertices]; // 2 * coneVertices (for the 2 bases top and bottom) * multiplier if 2 sides
        int[] tris; // triangles idx list

        if (coneRadiusBottom <= 0)
        {
            coneRadiusBottom = 0.01f;
        }

        float slope = Mathf.Atan((coneRadiusBottom - coneRadiusTop) / coneDepth); // (rad difference)/height TODO update in loop
        float slopeSin = Mathf.Sin(slope);
        float slopeCos = Mathf.Cos(slope);
        //int i;

        float offsetHeightDiff = coneDepth / (steps - 1);
        float radiusDiff = coneRadiusBottom - coneRadiusTop;

        for (int i = 0; i < coneVertices; i++)
        {
            float angle = 2 * Mathf.PI * i / coneVertices;
            float angleSin = Mathf.Sin(angle);
            float angleCos = Mathf.Cos(angle);
            float angleHalf = 2 * Mathf.PI * (i + 0.5f) / coneVertices; // for degenerated normals at cone tips
            float angleHalfSin = Mathf.Sin(angleHalf);
            float angleHalfCos = Mathf.Cos(angleHalf);

            //float prevRadius = coneRadiusBottom;
            for (int s = 0; s < steps; s++)
            {
                float coneDepthStep = offsetHeightDiff * s;
                float coneRadiusStep = ((radiusDiff * coneDepthStep) / coneDepth) + coneRadiusTop;
                int step_offset = coneVertices * s;

                // ---- vertices ----
                vertices[i + step_offset] = new Vector3(coneRadiusStep * angleCos, coneRadiusStep * angleSin, coneDepthStep);

                //vertices[i] = new Vector3(coneRadiusTop * angleCos, coneRadiusTop * angleSin, 0);
                //vertices[i + coneVertices] = new Vector3(coneRadiusBottom * angleCos, coneRadiusBottom * angleSin, coneDepth);

                // ---- normals ----
                if ((coneRadiusTop == 0 && s == 0) || (coneRadiusBottom == 0 && s == n_split - 1)) // coneRadiusTop == 0 and coneRadiusTop step or coneRadiusBottom == 0 and coneRadiusBottom step
                    normals[i + step_offset] = new Vector3(angleHalfCos * slopeCos, angleHalfSin * slopeCos, -slopeSin);
                else
                    normals[i + step_offset] = new Vector3(angleCos * slopeCos, angleSin * slopeCos, -slopeSin);

                // ---- uvs ----
                //uvs[i + step_offset] = new Vector2(1.0f * i / coneVertices, 1 - (coneDepthStep / coneDepth));
                float test = i / (coneVertices - 1.0f);
                uvs[i + step_offset] = new Vector2(1.0f - test, 1.0f - (coneDepthStep / coneDepth));

                if (outside && inside)
                {
                    // vertices and uvs are identical on inside and outside, so just copy
                    vertices[i + doubleside_offset + step_offset] = vertices[i + step_offset];
                    uvs[i + doubleside_offset + step_offset] = uvs[i + step_offset];
                }
                if (inside)
                {
                    // invert normals
                    normals[i + doubleside_offset + step_offset] = -normals[i + doubleside_offset + step_offset];
                }
            }

            // ---- vertices ----
            // vertices[i] = new Vector3(coneRadiusTop * angleCos, coneRadiusTop * angleSin, 0);
            // vertices[i + coneVertices] = new Vector3(coneRadiusBottom * angleCos, coneRadiusBottom * angleSin, coneDepth);

            // ---- normals ----
            // if (coneRadiusTop == 0)
            //     normals[i] = new Vector3(angleHalfCos * slopeCos, angleHalfSin * slopeCos, -slopeSin);
            // else
            //     normals[i] = new Vector3(angleCos * slopeCos, angleSin * slopeCos, -slopeSin);
            // if (coneRadiusBottom == 0)
            //     normals[i + coneVertices] = new Vector3(angleHalfCos * slopeCos, angleHalfSin * slopeCos, -slopeSin);
            // else
            //     normals[i + coneVertices] = new Vector3(angleCos * slopeCos, angleSin * slopeCos, -slopeSin);

            // ---- uvs ----
            // uvs[i] = new Vector2(1.0f * i / coneVertices, 1);
            // uvs[i + coneVertices] = new Vector2(1.0f * i / coneVertices, 0);

            // if (outside && inside)
            // {
            //     // vertices and uvs are identical on inside and outside, so just copy
            //     vertices[i + 2 * coneVertices] = vertices[i];
            //     vertices[i + 3 * coneVertices] = vertices[i + coneVertices];
            //     uvs[i + 2 * coneVertices] = uvs[i];
            //     uvs[i + 3 * coneVertices] = uvs[i + coneVertices];
            // }
            // if (inside)
            // {
            //     // invert normals
            //     normals[i + offset] = -normals[i];
            //     normals[i + coneVertices + offset] = -normals[i + coneVertices];
            // }
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;

        // create triangles
        int cnt = 0;
        if (coneRadiusTop == 0)
        {
            // top cone
            // tris = new int[coneVertices * 3 * multiplier];
            tris = new int[(coneVertices * 3 * multiplier) + (coneVertices * 6 * multiplier * n_split)];
            if (outside)
            {
                // top
                for (int i = 0; i < coneVertices; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == coneVertices)
                        ip1 = 0;

                    tris[cnt++] = i + coneVertices;
                    tris[cnt++] = i;
                    tris[cnt++] = ip1 + coneVertices;
                }
                // bottom
                for (int i = 0; i < coneVertices; i++)
                {
                    for (int s = 1; s < steps - 1; s++)
                    {
                        int step_offset = coneVertices * s;

                        int ip1 = i + 1;
                        if (ip1 == coneVertices)
                            ip1 = 0;

                        tris[cnt++] = step_offset + i;
                        tris[cnt++] = step_offset + ip1;
                        tris[cnt++] = step_offset + i + coneVertices;

                        tris[cnt++] = step_offset + ip1 + coneVertices;
                        tris[cnt++] = step_offset + i + coneVertices;
                        tris[cnt++] = step_offset + ip1;
                    }
                }
            }
            if (inside)
            {
                // top
                for (int i = doubleside_offset; i < coneVertices + doubleside_offset; i++)
                {
                    int ip1 = i + 1;
                    if (ip1 == coneVertices + doubleside_offset)
                        ip1 = doubleside_offset;

                    tris[cnt++] = i;
                    tris[cnt++] = i + coneVertices;
                    tris[cnt++] = ip1 + coneVertices;
                }
                // bottom
                for (int i = doubleside_offset; i < coneVertices + doubleside_offset; i++)
                {
                    for (int s = 1; s < steps - 1; s++)
                    {
                        int step_offset = coneVertices * s;

                        int ip1 = i + 1;
                        if (ip1 == coneVertices + doubleside_offset)
                            ip1 = doubleside_offset;

                        tris[cnt++] = step_offset + ip1;
                        tris[cnt++] = step_offset + i;
                        tris[cnt++] = step_offset + i + coneVertices;

                        tris[cnt++] = step_offset + i + coneVertices;
                        tris[cnt++] = step_offset + ip1 + coneVertices;
                        tris[cnt++] = step_offset + ip1;
                    }
                }
            }
        }
        // else if (coneRadiusBottom == 0)
        // {
        //     // bottom cone
        //     tris = new int[(coneVertices * 3 * multiplier) + (coneVertices * 6 * multiplier * n_split)];
        //     if (outside)
        //     {
        //         for (int i = 0; i < coneVertices; i++)
        //         {
        //             int ip1 = i + 1;
        //             if (ip1 == coneVertices)
        //                 ip1 = 0;

        //             tris[cnt++] = i;
        //             tris[cnt++] = ip1;
        //             tris[cnt++] = i + coneVertices;
        //         }
        //     }
        //     if (inside)
        //     {
        //         for (int i = doubleside_offset; i < coneVertices + doubleside_offset; i++)
        //         {
        //             int ip1 = i + 1;
        //             if (ip1 == coneVertices + doubleside_offset)
        //                 ip1 = doubleside_offset;

        //             tris[cnt++] = ip1;
        //             tris[cnt++] = i;
        //             tris[cnt++] = i + coneVertices;
        //         }
        //     }
        // }
        else
        {
            // truncated cone
            //tris = new int[coneVertices * 6 * multiplier];
            tris = new int[coneVertices * 6 * multiplier * (1 + n_split)];
            if (outside)
                for (int i = 0; i < coneVertices; i++)
                {
                    for (int s = 0; s < steps - 1; s++)
                    {
                        int step_offset = coneVertices * s;

                        int ip1 = i + 1;
                        if (ip1 == coneVertices)
                            ip1 = 0;

                        tris[cnt++] = step_offset + i;
                        tris[cnt++] = step_offset + ip1;
                        tris[cnt++] = step_offset + i + coneVertices;

                        tris[cnt++] = step_offset + ip1 + coneVertices;
                        tris[cnt++] = step_offset + i + coneVertices;
                        tris[cnt++] = step_offset + ip1;
                    }
                }
            if (inside)
                for (int i = doubleside_offset; i < coneVertices + doubleside_offset; i++)
                {
                    for (int s = 0; s < steps - 1; s++)
                    {
                        int step_offset = coneVertices * s;

                        int ip1 = i + 1;
                        if (ip1 == coneVertices + doubleside_offset)
                            ip1 = doubleside_offset;

                        tris[cnt++] = step_offset + ip1;
                        tris[cnt++] = step_offset + i;
                        tris[cnt++] = step_offset + i + coneVertices;

                        tris[cnt++] = step_offset + i + coneVertices;
                        tris[cnt++] = step_offset + ip1 + coneVertices;
                        tris[cnt++] = step_offset + ip1;
                    }
                }
        }
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Cone.GetComponent<MeshFilter>().sharedMesh = mesh;
        Cone.GetComponent<MeshRenderer>().material = coneMaterial;

        Cone.transform.SetParent(transform, false);

        AddPlane();
    }

    void AddPlane()
    {
        float offsetHeightDiff = coneDepth / (nPlanes - 1);
        float radiusDiff = coneRadiusBottom - coneRadiusTop;

        for (int i = 0; i < nPlanes; i++)
        {
            GameObject Plane = new GameObject("plane_" + i);
            Plane.AddComponent<MeshFilter>();
            Plane.AddComponent<MeshRenderer>();

            float depth = offsetHeightDiff * i;
            float r = ((radiusDiff * depth) / coneDepth) + coneRadiusTop;
            float side = (r * 2) * 1.1f;
            float offsetSide = -(side / 2);

            Mesh mesh = new Mesh();
            // mesh.vertices = new Vector3[]
            // {
            // new Vector3(offsetSide,         offsetSide,         depth),
            // new Vector3(offsetSide + side,  offsetSide,         depth),
            // new Vector3(offsetSide + side,  offsetSide + side,  depth),
            // new Vector3(offsetSide,         offsetSide + side,  depth)
            // };

            mesh.vertices = new Vector3[]
            {
            new Vector3(offsetSide,         offsetSide,         depth),
            new Vector3(offsetSide + side,  offsetSide,         depth),
            new Vector3(offsetSide + side,  offsetSide + side,  depth),
            new Vector3(offsetSide,         offsetSide + side,  depth),
            new Vector3(offsetSide,         offsetSide,         depth),
            new Vector3(offsetSide + side,  offsetSide,         depth),
            new Vector3(offsetSide + side,  offsetSide + side,  depth),
            new Vector3(offsetSide,         offsetSide + side,  depth)
            };

            // mesh.uv = new Vector2[]{
            // new Vector2 (0, 0),
            // new Vector2 (0, 1),
            // new Vector2 (1, 1),
            // new Vector2 (1, 0)
            // };

            mesh.normals = new Vector3[]
            {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1)
            };

            mesh.uv = new Vector2[]{
            new Vector2 (0, 0),
            new Vector2 (0, 1),
            new Vector2 (1, 1),
            new Vector2 (1, 0),
            new Vector2 (0, 0),
            new Vector2 (0, 1),
            new Vector2 (1, 1),
            new Vector2 (1, 0)
            };

            //mesh.triangles = new int[] { 0, 3, 2, 0, 2, 1 };
            mesh.triangles = new int[] { 0, 3, 2, 0, 2, 1, 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            Plane.GetComponent<MeshFilter>().sharedMesh = mesh;
            Plane.GetComponent<MeshRenderer>().material = planesMaterial;
            Plane.transform.SetParent(transform, false);
        }
    }

    void CleanChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
