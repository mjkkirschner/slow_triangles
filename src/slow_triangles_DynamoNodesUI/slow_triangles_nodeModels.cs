using CoreNodeModels;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace slow_triangles.DynamoNodes.UI
{
    public static class slow_triangles_nodeModels
    {


       // public MaterialNode

        //grabbed from DynamoRevit repo. Should probably be in core.

        /// <summary>
        /// Generic UI Dropdown node baseclass for Enumerations.
        /// This class populates a dropdown with all enumeration values of the specified type.
        /// </summary>
        public abstract class CustomGenericEnumerationDropDown : DSDropDownBase
        {

            /// <summary>
            /// Generic Enumeration Dropdown
            /// </summary>
            /// <param name="name">Node Name</param>
            /// <param name="enumerationType">Type of Enumeration to Display</param>
            public CustomGenericEnumerationDropDown(string name, Type enumerationType) : base(name)
            {
                this.EnumerationType = enumerationType;
                PopulateDropDownItems();
            }

            [JsonConstructor]
            public CustomGenericEnumerationDropDown(string name, Type enumerationType, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
                : base(name, inPorts, outPorts)
            {
                this.EnumerationType = enumerationType;
                PopulateDropDownItems();
            }

            /// <summary>
            /// Type of Enumeration
            /// </summary>
            private Type EnumerationType
            {
                get;
                set;
            }

            protected override CoreNodeModels.DSDropDownBase.SelectionState PopulateItemsCore(string currentSelection)
            {
                PopulateDropDownItems();
                return SelectionState.Done;
            }

            /// <summary>
            /// Populate Items in Dropdown menu
            /// </summary>
            public void PopulateDropDownItems()
            {
                if (this.EnumerationType != null)
                {
                    // Clear the dropdown list
                    Items.Clear();

                    // Get all enumeration names and add them to the dropdown menu
                    foreach (string name in Enum.GetNames(EnumerationType))
                    {
                        Items.Add(new CoreNodeModels.DynamoDropDownItem(name, Enum.Parse(EnumerationType, name)));
                    }

                    Items = Items.OrderBy(x => x.Name).ToObservableCollection();
                }
            }

            /// <summary>
            /// Assign the selected Enumeration value to the output
            /// </summary>
            public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
            {
                // If the dropdown is still empty try to populate it again          
                if (Items.Count == 0 || Items.Count == -1)
                {
                    if (this.EnumerationType != null && Enum.GetNames(this.EnumerationType).Length > 0)
                    {
                        PopulateItems();
                    }
                }

                // If there are no elements in the dropdown or the selected Index is invalid return a Null node.
                if (!CanBuildOutputAst())
                    return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };

                // get the selected items name
                var stringNode = AstFactory.BuildStringNode((string)Items[SelectedIndex].Name);

                // assign the selected name to an actual enumeration value
                var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);

                // return the enumeration value
                return new List<AssociativeNode> { assign };
            }

            /// <summary>
            /// whether it have valid Enumeration values to the output
            /// </summary>
            /// <param name="itemValueToIgnore"></param>
            /// <param name="selectedValueToIgnore"></param>
            /// <returns>true is that there are valid values to output,false is that only a null value to output</returns>
            public Boolean CanBuildOutputAst(string itemValueToIgnore = null, string selectedValueToIgnore = null)
            {
                if (Items.Count == 0 || SelectedIndex < 0)
                    return false;
                if (!string.IsNullOrEmpty(itemValueToIgnore) && Items[0].Name == itemValueToIgnore)
                    return false;
                if (!string.IsNullOrEmpty(selectedValueToIgnore) && Items[SelectedIndex].Name == selectedValueToIgnore)
                    return false;
                return true;
            }
        }
    }
}
