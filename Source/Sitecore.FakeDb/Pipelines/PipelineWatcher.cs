namespace Sitecore.FakeDb.Pipelines
{
  using System;
  using System.Collections.Generic;
  using System.Xml;
  using Sitecore.Diagnostics;
  using Sitecore.FakeDb.Data.Engines;
  using Sitecore.Pipelines;
  using Sitecore.StringExtensions;
  using Sitecore.Xml;

  public class PipelineWatcher : IDisposable
  {
    private readonly XmlDocument config;

    private readonly DataStorage dataStorage;

    private readonly IDictionary<string, PipelineArgs> expectedCalls = new Dictionary<string, PipelineArgs>();

    private readonly IDictionary<string, PipelineArgs> actualCalls = new Dictionary<string, PipelineArgs>();

    private readonly IDictionary<string, Func<PipelineArgs, bool>> checkThisArgs = new Dictionary<string, Func<PipelineArgs, bool>>();

    private readonly IDictionary<string, Func<PipelineArgs, bool>> filterThisArgs = new Dictionary<string, Func<PipelineArgs, bool>>();

    private IDictionary<string, Action<PipelineArgs>> processThisArgs = new Dictionary<string, Action<PipelineArgs>>();

    private string lastUsedPipelineName;

    private bool disposed;

    public PipelineWatcher(XmlDocument config, DataStorage dataStorage)
    {
      Assert.ArgumentNotNull(config, "config");

      this.config = config;
      this.dataStorage = dataStorage;

      PipelineWatcherProcessor.PipelineRun += this.PipelineRun;
    }

    protected internal XmlDocument ConfigSection
    {
      get { return this.config; }
    }

    public virtual void Expects(string pipelineName)
    {
      this.Expects(pipelineName, (PipelineArgs)null);
    }

    public virtual void Expects(string pipelineName, Func<PipelineArgs, bool> checkThisArgs)
    {
      this.checkThisArgs[pipelineName] = checkThisArgs;

      this.Expects(pipelineName);
    }

    public virtual PipelineWatcher WhenCall(string pipelineName)
    {
      this.Expects(pipelineName);

      return this;
    }

    public virtual PipelineWatcher WithArgs(Func<PipelineArgs, bool> filterThisArgs)
    {
      this.filterThisArgs.Add(this.lastUsedPipelineName, filterThisArgs);

      return this;
    }

    public virtual void Then(Action<PipelineArgs> processThisArgs)
    {
      this.processThisArgs.Add(this.lastUsedPipelineName, processThisArgs);
    }

    public virtual void Expects(string pipelineName, PipelineArgs pipelineArgs)
    {
      Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");

      this.expectedCalls[pipelineName] = pipelineArgs;
      this.lastUsedPipelineName = pipelineName;

      var path = "/sitecore/pipelines/" + pipelineName + "/processor";
      var processorNode = XmlUtil.EnsurePath(path, this.config);

      processorNode.RemoveAll();

      var type = typeof(PipelineWatcherProcessor);
      var value = type + ", " + type.Assembly.GetName().Name;
      XmlUtil.AddAttribute("type", value, processorNode);

      var expectedName = "<param desc=\"expectedName\">{0}</param>".FormatWith(pipelineName);
      XmlUtil.AddXml(expectedName, processorNode);

      var database = this.dataStorage.Database.Name;
      var databaseName = "<param desc=\"databaseName\">{0}</param>".FormatWith(database);
      XmlUtil.AddXml(databaseName, processorNode);
    }

    public virtual void EnsureExpectations()
    {
      const string MessageFormat = "Expected to receive a pipeline call matching ({0}). Actually received no matching calls.";

      foreach (var expectedCall in this.expectedCalls)
      {
        var expectedName = expectedCall.Key;
        Assert.IsTrue(this.actualCalls.ContainsKey(expectedName), MessageFormat, "pipelineName == \"" + expectedName + "\"");

        var expectedArgs = expectedCall.Value;
        if (expectedArgs != null)
        {
          Assert.IsTrue(expectedArgs == this.actualCalls[expectedName], MessageFormat, "pipelineArgs");
        }

        if (this.checkThisArgs.ContainsKey(expectedName))
        {
          Assert.IsTrue(this.checkThisArgs[expectedName](this.actualCalls[expectedName]), MessageFormat, "pipelineArgs");
        }
      }
    }

    public virtual void Register(string pipelineName, IPipelineProcessor processorMock)
    {
      this.dataStorage.Pipelines[pipelineName] = processorMock;

      this.Expects(pipelineName, delegate { return true; });
    }

    protected virtual void OnPipelineRun(PipelineRunEventArgs e)
    {
      var pipelineName = e.PipelineName;

      this.actualCalls[pipelineName] = e.PipelineArgs;

      if (this.filterThisArgs == null || !this.processThisArgs.ContainsKey(pipelineName))
      {
        return;
      }

      if (!this.filterThisArgs.ContainsKey(pipelineName))
      {
        this.filterThisArgs.Add(pipelineName, a => true);
      }

      if (this.filterThisArgs[pipelineName](e.PipelineArgs))
      {
        {
          this.processThisArgs[pipelineName](e.PipelineArgs);
        }
      }
    }

    private void PipelineRun(object sender, PipelineRunEventArgs e)
    {
      this.OnPipelineRun(e);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
      {
        return;
      }

      if (!disposing)
      {
        return;
      }

      PipelineWatcherProcessor.PipelineRun -= this.PipelineRun;
      this.disposed = true;
    }
  }
}