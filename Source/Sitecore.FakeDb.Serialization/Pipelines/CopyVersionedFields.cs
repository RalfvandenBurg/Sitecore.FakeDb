﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Diagnostics;
using Sitecore.FakeDb.Pipelines;

namespace Sitecore.FakeDb.Serialization.Pipelines
{
    public class CopyVersionedFields
    {
        public void Process(DsItemLoadingArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            args.DsDbItem.SyncItem.CopyVersionedFieldsTo(args.DsDbItem);
        }
    }
}
