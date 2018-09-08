﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISAAR.MSolve.XFEM.Geometry.CoordinateSystems;

namespace ISAAR.MSolve.XFEM.Geometry.Mesh
{
    interface ICell
    {
        IReadOnlyList<ICartesianPoint2D> Vertices { get; }
    }
}
