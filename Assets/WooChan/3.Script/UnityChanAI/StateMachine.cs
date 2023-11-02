using UnityEngine;

public enum State
{
    Idle,
    Walk,
    Run,
    Side_R,
    Side_L
}

public class IdleState
{
    private Animator animator;
    public IdleState(Animator animator)
    {
        this.animator = animator;
    }
    public void Update() 
    {
        
    }
}

public class WalkState
{
    private Transform transform;
    private float walkSpeed;

    public WalkState(Transform transform, float walkSpeed)
    {
        this.transform = transform;
        this.walkSpeed = walkSpeed;
    }

    private int count = 0;
    public void Update()
    {
        transform.position += transform.TransformDirection(Vector3.forward) * walkSpeed* Time.deltaTime;
        Debug.Log($"{count++}");
    }
}

public class RunState
{
    private Transform transform;
    private float RunSpeed;

    public RunState(Transform transform, float RunSpeed)
    {
        this.transform = transform;
        this.RunSpeed = RunSpeed;
    }

    public void Update()
    {
        transform.position += transform.TransformDirection(Vector3.forward) * RunSpeed * Time.deltaTime;
    }
}

public class SideRState
{
    private Transform transform;
    private float RunSpeed;

    public SideRState(Transform transform, float RunSpeed)
    {
        this.transform = transform;
        this.RunSpeed = RunSpeed;
    }

    public void Update()
    {
        transform.position += transform.TransformDirection(Vector3.right) * RunSpeed * Time.deltaTime;
    }
}

public class SideLState
{
    private Transform transform;
    private float RunSpeed;

    public SideLState(Transform transform, float RunSpeed)
    {
        this.transform = transform;
        this.RunSpeed = RunSpeed;
    }

    public void Update()
    {
        transform.position += transform.TransformDirection(Vector3.left) * RunSpeed * Time.deltaTime;
    }
}