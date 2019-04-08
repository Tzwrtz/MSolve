using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Elements.SupportiveClasses;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Problems;
using System.Collections.Generic;
using ISAAR.MSolve.Discretization;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.Solvers.Direct;
using ISAAR.MSolve.Analyzers.NonLinear;
using ISAAR.MSolve.Solvers;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.Discretization.Integration.Quadratures;
using Xunit;

namespace ISAAR.MSolve.Tests
{
    public class Beam3DToBeam3DCohesiveTest
    {
        [Fact]
        public void Beam3DToBeam3DCohesiveTest_example()
        {
            var m = new Model_v2();

            m.NodesDictionary.Add(1, new Node_v2() { ID = 1, X = 0, Y = 0, Z = 0 });
            m.NodesDictionary.Add(2, new Node_v2() { ID = 2, X = 5, Y = 0, Z = 0 });
            m.NodesDictionary.Add(3, new Node_v2() { ID = 3, X = 0, Y = 0, Z = 0 });
            m.NodesDictionary.Add(4, new Node_v2() { ID = 4, X = 5, Y = 0, Z = 0 });

            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.X });
            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.Y });
            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.Z });
            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.RotX });
            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.RotY });
            m.NodesDictionary[1].Constraints.Add(new Constraint { DOF = DOFType.RotZ });

            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.X });
            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Y });
            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.Z });
            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.RotX });
            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.RotY });
            m.NodesDictionary[2].Constraints.Add(new Constraint { DOF = DOFType.RotZ });

            m.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.RotX });
            m.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.RotY });
            m.NodesDictionary[3].Constraints.Add(new Constraint { DOF = DOFType.RotZ });

            m.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.RotX });
            m.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.RotY });
            m.NodesDictionary[4].Constraints.Add(new Constraint { DOF = DOFType.RotZ });

            m.ElementsDictionary.Add(1, new Element_v2()
            {
                ID = 1,
                ElementType = new Beam3DCorotationalQuaternion_v2(new List<Node_v2>(2){ m.NodesDictionary[3], m.NodesDictionary[4] }, new ElasticMaterial3D_v2() { YoungModulus = 2.1e6, PoissonRatio = 0.2 }, 1,
                new BeamSection3D(0.06, 0.0002, 0.00045, 0.000818, 0.05, 0.05))
            });
            m.ElementsDictionary[1].AddNodes(new List<Node_v2>(2) { m.NodesDictionary[3], m.NodesDictionary[4] });

            m.ElementsDictionary.Add(2, new Element_v2()
            {
                ID = 2,
                ElementType = new CohesiveBeam3DToBeam3D(new BondSlipCohMat_v2(100, 10, 100, 10, 1, new double [2] ,new double [2], 1e-10), GaussLegendre1D.GetQuadrature(2),
                 new List<Node_v2>(2) { m.NodesDictionary[3], m.NodesDictionary[4] }, new List<Node_v2>(2) { m.NodesDictionary[1], m.NodesDictionary[2] },
                 new ElasticMaterial3D_v2() { YoungModulus = 2.1e6, PoissonRatio = 0.2 }, 1,
                 new BeamSection3D(0.06, 0.0002, 0.00045, 0.000818, 0.05, 0.05))
            });
            m.ElementsDictionary[2].AddNodes(m.Nodes);

            m.SubdomainsDictionary.Add(1, new Subdomain_v2(1));
            m.SubdomainsDictionary[1].Elements.Add(m.ElementsDictionary[1]);
            m.SubdomainsDictionary[1].Elements.Add(m.ElementsDictionary[2]);
            m.Loads.Add(new Load_v2() { Node = m.NodesDictionary[3], Amount = 10, DOF = DOFType.Y });
            m.Loads.Add(new Load_v2() { Node = m.NodesDictionary[4], Amount = 10, DOF = DOFType.Y });

            // Solver
            var solverBuilder = new SkylineSolver.Builder();
            ISolver_v2 solver = solverBuilder.BuildSolver(m);

            // Problem type
            var provider = new ProblemStructural_v2(m, solver);

            // Analyzers
            int increments = 10;
            var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(m, solver, provider, increments);
            //childAnalyzerBuilder.SubdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) }; // This is the default
            LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();
            var parentAnalyzer = new StaticAnalyzer_v2(m, solver, provider, childAnalyzer);

            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }
    }
}
