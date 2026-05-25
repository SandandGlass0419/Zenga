using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockState : MonoBehaviour
{
    public BlockAttribute BlockAttribute { get; set; }
    public List<BlockAttribute> Neighbors { get; set; }
    
    private Rigidbody rigidBody;
    private float startTime;
    
    public event EventHandler<MotionFinishedEventArgs> MotionFinishedEvent;
    private bool finished = false;

    public void Initialize(BlockAttribute blockAttribute, List<BlockAttribute> neighbors)
    {
        this.BlockAttribute = blockAttribute;
        this.Neighbors = neighbors;
        this.rigidBody = this.GetComponent<Rigidbody>();
        this.startTime = Time.time;
    }

    public void FixedUpdate()
    {
        if (finished) return;
        if (Time.time - startTime < 0.1) return;
        if (rigidBody.linearVelocity.magnitude != 0 || rigidBody.angularVelocity.magnitude != 0) return;
        
        OnFinish("ok");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (finished) return;
        if (BlockAttribute.isSupport) return;
         
        if (collision.gameObject.name == "Plane")
        {
            OnFinish("plane");
            return;
        }

        var collisionAttribute = collision.gameObject.GetComponent<BlockState>().BlockAttribute;
        
        if (BlockAttribute.index > collisionAttribute.index && !Neighbors.Contains(collisionAttribute))
        {
            OnFinish("block");
        }
    }

    public void OnFinish(string state)
    {
        finished = true;
        Snapshot onFallSnapshot = new();
        
        onFallSnapshot.time = Time.time - startTime;
        onFallSnapshot.pos = this.transform.localPosition;
        onFallSnapshot.linearVelocity = rigidBody.linearVelocity.magnitude;
        onFallSnapshot.maxLinearVelocity = rigidBody.maxLinearVelocity;
        onFallSnapshot.rotation = this.transform.localRotation;
        onFallSnapshot.angularVelocity = rigidBody.angularVelocity.magnitude;
        onFallSnapshot.maxLinearVelocity = rigidBody.maxAngularVelocity;
        onFallSnapshot.state = state;
        
        MotionFinishedEvent?.Invoke(this, new(onFallSnapshot));
    }
}

public class Snapshot
{
    public float time;
    public Vector3 pos;
    public float linearVelocity;
    public float maxLinearVelocity;
    public Quaternion rotation;
    public float angularVelocity;
    public float maxAngularVelocity;
    public string state;
}

public class MotionFinishedEventArgs : EventArgs
{
    public Snapshot Snapshot { get; }

    public MotionFinishedEventArgs(Snapshot snapshot)
    {
        this.Snapshot = snapshot;
    }
}