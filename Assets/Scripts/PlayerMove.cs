void Update()
{
    if (!IsOwner) return;

    float inputHorizontal = Input.GetAxis("Horizontal");
    float inputVertical = Input.GetAxis("Vertical");
    
    // БЕГ: Проверяем, зажат ли левый Shift
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    
    // Считаем скорость: если бежим, умножаем на 2 (чтобы в Blend Tree перейти к бегу)
    float moveSpeed = new Vector2(inputHorizontal, inputVertical).magnitude;
    if (isRunning) moveSpeed *= 2f; 

    // ПЕРЕДАЕМ В АНИМАТОР
    animator.SetFloat("Speed", moveSpeed, 0.1f, Time.deltaTime);

    // ПРЫЖОК: Срабатывает один раз при нажатии
    if (Input.GetKeyDown(KeyCode.Space))
    {
        animator.SetTrigger("Jump");
        // Тут должна быть твоя физика прыжка для CharacterController
    }

    // ПРИСЕДАНИЕ: Пока держим кнопку — приседаем
    bool isCrouching = Input.GetKey(KeyCode.LeftControl);
    animator.SetBool("IsCrouching", isCrouching);
}