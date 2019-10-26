using System;
using System.Collections.Generic;
using System.Text;
using ISAAR.MSolve.LinearAlgebra.Matrices;

namespace ISAAR.MSolve.FEM.Interfaces
{
    public interface IEmbeddedBeamElement : IEmbeddedElement_v2
    {
        Matrix CalculateRotationMatrix();
    }
}
