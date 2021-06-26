﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public interface IHtmDistConfig
    {
        /// <summary>
        /// Time to wait to connect to Akka Cluster.
        /// </summary>
         TimeSpan ConnectionTimeout { get; set; } 

        /// <summary>
        /// Address of all nodes in cluster.
        /// </summary>
         List<string> Nodes { get; set; }

        /// <summary>
        /// Upload and Download page size.
        /// </summary>
         int PageSize { get; set; } 

        /// <summary>
        /// Number of partitions per node. Every partition at node will hold a number of elements.
        /// Note, a single actor implements a partition.
        /// </summary>
         int PartitionsPerNode { get; set; } 

         int ProcessingBatch { get; set; }
    }
}
