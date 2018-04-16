﻿using System.Collections;
using Neodroid.Environments;
using Neodroid.Prototyping.Actors;
using Neodroid.Prototyping.Resetables;
using Neodroid.Utilities;
using UnityEngine;

namespace SceneAssets.LunarLander.Scripts {
  public class CollisionExplosion : Resetable {
    public Actor _Actor;
    GameObject _broken_object;
    public GameObject _Broken_Object_Prefab;

    float _delay = 2f;

    public ParticleSystem _Explosion;
    public float _Explosion_Force = 50;
    public GameObject _Explosion_Prefab;
    bool _has_exploded;
    public Rigidbody _Rigidbody;
    public float _Threshold = 150;

    public bool _Debugging;

    public override string ResetableIdentifier { get { return this.name; } }

    void Start() {
      if (!this._Rigidbody)
        this._Rigidbody = this.GetComponent<Rigidbody>();
      this._Explosion = this.GetComponent<ParticleSystem>();

      NeodroidUtilities.RegisterCollisionTriggerCallbacksOnChildren(
          this,
          this._Rigidbody.transform,
          this.ChildOnCollisionEnter,
          this.ChildOnTriggerEnter,
          null,
          null,
          null,
          null,
          this._Debugging);
    }

    void ChildOnCollisionEnter(GameObject child, Collision col) {
      if (this._Debugging)
        print("Collision");
      if (!col.collider.isTrigger)
        this.De(child.GetComponent<Rigidbody>(), col.collider.attachedRigidbody);
    }

    void ChildOnTriggerEnter(GameObject child, Collider col) {
      if (this._Debugging)
        print("Trigger colliding");
      if (!col.isTrigger)
        this.De(child.GetComponent<Rigidbody>(), col.attachedRigidbody);
    }

    void De(Rigidbody rb, Rigidbody other = null) {
      var val = 0f;
      if (rb != null)
        val = NeodroidUtilities.KineticEnergy(rb);
      var val_other = 0f;
      if (other != null)
        val_other = NeodroidUtilities.KineticEnergy(rb);
      if (this._Debugging)
        print($"{val} {val_other}");
      if ((val >= this._Threshold || val_other >= this._Threshold) && !this._has_exploded) {
        this._Actor.Kill();
        this._has_exploded = true;
        if (this._Explosion) {
          this._Explosion.Play();
          this._delay = this._Explosion.main.duration;
        }

        this.StartCoroutine(
            this.SpawnBroken(
                this._delay,
                this._Rigidbody.transform.parent,
                this._Rigidbody.transform.position,
                this._Rigidbody.transform.rotation,
                this._Rigidbody.velocity,
                this._Rigidbody.angularVelocity));
        this._Rigidbody.gameObject.SetActive(false);
        this._Rigidbody.Sleep();
      }
    }

    public IEnumerator SpawnBroken(
        float wait_time,
        Transform parent,
        Vector3 pos,
        Quaternion rot,
        Vector3 vel,
        Vector3 ang) {
      var explosion = Instantiate(this._Explosion_Prefab, pos, rot, parent);
      this._broken_object = Instantiate(this._Broken_Object_Prefab, pos, rot, parent);
      var rbs = this._broken_object.GetComponentsInChildren<Rigidbody>();
      foreach (var rb in rbs) {
        rb.velocity = vel;
        rb.angularVelocity = ang;
        rb.AddForceAtPosition((pos - rb.transform.position) * this._Explosion_Force, pos);
      }

      yield return new WaitForSeconds(wait_time);
      Destroy(explosion);
      this._Parent_Environment.Terminate("Actor exploded");
    }

    public override void Reset() {
      if (this._broken_object)
        Destroy(this._broken_object);
      if (this._Rigidbody) {
        this._Rigidbody.WakeUp();
        this._Rigidbody.gameObject.SetActive(true);
      }

      this._has_exploded = false;
    }
  }
}