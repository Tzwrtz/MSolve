using System.Collections.Generic;
using System.Linq;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Interfaces;
using ISAAR.MSolve.LinearAlgebra;
using ISAAR.MSolve.LinearAlgebra.Matrices;

namespace ISAAR.MSolve.FEM.Embedding
{
    public class BeamElementEmbedder_v2 : ElementEmbedder_v2
    {  
        public BeamElementEmbedder_v2(Model_v2 model, Element_v2 embeddedElement, IEmbeddedDOFInHostTransformationVector_v2 transformation)
            : base(model, embeddedElement, transformation)
        {
        }
        
        protected override void CalculateTransformationMatrix()
        {
            var e = (IEmbeddedBeamElement)(embeddedElement.ElementType);
            base.CalculateTransformationMatrix();
            //transformationMatrix = e.CalculateRotationMatrix().MultiplyRight(transformationMatrix,true);
        }        
    }
}
