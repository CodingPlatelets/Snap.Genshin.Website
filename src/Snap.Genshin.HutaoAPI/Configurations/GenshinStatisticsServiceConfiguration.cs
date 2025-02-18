﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Genshin.Website.Services.StatisticCalculation;

namespace Snap.Genshin.Website.Configurations
{
    public class GenshinStatisticsServiceConfiguration
    {
        public List<Type> CalculatorTypes { get; } = new();

        public GenshinStatisticsServiceConfiguration AddCalculator<T>()
            where T : IStatisticCalculator
        {
            return AddCalculator(typeof(T));
        }

        private GenshinStatisticsServiceConfiguration AddCalculator(Type calculatorType)
        {
            CalculatorTypes.Add(calculatorType);
            return this;
        }
    }
}