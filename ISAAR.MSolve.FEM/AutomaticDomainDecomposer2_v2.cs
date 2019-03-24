﻿using ISAAR.MSolve.FEM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ISAAR.MSolve.FEM
{
    public class AutomaticDomainDecomposer2_v2 // exei allaxei mono to onoma den exei ginei update se model_v2 xrhsh klp.
    {
        private readonly Model_v2 Model;
        private readonly int NumberOfProcessors;
        private int numberOfElementsPerSubdomain;
        Dictionary<Element_v2, List<Element_v2>> ElementAdjacency;
        List<Element_v2> ElementsRenumbered = new List<Element_v2>();
        Dictionary<int,List<Node_v2>> SubdomainInterfaceNodes =new Dictionary<int, List<Node_v2>>();

        public AutomaticDomainDecomposer2_v2(Model_v2 model, int numberOfProcessors)
        {
            this.NumberOfProcessors = numberOfProcessors;
            this.Model = model;
        }


        public void UpdateModel(bool isColoringEnabled = false)
        {
            Adjacency();

            CreateSubdomains();
            AssignElementsRenumberedToSubdomains();

            if (isColoringEnabled)
            {
                var purgedElements = Purge();
                ColorDisconnectedElements(purgedElements);
            }
        }

        private void AssignElementsRenumberedToSubdomains()
        {
            Model.SubdomainsDictionary.Clear();
            var indexElement = 0;
            for (int i = 0; i < NumberOfProcessors; i++)
            {
                if (indexElement >= ElementsRenumbered.Count) break;
                Model.SubdomainsDictionary.Add(i, new Subdomain_v2(i)); //new Subdomain_v2() { ID = i }
                for (int j = 0; j < numberOfElementsPerSubdomain; j++)
                {
                    if (indexElement >= ElementsRenumbered.Count) break;
                    Model.SubdomainsDictionary[i].Elements.Add( ElementsRenumbered[indexElement++]);
                }
            }

            UpdateModelDataStructures();
        }

        private void UpdateModelDataStructures()
        {
            foreach (Subdomain_v2 subdomain in Model.SubdomainsDictionary.Values)
            {
                foreach (Element_v2 element in subdomain.Elements)
                    element.Subdomain = subdomain;
            }

            foreach (Node_v2 node in Model.NodesDictionary.Values)
            {
                node.SubdomainsDictionary.Clear();
                node.BuildSubdomainDictionary();
            }
            //TEMP comment
            //foreach (Subdomain_v2 subdomain in Model.SubdomainsDictionary.Values)
            //{
            //    subdomain.NodesDictionary.Clear();
            //    subdomain.BuildNodesDictionary();
            //}
        }

        private void Adjacency()
        {
            ElementAdjacency = new Dictionary<Element_v2, List<Element_v2>>();
            // mask is an integer that shows if the element is used
            

            foreach (var element in Model.ElementsDictionary.Values)
            {
                var usedElement = new Dictionary<Element_v2, bool>(Model.ElementsDictionary.Count);//bool[] usedElement = new bool[Model.ElementsDictionary.Count];//mask
                foreach(Element_v2 e1 in Model.ElementsDictionary.Values) { usedElement.Add(e1, false); }

                ElementAdjacency.Add(element,new List<Element_v2>());
                usedElement[element] = true;
                foreach (var node in element.NodesDictionary.Values)
                {
                    foreach (var nodeElement in node.ElementsDictionary.Values)
                    {
                        if (!usedElement.ContainsKey(nodeElement)) { usedElement.Add(nodeElement, false); };

                        if (usedElement[nodeElement]) continue;
                        ElementAdjacency[element].Add(nodeElement);
                        usedElement[nodeElement] = true;
                    }
                }
            }
        }
        
        private void CreateSubdomains()
        {
            //TODO: return IndexOutOfRangeException if nodes,elements or subdomains numbering does not start with 0.
            var isInteriorBoundaryElement = new Dictionary<Element_v2, bool>(Model.ElementsDictionary.Count); //bool[] isInteriorBoundaryElement= new bool[Model.ElementsDictionary.Count];
            foreach(Element_v2 element in Model.ElementsDictionary.Values) { isInteriorBoundaryElement.Add(element, false); }
            var isInteriorBoundaryNode = new Dictionary<Node_v2, bool>(Model.NodesDictionary.Count); //bool[] isInteriorBoundaryNode = new bool[Model.NodesDictionary.Count];
            foreach(Node_v2 node in Model.NodesDictionary.Values) { isInteriorBoundaryNode.Add(node, false); }

            // Number of Elements per subdomain
            numberOfElementsPerSubdomain =(Model.ElementsDictionary.Count % NumberOfProcessors==0)?
                Model.ElementsDictionary.Count / NumberOfProcessors: Model.ElementsDictionary.Count / NumberOfProcessors+1;

            Dictionary<Node_v2, int> nodeWeight = new Dictionary<Node_v2, int>();
            foreach (Node_v2 node in Model.NodesDictionary.Values)
                nodeWeight.Add(node, node.ElementsDictionary.Count);

            var usedElementsCounter = 0;
            var mlabel = 0;
            int counterSubdomain = 0;
            
            do
            {
                var flag = true;
                var flagStop=true;
                #region Find Node with next minimum weight
                var finalSubdomainElement = usedElementsCounter;
                var minimumNodeWeight = int.MaxValue;
                int nodeID = Model.NodesDictionary.ElementAt(0).Value.ID;// int nodeID = 0; ;
                for (int i = 0; i < Model.NodesDictionary.Count; i++)
                {
                    if (nodeWeight[Model.NodesDictionary.ElementAt(i).Value] == 0) continue;
                    if (nodeWeight[Model.NodesDictionary.ElementAt(i).Value] < minimumNodeWeight)
                    {
                        minimumNodeWeight = nodeWeight[Model.NodesDictionary.ElementAt(i).Value];
                        nodeID = Model.NodesDictionary.ElementAt(i).Value.ID;
                    }
                }
                #endregion

                // Start fill list with elements connected to node with minimum weight
                var counterSubdomainElements = 0;
                foreach (var element in Model.NodesDictionary[nodeID].ElementsDictionary.Values)
                {
                    var elementID = element.ID;
                    if (isInteriorBoundaryElement[element]) continue;
                    counterSubdomainElements++;
                    isInteriorBoundaryElement[element] = true;
                    ElementsRenumbered.Add(element);

                    #region nomask
                    //Reduce nodeWeight for all nodes connected to this element
                    foreach (Node_v2 node in Model.ElementsDictionary[elementID].NodesDictionary.Values)
                        nodeWeight[node]--;
                    #endregion

                    if (counterSubdomainElements == numberOfElementsPerSubdomain)
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    // Recursively add adjacent elements to list
                    do
                    {
                        var initialSubdomainElement = finalSubdomainElement;
                        finalSubdomainElement = usedElementsCounter + counterSubdomainElements;
                        var nnstart = initialSubdomainElement + 1;
                        
                        for (int i = initialSubdomainElement; i <= finalSubdomainElement-1; i++)
                        {
                            int lc = 0;
                            for (int j=0; j< ElementAdjacency.First(x=>x.Key.ID==ElementsRenumbered[i].ID).Value.Count; j++)
                            {
                                var element = ElementAdjacency[ElementsRenumbered[i]][j]; int elementID = element.ID;  //int elementID = ElementAdjacency[ElementsRenumbered[i]][j].ID;
                                if (isInteriorBoundaryElement[element]) continue;
                                lc++;
                                counterSubdomainElements++;
                                isInteriorBoundaryElement[element] = true;
                                ElementsRenumbered.Add(Model.ElementsDictionary.First(x =>x.Value.ID== elementID).Value);

                                #region nomask
                                foreach (Node_v2 node in Model.ElementsDictionary[elementID].NodesDictionary.Values)
                                    nodeWeight[node]--;
                                #endregion

                                if (counterSubdomainElements== numberOfElementsPerSubdomain)
                                {
                                    flag = false;
                                    break;
                                }
                            } // 800

                            if (flag)
                            {
                                if (lc == 0 && (usedElementsCounter + counterSubdomainElements) == finalSubdomainElement && i == finalSubdomainElement-1)
                                {
                                    usedElementsCounter = usedElementsCounter + counterSubdomainElements;
                                    flagStop = false;
                                    flag = false;
                                }
                            }

                            if (!flag) break;
                        } 
                        if (!flag) break;
                    } while (counterSubdomainElements < numberOfElementsPerSubdomain);
                    if (!flagStop) continue;
                }
                SubdomainInterfaceNodes.Add(counterSubdomain++,CalculateInterface(nodeWeight, isInteriorBoundaryNode, ElementsRenumbered, usedElementsCounter, counterSubdomainElements, counterSubdomain));
                usedElementsCounter = usedElementsCounter + counterSubdomainElements;
                mlabel = usedElementsCounter;
            } while (usedElementsCounter < Model.ElementsDictionary.Count);

        }

        //isInteriorBoundaryNode-> if node is on the interior interface
        private List<Node_v2>  CalculateInterface(Dictionary<Node_v2,int> nodeWeight, Dictionary<Node_v2,bool> isInteriorBoundaryNode, List<Element_v2>  ElementsRenumbered, int usedElementsCounter, int counterSubdomainElements, int counterSubdomain)
        {
            var locmask = new Dictionary<Node_v2, bool>(Model.NodesDictionary.Count);// new bool[Model.NodesDictionary.Count];
            foreach(Node_v2 node in Model.NodesDictionary.Values) { locmask.Add(node, false); }

            List<Node_v2> SubdomainInterfaceNodes = new List<Node_v2>();            

            for (int i = usedElementsCounter; i < usedElementsCounter + counterSubdomainElements; i++)
            {
                int elementID = ElementsRenumbered[i].ID;
                foreach (Node_v2 node in Model.ElementsDictionary[elementID].NodesDictionary.Values)
                {
                    if ((nodeWeight[node] !=0||isInteriorBoundaryNode[node])&&!locmask[node])
                    {
                        isInteriorBoundaryNode[node] = true;
                        locmask[node] = true;
                        SubdomainInterfaceNodes.Add(node);
                    }
                }
            }
            return SubdomainInterfaceNodes;
        }

        private void ColorDisconnectedElements(List<Element_v2> purgedElements)
        {
            int numberOfColors = 1;
            Dictionary<int, List<Element_v2>> elementsPerColor = new Dictionary<int, List<Element_v2>>();
            int indexColor = 0;
            int counterElement = 0;

            var isElementUsed = new Dictionary<Element_v2, bool>(Model.ElementsDictionary.Count);//bool[] isElementUsed = new bool[Model.ElementsDictionary.Count];

            // Form the list of distinct colors
            do
            {
                elementsPerColor.Add(indexColor,new List<Element_v2>());
                var usedNodes = new Dictionary<Node_v2, bool>(Model.NodesDictionary.Count);// bool[] usedNodes = new bool[Model.NodesDictionary.Count];
                foreach (var element in purgedElements)
                {
                    if (isElementUsed[element]) continue;

                    bool disjoint = CheckIfElementDisjoint(usedNodes, element.ID);

                    if (disjoint)
                    {
                        counterElement++;
                        elementsPerColor[indexColor].Add(Model.ElementsDictionary[element.ID]);

                        #region marker
                        foreach (Node_v2 node in Model.ElementsDictionary[element.ID].NodesDictionary.Values)
                            usedNodes[node] = true;
                        #endregion

                    }

                }
                indexColor++;
            } while (counterElement < purgedElements.Count);
            numberOfColors = indexColor;
        }

        private bool CheckIfElementDisjoint(Dictionary<Node_v2, bool> usedNodes, int elementID)
        {
            foreach (Node_v2 node in Model.ElementsDictionary[elementID].NodesDictionary.Values)
                if (usedNodes[node])
                    return false;
            return true;
        }


        private List<Element_v2> Purge()
        {
            var isElementUsed = new Dictionary<Element_v2, bool>(Model.ElementsDictionary.Count);// bool[] isElementUsed = new bool[Model.ElementsDictionary.Count];

            List<Node_v2> interfaceNodes = new List<Node_v2>();
            foreach (var nodeList in SubdomainInterfaceNodes.Values)
                interfaceNodes.AddRange(nodeList);

            interfaceNodes=interfaceNodes.Distinct().ToList();
            int numberOfInterfaceNodes = interfaceNodes.Count;

            List<Element_v2> purgedElements=new List<Element_v2>();
            int numberOfPurgedElements = 0;
            for (int indexInterfaceNode = 0; indexInterfaceNode < numberOfInterfaceNodes; indexInterfaceNode++)
            {
                foreach (Element_v2 element in interfaceNodes[indexInterfaceNode].ElementsDictionary.Values)
                {
                    if (!isElementUsed[element])
                    {
                        isElementUsed[element] = true;
                        purgedElements.Add(element);
                    }
                }
            }

            return purgedElements;
        }

    }
}