﻿using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.DistributedComputeLib
{
    /// <summary>
    /// Defines the confguration of Akka Cluster.
    /// </summary>
    public class AkkaDistributedDictConfig : IHtmDistConfig
    {
       

        /// <summary>
        /// Time to wait to connect to Akka Cluster.
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Address of all nodes in cluster.
        /// </summary>
        public List<string> Nodes { get; set; }

        /// <summary>
        /// Upload and Download page size.
        /// </summary>
        public int PageSize { get; set; } = 100;

        /// <summary>
        /// Number of partitions per node. Every partition at node will hold a number of elements.
        /// Note, a single actor implements a partition.
        /// </summary>
        public int PartitionsPerNode { get; set; } = 4;

        public int ProcessingBatch { get;  set; }
    }
}
