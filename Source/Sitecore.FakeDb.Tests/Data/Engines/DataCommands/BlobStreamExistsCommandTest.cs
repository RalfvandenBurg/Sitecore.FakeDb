﻿namespace Sitecore.FakeDb.Tests.Data.Engines.DataCommands
{
  using System;
  using System.IO;
  using FluentAssertions;
  using NSubstitute;
  using Sitecore.Data.Engines;
  using Sitecore.FakeDb.Data.Engines.DataCommands;
  using Xunit;

  public class BlobStreamExistsCommandTest : CommandTestBase
  {
    private readonly OpenBlobStreamExistsCommand command;

    public BlobStreamExistsCommandTest()
    {
      this.command = new OpenBlobStreamExistsCommand { Engine = new DataEngine(this.database) };
      this.command.Initialize(this.innerCommand);
    }

    [Fact]
    public void ShouldCreateInstance()
    {
      // arrange
      var createdCommand = Substitute.For<BlobStreamExistsCommand>();
      this.innerCommand.CreateInstance<Sitecore.Data.Engines.DataCommands.BlobStreamExistsCommand, BlobStreamExistsCommand>().Returns(createdCommand);

      // act & assert
      this.command.CreateInstance().Should().Be(createdCommand);
    }

    [Fact]
    public void ShouldReturnTrueIfBlobStreamExistsInDataStorage()
    {
      // arrange
      var blobId = Guid.NewGuid();
      var stream = new MemoryStream();

      this.dataStorage.Blobs.Add(blobId, stream);

      this.command.Initialize(blobId);

      // act & act
      this.command.DoExecute().Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnFalseIfNoBlobStreamExistsInDataStorage()
    {
      // arrange
      var blobId = Guid.NewGuid();

      this.command.Initialize(blobId);

      // act & act
      this.command.DoExecute().Should().BeFalse();
    }

    private class OpenBlobStreamExistsCommand : BlobStreamExistsCommand
    {
      public new Sitecore.Data.Engines.DataCommands.BlobStreamExistsCommand CreateInstance()
      {
        return base.CreateInstance();
      }

      public new bool DoExecute()
      {
        return base.DoExecute();
      }
    }
  }
}