using System.Linq;
using Microsoft.Extensions.Configuration;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Pwsh.PwshTasks;
using Project = Nuke.Common.ProjectModel.Project;

class Build : NukeBuild
{
    const string HomeAssistantSshHost = "homeassistant";
    const string NetDaemonAddonContainerId = "c6a2317c_netdaemon4";
    const string HostsNetDaemonDirectory = "/config/netdaemon4";

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    IConfiguration Configuration;
    Project TargetProject => Solution.Kicherkoje_Automations;

    AbsolutePath AppSettingsPath => TargetProject.Directory / "appsettings.json";

    AbsolutePath PublishDirectory => TargetProject.Directory / "bin" / "Publish";

    Target Initialize => d => d
        .Executes(() =>
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(AppSettingsPath)
                .Build();
        });

    Target Clean => d => d
        .Executes(() => DotNetClean(s => s.SetProject(TargetProject)));

    Target Restore => d => d
        .DependsOn(Clean)
        .Executes(() => DotNetRestore(s => s.SetProjectFile(TargetProject)));

    Target RestoreTools => d => d
        .Executes(() => DotNetToolRestore());

    Target RunCodeGenerator => d => d
        .DependsOn(RestoreTools, Initialize)
        .Executes(() =>
        {
            var host = Configuration["HomeAssistant:Host"];
            var port = Configuration["HomeAssistant:Port"];
            var token = Configuration["HomeAssistant:Token"];
            var haGeneratedNamespace = $"{TargetProject.Name}.Configuration.HomeAssistantGenerated";
            var outputFolder = TargetProject.Directory / "Configuration" / "HomeAssistantGenerated";
            var metaDataOutputFolder = outputFolder / "Metadata";
            var outputFile = outputFolder / "HomeAssistantGenerated.cs";

            return Pwsh(
                $@"-Command dotnet nd-codegen -o {outputFile} -f {metaDataOutputFolder} -ns {haGeneratedNamespace} -host {host} -port {port} -token {token}");
        });

    Target DotnetBuild => d => d
        .DependsOn(Restore, RunCodeGenerator)
        .Executes(() => DotNetBuild(s =>
            s.SetProjectFile(TargetProject)
                .SetConfiguration("Release")));

    Target Publish => d => d
        .DependsOn(DotnetBuild)
        .Executes(() => DotNetPublish(s =>
            s.SetProject(TargetProject)
                .SetConfiguration("Release")
                .SetOutput(PublishDirectory)));

    Target CheckNetDaemonVersion => d => d
        .Executes(() =>
        {
            var appsVersion = TargetProject.GetPackageReferenceVersion("NetDaemon.Runtime");
            Pwsh(
                $"""
                      -Command
                      ssh {HomeAssistantSshHost} 'ha addons info {NetDaemonAddonContainerId} --raw-json'
                      | ConvertFrom-Json
                      | Select-Object -ExpandProperty data
                      | Select-Object -ExpandProperty version
                 """
                ,
                exitHandler: p =>
                {
                    if (appsVersion != p.Output.First().Text)
                        Assert.Fail("NetDaemon runtime versions do not match!");
                }
            );
        });

    Target SynchronizeFiles => d => d
        .DependsOn(CheckNetDaemonVersion, Publish)
        .Executes(() =>
            Pwsh(
                $"""
                     -Command
                     rsync -a --delete {PublishDirectory}/. {HomeAssistantSshHost}:{HostsNetDaemonDirectory}
                 """
            )
        );

    Target RestartAddon => d => d
        .DependsOn(SynchronizeFiles)
        .Executes(() =>
            Pwsh(
                $"""
                     -Command
                     ssh {HomeAssistantSshHost} 'ha addons restart {NetDaemonAddonContainerId}'
                 """
            )
        );

    public static int Main() => Execute<Build>(x => x.RestartAddon);
}