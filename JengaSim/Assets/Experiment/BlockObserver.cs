using System;
using System.Collections.Generic; 
using UnityEngine;

public class BlockObserver : MonoBehaviour
{
    public BlockState StartBlockState;
    public List<BlockState> Neighbors;
    
    private Rigidbody rigidBody;
    private float startTime;

    public event EventHandler<MotionFinishedEventArgs> MotionFinishedEvent;
    public bool finished = false;

    public void Initialize(BlockState startBlockState, List<BlockState> neighbors)
    {
        this.StartBlockState = startBlockState;
        this.Neighbors = neighbors;
        this.rigidBody = this.GetComponent<Rigidbody>();
        this.startTime = Time.time;
    }

    public void FixedUpdate()
    {
        if (finished) return;
        if (Time.time - startTime < 0.04) return;
        if (rigidBody.linearVelocity.magnitude != 0 || rigidBody.angularVelocity.magnitude != 0) return;
        
        OnFinish(TestState.PASS);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (finished) return;
        if (StartBlockState.index <= 0) return; // excluding base blocks (index = 0)
         
        if (collision.gameObject.name == "Plane")
        {
            OnFinish(TestState.PLANE);
            return;
        }

        var collisionAttribute = collision.gameObject.GetComponent<BlockObserver>().StartBlockState;
        
        if (StartBlockState.index > collisionAttribute.index && !Neighbors.Contains(collisionAttribute))
        {
            OnFinish(TestState.BLOCK);
        }
    }

    public void OnFinish(TestState state)
    {
        finished = true;
        BlockState onFallBlockState = new(StartBlockState.index, StartBlockState.block, StartBlockState.axis)
        {
            pos = transform.localPosition,
            rotation = transform.localRotation,
            time = Time.time - startTime,
            angularVelocity = rigidBody.angularVelocity.magnitude,
            linearVelocity = rigidBody.linearVelocity.magnitude,
            maxAngularVelocity = rigidBody.maxAngularVelocity,
            maxLinearVelocity = rigidBody.maxLinearVelocity,
            testState = state
        };

        MotionFinishedEvent?.Invoke(this, new(onFallBlockState));
    }
}

public class BlockState : IEquatable<BlockState>
{
    public readonly int index;  // identity
    public readonly byte block; // identity (one bit is 1)
    public readonly Axis axis;  // identity
    
    public Vector3 pos;
    public Quaternion rotation;
    public float time;
    public float linearVelocity;
    public float maxLinearVelocity;
    public float angularVelocity;
    public float maxAngularVelocity;
    public TestState testState;

    public BlockState(int index, byte block, Axis axis)
    {
        this.index = index;
        this.block = block;
        this.axis = axis;
    }

    public bool Equals(BlockState other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return index == other.index && block == other.block && axis == other.axis;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BlockState)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(index, block, axis);
    }
}

public enum TestState
{
    RUNNING = 0,
    PLANE = 1,
    BLOCK = 2,
    PASS = 3,
}

public static class TestStateExt
{
    public static string ToString(this TestState state)
    {
        switch (state)
        {
            case TestState.RUNNING:
                return "Running";
            case TestState.PLANE:
                return "Plane";
            case TestState.BLOCK:
                return "Block";
            case TestState.PASS:
                return "Pass";
            default:
                return null;
        }
    }
}

public class MotionFinishedEventArgs : EventArgs
{
    public BlockState BlockState { get; }

    public MotionFinishedEventArgs(BlockState blockState)
    {
        this.BlockState = blockState;
    }
}