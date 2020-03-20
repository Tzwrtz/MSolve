using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.LinearAlgebra;
using ISAAR.MSolve.LinearAlgebra.Matrices;
using ISAAR.MSolve.LinearAlgebra.Vectors;

//TODO: _v2 delete FEM.EquivalentLoadsAssembler
//TODO: time logging must be refactored
//TODO: perhaps this belongs to Solvers.Assemblers, since the vector type depends on the solver. In that case, the 
//      elementMatrixProvider should be injected by the problem/provider.
namespace ISAAR.MSolve.Problems
{
    /// <summary>
    /// Calculates the equivalent nodal forces (at the subdomain level) due to Dirichlet boundary conditions.
    /// Authors: Maria Tavlaki
    /// </summary>
    public class DirichletEquivalentLoadsStructural : IDirichletEquivalentLoadsAssembler
    {
        private IElementMatrixProvider_v2 elementProvider; //TODO: not sure if df = K * du is the best way to calcuate df.

        public DirichletEquivalentLoadsStructural(IElementMatrixProvider_v2 elementProvider)
        {
            this.elementProvider = elementProvider;
        }

        public IVector GetEquivalentNodalLoads(ISubdomain_v2 subdomain, IVectorView solution, double constraintScalingFactor) 
        {
            var subdomainEquivalentForces = Vector.CreateZero(subdomain.DofOrdering.NumFreeDofs);
            foreach (IElement_v2 element in subdomain.Elements)
            {
                //var elStart = DateTime.Now;
                IMatrix elementK = elementProvider.Matrix(element);

                double[] localdSolution = 
                    subdomain.CalculateElementIncrementalConstraintDisplacements(element, constraintScalingFactor);

                var elementEquivalentForces = elementK.Multiply(localdSolution);

                subdomain.DofOrdering.AddVectorElementToSubdomain(element, elementEquivalentForces, subdomainEquivalentForces);
            }

            return subdomainEquivalentForces;
        }        
    }
}
