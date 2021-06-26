﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NeoCortexApi
{
    public class ThreadSafeRandom : Random
    {
        public ThreadSafeRandom() : base()
        {

        }

        public ThreadSafeRandom(int seed) : base(seed)
        {
            
        }

        public override int Next()
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next();
            }
        }

        public override int Next(int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next(maxValue);
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.Next(minValue, maxValue);
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            throw new NotSupportedException();
        }

        public override double NextDouble()
        {
            lock (typeof(ThreadSafeRandom))
            {
                return base.NextDouble();
            }
        }
    }
}
