using System.Collections.Generic;
using System.Threading.Tasks;

public interface IContentDataSource {
    Task<List<Topic>> LoadTopicsAsync();
    List<Level> GenerateLevels(string topicId);
}
