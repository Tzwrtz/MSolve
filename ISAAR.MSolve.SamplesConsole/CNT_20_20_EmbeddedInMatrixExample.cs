﻿using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Analyzers.Interfaces;
using ISAAR.MSolve.FEM.Elements;
using ISAAR.MSolve.FEM.Elements.SupportiveClasses;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Materials;
using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Skyline;
using System;
using System.Collections.Generic;
using System.Text;
using ISAAR.MSolve.Discretization.Interfaces;
using Xunit;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.FEM;
using ISAAR.MSolve.Discretization.Providers;
using ISAAR.MSolve.Logging;
using ISAAR.MSolve.FEM.Embedding;
using System.Linq;
using ISAAR.MSolve.Discretization.Integration.Quadratures;
using System.IO;
using ISAAR.MSolve.Solvers.Direct;

namespace ISAAR.MSolve.SamplesConsole
{
    public class CNT_20_20_EmbeddedInMatrixExample
    {
        private const string outputDirectory = @"E:\GEORGE_DATA\DESKTOP\MSolveResults";
        private const int subdomainID = 0;

        public static void EmbeddedCNT_20_20_inMatrix_NewtonRaphson()
        {
            VectorExtensions.AssignTotalAffinityCount();

            // No. of increments
            int increments = 1000;

            // Model creation
            var model = new Model_v2();

            // Subdomains
            //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Variables
            int monitorNode = 10001;
            DOFType monitorDof = DOFType.Z;

            // Choose model
            EmbeddedModelBuilder.EmbeddedExample(model);

            // Boundary Conditions - Left End [End-1]
            for (int iNode = 1; iNode <= 100; iNode++)
            {
                //model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                //model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
            }

            // Boundary Conditions - Bottom End [End-3] (y = -10)
            for (int iNode = 1; iNode <= 10010; iNode = iNode + 100)
            {
                for (int jj = 0; jj <= 9; jj++)
                {
                    model.NodesDictionary[iNode + jj].Constraints.Add(new Constraint { DOF = DOFType.Y });
                }                
            }

            // Boundary Conditions - Bottom End [End-5] (x = -10)
            for (int iNode = 1; iNode <= 10091; iNode = iNode + 10)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
            }

            // Bending Loading - [End-2]
            double nodalLoad = -2.0;
            for (int iNode = 10001; iNode <= 10100; iNode++)
            {
                model.Loads.Add(new Load() { Amount = nodalLoad, Node = model.NodesDictionary[iNode], DOF = DOFType.Z });
            }

            // Choose linear equation system solver
            //var solverBuilder = new SkylineSolver.Builder();
            //SkylineSolver solver = solverBuilder.BuildSolver(model);
            var solverBuilder = new SuiteSparseSolver.Builder();
            SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer            
            var childAnalyzerBuilder = new LoadControlAnalyzer_v2.Builder(model, solver, provider, increments) { ResidualTolerance = 5E-03 };
            LoadControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            string outputFile = outputDirectory + "\\CNT-Embedded-3D_Results.txt";
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }

        public static void EmbeddedCNT_20_20_inMatrix_DisplacementControl()
        {
            VectorExtensions.AssignTotalAffinityCount();

            // No. of increments
            int increments = 10;

            // Model creation
            var model = new Model_v2();

            // Subdomains
            //model.SubdomainsDictionary.Add(subdomainID, new Subdomain() { ID = 1 });
            model.SubdomainsDictionary.Add(subdomainID, new Subdomain_v2(subdomainID));

            // Variables
            int monitorNode = 10001;
            DOFType monitorDof = DOFType.Y;

            // Choose model
            EmbeddedModelBuilder.EmbeddedExample(model);

            // Boundary Conditions - Left End [End-1]
            for (int iNode = 1; iNode <= 100; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.X });
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y });
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Z });
            }

            // Applied Displacements [End-2]
            double nodalDisplacement = 100.0;
            for (int iNode = 10001; iNode <= 10010; iNode++)
            {
                model.NodesDictionary[iNode].Constraints.Add(new Constraint { DOF = DOFType.Y, Amount = nodalDisplacement });
            }

            // Choose linear equation system solver
            //var solverBuilder = new SkylineSolver.Builder();
            //SkylineSolver solver = solverBuilder.BuildSolver(model);
            var solverBuilder = new SuiteSparseSolver.Builder();
            SuiteSparseSolver solver = solverBuilder.BuildSolver(model);

            // Choose the provider of the problem -> here a structural problem
            var provider = new ProblemStructural_v2(model, solver);

            // Choose child analyzer -> Child: NewtonRaphsonNonLinearAnalyzer
            var subdomainUpdaters = new[] { new NonLinearSubdomainUpdater_v2(model.SubdomainsDictionary[subdomainID]) };
            var equivalentLoadsAssemblers = new Dictionary<int, IEquivalentLoadsAssembler_v2>
            {
                { subdomainID, new EquivalentLoadsAssembler_v2(model.SubdomainsDictionary[subdomainID],
                                                               new ElementStructuralStiffnessProvider())
                }
            };
            var childAnalyzerBuilder = new DisplacementControlAnalyzer_v2.Builder(model, solver, provider,
                equivalentLoadsAssemblers, increments);
            DisplacementControlAnalyzer_v2 childAnalyzer = childAnalyzerBuilder.Build();

            // Choose parent analyzer -> Parent: Static
            var parentAnalyzer = new StaticAnalyzer_v2(model, solver, provider, childAnalyzer);

            // Request output
            string outputFile = outputDirectory + "\\CNT-Embedded-3D_Results.txt";
            var logger = new TotalLoadsDisplacementsPerIncrementLog(model.SubdomainsDictionary[subdomainID], increments,
                model.NodesDictionary[monitorNode], monitorDof, outputFile);
            childAnalyzer.IncrementalLogs.Add(subdomainID, logger);

            // Run the analysis
            parentAnalyzer.Initialize();
            parentAnalyzer.Solve();
        }

        public static class EmbeddedModelBuilder
        {
            public static void EmbeddedExample(Model_v2 model)
            {
                HostElementsBuilder(model);
                EmbeddedElementsBuilder(model);
                var embeddedGrouping = new EmbeddedGrouping_v2(model, model.ElementsDictionary.Where(x => x.Key <= 8100).Select(kv => kv.Value), model.ElementsDictionary.Where(x => x.Key > 8100).Select(kv => kv.Value), true);
            }

            public static void HostElementsBuilder(Model_v2 model)
            {
                string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\input files"; //"..\..\..\Resources\Beam3DInputFiles";

                string MatrixGeometryFileName = "MATRIX_3D-L_x=20-L_y=20-L_z=100-9x9x100-Geometry_MSolve.inp";
                
                string MatrixGonnectivityFileName = "MATRIX_3D-L_x=20-L_y=20-L_z=100-9x9x100-ConnMatr_MSolve.inp";
                
                int matrixNodes = File.ReadLines(workingDirectory + '\\' + MatrixGeometryFileName).Count();
                int matrixElements = File.ReadLines(workingDirectory + '\\' + MatrixGonnectivityFileName).Count();

                // Nodes Geometry                
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixGeometryFileName))
                {
                    for (int i = 0; i < matrixNodes; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int nodeID = int.Parse(bits[0]);
                        double nodeX = double.Parse(bits[1]);
                        double nodeY = double.Parse(bits[2]);
                        double nodeZ = double.Parse(bits[3]);
                        model.NodesDictionary.Add(nodeID, new Node { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                    }
                }

                // Create Material
                ElasticMaterial3D solidMaterial = new ElasticMaterial3D()
                {
                    YoungModulus = 1.00,
                    PoissonRatio = 0.30,
                };

                // Generate elements
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + MatrixGonnectivityFileName))
                {
                    for (int i = 0; i < matrixElements; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int elementID = int.Parse(bits[0]);
                        int node1 = int.Parse(bits[1]);
                        int node2 = int.Parse(bits[2]);
                        int node3 = int.Parse(bits[3]);
                        int node4 = int.Parse(bits[4]);
                        int node5 = int.Parse(bits[5]);
                        int node6 = int.Parse(bits[6]);
                        int node7 = int.Parse(bits[7]);
                        int node8 = int.Parse(bits[8]);
                        // Hexa8NL element definition
                        Element hexa8NLelement = new Element()
                        {
                            ID = elementID,
                            ElementType = new Hexa8NonLinear(solidMaterial, GaussLegendre3D.GetQuadratureWithOrder(3, 3, 3))
                        };
                        // Add nodes to the created element
                        hexa8NLelement.AddNode(model.NodesDictionary[node1]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node2]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node3]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node4]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node5]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node6]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node7]);
                        hexa8NLelement.AddNode(model.NodesDictionary[node8]);
                        // Add Hexa element to the element and subdomains dictionary of the model
                        model.ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
                        //model.SubdomainsDictionary[0].ElementsDictionary.Add(hexa8NLelement.ID, hexa8NLelement);
                        model.SubdomainsDictionary[0].Elements.Add(hexa8NLelement);
                    }
                }
            }

            public static void EmbeddedElementsBuilder(Model_v2 model)
            {
                // define mechanical properties
                double youngModulus = 16710.0; // 5490; // 
                double shearModulus = 8080.0; // 871; // 
                double poissonRatio = 2.15; // 0.034; //(youngModulus / (2 * shearModulus)) - 1;
                double area = 5.594673861218848d - 003;  // CNT(20,20)-LinearEBE-TBT-L = 10nm
                double inertiaY = 2.490804749753243D - 006; //1058.55;
                double inertiaZ = 2.490804749753243D - 006; // 1058.55;
                double torsionalInertia = 4.981609499506486D - 006; //496.38;
                double effectiveAreaY = area;
                double effectiveAreaZ = area;
                string workingDirectory = @"E:\GEORGE_DATA\DESKTOP\input files"; //"..\..\..\Resources\Beam3DInputFiles";

                string CNTgeometryFileName = "CNT-20-20-L=100-h=2-Geometry.inp";
                
                string CNTconnectivityFileName = "CNT-20-20-L=100-h=2-ConnMatr.inp";
                
                int CNTNodes = File.ReadLines(workingDirectory + '\\' + CNTgeometryFileName).Count();
                int CNTElems = File.ReadLines(workingDirectory + '\\' + CNTconnectivityFileName).Count();

                // Geometry
                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTgeometryFileName))
                {
                    for (int i = 0; i < CNTNodes; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int nodeID = int.Parse(bits[0]) + 10100; // matrixNodes
                        double nodeX = double.Parse(bits[1]);
                        double nodeY = double.Parse(bits[2]);
                        double nodeZ = double.Parse(bits[3]);
                        model.NodesDictionary.Add(nodeID, new Node { ID = nodeID, X = nodeX, Y = nodeY, Z = nodeZ });
                    }
                }

                // Create new 3D material
                ElasticMaterial3D beamMaterial = new ElasticMaterial3D
                {
                    YoungModulus = youngModulus,
                    PoissonRatio = poissonRatio,
                };

                // Create new Beam3D section and element
                var beamSection = new BeamSection3D(area, inertiaY, inertiaZ, torsionalInertia, effectiveAreaY, effectiveAreaZ);
                // element nodes

                using (TextReader reader = File.OpenText(workingDirectory + '\\' + CNTconnectivityFileName))
                {
                    for (int i = 0; i < CNTElems; i++)
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(',');
                        int elementID = int.Parse(bits[0]) + 8100; // matrixElements
                        int node1 = int.Parse(bits[1]) + 10100; // matrixNodes
                        int node2 = int.Parse(bits[2]) + 10100; // matrixNodes
                        // element nodes
                        IList<Node> elementNodes = new List<Node>();
                        elementNodes.Add(model.NodesDictionary[node1]);
                        elementNodes.Add(model.NodesDictionary[node2]);
                        // create element
                        var beam_1 = new Beam3DCorotationalQuaternion(elementNodes, beamMaterial, 7.85, beamSection);
                        var beamElement = new Element { ID = elementID, ElementType = beam_1 };
                        // Add nodes to the created element
                        beamElement.AddNode(model.NodesDictionary[node1]);
                        beamElement.AddNode(model.NodesDictionary[node2]);
                        // beam stiffness matrix
                        // var a = beam_1.StiffnessMatrix(beamElement);
                        // Add beam element to the element and subdomains dictionary of the model
                        model.ElementsDictionary.Add(beamElement.ID, beamElement);
                        //model.SubdomainsDictionary[0].ElementsDictionary.Add(beamElement.ID, beamElement);
                        model.SubdomainsDictionary[0].Elements.Add(beamElement);
                    }
                }
            }
        }
    }
}