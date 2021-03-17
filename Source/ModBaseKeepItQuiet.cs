using HugsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeepItQuiet
{
    public class ModBaseKeepItQuiet : ModBase
    {
        public ModBaseKeepItQuiet()
        {
            Settings.EntryName = "Keep It Quiet";
        }

        public override string ModIdentifier
        {
            get
            {
                return "JPT_keepitquiet";
            }
        }

    }
}
