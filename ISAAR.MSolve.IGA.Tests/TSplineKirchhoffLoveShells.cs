﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ISAAR.MSolve.Analyzers;
using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.IGA.Entities;
using ISAAR.MSolve.IGA.Readers;
using ISAAR.MSolve.Materials;
using ISAAR.MSolve.Numerical.LinearAlgebra;
using ISAAR.MSolve.Problems;
using ISAAR.MSolve.Solvers.Interfaces;
using ISAAR.MSolve.Solvers.Skyline;
using Xunit;

namespace ISAAR.MSolve.IGA.Tests
{
	public class TSplineKirchhoffLoveShells
	{
		[Fact]
		public void CantileverShellBenchmark()
		{
			VectorExtensions.AssignTotalAffinityCount();
			Model model = new Model();
			string filename = "..\\..\\..\\InputFiles\\CantileverShell.iga";
			IGAFileReader modelReader = new IGAFileReader(model, filename);
			modelReader.CreateTSplineShellsModelFromFile();

			model.PatchesDictionary[0].Material = new ElasticMaterial2D(StressState2D.PlaneStress)
			{
				PoissonRatio = 0.0,
				YoungModulus = 100
			};
			model.PatchesDictionary[0].Thickness = 1;

			foreach (var controlPoint in model.ControlPointsDictionary.Values.Where(cp=>cp.X<3))
			{
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.X);
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.Y);
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.Z);
			}

			foreach (var controlPoint in model.ControlPointsDictionary.Values.Where(cp => cp.X >49.8))
			{
				model.Loads.Add(new Load()
				{
					Amount = -0.5,
					ControlPoint = model.ControlPointsDictionary[controlPoint.ID],
					DOF = DOFType.Z
				});
			}
			model.ConnectDataStructures();

			var linearSystems = new Dictionary<int, ILinearSystem>();
			linearSystems[0] = new SkylineLinearSystem(0, model.PatchesDictionary[0].Forces);
			SolverSkyline solver = new SolverSkyline(linearSystems[0]);
			ProblemStructural provider = new ProblemStructural(model, linearSystems);
			LinearAnalyzer analyzer = new LinearAnalyzer(solver, linearSystems);
			StaticAnalyzer parentAnalyzer = new StaticAnalyzer(provider, analyzer, linearSystems);

			parentAnalyzer.BuildMatrices();
			parentAnalyzer.Initialize();
			parentAnalyzer.Solve();

			var expectedSolutionVector = new Vector(new double[]
			{
				0, 0, -306.122431, 0, 0, -1552.478121, 0, 0, -3454.810388, 0, 0, -5881.924153, 0, 0, -8702.62361, 0, 0,
				-11785.71439, 0, 0, -13928.57064, 0, 0, -15000.0008, 0, 0, -306.1224369, 0, 0, -1552.47811, 0, 0,
				-3454.810407, 0, 0, -5881.924117, 0, 0, -8702.623683, 0, 0, -11785.71423, 0, 0, -13928.57093, 0, 0,
				-15000.00025, 0, 0, -306.1224493, 0, 0, -1552.478088, 0, 0, -3454.810449, 0, 0, -5881.924038, 0, 0,
				-8702.623837, 0, 0, -11785.71389, 0, 0, -13928.57157, 0, 0, -14999.99909, 0, 0, -306.1224494, 0, 0,
				-1552.478088, 0, 0, -3454.810449, 0, 0, -5881.924038, 0, 0, -8702.623837, 0, 0, -11785.71389, 0, 0,
				-13928.57157, 0, 0, -14999.99909, 0, 0, -306.1224369, 0, 0, -1552.47811, 0, 0, -3454.810407, 0, 0,
				-5881.924117, 0, 0, -8702.623683, 0, 0, -11785.71423, 0, 0, -13928.57093, 0, 0, -15000.00025, 0, 0,
				-306.122431, 0, 0, -1552.478121, 0, 0, -3454.810388, 0, 0, -5881.924154, 0, 0, -8702.62361, 0, 0,
				-11785.71439, 0, 0, -13928.57064, 0, 0, -15000.0008
			});
			for (int i = 0; i < expectedSolutionVector.Length; i++)
			{
				Assert.True(Utilities.AreValuesEqual(expectedSolutionVector[i], linearSystems[0].Solution[i],
					1e-9));
			}
			
		}

		//[Fact]
		public void SquareTSplinesGeoPDEsBenchmark()
		{
			VectorExtensions.AssignTotalAffinityCount();
			Model model = new Model();
			string filename = "..\\..\\..\\InputFiles\\square_unstructured.iga";
			IGAFileReader modelReader = new IGAFileReader(model, filename);
			modelReader.CreateTSplineShellsModelFromFile();

			model.PatchesDictionary[0].Material = new ElasticMaterial2D(StressState2D.PlaneStress)
			{
				PoissonRatio = 0.0,
				YoungModulus = 100
			};
			model.PatchesDictionary[0].Thickness = 1;

			
			model.ConnectDataStructures();

			var linearSystems = new Dictionary<int, ILinearSystem>();
			linearSystems[0] = new SkylineLinearSystem(0, model.PatchesDictionary[0].Forces);
			SolverSkyline solver = new SolverSkyline(linearSystems[0]);
			ProblemStructural provider = new ProblemStructural(model, linearSystems);
			LinearAnalyzer analyzer = new LinearAnalyzer(solver, linearSystems);
			StaticAnalyzer parentAnalyzer = new StaticAnalyzer(provider, analyzer, linearSystems);

			parentAnalyzer.BuildMatrices();
			parentAnalyzer.Initialize();
			parentAnalyzer.Solve();
		}



		//[Fact]
		public void IsogeometricCantileverShell6x10()
		{
			VectorExtensions.AssignTotalAffinityCount();
			Model model = new Model();
			string filename = "..\\..\\..\\InputFiles\\CantileverShell6x10.txt";
			IsogeometricShellReader modelReader = new IsogeometricShellReader(model, filename);
			modelReader.CreateShellModelFromFile();

			foreach (var controlPoint in model.ControlPointsDictionary.Values.Where(cp => cp.X < 3))
			{
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.X);
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.Y);
				model.ControlPointsDictionary[controlPoint.ID].Constrains.Add(DOFType.Z);
			}

			foreach (var controlPoint in model.ControlPointsDictionary.Values.Where(cp => cp.X > 49.8))
			{
				model.Loads.Add(new Load()
				{
					Amount = -0.5,
					ControlPoint = model.ControlPointsDictionary[controlPoint.ID],
					DOF = DOFType.Z
				});
			}

			model.ConnectDataStructures();

			// Solvers
			var linearSystems = new Dictionary<int, ILinearSystem>();
			linearSystems[0] = new SkylineLinearSystem(0, model.PatchesDictionary[0].Forces);
			SolverSkyline solver = new SolverSkyline(linearSystems[0]);
			ProblemStructural provider = new ProblemStructural(model, linearSystems);
			LinearAnalyzer analyzer = new LinearAnalyzer(solver, linearSystems);
			StaticAnalyzer parentAnalyzer = new StaticAnalyzer(provider, analyzer, linearSystems);

			parentAnalyzer.BuildMatrices();
			parentAnalyzer.Initialize();
			parentAnalyzer.Solve();

		}
		
	}
}
