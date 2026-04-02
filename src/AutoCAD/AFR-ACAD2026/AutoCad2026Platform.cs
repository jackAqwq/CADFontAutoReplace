using AFR.Abstractions;

namespace AFR;

/// <summary>
/// AutoCAD 2026 平台常量定义。
/// </summary>
internal sealed class AutoCad2026Platform : ICadPlatform
{
    public string BrandName => "AutoCAD";
    public string VersionName => "2026";
    public string AppName => "AFR-ACAD2026";
    public string DisplayName => "AutoCAD 2026";
    public string RegistryBasePath => @"Software\Autodesk\AutoCAD\R25.1";
    public string RegistryKeyPattern => @"^ACAD-[A-Za-z0-9]+:[A-Za-z0-9]+$";
    public string AcDbDllName => "acdb25.dll";
    public string LdFileExport => "?ldfile@@YAHPEB_WHPEAVAcDbDatabase@@PEAVAcFontDescription@@@Z";
    public int PrologueSize => 21;
    public bool SupportsLdFileHook => true;
}
