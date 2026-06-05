using System;
using System.Collections.Generic;
using Balancing;
using UnityEngine;

namespace Experiment
{
    public class BlockObserver : MonoBehaviour
    {
        public BlockState StartBlockState;
        public List<BlockState> Neighbors;
    
        private Rigidbody rigidBody;
        private float startFixedTime;

        public event EventHandler<ActionEventArgs> MotionFinishedEvent;
        public bool sleeping = false;

        public void Initialize(BlockState startBlockState, List<BlockState> neighbors)
        {
            this.StartBlockState = startBlockState;
            this.Neighbors = neighbors;
            this.rigidBody = this.GetComponent<Rigidbody>();
            this.startFixedTime = Time.fixedTime;
        }

        public void FixedUpdate()
        {
            if (Time.fixedTime - startFixedTime <= 0.04) return;

            if (!sleeping && rigidBody.IsSleeping())
            {
                sleeping = true;
                OnAction(TestStates.SLEEP);
            }
            else if (sleeping && !rigidBody.IsSleeping())
            {
                sleeping = false;
                OnAction(TestStates.AWAKE);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (StartBlockState.index <= 0) return; // excluding base blocks (index = 0)
         
            if (collision.gameObject.name == "Plane")
            {
                OnAction(TestStates.PLANE);
                return;
            }

            var collidingBlock = collision.gameObject.GetComponent<BlockObserver>().StartBlockState;
        
            if (StartBlockState.index > collidingBlock.index && !Neighbors.Contains(collidingBlock))
            {
                OnAction(TestStates.BLOCK);
            }
        }

        public void OnAction(TestStates state)
        {
            BlockState onFallBlockState = new(StartBlockState.index, StartBlockState.block, StartBlockState.axis)
            {
                pos = transform.localPosition,
                rotation = transform.localRotation,
                fixedTime = Time.fixedTime - startFixedTime,
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
        public float fixedTime;
        public float linearVelocity;
        public float maxLinearVelocity;
        public float angularVelocity;
        public float maxAngularVelocity;
        public TestStates testState;

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

    public enum TestStates
    {
        RUNNING = 0,
        PLANE = 1,
        BLOCK = 2,
        SLEEP = 3,
        AWAKE = 4,
    }

    public static class TestStateExt
    {
        public static string ToString(this TestStates state)
        {
            switch (state)
            {
                case TestStates.RUNNING:
                    return "Running";
                case TestStates.PLANE:
                    return "Plane";
                case TestStates.BLOCK:
                    return "Block";
                case TestStates.SLEEP:
                    return "Sleep";
                case TestStates.AWAKE:
                    return "Awake";
                default:
                    return null;
            }
        }
    }

    public class ActionEventArgs : EventArgs
    {
        public BlockState BlockState { get; }

        public ActionEventArgs(BlockState blockState)
        {
            this.BlockState = blockState;
        }
    }
}