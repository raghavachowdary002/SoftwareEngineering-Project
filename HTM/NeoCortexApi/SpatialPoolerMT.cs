﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NeoCortexApi.Entities;
using System.Linq;
using NeoCortexApi.Utility;
using System.Reflection;

namespace NeoCortexApi
{
    public class SpatialPoolerMT : SpatialPooler
    {
        /// <summary>
        /// Uses the same implementation as Single-Threaded.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="distMem"></param>
        public override void InitMatrices(Connections c, DistributedMemory distMem)
        {
            base.InitMatrices(c, distMem);
        }

        /// <summary>
        /// Implements muticore initialization of pooler.
        /// </summary>
        /// <param name="c"></param>
        protected override void ConnectAndConfigureInputs(Connections c)
        {
            List<KeyPair> colList = new List<KeyPair>();

            ConcurrentDictionary<int, KeyPair> colList2 = new ConcurrentDictionary<int, KeyPair>();

            int numColumns = c.NumColumns;

            // Parallel implementation of initialization
            ParallelOptions opts = new ParallelOptions();
            //int synapseCounter = 0;

            Parallel.For(0, numColumns, opts, (indx) =>
            {
                Random rnd = new Random(42);

                int colIndex = (int)indx;
                var data = new ProcessingData();

                // Gets RF
                data.Potential = HtmCompute.MapPotential(c.HtmConfig, colIndex, rnd /*(c.getRandom()*/);
                data.Column = c.getColumn(colIndex);

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                data.Column.CreatePotentialPool(c.HtmConfig, data.Potential, -1);
                //connectColumnToInputRF(c.HtmConfig, data.Potential, data.Column);

                //Interlocked.Add(ref synapseCounter, data.Column.ProximalDendrite.Synapses.Count);

                //colList.Add(new KeyPair() { Key = i, Value = column });

                data.Perm = HtmCompute.InitSynapsePermanences(c.HtmConfig, data.Potential, c.getRandom());

                data.AvgConnected = GetAvgSpanOfConnectedSynapses(c, colIndex);

                HtmCompute.UpdatePermanencesForColumn( c.HtmConfig, data.Perm, data.Column, data.Potential, true);

                if (!colList2.TryAdd(colIndex, new KeyPair() { Key = colIndex, Value = data }))
                {

                }
            });

            //c.setProximalSynapseCount(synapseCounter);

            List<double> avgSynapsesConnected = new List<double>();

            foreach (var item in colList2.Values)
            //for (int i = 0; i < numColumns; i++)
            {
                int i = (int)item.Key;

                ProcessingData data = (ProcessingData)item.Value;
                //ProcessingData data = new ProcessingData();

                // Debug.WriteLine(i);
                //data.Potential = mapPotential(c, i, c.isWrapAround());

                //var st = string.Join(",", data.Potential);
                //Debug.WriteLine($"{i} - [{st}]");

                //var counts = c.getConnectedCounts();

                //for (int h = 0; h < counts.getDimensions()[0]; h++)
                //{
                //    // Gets the synapse mapping between column-i with input vector.
                //    int[] slice = (int[])counts.getSlice(h);
                //    Debug.Write($"{slice.Count(y => y == 1)} - ");
                //}
                //Debug.WriteLine(" --- ");
                // Console.WriteLine($"{i} - [{String.Join(",", ((ProcessingData)item.Value).Potential)}]");

                // This line initializes all synases in the potential pool of synapses.
                // It creates the pool on proximal dendrite segment of the column.
                // After initialization permancences are set to zero.
                //var potPool = data.Column.createPotentialPool(c, data.Potential);
                //connectColumnToInputRF(c, data.Potential, data.Column);

                //data.Perm = initPermanence(c.getSynPermConnected(), c.getSynPermMax(),
                //      c.getRandom(), c.getSynPermTrimThreshold(), c, data.Potential, data.Column, c.getInitConnectedPct());

                //updatePermanencesForColumn(c, data.Perm, data.Column, data.Potential, true);

                avgSynapsesConnected.Add(data.AvgConnected);

                colList.Add(new KeyPair() { Key = i, Value = data.Column });
            }

            SparseObjectMatrix<Column> mem = (SparseObjectMatrix<Column>)c.getMemory();
            
            if (mem.IsRemotelyDistributed)
            {
                // Pool is created and attached to the local instance of Column.
                // Here we need to update the pool on remote Column instance.
                mem.set(colList);
            }

            // The inhibition radius determines the size of a column's local
            // neighborhood.  A cortical column must overcome the overlap score of
            // columns in its neighborhood in order to become active. This radius is
            // updated every learning round. It grows and shrinks with the average
            // number of connected synapses per column.
            updateInhibitionRadius(c, avgSynapsesConnected);
        }


        public override int[] CalculateOverlap(Connections c, int[] inputVector)
        {
            ParallelOptions opts = new ParallelOptions();
            opts.MaxDegreeOfParallelism = Environment.ProcessorCount;
            ConcurrentDictionary<int, int> overlaps = new ConcurrentDictionary<int, int>();

            Parallel.For(0, c.NumColumns, (col) =>
            {
                overlaps[col] = c.getColumn(col).GetColumnOverlapp(inputVector, c.StimulusThreshold);
            });

          
            return overlaps.Values.ToArray();
        }

        public override void AdaptSynapses(Connections c, int[] inputVector, int[] activeColumns)
        {

            // Get all indicies of input vector, which are set on '1'.
            var inputIndices = ArrayUtils.IndexWhere(inputVector, inpBit => inpBit > 0);

            double[] permChanges = new double[c.NumInputs];

            // First we initialize all permChanges to minimum decrement values,
            // which are used in a case of none-connections to input.
            ArrayUtils.fillArray(permChanges, -1 * c.getSynPermInactiveDec());

            // Then we update all connected permChanges to increment values for connected values.
            // Permanences are set in conencted input bits to default incremental value.
            ArrayUtils.SetIndexesTo(permChanges, inputIndices.ToArray(), c.getSynPermActiveInc());

            Parallel.For(0, activeColumns.Length, (i) =>
            {
                //Pool pool = c.getPotentialPools().get(activeColumns[i]);
                Pool pool = c.getColumn(activeColumns[i]).ProximalDendrite.RFPool;
                double[] perm = pool.getDensePermanences(c.NumInputs);
                int[] indexes = pool.getSparsePotential();
                ArrayUtils.raiseValuesBy(permChanges, perm);
                Column col = c.getColumn(activeColumns[i]);
                HtmCompute.UpdatePermanencesForColumn(c.HtmConfig, perm, col, indexes, true);
            });
        }

        class ProcessingData
        {
            public int[] Potential { get; set; }

            public Column Column { get; set; }

            public double[] Perm { get; internal set; }

            public double AvgConnected { get; set; }
        }

    }
}
