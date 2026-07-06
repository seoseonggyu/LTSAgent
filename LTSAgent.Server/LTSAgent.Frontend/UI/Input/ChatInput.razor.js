/**
 * textarea에 Enter→전송 키 바인딩을 설정
 * Shift+Enter는 줄바꿈을 유지
 */
export function setupKeyBindings(textarea, dotNetRef)
{
    textarea.addEventListener("keydown", function (e)
    {
        if (e.key === "Enter" && !e.shiftKey)
        {
            e.preventDefault();
            textarea.closest("form").requestSubmit();
        }
        else if (e.key === "Tab" && e.shiftKey)
        {
            e.preventDefault();
            dotNetRef.invokeMethodAsync("CycleMode");
        }
    });
}