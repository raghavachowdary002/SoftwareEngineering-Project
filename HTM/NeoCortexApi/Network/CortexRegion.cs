﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi.Network
{
    public class CortexRegion
    {
        #region Properties

        public string Name { get; set; }

        public List<CortexLayer<object, object>> HtmLayers { get; set; }
        #endregion

        #region Constructors and Initialization
        public CortexRegion(string name) : this(name, new List<CortexLayer<object, object>>())
        {

        }

        public CortexRegion(string name, List<CortexLayer<object, object>> layers)
        {
            this.Name = name;
            this.HtmLayers = layers;
        }

        #endregion

        #region Public Methods

        public CortexRegion AddLayer(CortexLayer<object, object> layer)
        {
            this.HtmLayers.Add(layer);
            return this;
        }

        public void Compute(int[] input, Boolean learn)
        {
            foreach (var layer in this.HtmLayers)
            {
                layer.Compute(input, learn);
            }
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
