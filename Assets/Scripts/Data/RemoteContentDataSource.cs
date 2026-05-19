using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Stub. Flip AppConfig.UseRemoteContent = true and implement per spec §22 when backend is ready.
public class RemoteContentDataSource : IContentDataSource {
    public Task<List<Topic>> LoadTopicsAsync() {
        throw new NotImplementedException("RemoteContentDataSource not yet implemented (spec §22).");
    }

    public List<Level> GenerateLevels(string topicId) {
        throw new NotImplementedException("RemoteContentDataSource not yet implemented (spec §22).");
    }
}
