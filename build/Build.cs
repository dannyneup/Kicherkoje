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
    const string NetDaemonAddonContainerId = "c6a2317c_netdaemon5";
    const string HostsNetDaemonDirectory = "/config/netdaemon5";

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    IConfiguration Configuration;
    Project TargetProject => Solution.Kicherkoje_Automations;

    AbsolutePath AppSettingsPath => TargetProject.Directory / "appsettings.json";

    AbsolutePath PublishDirectory => TargetProject.Directory / "bin" / "Publish";

    AbsolutePath PublishQuartzDatabasePath => PublishDirectory / "quartz.sqlite";

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
                .SetConfiguration(global::Configuration.Release)));

    Target Publish => d => d
        .DependsOn(DotnetBuild)
        .Executes(() => DotNetPublish(s =>
            s.SetProject(TargetProject)
                .SetConfiguration(global::Configuration.Release)
                .SetOutput(PublishDirectory)));

    Target CheckNetDaemonVersion => d => d
        .Executes(() =>
        {
            var appVersion = TargetProject
                .GetMSBuildProject()
                .GetItems("PackageReference")
                .SingleOrDefault(reference => reference.EvaluatedInclude == "NetDaemon.Runtime")?
                .GetMetadataValue("Version");
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
                    var serverVersion = p.Output.First().Text;
                    if (appVersion != serverVersion)
                        Assert.Fail($"Server runtime version '{serverVersion}' does not match app version '{appVersion}'!");
                    return null;
                }
            );
        });

    Target SynchronizeFiles => d => d
        .DependsOn(Publish, Publish, CheckNetDaemonVersion)
        .Executes(() =>
        {
            Pwsh(
                $"""
                     -Command
                     rsync -a --delete {PublishDirectory}/. --exclude {PublishQuartzDatabasePath} {HomeAssistantSshHost}:{HostsNetDaemonDirectory}
                 """
            );
            Pwsh(
                $"""
                    -Command
                    rsync -a --ignore-existing {PublishQuartzDatabasePath} {HomeAssistantSshHost}:{HostsNetDaemonDirectory}
                 """
            );
        });

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