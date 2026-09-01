namespace LTSAgent.Backend.Core;

/// <summary>
/// 프로젝트 경로를 제공하는 정적 클래스
/// </summary>
public static class AgentPaths
{
    // ~User/.ltsagent 사용자 설정 디렉터리 경로
    public static readonly string UserConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ltsagent");
    
    // ── 프로젝트 경로 ──
    
    /// <summary> ToolKit 프로젝트 루트 경로 (.slnx 파일이 위치한 디렉토리) </summary>
    public static string RootPath { get; } = string.Empty;

    /// <summary> ToolKit\Revit 파일 경로 </summary>
    public static string ChefToolkitPath { get; } = string.Empty;

    /// <summary> 프로젝트 레벨 설정 디렉토리 경로  ({RootPath}/.ltsagent) </summary>
    public static string ConfigDir => Path.Combine(RootPath, ".ltsagent");
    
    /// <summary> 스킬 디렉토리 경로  ({ConfigDir}/skills) </summary>
    public static string SkillsDir => Path.Combine(ConfigDir, "skills");
    
    // ── 초기화 ──
    static AgentPaths()
    {
        DirectoryInfo Dir = new(AppContext.BaseDirectory);
        while (Dir is not null)
        {
            // 현재 위치 바로 아래에 "Chef.Toolkit" 폴더가 있는지 확인
            DirectoryInfo ChefToolkit = Dir.GetDirectories("Chef.Toolkit").FirstOrDefault();

            if (ChefToolkit is not null)
            {
                RootPath = Dir.FullName;
                ChefToolkitPath = ChefToolkit.FullName;
                return;
            }

            Dir = Dir.Parent;
        }

        throw new DirectoryNotFoundException("Chef.Toolkit 폴더를 찾을 수 없습니다.");
    }
}
