﻿/****************************************************************************
 Copyright (c) 2014 Martin Ysa

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 ****************************************************************************/

using System.Collections.Generic;
using SceneAssets.Hide.SharpShadowLight.Scripts.Helpers;
using UnityEngine;

namespace SceneAssets.Hide.SharpShadowLight.Scripts.Light {
  // This allows for the use of lists, like <GameObject>
  //using UnityEngine; using UnityEditor; using pseudoSinCos;

  public class Verts {
    public float Angle { get; set; }
    public int Location { get; set; } // 1= left end point    0= middle     -1=right endpoint
    public Vector3 Pos { get; set; }
    public bool Endpoint { get; set; }
  }

  public class DynamicLight : MonoBehaviour {
    // Private variables
    Mesh _light_mesh; // Mesh for our light mesh
    [HideInInspector] public PolygonCollider2D[] _All_Meshes; // Array for all of the meshes in our scene

    [HideInInspector]
    public List<Verts> _All_Vertices = new List<Verts>(); // Array for all of the vertices in our meshes

    public LayerMask _Layer;

    public Material _Light_Material;

    [SerializeField] public float _Light_Radius = 20f;

    [Range(4, 20)] public int _Light_Segments = 8;

    // Public variables

    public string _Version = "1.0.5"; //release date 09/01/2017

    // Called at beginning of script execution
    void Start() {
      TableSinCos.Init();

      //Debug.Log((int) LayerMask.NameToLayer("Default"));

      //-- Step 1: obtain all active meshes in the scene --//
      //---------------------------------------------------------------------//

      var mesh_filter =
          (MeshFilter)this.gameObject.AddComponent(
              typeof(MeshFilter)); // Add a Mesh Filter component to the light game object so it can take on a form
      var renderer =
          this.gameObject.AddComponent(
              typeof(MeshRenderer)) as MeshRenderer; // Add a Mesh Renderer component to the light game object so the form can become visible
      //gameObject.name = "2DLight";
      //renderer.material.shader = Shader.Find ("Transparent/Diffuse");							// Find the specified type of material shader
      renderer.sharedMaterial = this._Light_Material; // Add this texture
      this._light_mesh = new Mesh(); // create a new mesh for our light mesh
      mesh_filter.mesh = this._light_mesh; // Set this newly created mesh to the mesh filter
      this._light_mesh.name = "Light Mesh"; // Give it a name
      this._light_mesh.MarkDynamic();
    }

    void Update() {
      this.GetAllMeshes();
      this.SetLight();
      this.RenderLightMesh();
      this.ResetBounds();
    }

    void GetAllMeshes() {
      //allMeshes = FindObjectsOfType(typeof(PolygonCollider2D)) as PolygonCollider2D[];

      var all_coll2_d = Physics2D.OverlapCircleAll(this.transform.position, this._Light_Radius, this._Layer);
      this._All_Meshes = new PolygonCollider2D[all_coll2_d.Length];

      for (var i = 0; i < all_coll2_d.Length; i++) this._All_Meshes[i] = (PolygonCollider2D)all_coll2_d[i];
    }

    void ResetBounds() {
      var b = this._light_mesh.bounds;
      b.center = Vector3.zero;
      this._light_mesh.bounds = b;
    }

    void SetLight() {
      var sort_angles = false;

      this._All_Vertices
          .Clear(); // Since these lists are populated every frame, clear them first to prevent overpopulation

      //layer = 1 << 8;

      //--Step 2: Obtain vertices for each mesh --//
      //---------------------------------------------------------------------//

      // las siguientes variables usadas para arregla bug de ordenamiento cuando
      // los angulos calcuados se encuentran en cuadrantes mixtos (1 y 4)
      var lows = false; // check si hay menores a -0.5
      var his = false; // check si hay mayores a 2.0
      var mag_range = 0.15f;

      var temp_verts = new List<Verts>();

      for (var m = 0; m < this._All_Meshes.Length; m++) {
        //for (int m = 0; m < 1; m++) {
        temp_verts.Clear();
        var mf = this._All_Meshes[m];

        // las siguientes variables usadas para arregla bug de ordenamiento cuando
        // los angulos calcuados se encuentran en cuadrantes mixtos (1 y 4)
        lows = false; // check si hay menores a -0.5
        his = false; // check si hay mayores a 2.0

        if (((1 << mf.transform.gameObject.layer) & this._Layer) != 0) {
          for (var i = 0; i < mf.GetTotalPointCount(); i++) {
            // ...and for ever vertex we have of each mesh filter...

            var v = new Verts();
            // Convert to world space
            var world_point = mf.transform.TransformPoint(mf.points[i]);

            // Reforma fecha 24/09/2014 (ultimo argumento lighradius X worldPoint.magnitude (expensivo pero preciso))
            var ray = Physics2D.Raycast(
                this.transform.position,
                world_point - this.transform.position,
                (world_point - this.transform.position).magnitude,
                this._Layer);

            if (ray) {
              v.Pos = ray.point;
              if (world_point.sqrMagnitude >= ray.point.sqrMagnitude - mag_range
                  && world_point.sqrMagnitude <= ray.point.sqrMagnitude + mag_range)
                v.Endpoint = true;
            } else {
              v.Pos = world_point;
              v.Endpoint = true;
            }

            Debug.DrawLine(this.transform.position, v.Pos, Color.white);

            //--Convert To local space for build mesh (mesh craft only in local vertex)
            v.Pos = this.transform.InverseTransformPoint(v.Pos);
            //--Calculate angle
            v.Angle = this.GetVectorAngle(true, v.Pos.x, v.Pos.y);

            // -- bookmark if an angle is lower than 0 or higher than 2f --//
            //-- helper method for fix bug on shape located in 2 or more quadrants
            if (v.Angle < 0f)
              lows = true;

            if (v.Angle > 2f)
              his = true;

            //--Add verts to the main array
            if (v.Pos.sqrMagnitude <= this._Light_Radius * this._Light_Radius) temp_verts.Add(v);

            if (sort_angles == false)
              sort_angles = true;
          }
        }

        // Indentify the endpoints (left and right)
        if (temp_verts.Count > 0) {
          this.SortList(temp_verts); // sort first

          var pos_low_angle = 0; // save the indice of left ray
          var pos_high_angle = 0; // same last in right side

          //Debug.Log(lows + " " + his);

          if (his && lows) { //-- FIX BUG OF SORTING CUANDRANT 1-4 --//
            var lowest_angle = -1f; //tempVerts[0].angle; // init with first data
            var highest_angle = temp_verts[0].Angle;

            for (var d = 0; d < temp_verts.Count; d++) {
              if (temp_verts[d].Angle < 1f && temp_verts[d].Angle > lowest_angle) {
                lowest_angle = temp_verts[d].Angle;
                pos_low_angle = d;
              }

              if (temp_verts[d].Angle > 2f && temp_verts[d].Angle < highest_angle) {
                highest_angle = temp_verts[d].Angle;
                pos_high_angle = d;
              }
            }
          } else {
            //-- convencional position of ray points
            // save the indice of left ray
            pos_low_angle = 0;
            pos_high_angle = temp_verts.Count - 1;
          }

          temp_verts[pos_low_angle].Location = 1; // right
          temp_verts[pos_high_angle].Location = -1; // left

          //--Add vertices to the main meshes vertexes--//
          this._All_Vertices.AddRange(temp_verts);
          //allVertices.Add(tempVerts[0]);
          //allVertices.Add(tempVerts[tempVerts.Count - 1]);

          // -- r ==0 --> right ray
          // -- r ==1 --> left ray
          for (var r = 0; r < 2; r++) {
            //-- Cast a ray in same direction continuos mode, start a last point of last ray --//
            var from_cast = new Vector3();
            var is_endpoint = false;

            if (r == 0) {
              from_cast = this.transform.TransformPoint(temp_verts[pos_low_angle].Pos);
              is_endpoint = temp_verts[pos_low_angle].Endpoint;
            } else if (r == 1) {
              from_cast = this.transform.TransformPoint(temp_verts[pos_high_angle].Pos);
              is_endpoint = temp_verts[pos_high_angle].Endpoint;
            }

            if (is_endpoint) {
              var from = from_cast;
              var dir = from - this.transform.position;

              var mag = this._Light_Radius; // - fromCast.magnitude;
              const float check_point_last_ray_offset = 0.005f;

              from += dir * check_point_last_ray_offset;

              var ray_cont = Physics2D.Raycast(from, dir, mag, this._Layer);
              Vector3 hitp;
              if (ray_cont)
                hitp = ray_cont.point;
              else {
                Vector2 new_dir = this.transform.InverseTransformDirection(dir); //local p
                hitp = (Vector2)this.transform.TransformPoint(new_dir.normalized * mag); //world p
              }

              if (((Vector2)hitp - (Vector2)this.transform.position).sqrMagnitude
                  > this._Light_Radius * this._Light_Radius) {
                dir = (Vector2)this.transform.InverseTransformDirection(dir); //local p
                hitp = (Vector2)this.transform.TransformPoint(dir.normalized * mag);
              }

              Debug.DrawLine(from_cast, hitp, Color.green);

              var v_l = new Verts();
              v_l.Pos = this.transform.InverseTransformPoint(hitp);

              v_l.Angle = this.GetVectorAngle(true, v_l.Pos.x, v_l.Pos.y);
              this._All_Vertices.Add(v_l);
            }
          }
        }
      }

      //--Step 3: Generate vectors for light cast--//
      //---------------------------------------------------------------------//

      var theta = 0;
      //float amount = (Mathf.PI * 2) / lightSegments;
      var amount = 360 / this._Light_Segments;

      for (var i = 0; i < this._Light_Segments; i++) {
        theta = amount * i;
        if (theta == 360) theta = 0;

        var v = new Verts();
        //v.pos = new Vector3((Mathf.Sin(theta)), (Mathf.Cos(theta)), 0); // in radians low performance
        v.Pos = new Vector3(
            TableSinCos._Sen_Array[theta],
            TableSinCos._Cos_Array[theta],
            0); // in dregrees (previous calculate)

        v.Angle = this.GetVectorAngle(true, v.Pos.x, v.Pos.y);
        v.Pos *= this._Light_Radius;
        v.Pos += this.transform.position;

        var ray = Physics2D.Raycast(
            this.transform.position,
            v.Pos - this.transform.position,
            this._Light_Radius,
            this._Layer);
        //Debug.DrawRay(transform.position, v.pos - transform.position, Color.white);

        if (!ray) {
          //Debug.DrawLine(transform.position, v.pos, Color.white);

          v.Pos = this.transform.InverseTransformPoint(v.Pos);
          this._All_Vertices.Add(v);
        }
      }

      //-- Step 4: Sort each vertice by angle (along sweep ray 0 - 2PI)--//
      //---------------------------------------------------------------------//
      if (sort_angles) this.SortList(this._All_Vertices);
      //-----------------------------------------------------------------------------

      //--auxiliar step (change order vertices close to light first in position when has same direction) --//
      var range_angle_comparision = 0.00001f;
      for (var i = 0; i < this._All_Vertices.Count - 1; i += 1) {
        var uno = this._All_Vertices[i];
        var dos = this._All_Vertices[i + 1];

        // -- Comparo el angulo local de cada vertex y decido si tengo que hacer un exchange-- //
        if (uno.Angle >= dos.Angle - range_angle_comparision
            && uno.Angle <= dos.Angle + range_angle_comparision) {
          if (dos.Location == -1) { // Right Ray

            if (uno.Pos.sqrMagnitude > dos.Pos.sqrMagnitude) {
              this._All_Vertices[i] = dos;
              this._All_Vertices[i + 1] = uno;
              //Debug.Log("changing left");
            }
          }

          // ALREADY DONE!!
          if (uno.Location == 1) { // Left Ray
            if (uno.Pos.sqrMagnitude < dos.Pos.sqrMagnitude) {
              this._All_Vertices[i] = dos;
              this._All_Vertices[i + 1] = uno;
              //Debug.Log("changing");
            }
          }
        }
      }
    }

    void RenderLightMesh() {
      //-- Step 5: fill the mesh with vertices--//
      //---------------------------------------------------------------------//

      //interface_touch.vertexCount = allVertices.Count; // notify to UI

      var init_vertices_mesh_light = new Vector3[this._All_Vertices.Count + 1];

      init_vertices_mesh_light[0] = Vector3.zero;

      for (var i = 0; i < this._All_Vertices.Count; i++) //Debug.Log(allVertices[i].angle);
        init_vertices_mesh_light[i + 1] = this._All_Vertices[i].Pos;

      //if(allVertices[i].endpoint == true)
      //Debug.Log(allVertices[i].angle);

      this._light_mesh.Clear();
      this._light_mesh.vertices = init_vertices_mesh_light;

      var uvs = new Vector2[init_vertices_mesh_light.Length];
      for (var i = 0; i < init_vertices_mesh_light.Length; i++)
        uvs[i] = new Vector2(init_vertices_mesh_light[i].x, init_vertices_mesh_light[i].y);
      this._light_mesh.uv = uvs;

      // triangles
      var idx = 0;
      var triangles = new int[this._All_Vertices.Count * 3];
      for (var i = 0; i < this._All_Vertices.Count * 3; i += 3) {
        triangles[i] = 0;
        triangles[i + 1] = idx + 1;

        if (i == this._All_Vertices.Count * 3 - 3) {
          //-- if is the last vertex (one loop)
          triangles[i + 2] = 1;
        } else
          triangles[i + 2] = idx + 2; //next next vertex	

        idx++;
      }

      this._light_mesh.triangles = triangles;
      //lightMesh.RecalculateNormals();
      this.GetComponent<Renderer>().sharedMaterial = this._Light_Material;
    }

    void SortList(List<Verts> lista) { lista.Sort((item1, item2) => item2.Angle.CompareTo(item1.Angle)); }

    void DrawLinePerVertex() {
      for (var i = 0; i < this._All_Vertices.Count; i++) {
        if (i < this._All_Vertices.Count - 1) {
          Debug.DrawLine(
              this._All_Vertices[i].Pos,
              this._All_Vertices[i + 1].Pos,
              new Color(i * 0.02f, i * 0.02f, i * 0.02f));
        } else {
          Debug.DrawLine(
              this._All_Vertices[i].Pos,
              this._All_Vertices[0].Pos,
              new Color(i * 0.02f, i * 0.02f, i * 0.02f));
        }
      }
    }

    float GetVectorAngle(bool pseudo, float x, float y) {
      float ang = 0;
      if (pseudo)
        ang = this.PseudoAngle(x, y);
      else
        ang = Mathf.Atan2(y, x);
      return ang;
    }

    float PseudoAngle(float dx, float dy) {
      // Hight performance for calculate angle on a vector (only for sort)
      // APROXIMATE VALUES -- NOT EXACT!! //
      var ax = Mathf.Abs(dx);
      var ay = Mathf.Abs(dy);
      var p = dy / (ax + ay);
      if (dx < 0) p = 2 - p;
      return p;
    }
  }
}