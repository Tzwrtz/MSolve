using System.Collections.Generic;
using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Discretization;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.Geometry.Shapes;
using ISAAR.MSolve.LinearAlgebra.Iterative.ConjugateGradient;
using ISAAR.MSolve.LinearAlgebra.Iterative.Termination;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.Numerical.LinearAlgebra;
//using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers;
using ISAAR.MSolve.Solvers.Direct;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Iterative;
using ISAAR.MSolve.Solvers.Skyline;
using Xunit;

namespace ISAAR.MSolve.Tests
{
    public static class Quad4LinearDisplacementControlExample
    {
        [Fact]
        private static void Quad4LinearDisplacementControlTest_v2()
        {
            VectorExtensions.AssignTotalAffinityCount();

            // Model & subdomains
            var model = new Model_v2();
            int subdomainID = 0;
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Materials
            double youngModulus = 4.0;
            double poissonRatio = 0.4;
            double thickness = 1.0;
            
            var material = new ElasticMaterial2D_v2(StressState2D.PlaneStress)
            {
                YoungModulus = youngModulus,
                PoissonRatio = poissonRatio
            };

            // Nodes
            var nodes = new Node_v2[]
            {
                new Node_v2 { ID = 1, X = 0.0, Y = 0.0, Z = 0.0 },
                new Node_v2 { ID = 2, X = 10.0, Y = 0.0, Z = 0.0 },
                new Node_v2 { ID = 3, X = 10.0, Y = 10.0, Z = 0.0 },
                new Node_v2 { ID = 4, X = 0.0, Y = 10.0, Z = 0.0 }
            };
            for (int i = 0; i < nodes.Length; ++i) model.NodesDictionary.Add(i + 1, nodes[i]);


            // Elements
            var factory = new ContinuumElement2DFactory(thickness, material, null);

            var elementWrapper = new Element_v2()
            {
                ID = 0,
                ElementType = factory.CreateElement(CellType2D.Quad4, nodes)
            };
            elementWrapper.AddNodes(nodes);
            model.ElementsDictionary.Add(elementWrapper.ID, elementWrapper);
            model.SubdomainsDictionary[subdomainID].Elements.Add(elementWrapper);

            //var a = quad.StiffnessMatrix(element);

            // Prescribed nodal displacements
            model.NodesDictionary[1].Constraints.Add(new Constraint() { DOF = DOFType.X, Amount = 0.0 });
            model.NodesDictionary[1].Constraints.Add(new Constraint() { DOF = DOFType.Y, Amount = 0.0 });
            model.NodesDictionary[4].Constraints.Add(new Constraint() { DOF = DOFType.X, Amount = 0.0 });
            model.NodesDictionary[4].Constraints.Add(new Constraint() { DOF = DOFType.Y, Amount = 0.0 });

            // Imposed nodal displacements
            double nodalDisplacement = -5.0;
            model.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.X, Amount = nodalDisplacement });
            model.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.X, Amount = nodalDisplacement });

            // Solver
            var solverBuilder = new SkylineSolver.Builder();
            ISolver_v2 solver = solverBuilder.BuildSolver(model);

            // Problem type
            var provider = new ProblemStructural_v2(model, solver);

            // Analyzers
            var childAnalyzer = new LinearAnalyzer_v2_new(model, solver, provider);
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);
            
            // Request output
            childAnalyzer.LogFactories[subdomainID] = new LinearAnalyzerLogFactory_v2(new int[] { 0 });

            // Run the anlaysis 
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();

            // Check output
            DOFSLog_v2 log = (DOFSLog_v2)childAnalyzer.Logs[subdomainID][0]; //There is a list of logs for each subdomain and we want the first one
            Assert.Equal(-1.39535, log.DOFValues[0], 5);
        }
    }
}
