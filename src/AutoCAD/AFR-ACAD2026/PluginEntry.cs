using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using AFR.Abstractions;
using AFR.FontMapping;
using AFR.Hosting;

[assembly: ExtensionApplication(typeof(AFR.PluginEntry))]
[assembly: CommandClass(typeof(AFR.Commands.AfrCommands))]
[assembly: CommandClass(typeof(AFR.Commands.MTextEditorCommand))]

namespace AFR;

/// <summary>
/// AutoCAD 2026 插件入口点。
/// 继承 PluginEntryBase，提供版本特定的平台实例。
/// </summary>
public class PluginEntry : PluginEntryBase
{
    static PluginEntry()
    {
        RegisterAssemblyResolve();
    }

    protected override ICadPlatform CreatePlatform() => new AutoCad2026Platform();
    protected override IFontHook CreateFontHook() => new AutoCadFontHook();
    protected override ICadHost CreateHost() => new AutoCadHost();
}
