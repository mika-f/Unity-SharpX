// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using UnityEngine;

namespace NatsunekoLaboratory.SharpX
{
    [Serializable]
    public class SharpXConfiguration
    {
        #region Location

        [SerializeField]
        private string _location;

        public string Location
        {
            get => _location;
            set => _location = value;
        }

        #endregion

        #region Reference Assemblies

        [SerializeField]
        private List<string> _referenceAssemblies;

        public List<string> ReferenceAssemblies
        {
            get => _referenceAssemblies;
            set => _referenceAssemblies = value;
        }

        #endregion

        #region Reference Projects

        [SerializeField]
        private List<string> _referenceProjects;

        public List<string> ReferenceProjects
        {
            get => _referenceProjects;
            set => _referenceProjects = value;
        }

        #endregion

        #region Workspaces

        [SerializeField]
        private List<string> _workspaces;

        public List<string> Workspaces
        {
            get => _workspaces;
            set => _workspaces = value;
        }

        #endregion
    }
}