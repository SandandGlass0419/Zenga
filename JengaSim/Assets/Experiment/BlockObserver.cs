using System;
using System.Collections.Generic; 
using UnityEngine;

public class BlockObserver : MonoBehaviour
{
    public BlockState StartBlockState { get; set; }
    public List<BlockState> Neighbors { get; set; }
    
    private Rigidbody rigidBody;
    private float startTime;

    public event EventHandler<MotionFinishedEventArgs> MotionFinishedEvent;
    private bool finished = false;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (finished) return;
        if (StartBlockState.index <= 0) return;
         
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
        BlockState onFallBlockState = new(StartBlockState.index, StartBlockState.axis, transform.localPosition, transform.rotation)
        {
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
    public int index { get; }
    public Axis axis { get; }
    
    public Vector3 pos { get; }
    public Quaternion rotation { get; }
    
    public float time { get; set; }
    public float linearVelocity { get; set; }
    public float maxLinearVelocity { get; set; }
    public float angularVelocity { get; set; }
    public float maxAngularVelocity { get; set; }
    public TestState testState { get; set; }

    public BlockState( int index, Axis axis, Vector3 pos, Quaternion rotation)
    {
        this.index = index;
        this.axis = axis;
        this.pos = pos;
        this.rotation = rotation;
    }

    public bool Equals(BlockState other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return pos.Equals(other.pos) && rotation.Equals(other.rotation) && index == other.index && axis == other.axis;
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
        return HashCode.Combine(pos, rotation, index, axis);
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