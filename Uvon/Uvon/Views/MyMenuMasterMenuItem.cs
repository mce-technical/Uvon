using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uvon.Views
{

    public class MyMenuMasterMenuItem
    {
        public MyMenuMasterMenuItem()
        {
            TargetType = typeof(MyMenuMasterMenuItem);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}