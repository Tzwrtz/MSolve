﻿using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Elements.SupportiveClasses;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.LinearAlgebra.Vectors;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.FEM;
using ISAAR.MSolve.Discretization.Providers;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.Solvers.Ordering;
using ISAAR.MSolve.Solvers.Direct;

namespace ISAAR.MSolve.SamplesConsole.Logging
{
    public static class PrintForceDisplacementCurve
    {
        private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\MSolveResults";
        private const int subdomainID = 0;
        private const int monitorNode = 3;
        private const DOFType monitorDof = DOFType.Y;

        public static void CantileverBeam2DCorotationalDisplacementControl()
        {
            Model_v2 model = CreateModelWithoutLoads();

            double nodalDisplacement = 146.558710945558;
            model.NodesDictionary[monitorNode].Constraints.Add(new Constraint { DOF = DOFType.Y, Amount = nodalDisplacement });

            Analyze(model, false);
        }

        public static void CantileverBeam2DCorotationalLoadControl()
        {
            Model_v2 model = CreateModelWithoutLoads();

            // Add nodal load values at the top nodes of the model
            double nodalLoad = 20000.0;
            model.Loads.Add(new Load() { Amount = nodalLoad, Node = model.NodesDictionary[monitorNode], DOF = DOFType.Y });

            Analyze(model, true);
        }

        private static Model_v2 CreateModelWithoutLoads()
        {
            double youngModulus = 21000.0;
            double poissonRatio = 0.3;
            double area = 91.04;
            double inertia = 8091.0;
            int nElems = 2;

            // Create new 2D material
            ElasticMaterial material = new ElasticMaterial
            {
                YoungModulus = youngModulus,
                PoissonRatio = poissonRatio,
            };

            // Node creation
            IList<Node> nodes = new List<Node>();
            Node node1 = new Node { ID = 1, X = 0.0, Y = 0.0 };
            Node node2 = new Node { ID = 2, X = 100.0, Y = 0.0 };
            Node node3 = new Node { ID = 3, X = 200.0, Y = 0.0 };

            nodes.Add(node1);
            nodes.Add(node2);
            nodes.Add(node3);

            // Model creation
            var model = new Model_v2();

            // Add a single subdomain to the model
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Add nodes to the nodes dictonary of the model
            for (int i = 0; i < nodes.Count; ++i)
            {
                model.NodesDictionary.Add(i + 1, nodes[i]);
            }

            // Constrain bottom nodes of the model
            model.NodesDictionary[1].Constraints.Add(new Constraint() { DOF = DOFType.X, Amount = 0.0 });
            model.NodesDictionary[1].Constraints.Add(new Constraint() { DOF = DOFType.Y, Amount = 0.0 });
            model.NodesDictionary[1].Constraints.Add(new Constraint() { DOF = DOFType.RotZ, Amount = 0.0 });

            // Generate elements of the structure
            int iNode = 1;
            for (int iElem = 0; iElem < nElems; iElem++)
            {
                // element nodes
                IList<Node> elementNodes = new List<Node>();
                elementNodes.Add(model.NodesDictionary[iNode]);
                elementNodes.Add(model.NodesDictionary[iNode + 1]);

                // Create new Beam3D section and element
                var beamSection = new BeamSection2D(area, inertia);
                var beam = new Beam2DCorotational(elementNodes, material, 7.85, beamSection);

                // Create elements
                var element = new Element()
                {
                    ID = iElem + 1,
                    ElementType = beam
                };

                // Add nodes to the created element
                element.AddNode(model.NodesDictionary[iNode]);
                element.AddNode(model.NodesDictionary[iNode + 1]);

                var a = beam.StiffnessMatrix(element);

                // Add beam element to the element and subdomains dictionary of the model
                model.ElementsDictionary.Add(element.ID, element);
                model.SubdomainsDictionary[subdomainID].Elements.Add(element);
                iNode++;
            }

            return model;
        }

        private static void Analyze(Model_v2 model, bool loadControl)
        {
            // Choose linear equation system solver
            var solverBuilder = new SkylineSolver.Builder();
            SkylineSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer
            NonLinearAnalyzerBase childAnalyzer;
            int increments = 10;
            if (loadControl)
            {
                var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments);
                childAnalyzerBuilder.ResidualTolerance = 1E-3;
                //childAnalyzerBuilder.SubdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) }; // This is the default
                childAnalyzer = childAnalyzerBuilder.Build();
            }
            else
            {
                var equivalentLoadsAssemblers = new Dictionary<int, IEquivalentLoadsAssembler_v2>
                {
                    { subdomainID, new EquivalentLoadsAssembler_v2(model.SubdomainsDictionary[subdomainID],
                                                                   new ElementStructuralStiffnessProvider())
                    }
                };
                var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider,
                    equivalentLoadsAssemblers, increments);
                childAnalyzer = childAnalyzerBuilder.Build();
            }

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            string outputFile = outputDirectory + "\\load_control_beam2D_corrotational.txt";
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }
    }
}