﻿using System.Threading;
using NuGet.Protocol.Core.Types;

namespace Velopack.Vpk.Updates;

public class UpdateChecker
{
    private readonly ILogger _logger;
    private IPackageSearchMetadata _cache;

    public UpdateChecker(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<bool> CheckForUpdates()
    {
        try {
            var myVer = VelopackRuntimeInfo.VelopackNugetVersion;
            var isPre = myVer.IsPrerelease || myVer.HasMetadata;

            if (_cache == null) {
                var cancel = new CancellationTokenSource(3000);
                var dl = new NugetDownloader(new NullNugetLogger());
                _cache = await dl.GetPackageMetadata("vpk", isPre ? "pre" : "latest", cancel.Token).ConfigureAwait(false);
            }

            var cacheVersion = _cache.Identity.Version;
            if (cacheVersion > myVer) {
                if (!isPre) {
                    _logger.Warn($"[bold]There is a newer version of vpk available ({cacheVersion}). Run 'dotnet tool update -g vpk'[/]");
                } else {
                    _logger.Warn($"[bold]There is a newer version of vpk available. Run 'dotnet tool update -g vpk --version {cacheVersion}'[/]");
                }
                return true;
            } else {
                _logger.Debug($"vpk is up to date (latest online = {cacheVersion})");
            }
        } catch (Exception ex) {
            _logger.Debug(ex, "Failed to check for updates.");
        }
        return false;
    }
}
