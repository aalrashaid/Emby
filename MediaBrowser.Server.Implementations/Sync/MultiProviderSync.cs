﻿using MediaBrowser.Common.IO;
using MediaBrowser.Common.Progress;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Sync;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Server.Implementations.Sync
{
    public class MultiProviderSync
    {
        private readonly ISyncManager _syncManager;
        private readonly IServerApplicationHost _appHost;
        private readonly ILogger _logger;
        private readonly IFileSystem _fileSystem;

        public MultiProviderSync(ISyncManager syncManager, IServerApplicationHost appHost, ILogger logger, IFileSystem fileSystem)
        {
            _syncManager = syncManager;
            _appHost = appHost;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public async Task Sync(IEnumerable<IServerSyncProvider> providers, IProgress<double> progress, CancellationToken cancellationToken)
        {
            var targets = providers
                .SelectMany(i => i.GetAllSyncTargets().Select(t => new Tuple<IServerSyncProvider, SyncTarget>(i, t)))
                .ToList();

            var numComplete = 0;
            double startingPercent = 0;
            double percentPerItem = 1;
            if (targets.Count > 0)
            {
                percentPerItem /= targets.Count;
            }

            foreach (var target in targets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentPercent = startingPercent;
                var innerProgress = new ActionableProgress<double>();
                innerProgress.RegisterAction(pct =>
                {
                    var totalProgress = pct * percentPerItem;
                    totalProgress += currentPercent;
                    progress.Report(totalProgress);
                });

                await new MediaSync(_logger, _syncManager, _appHost, _fileSystem)
                    .Sync(target.Item1, target.Item1.GetDataProvider(), target.Item2, innerProgress, cancellationToken)
                    .ConfigureAwait(false);

                numComplete++;
                startingPercent = numComplete;
                startingPercent /= targets.Count;
                startingPercent *= 100;
                progress.Report(startingPercent);
            }
        }
    }
}
