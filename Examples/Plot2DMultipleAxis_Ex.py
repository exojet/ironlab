﻿from ironplot import *
from math import *
x = [i*0.01 for i in range(0, 1000)]
y = [sin(i) * i**2 for i in x]
dydx = map(lambda j, i: (j - i) / 0.01, y[1:], y[0:-1])

#dydx = [(i2 - i) / 0.01 for (i2, i) in itertools.izip(y[1:], y[0:-1])]

curve1 = plot(x, y, 'r', Title="Function")
hold(True)
curve2 = plot(x[0:-1], dydx, 'b', Title="Gradient")
hold(False)
plot1 = curve1.Plot
# Create new axis
newAxis = YAxis(Position = YAxisPosition.Right, Max = 100, Min = -100)
# Override the current innermost right YAxis with the new axis.
plot1.Axes.YAxes.Right = newAxis
# Alternative is plot1.Axes.YAxes.Add(newAxis) to add outside innermost axis.
# Set curve2 to use the new Axis
curve2.YAxis = newAxis
newAxis.Foreground = newAxis.AxisTicks.Stroke = Brushes.Blue
plot1.Axes.YAxes.Left.Foreground = plot1.Axes.YAxes.Left.AxisTicks.Stroke = Brushes.Red


