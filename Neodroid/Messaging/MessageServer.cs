﻿using System;
using System.Threading;
using AsyncIO;
using FlatBuffers;
using Neodroid.FBS.Reaction;
using Neodroid.Messaging.FBS;
using Neodroid.Messaging.Messages;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

namespace Neodroid.Messaging {
  [Serializable]
  public class MessageServer {
    #region PublicMembers

    public bool _Client_Connected;

    #endregion

    #region PrivateMembers

    Thread _polling_thread;
    Thread _wait_for_client_thread;
     object _thread_lock = new object();
    bool _stop_thread;
    bool _waiting_for_main_loop_to_send;
     bool _use_inter_process_communication;
    bool _debugging;

     ResponseSocket _socket;

    //PairSocket _socket;
     string _ip_address;
     int _port;
    byte[] _byte_buffer;

    #endregion

    #region PrivateMethods

    #region Threads

    void WaitForClientToConnect(Action callback, Action<String> debug_callback) {
      if(this._debugging) debug_callback?.Invoke("Start listening for client");
      try {
        if (this._use_inter_process_communication)
          this._socket.Bind("ipc:///tmp/neodroid/messages");
        else
          this._socket.Bind("tcp://" + this._ip_address + ":" + this._port);
        
        callback?.Invoke();
        if(this._debugging) debug_callback?.Invoke("Now listening for client");
        this._Client_Connected = true;
      } catch (Exception e) {
        if(this._debugging) debug_callback?.Invoke(e.Message);
      }

    }

    public Reaction Receive(TimeSpan wait_time) {
      Reaction reaction = null;
      try {
        byte[] msg;
        if (wait_time > TimeSpan.Zero)
          this._socket.TryReceiveFrameBytes(wait_time, out msg);
        else
          msg = this._socket.ReceiveFrameBytes();

        if (msg != null && msg.Length > 0) {
          var flat_reaction = FReaction.GetRootAsFReaction(new ByteBuffer(msg));
          reaction = FbsReactionUtilities.unpack_reaction(flat_reaction);
        }
      } catch (Exception err) {
        if (err is TerminatingException)
          return reaction;
        Debug.Log(err);
      }

      return reaction;
    }

    void PollingThread(
        Action<Reaction> receive_callback,
        Action disconnect_callback,
        Action<string> debug_callback) {
      while (this._stop_thread == false) {
        if (!this._waiting_for_main_loop_to_send) {
          var reaction = this.Receive(TimeSpan.FromSeconds(2));
          if (reaction != null) {
            receive_callback(reaction);
            this._waiting_for_main_loop_to_send = true;
          }
        } else {
          if (this._debugging) debug_callback("Waiting for main loop to send reply");
        }
      }

      disconnect_callback();
      if (this._use_inter_process_communication)
        this._socket.Disconnect("inproc://neodroid");
      else
        this._socket.Disconnect("tcp://" + this._ip_address + ":" + this._port);
      try {
        this._socket.Dispose();
        this._socket.Close();
      } finally {
        NetMQConfig.Cleanup(false);
      }
    }

    #endregion

    #endregion

    #region PublicMethods

    public void SendStates(EnvironmentState[] environment_states) {
      this._byte_buffer = FbsStateUtilities.build_states(environment_states);
      this._socket.SendFrame(this._byte_buffer);
      this._waiting_for_main_loop_to_send = false;
    }

    public void ListenForClientToConnect(Action<string> debug_callback) { this.WaitForClientToConnect(null,debug_callback); }

    public void ListenForClientToConnect(Action callback, Action<string> debug_callback) {
      this._wait_for_client_thread =
          new Thread(unused_param => this.WaitForClientToConnect(callback, debug_callback)) {IsBackground = true};
      // Is terminated with foreground threads, when they terminate
      this._wait_for_client_thread.Start();
    }

    public void StartReceiving(
        Action<Reaction> cmd_callback,
        Action disconnect_callback,
        Action<string> debug_callback) {
      this._polling_thread =
          new Thread(unused_param => this.PollingThread(cmd_callback, disconnect_callback, debug_callback)) {
              IsBackground = true
          };
      // Is terminated with foreground threads, when they terminate
      this._polling_thread.Start();
    }

    #region Contstruction

    public MessageServer(
        string ip_address = "127.0.0.1",
        int port = 5555,
        bool use_inter_process_communication = false,
        bool debug = false) {
      this.Debugging = debug;
      this._ip_address = ip_address;
      this._port = port;
      this._use_inter_process_communication = use_inter_process_communication;
      if (!this._use_inter_process_communication)
        ForceDotNet.Force();
      this._socket = new ResponseSocket();
    }

    public MessageServer(bool debug = false) : this("127.0.0.1", 5555, false, debug) { }

    #endregion

    #region Getters

    public bool Debugging { get { return this._debugging; } set { this._debugging = value; } }

    #endregion

    #endregion

    #region Deconstruction

    public void Destroy() { this.CleanUp(); }

    public void CleanUp() {
      try {
        lock (this._thread_lock) this._stop_thread = true;

        if (this._use_inter_process_communication)
          this._socket.Disconnect("ipc:///tmp/neodroid/messages");
        else
          this._socket.Disconnect("tcp://" + this._ip_address + ":" + this._port);
        try {
          this._socket.Dispose();
          this._socket.Close();
        } finally {
          NetMQConfig.Cleanup(false);
        }

        this._wait_for_client_thread?.Join();
        this._polling_thread?.Join();
      } catch {
        Console.WriteLine("Exception thrown while killing threads");
      }
    }

    #endregion
  }
}