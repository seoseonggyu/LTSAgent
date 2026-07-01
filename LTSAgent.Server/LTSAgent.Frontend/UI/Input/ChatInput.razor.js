/**
 * textarea에 Enter→전송 키 바인딩을 설정
 * Shift+Enter는 줄바꿈을 유지
 */
export function setupEnterSubmit(textarea)
{
    textarea.addEventListener("keydown", function (e)
    {
        if (e.key === "Enter" && !e.shiftKey)
        {
            e.preventDefault();
            textarea.closest("form").requestSubmit();
        }
    });
}