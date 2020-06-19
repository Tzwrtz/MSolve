using ISAAR.MSolve.Discretization.Interfaces;
using ISAAR.MSolve.FEM.Entities;
using ISAAR.MSolve.FEM.Interfaces;
using ISAAR.MSolve.LinearAlgebra.Vectors;
using System.Collections.Generic;
using System.Diagnostics;

//TODO: finding the contributing elements and the corresponding local dof indices can be done only once in the constructor.
namespace ISAAR.MSolve.Logging.Utilities
{
    /// <summary>
    /// This does not work if the requested node belongs to an element that contains embedded elements.
    /// </summary>
    internal class ConstrainedDofForcesCalculator
    {
        private readonly Subdomain_v2 subdomain;
        private readonly Model_v2 model;

        internal ConstrainedDofForcesCalculator(Subdomain_v2 subdomain, Model_v2 model)
        {
            this.subdomain = subdomain;
            this.model = model;
        }

        internal double CalculateForceAt(Node_v2 node, DOFType dofType, IVectorView totalDisplacements)
        {
            double totalForce = 0.0;

            //foreach (Element_v2 element in node.ElementsDictionary.Values)
            //{

            //    // It is possible that one of the elements at this node does not engage this dof type, in which case -1 will be returned.
            //    // We will not have any contribution from them. If none of the elements engage this dof type, the total force will always be 0.
            //    int monitorDofIdx = FindLocalDofIndex(element, node, dofType);
            //    if (monitorDofIdx == -1) continue;

            //    //TODO: if an element has embedded elements, then we must also take into account their forces.
            //    double[] totalElementDisplacements = subdomain.CalculateElementDisplacements(element, totalDisplacements);
            //    double[] elementForces = element.ElementType.CalculateForcesForLogging(element, totalElementDisplacements);

            //    if (element.ElementType is IEmbeddedHostElement_v2)
            //    {
            //        foreach (var embeddedNode in element.EmbeddedNodes)
            //        {
            //            foreach (var embeddedElement in embeddedNode.ElementsDictionary.Values)
            //            {
            //                //var embeddedDisplacements=blah blah
            //                var embeddedBeamDisplacements = subdomain.CalculateElementDisplacements(embeddedElement, totalDisplacements);
            //                var embeddedDisplacements = element.ElementType.DofEnumerator.GetTransformedDisplacementsVector(embeddedBeamDisplacements);
            //                var embeddedForces = embeddedElement.ElementType.CalculateForcesForLogging(embeddedElement, embeddedDisplacements);
            //                //transformation gia forces hexas
            //                if (element.EmbeddedNodes.Count == 2)
            //                {
            //                    if (embeddedNode == element.EmbeddedNodes[0]) totalForce += embeddedForces[monitorDofIdx];
            //                    else if (embeddedNode == element.EmbeddedNodes[1]) totalForce += embeddedForces[monitorDofIdx + 24];
            //                }
            //                else if (element.EmbeddedNodes.Count == 1)
            //                {
            //                    if (embeddedNode == embeddedElement.Nodes[0] || embeddedNode == embeddedElement.Nodes[2]) totalForce += embeddedForces[monitorDofIdx];
            //                    else if (embeddedNode == embeddedElement.Nodes[1] || embeddedNode == embeddedElement.Nodes[3]) totalForce += embeddedForces[monitorDofIdx + 24];
            //                }
            //            }
            //        }
            //    }
            //    totalForce += elementForces[monitorDofIdx];
            //}

            // This adds all the internal forces of the constrained end            

            //Node_v2[] constrainedNodes = new Node_v2[441];
            //for (int ii = 0; ii < 441; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[8821 + ii];
            //}

            //Node_v2[] constrainedNodes = new Node_v2[1681];
            //for (int ii = 0; ii < 1681; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[67241 + ii];
            //}

            //Node_v2[] constrainedNodes = new Node_v2[4];
            //int jj = 0;
            //for (int ii = 0; ii < 4; ii++)
            //{
            //    jj += 11;
            //    constrainedNodes[ii] = model.NodesDictionary[jj];
            //}

            // Loading Conditions - Imposed Displacements at Nodes - [Right-End] - {121 nodes}

            //Node_v2[] constrainedNodes = new Node_v2[256];
            //for (int ii = 0; ii < 256; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[3841 + ii];
            //}

            // EmbeddedExample_25 & 26
            //Node_v2[] constrainedNodes = new Node_v2[4];
            //for (int ii = 0; ii < 4; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[21 + ii];
            //}

            // EmbeddedExample_27
            //Node_v2[] constrainedNodes = new Node_v2[36];
            //for (int ii = 0; ii < 36; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[181 + ii];
            //}

            //// EmbeddedExample_28
            //Node_v2[] constrainedNodes = new Node_v2[121];
            //for (int ii = 0; ii < 121; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[1211 + ii];
            //}
            
            // EmbeddedExample_16 - EmbeddedExample_17 - EmbeddedExample_23  - {121 nodes}
                // imposed_X
            Node_v2[] constrainedNodes = new Node_v2[121];
            for (int iNode = 11; iNode <= 1331; iNode = iNode + 11)
            {
                constrainedNodes[iNode] = model.NodesDictionary[iNode];
            }

            // EmbeddedExample_16 - EmbeddedExample_17 - EmbeddedExample_23  - {121 nodes}
            // imposed_Y
            //Node_v2[] constrainedNodes = new Node_v2[121];
            //for (int iNode = 111; iNode <= 1321; iNode += 121)
            //{
            //    for (int j = 0; j <= 10; j++)
            //    {
            //        constrainedNodes[iNode] = model.NodesDictionary[iNode + j];
            //    }
            //}

            // EmbeddedExample_16 - EmbeddedExample_17 - EmbeddedExample_23  - {121 nodes}
            // imposed_Z
            //Node_v2[] constrainedNodes = new Node_v2[121];
            //for (int ii = 0; ii < 121; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[1211 + ii];
            //}

            // EmbeddedExample_15  - {4 nodes}
            //Node_v2[] constrainedNodes = new Node_v2[4];
            //for (int ii = 0; ii < 4; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[5 + ii];
            //}

            // EmbeddedExample_20-Run3 - 1st Convergence study of example #23
            //Node_v2[] constrainedNodes = new Node_v2[441];
            //for (int ii = 0; ii < 441; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[8821 + ii];
            //}

            // EmbeddedExample_20-Run4 - 2nd Convergence study of example #23
            //Node_v2[] constrainedNodes = new Node_v2[36];
            //for (int ii = 0; ii < 36; ii++)
            //{
            //    constrainedNodes[ii] = model.NodesDictionary[181 + ii];
            //}

            foreach (Node_v2 constrainedNode in constrainedNodes)
            {
                foreach (Element_v2 element in constrainedNode.ElementsDictionary.Values)
                {
                    // It is possible that one of the elements at this node does not engage this dof type, in which case -1 will be returned.
                    // We will not have any contribution from them. If none of the elements engage this dof type, the total force will always be 0.
                    int monitorDofIdx = FindLocalDofIndex(element, constrainedNode, dofType);
                    if (monitorDofIdx == -1) continue;

                    //TODO: if an element has embedded elements, then we must also take into account their forces.
                    double[] totalElementDisplacements = subdomain.CalculateElementDisplacements(element, totalDisplacements);
                    double[] elementForces = element.ElementType.CalculateForcesForLogging(element, totalElementDisplacements);

                    if (element.ElementType is IEmbeddedHostElement_v2)
                    {
                        foreach (var embeddedNode in element.EmbeddedNodes)
                        {
                            foreach (var embeddedElement in embeddedNode.ElementsDictionary.Values)
                            {
                                //var embeddedDisplacements=blah blah
                                var embeddedBeamDisplacements = subdomain.CalculateElementDisplacements(embeddedElement, totalDisplacements);
                                var embeddedDisplacements = element.ElementType.DofEnumerator.GetTransformedDisplacementsVector(embeddedBeamDisplacements);
                                var embeddedForces = embeddedElement.ElementType.CalculateForcesForLogging(embeddedElement, embeddedDisplacements);
                                //transformation gia forces hexas
                                if (element.EmbeddedNodes.Count == 2)
                                {
                                    if (embeddedElement.Nodes.Count == 2)
                                    {
                                        if (embeddedNode == embeddedElement.Nodes[0]) totalForce += embeddedForces[monitorDofIdx];
                                        else if (embeddedNode == embeddedElement.Nodes[1]) totalForce += embeddedForces[monitorDofIdx + 24];
                                    }
                                    else if (embeddedElement.Nodes.Count == 4)
                                    {
                                        if (embeddedNode == embeddedElement.Nodes[2]) totalForce += embeddedForces[monitorDofIdx];
                                        else if (embeddedNode == embeddedElement.Nodes[3]) totalForce += embeddedForces[monitorDofIdx + 24];
                                    }
                                }
                                else if (element.EmbeddedNodes.Count == 1)
                                {
                                    if (embeddedElement.Nodes.Count == 2)
                                    {
                                        if (embeddedNode == embeddedElement.Nodes[0]) totalForce += embeddedForces[monitorDofIdx];
                                        else if (embeddedNode == embeddedElement.Nodes[1]) totalForce += embeddedForces[monitorDofIdx + 24];
                                    }
                                    else if (embeddedElement.Nodes.Count == 4)
                                    {
                                        if (embeddedNode == embeddedElement.Nodes[2]) totalForce += embeddedForces[monitorDofIdx];
                                        else if (embeddedNode == embeddedElement.Nodes[3]) totalForce += embeddedForces[monitorDofIdx + 24];
                                    }
                                }
                            }
                        }
                    }
                    totalForce += elementForces[monitorDofIdx];
                }
            }

            return totalForce;
        }

        /// <summary>
        /// Returns -1 if the element does not engage the requested <see cref="DOFType"/>
        /// </summary>
        private int FindLocalDofIndex(Element_v2 element, Node_v2 node, DOFType dofType)
        {
            int localNodeIdx = element.Nodes.IndexOf(node);
            Debug.Assert(localNodeIdx != -1, "The element does not contain this node.");
            IList<IList<DOFType>> elementDofs = element.ElementType.DofEnumerator.GetDOFTypes(element);
            int localDofIdx = elementDofs[localNodeIdx].IndexOf(dofType);
            int multNum = elementDofs[localNodeIdx].Count;
            //int dofIdx = multNum * (localNodeIdx + 1) - (localDofIdx + 1);
            int dofIdx = multNum * localNodeIdx + localDofIdx;
            return dofIdx;
        }
    }
}
