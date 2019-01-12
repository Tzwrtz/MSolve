﻿using ISAAR.MSolve.LinearAlgebra.Vectors;

namespace ISAAR.MSolve.LinearAlgebra.Iterative.ResidualUpdate
{
    internal static class ExactResidual
    {
        internal static IVector Calculate(ILinearTransformation matrix, IVectorView rhs, IVectorView solution)
        {
            IVector residual = rhs.CreateZeroVectorWithSameFormat();
            Calculate(matrix, rhs, solution, residual);
            return residual;
        }

        internal static void Calculate(ILinearTransformation matrix, IVectorView rhs, IVectorView solution, IVector residual)
        {
            //TODO: There is a BLAS operation y = y + a * A*x, that would be perfect for here. rhs.Copy() and then that.
            matrix.Multiply(solution, residual);
            residual.SubtractIntoThis(rhs);
        }

    }
}
