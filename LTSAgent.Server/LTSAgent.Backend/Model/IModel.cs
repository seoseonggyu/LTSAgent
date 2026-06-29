namespace LTSAgent.Backend.Model;

/// <summary>
/// 모델 정의 인터페이스
/// 각 모델 클래스가 이 인터페이스를 구현
/// </summary>
public interface IModel
{
    // Claude API 모델 ID (예: "claude-opus-4-6")
    string Id { get; }

    // UI에 표시할 모델 이름 (예: "Claude Opus 4.6")
    string DisplayName { get; }

    // 모델 설명
    string Description { get; }

    // 최대 출력 토큰 수
    int MaxOutputTokens { get; }

    // 컨텍스트 윈도우 크기
    int ContextWindow { get; }
}