/* Filename: BuildString.cs
   Author: neko2k (neko2k@beige-box.com), Thomas Hounsell
   Website: http://www.beige-box.com
   Description: Code to pull the build string from the program and
                return it to wherever it's needed.



   The following source code is CONFIDENTIAL and PROPRIETARY PROPERTY
   of The Little Beige Box and MAY NOT BE RELEASED under PENALTY OF LAW.

   This file Copyright (c) 2010-2018 The Little Beige Box.
*/

using System;
using System.Reflection;

namespace jeopardy_2._0 
{
    public class BuildString
    {
        public string DebugStrFull = "Version {0}.saellis-dev(debug)";
        public string ReleaseStrFull = "Version {0}.saellis-dev";
        public string DebugStr = "Version {0}(debug)";
        public string ReleaseStr = "Version {0}";

        public string SetEvalNotice(int BldType)
        {
            switch (BldType)
            {
                case 0: // Retail
                    return "";
                case 1: // Alpha/Demo
#if DEBUG
                    return (String.Format("Confidential - do not distribute or disclose. " + DebugStrFull, AssemblyVersion));
#else
                    return (String.Format("Confidential - do not distribute or disclose. " + ReleaseStrFull, AssemblyVersion));
#endif
                case 2: // Private Beta Release
#if DEBUG
                    return (String.Format("Private Beta Release. " + DebugStrFull, AssemblyVersion));
#else
                    return (String.Format("Private Beta Release. " + ReleaseStrFull, AssemblyVersion));
#endif
                case 3: // Public Beta Release
#if DEBUG
                    return (String.Format("Beta Release. " + DebugStr, AssemblyVersion));
#else
                    return (String.Format("Beta Release. " + ReleaseStr, AssemblyVersion));
#endif
                case 4: // RC
#if DEBUG
                    return (String.Format("Release Candidate. " + DebugStr, AssemblyVersion));
#else
                    return (String.Format("Release Candidate. " + ReleaseStr, AssemblyVersion));
#endif
                case 5: // just the version number
#if DEBUG
                    return (String.Format(DebugStrFull, AssemblyVersion));
#else
                    return (String.Format(ReleaseStrFull, AssemblyVersion));
#endif
                case 6: // everything else
#if DEBUG
                    return (String.Format("Experimental Version. " + DebugStrFull, AssemblyVersion));
#else
                    return (String.Format("Experimental Version. " + ReleaseStrFull, AssemblyVersion));
#endif

                default:
                    return "";
            }

        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
    }

}
