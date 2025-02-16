using UnityEngine;

public class DogController : MonoBehaviour
{
    private Animator animator;
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private bool shouldUpdatePosition = true;

    [SerializeField] private float movementScale = 1.0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        moveDirection = transform.forward;
        targetRotation = transform.rotation;
    }

    void OnAnimatorMove()
    {
        if (!shouldUpdatePosition) return;

        // 获取动画的移动增量
        Vector3 deltaPosition = animator.deltaPosition;
        
        // 根据当前朝向应用移动
        Vector3 scaledDelta = deltaPosition * movementScale;
        transform.position += scaledDelta;

        // 应用旋转
        transform.rotation *= animator.deltaRotation;
    }

    // 在转向动画开始时调用
    public void OnTurnStart()
    {
        // 记录当前朝向
        moveDirection = transform.forward;
        // 计算目标旋转（假设转向动画是180度，如果不是请调整角度）
        targetRotation = transform.rotation * Quaternion.Euler(0, 180, 0);
    }

    // 在转向动画结束时调用
    public void OnTurnFinish()
    {
        // 强制设置为目标旋转
        transform.rotation = targetRotation;
        moveDirection = transform.forward;
    }

    // 用于暂停/恢复位置更新的方法
    public void PausePositionUpdate()
    {
        shouldUpdatePosition = false;
    }

    public void ResumePositionUpdate()
    {
        shouldUpdatePosition = true;
    }
}