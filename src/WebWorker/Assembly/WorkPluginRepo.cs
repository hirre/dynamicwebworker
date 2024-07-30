using WebWorkerInterfaces;

namespace WebWorker.Assembly
{
    public class WorkPluginRepo
    {
        private Dictionary<string, IWork> _workPluginDict = [];

        public WorkPluginRepo()
        {
        }

        public void AddWorkPlugin(string workPluginClassName, IWork workPlugin)
        {
            _workPluginDict.TryAdd(workPluginClassName, workPlugin);
        }

        public IWork? GetWorkPlugin(string workPluginClassName)
        {
            return _workPluginDict.ContainsKey(workPluginClassName) ? _workPluginDict[workPluginClassName] : null;
        }

        public int GetWorkPluginCount()
        {
            return _workPluginDict.Count;
        }

        public string[] GetWorkPluginNames()
        {
            return [.. _workPluginDict.Keys];
        }
    }
}
